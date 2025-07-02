using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Implementation tests for LLMService - tests the real service implementation with mocked dependencies
/// </summary>
public class LLMServiceImplementationTests
{
private readonly ILanguageModelRepository _mockModelRepository;
private readonly IConversationRepository _mockConversationRepository;
private readonly LLMService _service;

public LLMServiceImplementationTests()
{
_mockModelRepository = Substitute.For<ILanguageModelRepository>();
_mockConversationRepository = Substitute.For<IConversationRepository>();
_service = new LLMService(_mockModelRepository, _mockConversationRepository);
}

[Fact]
public void Constructor_Should_ThrowArgumentNullException_When_ModelRepositoryIsNull()
{
// Act & Assert
Should.Throw<ArgumentNullException>(() => new LLMService(null!, _mockConversationRepository))
.ParamName.ShouldBe("modelRepository");
}

[Fact]
public void Constructor_Should_ThrowArgumentNullException_When_ConversationRepositoryIsNull()
{
// Act & Assert
Should.Throw<ArgumentNullException>(() => new LLMService(_mockModelRepository, null!))
.ParamName.ShouldBe("conversationRepository");
}

[Fact]
public async Task GenerateTextAsync_Should_ReturnFailure_When_PromptIsEmpty()
{
// Act
var result = await _service.GenerateTextAsync(Guid.NewGuid(), "");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe("Prompt cannot be null or empty");
}

[Fact]
public async Task GenerateTextAsync_Should_ReturnFailure_When_PromptIsWhitespace()
{
// Act
var result = await _service.GenerateTextAsync(Guid.NewGuid(), "   ");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe("Prompt cannot be null or empty");
}

[Fact]
public async Task GenerateTextAsync_Should_ReturnFailure_When_ModelNotFound()
{
// Arrange
var modelId = Guid.NewGuid();
_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Returns(Result<LanguageModel>.WithFailure("Model not found"));

// Act
var result = await _service.GenerateTextAsync(modelId, "test prompt");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe($"Model {modelId} not found");
}

[Fact]
public async Task GenerateTextAsync_Should_ReturnSuccess_When_ValidInput()
{
// Arrange
var modelId = Guid.NewGuid();
var prompt = "Generate a response";
var model = new LanguageModel { Id = modelId, Name = "TestModel" };

_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Returns(Result<LanguageModel>.WithSuccess(model));

// Act
var result = await _service.GenerateTextAsync(modelId, prompt);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldNotBeNull();
result.Value.Content.ShouldStartWith("Generated response for:");
result.Value.InputTokens.ShouldBeGreaterThan(0);
result.Value.OutputTokens.ShouldBeGreaterThan(0);
result.Value.EstimatedCost.ShouldBeGreaterThan(0);
}

[Fact]
public async Task GenerateTextAsync_Should_HandleExceptions()
{
// Arrange
var modelId = Guid.NewGuid();
_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Throws(new InvalidOperationException("Test exception"));

// Act
var result = await _service.GenerateTextAsync(modelId, "test prompt");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldStartWith("Error generating text:");
}

[Fact]
public async Task ContinueConversationAsync_Should_ReturnFailure_When_MessageIsEmpty()
{
// Act
var result = await _service.ContinueConversationAsync(Guid.NewGuid(), "");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe("Message cannot be null or empty");
}

[Fact]
public async Task ContinueConversationAsync_Should_ReturnFailure_When_MessageIsWhitespace()
{
// Act
var result = await _service.ContinueConversationAsync(Guid.NewGuid(), "   ");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe("Message cannot be null or empty");
}

[Fact]
public async Task ContinueConversationAsync_Should_ReturnFailure_When_UserMessageAddFails()
{
// Arrange
var conversationId = Guid.NewGuid();
_mockConversationRepository.AddMessageAsync(Arg.Any<ConversationMessage>(), Arg.Any<CancellationToken>())
.Returns(Result<ConversationMessage>.WithFailure("Failed to add message"));

// Act
var result = await _service.ContinueConversationAsync(conversationId, "test message");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe("Failed to add user message");
}

[Fact]
public async Task ContinueConversationAsync_Should_ReturnSuccess_When_ValidInput()
{
// Arrange
var conversationId = Guid.NewGuid();
var message = "Hello assistant";

_mockConversationRepository.AddMessageAsync(Arg.Any<ConversationMessage>(), Arg.Any<CancellationToken>())
.Returns(callInfo => Result<ConversationMessage>.WithSuccess(callInfo.Arg<ConversationMessage>()));

// Act
var result = await _service.ContinueConversationAsync(conversationId, message);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldNotBeNull();
result.Value.ConversationId.ShouldBe(conversationId);
result.Value.Role.ShouldBe(MessageRole.Assistant);
result.Value.Content.ShouldStartWith("Response to:");
}

[Fact]
public async Task CreateConversationAsync_Should_ReturnSuccess_When_ValidParameters()
{
// Arrange
var agentId = Guid.NewGuid();
var modelId = Guid.NewGuid();
var title = "Test Conversation";
var systemPrompt = "You are a helpful assistant";

_mockConversationRepository.AddAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>())
.Returns(callInfo => Result<Conversation>.WithSuccess(callInfo.Arg<Conversation>()));

// Act
var result = await _service.CreateConversationAsync(agentId, modelId, title, systemPrompt);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldNotBeNull();
result.Value.AgentId.ShouldBe(agentId);
result.Value.LanguageModelId.ShouldBe(modelId);
result.Value.Title.ShouldBe(title);
result.Value.SystemPrompt.ShouldBe(systemPrompt);
result.Value.Status.ShouldBe(ConversationStatus.Active);
}

