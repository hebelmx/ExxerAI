namespace ExxerAI.Domain;

/// <summary>
/// Represents a message in a conversation
/// </summary>
public class ConversationMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for the message
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the conversation this message belongs to
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// Gets or sets the conversation this message belongs to
    /// </summary>
    public Conversation? Conversation { get; set; }

    /// <summary>
    /// Gets or sets the message role
    /// </summary>
    public MessageRole Role { get; set; }

    /// <summary>
    /// Gets or sets the message content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the message was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the message was created (alias for CreatedAt)
    /// </summary>
    public DateTime Timestamp 
    { 
        get => CreatedAt; 
        set => CreatedAt = value; 
    }

    /// <summary>
    /// Gets or sets the token count for this message
    /// </summary>
    public int TokenCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets message metadata
    /// </summary>
    public MessageMetadata Metadata { get; set; } = new();
}