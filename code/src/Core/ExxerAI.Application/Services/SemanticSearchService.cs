using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Operations;
using Microsoft.Extensions.Logging;

namespace ExxerAI.Application.Services;

/// <summary>
/// High-level semantic search service that combines embedding generation and vector storage
/// Provides document indexing and semantic search capabilities
/// </summary>
public class SemanticSearchService
{
    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly ILogger<SemanticSearchService> _logger;

    public SemanticSearchService(
        IVectorStore vectorStore,
        IEmbeddingGenerator embeddingGenerator,
        ILogger<SemanticSearchService> logger)
    {
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initialize the semantic search system
    /// </summary>
    public async Task<Result> InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initializing semantic search service");
            
            var result = await _vectorStore.InitializeAsync(cancellationToken);
            if (result.IsFailure)
                return result;

            _logger.LogInformation("Semantic search service initialized successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize semantic search service");
            return Result.Failure($"Initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Index a single document for semantic search
    /// </summary>
    /// <param name="documentId">Unique document identifier</param>
    /// <param name="content">Document content to index</param>
    /// <param name="metadata">Additional metadata to store</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<Result> IndexDocumentAsync(
        string documentId,
        string content,
        Dictionary<string, object> metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentId))
                return Result.Failure("Document ID cannot be null or empty");

            if (string.IsNullOrWhiteSpace(content))
                return Result.Failure("Document content cannot be null or empty");

            _logger.LogDebug("Indexing document: {DocumentId}", documentId);

            // Generate embedding for the content
            var embeddingResult = await _embeddingGenerator.GenerateEmbeddingAsync(content, cancellationToken);
            if (embeddingResult.IsFailure)
            {
                _logger.LogError("Failed to generate embedding for document {DocumentId}: {Error}", 
                    documentId, embeddingResult.Error);
                return Result.Failure($"Embedding generation failed: {embeddingResult.Error}");
            }

            // Store in vector database
            var storeResult = await _vectorStore.StoreEmbeddingAsync(
                documentId, content, embeddingResult.Value, metadata, cancellationToken);

            if (storeResult.IsFailure)
            {
                _logger.LogError("Failed to store embedding for document {DocumentId}: {Error}", 
                    documentId, storeResult.Error);
                return Result.Failure($"Vector storage failed: {storeResult.Error}");
            }

