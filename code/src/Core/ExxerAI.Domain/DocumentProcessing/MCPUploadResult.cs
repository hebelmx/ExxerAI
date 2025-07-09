namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents MCP upload result for processed documents
/// </summary>
public class MCPUploadResult
{
    /// <summary>
    /// Gets or sets the uploaded file identifier
    /// </summary>
    public string FileId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the upload URL
    /// </summary>
    public string UploadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the upload was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the upload timestamp
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets additional upload metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];
}