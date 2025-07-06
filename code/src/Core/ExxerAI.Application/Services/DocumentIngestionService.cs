using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Services;

/// <summary>
/// Service for intelligent document ingestion from Google Drive with version management and deduplication.
/// Orchestrates the complete document processing pipeline from source to primary source of truth.
/// </summary>
public class DocumentIngestionService : IDocumentIngestionService
{
    private readonly IPolymorphicDocumentProcessor _documentProcessor;
    private readonly IDocumentHashGenerator _hashGenerator;
    private readonly ILogger<DocumentIngestionService> _logger;
    private readonly GoogleDriveEngine _googleDriveEngine;
    private readonly DocumentProcessingEngine _processingEngine;
    private readonly MetricsEngine _metricsEngine;
    private readonly HealthMonitoringEngine _healthEngine;

    // Simulated watch sessions for demonstration (in real implementation this would be persistent storage)
    private readonly Dictionary<string, WatchSession> _activeSessions = new();

    private readonly List<DocumentChangeEvent> _pendingChanges = new();

    /// <summary>
    /// Initializes a new instance of the DocumentIngestionService class.
    /// </summary>
    /// <param name="documentProcessor">The polymorphic document processor.</param>
    /// <param name="hashGenerator">The document hash generator.</param>
    /// <param name="logger">The logger.</param>
    public DocumentIngestionService(
        IPolymorphicDocumentProcessor documentProcessor,
        IDocumentHashGenerator hashGenerator,
        ILogger<DocumentIngestionService> logger)
    {
        _documentProcessor = documentProcessor ?? throw new ArgumentNullException(nameof(documentProcessor));
        _hashGenerator = hashGenerator ?? throw new ArgumentNullException(nameof(hashGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize focused engine components
        var loggerFactory = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
        _googleDriveEngine = new GoogleDriveEngine(loggerFactory.CreateLogger<GoogleDriveEngine>());
        _processingEngine = new DocumentProcessingEngine(loggerFactory.CreateLogger<DocumentProcessingEngine>());
        _metricsEngine = new MetricsEngine(loggerFactory.CreateLogger<MetricsEngine>());
        _healthEngine = new HealthMonitoringEngine(loggerFactory.CreateLogger<HealthMonitoringEngine>());
    }

    public DocumentIngestionService(IDocumentWatchService watchService, IVersionDetectionEngine versionEngine, IDocumentHashGenerator hashGenerator, IPolymorphicDocumentProcessor documentProcessor, IPrimarySourceOfTruthSystem truthSystem, IDocumentNotificationService notificationService, ILogger<DocumentIngestionService> logger)
    {
        _hashGenerator = hashGenerator;
        _documentProcessor = documentProcessor;
        _logger = logger;
    }

    /// <summary>
    /// Starts watching a Google Drive folder for document changes.
    /// </summary>
    /// <param name="folderId">The Google Drive folder ID to watch.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The watch session ID for tracking changes.</returns>
    public async Task<Result<string>> StartWatchingFolderAsync(string folderId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(folderId))
            {
                return Result<string>.WithFailure("Folder ID cannot be empty");
            }

            _logger.LogInformation("Starting watch for Google Drive folder {FolderId}", folderId);

            var sessionId = Guid.NewGuid().ToString();
            var session = new WatchSession
            {
                SessionId = sessionId,
                FolderId = folderId,
                StartedAt = DateTime.UtcNow,
                IsActive = true,
                DocumentsWatched = 0
            };

            _activeSessions[sessionId] = session;

            _logger.LogInformation("Started watching folder {FolderId} with session {SessionId}", folderId, sessionId);

            // In a real implementation, this would set up the Google Drive API watch
            // For now, we'll simulate this with a placeholder
            await _googleDriveEngine.SimulateGoogleDriveWatchSetupAsync(folderId, sessionId, cancellationToken);

            return Result<string>.WithSuccess(sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting folder watch for {FolderId}", folderId);
            return Result<string>.WithFailure($"Failed to start watching folder: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops watching a Google Drive folder.
    /// </summary>
    /// <param name="watchId">The watch session ID to stop.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The result of the stop operation.</returns>
    public async Task<Result<bool>> StopWatchingFolderAsync(string watchId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_activeSessions.TryGetValue(watchId, out var session))
            {
                return Result<bool>.WithFailure($"Watch session {watchId} not found");
            }

            session.IsActive = false;
            session.StoppedAt = DateTime.UtcNow;

            _logger.LogInformation("Stopped watching session {SessionId} for folder {FolderId}",
                watchId, session.FolderId);

            // In a real implementation, this would clean up the Google Drive API watch
            await _googleDriveEngine.SimulateGoogleDriveWatchCleanupAsync(watchId, cancellationToken);

            return Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping watch session {WatchId}", watchId);
            return Result<bool>.WithFailure($"Failed to stop watching: {ex.Message}");
        }
    }

    /// <summary>
    /// Detects and processes document changes from watched folders.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The list of detected document changes.</returns>
    public async Task<Result<IEnumerable<DocumentChangeEvent>>> DetectDocumentChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Detecting document changes across {SessionCount} active sessions",
                _activeSessions.Count(s => s.Value.IsActive));

            // In a real implementation, this would poll the Google Drive API for changes
            // For demonstration, we'll return any pending simulated changes
            var changes = new List<DocumentChangeEvent>(_pendingChanges);
            _pendingChanges.Clear();

            _logger.LogInformation("Detected {ChangeCount} document changes", changes.Count);

            return Result<IEnumerable<DocumentChangeEvent>>.WithSuccess(changes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting document changes");
            return Result<IEnumerable<DocumentChangeEvent>>.WithFailure($"Failed to detect changes: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes a document change event through the complete pipeline.
    /// </summary>
    /// <param name="changeEvent">The document change event to process.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The document processing result.</returns>
    public async Task<Result<DocumentProcessingResult>> ProcessDocumentChangeAsync(
        DocumentChangeEvent changeEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for cancellation at the start of the method
            cancellationToken.ThrowIfCancellationRequested();

            if (changeEvent == null)
            {
                return Result<DocumentProcessingResult>.WithFailure("Change event cannot be null");
            }

            _logger.LogInformation("Processing document change {EventId} for document {DocumentId} (Type: {ChangeType})",
                changeEvent.EventId, changeEvent.DocumentId, changeEvent.ChangeType);

            // Only process changes that require document processing
            if (!changeEvent.RequiresProcessing)
            {
                _logger.LogDebug("Change event {EventId} does not require processing", changeEvent.EventId);
                return Result<DocumentProcessingResult>.WithSuccess(new DocumentProcessingResult
                {
                    DocumentId = changeEvent.DocumentId,
                    Confidence = 1.0f,
                    LLMConfidence = 1.0f,
                    GroundingConfidence = 1.0f,
                    ProcessingTimeMs = 0
                });
            }

            // For deleted documents, handle separately
            if (changeEvent.ChangeType == DocumentChangeType.Deleted)
            {
                return await _processingEngine.HandleDocumentDeletionAsync(changeEvent, cancellationToken);
            }

            // Download and process the document
            var documentData = await _googleDriveEngine.DownloadDocumentAsync(changeEvent.DocumentId, cancellationToken);
            if (!documentData.IsSuccess)
            {
                return Result<DocumentProcessingResult>.WithFailure($"Failed to download document: {documentData.Error}");
            }

            // Process through the polymorphic document processor
            var processingResult = await _documentProcessor.ProcessDocumentAsync(
                documentData.Data!,
                changeEvent.Metadata,
                cancellationToken);

            if (processingResult.IsSuccess)
            {
                // Update the watch session statistics
                if (_activeSessions.TryGetValue(changeEvent.WatchSessionId, out var session))
                {
                    session.DocumentsProcessed++;
                    session.LastProcessedAt = DateTime.UtcNow;
                }

                _logger.LogInformation("Successfully processed document {DocumentId} with confidence {Confidence:F2}",
                    changeEvent.DocumentId, processingResult.Data!.OverallConfidence);
            }
            else
            {
                _logger.LogWarning("Failed to process document {DocumentId}: {Error}",
                    changeEvent.DocumentId, processingResult.Error);
            }

            return processingResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document change {EventId}", changeEvent.EventId);
            return Result<DocumentProcessingResult>.WithFailure($"Processing error: {ex.Message}");
        }
    }

    /// <summary>
    /// Ingests a document directly by ID through the complete processing pipeline.
    /// </summary>
    /// <param name="documentId">The Google Drive document ID.</param>
    /// <param name="forceReprocess">Whether to force reprocessing even if document exists.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The document processing result.</returns>
    public async Task<Result<DocumentProcessingResult>> IngestDocumentAsync(
        string documentId,
        bool forceReprocess = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentId))
            {
                return Result<DocumentProcessingResult>.WithFailure("Document ID cannot be empty");
            }

            _logger.LogInformation("Ingesting document {DocumentId} (Force reprocess: {ForceReprocess})",
                documentId, forceReprocess);

            // Check if document was already processed (unless forcing reprocess)
            if (!forceReprocess)
            {
                var existingResult = await _processingEngine.CheckExistingDocumentAsync(documentId, cancellationToken);
                if (existingResult.IsSuccess && existingResult.Data != null)
                {
                    _logger.LogDebug("Document {DocumentId} already processed, returning existing result", documentId);
                    return Result<DocumentProcessingResult>.WithSuccess(existingResult.Data);
                }
            }

            // Download document metadata and content
            var metadataResult = await _googleDriveEngine.GetDocumentMetadataAsync(documentId, cancellationToken);
            if (!metadataResult.IsSuccess)
            {
                return Result<DocumentProcessingResult>.WithFailure($"Failed to get metadata: {metadataResult.Error}");
            }

            var documentData = await _googleDriveEngine.DownloadDocumentAsync(documentId, cancellationToken);
            if (!documentData.IsSuccess)
            {
                return Result<DocumentProcessingResult>.WithFailure($"Failed to download document: {documentData.Error}");
            }

            // Process through the polymorphic document processor
            var processingResult = await _documentProcessor.ProcessDocumentAsync(
                documentData.Data!,
                metadataResult.Data!,
                cancellationToken);

            _logger.LogInformation("Completed ingestion of document {DocumentId} with result: {IsSuccess}",
                documentId, processingResult.IsSuccess);

            return processingResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ingesting document {DocumentId}", documentId);
            return Result<DocumentProcessingResult>.WithFailure($"Ingestion error: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a document has been modified since last processing.
    /// </summary>
    /// <param name="documentId">The document ID to check.</param>
    /// <param name="lastProcessed">When the document was last processed.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>True if the document has been modified.</returns>
    public async Task<Result<bool>> IsDocumentModifiedAsync(
        string documentId,
        DateTime lastProcessed,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would check the Google Drive API for modification time
            // For demonstration, we'll simulate this check
            var metadataResult = await _googleDriveEngine.GetDocumentMetadataAsync(documentId, cancellationToken);
            if (!metadataResult.IsSuccess)
            {
                return Result<bool>.WithFailure($"Failed to get document metadata: {metadataResult.Error}");
            }

            var isModified = metadataResult.Data!.ModifiedDate > lastProcessed;

            _logger.LogDebug("Document {DocumentId} modified check: {IsModified} (Last processed: {LastProcessed}, Modified: {ModifiedDate})",
                documentId, isModified, lastProcessed, metadataResult.Data.ModifiedDate);

            return Result<bool>.WithSuccess(isModified);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if document {DocumentId} was modified", documentId);
            return Result<bool>.WithFailure($"Modification check error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the ingestion agentStatus and statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The ingestion agentStatus information.</returns>
    public async Task<Result<IngestionStatus>> GetIngestionStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activeSessions = _activeSessions.Values.Where(s => s.IsActive).ToList();
            var totalDocumentsWatched = activeSessions.Sum(s => s.DocumentsWatched);
            var totalDocumentsProcessed = activeSessions.Sum(s => s.DocumentsProcessed);

            var status = new IngestionStatus
            {
                DocumentsWatched = totalDocumentsWatched,
                DocumentsProcessedToday = await _metricsEngine.GetDocumentsProcessedTodayAsync(cancellationToken),
                DocumentsProcessedThisWeek = await _metricsEngine.GetDocumentsProcessedThisWeekAsync(cancellationToken),
                ActiveWatchSessions = activeSessions.Count,
                PendingChanges = _pendingChanges.Count,
                AverageProcessingTimeMs = await _metricsEngine.GetAverageProcessingTimeAsync(cancellationToken),
                SystemHealth = await _healthEngine.DetermineSystemHealthAsync(activeSessions.Count, cancellationToken),
                LastProcessingTime = activeSessions.Any() ?
                    activeSessions.Max(s => s.LastProcessedAt ?? DateTime.MinValue) :
                    DateTime.MinValue,
                SystemMessages = await _healthEngine.GetSystemMessagesAsync(_activeSessions.Count, _pendingChanges.Count, cancellationToken),
                Metrics = await _metricsEngine.GetDetailedMetricsAsync(_activeSessions.Count, _pendingChanges.Count, cancellationToken)
            };

            return Result<IngestionStatus>.WithSuccess(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ingestion agentStatus");
            return Result<IngestionStatus>.WithFailure($"AgentStatus check error: {ex.Message}");
        }
    }
}