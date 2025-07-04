using ExxerAI.Domain.DomainEntities;
using ExxerAI.Domain.Helpers;
using ExxerAI.Domain.Entities; // For Agent, AgentStatus

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for agent entities
/// </summary>
public interface IAgentRepository : IRepository<Domain.Agent>
{
    /// <summary>
    /// Gets agents by agentStatus
    /// </summary>
    /// <param name="status">The agent agentStatus</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of agents with the specified agentStatus</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<Domain.Agent>>> GetByStatusAsync(
        Domain.AgentStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds agents that support a specific task type
    /// </summary>
    /// <param name="taskType">The task type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of agents that support the task type</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<Domain.Agent>>> FindByTaskTypeAsync(
        string taskType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets agents with their current task count
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of agents with task count information</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<(Domain.Agent Agent, int TaskCount)>>> GetAgentsWithTaskCountAsync(
        CancellationToken cancellationToken = default);
}