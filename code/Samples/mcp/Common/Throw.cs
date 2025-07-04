using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ModelContextProtocol;

/// <summary>
/// Provides utility methods for throwing exceptions with standardized argument validation.
/// </summary>
internal static class Throw
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the specified argument is null.
    /// </summary>
    /// <param name="argument">The argument to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is null.</exception>
    public static void IfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            ThrowArgumentNullException(paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the specified string argument is null or empty.
    /// </summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is null or empty.</exception>
    public static void IfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (string.IsNullOrEmpty(argument))
        {
            ThrowArgumentNullException(paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the specified string argument is null, empty, or whitespace.
    /// </summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is null, empty, or whitespace.</exception>
    public static void IfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            ThrowArgumentNullException(paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The error message.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="condition"/> is true.</exception>
    public static void IfTrue(bool condition, string message, [CallerArgumentExpression(nameof(condition))] string? paramName = null)
    {
        if (condition)
        {
            throw new ArgumentException(message, paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the specified condition is false.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The error message.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="condition"/> is false.</exception>
    public static void IfFalse(bool condition, string message, [CallerArgumentExpression(nameof(condition))] string? paramName = null)
    {
        if (!condition)
        {
            throw new ArgumentException(message, paramName);
        }
    }

    [DoesNotReturn]
    private static void ThrowArgumentNullException(string? paramName)
    {
        throw new ArgumentNullException(paramName);
    }
}