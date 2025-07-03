using ExxerAI.Domain;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service interface for managing workflows in the ExxerAI system
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Creates a new workflow
    /// </summary>
    /// <param name="name">The workflow name</param>
    /// <param name="description">The workflow description</param>
    /// <param name="steps">The workflow steps</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the created workflow</returns>
    Task<ExxerAI.Domain.Result<Workflow>> CreateWorkflowAsync(
        string name,
        string description,
        IEnumerable<WorkflowStep> steps,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a workflow by its identifier
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the workflow if found</returns>
    Task<ExxerAI.Domain.Result<Workflow>> GetWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active workflows
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of active workflows</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<Workflow>>> GetActiveWorkflowsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a workflow's configuration
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="configuration">The new configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> UpdateWorkflowConfigurationAsync(
        Guid workflowId,
        WorkflowConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts execution of a workflow
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="input">The input data for the workflow</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the workflow execution</returns>
    Task<ExxerAI.Domain.Result<WorkflowExecution>> ExecuteWorkflowAsync(
        Guid workflowId,
        Dictionary<string, object> input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a workflow execution by its identifier
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the execution if found</returns>
    Task<ExxerAI.Domain.Result<WorkflowExecution>> GetWorkflowExecutionAsync(
        Guid executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all executions for a specific workflow
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="status">Optional agentStatus filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of executions</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<WorkflowExecution>>> GetWorkflowExecutionsAsync(
        Guid workflowId,
        WorkflowExecutionStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses a running workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> PauseWorkflowExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> ResumeWorkflowExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> CancelWorkflowExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a workflow
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> DeleteWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default);
} 