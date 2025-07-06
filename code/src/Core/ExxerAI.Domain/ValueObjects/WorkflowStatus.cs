namespace ExxerAI.Domain.ValueObjects;

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