namespace ExxerAI.Domain;

/// <summary>
/// Represents the execution of a specific workflow step
/// </summary>
public class StepExecution
{
    /// <summary>
    /// Gets or sets the unique identifier for the step execution
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the workflow execution this belongs to
    /// </summary>
    public Guid WorkflowExecutionId { get; set; }

    /// <summary>
    /// Gets or sets the workflow execution this belongs to
    /// </summary>
    public WorkflowExecution? WorkflowExecution { get; set; }

    /// <summary>
    /// Gets or sets the step being executed
    /// </summary>
    public Guid StepId { get; set; }

    /// <summary>
    /// Gets or sets the execution agentStatus
    /// </summary>
    public StepExecutionStatus Status { get; set; } = StepExecutionStatus.Pending;

    /// <summary>
    /// Gets or sets when the step execution started
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the step execution completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the step input data
    /// </summary>
    public Dictionary<string, object> Input { get; init; } = new();

    /// <summary>
    /// Gets or sets the step output data
    /// </summary>
    public Dictionary<string, object> Output { get; init; } = new();

    /// <summary>
    /// Gets or sets error information if step failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the retry attempt count
    /// </summary>
    public int RetryCount { get; set; } = 0;
}