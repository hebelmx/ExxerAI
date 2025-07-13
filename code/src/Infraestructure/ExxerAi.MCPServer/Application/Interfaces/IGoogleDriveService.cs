using ExxerAI.Domain;
using ExxerAI.MCPServer.Application.Services;
using ExxerAI.Domain.Operations;

namespace ExxerAI.MCPServer.Application.Interfaces;

/// <summary>
/// Interface for Google Drive service providing real API integration
/// </summary>
public interface IGoogleDriveService
{
    /// <summary>
    /// Initializes Google Drive service with OAuth authentication
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    Task<Result<bool>> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts monitoring a Google Drive folder for changes
    /// </summary>
    /// <param name="folderId">The folder ID to monitor</param>
    /// <param name="includeSubdirectories">Include subdirectories in monitoring</param>
    /// <param name="autoProcess">Automatically process detected files</param>
    /// <param name="pollingIntervalSeconds">Polling interval in seconds</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Watch session information</returns>
    Task<Result<string>> StartFolderWatchAsync(string folderId, bool includeSubdirectories = true, bool autoProcess = true, int pollingIntervalSeconds = 60, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a document from Google Drive
    /// </summary>
    /// <param name="documentId">The document ID to download</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Downloaded document data</returns>
    Task<Result<byte[]>> DownloadDocumentAsync(string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata for a Google Drive document
    /// </summary>
    /// <param name="documentId">The document ID</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>Document metadata</returns>
    Task<Result<GoogleDriveFileMetadata>> GetDocumentMetadataAsync(string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active watch sessions
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    Task<Result<string>> GetActiveWatchesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops a watch session
    /// </summary>
    /// <param name="watchId">The watch session identifier</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    Task<Result<string>> StopWatchingAsync(string watchId, CancellationToken cancellationToken = default);
} 