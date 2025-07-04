using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.Helpers;

namespace ExxerAI.Orchestration.Interfaces;

/// <summary>
/// Interface for scheduling and managing agents
/// </summary>
public interface IAgentScheduler
{
    /// <summary>
    /// Finds the best available agent for a specific task
    /// </summary>
    /// <param name="task">The task to assign</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The best agent for the task if found</returns>
    Task<ExxerAI.Domain.Result<Agent>> FindBestAgentAsync(
        AgentTask task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers an agent with the scheduler
    /// </summary>
    /// <param name="agent">The agent to register</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> RegisterAgentAsync(Agent agent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters an agent from the scheduler
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> UnregisterAgentAsync(Guid agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current workload for an agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The agent's current workload</returns>
    Task<ExxerAI.Domain.Result<AgentWorkload>> GetAgentWorkloadAsync(
        Guid agentId,
        CancellationToken cancellationToken = default);
}