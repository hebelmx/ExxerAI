using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain;

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

/// <summary>
/// Represents the possible states of a workflow
/// </summary>
public enum WorkflowStatus
{
    /// <summary>
    /// Workflow is in draft state
    /// </summary>
    Draft,
    
    /// <summary>
    /// Workflow is active and can be executed
    /// </summary>
    Active,
    
    /// <summary>
    /// Workflow is currently executing
    /// </summary>
    Running,
    
    /// <summary>
    /// Workflow has completed execution
    /// </summary>
    Completed,
    
    /// <summary>
    /// Workflow execution failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// Workflow is paused
    /// </summary>
    Paused,
    
    /// <summary>
    /// Workflow is archived and cannot be executed
    /// </summary>
    Archived
}

/// <summary>
/// Represents the definition of a workflow
/// </summary>
public class WorkflowDefinition
{
    /// <summary>
    /// Gets or sets the workflow steps
    /// </summary>
    public ICollection<WorkflowStep> Steps { get; init; } = new List<WorkflowStep>();

    /// <summary>
    /// Gets or sets the workflow variables
    /// </summary>
    public Dictionary<string, object> Variables { get; init; } = new();

    /// <summary>
    /// Gets or sets the workflow configuration
    /// </summary>
    public WorkflowConfiguration Configuration { get; set; } = new();
}

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
    public Dictionary<string, object> Configuration { get; init; } = new();

    /// <summary>
    /// Gets or sets the conditions for step execution
    /// </summary>
    public StepConditions Conditions { get; set; } = new();

    /// <summary>
    /// Gets or sets the next steps to execute
    /// </summary>
    public ICollection<Guid> NextSteps { get; init; } = new List<Guid>();
}

/// <summary>
/// Represents conditions for workflow step execution
/// </summary>
public class StepConditions
{
    /// <summary>
    /// Gets or sets whether the step should be executed
    /// </summary>
    public string? ExecutionCondition { get; set; }

    /// <summary>
    /// Gets or sets the timeout for step execution in seconds
    /// </summary>
    public int? TimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets the maximum retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether to continue on failure
    /// </summary>
    public bool ContinueOnFailure { get; set; } = false;
}

/// <summary>
/// Represents configuration settings for a workflow
/// </summary>
public class WorkflowConfiguration
{
    /// <summary>
    /// Gets or sets the maximum execution time in seconds
    /// </summary>
    public int MaxExecutionTimeSeconds { get; set; } = 3600;

    /// <summary>
    /// Gets or sets whether parallel execution is allowed
    /// </summary>
    public bool AllowParallelExecution { get; set; } = true;

    /// <summary>
    /// Gets or sets the notification settings
    /// </summary>
    public NotificationSettings Notifications { get; set; } = new();

    /// <summary>
    /// Gets or sets custom configuration properties
    /// </summary>
    public Dictionary<string, object> CustomProperties { get; init; } = new();
}

/// <summary>
/// Represents notification settings for a workflow
/// </summary>
public class NotificationSettings
{
    /// <summary>
    /// Gets or sets whether to notify on completion
    /// </summary>
    public bool NotifyOnCompletion { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to notify on failure
    /// </summary>
    public bool NotifyOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the notification recipients
    /// </summary>
    public ICollection<string> Recipients { get; init; } = new List<string>();
}

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
    public Dictionary<string, object> Input { get; init; } = new();

    /// <summary>
    /// Gets or sets the execution output data
    /// </summary>
    public Dictionary<string, object> Output { get; init; } = new();

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
    public ICollection<StepExecution> StepExecutions { get; init; } = new List<StepExecution>();
}

/// <summary>
/// Represents the possible states of a workflow execution
/// </summary>
public enum WorkflowExecutionStatus
{
    /// <summary>
    /// Execution is starting
    /// </summary>
    Starting,
    
    /// <summary>
    /// Execution is running
    /// </summary>
    Running,
    
    /// <summary>
    /// Execution completed successfully
    /// </summary>
    Completed,
    
    /// <summary>
    /// Execution failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// Execution was cancelled
    /// </summary>
    Cancelled,
    
    /// <summary>
    /// Execution is paused
    /// </summary>
    Paused
}

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

/// <summary>
/// Represents the possible states of a step execution
/// </summary>
public enum StepExecutionStatus
{
    /// <summary>
    /// Step is pending execution
    /// </summary>
    Pending,
    
    /// <summary>
    /// Step is currently executing
    /// </summary>
    Running,
    
    /// <summary>
    /// Step completed successfully
    /// </summary>
    Completed,
    
    /// <summary>
    /// Step execution failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// Step was skipped
    /// </summary>
    Skipped,
    
    /// <summary>
    /// Step execution was cancelled
    /// </summary>
    Cancelled
} 