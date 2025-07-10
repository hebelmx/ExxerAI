using ExxerAI.Infrastructure.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ExxerAI.Infrastructure.Services;

/// <summary>
/// Implementation of agent scheduler for managing agent workloads and task assignment optimization
/// Supports workload balancing, capability matching, and performance optimization
/// </summary>
public class AgentScheduler : IAgentScheduler
{
    private readonly ILogger<AgentScheduler> _logger;
    private readonly ConcurrentDictionary<Guid, Agent> _registeredAgents;
    private readonly ConcurrentDictionary<Guid, AgentWorkload> _agentWorkloads;
    private readonly SemaphoreSlim _schedulingLock;

    /// <summary>
    /// Initializes a new instance of the AgentScheduler
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public AgentScheduler(ILogger<AgentScheduler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _registeredAgents = new ConcurrentDictionary<Guid, Agent>();
        _agentWorkloads = new ConcurrentDictionary<Guid, AgentWorkload>();
        _schedulingLock = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Finds the best available agent for a specific task
    /// </summary>
    /// <param name="task">The task to assign</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The best agent for the task if found</returns>
    public async Task<Result<Agent>> FindBestAgentAsync(AgentTask task, CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<Agent>();

        try
        {
            if (task is null)
            {
                _logger.LogWarning("Attempted to find agent for null task");
                return Result<Agent>.WithFailure("Task cannot be null");
            }

            await _schedulingLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var availableAgents = _registeredAgents.Values
                    .Where(agent => IsAgentAvailable(agent))
                    .ToList();

                if (!availableAgents.Any())
                {
                    _logger.LogWarning("No available agents found for task {TaskId}", task.Id);
                    return Result<Agent>.WithFailure("No available agents found");
                }

                // Score agents based on capability match and workload
                var scoredAgents = availableAgents
                    .Select(agent => new
                    {
                        Agent = agent,
                        Score = CalculateAgentScore(agent, task)
                    })
                    .OrderByDescending(x => x.Score)
                    .ToList();

                var bestAgent = scoredAgents.First().Agent;

                _logger.LogInformation("Selected agent {AgentId} for task {TaskId} with score {Score}",
                    bestAgent.Id, task.Id, scoredAgents.First().Score);

                return Result<Agent>.Success(bestAgent);
            }
            finally
            {
                _schedulingLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Find best agent operation was cancelled");
            return ResultExtensions.Cancelled<Agent>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding best agent for task {TaskId}", task?.Id);
            return Result<Agent>.WithFailure($"Failed to find best agent: {ex.Message}");
        }
    }

    /// <summary>
    /// Registers an agent with the scheduler
    /// </summary>
    /// <param name="agent">The agent to register</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> RegisterAgentAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<bool>();

        try
        {
            if (agent is null)
            {
                _logger.LogWarning("Attempted to register null agent");
                return Result<bool>.WithFailure("Agent cannot be null");
            }

            await _schedulingLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_registeredAgents.ContainsKey(agent.Id))
                {
                    _logger.LogWarning("Agent {AgentId} is already registered", agent.Id);
                    return Result<bool>.WithFailure("Agent is already registered");
                }

                _registeredAgents.TryAdd(agent.Id, agent);

                // Initialize workload tracking
                var workload = new AgentWorkload
                {
                    AgentId = agent.Id,
                    AssignedTasks = 0,
                    RunningTasks = 0,
                    UtilizationPercentage = 0.0,
                    EstimatedCompletionTime = TimeSpan.Zero
                };

                _agentWorkloads.TryAdd(agent.Id, workload);

                _logger.LogInformation("Registered agent {AgentId}: {AgentName} with utilization: {Utilization:P0}",
                    agent.Id, agent.Name, workload.UtilizationPercentage);

                return Result<bool>.Success(true);
            }
            finally
            {
                _schedulingLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Register agent operation was cancelled");
            return ResultExtensions.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering agent {AgentId}", agent?.Id);
            return Result<bool>.WithFailure($"Failed to register agent: {ex.Message}");
        }
    }

    /// <summary>
    /// Unregisters an agent from the scheduler
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> UnregisterAgentAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<bool>();

        try
        {
            if (agentId == Guid.Empty)
            {
                _logger.LogWarning("Attempted to unregister agent with empty ID");
                return Result<bool>.WithFailure("Agent ID cannot be empty");
            }

            await _schedulingLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var agentRemoved = _registeredAgents.TryRemove(agentId, out var removedAgent);
                var workloadRemoved = _agentWorkloads.TryRemove(agentId, out _);

                if (!agentRemoved)
                {
                    _logger.LogWarning("Agent {AgentId} not found for unregistration", agentId);
                    return Result<bool>.WithFailure("Agent not found");
                }

                _logger.LogInformation("Unregistered agent {AgentId}: {AgentName}",
                    agentId, removedAgent?.Name ?? "Unknown");

                return Result<bool>.Success(true);
            }
            finally
            {
                _schedulingLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Unregister agent operation was cancelled");
            return ResultExtensions.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering agent {AgentId}", agentId);
            return Result<bool>.WithFailure($"Failed to unregister agent: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the current workload for an agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The agent's current workload</returns>
    public async Task<Result<AgentWorkload>> GetAgentWorkloadAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<AgentWorkload>();

        try
        {
            if (agentId == Guid.Empty)
            {
                _logger.LogWarning("Attempted to get workload for agent with empty ID");
                return Result<AgentWorkload>.WithFailure("Agent ID cannot be empty");
            }

            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            if (_agentWorkloads.TryGetValue(agentId, out var workload))
            {
                // Update workload with current metrics
                workload.UtilizationPercentage = CalculateUtilization(workload);
                workload.EstimatedCompletionTime = CalculateEstimatedCompletion(workload);

                _logger.LogDebug("Retrieved workload for agent {AgentId}: {AssignedTasks} assigned, {RunningTasks} running, Utilization: {Utilization:P0}",
                    agentId, workload.AssignedTasks, workload.RunningTasks, workload.UtilizationPercentage);

                return Result<AgentWorkload>.Success(workload);
            }

            _logger.LogWarning("Workload not found for agent {AgentId}", agentId);
            return Result<AgentWorkload>.WithFailure("Agent workload not found");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get agent workload operation was cancelled");
            return ResultExtensions.Cancelled<AgentWorkload>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workload for agent {AgentId}", agentId);
            return Result<AgentWorkload>.WithFailure($"Failed to get agent workload: {ex.Message}");
        }
    }

    // Private helper methods

    private bool IsAgentAvailable(Agent agent)
    {
        if (!_agentWorkloads.TryGetValue(agent.Id, out var workload))
        {
            return false;
        }

        // Agent is available if it's active and has low utilization
        return agent.Status == AgentStatus.Active &&
               workload.UtilizationPercentage < 0.8; // Less than 80% utilization
    }

    private float CalculateAgentScore(Agent agent, AgentTask task)
    {
        float score = 0.0f;

        if (!_agentWorkloads.TryGetValue(agent.Id, out var workload))
        {
            return 0.0f;
        }

        // Capability matching (40% of score)
        var capabilityScore = CalculateCapabilityMatch(agent, task);
        score += capabilityScore * 0.4f;

        // Workload balancing (30% of score)
        var workloadScore = CalculateWorkloadScore(workload);
        score += workloadScore * 0.3f;

        // Performance metrics (20% of score)
        var performanceScore = CalculatePerformanceScore(workload);
        score += performanceScore * 0.2f;

        // Priority bonus (10% of score)
        var priorityScore = CalculatePriorityScore(task);
        score += priorityScore * 0.1f;

        return score;
    }

    private static float CalculateCapabilityMatch(Agent agent, AgentTask task)
    {
        // Simple capability matching based on agent capabilities
        // In a real implementation, this would be more sophisticated
        var requiredCapabilities = GetRequiredCapabilities(task);
        var agentCapabilities = agent.Capabilities?.SupportedTaskTypes ?? [];

        if (!requiredCapabilities.Any())
        {
            return 1.0f; // No specific requirements
        }

        var matchCount = requiredCapabilities.Count(cap => 
            agentCapabilities.Any(agentCap => 
                agentCap.Contains(cap, StringComparison.OrdinalIgnoreCase)));

        return (float)matchCount / requiredCapabilities.Count;
    }

    private static float CalculateWorkloadScore(AgentWorkload workload)
    {
        // Higher score for agents with lower workload
        return 1.0f - (float)workload.UtilizationPercentage;
    }

    private static float CalculatePerformanceScore(AgentWorkload workload)
    {
        // Higher score for agents with faster estimated completion
        var maxCompletionHours = 24.0; // Maximum expected completion time
        var completionHours = workload.EstimatedCompletionTime.TotalHours;
        return Math.Max(0.0f, 1.0f - (float)(completionHours / maxCompletionHours));
    }

    private static float CalculatePriorityScore(AgentTask task)
    {
        // Higher score for higher priority tasks
        return task.Priority switch
        {
            TaskPriority.Critical => 1.0f,
            TaskPriority.High => 0.8f,
                            TaskPriority.Normal => 0.6f,
            TaskPriority.Low => 0.4f,
            _ => 0.5f
        };
    }

    private static List<string> GetRequiredCapabilities(AgentTask task)
    {
        // Extract required capabilities from task metadata or type
        var capabilities = new List<string>();

        // Add default capabilities based on task type or description
        if (task.TaskType.Contains("document", StringComparison.OrdinalIgnoreCase))
        {
            capabilities.Add("DocumentProcessing");
        }

        if (task.TaskType.Contains("analysis", StringComparison.OrdinalIgnoreCase))
        {
            capabilities.Add("DataAnalysis");
        }

        if (task.Description.Contains("document", StringComparison.OrdinalIgnoreCase))
        {
            capabilities.Add("DocumentProcessing");
        }

        if (task.Description.Contains("analysis", StringComparison.OrdinalIgnoreCase))
        {
            capabilities.Add("DataAnalysis");
        }

        return capabilities;
    }

    private static double CalculateUtilization(AgentWorkload workload)
    {
        // Calculate utilization based on running vs assigned tasks
        if (workload.AssignedTasks == 0) return 0.0;
        return (double)workload.RunningTasks / workload.AssignedTasks;
    }

    private static TimeSpan CalculateEstimatedCompletion(AgentWorkload workload)
    {
        // Estimate completion time based on running tasks
        // Simple estimation: 1 hour per running task
        return TimeSpan.FromHours(workload.RunningTasks);
    }

    /// <summary>
    /// Updates the workload for an agent when a task is assigned
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="increment">Whether to increment (true) or decrement (false) the task count</param>
    public void UpdateAgentWorkload(Guid agentId, bool increment = true)
    {
        if (_agentWorkloads.TryGetValue(agentId, out var workload))
        {
            if (increment)
            {
                workload.AssignedTasks++;
                workload.RunningTasks++;
            }
            else
            {
                workload.AssignedTasks--;
                workload.RunningTasks--;
            }

            workload.UtilizationPercentage = CalculateUtilization(workload);
            workload.EstimatedCompletionTime = CalculateEstimatedCompletion(workload);

            _logger.LogDebug("Updated workload for agent {AgentId}: {AssignedTasks} assigned, {RunningTasks} running",
                agentId, workload.AssignedTasks, workload.RunningTasks);
        }
    }

    /// <summary>
    /// Gets all registered agents and their current workloads
    /// </summary>
    /// <returns>Dictionary of agent IDs to their workloads</returns>
    public async Task<Result<Dictionary<Guid, AgentWorkload>>> GetAllAgentWorkloadsAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<Dictionary<Guid, AgentWorkload>>();

        try
        {

            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            var workloads = new Dictionary<Guid, AgentWorkload>(_agentWorkloads);

            _logger.LogDebug("Retrieved workloads for {AgentCount} agents", workloads.Count);
            return Result<Dictionary<Guid, AgentWorkload>>.Success(workloads);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get all agent workloads operation was cancelled");
            return ResultExtensions.Cancelled<Dictionary<Guid, AgentWorkload>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all agent workloads");
            return Result<Dictionary<Guid, AgentWorkload>>.WithFailure($"Failed to get agent workloads: {ex.Message}");
        }
    }

    /// <summary>
    /// Disposes the resources used by the AgentScheduler
    /// </summary>
    public void Dispose()
    {
        _schedulingLock?.Dispose();
        _registeredAgents.Clear();
        _agentWorkloads.Clear();
        GC.SuppressFinalize(this);
    }
} 