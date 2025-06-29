namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// Represents the result of an agent execution operation
/// </summary>
public record AgentResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the output or response from the agent
    /// </summary>
    public string Output { get; init; } = string.Empty;

    /// <summary>
    /// Gets any error messages if the operation failed
    /// </summary>
    public string[] Errors { get; init; } = Array.Empty<string>();

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
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful result with the specified output
    /// </summary>
    /// <param name="output">The agent output</param>
    /// <param name="executionTimeMs">The execution time in milliseconds</param>
    /// <returns>A successful agent result</returns>
    public static AgentResult Success(string output, long executionTimeMs = 0) =>
        new() { Success = true, Output = output, ExecutionTimeMs = executionTimeMs };

    /// <summary>
    /// Creates a failed result with the specified errors
    /// </summary>
    /// <param name="errors">The error messages</param>
    /// <returns>A failed agent result</returns>
    public static AgentResult Failure(params string[] errors) =>
        new() { Success = false, Errors = errors };
} 