using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ExxerAI.Infrastructure.Services;

/// <summary>
/// Implementation of document notification service for handling document lifecycle events
/// Supports async event handling, subscriber management, and structured logging
/// </summary>
public class DocumentNotificationService : IDocumentNotificationService
{
    private readonly ILogger<DocumentNotificationService> _logger;
    private readonly ConcurrentDictionary<string, Func<DocumentNotification, Task>> _subscribers;
    private readonly SemaphoreSlim _notificationLock;

    /// <summary>
    /// Initializes a new instance of the DocumentNotificationService
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public DocumentNotificationService(ILogger<DocumentNotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subscribers = new ConcurrentDictionary<string, Func<DocumentNotification, Task>>();
        _notificationLock = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Sends a notification when a document is added
    /// </summary>
    /// <param name="document">The document that was added</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result<bool>> NotifyDocumentAddedAsync(DocumentAsset document, CancellationToken cancellationToken = default)
    {
        try
        {
            if (document is null)
            {
                _logger.LogWarning("Attempted to notify document added with null document");
                return Result<bool>.WithFailure("Document cannot be null");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var notification = new DocumentNotification(
                "DocumentAdded",
                document.Id,
                document.OriginalFileName,
                DateTime.UtcNow,
                new Dictionary<string, object>
                {
                    { "DocumentId", document.Id },
                    { "FileName", document.OriginalFileName },
                    { "FileSize", document.FileSize },
                    { "MimeType", document.MimeType }
                }
            );

            await SendNotificationAsync(notification, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Document added notification sent for {DocumentId}: {FileName}",
                document.Id, document.OriginalFileName);

            return Result<bool>.Success(true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Document added notification was cancelled");
            return Result<bool>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending document added notification for {DocumentId}",
                document?.Id);
            return Result<bool>.WithFailure($"Notification failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends a notification when a document is modified
    /// </summary>
    /// <param name="document">The document that was modified</param>
    /// <param name="previousVersion">The previous version information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result<bool>> NotifyDocumentModifiedAsync(DocumentAsset document, string previousVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            if (document is null)
            {
                _logger.LogWarning("Attempted to notify document modified with null document");
                return Result<bool>.WithFailure("Document cannot be null");
            }

            if (string.IsNullOrWhiteSpace(previousVersion))
            {
                _logger.LogWarning("Attempted to notify document modified without previous version for document {DocumentId}",
                    document.Id);
                return Result<bool>.WithFailure("Previous version cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var notification = new DocumentNotification(
                "DocumentModified",
                document.Id,
                document.OriginalFileName,
                DateTime.UtcNow,
                new Dictionary<string, object>
                {
                    { "DocumentId", document.Id },
                    { "FileName", document.OriginalFileName },
                    { "PreviousVersion", previousVersion },
                    { "CurrentVersion", document.Version.ToString() ?? "1.0" },
                    { "ModifiedAt", document.UpdatedAt }
                }
            );

            await SendNotificationAsync(notification, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Document modified notification sent for {DocumentId}: {FileName} (v{PreviousVersion} -> v{CurrentVersion})",
                document.Id, document.OriginalFileName, previousVersion, document.Version);

            return Result<bool>.Success(true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Document modified notification was cancelled");
            return Result<bool>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending document modified notification for {DocumentId}",
                document?.Id);
            return Result<bool>.WithFailure($"Notification failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends a notification when a document is removed
    /// </summary>
    /// <param name="documentId">The identifier of the document that was removed</param>
    /// <param name="documentName">The name of the document that was removed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result<bool>> NotifyDocumentRemovedAsync(string documentId, string documentName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentId))
            {
                _logger.LogWarning("Attempted to notify document removed with empty document ID");
                return Result<bool>.WithFailure("Document ID cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(documentName))
            {
                _logger.LogWarning("Attempted to notify document removed with empty document name for {DocumentId}",
                    documentId);
                return Result<bool>.WithFailure("Document name cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var notification = new DocumentNotification(
                "DocumentRemoved",
                documentId,
                documentName,
                DateTime.UtcNow,
                new Dictionary<string, object>
                {
                    { "DocumentId", documentId },
                    { "DocumentName", documentName },
                    { "RemovedAt", DateTime.UtcNow }
                }
            );

            await SendNotificationAsync(notification, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Document removed notification sent for {DocumentId}: {DocumentName}",
                documentId, documentName);

            return Result<bool>.Success(true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Document removed notification was cancelled");
            return Result<bool>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending document removed notification for {DocumentId}",
                documentId);
            return Result<bool>.WithFailure($"Notification failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends a notification when document processing fails
    /// </summary>
    /// <param name="document">The document that failed processing</param>
    /// <param name="error">The error that occurred</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result<bool>> NotifyProcessingFailedAsync(DocumentAsset document, Exception error, CancellationToken cancellationToken = default)
    {
        try
        {
            if (document is null)
            {
                _logger.LogWarning("Attempted to notify processing failed with null document");
                return Result<bool>.WithFailure("Document cannot be null");
            }

            if (error is null)
            {
                _logger.LogWarning("Attempted to notify processing failed with null error for document {DocumentId}",
                    document.Id);
                return Result<bool>.WithFailure("Error cannot be null");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var notification = new DocumentNotification(
                "ProcessingFailed",
                document.Id,
                document.OriginalFileName,
                DateTime.UtcNow,
                new Dictionary<string, object>
                {
                    { "DocumentId", document.Id },
                    { "FileName", document.OriginalFileName },
                    { "ErrorMessage", error.Message },
                    { "ErrorType", error.GetType().Name },
                    { "FailedAt", DateTime.UtcNow }
                }
            );

            await SendNotificationAsync(notification, cancellationToken).ConfigureAwait(false);

            _logger.LogError(error, "Document processing failed notification sent for {DocumentId}: {FileName}",
                document.Id, document.OriginalFileName);

            return Result<bool>.Success(true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Processing failed notification was cancelled");
            return Result<bool>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending processing failed notification for {DocumentId}",
                document?.Id);
            return Result<bool>.WithFailure($"Notification failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends a notification when document processing is completed
    /// </summary>
    /// <param name="document">The document that was processed</param>
    /// <param name="result">The processing result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result<bool>> NotifyProcessingCompletedAsync(DocumentAsset document, DocumentProcessingResult result, CancellationToken cancellationToken = default)
    {
        try
        {
            if (document is null)
            {
                _logger.LogWarning("Attempted to notify processing completed with null document");
                return Result<bool>.WithFailure("Document cannot be null");
            }

            if (result is null)
            {
                _logger.LogWarning("Attempted to notify processing completed with null result for document {DocumentId}",
                    document.Id);
                return Result<bool>.WithFailure("Processing result cannot be null");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var notification = new DocumentNotification(
                "ProcessingCompleted",
                document.Id,
                document.OriginalFileName,
                DateTime.UtcNow,
                new Dictionary<string, object>
                {
                    { "DocumentId", document.Id },
                    { "FileName", document.OriginalFileName },
                    { "ProcessingTime", result.ProcessingTimeMs },
                    { "OverallConfidence", result.OverallConfidence },
                    { "IsSuccessful", result.IsSuccessful },
                    { "ExtractedFieldsCount", result.ExtractedFields.Count },
                    { "CompletedAt", DateTime.UtcNow }
                }
            );

            await SendNotificationAsync(notification, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Document processing completed notification sent for {DocumentId}: {FileName} (Confidence: {Confidence:P0})",
                document.Id, document.OriginalFileName, result.OverallConfidence);

            return Result<bool>.Success(true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Processing completed notification was cancelled");
            return Result<bool>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending processing completed notification for {DocumentId}",
                document?.Id);
            return Result<bool>.WithFailure($"Notification failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Registers a notification subscriber
    /// </summary>
    /// <param name="subscriberId">The subscriber identifier</param>
    /// <param name="callback">The callback to invoke for notifications</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result<bool>> RegisterSubscriberAsync(string subscriberId, Func<DocumentNotification, Task> callback)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subscriberId))
            {
                _logger.LogWarning("Attempted to register subscriber with empty ID");
                return Result<bool>.WithFailure("Subscriber ID cannot be empty");
            }

            if (callback is null)
            {
                _logger.LogWarning("Attempted to register subscriber {SubscriberId} with null callback",
                    subscriberId);
                return Result<bool>.WithFailure("Callback cannot be null");
            }

            await Task.Delay(1).ConfigureAwait(false); // Simulate async operation

            if (_subscribers.ContainsKey(subscriberId))
            {
                _logger.LogWarning("Subscriber {SubscriberId} already exists", subscriberId);
                return Result<bool>.WithFailure("Subscriber already exists");
            }

            _subscribers.TryAdd(subscriberId, callback);

            _logger.LogInformation("Registered notification subscriber {SubscriberId}", subscriberId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering subscriber {SubscriberId}", subscriberId);
            return Result<bool>.WithFailure($"Registration failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Unregisters a notification subscriber
    /// </summary>
    /// <param name="subscriberId">The subscriber identifier</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result<bool>> UnregisterSubscriberAsync(string subscriberId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subscriberId))
            {
                _logger.LogWarning("Attempted to unregister subscriber with empty ID");
                return Result<bool>.WithFailure("Subscriber ID cannot be empty");
            }

            await Task.Delay(1).ConfigureAwait(false); // Simulate async operation

            if (!_subscribers.TryRemove(subscriberId, out _))
            {
                _logger.LogWarning("Subscriber {SubscriberId} not found for unregistration", subscriberId);
                return Result<bool>.WithFailure("Subscriber not found");
            }

            _logger.LogInformation("Unregistered notification subscriber {SubscriberId}", subscriberId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering subscriber {SubscriberId}", subscriberId);
            return Result<bool>.WithFailure($"Unregistration failed: {ex.Message}");
        }
    }

    // Private helper methods

    private async Task SendNotificationAsync(DocumentNotification notification, CancellationToken cancellationToken)
    {
        if (!_subscribers.Any())
        {
            _logger.LogDebug("No subscribers registered for notification {NotificationType}", notification.Type);
            return;
        }

        await _notificationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var tasks = _subscribers.Values.Select(async callback =>
            {
                try
                {
                    await callback(notification).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Subscriber callback failed for notification {NotificationType} on document {DocumentId}",
                        notification.Type, notification.DocumentId);
                }
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);

            _logger.LogDebug("Sent notification {NotificationType} to {SubscriberCount} subscribers",
                notification.Type, _subscribers.Count);
        }
        finally
        {
            _notificationLock.Release();
        }
    }

    /// <summary>
    /// Disposes the resources used by the DocumentNotificationService
    /// </summary>
    public void Dispose()
    {
        _notificationLock?.Dispose();
        _subscribers.Clear();
        GC.SuppressFinalize(this);
    }
} 