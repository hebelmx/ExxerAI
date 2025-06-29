using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

public interface IAgentOrchestrator
{
    Task ExecuteAgentAsync(string prompt, string agentType);

    Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken);

    Task<string> GetAgentStatusAsync();
}