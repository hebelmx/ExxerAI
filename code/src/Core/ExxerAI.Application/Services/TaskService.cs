using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ExxerAI.Application.Services;

/// <summary>
/// Implementation of task service for managing agent tasks and workflows
/// Supports CQRS patterns, validation, and async operations
/// </summary>
public class TaskService : ITaskService
{
    private readonly ILogger<TaskService> _logger;
    private readonly ConcurrentDictionary<string, AgentTask> _tasks;
    private readonly SemaphoreSlim _taskLock;

    /// <summary>
    /// Initializes a new instance of the TaskService
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public TaskService(ILogger<TaskService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tasks = new ConcurrentDictionary<string, AgentTask>();
        _taskLock = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="taskRequest">The task creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the created task</returns>
    public async Task<Result<AgentTask>> CreateTaskAsync(CreateTaskRequest taskRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            if (taskRequest is null)
            {
                _logger.LogWarning("Attempted to create task with null request");
                return Result<AgentTask>.WithFailure("Task request cannot be null");
            }

            var validationResult = ValidateTaskRequest(taskRequest);
            if (!validationResult.IsSuccess)
            {
                _logger.LogWarning("Task creation validation failed: {Errors}",
                    string.Join(", ", validationResult.Errors));
                return Result<AgentTask>.WithFailure(validationResult.Errors);
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _taskLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var task = new AgentTask
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = taskRequest.Name,
                    Description = taskRequest.Description,
                    Priority = taskRequest.Priority,
                    AssignedAgentId = taskRequest.AssignedAgentId,
                    Status = TaskStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>(taskRequest.Metadata)
                };

                _tasks.TryAdd(task.Id, task);

                _logger.LogInformation("Created task {TaskId}: {TaskName} assigned to agent {AgentId}",
                    task.Id, task.Name, task.AssignedAgentId);

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
            return Result<AgentTask>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task {TaskName}", taskRequest?.Name);
            return Result<AgentTask>.WithFailure($"Failed to create task: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="updateRequest">The task update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the updated task</returns>
    public async Task<Result<AgentTask>> UpdateTaskAsync(string taskId, UpdateTaskRequest updateRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                _logger.LogWarning("Attempted to update task with empty ID");
                return Result<AgentTask>.WithFailure("Task ID cannot be empty");
            }

            if (updateRequest is null)
            {
                _logger.LogWarning("Attempted to update task {TaskId} with null request", taskId);
                return Result<AgentTask>.WithFailure("Update request cannot be null");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _taskLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_tasks.TryGetValue(taskId, out var existingTask))
                {
                    _logger.LogWarning("Task {TaskId} not found for update", taskId);
                    return Result<AgentTask>.WithFailure("Task not found");
                }

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(updateRequest.Name))
                    existingTask.Name = updateRequest.Name;

                if (!string.IsNullOrWhiteSpace(updateRequest.Description))
                    existingTask.Description = updateRequest.Description;

                if (updateRequest.Priority.HasValue)
                    existingTask.Priority = updateRequest.Priority.Value;

                if (!string.IsNullOrWhiteSpace(updateRequest.AssignedAgentId))
                    existingTask.AssignedAgentId = updateRequest.AssignedAgentId;

                if (updateRequest.Status.HasValue)
                    existingTask.Status = updateRequest.Status.Value;

                existingTask.UpdatedAt = DateTime.UtcNow;

                // Update metadata
                foreach (var kvp in updateRequest.Metadata)
                {
                    existingTask.Metadata[kvp.Key] = kvp.Value;
                }

                _tasks.TryUpdate(taskId, existingTask, existingTask);

                _logger.LogInformation("Updated task {TaskId}: {TaskName}", taskId, existingTask.Name);
                return Result<AgentTask>.Success(existingTask);
            }
            finally
            {
                _taskLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Update task operation was cancelled");
            return Result<AgentTask>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", taskId);
            return Result<AgentTask>.WithFailure($"Failed to update task: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a task by its identifier
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the task</returns>
    public async Task<Result<AgentTask>> GetTaskByIdAsync(string taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                _logger.LogWarning("Attempted to get task with empty ID");
                return Result<AgentTask>.WithFailure("Task ID cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            if (_tasks.TryGetValue(taskId, out var task))
            {
                _logger.LogDebug("Retrieved task {TaskId}: {TaskName}", taskId, task.Name);
                return Result<AgentTask>.Success(task);
            }

            _logger.LogWarning("Task {TaskId} not found", taskId);
            return Result<AgentTask>.WithFailure("Task not found");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get task operation was cancelled");
            return Result<AgentTask>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}", taskId);
            return Result<AgentTask>.WithFailure($"Failed to retrieve task: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all tasks assigned to a specific agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the tasks</returns>
    public async Task<Result<IEnumerable<AgentTask>>> GetTasksByAgentAsync(string agentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(agentId))
            {
                _logger.LogWarning("Attempted to get tasks with empty agent ID");
                return Result<IEnumerable<AgentTask>>.WithFailure("Agent ID cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            var tasks = _tasks.Values
                .Where(t => t.AssignedAgentId == agentId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            _logger.LogDebug("Retrieved {TaskCount} tasks for agent {AgentId}", tasks.Count, agentId);
            return Result<IEnumerable<AgentTask>>.Success(tasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get tasks by agent operation was cancelled");
            return Result<IEnumerable<AgentTask>>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for agent {AgentId}", agentId);
            return Result<IEnumerable<AgentTask>>.WithFailure($"Failed to retrieve tasks: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets tasks filtered by status
    /// </summary>
    /// <param name="status">The task status to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the tasks</returns>
    public async Task<Result<IEnumerable<AgentTask>>> GetTasksByStatusAsync(TaskStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            var tasks = _tasks.Values
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            _logger.LogDebug("Retrieved {TaskCount} tasks with status {Status}", tasks.Count, status);
            return Result<IEnumerable<AgentTask>>.Success(tasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get tasks by status operation was cancelled");
            return Result<IEnumerable<AgentTask>>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks with status {Status}", status);
            return Result<IEnumerable<AgentTask>>.WithFailure($"Failed to retrieve tasks: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a task
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result<bool>> DeleteTaskAsync(string taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                _logger.LogWarning("Attempted to delete task with empty ID");
                return Result<bool>.WithFailure("Task ID cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _taskLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_tasks.TryRemove(taskId, out var removedTask))
                {
                    _logger.LogWarning("Task {TaskId} not found for deletion", taskId);
                    return Result<bool>.WithFailure("Task not found");
                }

                _logger.LogInformation("Deleted task {TaskId}: {TaskName}", taskId, removedTask.Name);
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
    /// Assigns a task to an agent
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the updated task</returns>
    public async Task<Result<AgentTask>> AssignTaskToAgentAsync(string taskId, string agentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                _logger.LogWarning("Attempted to assign task with empty task ID");
                return Result<AgentTask>.WithFailure("Task ID cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(agentId))
            {
                _logger.LogWarning("Attempted to assign task {TaskId} with empty agent ID", taskId);
                return Result<AgentTask>.WithFailure("Agent ID cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _taskLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_tasks.TryGetValue(taskId, out var task))
                {
                    _logger.LogWarning("Task {TaskId} not found for assignment", taskId);
                    return Result<AgentTask>.WithFailure("Task not found");
                }

                var previousAgentId = task.AssignedAgentId;
                task.AssignedAgentId = agentId;
                task.Status = TaskStatus.Assigned;
                task.UpdatedAt = DateTime.UtcNow;

                _tasks.TryUpdate(taskId, task, task);

                _logger.LogInformation("Assigned task {TaskId} from agent {PreviousAgentId} to agent {NewAgentId}",
                    taskId, previousAgentId, agentId);

                return Result<AgentTask>.Success(task);
            }
            finally
            {
                _taskLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Assign task operation was cancelled");
            return Result<AgentTask>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning task {TaskId} to agent {AgentId}", taskId, agentId);
            return Result<AgentTask>.WithFailure($"Failed to assign task: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the status of a task
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="status">The new status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the updated task</returns>
    public async Task<Result<AgentTask>> UpdateTaskStatusAsync(string taskId, TaskStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                _logger.LogWarning("Attempted to update task status with empty task ID");
                return Result<AgentTask>.WithFailure("Task ID cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _taskLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_tasks.TryGetValue(taskId, out var task))
                {
                    _logger.LogWarning("Task {TaskId} not found for status update", taskId);
                    return Result<AgentTask>.WithFailure("Task not found");
                }

                var previousStatus = task.Status;
                task.Status = status;
                task.UpdatedAt = DateTime.UtcNow;

                // Set completion time if task is completed
                if (status == TaskStatus.Completed && !task.CompletedAt.HasValue)
                {
                    task.CompletedAt = DateTime.UtcNow;
                }

                _tasks.TryUpdate(taskId, task, task);

                _logger.LogInformation("Updated task {TaskId} status from {PreviousStatus} to {NewStatus}",
                    taskId, previousStatus, status);

                return Result<AgentTask>.Success(task);
            }
            finally
            {
                _taskLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Update task status operation was cancelled");
            return Result<AgentTask>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for task {TaskId} to {Status}", taskId, status);
            return Result<AgentTask>.WithFailure($"Failed to update task status: {ex.Message}");
        }
    }

    // Private helper methods

    private static Result<bool> ValidateTaskRequest(CreateTaskRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add("Task name is required");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            errors.Add("Task description is required");
        }

        if (string.IsNullOrWhiteSpace(request.AssignedAgentId))
        {
            errors.Add("Assigned agent ID is required");
        }

        return errors.Any() 
            ? Result<bool>.WithFailure(errors) 
            : Result<bool>.Success(true);
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

/// <summary>
/// Request for creating a new task
/// </summary>
public class CreateTaskRequest
{
    /// <summary>
    /// Gets or sets the task name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task priority
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    /// <summary>
    /// Gets or sets the assigned agent identifier
    /// </summary>
    public string AssignedAgentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional task metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Request for updating an existing task
/// </summary>
public class UpdateTaskRequest
{
    /// <summary>
    /// Gets or sets the task name (optional)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the task description (optional)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the task priority (optional)
    /// </summary>
    public TaskPriority? Priority { get; set; }

    /// <summary>
    /// Gets or sets the assigned agent identifier (optional)
    /// </summary>
    public string? AssignedAgentId { get; set; }

    /// <summary>
    /// Gets or sets the task status (optional)
    /// </summary>
    public TaskStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets additional task metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Task priority levels
/// </summary>
public enum TaskPriority
{
    /// <summary>
    /// Low priority task
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium priority task
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High priority task
    /// </summary>
    High = 3,

    /// <summary>
    /// Critical priority task
    /// </summary>
    Critical = 4
}

/// <summary>
/// Task status values
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task is pending assignment or start
    /// </summary>
    Pending,

    /// <summary>
    /// Task has been assigned to an agent
    /// </summary>
    Assigned,

    /// <summary>
    /// Task is currently in progress
    /// </summary>
    InProgress,

    /// <summary>
    /// Task has been completed successfully
    /// </summary>
    Completed,

    /// <summary>
    /// Task has failed or been cancelled
    /// </summary>
    Failed,

    /// <summary>
    /// Task has been cancelled
    /// </summary>
    Cancelled
}

