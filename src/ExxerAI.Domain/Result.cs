using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Serialization;

namespace ExxerAI.Domain;

/// <summary>
/// Represents the result of an operation, including success status and error messages.
/// </summary>
public class Result
{
    private Result(bool succeeded, IEnumerable<string> errors)
    {
        IsSuccess = succeeded;
        Errors = errors.ToArray();
    }

    public Result()
    {
        IsSuccess = false;
        Errors = Array.Empty<string>();
    }

    public override string ToString()
    {
        return IsSuccess ? "Success" : ListOfErrorsString();
    }

    private string ListOfErrorsString()
    {
        var stringBuilder = new StringBuilder("WithFailure: ");

        foreach (var error in Errors)
        {
            stringBuilder.Append(error);
            stringBuilder.Append(", ");
        }

        // Remove the trailing comma and space, if any
        if (stringBuilder.Length > 9)
        {
            stringBuilder.Length -= 2; // Remove the last ", "
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets a value indicating whether the result is a success.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets or sets the collection of error messages.
    /// </summary>
    public IEnumerable<string> Errors { get; set; }

    /// <summary>
    /// Gets the first error of the colection.
    /// </summary>
    public string? Error => Errors.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e));

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="Result"/> instance.</returns>
    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The collection of error messages.</param>
    /// <returns>A failed <see cref="Result"/> instance.</returns>
    public static Result WithFailure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed <see cref="Result"/> instance.</returns>
    public static Result WithFailure(string error)
    {
        return new Result(false, new List<string>() { error });
    }

    // Fluent API Methods
    public Result OnSuccess(Action action)
    {
        if (IsSuccess)
        {
            action();
        }
        return this;
    }

    public Result OnFailure(Action<IEnumerable<string>> action)
    {
        if (IsFailure)
        {
            action(Errors);
        }
        return this;
    }

    public Result<T> Map<T>(Func<T> func)
    {
        return IsSuccess ? Result<T>.Success(func()) : Result<T>.WithFailure(Errors);
    }

    public Result<T> Bind<T>(Func<Result<T>> func)
    {
        return IsSuccess ? func() : Result<T>.WithFailure(Errors);
    }

    public Result Ensure(Func<bool> condition, string errorMessage)
    {
        if (IsSuccess && !condition())
        {
            return WithFailure(errorMessage);
        }
        return this;
    }

    public Result Tap(Action action)
    {
        if (IsSuccess)
        {
            action();
        }
        return this;
    }

    public Result Combine(params Result[] results)
    {
        var allErrors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList();
        return allErrors.Any() ? WithFailure(allErrors) : Success();
    }

    public T Match<T>(Func<T> onSuccess, Func<IEnumerable<string>, T> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Errors);
    }

    public Result Recover(Func<Result> recoverFunc)
    {
        return IsFailure ? recoverFunc() : this;
    }

    public static Result CombineErrors(IEnumerable<string>? primaryErrors, IEnumerable<string>? secondaryErrors)
    {
        // Combine non-null lists, or return a failure message if both are null.
        var combinedErrors = (primaryErrors ?? [])
            .Concat(secondaryErrors ?? [])
            .ToList();

        return combinedErrors.Any()
            ? WithFailure(combinedErrors)
            : WithFailure("No Errors were Found");
    }
}

/// <summary>
/// Represents the result of an operation with a value, including success status and error messages.
/// </summary>
public class Result<T> //where T : class
{
    // Parameterless constructor for deserialization
    // Constructor with parameters for convenience and deserialization
    [JsonConstructor]
    public Result(bool isSuccess, IEnumerable<string>? errors, T? value = default)
    {
        _isSuccess = isSuccess;
        Errors = errors ?? ImmutableList<string>.Empty;
        _value = value;
    }

    public Result(bool isSuccess, List<string>? errors, T? value = default)
    {
        _isSuccess = isSuccess;
        Errors = errors;
        _value = value;
    }

    /// <summary>
    /// Gets the value associated with the result.
    /// </summary>
    public T? Value => _value;

    /// <summary>
    /// Gets the data associated with the result (alias for Value).
    /// </summary>
    public T? Data => _value;

    private readonly T? _value;
    private readonly bool _isSuccess;

