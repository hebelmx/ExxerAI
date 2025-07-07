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

    /// <summary>
    /// Gets or sets the number of tokens used in the conversation
    /// </summary>
    public int TokensUsed { get; set; } = 0;

    /// <summary>
    /// Gets or sets the cost of the conversation
    /// </summary>
    public decimal Cost { get; set; } = 0;

    /// <summary>
    /// Gets or sets the response time in milliseconds
    /// </summary>
    public int ResponseTime { get; set; } = 0;

    /// <summary>
    /// Gets or sets the model name used in the conversation
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider name used in the conversation
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;
}