namespace ExxerAI.Domain;

/// <summary>
/// Simple result class for operation results
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public class Result<T>
{
    /// <summary>
    /// Gets whether the result is successful
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets whether the result is a failure
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the data if the result is successful
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Gets the error message if the result is a failure
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <param name="data">The data to return</param>
    /// <returns>A successful result</returns>
    public static Result<T> WithSuccess(T data)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    /// <summary>
    /// Creates a failure result
    /// </summary>
    /// <param name="error">The error message</param>
    /// <returns>A failure result</returns>
    public static Result<T> WithFailure(string error)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Error = error
        };
    }
} 