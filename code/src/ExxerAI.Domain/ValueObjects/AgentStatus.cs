namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// Represents the possible states of an agent
/// </summary>
public enum AgentStatus
{
    /// <summary>
    /// Agent is inactive and not processing tasks
    /// </summary>
    Inactive,
    
    /// <summary>
    /// Agent is active and ready to process tasks
    /// </summary>
    Active,
    
    /// <summary>
    /// Agent is currently busy processing a task
    /// </summary>
    Busy,
    
    /// <summary>
    /// Agent encountered an error and needs attention
    /// </summary>
    Error,
    
    /// <summary>
    /// Agent is paused and not accepting new tasks
    /// </summary>
    Paused
}