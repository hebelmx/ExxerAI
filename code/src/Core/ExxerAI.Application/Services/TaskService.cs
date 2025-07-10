using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Application.Services;

/// <summary>
/// Implementation of task service for managing agent tasks and workflows
/// Supports CQRS patterns, validation, and async operations
/// </summary>
public class TaskService : ITaskService
{
    private readonly ILogger<TaskService> _logger;
    private readonly ConcurrentDictionary<Guid, AgentTask> _tasks;
    private readonly SemaphoreSlim _taskLock;

    /// <summary>
    /// Initializes a new instance of the TaskService
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public TaskService(ILogger<TaskService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tasks = new ConcurrentDictionary<Guid, AgentTask>();
        _taskLock = new SemaphoreSlim(1, 1);
    }

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
    public async Task<Result<AgentTask>> CreateTaskAsync(
        string title,
        string description,
        string taskType,
        TaskPriority priority = TaskPriority.Normal,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<AgentTask>();

        try
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                _logger.LogWarning("Attempted to create task with empty title");
                return Result<AgentTask>.WithFailure("Task title cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                _logger.LogWarning("Attempted to create task with empty description");
                return Result<AgentTask>.WithFailure("Task description cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(taskType))
            {
                _logger.LogWarning("Attempted to create task with empty task type");
                return Result<AgentTask>.WithFailure("Task type cannot be empty");
            }

            await _taskLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var task = new AgentTask
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Description = description,
                    TaskType = taskType,
                    AgentStatus = TaskAgentStatus.Pending,
                    Priority = priority,
                    CreatedAt = DateTime.UtcNow,
                    Deadline = deadline
                };

                _tasks.TryAdd(task.Id, task);

                _logger.LogInformation("Created task {TaskId}: {TaskTitle} with type {TaskType} and priority {Priority}",
                    task.Id, title, taskType, priority);

                return Result<AgentTask>.Success(task);
            }
            finally
            {
                _taskLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Create task operation was cancelled");
            return ResultExtensions.Cancelled<AgentTask>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task {TaskTitle}", title);
            return Result<AgentTask>.WithFailure($"Failed to create task: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a task by its identifier
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the task if found</returns>
    public async Task<Result<AgentTask>> GetTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<AgentTask>();

        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            if (_tasks.TryGetValue(taskId, out var task))
            {
                _logger.LogDebug("Retrieved task {TaskId}: {TaskName}", taskId, task.Title);
                return Result<AgentTask>.Success(task);
            }

            _logger.LogWarning("Task {TaskId} not found", taskId);
            return Result<AgentTask>.WithFailure("Task not found");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get task operation was cancelled");
            return ResultExtensions.Cancelled<AgentTask>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}", taskId);
            return Result<AgentTask>.WithFailure($"Failed to retrieve task: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all pending tasks
    /// </summary>
    /// <param name="maxCount">Maximum number of tasks to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of pending tasks</returns>
    public async Task<Result<IEnumerable<AgentTask>>> GetPendingTasksAsync(
        int maxCount = 100, 
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<IEnumerable<AgentTask>>();

        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            var tasks = _tasks.Values
                .Where(t => t.AgentStatus == TaskAgentStatus.Pending)
                .OrderByDescending(t => t.CreatedAt)
                .Take(maxCount)
                .ToList();

            _logger.LogDebug("Retrieved {TaskCount} pending tasks (max: {MaxCount})", tasks.Count, maxCount);
            return Result<IEnumerable<AgentTask>>.Success(tasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get pending tasks operation was cancelled");
            return ResultExtensions.Cancelled<IEnumerable<AgentTask>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending tasks");
            return Result<IEnumerable<AgentTask>>.WithFailure($"Failed to retrieve pending tasks: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets tasks assigned to a specific agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of agent tasks</returns>
    public async Task<Result<IEnumerable<AgentTask>>> GetAgentTasksAsync(
        Guid agentId, 
        TaskAgentStatus? status = null, 
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<IEnumerable<AgentTask>>();

        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            var tasks = _tasks.Values
                .Where(t => t.AssignedAgentId == agentId)
                .Where(t => status == null || t.AgentStatus == status.Value)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            _logger.LogDebug("Retrieved {TaskCount} tasks for agent {AgentId} with status filter {Status}", 
                tasks.Count, agentId, status?.ToString() ?? "none");
            return Result<IEnumerable<AgentTask>>.Success(tasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get agent tasks operation was cancelled");
            return ResultExtensions.Cancelled<IEnumerable<AgentTask>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for agent {AgentId}", agentId);
            return Result<IEnumerable<AgentTask>>.WithFailure($"Failed to retrieve agent tasks: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates a task's status
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="agentStatus">The new status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> UpdateTaskStatusAsync(
        Guid taskId, 
        TaskAgentStatus agentStatus, 
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<bool>();

        try
        {
            await _taskLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_tasks.TryGetValue(taskId, out var task))
                {
                    _logger.LogWarning("Task {TaskId} not found for status update", taskId);
                    return Result<bool>.WithFailure("Task not found");
                }

                var previousStatus = task.AgentStatus;
                task.AgentStatus = agentStatus;

                // Set completion time if task is completed
                if (agentStatus == TaskAgentStatus.Completed && !task.CompletedAt.HasValue)
                {
                    task.CompletedAt = DateTime.UtcNow;
                }

                // Set started time if task is in progress
                if (agentStatus == TaskAgentStatus.InProgress && !task.StartedAt.HasValue)
                {
                    task.StartedAt = DateTime.UtcNow;
                }

                _tasks.TryUpdate(taskId, task, task);

                _logger.LogInformation("Updated task {TaskId} status from {PreviousStatus} to {NewStatus}",
                    taskId, previousStatus, agentStatus);

                return Result<bool>.Success(true);
            }
            finally
            {
                _taskLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Update task status operation was cancelled");
            return ResultExtensions.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for task {TaskId} to {Status}", taskId, agentStatus);
            return Result<bool>.WithFailure($"Failed to update task status: {ex.Message}");
        }
    }

    /// <summary>
    /// Assigns a task to an agent
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> AssignTaskToAgentAsync(
        Guid taskId, 
        Guid agentId, 
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<bool>();

        try
        {
            await _taskLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_tasks.TryGetValue(taskId, out var task))
                {
                    _logger.LogWarning("Task {TaskId} not found for assignment", taskId);
                    return Result<bool>.WithFailure("Task not found");
                }

                var previousAgentId = task.AssignedAgentId;
                task.AssignedAgentId = agentId;
                task.AgentStatus = TaskAgentStatus.Pending; // Reset to pending when reassigned

                _tasks.TryUpdate(taskId, task, task);

                _logger.LogInformation("Assigned task {TaskId} from agent {PreviousAgentId} to agent {NewAgentId}",
                    taskId, previousAgentId, agentId);

                return Result<bool>.Success(true);
            }
            finally
            {
                _taskLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Assign task operation was cancelled");
            return ResultExtensions.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning task {TaskId} to agent {AgentId}", taskId, agentId);
            return Result<bool>.WithFailure($"Failed to assign task: {ex.Message}");
        }
    }

    /// <summary>
    /// Completes a task with optional output data
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="outputData">Optional output data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> CompleteTaskAsync(
        Guid taskId, 
        TaskData? outputData = null, 
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<bool>();

        try
        {
            await _taskLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_tasks.TryGetValue(taskId, out var task))
                {
                    _logger.LogWarning("Task {TaskId} not found for completion", taskId);
                    return Result<bool>.WithFailure("Task not found");
                }

                task.AgentStatus = TaskAgentStatus.Completed;
                task.CompletedAt = DateTime.UtcNow;

                // Store output data if provided
                if (outputData != null)
                {
                    task.Output = outputData;
                }

                _tasks.TryUpdate(taskId, task, task);

                _logger.LogInformation("Completed task {TaskId}: {TaskName}", taskId, task.Title);
                return Result<bool>.Success(true);
            }
            finally
            {
                _taskLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Complete task operation was cancelled");
            return ResultExtensions.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing task {TaskId}", taskId);
            return Result<bool>.WithFailure($"Failed to complete task: {ex.Message}");
        }
    }

    /// <summary>
    /// Fails a task with an error message
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> FailTaskAsync(
        Guid taskId, 
        string errorMessage, 
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<bool>();

        try
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                _logger.LogWarning("Attempted to fail task {TaskId} with empty error message", taskId);
                return Result<bool>.WithFailure("Error message cannot be empty");
            }

            await _taskLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_tasks.TryGetValue(taskId, out var task))
                {
                    _logger.LogWarning("Task {TaskId} not found for failure", taskId);
                    return Result<bool>.WithFailure("Task not found");
                }

                task.AgentStatus = TaskAgentStatus.Failed;
                task.ErrorMessage = errorMessage;

                _tasks.TryUpdate(taskId, task, task);

                _logger.LogWarning("Failed task {TaskId}: {TaskName} - {ErrorMessage}", taskId, task.Title, errorMessage);
                return Result<bool>.Success(true);
            }
            finally
            {
                _taskLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Fail task operation was cancelled");
            return ResultExtensions.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error failing task {TaskId}", taskId);
            return Result<bool>.WithFailure($"Failed to fail task: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancels a task
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> CancelTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<bool>();

        try
        {
            await _taskLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_tasks.TryGetValue(taskId, out var task))
                {
                    _logger.LogWarning("Task {TaskId} not found for cancellation", taskId);
                    return Result<bool>.WithFailure("Task not found");
                }

                task.AgentStatus = TaskAgentStatus.Cancelled;

                _tasks.TryUpdate(taskId, task, task);

                _logger.LogInformation("Cancelled task {TaskId}: {TaskName}", taskId, task.Title);
                return Result<bool>.Success(true);
            }
            finally
            {
                _taskLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cancel task operation was cancelled");
            return ResultExtensions.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling task {TaskId}", taskId);
            return Result<bool>.WithFailure($"Failed to cancel task: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets overdue tasks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of overdue tasks</returns>
    public async Task<Result<IEnumerable<AgentTask>>> GetOverdueTasksAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<IEnumerable<AgentTask>>();

        try
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            var now = DateTime.UtcNow;
            var overdueTasks = _tasks.Values
                .Where(t => t.AgentStatus != TaskAgentStatus.Completed && 
                           t.AgentStatus != TaskAgentStatus.Cancelled && 
                           t.AgentStatus != TaskAgentStatus.Failed)
                .Where(t => t.Deadline.HasValue && t.Deadline.Value < now)
                .OrderBy(t => t.Deadline ?? DateTime.MaxValue)
                .ToList();

            _logger.LogDebug("Retrieved {TaskCount} overdue tasks", overdueTasks.Count);
            return Result<IEnumerable<AgentTask>>.Success(overdueTasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get overdue tasks operation was cancelled");
            return ResultExtensions.Cancelled<IEnumerable<AgentTask>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue tasks");
            return Result<IEnumerable<AgentTask>>.WithFailure($"Failed to retrieve overdue tasks: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a task
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _taskLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_tasks.TryRemove(taskId, out var removedTask))
                {
                    _logger.LogWarning("Task {TaskId} not found for deletion", taskId);
                    return Result<bool>.WithFailure("Task not found");
                }

                _logger.LogInformation("Deleted task {TaskId}: {TaskName}", taskId, removedTask.Title);
                return Result<bool>.Success(true);
            }
            finally
            {
                _taskLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Delete task operation was cancelled");
            return Result<bool>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", taskId);
            return Result<bool>.WithFailure($"Failed to delete task: {ex.Message}");
        }
    }

    /// <summary>
    /// Disposes the resources used by the TaskService
    /// </summary>
    public void Dispose()
    {
        _taskLock?.Dispose();
        _tasks.Clear();
        GC.SuppressFinalize(this);
    }
}

