using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a document asset in the system with content, metadata, and processing information
/// </summary>
public class DocumentAsset
{
    /// <summary>
    /// Gets or sets the unique identifier for the document
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the original filename
    /// </summary>
    [Required]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SHA-256 content hash for deduplication
    /// </summary>
    [Required]
    [StringLength(64)]
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document fingerprint for similarity detection
    /// </summary>
    public DocumentFingerprint Fingerprint { get; set; } = new();

    /// <summary>
    /// Gets or sets the document content as byte array
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the vector embeddings for semantic search
    /// </summary>
    public float[] Embeddings { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Gets or sets the current status of the document
    /// </summary>
    public DocumentStatus Status { get; set; } = DocumentStatus.Processing;

    /// <summary>
    /// Gets or sets when the document was processed
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the source path where the document originated
    /// </summary>
    [StringLength(1000)]
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document version information
    /// </summary>
    public DocumentVersion Version { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of related document IDs
    /// </summary>
    public List<Guid> RelatedDocuments { get; init; } = new();

    /// <summary>
    /// Gets or sets custom metadata properties
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new();

    /// <summary>
    /// Gets or sets the MIME type of the document
    /// </summary>
    [StringLength(100)]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the document in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets whether the document is currently deleted (soft delete)
    /// </summary>
    public bool IsDeleted => Status == DocumentStatus.Deleted;

    /// <summary>
    /// Marks the document as active and ready for use
    /// </summary>
    public void MarkAsActive()
    {
        Status = DocumentStatus.Active;
    }

    /// <summary>
    /// Marks the document as deleted (soft delete - preserves embeddings)
    /// </summary>
    public void MarkAsDeleted()
    {
        Status = DocumentStatus.Deleted;
    }

    /// <summary>
    /// Adds a related document ID to the relationship list
    /// </summary>
    /// <param name="documentId">The ID of the related document</param>
    public void AddRelatedDocument(Guid documentId)
    {
        if (!RelatedDocuments.Contains(documentId))
        {
            RelatedDocuments.Add(documentId);
        }
    }

    /// <summary>
    /// Sets the content hash for the document
    /// </summary>
    /// <param name="hash">The SHA-256 hash string</param>
    public void SetContentHash(string hash)
    {
        if (string.IsNullOrEmpty(hash))
            throw new ArgumentException("Content hash cannot be null or empty", nameof(hash));
        
        ContentHash = hash;
    }

    /// <summary>
    /// Sets the vector embeddings for the document
    /// </summary>
    /// <param name="embeddings">The embedding vector</param>
    public void SetEmbeddings(float[] embeddings)
    {
        Embeddings = embeddings ?? Array.Empty<float>();
    }
}

/// <summary>
/// Represents the possible states of a document
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Document is currently being processed
    /// </summary>
    Processing,
    
    /// <summary>
    /// Document is active and available for use
    /// </summary>
    Active,
    
    /// <summary>
    /// Document is marked as deleted but embeddings are preserved
    /// </summary>
    Deleted,
    
    /// <summary>
    /// Document is archived (older version, superseded)
    /// </summary>
    Archived,
    
    /// <summary>
    /// Document processing failed
    /// </summary>
    Error
}

/// <summary>
/// Represents document fingerprint information for similarity detection
/// </summary>
public class DocumentFingerprint
{
    /// <summary>
    /// Gets or sets the file size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the document
    /// </summary>
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// Gets or sets the modification date of the document
    /// </summary>
    public DateTime ModificationDate { get; set; }

    /// <summary>
    /// Gets or sets the title similarity hash for name pattern matching
    /// </summary>
    public string TitleHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional fingerprint properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
}

/// <summary>
/// Represents version information for a document
/// </summary>
public class DocumentVersion
{
    /// <summary>
    /// Gets or sets the version number
    /// </summary>
    public int VersionNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the version label (e.g., "v1", "final", "draft")
    /// </summary>
    public string VersionLabel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base document ID for version chains
    /// </summary>
    public Guid? BaseDocumentId { get; set; }

    /// <summary>
    /// Gets or sets the previous version document ID
    /// </summary>
    public Guid? PreviousVersionId { get; set; }

    /// <summary>
    /// Gets or sets the change description for this version
    /// </summary>
    public string ChangeDescription { get; set; } = string.Empty;
} 