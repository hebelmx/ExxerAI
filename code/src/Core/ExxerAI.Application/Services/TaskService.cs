using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Helpers.Operations;

namespace ExxerAI.Application.Services;

/// <summary>
/// Service implementation for managing tasks in the ExxerAI system
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAgentRepository _agentRepository;

    /// <summary>
    /// Initializes a new instance of the TaskService class
    /// </summary>
    /// <param name="taskRepository">The task repository</param>
    /// <param name="agentRepository">The agent repository</param>
    public TaskService(
        ITaskRepository taskRepository,
        IAgentRepository agentRepository)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));
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
        try
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result<AgentTask>.WithFailure("Task title cannot be null or empty");

            if (string.IsNullOrWhiteSpace(taskType))
                return Result<AgentTask>.WithFailure("Task type cannot be null or empty");

            var task = new AgentTask
            {
                Title = title,
                Description = description ?? string.Empty,
                TaskType = taskType,
                Input = new TaskData(),
                Priority = priority,
                AgentStatus = TaskAgentStatus.Pending,
                Deadline = deadline,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _taskRepository.AddAsync(task, cancellationToken).ConfigureAwait(false);
            return result.IsFailure ? Result<AgentTask>.WithFailure(result.Error ?? "Failed to add task") : Result<AgentTask>.WithSuccess(task);
        }
        catch (Exception ex)
        {
            return Result<AgentTask>.WithFailure($"An error occurred while creating the task: {ex.Message}");
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
        try
        {
            var result = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
            return result.IsFailure ? Result<AgentTask>.WithFailure($"Task with ID {taskId} not found") : result;
        }
        catch (Exception ex)
        {
            return Result<AgentTask>.WithFailure($"An error occurred while retrieving the task: {ex.Message}");
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
        try
        {
            var result = await _taskRepository.GetByStatusAsync(TaskAgentStatus.Pending, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
                return Result<IEnumerable<AgentTask>>.WithFailure(result.Error ?? "Failed to retrieve pending tasks");

            var tasks = result.Value ?? Enumerable.Empty<AgentTask>();
            var limitedTasks = tasks.Take(maxCount);

            return Result<IEnumerable<AgentTask>>.WithSuccess(limitedTasks);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<AgentTask>>.WithFailure($"An error occurred while retrieving pending tasks: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets tasks assigned to a specific agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="status">Optional agentStatus filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of agent tasks</returns>
    public async Task<Result<IEnumerable<AgentTask>>> GetAgentTasksAsync(
        Guid agentId,
       TaskAgentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _taskRepository.GetByAgentAsync(agentId, status, cancellationToken).ConfigureAwait(false);
            return result.IsFailure ? Result<IEnumerable<AgentTask>>.WithFailure(result.Error ?? "Failed to retrieve agent tasks") : result;
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<AgentTask>>.WithFailure($"An error occurred while retrieving agent tasks: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates a task's agentStatus
    /// </summary>
    /// <param name="taskId">The task identifier</param>
    /// <param name="agentStatus">The new agentStatus</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> UpdateTaskStatusAsync(
        Guid taskId,
       TaskAgentStatus agentStatus,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
            if (taskResult.IsFailure)
                return Result<bool>.WithFailure($"Task with ID {taskId} not found");

            var task = taskResult.Value!;
            task.AgentStatus = agentStatus;

            var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
            return updateResult.IsFailure ? Result<bool>.WithFailure(updateResult.Error ?? "Failed to update task") : Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"An error occurred while updating task agentStatus: {ex.Message}");
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
        try
        {
            var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
            if (taskResult.IsFailure)
                return Result<bool>.WithFailure($"Task with ID {taskId} not found");

            var task = taskResult.Value!;
            if (task.AgentStatus != TaskAgentStatus.Pending)
                return Result<bool>.WithFailure($"Task {taskId} is not available for assignment. Current agentStatus: {task.AgentStatus}");

            task.AssignedAgentId = agentId;

            var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
            return updateResult.IsFailure ? Result<bool>.WithFailure(updateResult.Error ?? "Failed to update task") : Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"An error occurred while assigning the task: {ex.Message}");
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
        try
        {
            var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
            if (taskResult.IsFailure)
                return Result<bool>.WithFailure($"Task with ID {taskId} not found");

            var task = taskResult.Value!;
            if (task.AgentStatus != TaskAgentStatus.InProgress)
                return Result<bool>.WithFailure($"Task {taskId} is not in InProgress agentStatus. Current agentStatus: {task.AgentStatus}");

            task.AgentStatus = TaskAgentStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.Output = outputData ?? new TaskData();

            var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
            return updateResult.IsFailure ? Result<bool>.WithFailure(updateResult.Error ?? "Failed to update task") : Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"An error occurred while completing the task: {ex.Message}");
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
        try
        {
            var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
            if (taskResult.IsFailure)
                return Result<bool>.WithFailure($"Task with ID {taskId} not found");

            var task = taskResult.Value!;
            if (task.AgentStatus != TaskAgentStatus.InProgress)
                return Result<bool>.WithFailure($"Task {taskId} is not in InProgress agentStatus. Current agentStatus: {task.AgentStatus}");

            task.AgentStatus = TaskAgentStatus.Failed;
            task.CompletedAt = DateTime.UtcNow;
            task.ErrorMessage = errorMessage ?? "Task failed without specific error message";

            var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
            return updateResult.IsFailure ? Result<bool>.WithFailure(updateResult.Error ?? "Failed to update task") : Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"An error occurred while failing the task: {ex.Message}");
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
        try
        {
            var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
            if (taskResult.IsFailure)
                return Result<bool>.WithFailure($"Task with ID {taskId} not found");

            var task = taskResult.Value!;
            if (task.AgentStatus == TaskAgentStatus.Completed || task.AgentStatus == TaskAgentStatus.Cancelled)
                return Result<bool>.WithFailure($"Task {taskId} cannot be cancelled. Current agentStatus: {task.AgentStatus}");

            task.AgentStatus = TaskAgentStatus.Cancelled;
            task.CompletedAt = DateTime.UtcNow;

            var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
            return updateResult.IsFailure ? Result<bool>.WithFailure(updateResult.Error ?? "Failed to update task") : Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"An error occurred while cancelling the task: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets overdue tasks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the list of overdue tasks</returns>
    public async Task<Result<IEnumerable<AgentTask>>> GetOverdueTasksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _taskRepository.GetOverdueTasksAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<AgentTask>>.WithFailure($"An error occurred while retrieving overdue tasks: {ex.Message}");
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
            var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
            if (taskResult.IsFailure)
                return Result<bool>.WithFailure($"Task with ID {taskId} not found");

            var deleteResult = await _taskRepository.DeleteAsync(taskId, cancellationToken).ConfigureAwait(false);
            return deleteResult.IsFailure ? Result<bool>.WithFailure(deleteResult.Error ?? "Failed to delete task") : Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"An error occurred while deleting the task: {ex.Message}");
        }
    }
}
