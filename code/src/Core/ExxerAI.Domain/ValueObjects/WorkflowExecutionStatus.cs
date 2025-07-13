namespace ExxerAI.Domain.ValueObjects;

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
    Paused,
    
    /// <summary>
    /// Execution is scheduled for future execution
    /// </summary>
    Scheduled
}