[Fact]
public async Task CreateConversationAsync_Should_UseDefaultValues_When_OptionalParametersNull()
{
// Arrange
var agentId = Guid.NewGuid();
var modelId = Guid.NewGuid();

_mockConversationRepository.AddAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>())
.Returns(callInfo => Result<Conversation>.WithSuccess(callInfo.Arg<Conversation>()));

// Act
var result = await _service.CreateConversationAsync(agentId, modelId);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.Title.ShouldBe("New Conversation");
result.Value.SystemPrompt.ShouldBe(string.Empty);
}

[Fact]
public async Task CreateConversationAsync_Should_ReturnFailure_When_RepositoryFails()
{
// Arrange
var agentId = Guid.NewGuid();
var modelId = Guid.NewGuid();

_mockConversationRepository.AddAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>())
.Returns(Result<Conversation>.WithFailure("Database error"));

// Act
var result = await _service.CreateConversationAsync(agentId, modelId);

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe("Database error");
}

[Fact]
public async Task EstimateCostAsync_Should_ReturnFailure_When_ModelNotFound()
{
// Arrange
var modelId = Guid.NewGuid();
_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Returns(Result<LanguageModel>.WithFailure("Model not found"));

// Act
var result = await _service.EstimateCostAsync(modelId, 100, 50);

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe($"Model {modelId} not found");
}

[Fact]
public async Task EstimateCostAsync_Should_ReturnSuccess_When_ValidInput()
{
// Arrange
var modelId = Guid.NewGuid();
var model = new LanguageModel { Id = modelId, Name = "TestModel" };

_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Returns(Result<LanguageModel>.WithSuccess(model));

// Act
var result = await _service.EstimateCostAsync(modelId, 1000, 500);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBeGreaterThan(0);
// 1000 * 0.00001 + 500 * 0.00002 = 0.02
result.Value.ShouldBe(0.02m);
}

[Fact]
public async Task CountTokensAsync_Should_ReturnZero_When_TextIsEmpty()
{
// Arrange
var modelId = Guid.NewGuid();
var model = new LanguageModel { Id = modelId, Name = "TestModel" };

_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Returns(Result<LanguageModel>.WithSuccess(model));

// Act
var result = await _service.CountTokensAsync(modelId, "");

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBe(0);
}

