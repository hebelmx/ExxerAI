using ExxerAI.Application;
using ExxerAI.Domain;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Generic repository interface for data access operations
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity if found</returns>
    Task<Result<T>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all entities</returns>
    Task<Result<IEnumerable<T>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added entity</returns>
    Task<Result<T>> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated entity</returns>
    Task<Result<T>> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the entity exists, false otherwise</returns>
    Task<Result<bool>> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for agent entities
/// </summary>
public interface IAgentRepository : IRepository<Domain.Agent>
{
    /// <summary>
    /// Gets agents by status
    /// </summary>
    /// <param name="status">The agent status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of agents with the specified status</returns>
    Task<Result<IEnumerable<Domain.Agent>>> GetByStatusAsync(
        Domain.AgentStatus status, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds agents that support a specific task type
    /// </summary>
    /// <param name="taskType">The task type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of agents that support the task type</returns>
    Task<Result<IEnumerable<Domain.Agent>>> FindByTaskTypeAsync(
        string taskType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets agents with their current task count
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of agents with task count information</returns>
    Task<Result<IEnumerable<(Domain.Agent Agent, int TaskCount)>>> GetAgentsWithTaskCountAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for task entities
/// </summary>
public interface ITaskRepository : IRepository<Domain.AgentTask>
{
    /// <summary>
    /// Gets tasks by status
    /// </summary>
    /// <param name="status">The task status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tasks with the specified status</returns>
    Task<Result<IEnumerable<Domain.AgentTask>>> GetByStatusAsync(
        Domain.TaskStatus status, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks assigned to a specific agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tasks assigned to the agent</returns>
    Task<Result<IEnumerable<Domain.AgentTask>>> GetByAgentAsync(
        Guid agentId, 
        Domain.TaskStatus? status = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets overdue tasks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of overdue tasks</returns>
    Task<Result<IEnumerable<Domain.AgentTask>>> GetOverdueTasksAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks by type
    /// </summary>
    /// <param name="taskType">The task type</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of tasks of the specified type</returns>
    Task<Result<IEnumerable<Domain.AgentTask>>> GetByTypeAsync(
        string taskType, 
        Domain.TaskStatus? status = null, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for workflow entities
/// </summary>
public interface IWorkflowRepository : IRepository<Domain.Workflow>
{
    /// <summary>
    /// Gets workflows by status
    /// </summary>
    /// <param name="status">The workflow status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of workflows with the specified status</returns>
    Task<Result<IEnumerable<Domain.Workflow>>> GetByStatusAsync(
        Domain.WorkflowStatus status, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets workflow executions for a specific workflow
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="status">Optional execution status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of workflow executions</returns>
    Task<Result<IEnumerable<Domain.WorkflowExecution>>> GetExecutionsAsync(
        Guid workflowId, 
        Domain.WorkflowExecutionStatus? status = null, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for language model entities
/// </summary>
public interface ILanguageModelRepository : IRepository<Domain.LanguageModel>
{
    /// <summary>
    /// Gets available language models
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of available language models</returns>
    Task<Result<IEnumerable<Domain.LanguageModel>>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets language models by provider
    /// </summary>
    /// <param name="provider">The provider name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of language models from the specified provider</returns>
    Task<Result<IEnumerable<Domain.LanguageModel>>> GetByProviderAsync(
        string provider, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the best model for a specific capability
    /// </summary>
    /// <param name="capability">The required capability</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The best model for the capability if found</returns>
    Task<Result<Domain.LanguageModel>> FindBestModelForCapabilityAsync(
        string capability, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for conversation entities
/// </summary>
public interface IConversationRepository : IRepository<Domain.Conversation>
{
    /// <summary>
    /// Gets conversations for a specific agent
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of conversations for the agent</returns>
    Task<Result<IEnumerable<Domain.Conversation>>> GetByAgentAsync(
        Guid agentId, 
        Domain.ConversationStatus? status = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets conversations using a specific language model
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of conversations using the model</returns>
    Task<Result<IEnumerable<Domain.Conversation>>> GetByLanguageModelAsync(
        Guid modelId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets messages for a specific conversation
    /// </summary>
    /// <param name="conversationId">The conversation identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of messages in the conversation</returns>
    Task<Result<IEnumerable<Domain.ConversationMessage>>> GetMessagesAsync(
        Guid conversationId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a message to a conversation
    /// </summary>
    /// <param name="message">The message to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added message</returns>
    Task<Result<Domain.ConversationMessage>> AddMessageAsync(
        Domain.ConversationMessage message, 
        CancellationToken cancellationToken = default);
} 