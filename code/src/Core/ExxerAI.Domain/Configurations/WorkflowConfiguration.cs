namespace ExxerAI.Domain.Configurations;

/// <summary>
/// Represents configuration settings for a workflow
/// </summary>
public class WorkflowConfiguration
{
    /// <summary>
    /// Gets or sets the maximum execution time in seconds
    /// </summary>
    public int MaxExecutionTimeSeconds { get; set; } = 3600;

    /// <summary>
    /// Gets or sets whether parallel execution is allowed
    /// </summary>
    public bool AllowParallelExecution { get; set; } = true;

    /// <summary>
    /// Gets or sets the notification settings
    /// </summary>
    public NotificationSettings Notifications { get; set; } = new();

    /// <summary>
    /// Gets or sets custom configuration properties
    /// </summary>
    public Dictionary<string, object> CustomProperties { get; init; } = new();
}