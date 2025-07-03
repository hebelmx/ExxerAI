namespace ExxerAI.Domain.Tests;

/// <summary>
/// Utility class for handling enum conversions in tests
/// </summary>
public static class EnumModelHelper
{
    /// <summary>
    /// Converts a string name to an enum value
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="name">The enum name</param>
    /// <returns>The enum value</returns>
    public static T FromName<T>(string name) where T : struct, Enum
    {
        return Enum.Parse<T>(name);
    }
}