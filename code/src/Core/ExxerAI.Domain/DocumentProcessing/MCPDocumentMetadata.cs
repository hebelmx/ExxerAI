namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents MCP document metadata from Google Drive
/// </summary>
public class MCPDocumentMetadata
{
    /// <summary>
    /// Gets or sets the document identifier in Google Drive
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME type of the document
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// Gets or sets the last modification timestamp
    /// </summary>
    public DateTime ModifiedTime { get; set; }

    /// <summary>
    /// Gets or sets the full path in Google Drive
    /// </summary>
    public string DriveFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional metadata properties
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = [];
}