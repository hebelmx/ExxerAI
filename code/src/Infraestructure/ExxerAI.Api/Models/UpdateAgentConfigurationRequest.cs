using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Api.Models;

/// <summary>
/// Request model for updating agent configuration
/// </summary>
public class UpdateAgentConfigurationRequest
{
    /// <summary>
    /// Gets or sets the timeout for task execution in seconds
    /// </summary>
    [Range(1, 3600)]
    public int TaskTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the maximum retries for failed tasks
    /// </summary>
    [Range(0, 10)]
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the agent's priority level
    /// </summary>
    [Range(1, 10)]
    public int Priority { get; set; } = 1;

    /// <summary>
    /// Gets or sets custom configuration properties
    /// </summary>
    public Dictionary<string, object> CustomProperties { get; set; } = [];
}