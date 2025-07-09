namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents MCP watch options for monitoring Google Drive folders
/// </summary>
public class MCPWatchOptions
{
    /// <summary>
    /// Gets or sets whether to include subdirectories in monitoring
    /// </summary>
    public bool IncludeSubdirectories { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to automatically process new documents
    /// </summary>
    public bool AutoProcess { get; set; } = false;

    /// <summary>
    /// Gets or sets the polling interval in seconds
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Gets the collection of file types to monitor
    /// </summary>
    public List<string> FileTypes { get; set; } = [".pdf", ".docx", ".xlsx"];

    /// <summary>
    /// Gets or sets additional watch parameters
    /// </summary>
    public Dictionary<string, object> CustomParameters { get; set; } = [];
}