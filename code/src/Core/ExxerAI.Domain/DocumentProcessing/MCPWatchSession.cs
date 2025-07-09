namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents an active MCP watch session
/// </summary>
public class MCPWatchSession
{
    /// <summary>
    /// Gets or sets the watch session ID
    /// </summary>
    public string WatchId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder ID being watched
    /// </summary>
    public string FolderId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the watch was started
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the watch options
    /// </summary>
    public MCPWatchOptions Options { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the watch is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}