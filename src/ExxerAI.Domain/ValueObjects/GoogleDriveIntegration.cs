namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// Authentication result for Google Drive integration
/// </summary>
public record AuthenticationResult
{
    public bool IsSuccessful { get; init; }
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public List<string> Scopes { get; init; } = new();
    public string ErrorMessage { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
}

/// <summary>
/// Google Drive document information
/// </summary>
public record DriveDocument
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public long Size { get; init; }
    public DateTime CreatedTime { get; init; }
    public DateTime ModifiedTime { get; init; }
    public string WebViewLink { get; init; } = string.Empty;
    public string WebContentLink { get; init; } = string.Empty;
    public List<string> Parents { get; init; } = new();
    public DriveDocumentOwner Owner { get; init; } = new();
    public bool Shared { get; init; }
    public string Md5Checksum { get; init; } = string.Empty;
    public Dictionary<string, object> Properties { get; init; } = new();
}

/// <summary>
/// Google Drive document owner information
/// </summary>
public record DriveDocumentOwner
{
    public string DisplayName { get; init; } = string.Empty;
    public string EmailAddress { get; init; } = string.Empty;
    public bool Me { get; init; }
    public string PhotoLink { get; init; } = string.Empty;
}

/// <summary>
/// Google Drive document content with metadata
/// </summary>
public record DriveDocumentContent
{
    public string DocumentId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public string TextContent { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public DateTime DownloadedAt { get; init; } = DateTime.UtcNow;
    public DriveDocument Metadata { get; init; } = new();
    public long ContentLength { get; init; }
}

/// <summary>
/// Google Drive watch channel for folder monitoring
/// </summary>
public record WatchChannel
{
    public string ChannelId { get; init; } = Guid.NewGuid().ToString();
    public string ResourceId { get; init; } = string.Empty;
    public string ResourceUri { get; init; } = string.Empty;
    public string WebhookUrl { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; init; }
    public string Token { get; init; } = string.Empty;
    public Dictionary<string, string> Params { get; init; } = new();
}

/// <summary>
/// Google Drive webhook notification payload
/// </summary>
public record DriveWebhookNotification
{
    public string ChannelId { get; init; } = string.Empty;
    public string ResourceId { get; init; } = string.Empty;
    public string ResourceState { get; init; } = string.Empty; // sync, add, remove, update, move, trash
    public string ResourceUri { get; init; } = string.Empty;
    public DateTime EventTime { get; init; } = DateTime.UtcNow;
    public string ChangedDocumentId { get; init; } = string.Empty;
    public Dictionary<string, string> Headers { get; init; } = new();
}

/// <summary>
/// Result of processing a Google Drive webhook
/// </summary>
public record WebhookProcessingResult
{
    public bool IsSuccessful { get; init; }
    public string ProcessingAction { get; init; } = string.Empty; // Downloaded, Updated, Deleted, Ignored
    public string DocumentId { get; init; } = string.Empty;
    public string DocumentName { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
    public string ErrorMessage { get; init; } = string.Empty;
    public bool RequiresReprocessing { get; init; }
    public Dictionary<string, object> ProcessingMetadata { get; init; } = new();
}

/// <summary>
/// Document processing metadata and fingerprint
/// </summary>
public record DocumentMetadata
{
    public string DocumentId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentHash { get; init; } = string.Empty; // SHA-256
    public DocumentFingerprint Fingerprint { get; init; } = new();
    public long FileSize { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime ModifiedDate { get; init; }
    public string MimeType { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public DocumentType DocumentType { get; init; } = DocumentType.Unknown;
    public int PageCount { get; init; }
    public string Language { get; init; } = string.Empty;
    public Dictionary<string, object> CustomProperties { get; init; } = new();
}

/// <summary>
/// Document processing result
/// </summary>
public record DocumentProcessingResult
{
    public bool IsSuccessful { get; init; }
    public string DocumentId { get; init; } = string.Empty;
    public string ExtractedText { get; init; } = string.Empty;
    public DocumentMetadata Metadata { get; init; } = new();
    public List<DocumentSection> Sections { get; init; } = new();
    public float[] Embeddings { get; init; } = Array.Empty<float>();
    public TimeSpan ProcessingTime { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public List<string> ProcessingWarnings { get; init; } = new();
}

/// <summary>
/// Document section for structured content
/// </summary>
public record DocumentSection
{
    public string SectionId { get; init; } = Guid.NewGuid().ToString();
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public int StartPage { get; init; }
    public int EndPage { get; init; }
    public DocumentSectionType Type { get; init; } = DocumentSectionType.Content;
    public float[] Embeddings { get; init; } = Array.Empty<float>();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Document fingerprint for deduplication
/// </summary>
public record DocumentFingerprint
{
    public long FileSize { get; init; }
    public DateTime CreatedDate { get; init; }
    public string MimeType { get; init; } = string.Empty;
    public string TitlePattern { get; init; } = string.Empty;
    public string ContentPreview { get; init; } = string.Empty; // First 500 chars
    public string StructuralHash { get; init; } = string.Empty; // Based on structure, not content
}

// Enums for document processing
public enum DocumentType
{
    Unknown,
    PDF,
    Word,
    Excel,
    PowerPoint,
    Text,
    Markdown,
    HTML,
    Image,
    GoogleDoc,
    GoogleSheet,
    GoogleSlides
}

public enum DocumentSectionType
{
    Content,
    Header,
    Footer,
    Table,
    Image,
    Chart,
    Appendix,
    Bibliography
} 