using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Helpers.Operations;
using ExxerAI.Domain.ValueObjects;

// For Agent, AgentStatus

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for agent entities
/// </summary>
public interface IAgentRepository : IRepository<Agent>
{
    /// <summary>
    /// Gets agents by agentStatus
    /// </summary>
    /// <param name="status">The agent agentStatus</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of agents with the specified agentStatus</returns>
    Task<Result<IEnumerable<Agent>>> GetByStatusAsync(
        AgentStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds agents that support a specific task type
    /// </summary>
    /// <param name="taskType">The task type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of agents that support the task type</returns>
    Task<Result<IEnumerable<Agent>>> FindByTaskTypeAsync(
        string taskType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets agents with their current task count
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of agents with task count information</returns>
    Task<Result<IEnumerable<(Agent Agent, int TaskCount)>>> GetAgentsWithTaskCountAsync(
        CancellationToken cancellationToken = default);
}
