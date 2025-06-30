using ExxerAI.Domain;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service interface for managing tasks in the ExxerAI system
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="title">The task title</param>
    /// <param name="description">The task description</param>
    /// <param name="taskType">The task type</param>
    /// <param name="input">The task input data</param>
    /// <param name="priority">The task priority</param>
    /// <param name="deadline">The optional deadline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the created task</returns>
    Task<Result<AgentTask>> CreateTaskAsync(
        string title,
        string description,
        string taskType,
        TaskData input,
        TaskPriority priority = TaskPriority.Normal,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task by its identifier
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the task if found</returns>
    Task<Result<AgentTask>> GetTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending tasks that are ready for assignment
    /// </summary>
    /// <param name="taskType">Optional task type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of pending tasks</returns>
    Task<Result<IEnumerable<AgentTask>>> GetPendingTasksAsync(
        string? taskType = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks assigned to a specific agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of agent tasks</returns>
    Task<Result<IEnumerable<AgentTask>>> GetAgentTasksAsync(
        Guid agentId, 
        Domain.TaskStatus? status = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts execution of a task
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> StartTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a task with output data
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="output">The task output data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> CompleteTaskAsync(
        Guid taskId, 
        TaskData output, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fails a task with an error message
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> FailTaskAsync(
        Guid taskId, 
        string errorMessage, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries a failed task
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> RetryTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a task
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> CancelTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates task metadata
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="metadata">The task metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> UpdateTaskMetadataAsync(
        Guid taskId, 
        TaskMetadata metadata, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets overdue tasks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of overdue tasks</returns>
    Task<Result<IEnumerable<AgentTask>>> GetOverdueTasksAsync(CancellationToken cancellationToken = default);
} 