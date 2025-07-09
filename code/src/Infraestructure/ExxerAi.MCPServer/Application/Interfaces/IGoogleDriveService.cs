using ExxerAI.Domain;
using ExxerAi.MCPServer.Application.Services;
using ExxerAI.Domain.Operations;

namespace ExxerAi.MCPServer.Application.Interfaces;

/// <summary>
/// Interface for Google Drive service providing real API integration
/// </summary>
public interface IGoogleDriveService
{
    /// <summary>
    /// Initializes Google Drive service with OAuth authentication
    /// </summary>
    Task<Result<bool>> InitializeAsync();

    /// <summary>
    /// Starts monitoring a Google Drive folder for changes
    /// </summary>
    /// <param name="folderId">The folder ID to monitor</param>
    /// <param name="includeSubdirectories">Include subdirectories in monitoring</param>
    /// <param name="autoProcess">Automatically process detected files</param>
    /// <param name="pollingIntervalSeconds">Polling interval in seconds</param>
    /// <returns>Watch session information</returns>
    Task<Result<string>> StartFolderWatchAsync(string folderId, bool includeSubdirectories = true, bool autoProcess = true, int pollingIntervalSeconds = 60);

    /// <summary>
    /// Downloads a document from Google Drive
    /// </summary>
    /// <param name="documentId">The document ID to download</param>
    /// <returns>Downloaded document data</returns>
    Task<Result<byte[]>> DownloadDocumentAsync(string documentId);

    /// <summary>
    /// Gets metadata for a Google Drive document
    /// </summary>
    /// <param name="documentId">The document ID</param>
    /// <returns>Document metadata</returns>
    Task<Result<GoogleDriveFileMetadata>> GetDocumentMetadataAsync(string documentId);

    /// <summary>
    /// Gets active watch sessions
    /// </summary>
    Task<Result<string>> GetActiveWatchesAsync();

    /// <summary>
    /// Stops a watch session
    /// </summary>
    /// <param name="watchId">The watch session identifier</param>
    Task<Result<string>> StopWatchingAsync(string watchId);
} 