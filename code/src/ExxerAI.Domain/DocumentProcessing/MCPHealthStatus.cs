namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents MCP server health agentStatus
/// </summary>
public class MCPHealthStatus
{
    /// <summary>
    /// Gets or sets whether the MCP server is healthy
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Gets or sets the server version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last health check timestamp
    /// </summary>
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Gets or sets any health check errors
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets additional health metrics
    /// </summary>
    public Dictionary<string, object> Metrics { get; set; } = new();
}