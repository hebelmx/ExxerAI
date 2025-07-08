using ExxerAI.Application.Services;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using System.Linq;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Unit tests for Enhanced LLM Service with comprehensive mocking
/// </summary>
public class EnhancedLLMServiceTests
{
    private readonly ILLMProvider _mockProvider;
    private readonly ILanguageModelRepository _mockModelRepository;
    private readonly IConversationRepository _mockConversationRepository;
    private readonly LLMServiceConfiguration _configuration;
    private readonly EnhancedLLMService _service;

    public EnhancedLLMServiceTests()
    {
        _mockProvider = Substitute.For<ILLMProvider>();
        _mockModelRepository = Substitute.For<ILanguageModelRepository>();
        _mockConversationRepository = Substitute.For<IConversationRepository>();

        _configuration = new LLMServiceConfiguration
        {
            MaxDailyCost = 100m,
            DefaultMaxTokens = 1000,
            DefaultTemperature = 0.7,
            EnableCostTracking = true,
            EnableRateLimitChecking = true,
            DefaultTimeoutSeconds = 120
        };

        var providers = new List<ILLMProvider> { _mockProvider };

        _service = new EnhancedLLMService(
            providers,
            _mockModelRepository,
            _mockConversationRepository,
            _configuration);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Act & Assert
        _service.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithNullProviders_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new EnhancedLLMService(
            null!,
            _mockModelRepository,
            _mockConversationRepository,
            _configuration));
    }

    [Fact]
    public void Constructor_WithNullModelRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new EnhancedLLMService(
            new List<ILLMProvider> { _mockProvider },
            null!,
            _mockConversationRepository,
            _configuration));
    }

    [Fact]
    public void Constructor_WithNullConversationRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new EnhancedLLMService(
            new List<ILLMProvider> { _mockProvider },
            _mockModelRepository,
            null!,
            _configuration));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new EnhancedLLMService(
            new List<ILLMProvider> { _mockProvider },
            _mockModelRepository,
            _mockConversationRepository,
            null!));
    }

    [Fact]
    public async Task GenerateTextAsync_WithEmptyModelId_ShouldReturnFailure()
    {
        // Arrange
        var modelId = Guid.Empty;
        var prompt = "Test prompt";

        // Act
        var result = await _service.GenerateTextAsync(modelId, prompt);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Invalid model identifier");
    }

    [Fact]
    public async Task GenerateTextAsync_WithEmptyPrompt_ShouldReturnFailure()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var prompt = "";

        // Act
        var result = await _service.GenerateTextAsync(modelId, prompt);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Prompt cannot be empty");
    }

    [Fact]
    public async Task GenerateTextAsync_WithNonExistentModel_ShouldReturnFailure()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var prompt = "Test prompt";

        _mockModelRepository
            .GetByIdAsync(modelId, Arg.Any<CancellationToken>())
            .Returns(Result<LanguageModel>.WithFailure("Model not found"));

        // Act
        var result = await _service.GenerateTextAsync(modelId, prompt);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Model not found");
    }

    [Fact]
    public async Task GenerateTextAsync_WithValidInputs_ShouldReturnSuccess()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var prompt = "Test prompt";
        var model = CreateTestLanguageModel(modelId, "gpt-3.5-turbo");

        var expectedResponse = new LLMResponse
        {
            Content = "Test response",
            InputTokens = 10,
            OutputTokens = 15,
            EstimatedCost = 0.001m,
            ResponseTimeMs = 100,
            FinishReason = "stop"
        };

        // Setup mocks
        _mockModelRepository
            .GetByIdAsync(modelId, Arg.Any<CancellationToken>())
            .Returns(Result<LanguageModel>.WithSuccess(model));

        _mockProvider.SupportedModels.Returns(new[] { "gpt-3.5-turbo" });
        _mockProvider.ProviderName.Returns("OpenAI");

        _mockProvider
            .GetRateLimitInfoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<RateLimitInfo>.WithSuccess(new RateLimitInfo { IsLimitExceeded = false }));

        _mockProvider
            .EstimateCostAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<decimal>.WithSuccess(0.001m));

        _mockProvider
            .GenerateCompletionAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<LLMParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result<LLMResponse>.WithSuccess(expectedResponse));

        // Act
        var result = await _service.GenerateTextAsync(modelId, prompt);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Content.ShouldBe("Test response");
        result.Value!.Metadata.ShouldContainKey("model_id");
        result.Value!.Metadata.ShouldContainKey("model_name");
        result.Value!.Metadata.ShouldContainKey("provider_name");
    }

    [Fact]
    public async Task CreateConversationAsync_WithEmptyAgentId_ShouldReturnFailure()
    {
        // Arrange
        var agentId = Guid.Empty;
        var modelId = Guid.NewGuid();

        // Act
        var result = await _service.CreateConversationAsync(agentId, modelId);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Invalid agent identifier");
    }

    [Fact]
    public async Task CreateConversationAsync_WithEmptyModelId_ShouldReturnFailure()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var modelId = Guid.Empty;

        // Act
        var result = await _service.CreateConversationAsync(agentId, modelId);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Invalid model identifier");
    }

    [Fact]
    public async Task CreateConversationAsync_WithValidInputs_ShouldReturnSuccess()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var modelId = Guid.NewGuid();
        var model = CreateTestLanguageModel(modelId, "gpt-3.5-turbo");
        var title = "Test Conversation";
        var systemPrompt = "You are a helpful assistant";

        var expectedConversation = new Conversation
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            LanguageModelId = modelId,
            Title = title,
            SystemPrompt = systemPrompt,
            Status = ConversationStatus.Active
        };

        // Setup mocks
        _mockModelRepository
            .GetByIdAsync(modelId, Arg.Any<CancellationToken>())
            .Returns(Result<LanguageModel>.WithSuccess(model));

        _mockConversationRepository
            .AddAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>())
            .Returns(Result<Conversation>.WithSuccess(expectedConversation));

        // Act
        var result = await _service.CreateConversationAsync(agentId, modelId, title, systemPrompt);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.AgentId.ShouldBe(agentId);
        result.Value!.LanguageModelId.ShouldBe(modelId);
        result.Value!.Title.ShouldBe(title);
        result.Value!.SystemPrompt.ShouldBe(systemPrompt);
    }

    [Fact]
    public async Task ContinueConversationAsync_WithEmptyConversationId_ShouldReturnFailure()
    {
        // Arrange
        var conversationId = Guid.Empty;
        var message = "Test message";

        // Act
        var result = await _service.ContinueConversationAsync(conversationId, message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Invalid conversation identifier");
    }

    [Fact]
    public async Task ContinueConversationAsync_WithEmptyMessage_ShouldReturnFailure()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var message = "";

        // Act
        var result = await _service.ContinueConversationAsync(conversationId, message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Message cannot be empty");
    }

    [Fact]
    public async Task ContinueConversationAsync_WithValidInputs_ShouldReturnSuccess()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var message = "Hello, assistant!";
        var modelId = Guid.NewGuid();
        var model = CreateTestLanguageModel(modelId, "gpt-3.5-turbo");

        var conversation = new Conversation
        {
            Id = conversationId,
            AgentId = Guid.NewGuid(),
            LanguageModelId = modelId,
            Title = "Test Conversation",
            SystemPrompt = "You are a helpful assistant",
            Status = ConversationStatus.Active,
            Messages = new List<ConversationMessage>()
        };

        var expectedResponse = new LLMResponse
        {
            Content = "Hello! How can I help you today?",
            InputTokens = 20,
            OutputTokens = 10,
            EstimatedCost = 0.002m,
            ResponseTimeMs = 150,
            FinishReason = "stop"
        };

        // Setup mocks
        _mockConversationRepository
            .GetByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(Result<Conversation>.WithSuccess(conversation));

        _mockModelRepository
            .GetByIdAsync(modelId, Arg.Any<CancellationToken>())
            .Returns(Result<LanguageModel>.WithSuccess(model));

        _mockProvider.SupportedModels.Returns(new[] { "gpt-3.5-turbo" });
        _mockProvider.ProviderName.Returns("OpenAI");

        _mockProvider
            .GenerateChatCompletionAsync(Arg.Any<string>(), Arg.Any<IEnumerable<ChatMessage>>(), Arg.Any<LLMParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result<LLMResponse>.WithSuccess(expectedResponse));

        _mockConversationRepository
            .UpdateAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>())
            .Returns(Result<Conversation>.WithSuccess(conversation));

        // Act
        var result = await _service.ContinueConversationAsync(conversationId, message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Content.ShouldBe("Hello! How can I help you today?");
        result.Value!.Role.ShouldBe(MessageRole.Assistant);
        result.Value!.ConversationId.ShouldBe(conversationId);
    }

    [Fact]
    public async Task EstimateCostAsync_WithEmptyModelId_ShouldReturnFailure()
    {
        // Arrange
        var modelId = Guid.Empty;
        var inputTokens = 100;
        var outputTokens = 50;

        // Act
        var result = await _service.EstimateCostAsync(modelId, inputTokens, outputTokens, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Invalid model identifier");
    }

    [Fact]
    public async Task EstimateCostAsync_WithValidInputs_ShouldReturnSuccess()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var inputTokens = 1000;
        var outputTokens = 500;
        var model = CreateTestLanguageModel(modelId, "gpt-3.5-turbo");
        var expectedCost = 0.00125m;

        // Setup mocks
        _mockModelRepository
            .GetByIdAsync(modelId, Arg.Any<CancellationToken>())
            .Returns(Result<LanguageModel>.WithSuccess(model));

        _mockProvider.SupportedModels.Returns(new[] { "gpt-3.5-turbo" });

        _mockProvider
            .EstimateCostAsync(Arg.Any<string>(), inputTokens, outputTokens, Arg.Any<CancellationToken>())
            .Returns(Result<decimal>.WithSuccess(expectedCost));

        // Act
        var result = await _service.EstimateCostAsync(modelId, inputTokens, outputTokens, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBe(expectedCost);
    }

    [Fact]
    public async Task CountTokensAsync_WithEmptyModelId_ShouldReturnFailure()
    {
        // Arrange
        var modelId = Guid.Empty;
        var text = "Test text";

        // Act
        var result = await _service.CountTokensAsync(modelId, text, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Invalid model identifier");
    }

    [Fact]
    public async Task CountTokensAsync_WithEmptyText_ShouldReturnZero()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var text = "";

        // Act
        var result = await _service.CountTokensAsync(modelId, text, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBe(0);
    }

    [Fact]
    public async Task CountTokensAsync_WithValidInputs_ShouldReturnSuccess()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var text = "This is a test message for token counting.";
        var model = CreateTestLanguageModel(modelId, "gpt-3.5-turbo");
        var expectedTokenCount = 10;

        // Setup mocks
        _mockModelRepository
            .GetByIdAsync(modelId, Arg.Any<CancellationToken>())
            .Returns(Result<LanguageModel>.WithSuccess(model));

        _mockProvider.SupportedModels.Returns(new[] { "gpt-3.5-turbo" });

        _mockProvider
            .CountTokensAsync(Arg.Any<string>(), text, Arg.Any<CancellationToken>())
            .Returns(Result<int>.WithSuccess(expectedTokenCount));

        // Act
        var result = await _service.CountTokensAsync(modelId, text, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBe(expectedTokenCount);
    }

    [Fact]
    public async Task ValidateModelAsync_WithEmptyModelId_ShouldReturnFailure()
    {
        // Arrange
        var modelId = Guid.Empty;

        // Act
        var result = await _service.ValidateModelAsync(modelId);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Invalid model identifier");
    }

    [Fact]
    public async Task ValidateModelAsync_WithValidModel_ShouldReturnSuccess()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var model = CreateTestLanguageModel(modelId, "gpt-3.5-turbo");

        var validationResult = new ProviderValidationResult
        {
            IsValid = true,
            TestedModel = "gpt-3.5-turbo",
            ResponseTime = TimeSpan.FromMilliseconds(100)
        };

        // Setup mocks
        _mockModelRepository
            .GetByIdAsync(modelId, Arg.Any<CancellationToken>())
            .Returns(Result<LanguageModel>.WithSuccess(model));

        _mockProvider.SupportedModels.Returns(new[] { "gpt-3.5-turbo" });

        _mockProvider
            .ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProviderValidationResult>.WithSuccess(validationResult));

        // Act
        var result = await _service.ValidateModelAsync(modelId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBeTrue();
    }

    [Fact]
    public async Task StreamTextAsync_WithValidInputs_ShouldReturnStream()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var prompt = "Test streaming prompt";
        var model = CreateTestLanguageModel(modelId, "gpt-3.5-turbo");

        // Setup mocks
        _mockModelRepository
            .GetByIdAsync(modelId, Arg.Any<CancellationToken>())
            .Returns(Result<LanguageModel>.WithSuccess(model));

        _mockProvider.SupportedModels.Returns(new[] { "gpt-3.5-turbo" });
        _mockProvider.SupportsStreaming.Returns(true);

        var mockChunks = new[]
        {
            new LLMResponseChunk { Content = "Hello", ChunkIndex = 0 },
            new LLMResponseChunk { Content = " world", ChunkIndex = 1 },
            new LLMResponseChunk { Content = "!", ChunkIndex = 2, FinishReason = "stop" }
        };
        //using System.Linq.Async; //Remove this if .NET 10 OR ABOVE

        var chuncksSeq = mockChunks.ToAsyncEnumerable();

        _mockProvider
            .StreamCompletionAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<LLMParameters>(), Arg.Any<CancellationToken>())
            .Returns(chuncksSeq);

        // Act
        var chunks = new List<LLMResponseChunk>();
        await foreach (var chunk in _service.StreamTextAsync(modelId, prompt))
        {
            chunks.Add(chunk);
        }

        // Assert
        chunks.Count.ShouldBe(3);
        chunks[0].Content.ShouldBe("Hello");
        chunks[1].Content.ShouldBe(" world");
        chunks[2].Content.ShouldBe("!");
        chunks[2].FinishReason.ShouldBe("stop");

        // Check metadata was added
        chunks.All(c => c.Metadata.ContainsKey("model_id")).ShouldBeTrue();
        chunks.All(c => c.Metadata.ContainsKey("model_name")).ShouldBeTrue();
        chunks.All(c => c.Metadata.ContainsKey("provider_name")).ShouldBeTrue();
    }

    private static LanguageModel CreateTestLanguageModel(Guid id, string name)
    {
        return new LanguageModel
        {
            Id = id,
            Name = name,
            Provider = "OpenAI",
            Version = "1.0",
            IsAvailable = true,
            ContextWindowSize = 4096,
            Capabilities = new ModelCapabilities
            {
                SupportsTextGeneration = true,
                SupportsStreaming = true,
                SupportsFunctionCalling = true,
                MaxOutputTokens = 2048
            },
            Configuration = new ModelConfiguration
            {
                DefaultTemperature = 0.7,
                MaxTokensPerRequest = 1000,
                RequestTimeoutSeconds = 30
            }
        };
    }
}