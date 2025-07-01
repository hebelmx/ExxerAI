using System.Collections.Concurrent;
using ExxerAI.Application;
using ExxerAI.Domain;
using ExxerAI.Application.Interfaces;

namespace ExxerAI.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of agent repository for development and testing
/// </summary>
public class InMemoryAgentRepository : IAgentRepository
{
    private readonly ConcurrentDictionary<Guid, Agent> _agents = new();

    /// <summary>
    /// Gets an agent by its identifier
    /// </summary>
    /// <param name="id">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The agent if found</returns>
    public Task<ExxerAI.Domain.Result<Agent>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(ExxerAI.Domain.Result<Agent>.WithFailure("Operation was cancelled"));

        if (id == Guid.Empty)
            return Task.FromResult(ExxerAI.Domain.Result<Agent>.WithFailure("Agent ID cannot be empty"));

        var success = _agents.TryGetValue(id, out var agent);
        return success && agent != null
            ? Task.FromResult(ExxerAI.Domain.Result<Agent>.WithSuccess(agent))
            : Task.FromResult(ExxerAI.Domain.Result<Agent>.WithFailure($"Agent with ID '{id}' not found"));
    }

    /// <summary>
    /// Gets all agents
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all agents</returns>
    public Task<ExxerAI.Domain.Result<IEnumerable<Agent>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<Agent>>.WithFailure("Operation was cancelled"));

        var agents = _agents.Values.ToList().AsEnumerable();
        return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<Agent>>.WithSuccess(agents));
    }

    /// <summary>
    /// Adds a new agent
    /// </summary>
    /// <param name="entity">The agent to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added agent</returns>
    public Task<ExxerAI.Domain.Result<Agent>> AddAsync(Agent entity, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(ExxerAI.Domain.Result<Agent>.WithFailure("Operation was cancelled"));

        if (entity == null)
            return Task.FromResult(ExxerAI.Domain.Result<Agent>.WithFailure("Agent cannot be null"));

        if (string.IsNullOrWhiteSpace(entity.Name))
            return Task.FromResult(ExxerAI.Domain.Result<Agent>.WithFailure("Agent name is required"));

        if (_agents.ContainsKey(entity.Id))
            return Task.FromResult(ExxerAI.Domain.Result<Agent>.WithFailure($"Agent with ID '{entity.Id}' already exists"));

        // Ensure timestamps are set
        if (entity.CreatedAt == default)
            entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        var success = _agents.TryAdd(entity.Id, entity);
        return success
            ? Task.FromResult(ExxerAI.Domain.Result<Agent>.WithSuccess(entity))
            : Task.FromResult(ExxerAI.Domain.Result<Agent>.WithFailure("Failed to add agent"));
    }

    /// <summary>
    /// Updates an existing agent
    /// </summary>
    /// <param name="entity">The agent to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated agent</returns>
    public Task<ExxerAI.Domain.Result<Agent>> UpdateAsync(Agent entity, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(ExxerAI.Domain.Result<Agent>.WithFailure("Operation was cancelled"));

        if (entity == null)
            return Task.FromResult(ExxerAI.Domain.Result<Agent>.WithFailure("Agent cannot be null"));

        if (string.IsNullOrWhiteSpace(entity.Name))
            return Task.FromResult(ExxerAI.Domain.Result<Agent>.WithFailure("Agent name is required"));

        if (!_agents.ContainsKey(entity.Id))
            return Task.FromResult(ExxerAI.Domain.Result<Agent>.WithFailure($"Agent with ID '{entity.Id}' not found"));

        // Update timestamp
        entity.UpdatedAt = DateTime.UtcNow;

        _agents[entity.Id] = entity;
        return Task.FromResult(ExxerAI.Domain.Result<Agent>.WithSuccess(entity));
    }

    /// <summary>
    /// Deletes an agent by its identifier
    /// </summary>
    /// <param name="id">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public Task<ExxerAI.Domain.Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(ExxerAI.Domain.Result<bool>.WithFailure("Operation was cancelled"));

        if (id == Guid.Empty)
            return Task.FromResult(ExxerAI.Domain.Result<bool>.WithFailure("Agent ID cannot be empty"));

        var success = _agents.TryRemove(id, out _);
        return success
            ? Task.FromResult(ExxerAI.Domain.Result<bool>.WithSuccess(true))
            : Task.FromResult(ExxerAI.Domain.Result<bool>.WithFailure($"Agent with ID '{id}' not found"));
    }

    /// <summary>
    /// Checks if an agent exists by its identifier
    /// </summary>
    /// <param name="id">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the agent exists, false otherwise</returns>
    public Task<ExxerAI.Domain.Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(ExxerAI.Domain.Result<bool>.WithFailure("Operation was cancelled"));

        if (id == Guid.Empty)
            return Task.FromResult(ExxerAI.Domain.Result<bool>.WithFailure("Agent ID cannot be empty"));

        var exists = _agents.ContainsKey(id);
        return Task.FromResult(ExxerAI.Domain.Result<bool>.WithSuccess(exists));
    }

    /// <summary>
    /// Gets agents by status
    /// </summary>
    /// <param name="status">The agent status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of agents with the specified status</returns>
    public Task<ExxerAI.Domain.Result<IEnumerable<Agent>>> GetByStatusAsync(AgentStatus status, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<Agent>>.WithFailure("Operation was cancelled"));

        var agents = _agents.Values
            .Where(a => a.Status == status)
            .ToList()
            .AsEnumerable();

        return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<Agent>>.WithSuccess(agents));
    }

    /// <summary>
    /// Finds agents that support a specific task type
    /// </summary>
    /// <param name="taskType">The task type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of agents that support the task type</returns>
    public Task<ExxerAI.Domain.Result<IEnumerable<Agent>>> FindByTaskTypeAsync(string taskType, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<Agent>>.WithFailure("Operation was cancelled"));

        if (string.IsNullOrWhiteSpace(taskType))
            return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<Agent>>.WithFailure("Task type cannot be empty"));

        var agents = _agents.Values
            .Where(a => a.Capabilities?.SupportedTaskTypes?.Contains(taskType) == true)
            .ToList()
            .AsEnumerable();

        return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<Agent>>.WithSuccess(agents));
    }

    /// <summary>
    /// Gets agents with their current task count
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of agents with task count information</returns>
    public Task<ExxerAI.Domain.Result<IEnumerable<(Agent Agent, int TaskCount)>>> GetAgentsWithTaskCountAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<(Agent Agent, int TaskCount)>>.WithFailure("Operation was cancelled"));

        // For in-memory implementation, we'll simulate task count as 0 for now
        // In a real implementation, this would query the task repository
        var agentsWithTaskCount = _agents.Values
            .Select(agent => (Agent: agent, TaskCount: 0))
            .ToList()
            .AsEnumerable();

        return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<(Agent Agent, int TaskCount)>>.WithSuccess(agentsWithTaskCount));
    }

    /// <summary>
    /// Gets the current number of agents in the repository
    /// </summary>
    public int Count => _agents.Count;

    /// <summary>
    /// Clears all agents from the repository (useful for testing)
    /// </summary>
    public void Clear()
    {
        _agents.Clear();
    }
} 