namespace ExxerAI.Domain;

/// <summary>
/// Represents the possible states of a task
/// </summary>
public enum TaskAgentStatus
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