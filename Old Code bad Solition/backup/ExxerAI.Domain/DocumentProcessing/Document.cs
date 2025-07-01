using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a document in the processing pipeline
/// </summary>
public class Document
{
    /// <summary>
    /// Gets or sets the unique document identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the document content as byte array
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the document metadata
    /// </summary>
    public DocumentMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the extracted text content if available
    /// </summary>
    public string? ExtractedText { get; set; }

    /// <summary>
    /// Gets or sets the document processing status
    /// </summary>
    public DocumentStatus Status { get; set; } = DocumentStatus.Processing;

    /// <summary>
    /// Gets or sets when the document was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the document was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the file size of the document content
    /// </summary>
    public long Size => Content?.Length ?? 0;

    /// <summary>
    /// Gets whether the document has content
    /// </summary>
    public bool HasContent => Content?.Length > 0;

    /// <summary>
    /// Gets whether the document has extracted text
    /// </summary>
    public bool HasExtractedText => !string.IsNullOrEmpty(ExtractedText);

    /// <summary>
    /// Initializes a new instance of the Document class
    /// </summary>
    public Document() { }

    /// <summary>
    /// Initializes a new instance of the Document class with content and metadata
    /// </summary>
    /// <param name="content">The document content</param>
    /// <param name="metadata">The document metadata</param>
    public Document(byte[] content, DocumentMetadata metadata)
    {
        Content = content;
        Metadata = metadata;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the extracted text content
    /// </summary>
    /// <param name="text">The extracted text</param>
    public void SetExtractedText(string text)
    {
        ExtractedText = text;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the document status
    /// </summary>
    /// <param name="status">The new status</param>
    public void UpdateStatus(DocumentStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
} 