    /// <summary>
    /// Gets a value indicating whether the result is a success.
    /// </summary>
    public bool IsSuccess
    {
        get
        {
            // Check if T is null
            if (_value == null) return false;

            // Check if errors are not null or empty
            if (Errors != null && Errors.Any()) return false;

            // Check if T implements IEnumerable and is empty
            if (_value is IEnumerable<object> enumerable)
            {
                // Cache the result to avoid double enumeration
                return enumerable.Any();
            }

            // Otherwise, return true since T is not null and errors are empty
            return _isSuccess;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the result has warnings.
    /// </summary>
    public bool HasWarnings => IsSuccess && Errors is not null && Errors.Any();

    /// <summary>
    /// Gets a value indicating whether the result is recoverable.
    /// </summary>
    public bool IsRecoverable => IsSuccess || HasWarnings;

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the collection of error messages.
    /// </summary>
    public IEnumerable<string>? Errors { get; init; }

    /// <summary>
    /// Gets the first error of the collection.
    /// </summary>
    public string? Error => Errors?.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e));

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="data">The value associated with the result.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance.</returns>
    public static Result<T> Success(T data)
    {
        return new Result<T>(true, default, data);
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="data">The value associated with the result.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance.</returns>
    public static Result<T> WithSuccess(T data)
    {
        return new Result<T>(true, default, data);
    }

    public override string ToString()
    {
        return _isSuccess ? $"Success: {Value!.ToString()}" : ListOfErrorsString() ?? "WithFailure";
    }

    private string? ListOfErrorsString()
    {
        if (Errors is null) return default;
        var stringBuilder = new StringBuilder("WithFailure: ");

        foreach (var error in Errors)
        {
            stringBuilder.Append(error);
            stringBuilder.Append(", ");
        }

        // Remove the trailing comma and space, if any
        if (stringBuilder.Length > 9)
        {
            stringBuilder.Length -= 2; // Remove the last ", "
        }

        return stringBuilder.ToString();
    }

    public static Result<T> WithFailure(IEnumerable<string>? errors, T? value = default)
    {
        // Use the provided errors or fall back to the default error message
        errors ??= ["WithFailure to execute Request"];
        return new Result<T>(false, errors, value);
    }

    //TODO [PERFORMANCE][CURSOR][20/JUNE/2025] - Possible double enumeration of Errors in ListOfErrorsString and HasWarnings. Consider materializing Errors with .ToList() if performance is a concern.
    //TODO [URGENT]
    //TODO [FIRST THING] 25-05-205
    //TODO // TODO: Enhance Result<T> to support severity levels.
    // - Introduce `WithWarnings` property: List<string> for non-fatal diagnostics.
    // - Preserve `Errors` for hard failures only.
    // - Add helper methods:
    //     - Result<T>.WithWarnings(IEnumerable<string> warnings)
    //     - Result<T>.HasWarnings => WithWarnings.Any()  <-- [DONE] jun 4 2025 ABR
    //     - Result<T>.IsRecoverable => IsSuccess || HasWarnings
    //
    // This enables accurate separation of recoverable fallbacks from true failures,
    // and cleaner assertions in tests.

    //

    public List<string> Warnings { get; private set; }

    public static Result<T> WithFailure(List<string> errors, T? value = default)
    {
        errors ??= ["WithFailure to execute Request"];
        return new Result<T>(false, errors, value);
    }

    public static Result<T> WithWarnings(List<string> errors, T? value = default)
    {
        errors ??= ["WithFailure to execute Request"];
        return new Result<T>(true, errors, value);
    }

    public static Result<T> WithFailure(string error, T? value = default)
    {
        return new Result<T>(false, [error], value);
    }

    // Implicit conversion from T to Result<T>
    public static implicit operator Result<T>(T data)
    {
        return Success(data);
    }

    // Implicit conversion from Result<T> to Result
    public static implicit operator Result(Result<T> result)
    {
        return result._isSuccess ? Result.Success() : Result.WithFailure(result.Errors);
    }

    // Deconstruct method to allow deconstruction
    public void Deconstruct(out bool succeeded, out T? data, out IEnumerable<string> errors)
    {
        succeeded = _isSuccess;
        data = Value;
        errors = Errors;
    }

    // Fluent API Methods
    public Result<T> OnSuccess(Action<T> action)
    {
        if (_isSuccess && Value is not null)
        {
            action(Value);
        }
        return this;
    }

    public Result<T> OnFailure(Action<IEnumerable<string>> action)
    {
        if (IsFailure)
        {
            action(Errors);
        }
        return this;
    }

    public Result<TOut> Map<TOut>(Func<T, TOut> func)
    {
        return _isSuccess && Value is not null ? Result<TOut>.Success(func(Value)) : Result<TOut>.WithFailure(Errors);
    }

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func)
    {
        return _isSuccess && Value is not null ? func(Value) : Result<TOut>.WithFailure(Errors);
    }

    public Result<T> Ensure(Func<T, bool> condition, string errorMessage)
    {
        if (_isSuccess && !condition(Value!))
        {
            return WithFailure(errorMessage);
        }
        return this;
    }

    public Result<T> Tap(Action<T> action)
    {
        if (_isSuccess && Value is not null)
        {
            action(Value);
        }
        return this;
    }

    public Result<T> Combine(params Result[] results)
    {
        var allErrors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList();
        return allErrors.Any() ? Result<T>.WithFailure(allErrors) : Result<T>.Success(this.Value!);
    }

    public Result<TOut> Match<TOut>(Func<T, TOut> onSuccess, Func<IEnumerable<string>, TOut> onFailure)
    {
        return _isSuccess ? onSuccess(Value!) : onFailure(Errors);
    }

    public Result<T> Recover(Func<Result<T>> recoverFunc)
    {
        return IsFailure ? recoverFunc() : this;
    }

    public Result<TOut> RecoverWith<TOut>(Func<Result<TOut>> recoverFunc)
    {
        return IsFailure ? recoverFunc() : Result<TOut>.Success((TOut)(object)Value!);
    }

    public static Result<TOut> CombineErrors<TOut>(IEnumerable<string>? primaryErrors, IEnumerable<string>? secondaryErrors, TOut? value = default)
    {
        // Combine non-null lists, or return a failure message if both are null.
        var combinedErrors = (primaryErrors ?? [])
            .Concat(secondaryErrors ?? [])
            .ToList();

        return combinedErrors.Any()
            ? Result<TOut>.WithFailure(combinedErrors, value)
            : Result<TOut>.WithFailure("No Errors were Found", value);
    }
}

/// <summary>
/// Provides extension methods for working with enumerables.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Checks if the enumerable is not null and contains at least one element.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The enumerable to check.</param>
    /// <returns>True if the enumerable is not null and contains at least one element; otherwise, false.</returns>
    public static bool NotNullOrEmpty<T>(this IEnumerable<T> source)
    {
        return source != null && source.Any();
    }
}