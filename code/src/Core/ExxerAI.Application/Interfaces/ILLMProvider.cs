using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for Large Language Model providers that enables pluggable LLM integrations
/// Supports multiple providers like OpenAI, Azure OpenAI, Ollama, and HuggingFace
/// </summary>
public interface ILLMProvider
{
    /// <summary>
    /// Gets the provider name (e.g., "OpenAI", "Azure OpenAI", "Ollama")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the supported model identifiers for this provider
    /// </summary>
    IEnumerable<string> SupportedModels { get; }

    /// <summary>
    /// Gets whether this provider supports streaming responses
    /// </summary>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Gets whether this provider supports function calling
    /// </summary>
    bool SupportsFunctionCalling { get; }

    /// <summary>
    /// Generates a text completion using the specified model and parameters
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="prompt">The input prompt or system message</param>
    /// <param name="parameters">Generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The generated response</returns>
    Task<Result<LLMResponse>> GenerateCompletionAsync(
        string modelName,
        string prompt,
        LLMParameters? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a chat completion using the specified model and messages
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="messages">The conversation messages</param>
    /// <param name="parameters">Generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The generated response</returns>
    Task<Result<LLMResponse>> GenerateChatCompletionAsync(
        string modelName,
        IEnumerable<ChatMessage> messages,
        LLMParameters? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams a text completion using the specified model and parameters
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="prompt">The input prompt</param>
    /// <param name="parameters">Generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of response chunks</returns>
    IAsyncEnumerable<LLMResponseChunk> StreamCompletionAsync(
        string modelName,
        string prompt,
        LLMParameters? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams a chat completion using the specified model and messages
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="messages">The conversation messages</param>
    /// <param name="parameters">Generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of response chunks</returns>
    IAsyncEnumerable<LLMResponseChunk> StreamChatCompletionAsync(
        string modelName,
        IEnumerable<ChatMessage> messages,
        LLMParameters? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of tokens in the given text for the specified model
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="text">The text to count tokens for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The token count</returns>
    Task<Result<int>> CountTokensAsync(
        string modelName,
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the cost for the given token counts using the specified model
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="inputTokens">Number of input tokens</param>
    /// <param name="outputTokens">Number of output tokens</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The estimated cost in USD</returns>
    Task<Result<decimal>> EstimateCostAsync(
        string modelName,
        int inputTokens,
        int outputTokens,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the provider is properly configured and the model is accessible
    /// </summary>
    /// <param name="modelName">The model name to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with any error messages</returns>
    Task<Result<ProviderValidationResult>> ValidateAsync(
        string modelName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the rate limiting information for this provider
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rate limiting information</returns>
    Task<Result<RateLimitInfo>> GetRateLimitInfoAsync(
        string modelName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists available models for this provider
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of available models</returns>
    Task<Result<IEnumerable<LLMModelInfo>>> ListModelsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a chat message for conversation-based completions
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Gets or sets the role of the message sender (system, user, assistant, function)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content of the message
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the function (if role is function)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the message
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = [];

    /// <summary>
    /// Creates a system message
    /// </summary>
    /// <param name="content">The system message content</param>
    /// <returns>A new ChatMessage with system role</returns>
    public static ChatMessage System(string content) => new() { Role = "system", Content = content };

    /// <summary>
    /// Creates a user message
    /// </summary>
    /// <param name="content">The user message content</param>
    /// <returns>A new ChatMessage with user role</returns>
    public static ChatMessage User(string content) => new() { Role = "user", Content = content };

    /// <summary>
    /// Creates an assistant message
    /// </summary>
    /// <param name="content">The assistant message content</param>
    /// <returns>A new ChatMessage with assistant role</returns>
    public static ChatMessage Assistant(string content) => new() { Role = "assistant", Content = content };
}

/// <summary>
/// Represents validation result for a provider
/// </summary>
public class ProviderValidationResult
{
    /// <summary>
    /// Gets or sets whether the provider is valid and accessible
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets validation error messages
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Gets or sets validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// Gets or sets the tested model information
    /// </summary>
    public string? TestedModel { get; set; }

    /// <summary>
    /// Gets or sets the response time for the validation test
    /// </summary>
    public TimeSpan ResponseTime { get; set; }
}

/// <summary>
/// Represents rate limiting information for a provider
/// </summary>
public class RateLimitInfo
{
    /// <summary>
    /// Gets or sets the requests per minute limit
    /// </summary>
    public int RequestsPerMinute { get; set; }

    /// <summary>
    /// Gets or sets the tokens per minute limit
    /// </summary>
    public int TokensPerMinute { get; set; }

    /// <summary>
    /// Gets or sets the remaining requests in the current window
    /// </summary>
    public int RemainingRequests { get; set; }

    /// <summary>
    /// Gets or sets the remaining tokens in the current window
    /// </summary>
    public int RemainingTokens { get; set; }

    /// <summary>
    /// Gets or sets when the rate limit window resets
    /// </summary>
    public DateTime ResetTime { get; set; }

    /// <summary>
    /// Gets or sets whether the rate limit is currently exceeded
    /// </summary>
    public bool IsLimitExceeded { get; set; }
}

/// <summary>
/// Represents information about an available LLM model
/// </summary>
public class LLMModelInfo
{
    /// <summary>
    /// Gets or sets the model identifier/name
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum context length in tokens
    /// </summary>
    public int MaxContextLength { get; set; }

    /// <summary>
    /// Gets or sets the maximum output tokens
    /// </summary>
    public int MaxOutputTokens { get; set; }

    /// <summary>
    /// Gets or sets the input token cost per 1K tokens
    /// </summary>
    public decimal InputTokenCostPer1K { get; set; }

    /// <summary>
    /// Gets or sets the output token cost per 1K tokens
    /// </summary>
    public decimal OutputTokenCostPer1K { get; set; }

    /// <summary>
    /// Gets or sets whether the model supports function calling
    /// </summary>
    public bool SupportsFunctionCalling { get; set; }

    /// <summary>
    /// Gets or sets whether the model supports streaming
    /// </summary>
    public bool SupportsStreaming { get; set; }

    /// <summary>
    /// Gets or sets the model capabilities
    /// </summary>
    public List<string> Capabilities { get; set; } = [];

    /// <summary>
    /// Gets or sets additional model metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];
}