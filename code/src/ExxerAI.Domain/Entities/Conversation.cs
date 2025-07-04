namespace ExxerAI.Domain.Entities;

/// <summary>
/// Represents a conversation session with an LLM
/// </summary>
public class Conversation
{
    /// <summary>
    /// Gets or sets the unique identifier for the conversation
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the conversation title
    /// </summary>
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the system prompt for the conversation
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agent participating in this conversation
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// Gets or sets the agent participating in this conversation
    /// </summary>
    public Agent? Agent { get; set; }

    /// <summary>
    /// Gets or sets the language model used for this conversation
    /// </summary>
    public Guid LanguageModelId { get; set; }

    /// <summary>
    /// Gets or sets the language model used for this conversation
    /// </summary>
    public LanguageModel? LanguageModel { get; set; }

    /// <summary>
    /// Gets or sets the conversation agentStatus
    /// </summary>
    public ConversationStatus Status { get; set; } = ConversationStatus.Active;

    /// <summary>
    /// Gets or sets when the conversation was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the conversation was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the collection of messages in this conversation
    /// </summary>
    public ICollection<ConversationMessage> Messages { get; init; } = new List<ConversationMessage>();

    /// <summary>
    /// Gets or sets conversation metadata
    /// </summary>
    public ConversationMetadata Metadata { get; set; } = new();
}