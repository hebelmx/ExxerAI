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
    /// Gets or sets the agent's current status
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

/// <summary>
/// Represents the capabilities of an agent
/// </summary>
public class AgentCapabilities
{
    /// <summary>
    /// Gets or sets whether the agent can process natural language
    /// </summary>
    public bool CanProcessNaturalLanguage { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the agent can generate code
    /// </summary>
    public bool CanGenerateCode { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the agent can analyze data
    /// </summary>
    public bool CanAnalyzeData { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the agent can interact with external APIs
    /// </summary>
    public bool CanCallExternalAPIs { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum concurrent tasks the agent can handle
    /// </summary>
    public int MaxConcurrentTasks { get; set; } = 1;

    /// <summary>
    /// Gets or sets the supported task types for this agent
    /// </summary>
    public ICollection<string> SupportedTaskTypes { get; init; } = new List<string>();
}

/// <summary>
/// Represents the configuration settings for an agent
/// </summary>
public class AgentConfiguration
{
    /// <summary>
    /// Gets or sets the timeout for task execution in seconds
    /// </summary>
    public int TaskTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the maximum retries for failed tasks
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the agent's priority level
    /// </summary>
    public int Priority { get; set; } = 1;

    /// <summary>
    /// Gets or sets custom configuration properties
    /// </summary>
    public Dictionary<string, object> CustomProperties { get; init; } = new();
}
