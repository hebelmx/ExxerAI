using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain;

/// <summary>
/// Represents a task that can be executed by an agent
/// </summary>
public class AgentTask
{
    /// <summary>
    /// Gets or sets the unique identifier for the task
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the task title
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the task
    /// </summary>
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task type identifier
    /// </summary>
    [Required]
    [StringLength(50)]
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the task
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    /// <summary>
    /// Gets or sets the task priority
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;

    /// <summary>
    /// Gets or sets the agent assigned to this task
    /// </summary>
    public Guid? AssignedAgentId { get; set; }

    /// <summary>
    /// Gets or sets the agent assigned to this task
    /// </summary>
    public Agent? AssignedAgent { get; set; }

    /// <summary>
    /// Gets or sets the task input data
    /// </summary>
    public TaskData Input { get; set; } = new();

    /// <summary>
    /// Gets or sets the task output data
    /// </summary>
    public TaskData Output { get; set; } = new();

    /// <summary>
    /// Gets or sets when the task was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the task was started
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the task was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the deadline for task completion
    /// </summary>
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// Gets or sets error information if the task failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets task execution metadata
    /// </summary>
    public TaskMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets the task execution duration
    /// </summary>
    public TimeSpan? ExecutionDuration =>
        StartedAt.HasValue && CompletedAt.HasValue
            ? CompletedAt.Value - StartedAt.Value
            : null;

    /// <summary>
    /// Gets whether the task is overdue
    /// </summary>
    public bool IsOverdue =>
        Deadline.HasValue && DateTime.UtcNow > Deadline.Value && Status != TaskStatus.Completed;
}

/// <summary>
/// Represents the possible states of a task
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task is created but not yet started
    /// </summary>
    Pending,
    
    /// <summary>
    /// Task is being processed by an agent
    /// </summary>
    InProgress,
    
    /// <summary>
    /// Task has been completed successfully
    /// </summary>
    Completed,
    
    /// <summary>
    /// Task failed during execution
    /// </summary>
    Failed,
    
    /// <summary>
    /// Task was cancelled before completion
    /// </summary>
    Cancelled,
    
    /// <summary>
    /// Task is paused and waiting to be resumed
    /// </summary>
    Paused
}

/// <summary>
/// Represents the priority levels for tasks
/// </summary>
public enum TaskPriority
{
    /// <summary>
    /// Low priority task
    /// </summary>
    Low = 1,
    
    /// <summary>
    /// Normal priority task
    /// </summary>
    Normal = 2,
    
    /// <summary>
    /// High priority task
    /// </summary>
    High = 3,
    
    /// <summary>
    /// Critical priority task
    /// </summary>
    Critical = 4
}

/// <summary>
/// Represents data associated with a task
/// </summary>
public class TaskData
{
    /// <summary>
    /// Gets or sets the content type of the data
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Gets or sets the raw data content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional properties for the data
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
}

/// <summary>
/// Represents metadata for task execution
/// </summary>
public class TaskMetadata
{
    /// <summary>
    /// Gets or sets custom metadata properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();

    /// <summary>
    /// Gets or sets execution context information
    /// </summary>
    public Dictionary<string, string> Context { get; init; } = new();

    /// <summary>
    /// Gets or sets performance metrics
    /// </summary>
    public Dictionary<string, double> Metrics { get; init; } = new();
} 