using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain.Entities;

/// <summary>
/// Represents a workflow that orchestrates multiple agents and tasks
/// </summary>
public class Workflow
{
    /// <summary>
    /// Gets or sets the unique identifier for the workflow
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the workflow name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workflow description
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current agentStatus of the workflow
    /// </summary>
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;

    /// <summary>
    /// Gets or sets the workflow definition
    /// </summary>
    public WorkflowDefinition Definition { get; set; } = new();

    /// <summary>
    /// Gets or sets when the workflow was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the workflow was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the workflow was started
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the workflow was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets the collection of workflow executions
    /// </summary>
    public ICollection<WorkflowExecution> Executions { get; init; } = new List<WorkflowExecution>();
}