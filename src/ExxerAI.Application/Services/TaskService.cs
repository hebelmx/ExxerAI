using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;

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
	public async Task<ExxerAI.Domain.Result<AgentTask>> CreateTaskAsync(
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
				return ExxerAI.Domain.Result<AgentTask>.WithFailure("Task title cannot be null or empty");

			if (string.IsNullOrWhiteSpace(taskType))
				return ExxerAI.Domain.Result<AgentTask>.WithFailure("Task type cannot be null or empty");

			var task = new AgentTask
			{
				Title = title,
				Description = description ?? string.Empty,
				TaskType = taskType,
				Input = new TaskData(),
				Priority = priority,
				Status = Domain.TaskStatus.Pending,
				Deadline = deadline,
				CreatedAt = DateTime.UtcNow
			};

			var result = await _taskRepository.AddAsync(task, cancellationToken).ConfigureAwait(false);
			return result.IsFailure ? ExxerAI.Domain.Result<AgentTask>.WithFailure(result.Error ?? "Failed to add task") : ExxerAI.Domain.Result<AgentTask>.WithSuccess(task);
		}
		catch (Exception ex)
		{
			return ExxerAI.Domain.Result<AgentTask>.WithFailure($"An error occurred while creating the task: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets a task by its identifier
	/// </summary>
	/// <param name="taskId">The task identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation containing the task if found</returns>
	public async Task<ExxerAI.Domain.Result<AgentTask>> GetTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			return result.IsFailure ? ExxerAI.Domain.Result<AgentTask>.WithFailure($"Task with ID {taskId} not found") : result;
		}
		catch (Exception ex)
		{
			return ExxerAI.Domain.Result<AgentTask>.WithFailure($"An error occurred while retrieving the task: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets all pending tasks
	/// </summary>
	/// <param name="maxCount">Maximum number of tasks to return</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation containing the list of pending tasks</returns>
	public async Task<ExxerAI.Domain.Result<IEnumerable<AgentTask>>> GetPendingTasksAsync(
		int maxCount = 100, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await _taskRepository.GetByStatusAsync(Domain.TaskStatus.Pending, cancellationToken).ConfigureAwait(false);
			if (result.IsFailure) 
				return ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithFailure(result.Error ?? "Failed to retrieve pending tasks");

			var tasks = result.Value ?? Enumerable.Empty<AgentTask>();
			var limitedTasks = tasks.Take(maxCount);

			return ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithSuccess(limitedTasks);
		}
		catch (Exception ex)
		{
			return ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithFailure($"An error occurred while retrieving pending tasks: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets tasks assigned to a specific agent
	/// </summary>
	/// <param name="agentId">The agent identifier</param>
	/// <param name="status">Optional status filter</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation containing the list of agent tasks</returns>
	public async Task<ExxerAI.Domain.Result<IEnumerable<AgentTask>>> GetAgentTasksAsync(
		Guid agentId,
		ExxerAI.Domain.TaskStatus? status = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await _taskRepository.GetByAgentAsync(agentId, status, cancellationToken).ConfigureAwait(false);
			return result.IsFailure ? ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithFailure(result.Error ?? "Failed to retrieve agent tasks") : result;
		}
		catch (Exception ex)
		{
			return ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithFailure($"An error occurred while retrieving agent tasks: {ex.Message}");
		}
	}

	/// <summary>
	/// Updates a task's status
	/// </summary>
	/// <param name="taskId">The task identifier</param>
	/// <param name="status">The new status</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public async Task<ExxerAI.Domain.Result<bool>> UpdateTaskStatusAsync(
		Guid taskId, 
		ExxerAI.Domain.TaskStatus status, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			if (taskResult.IsFailure) 
				return ExxerAI.Domain.Result<bool>.WithFailure($"Task with ID {taskId} not found");

			var task = taskResult.Value!;
			task.Status = status;

			var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
			return updateResult.IsFailure ? ExxerAI.Domain.Result<bool>.WithFailure(updateResult.Error ?? "Failed to update task") : ExxerAI.Domain.Result<bool>.WithSuccess(true);
		}
		catch (Exception ex)
		{
			return ExxerAI.Domain.Result<bool>.WithFailure($"An error occurred while updating task status: {ex.Message}");
		}
	}

	/// <summary>
	/// Assigns a task to an agent
	/// </summary>
	/// <param name="taskId">The task identifier</param>
	/// <param name="agentId">The agent identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public async Task<ExxerAI.Domain.Result<bool>> AssignTaskToAgentAsync(
		Guid taskId, 
		Guid agentId, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			if (taskResult.IsFailure) 
				return ExxerAI.Domain.Result<bool>.WithFailure($"Task with ID {taskId} not found");

			var task = taskResult.Value!;
			if (task.Status != Domain.TaskStatus.Pending)
				return ExxerAI.Domain.Result<bool>.WithFailure($"Task {taskId} is not available for assignment. Current status: {task.Status}");

			task.AssignedAgentId = agentId;

			var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
			return updateResult.IsFailure ? ExxerAI.Domain.Result<bool>.WithFailure(updateResult.Error ?? "Failed to update task") : ExxerAI.Domain.Result<bool>.WithSuccess(true);
		}
		catch (Exception ex)
		{
			return ExxerAI.Domain.Result<bool>.WithFailure($"An error occurred while assigning the task: {ex.Message}");
		}
	}

	/// <summary>
	/// Completes a task with optional output data
	/// </summary>
	/// <param name="taskId">The task identifier</param>
	/// <param name="outputData">Optional output data</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public async Task<ExxerAI.Domain.Result<bool>> CompleteTaskAsync(
		Guid taskId,
		TaskData? outputData = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			if (taskResult.IsFailure) 
				return ExxerAI.Domain.Result<bool>.WithFailure($"Task with ID {taskId} not found");

			var task = taskResult.Value!;
			if (task.Status != Domain.TaskStatus.InProgress)
				return ExxerAI.Domain.Result<bool>.WithFailure($"Task {taskId} is not in InProgress status. Current status: {task.Status}");

			task.Status = Domain.TaskStatus.Completed;
			task.CompletedAt = DateTime.UtcNow;
			task.Output = outputData ?? new TaskData();

			var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
			return updateResult.IsFailure ? ExxerAI.Domain.Result<bool>.WithFailure(updateResult.Error ?? "Failed to update task") : ExxerAI.Domain.Result<bool>.WithSuccess(true);
		}
		catch (Exception ex)
		{
			return ExxerAI.Domain.Result<bool>.WithFailure($"An error occurred while completing the task: {ex.Message}");
		}
	}

	/// <summary>
	/// Fails a task with an error message
	/// </summary>
	/// <param name="taskId">The task identifier</param>
	/// <param name="errorMessage">The error message</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public async Task<ExxerAI.Domain.Result<bool>> FailTaskAsync(
		Guid taskId,
		string errorMessage,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			if (taskResult.IsFailure) 
				return ExxerAI.Domain.Result<bool>.WithFailure($"Task with ID {taskId} not found");

			var task = taskResult.Value!;
			if (task.Status != Domain.TaskStatus.InProgress)
				return ExxerAI.Domain.Result<bool>.WithFailure($"Task {taskId} is not in InProgress status. Current status: {task.Status}");

			task.Status = Domain.TaskStatus.Failed;
			task.CompletedAt = DateTime.UtcNow;
			task.ErrorMessage = errorMessage ?? "Task failed without specific error message";

			var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
			return updateResult.IsFailure ? ExxerAI.Domain.Result<bool>.WithFailure(updateResult.Error ?? "Failed to update task") : ExxerAI.Domain.Result<bool>.WithSuccess(true);
		}
		catch (Exception ex)
		{
			return ExxerAI.Domain.Result<bool>.WithFailure($"An error occurred while failing the task: {ex.Message}");
		}
	}

	/// <summary>
	/// Cancels a task
	/// </summary>
	/// <param name="taskId">The task identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public async Task<ExxerAI.Domain.Result<bool>> CancelTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
	{
		try
		{
			var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			if (taskResult.IsFailure) 
				return ExxerAI.Domain.Result<bool>.WithFailure($"Task with ID {taskId} not found");

			var task = taskResult.Value!;
			if (task.Status == Domain.TaskStatus.Completed || task.Status == Domain.TaskStatus.Cancelled)
				return ExxerAI.Domain.Result<bool>.WithFailure($"Task {taskId} cannot be cancelled. Current status: {task.Status}");

			task.Status = Domain.TaskStatus.Cancelled;
			task.CompletedAt = DateTime.UtcNow;

			var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
			return updateResult.IsFailure ? ExxerAI.Domain.Result<bool>.WithFailure(updateResult.Error ?? "Failed to update task") : ExxerAI.Domain.Result<bool>.WithSuccess(true);
		}
		catch (Exception ex)
		{
			return ExxerAI.Domain.Result<bool>.WithFailure($"An error occurred while cancelling the task: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets overdue tasks
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation containing the list of overdue tasks</returns>
	public async Task<ExxerAI.Domain.Result<IEnumerable<AgentTask>>> GetOverdueTasksAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await _taskRepository.GetOverdueTasksAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			return ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithFailure($"An error occurred while retrieving overdue tasks: {ex.Message}");
		}
	}

	/// <summary>
	/// Deletes a task
	/// </summary>
	/// <param name="taskId">The task identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public async Task<ExxerAI.Domain.Result<bool>> DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
	{
		try
		{
			var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			if (taskResult.IsFailure) 
				return ExxerAI.Domain.Result<bool>.WithFailure($"Task with ID {taskId} not found");

			var deleteResult = await _taskRepository.DeleteAsync(taskId, cancellationToken).ConfigureAwait(false);
			return deleteResult.IsFailure ? ExxerAI.Domain.Result<bool>.WithFailure(deleteResult.Error ?? "Failed to delete task") : ExxerAI.Domain.Result<bool>.WithSuccess(true);
		}
		catch (Exception ex)
		{
			return ExxerAI.Domain.Result<bool>.WithFailure($"An error occurred while deleting the task: {ex.Message}");
		}
	}
}
