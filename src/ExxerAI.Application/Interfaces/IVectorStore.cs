using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for vector database operations (technology-agnostic)
/// Initial evaluation targets: Qdrant, pgvector, others
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Gets the provider name (Qdrant, pgvector, etc.)
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Stores document embeddings in vector database
    /// </summary>
    /// <param name="documentId">Unique document identifier</param>
    /// <param name="embeddings">Vector embeddings</param>
    /// <param name="metadata">Document metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Storage result</returns>
    Task<VectorStorageResult> StoreEmbeddingsAsync(
        string documentId, 
        float[] embeddings, 
        Dictionary<string, object> metadata, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs semantic search using vector similarity
    /// </summary>
    /// <param name="queryEmbeddings">Query vector embeddings</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="scoreThreshold">Minimum similarity score</param>
    /// <param name="filter">Optional metadata filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results with similarity scores</returns>
    Task<IEnumerable<VectorSearchResult>> SearchAsync(
        float[] queryEmbeddings, 
        int limit = 10, 
        float scoreThreshold = 0.7f,
        Dictionary<string, object>? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates existing document embeddings
    /// </summary>
    /// <param name="documentId">Document identifier</param>
    /// <param name="embeddings">New vector embeddings</param>
    /// <param name="metadata">Updated metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update result</returns>
    Task<bool> UpdateEmbeddingsAsync(
        string documentId, 
        float[] embeddings, 
        Dictionary<string, object> metadata, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes document embeddings from vector store
    /// </summary>
    /// <param name="documentId">Document identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion result</returns>
    Task<bool> DeleteEmbeddingsAsync(string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new collection/index in the vector store
    /// </summary>
    /// <param name="collectionName">Collection name</param>
    /// <param name="vectorSize">Embedding vector dimensions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Creation result</returns>
    Task<bool> CreateCollectionAsync(string collectionName, int vectorSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks vector store health and connectivity
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health status</returns>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
} 