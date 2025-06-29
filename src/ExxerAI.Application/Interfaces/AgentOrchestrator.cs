using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces
{
    /// <summary>
    /// Interface for orchestrating AI agents
    /// </summary>
    /// <param name="agent"></param>
    public class AgentOrchestrator(IAgent agent) : IAgentOrchestrator
    {
        private readonly IAgent _agent = agent;

        /// <summary>
        /// Executes the specified agent with the given prompt and agent type.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="agentType"></param>
        /// <returns></returns>
        public Task ExecuteAgentAsync(string prompt, string agentType)
        {
            return _agent.ExecuteAsync(prompt, agentType);
        }

        public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAgentStatusAsync()
        {
            return _agent.GetAgentStatusAsync();
        }
    }
}