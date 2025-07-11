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
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled();

        try
        {
            _logger.LogInformation("Initializing semantic search service");

            var result = await _vectorStore.InitializeAsync(cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
                return result;

            _logger.LogInformation("Semantic search service initialized successfully");
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Initialize operation was cancelled");
            return ResultExtensions.Cancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize semantic search service");
            return Result.WithFailure($"Initialization failed: {ex.Message}");
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
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled();

        try
        {
            if (string.IsNullOrWhiteSpace(documentId))
                return Result.WithFailure("Document ID cannot be null or empty");

            if (string.IsNullOrWhiteSpace(content))
                return Result.WithFailure("Document content cannot be null or empty");

            _logger.LogDebug("Indexing document: {DocumentId}", documentId);

            // Generate embedding for the content
            var embeddingResult = await _embeddingGenerator.GenerateEmbeddingAsync(content, cancellationToken).ConfigureAwait(false);
            if (embeddingResult.IsFailure)
            {
                _logger.LogError("Failed to generate embedding for document {DocumentId}: {Error}",
                    documentId, embeddingResult.Error);
                return Result.WithFailure($"Embedding generation failed: {embeddingResult.Error}");
            }

            if (embeddingResult.Value is null)
            {
                _logger.LogError("Generated embedding is null for document {DocumentId}", documentId);
                return Result.WithFailure("Generated embedding cannot be null");
            }

            // Store in vector database
            var storeResult = await _vectorStore.StoreEmbeddingAsync(
                documentId, content, embeddingResult.Value, metadata, cancellationToken).ConfigureAwait(false);

            if (storeResult.IsFailure)
            {
                _logger.LogError("Failed to store embedding for document {DocumentId}: {Error}",
                    documentId, storeResult.Error);
                return Result.WithFailure($"Vector storage failed: {storeResult.Error}");
            }

            _logger.LogInformation("Successfully indexed document: {DocumentId}", documentId);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Index document operation was cancelled");
            return ResultExtensions.Cancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index document: {DocumentId}", documentId);
            return Result.WithFailure($"Document indexing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Index multiple documents in batch for efficiency
    /// </summary>
    public async Task<Result<BatchIndexingResult>> IndexDocumentsBatchAsync(
        IEnumerable<DocumentToIndex> documents,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<BatchIndexingResult>();

        try
        {
            var documentList = documents?.ToList() ?? [];

            if (!documentList.Any())
                return Result<BatchIndexingResult>.Success(new BatchIndexingResult());

            _logger.LogInformation("Starting batch indexing of {Count} documents", documentList.Count);

            // Generate embeddings for all documents
            var texts = documentList.Select(d => d.Content).ToList();
            var embeddingsResult = await _embeddingGenerator.GenerateBatchEmbeddingsAsync(texts, cancellationToken).ConfigureAwait(false);

            if (embeddingsResult.IsFailure)
            {
                _logger.LogError("Failed to generate batch embeddings: {Error}", embeddingsResult.Error);
                return Result<BatchIndexingResult>.WithFailure($"Batch embedding generation failed: {embeddingsResult.Error}");
            }

            if (embeddingsResult.Value is null)
            {
                _logger.LogError("Generated batch embeddings result is null");
                return Result<BatchIndexingResult>.WithFailure("Generated batch embeddings cannot be null");
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
                    Metadata = doc.Metadata ?? []
                });
            }

            // Store in vector database
            var storeResult = await _vectorStore.StoreBatchAsync(vectorItems, cancellationToken).ConfigureAwait(false);

            var batchResult = new BatchIndexingResult
            {
                TotalDocuments = documentList.Count,
                SuccessfullyIndexed = storeResult.IsSuccess ? documentList.Count : 0,
                Failed = storeResult.IsSuccess ? 0 : documentList.Count,
                Errors = storeResult.IsFailure ? new[] { storeResult.Error ?? "Unknown error" } : Array.Empty<string>()
            };

            if (storeResult.IsFailure)
            {
                _logger.LogError("Failed to store batch embeddings: {Error}", storeResult.Error);
                return Result<BatchIndexingResult>.WithFailure($"Batch vector storage failed: {storeResult.Error}");
            }

            _logger.LogInformation("Successfully indexed {Count} documents in batch", documentList.Count);
            return Result<BatchIndexingResult>.Success(batchResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Index documents batch operation was cancelled");
            return ResultExtensions.Cancelled<BatchIndexingResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index documents in batch");
            return Result<BatchIndexingResult>.WithFailure($"Batch indexing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Search for documents semantically similar to the query
    /// </summary>
    public async Task<Result<SemanticSearchResults>> SearchAsync(
        string query,
        int maxResults = 10,
        float similarityThreshold = 0.7f,
        Dictionary<string, object>? filter = null,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<SemanticSearchResults>();

        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return Result<SemanticSearchResults>.WithFailure("Query cannot be null or empty");

            _logger.LogDebug("Performing semantic search for query: {Query}", query);

            // Generate embedding for the query
            var queryEmbeddingResult = await _embeddingGenerator.GenerateEmbeddingAsync(query, cancellationToken).ConfigureAwait(false);
            if (queryEmbeddingResult.IsFailure)
            {
                _logger.LogError("Failed to generate query embedding: {Error}", queryEmbeddingResult.Error);
                return Result<SemanticSearchResults>.WithFailure($"Query embedding failed: {queryEmbeddingResult.Error}");
            }

            if (queryEmbeddingResult.Value is null)
            {
                _logger.LogError("Generated query embedding is null");
                return Result<SemanticSearchResults>.WithFailure("Generated query embedding cannot be null");
            }

            // Search vector store
            var searchResult = await _vectorStore.SearchSimilarAsync(
                queryEmbeddingResult.Value, maxResults, similarityThreshold, filter, cancellationToken).ConfigureAwait(false);

            if (searchResult.IsFailure)
            {
                _logger.LogError("Vector search failed: {Error}", searchResult.Error);
                return Result<SemanticSearchResults>.WithFailure($"Vector search failed: {searchResult.Error}");
            }

            if (searchResult.Value is null)
            {
                _logger.LogError("Vector search returned null results");
                return Result<SemanticSearchResults>.WithFailure("Vector search returned null results");
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
            return Result<SemanticSearchResults>.Success(results);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Search operation was cancelled");
            return ResultExtensions.Cancelled<SemanticSearchResults>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform semantic search");
            return Result<SemanticSearchResults>.WithFailure($"Semantic search failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Remove document from semantic search index
    /// </summary>
    public async Task<Result> RemoveDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled();

        try
        {
            if (string.IsNullOrWhiteSpace(documentId))
                return Result.WithFailure("Document ID cannot be null or empty");

            _logger.LogDebug("Removing document from index: {DocumentId}", documentId);

            var result = await _vectorStore.DeleteEmbeddingAsync(documentId, cancellationToken).ConfigureAwait(false);

            if (result.IsSuccess)
                _logger.LogInformation("Successfully removed document: {DocumentId}", documentId);
            else
                _logger.LogError("Failed to remove document {DocumentId}: {Error}", documentId, result.Error ?? "Unknown error");

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Remove document operation was cancelled");
            return ResultExtensions.Cancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove document: {DocumentId}", documentId);
            return Result.WithFailure($"Document removal failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get statistics about the semantic search index
    /// </summary>
    public async Task<Result<VectorStoreStats>> GetIndexStatsAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<VectorStoreStats>();

        try
        {
            var result = await _vectorStore.GetStatsAsync(cancellationToken).ConfigureAwait(false);

            if (result.IsSuccess && result.Value is not null)
                _logger.LogDebug("Retrieved index stats: {TotalVectors} vectors", result.Value.TotalVectors);
            else
                _logger.LogError("Failed to get index stats: {Error}", result.Error);

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get index stats operation was cancelled");
            return ResultExtensions.Cancelled<VectorStoreStats>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get index statistics");
            return Result<VectorStoreStats>.WithFailure($"Stats retrieval failed: {ex.Message}");
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
    public Dictionary<string, object> Metadata { get; set; } = [];
}

/// <summary>
/// Results from semantic search
/// </summary>
public class SemanticSearchResults
{
    public string Query { get; set; } = string.Empty;
    public List<SemanticSearchResult> Results { get; set; } = [];
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
    public Dictionary<string, object> Metadata { get; set; } = [];
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