[Fact]
public async Task CountTokensAsync_Should_ReturnZero_When_TextIsNull()
{
// Arrange
var modelId = Guid.NewGuid();
var model = new LanguageModel { Id = modelId, Name = "TestModel" };

_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Returns(Result<LanguageModel>.WithSuccess(model));

// Act
var result = await _service.CountTokensAsync(modelId, null!);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBe(0);
}

[Fact]
public async Task CountTokensAsync_Should_ReturnTokenCount_When_ValidText()
{
// Arrange
var modelId = Guid.NewGuid();
var text = "This is a test sentence with multiple words";
var model = new LanguageModel { Id = modelId, Name = "TestModel" };

_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Returns(Result<LanguageModel>.WithSuccess(model));

// Act
var result = await _service.CountTokensAsync(modelId, text);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBeGreaterThan(0);
result.Value.ShouldBe(text.Length / 4); // Based on implementation
}

[Fact]
public async Task CountTokensAsync_Should_ReturnFailure_When_ModelNotFound()
{
// Arrange
var modelId = Guid.NewGuid();
_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Returns(Result<LanguageModel>.WithFailure("Model not found"));

// Act
var result = await _service.CountTokensAsync(modelId, "test text");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe($"Model {modelId} not found");
}

[Fact]
public async Task StreamTextAsync_Should_YieldNothing_When_ModelNotFound()
{
// Arrange
var modelId = Guid.NewGuid();
_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Returns(Result<LanguageModel>.WithFailure("Model not found"));

// Act
var chunks = new List<LLMResponseChunk>();
await foreach (var chunk in _service.StreamTextAsync(modelId, "test prompt"))
{
chunks.Add(chunk);
}

// Assert
chunks.ShouldBeEmpty();
}

[Fact]
public async Task StreamTextAsync_Should_YieldChunks_When_ValidModel()
{
// Arrange
var modelId = Guid.NewGuid();
var model = new LanguageModel { Id = modelId, Name = "TestModel" };

_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Returns(Result<LanguageModel>.WithSuccess(model));

// Act
var chunks = new List<LLMResponseChunk>();
await foreach (var chunk in _service.StreamTextAsync(modelId, "test prompt"))
{
chunks.Add(chunk);
}

// Assert
chunks.ShouldNotBeEmpty();
chunks.Count.ShouldBe(5); // Based on implementation
chunks[0].Content.ShouldBe("Hello");
chunks[4].Content.ShouldBe(" LLM!");
chunks[4].IsComplete.ShouldBeTrue();
chunks[0].IsComplete.ShouldBeFalse();
}

[Fact]
public async Task ValidateModelAsync_Should_ReturnTrue_When_ModelExists()
{
// Arrange
var modelId = Guid.NewGuid();
var model = new LanguageModel { Id = modelId, Name = "TestModel" };

_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Returns(Result<LanguageModel>.WithSuccess(model));

// Act
var result = await _service.ValidateModelAsync(modelId);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBeTrue();
}

[Fact]
public async Task ValidateModelAsync_Should_ReturnFalse_When_ModelNotFound()
{
// Arrange
var modelId = Guid.NewGuid();
_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Returns(Result<LanguageModel>.WithFailure("Model not found"));

// Act
var result = await _service.ValidateModelAsync(modelId);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBeFalse();
}

[Fact]
public async Task ValidateModelAsync_Should_ReturnFailure_When_ExceptionThrown()
{
// Arrange
var modelId = Guid.NewGuid();
_mockModelRepository.GetByIdAsync(modelId, Arg.Any<CancellationToken>())
.Throws(new InvalidOperationException("Test exception"));

// Act
var result = await _service.ValidateModelAsync(modelId);

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldStartWith("Error validating model:");
}
}
