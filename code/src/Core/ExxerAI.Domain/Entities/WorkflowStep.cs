namespace ExxerAI.Domain.Entities;

/// <summary>
/// Represents a step in a workflow
/// </summary>
public class WorkflowStep
{
    /// <summary>
    /// Gets or sets the unique identifier for the step
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the step name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the step type
    /// </summary>
    [Required]
    [StringLength(50)]
    public string StepType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order of execution
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the step configuration
    /// </summary>
    public Dictionary<string, object> Configuration { get; init; } = [];

    /// <summary>
    /// Gets or sets the conditions for step execution
    /// </summary>
    public StepConditions Conditions { get; set; } = new();

    /// <summary>
    /// Gets or sets the next steps to execute
    /// </summary>
    public ICollection<Guid> NextSteps { get; init; } = [];
}