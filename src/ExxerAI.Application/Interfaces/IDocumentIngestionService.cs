using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service for ingesting and processing documents
/// </summary>
public interface IDocumentIngestionService
{
    /// <summary>
    /// Ingests a document from the specified source
    /// </summary>
    /// <param name="documentId">The document identifier</param>
    /// <param name="source">The document source path or URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document asset representing the ingested document</returns>
    Task<DocumentAsset> IngestDocumentAsync(string documentId, string source, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ingests multiple documents in batch
    /// </summary>
    /// <param name="documents">Collection of document identifiers and sources</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of ingested document assets</returns>
    Task<IEnumerable<DocumentAsset>> IngestDocumentsBatchAsync(IEnumerable<(string Id, string Source)> documents, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the ingestion status of a document
    /// </summary>
    /// <param name="documentId">The document identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Ingestion status</returns>
    Task<string> GetIngestionStatusAsync(string documentId, CancellationToken cancellationToken = default);
} 