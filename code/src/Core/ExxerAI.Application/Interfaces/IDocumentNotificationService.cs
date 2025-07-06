using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Domain;
using ExxerAI.Domain.Helpers.Operations;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service for sending notifications about document events
/// </summary>
public interface IDocumentNotificationService
{
    /// <summary>
    /// Sends a notification when a document is added
    /// </summary>
    /// <param name="document">The document that was added</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> NotifyDocumentAddedAsync(DocumentAsset document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification when a document is modified
    /// </summary>
    /// <param name="document">The document that was modified</param>
    /// <param name="previousVersion">The previous version information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> NotifyDocumentModifiedAsync(DocumentAsset document, string previousVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification when a document is removed
    /// </summary>
    /// <param name="documentId">The identifier of the document that was removed</param>
    /// <param name="documentName">The name of the document that was removed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> NotifyDocumentRemovedAsync(string documentId, string documentName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification when document processing fails
    /// </summary>
    /// <param name="document">The document that failed processing</param>
    /// <param name="error">The error that occurred</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> NotifyProcessingFailedAsync(DocumentAsset document, Exception error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification when document processing is completed
    /// </summary>
    /// <param name="document">The document that was processed</param>
    /// <param name="result">The processing result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> NotifyProcessingCompletedAsync(DocumentAsset document, DocumentProcessingResult result, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a notification subscriber
    /// </summary>
    /// <param name="subscriberId">The subscriber identifier</param>
    /// <param name="callback">The callback to invoke for notifications</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> RegisterSubscriberAsync(string subscriberId, Func<DocumentNotification, Task> callback);

    /// <summary>
    /// Unregisters a notification subscriber
    /// </summary>
    /// <param name="subscriberId">The subscriber identifier</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> UnregisterSubscriberAsync(string subscriberId);
}