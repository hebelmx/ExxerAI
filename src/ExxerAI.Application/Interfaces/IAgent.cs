namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for AI agents with execution capabilities
/// </summary>
public interface IAgent
{
    string AgentId { get; }
    string AgentType { get; }
    Task<AgentResult> ExecuteAsync(AgentContext context);
    Task<bool> CanHandleAsync(AgentContext context);
}
