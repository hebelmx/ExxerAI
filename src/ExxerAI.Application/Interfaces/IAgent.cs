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

    Task ExecuteAsync(string prompt, string agentType);

    Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken);

    Task<string> GetAgentStatusAsync();
}