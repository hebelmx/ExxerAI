using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Domain.Operations;

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
    Task<Result<MCPResponse>> WatchFolderAsync(
        string folderId,
        MCPWatchOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets document changes for a specific watch session
    /// </summary>
    /// <param name="watchId">The watch session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing document changes</returns>
    Task<Result<IEnumerable<DocumentChange>>> GetDocumentChangesAsync(
        string watchId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata for a specific document
    /// </summary>
    /// <param name="documentId">The document identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing document metadata</returns>
    Task<Result<MCPDocumentMetadata>> GetDocumentMetadataAsync(
        string documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a document from Google Drive
    /// </summary>
    /// <param name="documentId">The document identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the document data</returns>
    Task<Result<byte[]>> DownloadDocumentAsync(
        string documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads processed data back to Google Drive
    /// </summary>
    /// <param name="folderId">The target folder ID</param>
    /// <param name="document">The processed document to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the upload operation</returns>
    Task<Result<MCPUploadResult>> UploadProcessedDataAsync(
        string folderId,
        ProcessedDocument document,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the health agentStatus of the MCP server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the health check operation</returns>
    Task<Result<MCPHealthStatus>> CheckMCPServerHealthAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops watching a specific folder
    /// </summary>
    /// <param name="watchId">The watch session identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the stop operation</returns>
    Task<Result<bool>> StopWatchingAsync(
        string watchId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active watch sessions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing active watches</returns>
    Task<Result<IEnumerable<MCPWatchSession>>> GetActiveWatchesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles document processing requests through MCP protocol
    /// </summary>
    /// <param name="request">The MCP document processing request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the processing operation</returns>
    Task<Result<DocumentProcessingResult>> HandleMCPDocumentProcessingAsync(
        MCPDocumentRequest request,
        CancellationToken cancellationToken = default);
}