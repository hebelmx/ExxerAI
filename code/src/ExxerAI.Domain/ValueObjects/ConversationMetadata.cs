namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// Represents metadata for a conversation
/// </summary>
public class ConversationMetadata
{
    /// <summary>
    /// Gets or sets the total token count for the conversation
    /// </summary>
    public int TotalTokens { get; set; } = 0;

    /// <summary>
    /// Gets or sets the estimated cost of the conversation
    /// </summary>
    public decimal EstimatedCost { get; set; } = 0;

    /// <summary>
    /// Gets or sets custom metadata properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
}