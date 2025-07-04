namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents metadata about a document including type, schema, and processing options
/// </summary>
public class DocumentMetadata
{
    /// <summary>
    /// Gets or sets the document identifier
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filename
    /// </summary>
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the expected schema for this document
    /// </summary>
    public SchemaDefinition? ExpectedSchema { get; set; }

    /// <summary>
    /// Gets or sets the source path of the document
    /// </summary>
    [StringLength(1000)]
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the processing options for this document
    /// </summary>
    public ProcessingOptions ProcessingOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the creation date of the document
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the modification date of the document
    /// </summary>
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the file size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the MIME type
    /// </summary>
    [StringLength(100)]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional metadata properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
}