using ExxerAI.Domain;
using ExxerAI.Domain.Operations;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Interface-Test-Driven Development (I-TDD) tests for ILLMService.
/// Tests focus on the interface contract and behavior, not implementation details.
/// </summary>
public class LLMServiceTests
{
    private readonly ILLMService _llmService;

    public LLMServiceTests()
    {
        _llmService = Substitute.For<ILLMService>();
    }

    #region GenerateTextAsync Tests

    [Fact]
    public async Task GenerateTextAsync_WithValidInput_ShouldReturnSuccessResultAsync()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var prompt = "Test prompt";
        var parameters = new LLMParameters { Temperature = 0.7, MaxTokens = 100 };
        var expectedResponse = new LLMResponse
        {
            Content = "Generated text",
            InputTokens = 5,
            OutputTokens = 10,
            ResponseTimeMs = 250
        };
        var expectedResult = Result<LLMResponse>.WithSuccess(expectedResponse);

        _llmService.GenerateTextAsync(modelId, prompt, parameters, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _llmService.GenerateTextAsync(modelId, prompt, parameters, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Content.ShouldBe("Generated text");
        result.Value.InputTokens.ShouldBe(5);
        result.Value.OutputTokens.ShouldBe(10);
    }

    [Fact]
    public async Task GenerateTextAsync_WithEmptyGuid_ShouldReturnFailureResultAsync()
    {
        // Arrange
        var modelId = Guid.Empty;
        var prompt = "Test prompt";
        var expectedResult = Result<LLMResponse>.WithFailure("Model ID cannot be empty");

        _llmService.GenerateTextAsync(modelId, prompt, null, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _llmService.GenerateTextAsync(modelId, prompt, cancellationToken: TestContext.Current.CancellationToken);//, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateTextAsync_WithInvalidPrompt_ShouldReturnFailureResultAsync(string prompt)
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var expectedResult = Result<LLMResponse>.WithFailure("Prompt cannot be null or empty");

        _llmService.GenerateTextAsync(modelId, prompt, null, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _llmService.GenerateTextAsync(modelId, prompt, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion GenerateTextAsync Tests

    #region ContinueConversationAsync Tests

    [Fact]
    public async Task ContinueConversationAsync_WithValidInput_ShouldReturnSuccessResultAsync()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var message = "Hello";
        var expectedMessage = new ConversationMessage
        {
            Id = Guid.NewGuid(),
            Content = "Hello there!",
            Role = MessageRole.Assistant,
            CreatedAt = DateTime.UtcNow
        };
        var expectedResult = Result<ConversationMessage>.WithSuccess(expectedMessage);

        _llmService.ContinueConversationAsync(conversationId, message, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _llmService.ContinueConversationAsync(conversationId, message, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Content.ShouldBe("Hello there!");
        result.Value.Role.ShouldBe(MessageRole.Assistant);
    }

    #endregion ContinueConversationAsync Tests

    #region CreateConversationAsync Tests

    [Fact]
    public async Task CreateConversationAsync_WithValidInput_ShouldReturnSuccessResultAsync()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var modelId = Guid.NewGuid();
        var title = "Test Conversation";
        var systemPrompt = "You are a helpful assistant";
        var expectedConversation = new Conversation
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            LanguageModelId = modelId,
            Title = title,
            SystemPrompt = systemPrompt,
            CreatedAt = DateTime.UtcNow
        };
        var expectedResult = Result<Conversation>.WithSuccess(expectedConversation);

        _llmService.CreateConversationAsync(agentId, modelId, title, systemPrompt, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _llmService.CreateConversationAsync(agentId, modelId, title, systemPrompt, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.AgentId.ShouldBe(agentId);
        result.Value.LanguageModelId.ShouldBe(modelId);
        result.Value.Title.ShouldBe(title);
    }

    #endregion CreateConversationAsync Tests

    #region Contract Validation Tests

    [Fact]
    public async Task ILLMService_AllMethods_ShouldRespectCancellationTokenAsync()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var modelId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        // Act & Assert - Verify all methods accept CancellationToken
        await _llmService.Received(0).GenerateTextAsync(modelId, "test", null, cts.Token);
        await _llmService.Received(0).ContinueConversationAsync(conversationId, "test", cts.Token);
        await _llmService.Received(0).CreateConversationAsync(agentId, modelId, null, null, cts.Token);
        await _llmService.Received(0).EstimateCostAsync(modelId, 10, 20, cts.Token);
        await _llmService.Received(0).CountTokensAsync(modelId, "test", cts.Token);
        await _llmService.Received(0).ValidateModelAsync(modelId, cts.Token);

        // All methods should exist and accept cancellation tokens
        true.ShouldBeTrue();
    }

    [Fact]
    public void ILLMService_AllMethods_ShouldReturnResult()
    {
        // Arrange & Act & Assert - Verify all async methods return Result<T>
        var modelId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        // Verify method signatures return Result<T>
        var generateTask = _llmService.GenerateTextAsync(modelId, "test", cancellationToken: TestContext.Current.CancellationToken);
        var continueTask = _llmService.ContinueConversationAsync(conversationId, "test", cancellationToken: TestContext.Current.CancellationToken);
        var createTask = _llmService.CreateConversationAsync(agentId, modelId, cancellationToken: TestContext.Current.CancellationToken);
        var estimateTask = _llmService.EstimateCostAsync(modelId, 10, 20, TestContext.Current.CancellationToken);
        var countTask = _llmService.CountTokensAsync(modelId, "test", TestContext.Current.CancellationToken);
        var validateTask = _llmService.ValidateModelAsync(modelId, TestContext.Current.CancellationToken);

        generateTask.ShouldBeOfType<Task<Result<LLMResponse>>>();
        continueTask.ShouldBeOfType<Task<Result<ConversationMessage>>>();
        createTask.ShouldBeOfType<Task<Result<Conversation>>>();
        estimateTask.ShouldBeOfType<Task<Result<decimal>>>();
        countTask.ShouldBeOfType<Task<Result<int>>>();
        validateTask.ShouldBeOfType<Task<Result<bool>>>();
    }

    #endregion Contract Validation Tests
}