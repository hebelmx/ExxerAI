namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents an MCP response with operation results
/// </summary>
public class MCPResponse
{
    /// <summary>
    /// Gets or sets the response identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets additional response data
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
}