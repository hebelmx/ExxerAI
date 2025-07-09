namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// Represents metadata for task execution
/// </summary>
public class TaskMetadata
{
    /// <summary>
    /// Gets or sets custom metadata properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = [];

    /// <summary>
    /// Gets or sets execution context information
    /// </summary>
    public Dictionary<string, string> Context { get; init; } = [];

    /// <summary>
    /// Gets or sets performance metrics
    /// </summary>
    public Dictionary<string, double> Metrics { get; init; } = [];
}