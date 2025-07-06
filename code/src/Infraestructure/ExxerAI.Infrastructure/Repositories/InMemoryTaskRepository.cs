using System.Collections.Concurrent;
using ExxerAI.Application;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.Operations;

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
	public Task<Result<AgentTask>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		if (id == Guid.Empty)
		{
			return Task.FromResult(Result<AgentTask>.WithFailure("Task ID cannot be empty"));
		}

		if (_tasks.TryGetValue(id, out var task))
		{
			return Task.FromResult(Result<AgentTask>.WithSuccess(task));
		}

		return Task.FromResult(Result<AgentTask>.WithFailure("Task not found"));
	}

	/// <summary>
	/// Gets all tasks
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of all tasks</returns>
	public Task<Result<IEnumerable<AgentTask>>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var tasks = _tasks.Values.AsEnumerable();
		return Task.FromResult(Result<IEnumerable<AgentTask>>.WithSuccess(tasks));
	}

	/// <summary>
	/// Adds a new task
	/// </summary>
	/// <param name="entity">The task to add</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The added task</returns>
	public Task<Result<AgentTask>> AddAsync(AgentTask entity, CancellationToken cancellationToken = default)
	{
		if (entity == null)
		{
			return Task.FromResult(Result<AgentTask>.WithFailure("Task cannot be null"));
		}

		// Create a copy to avoid modifying the original entity
		var taskToAdd = new AgentTask
		{
			Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id,
			Title = entity.Title,
			Description = entity.Description,
			TaskType = entity.TaskType,
			Priority = entity.Priority,
			AgentStatus = entity.AgentStatus,
			AssignedAgentId = entity.AssignedAgentId,
			Deadline = entity.Deadline,
			CreatedAt = DateTime.UtcNow
		};

		if (_tasks.ContainsKey(taskToAdd.Id))
		{
			return Task.FromResult(Result<AgentTask>.WithFailure($"Task with ID {taskToAdd.Id} already exists"));
		}

		if (_tasks.TryAdd(taskToAdd.Id, taskToAdd))
		{
			return Task.FromResult(Result<AgentTask>.WithSuccess(taskToAdd));
		}

		return Task.FromResult(Result<AgentTask>.WithFailure("Failed to add task"));
	}

	/// <summary>
	/// Updates an existing task
	/// </summary>
	/// <param name="entity">The task to update</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The updated task</returns>
	public Task<Result<AgentTask>> UpdateAsync(AgentTask entity, CancellationToken cancellationToken = default)
	{
		if (entity == null)
		{
			return Task.FromResult(Result<AgentTask>.WithFailure("Task cannot be null"));
		}

		if (entity.Id == Guid.Empty)
		{
			return Task.FromResult(Result<AgentTask>.WithFailure("Task ID cannot be empty"));
		}

		        if (!_tasks.ContainsKey(entity.Id))
        {
            return Task.FromResult(Result<AgentTask>.WithFailure($"Task not found with ID: {entity.Id}"));
        }

		_tasks[entity.Id] = entity;

		return Task.FromResult(Result<AgentTask>.WithSuccess(entity));
	}

	/// <summary>
	/// Deletes a task by its identifier
	/// </summary>
	/// <param name="id">The task identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The result of the operation</returns>
	public Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		if (id == Guid.Empty)
		{
			return Task.FromResult(Result<bool>.WithFailure("Task ID cannot be empty"));
		}

		if (_tasks.TryRemove(id, out _))
		{
			return Task.FromResult(Result<bool>.WithSuccess(true));
		}

		        return Task.FromResult(Result<bool>.WithFailure($"Task not found with ID: {id}"));
	}

	/// <summary>
	/// Checks if a task exists by its identifier
	/// </summary>
	/// <param name="id">The task identifier</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>True if the task exists, false otherwise</returns>
	public Task<Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
	{
		if (id == Guid.Empty)
		{
			return Task.FromResult(Result<bool>.WithFailure("Task ID cannot be empty"));
		}

		var exists = _tasks.ContainsKey(id);
		return Task.FromResult(Result<bool>.WithSuccess(exists));
	}

	/// <summary>
	/// Gets tasks by agentStatus
	/// </summary>
	/// <param name="agentStatus">The task agentStatus</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of tasks with the specified agentStatus</returns>
	public Task<Result<IEnumerable<AgentTask>>> GetByStatusAsync(
		TaskAgentStatus agentStatus, 
		CancellationToken cancellationToken = default)
	{
		var tasks = _tasks.Values.Where(t => t.AgentStatus == agentStatus);
		return Task.FromResult(Result<IEnumerable<AgentTask>>.WithSuccess(tasks));
	}

	/// <summary>
	/// Gets tasks assigned to a specific agent
	/// </summary>
	/// <param name="agentId">The agent identifier</param>
	/// <param name="status">Optional agentStatus filter</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of tasks assigned to the agent</returns>
	public Task<Result<IEnumerable<AgentTask>>> GetByAgentAsync(
		Guid agentId, 
		TaskAgentStatus? status = null, 
		CancellationToken cancellationToken = default)
	{
		if (agentId == Guid.Empty)
		{
			return Task.FromResult(Result<IEnumerable<AgentTask>>.WithFailure("Agent ID cannot be empty"));
		}

		var query = _tasks.Values.Where(t => t.AssignedAgentId == agentId);

		if (status.HasValue)
		{
			query = query.Where(t => t.AgentStatus == status.Value);
		}

		var tasks = query.ToList();
		return Task.FromResult(Result<IEnumerable<AgentTask>>.WithSuccess(tasks));
	}

	/// <summary>
	/// Gets overdue tasks
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of overdue tasks</returns>
	public Task<Result<IEnumerable<AgentTask>>> GetOverdueTasksAsync(CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;
		var overdueTasks = _tasks.Values
			.Where(t => t.Deadline.HasValue && t.Deadline.Value < now && 
			           (t.AgentStatus == TaskAgentStatus.Pending || t.AgentStatus == TaskAgentStatus.InProgress))
			.ToList();

		return Task.FromResult(Result<IEnumerable<AgentTask>>.WithSuccess(overdueTasks));
	}

	/// <summary>
	/// Gets tasks by type
	/// </summary>
	/// <param name="taskType">The task type</param>
	/// <param name="status">Optional agentStatus filter</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of tasks of the specified type</returns>
	public Task<Result<IEnumerable<AgentTask>>> GetByTypeAsync(
		string taskType, 
		TaskAgentStatus? status = null, 
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(taskType))
		{
			return Task.FromResult(Result<IEnumerable<AgentTask>>.WithFailure("Task type cannot be empty"));
		}

		var query = _tasks.Values.Where(t => t.TaskType.Equals(taskType, StringComparison.OrdinalIgnoreCase));

		if (status.HasValue)
		{
			query = query.Where(t => t.AgentStatus == status.Value);
		}

		var tasks = query.ToList();
		return Task.FromResult(Result<IEnumerable<AgentTask>>.WithSuccess(tasks));
	}

	/// <summary>
	/// Seeds the repository with sample tasks for development/testing
	/// </summary>
	/// <returns>The result of the seeding operation</returns>
	public Task<Result<bool>> SeedAsync()
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
				AgentStatus = TaskAgentStatus.Pending,
				CreatedAt = DateTime.UtcNow.AddHours(-2),
				Deadline = DateTime.UtcNow.AddHours(24)
			},
			new AgentTask
			{
				Id = Guid.NewGuid(),
				Title = "Analyze Value Patterns",
				Description = "Analyze incoming data for patterns and anomalies",
				TaskType = "analysis",
				Priority = TaskPriority.High,
				AgentStatus = TaskAgentStatus.Pending,
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
				AgentStatus = TaskAgentStatus.Pending,
				CreatedAt = DateTime.UtcNow.AddMinutes(-30),
				Deadline = DateTime.UtcNow.AddDays(3)
			}
		};

		foreach (var task in sampleTasks)
		{
			_tasks.TryAdd(task.Id, task);
		}

		return Task.FromResult(Result<bool>.WithSuccess(true));
	}
} 