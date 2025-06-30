using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;
using Microsoft.Extensions.Logging;

namespace ExxerAI.Application.Services;

public class TaskService : ITaskService
{
private readonly ITaskRepository _taskRepository;
private readonly IAgentRepository _agentRepository;
private readonly ILogger<TaskService> _logger;

public TaskService(
ITaskRepository taskRepository,
IAgentRepository agentRepository,
ILogger<TaskService> logger)
{
_taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
_agentRepository = agentRepository ?? throw new ArgumentNullException(nameof(agentRepository));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
}

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

var result = await _taskRepository.AddAsync(task, cancellationToken);
return result.IsFailure ? Result<AgentTask>.WithFailure(result.Errors) : Result<AgentTask>.Success(task);
}
catch (Exception ex)
{
return Result<AgentTask>.WithFailure($"Error creating task: {ex.Message}");
}
}

public async Task<Result<AgentTask>> GetTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
{
try
{
return await _taskRepository.GetByIdAsync(taskId, cancellationToken);
}
catch (Exception ex)
{
return Result<AgentTask>.WithFailure($"Error retrieving task: {ex.Message}");
}
}

public async Task<Result<IEnumerable<AgentTask>>> GetPendingTasksAsync(string? taskType = null, CancellationToken cancellationToken = default)
{
try
{
var result = await _taskRepository.GetByStatusAsync(Domain.TaskStatus.Pending, cancellationToken);
if (result.IsFailure) return Result<IEnumerable<AgentTask>>.WithFailure(result.Errors);

var tasks = result.Value ?? Enumerable.Empty<AgentTask>();
if (!string.IsNullOrWhiteSpace(taskType))
tasks = tasks.Where(t => string.Equals(t.TaskType, taskType, StringComparison.OrdinalIgnoreCase));

return Result<IEnumerable<AgentTask>>.Success(tasks);
}
catch (Exception ex)
{
return Result<IEnumerable<AgentTask>>.WithFailure($"Error retrieving pending tasks: {ex.Message}");
}
}

public async Task<Result<IEnumerable<AgentTask>>> GetAgentTasksAsync(Guid agentId, Domain.TaskStatus? status = null, CancellationToken cancellationToken = default)
{
try
{
return await _taskRepository.GetByAgentAsync(agentId, status, cancellationToken);
}
catch (Exception ex)
{
return Result<IEnumerable<AgentTask>>.WithFailure($"Error retrieving agent tasks: {ex.Message}");
}
}

public async Task<Result> StartTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
{
try
{
var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
if (taskResult.IsFailure) return Result.WithFailure($"Task {taskId} not found");

var task = taskResult.Value!;
if (task.Status != Domain.TaskStatus.Pending)
return Result.WithFailure($"Task {taskId} is not pending. Status: {task.Status}");

task.Status = Domain.TaskStatus.InProgress;
task.StartedAt = DateTime.UtcNow;

var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken);
return updateResult.IsFailure ? Result.WithFailure(updateResult.Errors) : Result.Success();
}
catch (Exception ex)
{
return Result.WithFailure($"Error starting task: {ex.Message}");
}
}

public async Task<Result> CompleteTaskAsync(Guid taskId, TaskData output, CancellationToken cancellationToken = default)
{
try
{
var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
if (taskResult.IsFailure) return Result.WithFailure($"Task {taskId} not found");

var task = taskResult.Value!;
if (task.Status != Domain.TaskStatus.InProgress)
return Result.WithFailure($"Task {taskId} is not in progress. Status: {task.Status}");

task.Status = Domain.TaskStatus.Completed;
task.CompletedAt = DateTime.UtcNow;
task.Output = output ?? new TaskData();

var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken);
return updateResult.IsFailure ? Result.WithFailure(updateResult.Errors) : Result.Success();
}
catch (Exception ex)
{
return Result.WithFailure($"Error completing task: {ex.Message}");
}
}

public async Task<Result> FailTaskAsync(Guid taskId, string errorMessage, CancellationToken cancellationToken = default)
{
try
{
var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
if (taskResult.IsFailure) return Result.WithFailure($"Task {taskId} not found");

var task = taskResult.Value!;
task.Status = Domain.TaskStatus.Failed;
task.CompletedAt = DateTime.UtcNow;
task.ErrorMessage = errorMessage;

var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken);
return updateResult.IsFailure ? Result.WithFailure(updateResult.Errors) : Result.Success();
}
catch (Exception ex)
{
return Result.WithFailure($"Error failing task: {ex.Message}");
}
}

public async Task<Result> RetryTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
{
try
{
var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
if (taskResult.IsFailure) return Result.WithFailure($"Task {taskId} not found");

var task = taskResult.Value!;
if (task.Status != Domain.TaskStatus.Failed)
return Result.WithFailure($"Task {taskId} is not failed. Status: {task.Status}");

task.Status = Domain.TaskStatus.Pending;
task.RetryCount++;
task.ErrorMessage = null;
task.StartedAt = null;
task.CompletedAt = null;

var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken);
return updateResult.IsFailure ? Result.WithFailure(updateResult.Errors) : Result.Success();
}
catch (Exception ex)
{
return Result.WithFailure($"Error retrying task: {ex.Message}");
}
}

public async Task<Result> CancelTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
{
try
{
var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
if (taskResult.IsFailure) return Result.WithFailure($"Task {taskId} not found");

var task = taskResult.Value!;
if (task.Status == Domain.TaskStatus.Completed || task.Status == Domain.TaskStatus.Cancelled)
return Result.WithFailure($"Task {taskId} cannot be cancelled. Status: {task.Status}");

task.Status = Domain.TaskStatus.Cancelled;
task.CompletedAt = DateTime.UtcNow;

var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken);
return updateResult.IsFailure ? Result.WithFailure(updateResult.Errors) : Result.Success();
}
catch (Exception ex)
{
return Result.WithFailure($"Error cancelling task: {ex.Message}");
}
}

public async Task<Result> UpdateTaskMetadataAsync(Guid taskId, TaskMetadata metadata, CancellationToken cancellationToken = default)
{
try
{
var taskResult = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
if (taskResult.IsFailure) return Result.WithFailure($"Task {taskId} not found");

var task = taskResult.Value!;
task.Metadata = metadata ?? new TaskMetadata();

var updateResult = await _taskRepository.UpdateAsync(task, cancellationToken);
return updateResult.IsFailure ? Result.WithFailure(updateResult.Errors) : Result.Success();
}
catch (Exception ex)
{
return Result.WithFailure($"Error updating task metadata: {ex.Message}");
}
}

public async Task<Result<IEnumerable<AgentTask>>> GetOverdueTasksAsync(CancellationToken cancellationToken = default)
{
try
{
		return await _taskRepository.GetOverdueTasksAsync(cancellationToken);
}
catch (Exception ex)
{
return Result<IEnumerable<AgentTask>>.WithFailure($"Error retrieving overdue tasks: {ex.Message}");
}
}
}
