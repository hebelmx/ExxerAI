namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// Represents the execution context for an agent operation
/// </summary>
public record AgentContext
{
    /// <summary>
    /// Gets the unique identifier for this execution context
    /// </summary>
    public string ContextId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the input prompt or command for the agent
    /// </summary>
    public string Input { get; init; } = string.Empty;

    /// <summary>
    /// Gets additional context data for the agent execution
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Gets the user identifier if applicable
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the session identifier for tracking related operations
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Gets the timestamp when this context was created
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
} 