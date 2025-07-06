using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service interface for managing agents in the ExxerAI system
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Creates a new agent
    /// </summary>
    /// <param name="name">The agent name</param>
    /// <param name="description">The agent description</param>
    /// <param name="capabilities">The agent capabilities</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the created agent</returns>
    Task<Result<Agent>> CreateAgentAsync(
        string name, 
        string description, 
        AgentCapabilities capabilities, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an agent by its identifier
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the agent if found</returns>
    Task<Result<Agent>> GetAgentAsync(Guid agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all agents
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing all agents</returns>
    Task<Result<IEnumerable<Agent>>> GetAllAgentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active agents
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of active agents</returns>
    Task<Result<IEnumerable<Agent>>> GetActiveAgentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an agent's configuration
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="configuration">The new configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> UpdateAgentConfigurationAsync(
        Guid agentId, 
        AgentConfiguration configuration, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an agent's agentStatus
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="status">The new agentStatus</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> UpdateAgentStatusAsync(
        Guid agentId, 
        AgentStatus status, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a task to an agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> AssignTaskAsync(
        Guid agentId, 
        Guid taskId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a task to an agent (alias for compatibility)
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> AssignTaskToAgentAsync(
        Guid agentId, 
        Guid taskId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the best available agent for a specific task type
    /// </summary>
    /// <param name="taskType">The task type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the best agent if found</returns>
    Task<Result<Agent>> FindBestAgentForTaskAsync(
        string taskType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> DeleteAgentAsync(Guid agentId, CancellationToken cancellationToken = default);
} 
