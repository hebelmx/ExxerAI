using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain;

/// <summary>
/// Represents an intelligent agent in the ExxerAI system
/// </summary>
public class Agent
{
    /// <summary>
    /// Gets or sets the unique identifier for the agent
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the agent's name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agent's description
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agent's current agentStatus
    /// </summary>
    public AgentStatus Status { get; set; } = AgentStatus.Inactive;

    /// <summary>
    /// Gets or sets the agent's capabilities
    /// </summary>
    public AgentCapabilities Capabilities { get; set; } = new();

    /// <summary>
    /// Gets or sets when the agent was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the agent was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the agent's configuration
    /// </summary>
    public AgentConfiguration Configuration { get; set; } = new();

    /// <summary>
    /// Gets the collection of tasks assigned to this agent
    /// </summary>
    public ICollection<AgentTask> Tasks { get; init; } = new List<AgentTask>();
}