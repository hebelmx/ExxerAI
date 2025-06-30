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
    /// <param name="definition">The workflow definition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the created workflow</returns>
    Task<Result<Workflow>> CreateWorkflowAsync(
        string name,
        string description,
        WorkflowDefinition definition,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a workflow by its identifier
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the workflow if found</returns>
    Task<Result<Workflow>> GetWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active workflows
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of active workflows</returns>
    Task<Result<IEnumerable<Workflow>>> GetActiveWorkflowsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a workflow definition
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="definition">The new workflow definition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> UpdateWorkflowDefinitionAsync(
        Guid workflowId,
        WorkflowDefinition definition,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a workflow for execution
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> ActivateWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a workflow with the specified input data
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="input">The input data for the workflow</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the workflow execution</returns>
    Task<Result<WorkflowExecution>> ExecuteWorkflowAsync(
        Guid workflowId,
        Dictionary<string, object> input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a workflow execution by its identifier
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the execution if found</returns>
    Task<Result<WorkflowExecution>> GetWorkflowExecutionAsync(
        Guid executionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all executions for a specific workflow
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of executions</returns>
    Task<Result<IEnumerable<WorkflowExecution>>> GetWorkflowExecutionsAsync(
        Guid workflowId,
        WorkflowExecutionStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses a running workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> PauseWorkflowExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> ResumeWorkflowExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> CancelWorkflowExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives a workflow
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> ArchiveWorkflowAsync(Guid workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a workflow definition
    /// </summary>
    /// <param name="definition">The workflow definition to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the validation operation</returns>
    Task<Result> ValidateWorkflowDefinitionAsync(
        WorkflowDefinition definition, 
        CancellationToken cancellationToken = default);
} 