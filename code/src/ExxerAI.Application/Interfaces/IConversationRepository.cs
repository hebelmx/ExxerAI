namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for conversation entities
/// </summary>
public interface IConversationRepository : IRepository<Domain.Conversation>
{
    /// <summary>
    /// Gets conversations for a specific agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="status">Optional agentStatus filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of conversations for the agent</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<Domain.Conversation>>> GetByAgentAsync(
        Guid agentId,
        Domain.ConversationStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets conversations using a specific language model
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of conversations using the model</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<Domain.Conversation>>> GetByLanguageModelAsync(
        Guid modelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets messages for a specific conversation
    /// </summary>
    /// <param name="conversationId">The conversation identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of messages in the conversation</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<Domain.ConversationMessage>>> GetMessagesAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a message to a conversation
    /// </summary>
    /// <param name="message">The message to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added message</returns>
    Task<ExxerAI.Domain.Result<Domain.ConversationMessage>> AddMessageAsync(
        Domain.ConversationMessage message,
        CancellationToken cancellationToken = default);
}