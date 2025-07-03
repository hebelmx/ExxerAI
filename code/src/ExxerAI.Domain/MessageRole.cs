namespace ExxerAI.Domain;

/// <summary>
/// Represents the role of a message in a conversation
/// </summary>
public enum MessageRole
{
    /// <summary>
    /// Message from the system
    /// </summary>
    System,
    
    /// <summary>
    /// Message from the user
    /// </summary>
    User,
    
    /// <summary>
    /// Message from the assistant/agent
    /// </summary>
    Assistant,
    
    /// <summary>
    /// Message from a function call
    /// </summary>
    Function
}