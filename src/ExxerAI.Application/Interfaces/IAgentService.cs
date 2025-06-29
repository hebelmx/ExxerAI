namespace ExxerAI.Application.Interfaces
{
    public interface IAgentService
    {
        Task ExecuteAgentTaskAsync(string prompt, string agentType);

        Task ExecuteTaskAsync(string prompt, string agentType);
    }
}