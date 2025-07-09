namespace ExxerAI.Domain.Enums;

/// <summary>
/// Represents task agent status with rich domain modeling and state transition validation.
/// Replaces the primitive TaskAgentStatus enum with enhanced functionality.
/// </summary>
public class TaskAgentStatusEnumModel : EnumModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskAgentStatusEnumModel"/> class.
    /// </summary>
    public TaskAgentStatusEnumModel() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskAgentStatusEnumModel"/> class with specified values.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    private TaskAgentStatusEnumModel(int value, string name, string displayName = "")
        : base(value, name, displayName)
    {
    }

    /// <summary>
    /// Gets the invalid task agent status instance.
    /// </summary>
    public new static readonly TaskAgentStatusEnumModel Invalid = new(0, "Invalid", "Invalid Status");

    /// <summary>
    /// Task is pending assignment or processing.
    /// </summary>
    public static readonly TaskAgentStatusEnumModel Pending = new(1, "Pending", "Pending Assignment");

    /// <summary>
    /// Task is actively being processed by an agent.
    /// </summary>
    public static readonly TaskAgentStatusEnumModel InProgress = new(2, "InProgress", "In Progress");

    /// <summary>
    /// Task has been temporarily paused but can be resumed.
    /// </summary>
    public static readonly TaskAgentStatusEnumModel Paused = new(3, "Paused", "Temporarily Paused");

    /// <summary>
    /// Task has been successfully completed.
    /// </summary>
    public static readonly TaskAgentStatusEnumModel Completed = new(4, "Completed", "Successfully Completed");

    /// <summary>
    /// Task has failed and requires intervention.
    /// </summary>
    public static readonly TaskAgentStatusEnumModel Failed = new(5, "Failed", "Failed with Errors");

    /// <summary>
    /// Task has been cancelled before completion.
    /// </summary>
    public static readonly TaskAgentStatusEnumModel Cancelled = new(6, "Cancelled", "Cancelled by User");

    /// <summary>
    /// Determines if the task is in an active processing state.
    /// </summary>
    public bool IsActive => this.Equals(InProgress) || this.Equals(Paused);

    /// <summary>
    /// Determines if the task is in a terminal state (completed, failed, or cancelled).
    /// </summary>
    public bool IsTerminal => this.Equals(Completed) || this.Equals(Failed) || this.Equals(Cancelled);

    /// <summary>
    /// Determines if the task can be started from its current state.
    /// </summary>
    public bool CanStart => this.Equals(Pending);

    /// <summary>
    /// Determines if the task can be paused from its current state.
    /// </summary>
    public bool CanPause => this.Equals(InProgress);

    /// <summary>
    /// Determines if the task can be resumed from its current state.
    /// </summary>
    public bool CanResume => this.Equals(Paused);

    /// <summary>
    /// Determines if the task can be cancelled from its current state.
    /// </summary>
    public bool CanCancel => !IsTerminal;

    /// <summary>
    /// Determines if the task is overdue (not completed and has a deadline).
    /// </summary>
    /// <param name="deadline">The task deadline.</param>
    /// <returns>True if the task is overdue, false otherwise.</returns>
    public bool IsOverdue(DateTime? deadline)
    {
        return deadline.HasValue &&
               deadline.Value < DateTime.UtcNow &&
               !this.Equals(Completed);
    }

    /// <summary>
    /// Validates if a transition to the specified status is allowed.
    /// </summary>
    /// <param name="newStatus">The target status.</param>
    /// <returns>True if the transition is valid, false otherwise.</returns>
    public bool CanTransitionTo(TaskAgentStatusEnumModel newStatus)
    {
        if (newStatus == null) return false;
        if (this.Equals(newStatus)) return true; // Same status is always valid

        return this.Value switch
        {
            _ when this.Equals(Pending) => newStatus.Equals(InProgress) || newStatus.Equals(Cancelled),
            _ when this.Equals(InProgress) => newStatus.Equals(Paused) || newStatus.Equals(Completed) ||
                                             newStatus.Equals(Failed) || newStatus.Equals(Cancelled),
            _ when this.Equals(Paused) => newStatus.Equals(InProgress) || newStatus.Equals(Cancelled),
            _ when IsTerminal => false, // Terminal states cannot transition
            _ => false
        };
    }

    /// <summary>
    /// Gets the next logical status in normal workflow progression.
    /// </summary>
    /// <returns>The next status, or the current status if no progression is available.</returns>
    public TaskAgentStatusEnumModel GetNextStatus()
    {
        return this.Value switch
        {
            _ when this.Equals(Pending) => InProgress,
            _ when this.Equals(InProgress) => Completed,
            _ when this.Equals(Paused) => InProgress,
            _ => this // Terminal or invalid states stay the same
        };
    }
}