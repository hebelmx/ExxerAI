using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for AI agents with execution capabilities
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Gets the unique identifier for this agent
    /// </summary>
    string AgentId { get; }

    /// <summary>
    /// Gets the type or category of this agent
    /// </summary>
    string AgentType { get; }

    /// <summary>
    /// Executes the agent with the provided context
    /// </summary>
    /// <param name="context">The execution context</param>
    /// <returns>The execution result</returns>
    Task<AgentResult> ExecuteAsync(AgentContext context);

    /// <summary>
    /// Determines if this agent can handle the given context
    /// </summary>
    /// <param name="context">The execution context to evaluate</param>
    /// <returns>True if the agent can handle the context, false otherwise</returns>
    Task<bool> CanHandleAsync(AgentContext context);

    /// <summary>
    /// Executes the agent with simple string parameters (legacy method)
    /// </summary>
    /// <param name="prompt">The input prompt or request</param>
    /// <param name="agentType">The type of agent to use</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ExecuteAsync(string prompt, string agentType);

    /// <summary>
    /// Executes the agent with the provided context and cancellation token
    /// </summary>
    /// <param name="context">The execution context</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation</param>
    /// <returns>The execution result</returns>
    Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current status and health information of this agent
    /// </summary>
    /// <returns>A string description of the agent's current status</returns>
    Task<string> GetAgentStatusAsync();
}