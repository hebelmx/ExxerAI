using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for Google Drive integration - CORE BUSINESS REQUIREMENT
/// </summary>
public interface IGoogleDriveService
{
    /// <summary>
    /// Authenticates with Google Drive using service account or OAuth
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication result</returns>
    Task<AuthenticationResult> AuthenticateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists documents in specified Google Drive folder
    /// </summary>
    /// <param name="folderId">Google Drive folder ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of drive documents</returns>
    Task<IEnumerable<DriveDocument>> ListDocumentsAsync(string folderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads document content from Google Drive
    /// </summary>
    /// <param name="documentId">Google Drive document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document content and metadata</returns>
    Task<DriveDocumentContent> DownloadDocumentAsync(string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Watches folder for changes using Google Drive API webhooks
    /// </summary>
    /// <param name="folderId">Google Drive folder ID</param>
    /// <param name="webhookUrl">Webhook callback URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Watch channel information</returns>
    Task<WatchChannel> WatchFolderAsync(string folderId, string webhookUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops watching folder for changes
    /// </summary>
    /// <param name="channelId">Watch channel ID</param>
    /// <param name="resourceId">Resource ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stop result</returns>
    Task<bool> StopWatchingAsync(string channelId, string resourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes Google Drive webhook notification
    /// </summary>
    /// <param name="notification">Webhook notification payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processing result</returns>
    Task<WebhookProcessingResult> ProcessWebhookAsync(DriveWebhookNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if document has been modified since last processing
    /// </summary>
    /// <param name="documentId">Google Drive document ID</param>
    /// <param name="lastProcessedTime">Last processing timestamp</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if modified</returns>
    Task<bool> IsDocumentModifiedAsync(string documentId, DateTime lastProcessedTime, CancellationToken cancellationToken = default);
} 