            _logger.LogInformation("Successfully indexed document: {DocumentId}", documentId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index document: {DocumentId}", documentId);
            return Result.Failure($"Document indexing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Index multiple documents in batch for efficiency
    /// </summary>
    public async Task<Result<BatchIndexingResult>> IndexDocumentsBatchAsync(
        IEnumerable<DocumentToIndex> documents,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documentList = documents?.ToList() ?? new List<DocumentToIndex>();
            
            if (!documentList.Any())
                return Result.Success(new BatchIndexingResult());

            _logger.LogInformation("Starting batch indexing of {Count} documents", documentList.Count);

            // Generate embeddings for all documents
            var texts = documentList.Select(d => d.Content).ToList();
            var embeddingsResult = await _embeddingGenerator.GenerateBatchEmbeddingsAsync(texts, cancellationToken);

            if (embeddingsResult.IsFailure)
            {
                _logger.LogError("Failed to generate batch embeddings: {Error}", embeddingsResult.Error);
                return Result.Failure<BatchIndexingResult>($"Batch embedding generation failed: {embeddingsResult.Error}");
            }

            // Create vector store items
            var embeddings = embeddingsResult.Value.ToList();
            var vectorItems = new List<VectorStoreItem>();

            for (int i = 0; i < documentList.Count && i < embeddings.Count; i++)
            {
                var doc = documentList[i];
                var embedding = embeddings[i];

                vectorItems.Add(new VectorStoreItem
                {
                    DocumentId = doc.DocumentId,
                    Content = doc.Content,
                    Embeddings = embedding.Embedding,
                    Metadata = doc.Metadata ?? new Dictionary<string, object>()
                });
            }

            // Store in vector database
            var storeResult = await _vectorStore.StoreBatchAsync(vectorItems, cancellationToken);

            var batchResult = new BatchIndexingResult
            {
                TotalDocuments = documentList.Count,
                SuccessfullyIndexed = storeResult.IsSuccess ? documentList.Count : 0,
                Failed = storeResult.IsSuccess ? 0 : documentList.Count,
                Errors = storeResult.IsFailure ? new[] { storeResult.Error } : Array.Empty<string>()
            };

            if (storeResult.IsFailure)
            {
                _logger.LogError("Failed to store batch embeddings: {Error}", storeResult.Error);
                return Result.Failure<BatchIndexingResult>($"Batch vector storage failed: {storeResult.Error}");
            }

            _logger.LogInformation("Successfully indexed {Count} documents in batch", documentList.Count);
            return Result.Success(batchResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index documents in batch");
            return Result.Failure<BatchIndexingResult>($"Batch indexing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Search for documents semantically similar to the query
    /// </summary>
    public async Task<Result<SemanticSearchResults>> SearchAsync(
        string query,
        int maxResults = 10,
        float similarityThreshold = 0.7f,
        Dictionary<string, object> filter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return Result.Failure<SemanticSearchResults>("Query cannot be null or empty");

            _logger.LogDebug("Performing semantic search for query: {Query}", query);

            // Generate embedding for the query
            var queryEmbeddingResult = await _embeddingGenerator.GenerateEmbeddingAsync(query, cancellationToken);
            if (queryEmbeddingResult.IsFailure)
            {
                _logger.LogError("Failed to generate query embedding: {Error}", queryEmbeddingResult.Error);
                return Result.Failure<SemanticSearchResults>($"Query embedding failed: {queryEmbeddingResult.Error}");
            }

            // Search vector store
            var searchResult = await _vectorStore.SearchSimilarAsync(
                queryEmbeddingResult.Value, maxResults, similarityThreshold, filter, cancellationToken);

            if (searchResult.IsFailure)
            {
                _logger.LogError("Vector search failed: {Error}", searchResult.Error);
                return Result.Failure<SemanticSearchResults>($"Vector search failed: {searchResult.Error}");
            }

            var results = new SemanticSearchResults
            {
                Query = query,
                Results = searchResult.Value.Select(r => new SemanticSearchResult
                {
                    DocumentId = r.DocumentId,
                    Content = r.Content,
                    SimilarityScore = r.Score,
                    Metadata = r.Metadata
                }).ToList(),
                TotalFound = searchResult.Value.Count(),
                SearchTime = DateTime.UtcNow
            };

            _logger.LogInformation("Semantic search completed. Found {Count} results for query", results.TotalFound);
            return Result.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform semantic search");
            return Result.Failure<SemanticSearchResults>($"Semantic search failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Remove document from semantic search index
    /// </summary>
    public async Task<Result> RemoveDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentId))
                return Result.Failure("Document ID cannot be null or empty");

            _logger.LogDebug("Removing document from index: {DocumentId}", documentId);

            var result = await _vectorStore.DeleteEmbeddingAsync(documentId, cancellationToken);
            
            if (result.IsSuccess)
                _logger.LogInformation("Successfully removed document: {DocumentId}", documentId);
            else
                _logger.LogError("Failed to remove document {DocumentId}: {Error}", documentId, result.Error);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove document: {DocumentId}", documentId);
            return Result.Failure($"Document removal failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get statistics about the semantic search index
    /// </summary>
    public async Task<Result<VectorStoreStats>> GetIndexStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _vectorStore.GetStatsAsync(cancellationToken);
            
            if (result.IsSuccess)
                _logger.LogDebug("Retrieved index stats: {TotalVectors} vectors", result.Value.TotalVectors);
            else
                _logger.LogError("Failed to get index stats: {Error}", result.Error);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get index statistics");
            return Result.Failure<VectorStoreStats>($"Stats retrieval failed: {ex.Message}");
        }
    }
}

/// <summary>
/// Document to be indexed for semantic search
/// </summary>
public class DocumentToIndex
{
    public string DocumentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Results from semantic search
/// </summary>
public class SemanticSearchResults
{
    public string Query { get; set; } = string.Empty;
    public List<SemanticSearchResult> Results { get; set; } = new();
    public int TotalFound { get; set; }
    public DateTime SearchTime { get; set; }
}

/// <summary>
/// Individual semantic search result
/// </summary>
public class SemanticSearchResult
{
    public string DocumentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public float SimilarityScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result from batch indexing operation
/// </summary>
public class BatchIndexingResult
{
    public int TotalDocuments { get; set; }
    public int SuccessfullyIndexed { get; set; }
    public int Failed { get; set; }
    public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();
}