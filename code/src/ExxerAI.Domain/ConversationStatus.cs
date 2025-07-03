namespace ExxerAI.Domain;

/// <summary>
/// Represents the possible states of a conversation
/// </summary>
public enum ConversationStatus
{
    /// <summary>
    /// Conversation is active and accepting messages
    /// </summary>
    Active,
    
    /// <summary>
    /// Conversation is paused
    /// </summary>
    Paused,
    
    /// <summary>
    /// Conversation has completed
    /// </summary>
    Completed,
    
    /// <summary>
    /// Conversation was archived
    /// </summary>
    Archived
}