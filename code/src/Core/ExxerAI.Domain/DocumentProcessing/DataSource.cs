namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents information about the source of data
/// </summary>
public class DataSource
{
    /// <summary>
    /// Gets or sets the type of the data source
    /// </summary>
    [StringLength(100)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the source
    /// </summary>
    [StringLength(255)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path or location of the source
    /// </summary>
    [StringLength(1000)]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets who or what processed this data
    /// </summary>
    [StringLength(255)]
    public string ProcessedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MCP session ID if applicable
    /// </summary>
    [StringLength(255)]
    public string? MCPSessionId { get; set; }

    /// <summary>
    /// Gets or sets additional source metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Gets or sets when the source was accessed
    /// </summary>
    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the source was proccesed
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}