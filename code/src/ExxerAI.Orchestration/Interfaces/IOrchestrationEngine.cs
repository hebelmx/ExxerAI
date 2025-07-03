using ExxerAI.Application;
using ExxerAI.Domain;

namespace ExxerAI.Orchestration.Interfaces;

/// <summary>
/// Main orchestration engine for coordinating agents and workflows
/// </summary>
public interface IOrchestrationEngine
{
    /// <summary>
    /// Starts the orchestration engine
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the orchestration engine
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules a workflow for execution
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="input">The input data for the workflow</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the workflow execution</returns>
    Task<ExxerAI.Domain.Result<WorkflowExecution>> ScheduleWorkflowAsync(
        Guid workflowId,
        Dictionary<string, object> input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a task to the most suitable agent
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the assigned agent</returns>
    Task<ExxerAI.Domain.Result<Agent>> AssignTaskToAgentAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current agentStatus of the orchestration engine
    /// </summary>
    /// <returns>Result containing the orchestration agentStatus</returns>
    Result<OrchestrationStatus> GetStatus();

    /// <summary>
    /// Gets performance metrics for the orchestration engine
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The orchestration metrics</returns>
    Task<ExxerAI.Domain.Result<OrchestrationMetrics>> GetMetricsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the agentStatus of the orchestration engine
/// </summary>
public enum OrchestrationStatus
{
    /// <summary>
    /// Engine is stopped
    /// </summary>
    Stopped,

    /// <summary>
    /// Engine is starting
    /// </summary>
    Starting,

    /// <summary>
    /// Engine is running
    /// </summary>
    Running,

    /// <summary>
    /// Engine is stopping
    /// </summary>
    Stopping,

    /// <summary>
    /// Engine encountered an error
    /// </summary>
    Error
}

/// <summary>
/// Represents performance metrics for the orchestration engine
/// </summary>
public class OrchestrationMetrics
{
    /// <summary>
    /// Gets or sets the number of active agents
    /// </summary>
    public int ActiveAgents { get; set; }

    /// <summary>
    /// Gets or sets the number of pending tasks
    /// </summary>
    public int PendingTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of running workflows
    /// </summary>
    public int RunningWorkflows { get; set; }

    /// <summary>
    /// Gets or sets the number of completed tasks in the last hour
    /// </summary>
    public int CompletedTasksLastHour { get; set; }

    /// <summary>
    /// Gets or sets the average task execution time in milliseconds
    /// </summary>
    public double AverageTaskExecutionTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the engine uptime
    /// </summary>
    public TimeSpan Uptime { get; set; }

    /// <summary>
    /// Gets or sets custom metrics
    /// </summary>
    public Dictionary<string, object> CustomMetrics { get; init; } = new();
}

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

/// <summary>
/// Represents the current workload of an agent
/// </summary>
public class AgentWorkload
{
    /// <summary>
    /// Gets or sets the agent identifier
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// Gets or sets the number of assigned tasks
    /// </summary>
    public int AssignedTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of running tasks
    /// </summary>
    public int RunningTasks { get; set; }

    /// <summary>
    /// Gets or sets the utilization percentage (0-100)
    /// </summary>
    public double UtilizationPercentage { get; set; }

    /// <summary>
    /// Gets or sets the estimated completion time for current tasks
    /// </summary>
    public TimeSpan EstimatedCompletionTime { get; set; }
}

/// <summary>
/// Interface for executing workflow steps
/// </summary>
public interface IWorkflowExecutor
{
    /// <summary>
    /// Executes a workflow
    /// </summary>
    /// <param name="workflow">The workflow to execute</param>
    /// <param name="input">The input data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The workflow execution result</returns>
    Task<ExxerAI.Domain.Result<WorkflowExecution>> ExecuteAsync(
        Workflow workflow,
        Dictionary<string, object> input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses a running workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> PauseExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> ResumeExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a workflow execution
    /// </summary>
    /// <param name="executionId">The execution identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> CancelExecutionAsync(Guid executionId, CancellationToken cancellationToken = default);
}