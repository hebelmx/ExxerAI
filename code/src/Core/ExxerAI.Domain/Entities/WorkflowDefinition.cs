namespace ExxerAI.Domain.Entities;

/// <summary>
/// Represents the definition of a workflow
/// </summary>
public class WorkflowDefinition
{
    /// <summary>
    /// Gets or sets the workflow steps
    /// </summary>
    public ICollection<WorkflowStep> Steps { get; init; } = [];

    /// <summary>
    /// Gets or sets the workflow variables
    /// </summary>
    public Dictionary<string, object> Variables { get; init; } = [];

    /// <summary>
    /// Gets or sets the workflow configuration
    /// </summary>
    public WorkflowConfiguration Configuration { get; set; } = new();
}