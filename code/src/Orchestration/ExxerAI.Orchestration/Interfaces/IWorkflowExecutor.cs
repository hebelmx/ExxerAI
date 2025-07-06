using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.Helpers.Operations;

namespace ExxerAI.Orchestration.Interfaces;

/// <summary>
/// Interface for executing workflow steps
/// </summary>
public interface IWorkflowExecutor
{
    /// <summary>
    /// Executes a workflow
    /// </summary>
    /// <param name="workflow">The workflow to execute</param>
    /// <param name="input">The input data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The workflow execution result</returns>
    Task<Result<WorkflowExecution>> ExecuteAsync(
        Workflow workflow,
        Dictionary<string, object> input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses a running workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> PauseExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> ResumeExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> CancelExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);
}