namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// Represents metadata for a message
/// </summary>
public class MessageMetadata
{
    /// <summary>
    /// Gets or sets the response time in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; set; } = 0;

    /// <summary>
    /// Gets or sets the model parameters used for generation
    /// </summary>
    public Dictionary<string, object> ModelParameters { get; init; } = new();

    /// <summary>
    /// Gets or sets custom metadata properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();

    /// <summary>
    /// Gets or sets the number of tokens used in the message
    /// </summary>
    public int TokensUsed { get; set; } = 0;

    /// <summary>
    /// Gets or sets the cost of the message
    /// </summary>
    public decimal Cost { get; set; } = 0;

    /// <summary>
    /// Gets or sets the response time in milliseconds
    /// </summary>
    public int ResponseTime { get; set; } = 0;

    /// <summary>
    /// Gets or sets the model name used for the message
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider name used for the message
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;
}