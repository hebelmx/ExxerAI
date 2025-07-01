using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for modern MCP-based Google Drive integration replacing legacy API calls
/// Provides real-time document monitoring and change detection
/// Integrates with existing Python MCP server for protocol compliance
/// </summary>
public interface IMCPGoogleDriveService
{
    /// <summary>
    /// Starts watching a Google Drive folder for changes
    /// </summary>
    /// <param name="folderId">The Google Drive folder ID to watch</param>
    /// <param name="options">Watch configuration options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the watch operation containing the MCP response</returns>
    Task<ExxerAI.Domain.Result<MCPResponse>> WatchFolderAsync(
        string folderId, 
        MCPWatchOptions options, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets document changes for a specific watch session
    /// </summary>
    /// <param name="watchId">The watch session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing document changes</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<DocumentChange>>> GetDocumentChangesAsync(
        string watchId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata for a specific document
    /// </summary>
    /// <param name="documentId">The document identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing document metadata</returns>
    Task<ExxerAI.Domain.Result<MCPDocumentMetadata>> GetDocumentMetadataAsync(
        string documentId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a document from Google Drive
    /// </summary>
    /// <param name="documentId">The document identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the document data</returns>
    Task<ExxerAI.Domain.Result<byte[]>> DownloadDocumentAsync(
        string documentId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads processed data back to Google Drive
    /// </summary>
    /// <param name="folderId">The target folder ID</param>
    /// <param name="document">The processed document to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the upload operation</returns>
    Task<ExxerAI.Domain.Result<MCPUploadResult>> UploadProcessedDataAsync(
        string folderId, 
        ProcessedDocument document, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the health status of the MCP server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the health check operation</returns>
    Task<ExxerAI.Domain.Result<MCPHealthStatus>> CheckMCPServerHealthAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops watching a specific folder
    /// </summary>
    /// <param name="watchId">The watch session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the stop operation</returns>
    Task<ExxerAI.Domain.Result<bool>> StopWatchingAsync(
        string watchId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active watch sessions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing active watches</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<MCPWatchSession>>> GetActiveWatchesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles document processing requests through MCP protocol
    /// </summary>
    /// <param name="request">The MCP document processing request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the processing operation</returns>
    Task<ExxerAI.Domain.Result<DocumentProcessingResult>> HandleMCPDocumentProcessingAsync(
        MCPDocumentRequest request, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an MCP response from the server
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
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Gets or sets the response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response data
    /// </summary>
    public Dictionary<string, object> Data { get; init; } = new();

    /// <summary>
    /// Gets or sets the timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents options for watching Google Drive folders
/// </summary>
public class MCPWatchOptions
{
    /// <summary>
    /// Gets or sets whether to include subdirectories
    /// </summary>
    public bool IncludeSubdirectories { get; set; } = true;

    /// <summary>
    /// Gets or sets the file types to monitor
    /// </summary>
    public List<string> FileTypes { get; init; } = new() { ".pdf", ".docx", ".xlsx" };

    /// <summary>
    /// Gets or sets the polling interval in seconds
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets whether to process files immediately upon detection
    /// </summary>
    public bool AutoProcess { get; set; } = true;

    /// <summary>
    /// Gets or sets additional watch parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = new();
}

/// <summary>
/// Represents a document change detected through MCP
/// </summary>
public class DocumentChange
{
    /// <summary>
    /// Gets or sets the document identifier
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of change
    /// </summary>
    public ChangeType ChangeType { get; set; } = ChangeType.Modified;

    /// <summary>
    /// Gets or sets the document metadata
    /// </summary>
    public MCPDocumentMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets when the change was detected
    /// </summary>
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets additional change information
    /// </summary>
    public Dictionary<string, object> ChangeDetails { get; init; } = new();
}

/// <summary>
/// Represents the types of changes that can occur to documents
/// </summary>
public enum ChangeType
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
    /// Document was renamed
    /// </summary>
    Renamed,
    
    /// <summary>
    /// Document was moved
    /// </summary>
    Moved
}

/// <summary>
/// Represents MCP document metadata
/// </summary>
public class MCPDocumentMetadata
{
    /// <summary>
    /// Gets or sets the document ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME type
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the creation time
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// Gets or sets the modification time
    /// </summary>
    public DateTime ModifiedTime { get; set; }

    /// <summary>
    /// Gets or sets the Google Drive file path
    /// </summary>
    public string DriveFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional metadata properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
}

/// <summary>
/// Represents the result of an MCP upload operation
/// </summary>
public class MCPUploadResult
{
    /// <summary>
    /// Gets or sets the uploaded file ID
    /// </summary>
    public string FileId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file URL
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the upload was successful
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Gets or sets any error message
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents the health status of the MCP server
/// </summary>
public class MCPHealthStatus
{
    /// <summary>
    /// Gets or sets whether the server is healthy
    /// </summary>
    public bool IsHealthy { get; set; } = true;

    /// <summary>
    /// Gets or sets the server version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last check time
    /// </summary>
    public DateTime LastCheckTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets additional status information
    /// </summary>
    public Dictionary<string, object> StatusInfo { get; init; } = new();
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
/// Represents a processed document ready for upload
/// </summary>
public class ProcessedDocument
{
    /// <summary>
    /// Gets or sets the document content
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the filename
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME type
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the processing results
    /// </summary>
    public DocumentProcessingResult ProcessingResults { get; set; } = new();
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