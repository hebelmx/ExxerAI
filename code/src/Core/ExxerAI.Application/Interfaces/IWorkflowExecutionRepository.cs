using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for managing workflow execution persistence and retrieval
/// </summary>
public interface IWorkflowExecutionRepository
{
    /// <summary>
    /// Retrieves a workflow execution by its unique identifier
    /// </summary>
    /// <param name="executionId">The unique identifier of the workflow execution</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result containing the workflow execution if found, or failure if not found</returns>
    Task<Result<WorkflowExecution>> GetExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all executions for a specific workflow with optional status filtering
    /// </summary>
    /// <param name="workflowId">The workflow identifier to filter by</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result containing collection of workflow executions</returns>
    Task<Result<IEnumerable<WorkflowExecution>>> GetExecutionsForWorkflowAsync(
        Guid workflowId, 
        WorkflowExecutionStatus? status = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new workflow execution
    /// </summary>
    /// <param name="execution">The workflow execution to create</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result containing the created workflow execution</returns>
    Task<Result<WorkflowExecution>> CreateExecutionAsync(WorkflowExecution execution, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing workflow execution
    /// </summary>
    /// <param name="execution">The workflow execution to update</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result containing the updated workflow execution</returns>
    Task<Result<WorkflowExecution>> UpdateExecutionAsync(WorkflowExecution execution, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a workflow execution by its identifier
    /// </summary>
    /// <param name="executionId">The unique identifier of the workflow execution to delete</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result indicating success or failure of the deletion</returns>
    Task<Result<bool>> DeleteExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all currently active (running or paused) workflow executions
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result containing collection of active workflow executions</returns>
    Task<Result<IEnumerable<WorkflowExecution>>> GetActiveExecutionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves executions that need to be resumed (scheduled or paused)
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result containing collection of workflow executions that need resumption</returns>
    Task<Result<IEnumerable<WorkflowExecution>>> GetExecutionsNeedingResumptionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets execution statistics for monitoring and reporting
    /// </summary>
    /// <param name="workflowId">Optional workflow identifier to filter statistics</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Result containing execution statistics</returns>
    Task<Result<WorkflowExecutionStatistics>> GetExecutionStatisticsAsync(Guid? workflowId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics about workflow executions for monitoring and reporting
/// </summary>
public record WorkflowExecutionStatistics
{
    /// <summary>
    /// Total number of executions
    /// </summary>
    public int TotalExecutions { get; init; }

    /// <summary>
    /// Number of currently running executions
    /// </summary>
    public int RunningExecutions { get; init; }

    /// <summary>
    /// Number of paused executions
    /// </summary>
    public int PausedExecutions { get; init; }

    /// <summary>
    /// Number of completed executions
    /// </summary>
    public int CompletedExecutions { get; init; }

    /// <summary>
    /// Number of failed executions
    /// </summary>
    public int FailedExecutions { get; init; }

    /// <summary>
    /// Number of cancelled executions
    /// </summary>
    public int CancelledExecutions { get; init; }

    /// <summary>
    /// Average execution duration for completed executions
    /// </summary>
    public TimeSpan? AverageExecutionDuration { get; init; }

    /// <summary>
    /// Success rate (completed / total completed or failed)
    /// </summary>
    public double SuccessRate => (CompletedExecutions + FailedExecutions + CancelledExecutions) > 0 
        ? (double)CompletedExecutions / (CompletedExecutions + FailedExecutions + CancelledExecutions) 
        : 0.0;
}