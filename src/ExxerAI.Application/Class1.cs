namespace ExxerAI.Application;

/// <summary>
/// Represents the result of an operation, including success status and error messages
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the Result class
    /// </summary>
    /// <param name="isSuccess">Whether the operation was successful</param>
    /// <param name="errors">Collection of error messages</param>
    protected Result(bool isSuccess, IEnumerable<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors?.ToList() ?? new List<string>();
    }

    /// <summary>
    /// Gets a value indicating whether the result is successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is a failure
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the collection of error messages
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <returns>A successful <see cref="Result"/> instance</returns>
    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed result with a single error message
    /// </summary>
    /// <param name="error">The error message</param>
    /// <returns>A failed <see cref="Result"/> instance</returns>
    public static Result WithFailure(string error)
    {
        return new Result(false, new[] { error });
    }

    /// <summary>
    /// Creates a failed result with the specified errors
    /// </summary>
    /// <param name="errors">The collection of error messages</param>
    /// <returns>A failed <see cref="Result"/> instance</returns>
    public static Result WithFailure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }
}

/// <summary>
/// Represents the result of an operation that returns a value
/// </summary>
/// <typeparam name="T">The type of the result value</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Initializes a new instance of the Result class with a value
    /// </summary>
    /// <param name="isSuccess">Whether the operation was successful</param>
    /// <param name="value">The result value</param>
    /// <param name="errors">Collection of error messages</param>
    private Result(bool isSuccess, T? value, IEnumerable<string> errors) : base(isSuccess, errors)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the result value
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Creates a successful result with a value
    /// </summary>
    /// <param name="value">The result value</param>
    /// <returns>A successful <see cref="Result{T}"/> instance</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed result with a single error message
    /// </summary>
    /// <param name="error">The error message</param>
    /// <returns>A failed <see cref="Result{T}"/> instance</returns>
    public static new Result<T> WithFailure(string error)
    {
        return new Result<T>(false, default, new[] { error });
    }

    /// <summary>
    /// Creates a failed result with the specified errors
    /// </summary>
    /// <param name="errors">The collection of error messages</param>
    /// <returns>A failed <see cref="Result{T}"/> instance</returns>
    public static new Result<T> WithFailure(IEnumerable<string> errors)
    {
        return new Result<T>(false, default, errors);
    }

    /// <summary>
    /// Implicitly converts a value to a successful result
    /// </summary>
    /// <param name="value">The value to convert</param>
    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }
}
