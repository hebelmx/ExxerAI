namespace ExxerAI.Domain.Enums;

/// <summary>
/// Represents task priority levels with rich domain modeling capabilities.
/// Replaces the primitive TaskPriority enum with enhanced functionality.
/// </summary>
public class TaskPriorityEnumModel : EnumModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPriorityEnumModel"/> class.
    /// </summary>
    public TaskPriorityEnumModel() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPriorityEnumModel"/> class with specified values.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    private TaskPriorityEnumModel(int value, string name, string displayName = "")
        : base(value, name, displayName)
    {
    }

    /// <summary>
    /// Gets the invalid task priority instance.
    /// </summary>
    public new static readonly TaskPriorityEnumModel Invalid = new(0, "Invalid", "Invalid Priority");

    /// <summary>
    /// Represents low priority tasks - typically maintenance or non-urgent work.
    /// </summary>
    public static readonly TaskPriorityEnumModel Low = new(1, "Low", "Low Priority");

    /// <summary>
    /// Represents normal priority tasks - standard operational work.
    /// </summary>
    public static readonly TaskPriorityEnumModel Normal = new(2, "Normal", "Normal Priority");

    /// <summary>
    /// Represents high priority tasks - important work requiring expedited handling.
    /// </summary>
    public static readonly TaskPriorityEnumModel High = new(3, "High", "High Priority");

    /// <summary>
    /// Represents critical priority tasks - urgent work requiring immediate attention.
    /// </summary>
    public static readonly TaskPriorityEnumModel Critical = new(4, "Critical", "Critical Priority");

    /// <summary>
    /// Determines if this priority level requires immediate escalation.
    /// </summary>
    public bool RequiresEscalation => Value >= Critical.Value;

    /// <summary>
    /// Determines if this priority level allows for standard processing delays.
    /// </summary>
    public bool AllowsDelay => Value <= Normal.Value;

    /// <summary>
    /// Gets the next higher priority level, if available.
    /// </summary>
    /// <returns>The next higher priority level, or the current level if already at maximum.</returns>
    public TaskPriorityEnumModel Escalate()
    {
        return Value switch
        {
            _ when Value >= Critical.Value => Critical,
            _ when Value >= High.Value => Critical,
            _ when Value >= Normal.Value => High,
            _ when Value >= Low.Value => Normal,
            _ => Low
        };
    }

    /// <summary>
    /// Gets the next lower priority level, if available.
    /// </summary>
    /// <returns>The next lower priority level, or the current level if already at minimum.</returns>
    public TaskPriorityEnumModel Deescalate()
    {
        return Value switch
        {
            _ when Value <= Low.Value => Low,
            _ when Value <= Normal.Value => Low,
            _ when Value <= High.Value => Normal,
            _ when Value <= Critical.Value => High,
            _ => Normal
        };
    }
}