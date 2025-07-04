using ExxerAI.Domain.Helpers;
using ExxerAI.Domain.Entities; // For AgentTask, TaskAgentStatus

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for task entities
/// </summary>
public interface ITaskRepository : IRepository<AgentTask>
{
    /// <summary>
    /// Gets tasks by agentStatus
    /// </summary>
    /// <param name="agentStatus">The task agentStatus</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tasks with the specified agentStatus</returns>
    Task<Result<IEnumerable<AgentTask>>> GetByStatusAsync(
        TaskAgentStatus agentStatus,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks assigned to a specific agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="status">Optional agentStatus filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tasks assigned to the agent</returns>
    Task<Result<IEnumerable<AgentTask>>> GetByAgentAsync(
        Guid agentId,
        TaskAgentStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets overdue tasks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of overdue tasks</returns>
    Task<Result<IEnumerable<AgentTask>>> GetOverdueTasksAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks by type
    /// </summary>
    /// <param name="taskType">The task type</param>
    /// <param name="status">Optional agentStatus filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tasks of the specified type</returns>
    Task<Result<IEnumerable<AgentTask>>> GetByTypeAsync(
        string taskType,
        TaskAgentStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds the repository with sample tasks for development/testing
    /// </summary>
    /// <returns>The result of the seeding operation</returns>
    Task<Result<bool>> SeedAsync();
}
