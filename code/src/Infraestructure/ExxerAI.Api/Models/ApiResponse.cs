namespace ExxerAI.Api.Models;

/// <summary>
/// Generic API response wrapper
/// </summary>
/// <typeparam name="T">The type of data in the response</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets error messages if the operation failed
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets a message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;
}