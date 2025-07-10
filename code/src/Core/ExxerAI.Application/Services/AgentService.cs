using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Services;

/// <summary>
/// Service implementation for managing agents in the ExxerAI system
/// </summary>
public class AgentService : IAgentService
{
    private readonly IAgentRepository _agentRepository;
    private readonly ITaskRepository _taskRepository;

    /// <summary>
    /// Initializes a new instance of the AgentService class
    /// </summary>
    /// <param name="agentRepository">The agent repository</param>
    /// <param name="taskRepository">The task repository</param>
    public AgentService(
        IAgentRepository agentRepository,
        ITaskRepository taskRepository)
    {
        _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }

    /// <summary>
    /// Creates a new agent
    /// </summary>
    /// <param name="name">The agent name</param>
    /// <param name="description">The agent description</param>
    /// <param name="capabilities">The agent capabilities</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the created agent</returns>
    public async Task<Result<Agent>> CreateAgentAsync(
        string name,
        string description,
        AgentCapabilities capabilities,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<Agent>();

        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result<Agent>.WithFailure("Agent name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                return Result<Agent>.WithFailure("Agent description cannot be empty");
            }

            if (capabilities == null)
            {
                return Result<Agent>.WithFailure("Agent capabilities cannot be null");
            }

            var agent = new Agent
            {
                Name = name,
                Description = description,
                Capabilities = capabilities,
                Configuration = new AgentConfiguration(),
                Status = AgentStatus.Active
            };

            var addResult = await _agentRepository.AddAsync(agent, cancellationToken).ConfigureAwait(false);

            if (addResult.IsFailure)
            {
                return Result<Agent>.WithFailure(addResult.Error ?? "Failed to add agent");
            }

            return addResult;
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<Agent>();
        }
        catch (Exception ex)
        {
            return Result<Agent>.WithFailure($"An error occurred while creating the agent: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets an agent by its identifier
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the agent if found</returns>
    public async Task<Result<Agent>> GetAgentAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<Agent>();

        try
        {
            if (agentId == Guid.Empty)
            {
                return Result<Agent>.WithFailure("Agent ID cannot be empty");
            }

            var result = await _agentRepository.GetByIdAsync(agentId, cancellationToken).ConfigureAwait(false);

            if (result.IsFailure)
            {
                return Result<Agent>.WithFailure($"Agent not found with ID: {agentId}");
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<Agent>();
        }
        catch (Exception ex)
        {
            return Result<Agent>.WithFailure($"An error occurred while retrieving the agent: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all agents
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing all agents</returns>
    public async Task<Result<IEnumerable<Agent>>> GetAllAgentsAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<IEnumerable<Agent>>();

        try
        {
            var result = await _agentRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

            if (result.IsFailure)
            {
                return Result<IEnumerable<Agent>>.WithFailure(result.Error ?? "Failed to retrieve agents");
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<IEnumerable<Agent>>();
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Agent>>.WithFailure($"An error occurred while retrieving all agents: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all active agents
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of active agents</returns>
    public async Task<Result<IEnumerable<Agent>>> GetActiveAgentsAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<IEnumerable<Agent>>();

        try
        {
            var result = await _agentRepository.GetByStatusAsync(AgentStatus.Active, cancellationToken).ConfigureAwait(false);

            if (result.IsFailure)
            {
                return Result<IEnumerable<Agent>>.WithFailure(result.Error ?? "Failed to retrieve active agents");
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<IEnumerable<Agent>>();
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Agent>>.WithFailure($"An error occurred while retrieving active agents: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an agent's configuration
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="configuration">The new configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> UpdateAgentConfigurationAsync(
        Guid agentId,
        AgentConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<bool>();

        try
        {
            if (agentId == Guid.Empty)
            {
                return Result<bool>.WithFailure("Agent ID cannot be empty");
            }

            if (configuration == null)
            {
                return Result<bool>.WithFailure("Configuration cannot be null");
            }

            var agentResult = await _agentRepository.GetByIdAsync(agentId, cancellationToken).ConfigureAwait(false);
            if (agentResult.IsFailure)
            {
                return Result<bool>.WithFailure($"Agent not found with ID: {agentId}");
            }

            agentResult.Value!.Configuration = configuration;
            agentResult.Value.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _agentRepository.UpdateAsync(agentResult.Value, cancellationToken).ConfigureAwait(false);
            if (updateResult.IsFailure)
            {
                return Result<bool>.WithFailure(updateResult.Error ?? "Failed to update agent");
            }

            return Result<bool>.WithSuccess(true);
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"An error occurred while updating agent configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an agent's agentStatus
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="status">The new agentStatus</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> UpdateAgentStatusAsync(
        Guid agentId,
        AgentStatus status,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<bool>();

        try
        {
            if (agentId == Guid.Empty)
            {
                return Result<bool>.WithFailure("Agent ID cannot be empty");
            }

            var agentResult = await _agentRepository.GetByIdAsync(agentId, cancellationToken).ConfigureAwait(false);
            if (agentResult.IsFailure)
            {
                return Result<bool>.WithFailure($"Agent not found with ID: {agentId}");
            }

            agentResult.Value!.Status = status;
            agentResult.Value.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _agentRepository.UpdateAsync(agentResult.Value, cancellationToken).ConfigureAwait(false);
            if (updateResult.IsFailure)
            {
                return Result<bool>.WithFailure(updateResult.Error ?? "Failed to update agent");
            }

            return Result<bool>.WithSuccess(true);
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"An error occurred while updating agent agentStatus: {ex.Message}");
        }
    }

    /// <summary>
    /// Assigns a task to an agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> AssignTaskAsync(
        Guid agentId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (agentId == Guid.Empty)
            {
                return Result<bool>.WithFailure("Agent ID cannot be empty");
            }

            if (taskId == Guid.Empty)
            {
                return Result<bool>.WithFailure("Task ID cannot be empty");
            }

            // Verify agent exists and is active
            var agentResult = await _agentRepository.GetByIdAsync(agentId, cancellationToken).ConfigureAwait(false);
            if (agentResult.IsFailure)
            {
                return Result<bool>.WithFailure($"Agent not found with ID: {agentId}");
            }

            if (agentResult.Value!.Status != AgentStatus.Active)
            {
                return Result<bool>.WithFailure($"Agent {agentId} is not active and cannot be assigned tasks");
            }

            // Verify task exists and is available for assignment
            var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
            if (taskResult.IsFailure)
            {
                return Result<bool>.WithFailure($"Task not found with ID: {taskId}");
            }

            if (taskResult.Value!.AgentStatus != TaskAgentStatus.Pending)
            {
                return Result<bool>.WithFailure($"Task {taskId} is not available for assignment (AgentStatus: {taskResult.Value.AgentStatus})");
            }

            // Assign the task
            taskResult.Value.AssignedAgentId = agentId;

            var updateResult = await _taskRepository.UpdateAsync(taskResult.Value, cancellationToken).ConfigureAwait(false);
            if (updateResult.IsFailure)
            {
                return Result<bool>.WithFailure(updateResult.Error ?? "Failed to update task");
            }

            return Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"An error occurred while assigning the task: {ex.Message}");
        }
    }

    /// <summary>
    /// Assigns a task to an agent (alias for AssignTaskAsync)
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> AssignTaskToAgentAsync(
        Guid agentId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return await AssignTaskAsync(agentId, taskId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Finds the best available agent for a specific task type
    /// </summary>
    /// <param name="taskType">The task type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the best agent if found</returns>
    public async Task<Result<Agent>> FindBestAgentForTaskAsync(
        string taskType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskType))
            {
                return Result<Agent>.WithFailure("Task type cannot be empty");
            }

            // Find agents that support this task type
            var agentsResult = await _agentRepository.FindByTaskTypeAsync(taskType, cancellationToken).ConfigureAwait(false);
            if (agentsResult.IsFailure)
            {
                return Result<Agent>.WithFailure(agentsResult.Error ?? "Failed to find agents");
            }

            var supportingAgents = agentsResult.Value!.Where(a => a.Status == AgentStatus.Active).ToList();
            if (!supportingAgents.Any())
            {
                return Result<Agent>.WithFailure($"No active agents found that support task type: {taskType}");
            }

            // Get agents with their current task count
            var agentsWithTaskCountResult = await _agentRepository.GetAgentsWithTaskCountAsync(cancellationToken).ConfigureAwait(false);
            if (agentsWithTaskCountResult.IsFailure)
            {
                // Fallback to first supporting agent if we can't get task counts
                return Result<Agent>.WithSuccess(supportingAgents.First());
            }

            // Find the agent with the least tasks among those that support this task type
            var bestAgent = agentsWithTaskCountResult.Value!
                .Where(atc => supportingAgents.Contains(atc.Agent))
                .OrderBy(atc => atc.TaskCount)
                .ThenBy(atc => atc.Agent.CreatedAt) // Tie-breaker: oldest agent first
                .Select(atc => atc.Agent)
                .FirstOrDefault();

            if (bestAgent == null)
            {
                return Result<Agent>.WithFailure($"No suitable agent found for task type: {taskType}");
            }

            return Result<Agent>.WithSuccess(bestAgent);
        }
        catch (Exception ex)
        {
            return Result<Agent>.WithFailure($"An error occurred while finding the best agent: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes an agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> DeleteAgentAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (agentId == Guid.Empty)
            {
                return Result<bool>.WithFailure("Agent ID cannot be empty");
            }

            // Check if agent exists
            var agentResult = await _agentRepository.GetByIdAsync(agentId, cancellationToken).ConfigureAwait(false);
            if (agentResult.IsFailure)
            {
                return Result<bool>.WithFailure($"Agent not found with ID: {agentId}");
            }

            // Check for active tasks before deletion
            var activeTasksResult = await _taskRepository.GetByAgentAsync(agentId, TaskAgentStatus.InProgress, cancellationToken).ConfigureAwait(false);
            if (activeTasksResult.IsSuccess && activeTasksResult.Value!.Any())
            {
                return Result<bool>.WithFailure($"Cannot delete agent {agentId} as it has active tasks in progress");
            }

            var deleteResult = await _agentRepository.DeleteAsync(agentId, cancellationToken).ConfigureAwait(false);
            if (deleteResult.IsFailure)
            {
                return Result<bool>.WithFailure(deleteResult.Error ?? "Failed to delete agent");
            }

            return Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"An error occurred while deleting the agent: {ex.Message}");
        }
    }
}
