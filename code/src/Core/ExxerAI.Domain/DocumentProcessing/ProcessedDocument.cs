namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a processed document for upload
/// </summary>
public class ProcessedDocument
{
    /// <summary>
    /// Gets or sets the original document identifier
    /// </summary>
    public string OriginalDocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the processed content
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the filename for the processed document
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME type
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the processing results
    /// </summary>
    public DocumentProcessingResult? ProcessingResults { get; set; }

    /// <summary>
    /// Gets or sets additional processing metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}