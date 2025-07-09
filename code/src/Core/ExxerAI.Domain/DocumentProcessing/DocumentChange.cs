namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a document change notification from MCP
/// </summary>
public class DocumentChange
{
    /// <summary>
    /// Gets or sets the change identifier
    /// </summary>
    public string ChangeId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document identifier
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of change
    /// </summary>
    public DocumentChangeType ChangeType { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the change
    /// </summary>
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the document metadata after the change
    /// </summary>
    public MCPDocumentMetadata? DocumentMetadata { get; set; }
}