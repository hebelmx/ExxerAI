using ExxerAI.Domain;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for managing agent tasks
/// </summary>
public interface ITaskRepository : IRepository<AgentTask>
{
	/// <summary>
	/// Gets tasks by status
	/// </summary>
	/// <param name="status">The task status</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of tasks with the specified status</returns>
	Task<Result<IEnumerable<AgentTask>>> GetByStatusAsync(
		TaskStatus status, 
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets tasks assigned to a specific agent
	/// </summary>
	/// <param name="agentId">The agent identifier</param>
	/// <param name="status">Optional status filter</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of tasks assigned to the agent</returns>
	Task<Result<IEnumerable<AgentTask>>> GetByAgentAsync(
		Guid agentId, 
		TaskStatus? status = null, 
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets overdue tasks
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of overdue tasks</returns>
	Task<Result<IEnumerable<AgentTask>>> GetOverdueTasksAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets tasks by type
	/// </summary>
	/// <param name="taskType">The task type</param>
	/// <param name="status">Optional status filter</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Collection of tasks of the specified type</returns>
	Task<Result<IEnumerable<AgentTask>>> GetByTypeAsync(
		string taskType, 
		TaskStatus? status = null, 
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Seeds the repository with sample tasks for development/testing
	/// </summary>
	/// <returns>The result of the seeding operation</returns>
	Task<Result> SeedAsync();
} 