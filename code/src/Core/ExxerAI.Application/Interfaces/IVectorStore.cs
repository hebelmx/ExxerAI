using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for vector database operations
/// Provides semantic search capabilities for document embeddings
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Initialize the vector store with collection and schema setup
    /// </summary>
    Task<Result> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Store document embeddings in the vector database
    /// </summary>
    /// <param name="documentId">Unique document identifier</param>
    /// <param name="content">Document content for indexing</param>
    /// <param name="embeddings">Pre-computed embeddings vector</param>
    /// <param name="metadata">Additional metadata to store with the vector</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result> StoreEmbeddingAsync(
        string documentId,
        string content,
        float[] embeddings,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for similar documents using vector similarity
    /// </summary>
    /// <param name="queryEmbedding">Query vector for similarity search</param>
    /// <param name="limit">Maximum number of results to return</param>
    /// <param name="threshold">Minimum similarity threshold (0.0 to 1.0)</param>
    /// <param name="filter">Optional metadata filter conditions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result<IEnumerable<VectorSearchResult>>> SearchSimilarAsync(
        float[] queryEmbedding,
        int limit = 10,
        float threshold = 0.7f,
        Dictionary<string, object>? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete document embeddings from the vector store
    /// </summary>
    /// <param name="documentId">Document identifier to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result> DeleteEmbeddingAsync(string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing document embeddings
    /// </summary>
    /// <param name="documentId">Document identifier to update</param>
    /// <param name="content">Updated document content</param>
    /// <param name="embeddings">Updated embeddings vector</param>
    /// <param name="metadata">Updated metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result> UpdateEmbeddingAsync(
        string documentId,
        string content,
        float[] embeddings,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get collection statistics and health information
    /// </summary>
    Task<Result<VectorStoreStats>> GetStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch operations for efficiency
    /// </summary>
    Task<Result> StoreBatchAsync(
        IEnumerable<VectorStoreItem> items,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from vector similarity search
/// </summary>
public class VectorSearchResult
{
    public string DocumentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public float Score { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Item for batch vector store operations
/// </summary>
public class VectorStoreItem
{
    public string DocumentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public float[] Embeddings { get; set; } = Array.Empty<float>();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Vector store collection statistics
/// </summary>
public class VectorStoreStats
{
    public long TotalVectors { get; set; }
    public long CollectionSize { get; set; }
    public int VectorDimensions { get; set; }
    public string IndexingStatus { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, object> AdditionalStats { get; set; } = new();
}