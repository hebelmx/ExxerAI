using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;
using System.Runtime.CompilerServices;

namespace ExxerAI.Application.Services;

public class LLMService : ILLMService
{
private readonly ILanguageModelRepository _modelRepository;
private readonly IConversationRepository _conversationRepository;

public LLMService(ILanguageModelRepository modelRepository, IConversationRepository conversationRepository)
{
_modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
_conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
}

public async Task<Result<LLMResponse>> GenerateTextAsync(Guid modelId, string prompt, LLMParameters? parameters = null, CancellationToken cancellationToken = default)
{
try
{
if (string.IsNullOrWhiteSpace(prompt))
return Result<LLMResponse>.WithFailure("Prompt cannot be null or empty");

var modelResult = await _modelRepository.GetByIdAsync(modelId, cancellationToken);
if (modelResult.IsFailure) return Result<LLMResponse>.WithFailure($"Model {modelId} not found");

var response = new LLMResponse
{
Content = $"Generated response for: {prompt}",
InputTokens = prompt.Length / 4, // Rough token estimation
OutputTokens = 50,
EstimatedCost = 0.001m,
ResponseTimeMs = 500,
FinishReason = "completed"
};

return Result<LLMResponse>.Success(response);
}
catch (Exception ex)
{
return Result<LLMResponse>.WithFailure($"Error generating text: {ex.Message}");
}
}

public async Task<Result<ConversationMessage>> ContinueConversationAsync(Guid conversationId, string message, CancellationToken cancellationToken = default)
{
try
{
if (string.IsNullOrWhiteSpace(message))
return Result<ConversationMessage>.WithFailure("Message cannot be null or empty");

var userMessage = new ConversationMessage
{
ConversationId = conversationId,
Role = MessageRole.User,
Content = message,
Timestamp = DateTime.UtcNow
};

var addResult = await _conversationRepository.AddMessageAsync(userMessage, cancellationToken);
if (addResult.IsFailure) return Result<ConversationMessage>.WithFailure(addResult.Errors);

var assistantMessage = new ConversationMessage
{
ConversationId = conversationId,
Role = MessageRole.Assistant,
Content = $"Response to: {message}",
Timestamp = DateTime.UtcNow
};

var assistantResult = await _conversationRepository.AddMessageAsync(assistantMessage, cancellationToken);
return assistantResult.IsFailure ? Result<ConversationMessage>.WithFailure(assistantResult.Errors) : Result<ConversationMessage>.Success(assistantMessage);
}
catch (Exception ex)
{
return Result<ConversationMessage>.WithFailure($"Error continuing conversation: {ex.Message}");
}
}

public async Task<Result<Conversation>> CreateConversationAsync(Guid agentId, Guid modelId, string? title = null, string? systemPrompt = null, CancellationToken cancellationToken = default)
{
try
{
var conversation = new Conversation
{
AgentId = agentId,
LanguageModelId = modelId,
Title = title ?? "New Conversation",
SystemPrompt = systemPrompt ?? string.Empty,
Status = ConversationStatus.Active,
CreatedAt = DateTime.UtcNow
};

var result = await _conversationRepository.AddAsync(conversation, cancellationToken);
return result.IsFailure ? Result<Conversation>.WithFailure(result.Errors) : Result<Conversation>.Success(conversation);
}
catch (Exception ex)
{
return Result<Conversation>.WithFailure($"Error creating conversation: {ex.Message}");
}
}

public async Task<Result<decimal>> EstimateCostAsync(Guid modelId, int inputTokens, int outputTokens, CancellationToken cancellationToken = default)
{
try
{
var modelResult = await _modelRepository.GetByIdAsync(modelId, cancellationToken);
if (modelResult.IsFailure) return Result<decimal>.WithFailure($"Model {modelId} not found");

// Basic cost estimation (would use real pricing from model)
decimal cost = (inputTokens * 0.00001m) + (outputTokens * 0.00002m);
return Result<decimal>.Success(cost);
}
catch (Exception ex)
{
return Result<decimal>.WithFailure($"Error estimating cost: {ex.Message}");
}
}

public async Task<Result<int>> CountTokensAsync(Guid modelId, string text, CancellationToken cancellationToken = default)
{
try
{
if (string.IsNullOrEmpty(text))
return Result<int>.Success(0);

var modelResult = await _modelRepository.GetByIdAsync(modelId, cancellationToken);
if (modelResult.IsFailure) return Result<int>.WithFailure($"Model {modelId} not found");

// Basic token counting (would use real tokenizer)
int tokenCount = text.Length / 4; // Rough estimation
return Result<int>.Success(tokenCount);
}
catch (Exception ex)
{
return Result<int>.WithFailure($"Error counting tokens: {ex.Message}");
}
}

public async IAsyncEnumerable<LLMResponseChunk> StreamTextAsync(Guid modelId, string prompt, LLMParameters? parameters = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
var modelResult = await _modelRepository.GetByIdAsync(modelId, cancellationToken);
if (modelResult.IsFailure) yield break;

// Simulate streaming response
string[] chunks = { "Hello", " world", " from", " streaming", " LLM!" };

for (int i = 0; i < chunks.Length; i++)
{
yield return new LLMResponseChunk
{
Content = chunks[i],
ChunkIndex = i,
IsComplete = i == chunks.Length - 1
};

await Task.Delay(100, cancellationToken); // Simulate processing delay
}
}

public async Task<Result<bool>> ValidateModelAsync(Guid modelId, CancellationToken cancellationToken = default)
{
try
{
var modelResult = await _modelRepository.GetByIdAsync(modelId, cancellationToken);
return Result<bool>.Success(modelResult.IsSuccess);
}
catch (Exception ex)
{
return Result<bool>.WithFailure($"Error validating model: {ex.Message}");
}
}
}
