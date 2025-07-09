namespace ExxerAI.Domain.Enums;

/// <summary>
/// Provides extension methods for converting enumeration values to strings.
/// </summary>
public static partial class EnumToString
{
    /// <summary>
    /// Converts an integer value to a FlowStatus enumeration.
    /// </summary>
    /// <param name="enumeration">The enumeration instance.</param>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The corresponding FlowStatus enumeration.</returns>
    public static FlowStatus ToEnum(this FlowStatus enumeration, int value)
    {
        return EnumModel.FromValue<FlowStatus>(value);
    }

    /// <summary>
    /// Converts an integer value to a FlowStatus enumeration.
    /// </summary>
    /// <param name="enumeration">The enumeration instance.</param>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The corresponding string Value from the enumeration.</returns>
    public static string ToString<TEnumModel>(this TEnumModel enumeration, int value) where TEnumModel : EnumModel, new()
    {
        return EnumModel.FromValue<TEnumModel>(value).ToString();
    }
}