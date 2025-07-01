using System.Collections.Concurrent;
using ExxerAI.Application;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;

namespace ExxerAI.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of task repository for development and testing
/// </summary>
public class InMemoryTaskRepository : ITaskRepository
{
	private readonly ConcurrentDictionary<Guid, AgentTask> _tasks = new();

	/// <summary>
	/// Gets a task by its identifier
	/// </summary>
	/// <param name="id">The task identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The task if found</returns>
	public Task<ExxerAI.Domain.Result<AgentTask>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		if (id == Guid.Empty)
		{
			return Task.FromResult(ExxerAI.Domain.Result<AgentTask>.WithFailure("Task ID cannot be empty"));
		}

		if (_tasks.TryGetValue(id, out var task))
		{
			return Task.FromResult(ExxerAI.Domain.Result<AgentTask>.WithSuccess(task));
		}

		return Task.FromResult(ExxerAI.Domain.Result<AgentTask>.WithFailure($"Task not found with ID: {id}"));
	}

	/// <summary>
	/// Gets all tasks
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of all tasks</returns>
	public Task<ExxerAI.Domain.Result<IEnumerable<AgentTask>>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var tasks = _tasks.Values.AsEnumerable();
		return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithSuccess(tasks));
	}

	/// <summary>
	/// Adds a new task
	/// </summary>
	/// <param name="entity">The task to add</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The added task</returns>
	public Task<ExxerAI.Domain.Result<AgentTask>> AddAsync(AgentTask entity, CancellationToken cancellationToken = default)
	{
		if (entity == null)
		{
			return Task.FromResult(ExxerAI.Domain.Result<AgentTask>.WithFailure("Task cannot be null"));
		}

		if (entity.Id == Guid.Empty)
		{
			entity.Id = Guid.NewGuid();
		}

		if (_tasks.ContainsKey(entity.Id))
		{
			return Task.FromResult(ExxerAI.Domain.Result<AgentTask>.WithFailure($"Task with ID {entity.Id} already exists"));
		}

		entity.CreatedAt = DateTime.UtcNow;

		if (_tasks.TryAdd(entity.Id, entity))
		{
			return Task.FromResult(ExxerAI.Domain.Result<AgentTask>.WithSuccess(entity));
		}

		return Task.FromResult(ExxerAI.Domain.Result<AgentTask>.WithFailure("Failed to add task"));
	}

	/// <summary>
	/// Updates an existing task
	/// </summary>
	/// <param name="entity">The task to update</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The updated task</returns>
	public Task<ExxerAI.Domain.Result<AgentTask>> UpdateAsync(AgentTask entity, CancellationToken cancellationToken = default)
	{
		if (entity == null)
		{
			return Task.FromResult(ExxerAI.Domain.Result<AgentTask>.WithFailure("Task cannot be null"));
		}

		if (entity.Id == Guid.Empty)
		{
			return Task.FromResult(ExxerAI.Domain.Result<AgentTask>.WithFailure("Task ID cannot be empty"));
		}

		if (!_tasks.ContainsKey(entity.Id))
		{
			return Task.FromResult(ExxerAI.Domain.Result<AgentTask>.WithFailure($"Task not found with ID: {entity.Id}"));
		}

		_tasks[entity.Id] = entity;

		return Task.FromResult(ExxerAI.Domain.Result<AgentTask>.WithSuccess(entity));
	}

	/// <summary>
	/// Deletes a task by its identifier
	/// </summary>
	/// <param name="id">The task identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public Task<ExxerAI.Domain.Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		if (id == Guid.Empty)
		{
			return Task.FromResult(ExxerAI.Domain.Result<bool>.WithFailure("Task ID cannot be empty"));
		}

		if (_tasks.TryRemove(id, out _))
		{
			return Task.FromResult(ExxerAI.Domain.Result<bool>.WithSuccess(true));
		}

		return Task.FromResult(ExxerAI.Domain.Result<bool>.WithFailure($"Task not found with ID: {id}"));
	}

	/// <summary>
	/// Checks if a task exists by its identifier
	/// </summary>
	/// <param name="id">The task identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>True if the task exists, false otherwise</returns>
	public Task<ExxerAI.Domain.Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
	{
		if (id == Guid.Empty)
		{
			return Task.FromResult(ExxerAI.Domain.Result<bool>.WithFailure("Task ID cannot be empty"));
		}

		var exists = _tasks.ContainsKey(id);
		return Task.FromResult(ExxerAI.Domain.Result<bool>.WithSuccess(exists));
	}

	/// <summary>
	/// Gets tasks by status
	/// </summary>
	/// <param name="status">The task status</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of tasks with the specified status</returns>
	public Task<ExxerAI.Domain.Result<IEnumerable<AgentTask>>> GetByStatusAsync(
		Domain.TaskStatus status, 
		CancellationToken cancellationToken = default)
	{
		var tasks = _tasks.Values.Where(t => t.Status == status);
		return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithSuccess(tasks));
	}

	/// <summary>
	/// Gets tasks assigned to a specific agent
	/// </summary>
	/// <param name="agentId">The agent identifier</param>
	/// <param name="status">Optional status filter</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of tasks assigned to the agent</returns>
	public Task<ExxerAI.Domain.Result<IEnumerable<AgentTask>>> GetByAgentAsync(
		Guid agentId, 
		Domain.TaskStatus? status = null, 
		CancellationToken cancellationToken = default)
	{
		if (agentId == Guid.Empty)
		{
			return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithFailure("Agent ID cannot be empty"));
		}

		var query = _tasks.Values.Where(t => t.AssignedAgentId == agentId);

		if (status.HasValue)
		{
			query = query.Where(t => t.Status == status.Value);
		}

		var tasks = query.ToList();
		return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithSuccess(tasks));
	}

	/// <summary>
	/// Gets overdue tasks
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of overdue tasks</returns>
	public Task<ExxerAI.Domain.Result<IEnumerable<AgentTask>>> GetOverdueTasksAsync(CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;
		var overdueTasks = _tasks.Values
			.Where(t => t.Deadline.HasValue && t.Deadline.Value < now && 
			           (t.Status == Domain.TaskStatus.Pending || t.Status == Domain.TaskStatus.InProgress))
			.ToList();

		return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithSuccess(overdueTasks));
	}

	/// <summary>
	/// Gets tasks by type
	/// </summary>
	/// <param name="taskType">The task type</param>
	/// <param name="status">Optional status filter</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of tasks of the specified type</returns>
	public Task<ExxerAI.Domain.Result<IEnumerable<AgentTask>>> GetByTypeAsync(
		string taskType, 
		Domain.TaskStatus? status = null, 
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(taskType))
		{
			return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithFailure("Task type cannot be empty"));
		}

		var query = _tasks.Values.Where(t => t.TaskType.Equals(taskType, StringComparison.OrdinalIgnoreCase));

		if (status.HasValue)
		{
			query = query.Where(t => t.Status == status.Value);
		}

		var tasks = query.ToList();
		return Task.FromResult(ExxerAI.Domain.Result<IEnumerable<AgentTask>>.WithSuccess(tasks));
	}

	/// <summary>
	/// Seeds the repository with sample tasks for development/testing
	/// </summary>
	/// <returns>The result of the seeding operation</returns>
	public Task<ExxerAI.Domain.Result<bool>> SeedAsync()
	{
		var sampleTasks = new[]
		{
			new AgentTask
			{
				Id = Guid.NewGuid(),
				Title = "Process Natural Language Query",
				Description = "Process and respond to a natural language query from user",
				TaskType = "natural-language",
				Priority = TaskPriority.Normal,
				Status = Domain.TaskStatus.Pending,
				CreatedAt = DateTime.UtcNow.AddHours(-2),
				Deadline = DateTime.UtcNow.AddHours(24)
			},
			new AgentTask
			{
				Id = Guid.NewGuid(),
				Title = "Analyze Data Patterns",
				Description = "Analyze incoming data for patterns and anomalies",
				TaskType = "analysis",
				Priority = TaskPriority.High,
				Status = Domain.TaskStatus.Pending,
				CreatedAt = DateTime.UtcNow.AddHours(-1),
				Deadline = DateTime.UtcNow.AddHours(12)
			},
			new AgentTask
			{
				Id = Guid.NewGuid(),
				Title = "Generate Code Documentation",
				Description = "Generate comprehensive documentation for the codebase",
				TaskType = "code-generation",
				Priority = TaskPriority.Low,
				Status = Domain.TaskStatus.Pending,
				CreatedAt = DateTime.UtcNow.AddMinutes(-30),
				Deadline = DateTime.UtcNow.AddDays(3)
			}
		};

		foreach (var task in sampleTasks)
		{
			_tasks.TryAdd(task.Id, task);
		}

		return Task.FromResult(ExxerAI.Domain.Result<bool>.WithSuccess(true));
	}
} 