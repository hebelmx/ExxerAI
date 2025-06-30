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
	public async Task<Result<AgentTask>> CreateTaskAsync(
		string title,
		string description,
		string taskType,
		TaskData input,
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
				Input = input ?? new TaskData(),
				Priority = priority,
				Status = Domain.TaskStatus.Pending,
				Deadline = deadline,
				CreatedAt = DateTime.UtcNow
			};

			var result = await _taskRepository.AddAsync(task, cancellationToken).ConfigureAwait(false);
			return result.IsFailure ? Result<AgentTask>.WithFailure(result.Errors) : Result<AgentTask>.Success(task);
		}
		catch (Exception ex)
		{
			return Result<AgentTask>.WithFailure($"An error occurred while creating the task: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets a task by its identifier
	/// </summary>
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
	/// Gets pending tasks that are ready for assignment
	/// </summary>
	public async Task<Result<IEnumerable<AgentTask>>> GetPendingTasksAsync(
		string? taskType = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await _taskRepository.GetByStatusAsync(Domain.TaskStatus.Pending, cancellationToken).ConfigureAwait(false);
			if (result.IsFailure) 
				return Result<IEnumerable<AgentTask>>.WithFailure(result.Errors);

			var tasks = result.Value ?? Enumerable.Empty<AgentTask>();
			if (!string.IsNullOrWhiteSpace(taskType))
				tasks = tasks.Where(t => string.Equals(t.TaskType, taskType, StringComparison.OrdinalIgnoreCase));

			return Result<IEnumerable<AgentTask>>.Success(tasks);
		}
		catch (Exception ex)
		{
			return Result<IEnumerable<AgentTask>>.WithFailure($"An error occurred while retrieving pending tasks: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets tasks assigned to a specific agent
	/// </summary>
	public async Task<Result<IEnumerable<AgentTask>>> GetAgentTasksAsync(
		Guid agentId,
		Domain.TaskStatus? status = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var result = await _taskRepository.GetByAgentAsync(agentId, status, cancellationToken).ConfigureAwait(false);
			return result.IsFailure ? Result<IEnumerable<AgentTask>>.WithFailure(result.Errors) : result;
		}
		catch (Exception ex)
		{
			return Result<IEnumerable<AgentTask>>.WithFailure($"An error occurred while retrieving agent tasks: {ex.Message}");
		}
	}

	/// <summary>
	/// Starts execution of a task
	/// </summary>
	public async Task<Result> StartTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
	{
		try
		{
			var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			if (taskResult.IsFailure) 
				return Result.WithFailure($"Task with ID {taskId} not found");

			var task = taskResult.Value!;
			if (task.Status != Domain.TaskStatus.Pending)
				return Result.WithFailure($"Task {taskId} is not in Pending status. Current status: {task.Status}");

			task.Status = Domain.TaskStatus.InProgress;
			task.StartedAt = DateTime.UtcNow;

			var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
			return updateResult.IsFailure ? Result.WithFailure(updateResult.Errors) : Result.Success();
		}
		catch (Exception ex)
		{
			return Result.WithFailure($"An error occurred while starting the task: {ex.Message}");
		}
	}

	/// <summary>
	/// Completes a task with output data
	/// </summary>
	public async Task<Result> CompleteTaskAsync(
		Guid taskId,
		TaskData output,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			if (taskResult.IsFailure) 
				return Result.WithFailure($"Task with ID {taskId} not found");

			var task = taskResult.Value!;
			if (task.Status != Domain.TaskStatus.InProgress)
				return Result.WithFailure($"Task {taskId} is not in InProgress status. Current status: {task.Status}");

			task.Status = Domain.TaskStatus.Completed;
			task.CompletedAt = DateTime.UtcNow;
			task.Output = output ?? new TaskData();

			var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
			return updateResult.IsFailure ? Result.WithFailure(updateResult.Errors) : Result.Success();
		}
		catch (Exception ex)
		{
			return Result.WithFailure($"An error occurred while completing the task: {ex.Message}");
		}
	}

	/// <summary>
	/// Fails a task with an error message
	/// </summary>
	public async Task<Result> FailTaskAsync(
		Guid taskId,
		string errorMessage,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			if (taskResult.IsFailure) 
				return Result.WithFailure($"Task with ID {taskId} not found");

			var task = taskResult.Value!;
			if (task.Status != Domain.TaskStatus.InProgress)
				return Result.WithFailure($"Task {taskId} is not in InProgress status. Current status: {task.Status}");

			task.Status = Domain.TaskStatus.Failed;
			task.CompletedAt = DateTime.UtcNow;
			task.ErrorMessage = errorMessage ?? "Task failed without specific error message";

			var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
			return updateResult.IsFailure ? Result.WithFailure(updateResult.Errors) : Result.Success();
		}
		catch (Exception ex)
		{
			return Result.WithFailure($"An error occurred while failing the task: {ex.Message}");
		}
	}

	/// <summary>
	/// Retries a failed task
	/// </summary>
	public async Task<Result> RetryTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
	{
		try
		{
			var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			if (taskResult.IsFailure) 
				return Result.WithFailure($"Task with ID {taskId} not found");

			var task = taskResult.Value!;
			if (task.Status != Domain.TaskStatus.Failed)
				return Result.WithFailure($"Task {taskId} is not in Failed status. Current status: {task.Status}");

			task.Status = Domain.TaskStatus.Pending;
			task.RetryCount++;
			task.ErrorMessage = null;
			task.StartedAt = null;
			task.CompletedAt = null;

			var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
			return updateResult.IsFailure ? Result.WithFailure(updateResult.Errors) : Result.Success();
		}
		catch (Exception ex)
		{
			return Result.WithFailure($"An error occurred while retrying the task: {ex.Message}");
		}
	}

	/// <summary>
	/// Cancels a task
	/// </summary>
	public async Task<Result> CancelTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
	{
		try
		{
			var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			if (taskResult.IsFailure) 
				return Result.WithFailure($"Task with ID {taskId} not found");

			var task = taskResult.Value!;
			if (task.Status == Domain.TaskStatus.Completed || task.Status == Domain.TaskStatus.Cancelled)
				return Result.WithFailure($"Task {taskId} cannot be cancelled. Current status: {task.Status}");

			task.Status = Domain.TaskStatus.Cancelled;
			task.CompletedAt = DateTime.UtcNow;

			var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
			return updateResult.IsFailure ? Result.WithFailure(updateResult.Errors) : Result.Success();
		}
		catch (Exception ex)
		{
			return Result.WithFailure($"An error occurred while cancelling the task: {ex.Message}");
		}
	}

	/// <summary>
	/// Updates task metadata
	/// </summary>
	public async Task<Result> UpdateTaskMetadataAsync(
		Guid taskId,
		TaskMetadata metadata,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
			if (taskResult.IsFailure) 
				return Result.WithFailure($"Task with ID {taskId} not found");

			var task = taskResult.Value!;
			task.Metadata = metadata ?? new TaskMetadata();

			var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken).ConfigureAwait(false);
			return updateResult.IsFailure ? Result.WithFailure(updateResult.Errors) : Result.Success();
		}
		catch (Exception ex)
		{
			return Result.WithFailure($"An error occurred while updating task metadata: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets overdue tasks
	/// </summary>
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
}
