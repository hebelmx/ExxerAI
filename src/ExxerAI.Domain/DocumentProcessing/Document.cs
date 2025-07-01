using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a document in the processing pipeline with its content and metadata.
/// </summary>
public class Document
{
    /// <summary>
    /// Gets or sets the unique identifier for the document.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the document filename.
    /// </summary>
    [StringLength(500)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw document content.
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the extracted text content.
    /// </summary>
    public string TextContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type classification.
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the MIME type of the document.
    /// </summary>
    [StringLength(100)]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document metadata.
    /// </summary>
    public DocumentMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets when the document was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the document was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the content hash for integrity verification.
    /// </summary>
    [StringLength(64)]
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional document properties.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the Document class.
    /// </summary>
    public Document() { }

    /// <summary>
    /// Initializes a new instance of the Document class with basic properties.
    /// </summary>
    /// <param name="fileName">The document filename.</param>
    /// <param name="content">The document content.</param>
    /// <param name="mimeType">The MIME type.</param>
    public Document(string fileName, byte[] content, string mimeType)
    {
        FileName = fileName;
        Content = content;
        MimeType = mimeType;
        Metadata = new DocumentMetadata
        {
            FileName = fileName,
            MimeType = mimeType,
            FileSize = content.Length
        };
    }

    /// <summary>
    /// Gets the size of the document content in bytes.
    /// </summary>
    public long SizeInBytes => Content.Length;

    /// <summary>
    /// Checks if the document has text content extracted.
    /// </summary>
    public bool HasTextContent => !string.IsNullOrWhiteSpace(TextContent);

    /// <summary>
    /// Gets a summary of the document for display purposes.
    /// </summary>
    /// <returns>A formatted summary string.</returns>
    public string GetSummary()
    {
        return $"Document [{Id[..8]}]: {FileName} ({SizeInBytes} bytes, {DocumentType})";
    }
} 