namespace ExxerAI.Orchestration.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.Helpers;

/// <summary>
/// Represents performance metrics for the orchestration engine
/// </summary>
public class OrchestrationMetrics
{
    /// <summary>
    /// Gets or sets the number of active agents
    /// </summary>
    public int ActiveAgents { get; set; }

    /// <summary>
    /// Gets or sets the number of pending tasks
    /// </summary>
    public int PendingTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of running workflows
    /// </summary>
    public int RunningWorkflows { get; set; }

    /// <summary>
    /// Gets or sets the number of completed tasks in the last hour
    /// </summary>
    public int CompletedTasksLastHour { get; set; }

    /// <summary>
    /// Gets or sets the average task execution time in milliseconds
    /// </summary>
    public double AverageTaskExecutionTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the engine uptime
    /// </summary>
    public TimeSpan Uptime { get; set; }

    /// <summary>
    /// Gets or sets custom metrics
    /// </summary>
    public Dictionary<string, object> CustomMetrics { get; init; } = new();
}