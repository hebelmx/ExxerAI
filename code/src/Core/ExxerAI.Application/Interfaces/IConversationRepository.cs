using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for conversation entities
/// </summary>
public interface IConversationRepository : IRepository<Conversation>
{
    /// <summary>
    /// Gets conversations for a specific agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="status">Optional agentStatus filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of conversations for the agent</returns>
    Task<Result<IEnumerable<Conversation>>> GetByAgentAsync(
        Guid agentId,
        ConversationStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets conversations using a specific language model
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of conversations using the model</returns>
    Task<Result<IEnumerable<Conversation>>> GetByLanguageModelAsync(
        Guid modelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets messages for a specific conversation
    /// </summary>
    /// <param name="conversationId">The conversation identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of messages in the conversation</returns>
    Task<Result<IEnumerable<ConversationMessage>>> GetMessagesAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a message to a conversation
    /// </summary>
    /// <param name="message">The message to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added message</returns>
    Task<Result<ConversationMessage>> AddMessageAsync(
        ConversationMessage message,
        CancellationToken cancellationToken = default);
}