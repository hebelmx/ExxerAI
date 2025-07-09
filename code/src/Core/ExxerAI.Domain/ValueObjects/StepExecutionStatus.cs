namespace ExxerAI.Domain.ValueObjects;

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