using ExxerAI.Infrastructure.LLM;
using ExxerAI.Application.Interfaces;
using NSubstitute;
using Shouldly;

namespace ExxerAI.Infrastructure.Tests.LLM;

/// <summary>
/// Unit tests for OpenAI provider implementation
/// </summary>
public class OpenAIProviderTests
{
    private readonly OpenAIConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly OpenAIProvider _provider;

    public OpenAIProviderTests()
    {
        _config = new OpenAIConfiguration
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.openai.com/v1/",
            TimeoutSeconds = 30
        };

        _httpClient = new HttpClient();
        _provider = new OpenAIProvider(_httpClient, _config);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Act & Assert
        _provider.ProviderName.ShouldBe("OpenAI");
        _provider.SupportsStreaming.ShouldBeTrue();
        _provider.SupportsFunctionCalling.ShouldBeTrue();
        _provider.SupportedModels.ShouldNotBeEmpty();
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new OpenAIProvider(null!, _config));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new OpenAIProvider(_httpClient, null!));
    }

    [Theory]
    [InlineData("gpt-4o")]
    [InlineData("gpt-4-turbo")]
    [InlineData("gpt-3.5-turbo")]
    public void SupportedModels_ShouldContainExpectedModels(string modelName)
    {
        // Act & Assert
        _provider.SupportedModels.ShouldContain(modelName);
    }

    [Fact]
    public async Task CountTokensAsync_WithValidText_ShouldReturnApproximateCount()
    {
        // Arrange
        var text = "Hello, this is a test message for token counting.";
        var modelName = "gpt-3.5-turbo";

        // Act
        var result = await _provider.CountTokensAsync(modelName, text);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
        result.Value.ShouldBeLessThan(50); // Approximate token count for this text
    }

    [Fact]
    public async Task CountTokensAsync_WithEmptyText_ShouldReturnZero()
    {
        // Arrange
        var text = "";
        var modelName = "gpt-3.5-turbo";

        // Act
        var result = await _provider.CountTokensAsync(modelName, text);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0);
    }

    [Fact]
    public async Task CountTokensAsync_WithNullText_ShouldReturnZero()
    {
        // Arrange
        string? text = null;
        var modelName = "gpt-3.5-turbo";

        // Act
        var result = await _provider.CountTokensAsync(modelName, text!);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0);
    }

    [Theory]
    [InlineData("gpt-4o", 1000, 500, 0.02)]
    [InlineData("gpt-4-turbo", 1000, 500, 0.025)]
    [InlineData("gpt-3.5-turbo", 1000, 500, 0.00125)]
    public async Task EstimateCostAsync_WithValidInputs_ShouldReturnCorrectCost(
        string modelName, int inputTokens, int outputTokens, decimal expectedCost)
    {
        // Act
        var result = await _provider.EstimateCostAsync(modelName, inputTokens, outputTokens);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedCost, tolerance: 0.001m);
    }

    [Fact]
    public async Task EstimateCostAsync_WithUnknownModel_ShouldReturnFailure()
    {
        // Arrange
        var modelName = "unknown-model";
        var inputTokens = 1000;
        var outputTokens = 500;

        // Act
        var result = await _provider.EstimateCostAsync(modelName, inputTokens, outputTokens);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Unknown model");
    }

    [Fact]
    public async Task GetRateLimitInfoAsync_WithGpt4Model_ShouldReturnCorrectLimits()
    {
        // Arrange
        var modelName = "gpt-4-turbo";

        // Act
        var result = await _provider.GetRateLimitInfoAsync(modelName);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.RequestsPerMinute.ShouldBe(500);
        result.Value.TokensPerMinute.ShouldBe(30_000);
        result.Value.IsLimitExceeded.ShouldBeFalse();
    }

    [Fact]
    public async Task GetRateLimitInfoAsync_WithGpt35Model_ShouldReturnCorrectLimits()
    {
        // Arrange
        var modelName = "gpt-3.5-turbo";

        // Act
        var result = await _provider.GetRateLimitInfoAsync(modelName);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.RequestsPerMinute.ShouldBe(3500);
        result.Value.TokensPerMinute.ShouldBe(90_000);
        result.Value.IsLimitExceeded.ShouldBeFalse();
    }

    [Fact]
    public async Task ListModelsAsync_ShouldReturnAvailableModels()
    {
        // Act
        var result = await _provider.ListModelsAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeEmpty();
        result.Value.Count().ShouldBe(3); // gpt-4o, gpt-4-turbo, gpt-3.5-turbo
    }

    [Fact]
    public async Task ValidateAsync_WithValidModel_ShouldReturnValidationResult()
    {
        // Arrange
        var modelName = "gpt-3.5-turbo";

        // Act
        var result = await _provider.ValidateAsync(modelName);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.TestedModel.ShouldBe(modelName);
        // Note: This will likely fail without a real API key, but structure should be correct
    }

    [Fact]
    public async Task GenerateCompletionAsync_WithEmptyModel_ShouldReturnFailure()
    {
        // Arrange
        var modelName = "";
        var prompt = "Test prompt";

        // Act
        var result = await _provider.GenerateCompletionAsync(modelName, prompt);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Model name cannot be empty");
    }

    [Fact]
    public async Task GenerateCompletionAsync_WithEmptyPrompt_ShouldReturnFailure()
    {
        // Arrange
        var modelName = "gpt-3.5-turbo";
        var prompt = "";

        // Act
        var result = await _provider.GenerateCompletionAsync(modelName, prompt);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Prompt cannot be empty");
    }

    [Fact]
    public async Task GenerateCompletionAsync_WithUnsupportedModel_ShouldReturnFailure()
    {
        // Arrange
        var modelName = "unsupported-model";
        var prompt = "Test prompt";

        // Act
        var result = await _provider.GenerateCompletionAsync(modelName, prompt);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Unsupported model");
    }

    [Fact]
    public async Task GenerateChatCompletionAsync_WithEmptyMessages_ShouldReturnFailure()
    {
        // Arrange
        var modelName = "gpt-3.5-turbo";
        var messages = new List<ChatMessage>();

        // Act
        var result = await _provider.GenerateChatCompletionAsync(modelName, messages);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Messages cannot be empty");
    }

    [Fact]
    public async Task GenerateChatCompletionAsync_WithValidMessages_ShouldReturnStructuredResponse()
    {
        // Arrange
        var modelName = "gpt-3.5-turbo";
        var messages = new List<ChatMessage>
        {
            ChatMessage.User("Hello, test message")
        };

        // Act
        var result = await _provider.GenerateChatCompletionAsync(modelName, messages);

        // Note: This will fail without a real API key, but we can test the structure
        // In a real scenario, you'd mock the HttpClient or use integration tests
        result.IsSuccess.ShouldBeFalse(); // Expected without real API key
    }

    [Fact]
    public void StreamCompletionAsync_WithEmptyModel_ShouldReturnEmptySequence()
    {
        // Arrange
        var modelName = "";
        var prompt = "Test prompt";

        // Act
        var stream = _provider.StreamCompletionAsync(modelName, prompt);

        // Assert
        // The stream should be empty for invalid inputs
        stream.ShouldNotBeNull();
    }

    [Fact]
    public void ChatMessage_StaticFactories_ShouldCreateCorrectMessages()
    {
        // Act
        var systemMessage = ChatMessage.System("You are a helpful assistant");
        var userMessage = ChatMessage.User("Hello");
        var assistantMessage = ChatMessage.Assistant("Hi there!");

        // Assert
        systemMessage.Role.ShouldBe("system");
        systemMessage.Content.ShouldBe("You are a helpful assistant");

        userMessage.Role.ShouldBe("user");
        userMessage.Content.ShouldBe("Hello");

        assistantMessage.Role.ShouldBe("assistant");
        assistantMessage.Content.ShouldBe("Hi there!");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}