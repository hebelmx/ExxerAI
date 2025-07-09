namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a document asset with complete processing metadata and version tracking.
/// Supports polymorphic document processing with content hash-based deduplication.
/// </summary>
public class DocumentAsset
{
    /// <summary>
    /// Gets or sets the unique identifier for the document asset.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Gets or sets the original filename as uploaded.
    /// </summary>
    [Required]
    [StringLength(500)]
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SHA-256 content hash for deduplication.
    /// </summary>
    [Required]
    [StringLength(64)]
    public string ContentHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document fingerprint for similarity detection.
    /// </summary>
    public DocumentFingerprint Fingerprint { get; set; } = new();

    /// <summary>
    /// Gets or sets the raw document content as byte array.
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the vector embeddings for semantic search.
    /// </summary>
    public float[] Embeddings { get; private set; } = Array.Empty<float>();

    /// <summary>
    /// Gets or sets the current agentStatus of the document.
    /// </summary>
    public DocumentStatus Status { get; private set; } = DocumentStatus.Processing;

    /// <summary>
    /// Gets or sets when the document was processed.
    /// </summary>
    public DateTime ProcessedAt { get; private set; }

    /// <summary>
    /// Gets or sets the source path (Google Drive, file system, etc.).
    /// </summary>
    [StringLength(1000)]
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document version information.
    /// </summary>
    public DocumentVersion Version { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of related document IDs (version chain).
    /// </summary>
    public List<string> RelatedDocuments { get; set; } = [];

    /// <summary>
    /// Gets or sets additional metadata as key-value pairs.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

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
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets when the document was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets or sets when the document was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Gets or sets the extracted text content for search indexing.
    /// </summary>
    public string ExtractedText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the processing confidence score (0.0 to 1.0).
    /// </summary>
    public float ProcessingConfidence { get; set; }

    /// <summary>
    /// Gets or sets any processing errors encountered.
    /// </summary>
    public List<string> ProcessingErrors { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the DocumentAsset class.
    /// </summary>
    /// <param name="fileName">The original filename.</param>
    /// <param name="content">The document content.</param>
    /// <param name="sourcePath">The source path or location.</param>
    public DocumentAsset(string fileName, byte[] content, string sourcePath)
    {
        Id = Guid.NewGuid().ToString();
        OriginalFileName = fileName;
        Content = content;
        SourcePath = sourcePath;
        FileSize = content.Length;
        Status = DocumentStatus.Processing;
        ProcessedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Private parameterless constructor for serialization.
    /// </summary>
    private DocumentAsset()
    {
        Id = Guid.NewGuid().ToString();
        ProcessedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the content hash for the document.
    /// </summary>
    /// <param name="hash">The SHA-256 content hash.</param>
    /// <exception cref="ArgumentException">Thrown when hash is null, empty, or whitespace.</exception>
    public void SetContentHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("Content hash cannot be null, empty, or whitespace.", nameof(hash));
        }

        ContentHash = hash;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the vector embeddings for semantic search.
    /// </summary>
    /// <param name="embeddings">The vector embeddings array.</param>
    public void SetEmbeddings(float[] embeddings)
    {
        Embeddings = embeddings ?? Array.Empty<float>();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the document as successfully processed and active.
    /// </summary>
    public void MarkAsActive()
    {
        Status = DocumentStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the document as deleted (soft delete).
    /// </summary>
    public void MarkAsDeleted()
    {
        Status = DocumentStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the document as archived (superseded by newer version).
    /// </summary>
    public void MarkAsArchived()
    {
        Status = DocumentStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the document processing as failed with error details.
    /// </summary>
    /// <param name="error">The error message.</param>
    public void MarkAsFailed(string error)
    {
        Status = DocumentStatus.Error;
        ProcessingErrors.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}: {error}");
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a related document to the version chain.
    /// </summary>
    /// <param name="documentId">The related document identifier.</param>
    public void AddRelatedDocument(string documentId)
    {
        if (!RelatedDocuments.Contains(documentId))
        {
            RelatedDocuments.Add(documentId);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Adds metadata to the document.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    public void AddMetadata(string key, string value)
    {
        Metadata[key] = value;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets metadata value by key.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <returns>The metadata value or null if not found.</returns>
    public string? GetMetadata(string key)
    {
        return Metadata.GetValueOrDefault(key);
    }

    /// <summary>
    /// Checks if the document is in a deleted state.
    /// </summary>
    /// <returns>True if the document is deleted, false otherwise.</returns>
    public bool IsDeleted() => Status == DocumentStatus.Deleted;

    /// <summary>
    /// Checks if the document is active and available for use.
    /// </summary>
    /// <returns>True if the document is active, false otherwise.</returns>
    public bool IsActive() => Status == DocumentStatus.Active;

    /// <summary>
    /// Checks if the document processing failed.
    /// </summary>
    /// <returns>True if the document processing failed, false otherwise.</returns>
    public bool HasErrors() => Status == DocumentStatus.Error || ProcessingErrors.Any();

    /// <summary>
    /// Gets a summary of the document for logging and debugging.
    /// </summary>
    /// <returns>A formatted string with document summary information.</returns>
    public string GetSummary()
    {
        return $"Document [{Id}]: {OriginalFileName} ({FileSize} bytes, {Status}, Confidence: {ProcessingConfidence:P0})";
    }
}