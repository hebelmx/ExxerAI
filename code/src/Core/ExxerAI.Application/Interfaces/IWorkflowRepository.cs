using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;

// For Workflow, WorkflowStatus, WorkflowExecution, WorkflowExecutionStatus

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for workflow entities
/// </summary>
public interface IWorkflowRepository : IRepository<Workflow>
{
    /// <summary>
    /// Gets workflows by agentStatus
    /// </summary>
    /// <param name="status">The workflow agentStatus</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of workflows with the specified agentStatus</returns>
    Task<Result<IEnumerable<Workflow>>> GetByStatusAsync(
        WorkflowStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workflow executions for a specific workflow
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="status">Optional execution agentStatus filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of workflow executions</returns>
    Task<Result<IEnumerable<WorkflowExecution>>> GetExecutionsAsync(
        Guid workflowId,
        WorkflowExecutionStatus? status = null,
        CancellationToken cancellationToken = default);
}
