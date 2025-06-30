namespace ExxerAI.Domain.Entities;

/// <summary>
/// Represents an AI agent with specific capabilities and state
/// </summary>
public class Agent
{
    /// <summary>
    /// Gets the unique identifier for this agent
    /// </summary>
    public string AgentId { get; private set; }
    
    /// <summary>
    /// Gets the type or category of this agent (e.g., Analysis, Writing, Research)
    /// </summary>
    public string AgentType { get; private set; }
    
    /// <summary>
    /// Gets the capabilities and configuration options for this agent
    /// </summary>
    public Dictionary<string, object> Capabilities { get; private set; }
    
    /// <summary>
    /// Gets the timestamp when this agent was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the Agent class
    /// </summary>
    /// <param name="agentType">The type or category of the agent</param>
    /// <param name="capabilities">The agent's capabilities and configuration</param>
    public Agent(string agentType, Dictionary<string, object> capabilities)
    {
        AgentId = Guid.NewGuid().ToString();
        AgentType = agentType;
        Capabilities = capabilities;
        CreatedAt = DateTime.UtcNow;
    }
}
