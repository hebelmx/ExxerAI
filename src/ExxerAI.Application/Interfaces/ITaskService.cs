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
    /// <param name="priority">The task priority</param>
    /// <param name="deadline">Optional deadline for the task</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the created task</returns>
    Task<ExxerAI.Domain.Result<AgentTask>> CreateTaskAsync(
        string title,
        string description,
        string taskType,
        TaskPriority priority = TaskPriority.Normal,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task by its identifier
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the task if found</returns>
    Task<ExxerAI.Domain.Result<AgentTask>> GetTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending tasks
    /// </summary>
    /// <param name="maxCount">Maximum number of tasks to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of pending tasks</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<AgentTask>>> GetPendingTasksAsync(
        int maxCount = 100, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks assigned to a specific agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of agent tasks</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<AgentTask>>> GetAgentTasksAsync(
        Guid agentId, 
        ExxerAI.Domain.TaskStatus? status = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a task's status
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="status">The new status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> UpdateTaskStatusAsync(
        Guid taskId, 
        ExxerAI.Domain.TaskStatus status, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a task to an agent
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> AssignTaskToAgentAsync(
        Guid taskId, 
        Guid agentId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a task with optional output data
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="outputData">Optional output data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> CompleteTaskAsync(
        Guid taskId, 
        TaskData? outputData = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fails a task with an error message
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> FailTaskAsync(
        Guid taskId, 
        string errorMessage, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a task
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> CancelTaskAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets overdue tasks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of overdue tasks</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<AgentTask>>> GetOverdueTasksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
} 