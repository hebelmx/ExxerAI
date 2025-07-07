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
        try
        {
            if (task is null)
            {
                _logger.LogWarning("Attempted to find agent for null task");
                return Result<Agent>.WithFailure("Task cannot be null");
            }

            cancellationToken.ThrowIfCancellationRequested();

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
            return Result<Agent>.WithFailure("Operation was cancelled");
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
        try
        {
            if (agent is null)
            {
                _logger.LogWarning("Attempted to register null agent");
                return Result<bool>.WithFailure("Agent cannot be null");
            }

            cancellationToken.ThrowIfCancellationRequested();

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
                    ActiveTasks = 0,
                    MaxConcurrentTasks = GetMaxConcurrentTasks(agent),
                    CurrentCpuUsage = 0.0f,
                    CurrentMemoryUsage = 0.0f,
                    LastUpdated = DateTime.UtcNow
                };

                _agentWorkloads.TryAdd(agent.Id, workload);

                _logger.LogInformation("Registered agent {AgentId}: {AgentName} with max concurrent tasks: {MaxTasks}",
                    agent.Id, agent.Name, workload.MaxConcurrentTasks);

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
            return Result<bool>.WithFailure("Operation was cancelled");
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
        try
        {
            if (agentId == Guid.Empty)
            {
                _logger.LogWarning("Attempted to unregister agent with empty ID");
                return Result<bool>.WithFailure("Agent ID cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

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
            return Result<bool>.WithFailure("Operation was cancelled");
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
        try
        {
            if (agentId == Guid.Empty)
            {
                _logger.LogWarning("Attempted to get workload for agent with empty ID");
                return Result<AgentWorkload>.WithFailure("Agent ID cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            if (_agentWorkloads.TryGetValue(agentId, out var workload))
            {
                // Update workload with current metrics
                workload.LastUpdated = DateTime.UtcNow;
                workload.CurrentCpuUsage = SimulateCurrentCpuUsage();
                workload.CurrentMemoryUsage = SimulateCurrentMemoryUsage();

                _logger.LogDebug("Retrieved workload for agent {AgentId}: {ActiveTasks}/{MaxTasks} tasks, CPU: {CpuUsage:P0}, Memory: {MemoryUsage:P0}",
                    agentId, workload.ActiveTasks, workload.MaxConcurrentTasks, workload.CurrentCpuUsage, workload.CurrentMemoryUsage);

                return Result<AgentWorkload>.Success(workload);
            }

            _logger.LogWarning("Workload not found for agent {AgentId}", agentId);
            return Result<AgentWorkload>.WithFailure("Agent workload not found");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get agent workload operation was cancelled");
            return Result<AgentWorkload>.WithFailure("Operation was cancelled");
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

        // Agent is available if it's active and has capacity
        return agent.Status == AgentStatus.Active &&
               workload.ActiveTasks < workload.MaxConcurrentTasks &&
               workload.CurrentCpuUsage < 0.9f && // Less than 90% CPU usage
               workload.CurrentMemoryUsage < 0.8f; // Less than 80% memory usage
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
        var agentCapabilities = agent.Capabilities?.SupportedOperations ?? new List<string>();

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
        var utilizationRatio = (float)workload.ActiveTasks / workload.MaxConcurrentTasks;
        return 1.0f - utilizationRatio;
    }

    private static float CalculatePerformanceScore(AgentWorkload workload)
    {
        // Higher score for agents with better performance metrics
        var cpuScore = 1.0f - workload.CurrentCpuUsage;
        var memoryScore = 1.0f - workload.CurrentMemoryUsage;
        return (cpuScore + memoryScore) / 2.0f;
    }

    private static float CalculatePriorityScore(AgentTask task)
    {
        // Higher score for higher priority tasks
        return task.Priority switch
        {
            TaskPriority.Critical => 1.0f,
            TaskPriority.High => 0.8f,
            TaskPriority.Medium => 0.6f,
            TaskPriority.Low => 0.4f,
            _ => 0.5f
        };
    }

    private static List<string> GetRequiredCapabilities(AgentTask task)
    {
        // Extract required capabilities from task metadata or type
        var capabilities = new List<string>();

        if (task.Metadata.TryGetValue("RequiredCapabilities", out var capabilitiesObj) &&
            capabilitiesObj is IEnumerable<string> capabilityList)
        {
            capabilities.AddRange(capabilityList);
        }

        // Add default capabilities based on task name or description
        if (task.Name.Contains("document", StringComparison.OrdinalIgnoreCase))
        {
            capabilities.Add("DocumentProcessing");
        }

        if (task.Name.Contains("analysis", StringComparison.OrdinalIgnoreCase))
        {
            capabilities.Add("DataAnalysis");
        }

        return capabilities;
    }

    private static int GetMaxConcurrentTasks(Agent agent)
    {
        // Determine max concurrent tasks based on agent capabilities
        var baseCapacity = 5; // Default capacity

        if (agent.Capabilities?.MaxConcurrentTasks > 0)
        {
            return agent.Capabilities.MaxConcurrentTasks;
        }

        // Adjust based on agent type or other factors
        return baseCapacity;
    }

    private static float SimulateCurrentCpuUsage()
    {
        // Simulate CPU usage - in real implementation, this would come from system metrics
        var random = new Random();
        return (float)(random.NextDouble() * 0.7); // 0-70% usage
    }

    private static float SimulateCurrentMemoryUsage()
    {
        // Simulate memory usage - in real implementation, this would come from system metrics
        var random = new Random();
        return (float)(random.NextDouble() * 0.6); // 0-60% usage
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
                Interlocked.Increment(ref workload.ActiveTasks);
            }
            else
            {
                Interlocked.Decrement(ref workload.ActiveTasks);
            }

            workload.LastUpdated = DateTime.UtcNow;

            _logger.LogDebug("Updated workload for agent {AgentId}: {ActiveTasks} active tasks",
                agentId, workload.ActiveTasks);
        }
    }

    /// <summary>
    /// Gets all registered agents and their current workloads
    /// </summary>
    /// <returns>Dictionary of agent IDs to their workloads</returns>
    public async Task<Result<Dictionary<Guid, AgentWorkload>>> GetAllAgentWorkloadsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            var workloads = new Dictionary<Guid, AgentWorkload>(_agentWorkloads);

            _logger.LogDebug("Retrieved workloads for {AgentCount} agents", workloads.Count);
            return Result<Dictionary<Guid, AgentWorkload>>.Success(workloads);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get all agent workloads operation was cancelled");
            return Result<Dictionary<Guid, AgentWorkload>>.WithFailure("Operation was cancelled");
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