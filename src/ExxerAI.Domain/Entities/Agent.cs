namespace ExxerAI.Domain.Entities;

/// <summary>
/// Represents an AI agent with specific capabilities and state
/// </summary>
public class Agent
{
    public string AgentId { get; private set; }
    public string AgentType { get; private set; }
    public Dictionary<string, object> Capabilities { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public Agent(string agentType, Dictionary<string, object> capabilities)
    {
        AgentId = Guid.NewGuid().ToString();
        AgentType = agentType;
        Capabilities = capabilities;
        CreatedAt = DateTime.UtcNow;
    }
}
