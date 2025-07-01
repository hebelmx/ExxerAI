using ExxerAI.Domain;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service interface for interacting with Large Language Models
/// </summary>
public interface ILLMService
{
    /// <summary>
    /// Generates a text response using the specified language model
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="prompt">The input prompt</param>
    /// <param name="parameters">Optional generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The generated response</returns>
    Task<Result<LLMResponse>> GenerateTextAsync(
        Guid modelId,
        string prompt,
        LLMParameters? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Continues a conversation with a language model
    /// </summary>
    /// <param name="conversationId">The conversation identifier</param>
    /// <param name="message">The new message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The model's response</returns>
    Task<Result<ConversationMessage>> ContinueConversationAsync(
        Guid conversationId,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new conversation with a language model
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="title">Optional conversation title</param>
    /// <param name="systemPrompt">Optional system prompt</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created conversation</returns>
    Task<Result<Conversation>> CreateConversationAsync(
        Guid agentId,
        Guid modelId,
        string? title = null,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the cost of a language model request
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="inputTokens">The number of input tokens</param>
    /// <param name="outputTokens">The estimated number of output tokens</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The estimated cost</returns>
    Task<Result<decimal>> EstimateCostAsync(
        Guid modelId,
        int inputTokens,
        int outputTokens,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of tokens in a text string for a specific model
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="text">The text to count tokens for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The token count</returns>
    Task<Result<int>> CountTokensAsync(
        Guid modelId,
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams a text response using the specified language model
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="prompt">The input prompt</param>
    /// <param name="parameters">Optional generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of response chunks</returns>
    IAsyncEnumerable<LLMResponseChunk> StreamTextAsync(
        Guid modelId,
        string prompt,
        LLMParameters? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether a language model is available and accessible
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The validation result</returns>
    Task<Result<bool>> ValidateModelAsync(Guid modelId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents parameters for language model generation
/// </summary>
public class LLMParameters
{
    /// <summary>
    /// Gets or sets the temperature for generation (0.0-2.0)
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets the maximum number of tokens to generate
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the top-p value for nucleus sampling
    /// </summary>
    public double TopP { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the frequency penalty
    /// </summary>
    public double FrequencyPenalty { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the presence penalty
    /// </summary>
    public double PresencePenalty { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets stop sequences
    /// </summary>
    public ICollection<string> StopSequences { get; init; } = new List<string>();

    /// <summary>
    /// Gets or sets custom parameters
    /// </summary>
    public Dictionary<string, object> CustomParameters { get; init; } = new();
}

/// <summary>
/// Represents a response from a language model
/// </summary>
public class LLMResponse
{
    /// <summary>
    /// Gets or sets the generated content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of tokens in the input
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// Gets or sets the number of tokens in the output
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// Gets or sets the total tokens used
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;

    /// <summary>
    /// Gets or sets the estimated cost of the request
    /// </summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>
    /// Gets or sets the response time in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the reason the generation finished
    /// </summary>
    public string FinishReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets response metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a chunk of streamed response from a language model
/// </summary>
public class LLMResponseChunk
{
    /// <summary>
    /// Gets or sets the content chunk
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is the final chunk
    /// </summary>
    public bool IsComplete { get; set; } = false;

    /// <summary>
    /// Gets or sets the chunk index
    /// </summary>
    public int ChunkIndex { get; set; }

    /// <summary>
    /// Gets or sets chunk metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
} 