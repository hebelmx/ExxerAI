using ExxerAI.Application;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.Helpers;

namespace ExxerAI.Orchestration.Interfaces;

/// <summary>
/// Main orchestration engine for coordinating agents and workflows
/// </summary>
public interface IOrchestrationEngine
{
    /// <summary>
    /// Starts the orchestration engine
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the orchestration engine
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules a workflow for execution
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="input">The input data for the workflow</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the workflow execution</returns>
    Task<ExxerAI.Domain.Result<WorkflowExecution>> ScheduleWorkflowAsync(
        Guid workflowId,
        Dictionary<string, object> input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a task to the most suitable agent
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the assigned agent</returns>
    Task<ExxerAI.Domain.Result<Agent>> AssignTaskToAgentAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current agentStatus of the orchestration engine
    /// </summary>
    /// <returns>Result containing the orchestration agentStatus</returns>
    Result<OrchestrationStatus> GetStatus();

    /// <summary>
    /// Gets performance metrics for the orchestration engine
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The orchestration metrics</returns>
    Task<ExxerAI.Domain.Result<OrchestrationMetrics>> GetMetricsAsync(CancellationToken cancellationToken = default);
}