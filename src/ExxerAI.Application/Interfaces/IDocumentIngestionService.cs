using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service for intelligent document ingestion from Google Drive with version management and deduplication.
/// Implements the complete document processing pipeline from the design specification.
/// </summary>
public interface IDocumentIngestionService
{
    /// <summary>
    /// Starts watching a Google Drive folder for document changes.
    /// </summary>
    /// <param name="folderId">The Google Drive folder ID to watch.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The watch session ID for tracking changes.</returns>
    Task<Result<string>> StartWatchingFolderAsync(
        string folderId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops watching a Google Drive folder.
    /// </summary>
    /// <param name="watchId">The watch session ID to stop.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The result of the stop operation.</returns>
    Task<Result<bool>> StopWatchingFolderAsync(
        string watchId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects and processes document changes from watched folders.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The list of detected document changes.</returns>
    Task<Result<IEnumerable<DocumentChangeEvent>>> DetectDocumentChangesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a document change event through the complete pipeline.
    /// </summary>
    /// <param name="changeEvent">The document change event to process.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The document processing result.</returns>
    Task<Result<DocumentProcessingResult>> ProcessDocumentChangeAsync(
        DocumentChangeEvent changeEvent, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ingests a document directly by ID through the complete processing pipeline.
    /// </summary>
    /// <param name="documentId">The Google Drive document ID.</param>
    /// <param name="forceReprocess">Whether to force reprocessing even if document exists.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The document processing result.</returns>
    Task<Result<DocumentProcessingResult>> IngestDocumentAsync(
        string documentId, 
        bool forceReprocess = false, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a document has been modified since last processing.
    /// </summary>
    /// <param name="documentId">The document ID to check.</param>
    /// <param name="lastProcessed">When the document was last processed.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>True if the document has been modified.</returns>
    Task<Result<bool>> IsDocumentModifiedAsync(
        string documentId, 
        DateTime lastProcessed, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the ingestion status and statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The ingestion status information.</returns>
    Task<Result<IngestionStatus>> GetIngestionStatusAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a document change event detected during folder watching.
/// </summary>
public class DocumentChangeEvent
{
    /// <summary>
    /// Gets or sets the unique event identifier.
    /// </summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the document ID that changed.
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of change that occurred.
    /// </summary>
    public DocumentChangeType ChangeType { get; set; }

    /// <summary>
    /// Gets or sets when the change was detected.
    /// </summary>
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the change actually occurred.
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// Gets or sets the document metadata at time of change.
    /// </summary>
    public DocumentMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the previous document hash for change detection.
    /// </summary>
    public string? PreviousHash { get; set; }

    /// <summary>
    /// Gets or sets the new document hash.
    /// </summary>
    public string? NewHash { get; set; }

    /// <summary>
    /// Gets or sets the watch session that detected this change.
    /// </summary>
    public string WatchSessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional change metadata.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Gets whether this change represents a content modification.
    /// </summary>
    public bool IsContentChange => ChangeType == DocumentChangeType.Modified && 
                                   !string.IsNullOrEmpty(PreviousHash) && 
                                   !string.IsNullOrEmpty(NewHash) && 
                                   !PreviousHash.Equals(NewHash);

    /// <summary>
    /// Gets whether this change requires processing.
    /// </summary>
    public bool RequiresProcessing => ChangeType == DocumentChangeType.Created || 
                                      ChangeType == DocumentChangeType.Modified || 
                                      ChangeType == DocumentChangeType.Restored;
}

/// <summary>
/// Enumeration of document change types.
/// </summary>
public enum DocumentChangeType
{
    /// <summary>
    /// Document was created.
    /// </summary>
    Created,

    /// <summary>
    /// Document was modified.
    /// </summary>
    Modified,

    /// <summary>
    /// Document was deleted.
    /// </summary>
    Deleted,

    /// <summary>
    /// Document was moved.
    /// </summary>
    Moved,

    /// <summary>
    /// Document was renamed.
    /// </summary>
    Renamed,

    /// <summary>
    /// Document was restored from trash.
    /// </summary>
    Restored,

    /// <summary>
    /// Document permissions were changed.
    /// </summary>
    PermissionsChanged
}

/// <summary>
/// Represents the status and statistics of the document ingestion system.
/// </summary>
public class IngestionStatus
{
    /// <summary>
    /// Gets or sets the total number of documents being watched.
    /// </summary>
    public int DocumentsWatched { get; set; }

    /// <summary>
    /// Gets or sets the number of documents processed today.
    /// </summary>
    public int DocumentsProcessedToday { get; set; }

    /// <summary>
    /// Gets or sets the number of documents processed this week.
    /// </summary>
    public int DocumentsProcessedThisWeek { get; set; }

    /// <summary>
    /// Gets or sets the number of active watch sessions.
    /// </summary>
    public int ActiveWatchSessions { get; set; }

    /// <summary>
    /// Gets or sets the number of pending changes to process.
    /// </summary>
    public int PendingChanges { get; set; }

    /// <summary>
    /// Gets or sets the average processing time in milliseconds.
    /// </summary>
    public double AverageProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the overall system health status.
    /// </summary>
    public HealthStatus SystemHealth { get; set; } = HealthStatus.Healthy;

    /// <summary>
    /// Gets or sets the last time the system processed changes.
    /// </summary>
    public DateTime LastProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets any current system errors or warnings.
    /// </summary>
    public List<string> SystemMessages { get; set; } = new();

    /// <summary>
    /// Gets or sets performance metrics for the ingestion system.
    /// </summary>
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Enumeration of system health statuses.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// System is operating normally.
    /// </summary>
    Healthy,

    /// <summary>
    /// System has minor issues but is functional.
    /// </summary>
    Warning,

    /// <summary>
    /// System has significant issues affecting functionality.
    /// </summary>
    Degraded,

    /// <summary>
    /// System is not functional.
    /// </summary>
    Critical
} 