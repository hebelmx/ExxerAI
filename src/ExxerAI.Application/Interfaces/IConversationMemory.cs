using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for persistent conversation memory and chat history management
/// Enables autonomous agents to maintain context across interactions
/// </summary>
public interface IConversationMemory
{
    /// <summary>
    /// Stores a chat message in persistent memory
    /// </summary>
    /// <param name="message">The chat message to store</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Storage result with message ID</returns>
    Task<ConversationStorageResult> StoreMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves conversation history for a specific session
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="limit">Maximum number of messages to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversation history with messages</returns>
    Task<ConversationHistory> GetConversationHistoryAsync(string sessionId, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches conversation history for specific content or patterns
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="searchQuery">Search query string</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results with relevance scores</returns>
    Task<IEnumerable<ConversationSearchResult>> SearchConversationsAsync(string userId, string searchQuery, int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates context summary for ongoing conversation
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversation context for agent use</returns>
    Task<ConversationContext> GetConversationContextAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new conversation session
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="sessionType">Type of conversation session</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New conversation session</returns>
    Task<ConversationSession> CreateSessionAsync(string userId, string sessionType = "Standard", CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends a conversation session and generates summary
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversation summary</returns>
    Task<ConversationSummary> EndSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes conversation history for privacy compliance
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion result</returns>
    Task<bool> DeleteConversationAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user's conversation statistics and patterns
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="timeRange">Date range for statistics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User interaction patterns and statistics</returns>
    Task<IEnumerable<UserInteraction>> GetUserInteractionPatternsAsync(string userId, DateTimeRange timeRange, CancellationToken cancellationToken = default);
} 