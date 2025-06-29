namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// Represents the result of an agent execution operation
/// </summary>
public record AgentResult
{
    /// <summary>
    /// Gets a value indicating whether the execution was successful
    /// </summary>
    public bool IsSuccessful { get; init; }

    /// <summary>
    /// Gets the output or response from the agent
    /// </summary>
    public string Output { get; init; } = string.Empty;

    /// <summary>
    /// Gets the error message if execution failed
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// Gets additional metadata about the execution
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Gets the execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; init; }

    /// <summary>
    /// Gets the timestamp when this result was created
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a value indicating whether the execution failed
    /// </summary>
    public bool IsFailure => !IsSuccessful;

    /// <summary>
    /// Creates a successful result with the specified output
    /// </summary>
    /// <param name="output">The agent output</param>
    /// <param name="executionTimeMs">The execution time in milliseconds</param>
    /// <returns>A successful agent result</returns>
    public static AgentResult CreateSuccess(string output, long executionTimeMs = 0) =>
        new()
        {
            IsSuccessful = true,
            Output = output,
            ExecutionTimeMs = executionTimeMs
        };

    /// <summary>
    /// Creates a failed result with the specified error message
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <param name="executionTimeMs">The execution time in milliseconds</param>
    /// <returns>A failed agent result</returns>
    public static AgentResult CreateFailure(string errorMessage, long executionTimeMs = 0) =>
        new()
        {
            IsSuccessful = false,
            ErrorMessage = errorMessage,
            ExecutionTimeMs = executionTimeMs
        };

    /// <summary>
    /// Creates a failed result with exception details
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="executionTimeMs">The execution time in milliseconds</param>
    /// <returns>A failed agent result</returns>
    public static AgentResult CreateFailure(Exception exception, long executionTimeMs = 0) =>
        new()
        {
            IsSuccessful = false,
            ErrorMessage = exception.Message,
            ExecutionTimeMs = executionTimeMs,
            Metadata = new Dictionary<string, object>
            {
                ["ExceptionType"] = exception.GetType().Name,
                ["StackTrace"] = exception.StackTrace ?? string.Empty
            }
        };
} 