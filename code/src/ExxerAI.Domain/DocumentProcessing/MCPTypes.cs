namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents MCP watch options for monitoring Google Drive folders
/// </summary>
public class MCPWatchOptions
{
    /// <summary>
    /// Gets or sets whether to include subdirectories in monitoring
    /// </summary>
    public bool IncludeSubdirectories { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to automatically process new documents
    /// </summary>
    public bool AutoProcess { get; set; } = false;

    /// <summary>
    /// Gets or sets the polling interval in seconds
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Gets the collection of file types to monitor
    /// </summary>
    public List<string> FileTypes { get; set; } = new() { ".pdf", ".docx", ".xlsx" };

    /// <summary>
    /// Gets or sets additional watch parameters
    /// </summary>
    public Dictionary<string, object> CustomParameters { get; set; } = new();
}

/// <summary>
/// Represents an MCP response with operation results
/// </summary>
public class MCPResponse
{
    /// <summary>
    /// Gets or sets the response identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets additional response data
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
}

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
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Represents MCP upload result for processed documents
/// </summary>
public class MCPUploadResult
{
    /// <summary>
    /// Gets or sets the uploaded file identifier
    /// </summary>
    public string FileId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the upload URL
    /// </summary>
    public string UploadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the upload was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the upload timestamp
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets additional upload metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents MCP server health status
/// </summary>
public class MCPHealthStatus
{
    /// <summary>
    /// Gets or sets whether the MCP server is healthy
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Gets or sets the server version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last health check timestamp
    /// </summary>
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Gets or sets any health check errors
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets additional health metrics
    /// </summary>
    public Dictionary<string, object> Metrics { get; set; } = new();
}

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

/// <summary>
/// Enumeration of document change types
/// </summary>
public enum DocumentChangeType
{
    /// <summary>
    /// Document was created
    /// </summary>
    Created,

    /// <summary>
    /// Document was modified
    /// </summary>
    Modified,

    /// <summary>
    /// Document was deleted
    /// </summary>
    Deleted,

    /// <summary>
    /// Document was moved
    /// </summary>
    Moved,

    /// <summary>
    /// Document was renamed
    /// </summary>
    Renamed,

    /// <summary>
    /// Document was restored
    /// </summary>
    Restored
}

/// <summary>
/// Represents an active MCP watch session
/// </summary>
public class MCPWatchSession
{
    /// <summary>
    /// Gets or sets the watch session ID
    /// </summary>
    public string WatchId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder ID being watched
    /// </summary>
    public string FolderId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the watch was started
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the watch options
    /// </summary>
    public MCPWatchOptions Options { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the watch is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Represents an MCP document processing request
/// </summary>
public class MCPDocumentRequest
{
    /// <summary>
    /// Gets or sets the document ID
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the processing options
    /// </summary>
    public ProcessingOptions ProcessingOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the session ID
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional request parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = new();
}