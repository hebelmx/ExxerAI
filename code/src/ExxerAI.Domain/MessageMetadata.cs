namespace ExxerAI.Domain;

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
}