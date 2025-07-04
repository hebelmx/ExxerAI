using ExxerAI.Domain.Helpers;
using ExxerAI.Domain.Entities;
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
    /// Gets the ingestion agentStatus and statistics.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The ingestion agentStatus information.</returns>
    Task<Result<IngestionStatus>> GetIngestionStatusAsync(
        CancellationToken cancellationToken = default);
}

//Duplicated code removed for brevity
///// <summary>
///// Enumeration of document change types.
///// </summary>
//public enum DocumentChangeType
//{
//    /// <summary>
//    /// Document was created.
//    /// </summary>
//    Created,

//    /// <summary>
//    /// Document was modified.
//    /// </summary>
//    Modified,

//    /// <summary>
//    /// Document was deleted.
//    /// </summary>
//    Deleted,

//    /// <summary>
//    /// Document was moved.
//    /// </summary>
//    Moved,

//    /// <summary>
//    /// Document was renamed.
//    /// </summary>
//    Renamed,

//    /// <summary>
//    /// Document was restored from trash.
//    /// </summary>
//    Restored,

//    /// <summary>
//    /// Document permissions were changed.
//    /// </summary>
//    PermissionsChanged
//}