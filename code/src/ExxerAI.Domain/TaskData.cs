namespace ExxerAI.Domain;

/// <summary>
/// Represents data associated with a task
/// </summary>
public class TaskData
{
    /// <summary>
    /// Gets or sets the content type of the data
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Gets or sets the raw data content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional properties for the data
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
}