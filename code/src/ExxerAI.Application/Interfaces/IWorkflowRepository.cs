namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for workflow entities
/// </summary>
public interface IWorkflowRepository : IRepository<Domain.Workflow>
{
    /// <summary>
    /// Gets workflows by agentStatus
    /// </summary>
    /// <param name="status">The workflow agentStatus</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of workflows with the specified agentStatus</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<Domain.Workflow>>> GetByStatusAsync(
        Domain.WorkflowStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workflow executions for a specific workflow
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="status">Optional execution agentStatus filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of workflow executions</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<Domain.WorkflowExecution>>> GetExecutionsAsync(
        Guid workflowId,
        Domain.WorkflowExecutionStatus? status = null,
        CancellationToken cancellationToken = default);
}