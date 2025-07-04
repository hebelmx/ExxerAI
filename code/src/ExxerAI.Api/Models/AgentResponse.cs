using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Api.Models;

/// <summary>
/// Response model for agent operations
/// </summary>
public class AgentResponse
{
    /// <summary>
    /// Gets or sets the agent identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the agent name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agent description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agent agentStatus
    /// </summary>
    public AgentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the agent capabilities
    /// </summary>
    public AgentCapabilitiesDto Capabilities { get; set; } = new();

    /// <summary>
    /// Gets or sets the agent configuration
    /// </summary>
    public AgentConfigurationDto Configuration { get; set; } = new();

    /// <summary>
    /// Gets or sets when the agent was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the agent was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}