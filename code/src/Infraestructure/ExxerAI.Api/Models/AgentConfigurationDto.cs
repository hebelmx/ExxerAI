namespace ExxerAI.Api.Models;

/// <summary>
/// DTO for agent configuration
/// </summary>
public class AgentConfigurationDto
{
    /// <summary>
    /// Gets or sets the timeout for task execution in seconds
    /// </summary>
    public int TaskTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the maximum retries for failed tasks
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the agent's priority level
    /// </summary>
    public int Priority { get; set; } = 1;

    /// <summary>
    /// Gets or sets custom configuration properties
    /// </summary>
    public Dictionary<string, object> CustomProperties { get; set; } = [];
}