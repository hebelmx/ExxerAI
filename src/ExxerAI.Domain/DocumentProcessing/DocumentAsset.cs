using System.ComponentModel.DataAnnotations;

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
    /// Gets or sets the current status of the document.
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
    public List<string> RelatedDocuments { get; set; } = new();

    /// <summary>
    /// Gets or sets additional metadata as key-value pairs.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

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
    public List<string> ProcessingErrors { get; set; } = new();

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
    public void SetContentHash(string hash)
    {
        ContentHash = hash;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the vector embeddings for semantic search.
    /// </summary>
    /// <param name="embeddings">The vector embeddings array.</param>
    public void SetEmbeddings(float[] embeddings)
    {
        Embeddings = embeddings;
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

/// <summary>
/// Represents document fingerprint information for similarity detection.
/// </summary>
public class DocumentFingerprint
{
    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the document creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the MIME type.
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title pattern for similarity matching.
    /// </summary>
    public string TitlePattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content structure hash (layout, headings, etc.).
    /// </summary>
    public string StructureHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional fingerprint metadata.
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();

    /// <summary>
    /// Calculates similarity score with another fingerprint.
    /// </summary>
    /// <param name="other">The other fingerprint to compare with.</param>
    /// <returns>Similarity score from 0.0 to 1.0.</returns>
    public float CalculateSimilarity(DocumentFingerprint other)
    {
        if (other == null) return 0.0f;

        var score = 0.0f;
        var factors = 0;

        // File size similarity (weight: 0.2)
        if (FileSize > 0 && other.FileSize > 0)
        {
            var sizeRatio = Math.Min(FileSize, other.FileSize) / (float)Math.Max(FileSize, other.FileSize);
            score += sizeRatio * 0.2f;
            factors++;
        }

        // MIME type match (weight: 0.3)
        if (!string.IsNullOrEmpty(MimeType) && !string.IsNullOrEmpty(other.MimeType))
        {
            score += (MimeType.Equals(other.MimeType, StringComparison.OrdinalIgnoreCase) ? 1.0f : 0.0f) * 0.3f;
            factors++;
        }

        // Title pattern similarity (weight: 0.3)
        if (!string.IsNullOrEmpty(TitlePattern) && !string.IsNullOrEmpty(other.TitlePattern))
        {
            var titleSimilarity = CalculateStringSimilarity(TitlePattern, other.TitlePattern);
            score += titleSimilarity * 0.3f;
            factors++;
        }

        // Structure hash match (weight: 0.2)
        if (!string.IsNullOrEmpty(StructureHash) && !string.IsNullOrEmpty(other.StructureHash))
        {
            score += (StructureHash.Equals(other.StructureHash, StringComparison.Ordinal) ? 1.0f : 0.0f) * 0.2f;
            factors++;
        }

        return factors > 0 ? score / factors : 0.0f;
    }

    private static float CalculateStringSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2)) return 0.0f;
        if (str1.Equals(str2, StringComparison.OrdinalIgnoreCase)) return 1.0f;

        // Simple Levenshtein-based similarity
        var maxLength = Math.Max(str1.Length, str2.Length);
        var distance = LevenshteinDistance(str1.ToLowerInvariant(), str2.ToLowerInvariant());
        return 1.0f - (distance / (float)maxLength);
    }

    private static int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        var matrix = new int[source.Length + 1, target.Length + 1];

        for (var i = 0; i <= source.Length; i++) matrix[i, 0] = i;
        for (var j = 0; j <= target.Length; j++) matrix[0, j] = j;

        for (var i = 1; i <= source.Length; i++)
        {
            for (var j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1), matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[source.Length, target.Length];
    }
}

/// <summary>
/// Represents document version information.
/// </summary>
public class DocumentVersion
{
    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    public int Number { get; set; } = 1;

    /// <summary>
    /// Gets or sets the version label (e.g., "final", "draft", "v2").
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the previous version document ID.
    /// </summary>
    public string? PreviousVersionId { get; set; }

    /// <summary>
    /// Gets or sets the next version document ID.
    /// </summary>
    public string? NextVersionId { get; set; }

    /// <summary>
    /// Gets or sets whether this is the latest version.
    /// </summary>
    public bool IsLatest { get; set; } = true;

    /// <summary>
    /// Gets or sets version-specific notes or comments.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this version was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enumeration of document processing statuses.
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Document is currently being processed.
    /// </summary>
    Processing,

    /// <summary>
    /// Document is successfully processed and active.
    /// </summary>
    Active,

    /// <summary>
    /// Document has been deleted (soft delete).
    /// </summary>
    Deleted,

    /// <summary>
    /// Document is archived (superseded by newer version).
    /// </summary>
    Archived,

    /// <summary>
    /// Document processing failed.
    /// </summary>
    Error
} 