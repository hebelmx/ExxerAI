using ExxerAI.Domain.Helpers;
using ExxerAI.Domain.Entities;

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