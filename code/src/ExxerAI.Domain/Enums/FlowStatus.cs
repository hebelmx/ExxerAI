namespace ExxerAI.Domain.Enums;

/// <summary>
/// Represents the status of a workflow or process flow in the system.
/// </summary>
public class FlowStatus : EnumModel
{
    /// <summary>
    /// Gets the 'None' flow status.
    /// </summary>
    public static readonly FlowStatus None
        = new(0, "None");

    /// <summary>
    /// Gets the 'Created' flow status.
    /// </summary>
    public static readonly FlowStatus Created
        = new(1, "Created");

    /// <summary>
    /// Gets the 'InProcess' flow status.
    /// </summary>
    public static readonly FlowStatus InProcess
        = new(2, "InProcess");

    /// <summary>
    /// Gets the 'Finished' flow status.
    /// </summary>
    public static readonly FlowStatus Finished
        = new(4, "Finished");

    /// <summary>
    /// Gets the 'Invalid' flow status.
    /// </summary>
    public new static readonly FlowStatus Invalid
        = new(8, "Invalid");

    /// <summary>
    /// Gets the 'Restored' flow status.
    /// </summary>
    public static readonly FlowStatus Restored
        = new(16, "Restored");

    /// <summary>
    /// Gets the 'Rejected' flow status.
    /// </summary>
    public static readonly FlowStatus Rejected
        = new(32, "Rejected");

    public static implicit operator int(FlowStatus enumerator) => enumerator.Value;

    public static implicit operator string(FlowStatus enumerator) => enumerator.Value.ToString();

    public static implicit operator FlowStatus(int value) => FromValue<FlowStatus>(value);

    public FlowStatus()
    { }

    private FlowStatus(int value, string name, string displayName = "") : base(value, name, displayName)
    {
    }

    /// <summary>
    /// Retrieves a <see cref="FlowStatus"/> instance from an integer value.
    /// </summary>
    /// <param name="value">The integer value representing the status.</param>
    /// <returns>A <see cref="FlowStatus"/> instance corresponding to the specified value.</returns>
    public static FlowStatus FromValue(int value) => FromValue<FlowStatus>(value);
}