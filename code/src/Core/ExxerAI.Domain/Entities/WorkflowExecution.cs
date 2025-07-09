namespace ExxerAI.Domain.Entities;

/// <summary>
/// Represents an execution instance of a workflow
/// </summary>
public class WorkflowExecution
{
    /// <summary>
    /// Gets or sets the unique identifier for the execution
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the workflow being executed
    /// </summary>
    public Guid WorkflowId { get; set; }

    /// <summary>
    /// Gets or sets the workflow being executed
    /// </summary>
    public Workflow? Workflow { get; set; }

    /// <summary>
    /// Gets or sets the execution agentStatus
    /// </summary>
    public WorkflowExecutionStatus Status { get; set; } = WorkflowExecutionStatus.Starting;

    /// <summary>
    /// Gets or sets when the execution started
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the execution completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the execution input data
    /// </summary>
    public Dictionary<string, object> Input { get; init; } = [];

    /// <summary>
    /// Gets or sets the execution output data
    /// </summary>
    public Dictionary<string, object> Output { get; init; } = [];

    /// <summary>
    /// Gets or sets the current step being executed
    /// </summary>
    public Guid? CurrentStepId { get; set; }

    /// <summary>
    /// Gets or sets error information if execution failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the collection of step executions
    /// </summary>
    public ICollection<StepExecution> StepExecutions { get; init; } = [];
}