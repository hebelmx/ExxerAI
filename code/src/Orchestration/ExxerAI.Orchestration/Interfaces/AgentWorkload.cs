namespace ExxerAI.Orchestration.Interfaces;

using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.Operations;

/// <summary>
/// Represents the current workload of an agent
/// </summary>
public class AgentWorkload
{
    /// <summary>
    /// Gets or sets the agent identifier
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// Gets or sets the number of assigned tasks
    /// </summary>
    public int AssignedTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of running tasks
    /// </summary>
    public int RunningTasks { get; set; }

    /// <summary>
    /// Gets or sets the utilization percentage (0-100)
    /// </summary>
    public double UtilizationPercentage { get; set; }

    /// <summary>
    /// Gets or sets the estimated completion time for current tasks
    /// </summary>
    public TimeSpan EstimatedCompletionTime { get; set; }
}