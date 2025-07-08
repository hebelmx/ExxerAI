using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Operations;
using Microsoft.Extensions.Logging;

namespace ExxerAI.Application.Services;

/// <summary>
/// Hybrid knowledge service combining vector embeddings and graph relationships
/// Provides comprehensive document knowledge management with both semantic and structural search
/// </summary>
public class HybridKnowledgeService
{
    private readonly IVectorStore _vectorStore;
    private readonly IGraphKnowledgeStore _graphStore;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly ILogger<HybridKnowledgeService> _logger;

    public HybridKnowledgeService(
        IVectorStore vectorStore,
        IGraphKnowledgeStore graphStore,
        IEmbeddingGenerator embeddingGenerator,
        ILogger<HybridKnowledgeService> logger)
    {
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
        _graphStore = graphStore ?? throw new ArgumentNullException(nameof(graphStore));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initialize both vector and graph stores
    /// </summary>
    public async Task<Result> InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initializing hybrid knowledge service");

            // Initialize vector store
            var vectorResult = await _vectorStore.InitializeAsync(cancellationToken);
            if (vectorResult.IsFailure)
            {
                _logger.LogError("Vector store initialization failed: {Error}", vectorResult.Error);
                return vectorResult;
            }

            // Initialize graph store
            var graphResult = await _graphStore.InitializeAsync(cancellationToken);
            if (graphResult.IsFailure)
            {
                _logger.LogError("Graph store initialization failed: {Error}", graphResult.Error);
                return graphResult;
            }

            _logger.LogInformation("Hybrid knowledge service initialized successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize hybrid knowledge service");
            return Result.Failure($"Initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Store document in both vector and graph stores with extracted concepts
    /// </summary>
    public async Task<Result> StoreDocumentWithKnowledgeAsync(
        KnowledgeDocument document,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (document == null) return Result.Failure("Document cannot be null");
            if (string.IsNullOrWhiteSpace(document.DocumentId)) return Result.Failure("Document ID cannot be empty");

            _logger.LogInformation("Storing document with knowledge: {DocumentId}", document.DocumentId);

            // Generate embeddings for vector store
            var embeddingResult = await _embeddingGenerator.GenerateEmbeddingAsync(document.Content, cancellationToken);
            if (embeddingResult.IsFailure)
            {
                _logger.LogError("Failed to generate embeddings for document {DocumentId}: {Error}", 
                    document.DocumentId, embeddingResult.Error);
                return Result.Failure($"Embedding generation failed: {embeddingResult.Error}");
            }

            // Store in vector database
            var vectorStoreResult = await _vectorStore.StoreEmbeddingAsync(
                document.DocumentId,
                document.Content,
                embeddingResult.Value,
                document.Metadata,
                cancellationToken);

            if (vectorStoreResult.IsFailure)
            {
                _logger.LogError("Failed to store document in vector store: {Error}", vectorStoreResult.Error);
                return vectorStoreResult;
            }

            // Convert to graph document
            var graphDocument = new GraphDocument
            {
                DocumentId = document.DocumentId,
                Title = document.Title,
                Content = document.Content,
                DocumentType = document.DocumentType,
                CreatedAt = document.CreatedAt,
                ModifiedAt = document.ModifiedAt,
                Properties = document.Metadata,
                Tags = document.Tags
            };

            // Store in graph database
            var graphStoreResult = await _graphStore.StoreDocumentAsync(graphDocument, cancellationToken);
            if (graphStoreResult.IsFailure)
            {
                _logger.LogError("Failed to store document in graph store: {Error}", graphStoreResult.Error);
                return graphStoreResult;
            }

            // Store concepts if provided
            if (document.ExtractedConcepts?.Any() == true)
            {
                var conceptsResult = await _graphStore.StoreConceptsAsync(document.ExtractedConcepts, cancellationToken);
                if (conceptsResult.IsFailure)
                {
                    _logger.LogWarning("Failed to store concepts for document {DocumentId}: {Error}", 
                        document.DocumentId, conceptsResult.Error);
                }

                // Create relationships between document and concepts
                var relationships = document.ExtractedConcepts.Select(concept => new GraphRelationship
                {
                    FromNodeId = document.DocumentId,
                    ToNodeId = concept.ConceptId,
                    RelationshipType = "CONTAINS_CONCEPT",
                    Weight = concept.Confidence,
                    Properties = new Dictionary<string, object>
                    {
                        ["extraction_method"] = "automated",
                        ["document_section"] = "content"
                    }
                });

                var relationshipsResult = await _graphStore.CreateRelationshipsAsync(relationships, cancellationToken);
                if (relationshipsResult.IsFailure)
                {
                    _logger.LogWarning("Failed to create concept relationships for document {DocumentId}: {Error}", 
                        document.DocumentId, relationshipsResult.Error);
                }
            }

            _logger.LogInformation("Successfully stored document with knowledge: {DocumentId}", document.DocumentId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store document with knowledge: {DocumentId}", document?.DocumentId);
            return Result.Failure($"Document storage failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Perform hybrid search combining semantic similarity and graph relationships
    /// </summary>
    public async Task<Result<HybridSearchResults>> SearchHybridAsync(
        string query,
        HybridSearchOptions options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return Result.Failure<HybridSearchResults>("Query cannot be empty");

            options ??= new HybridSearchOptions();

            _logger.LogInformation("Performing hybrid search for query: {Query}", query);

            var results = new HybridSearchResults { Query = query };

            // Parallel execution of vector and graph searches
            var vectorSearchTask = PerformVectorSearchAsync(query, options, cancellationToken);
            var conceptSearchTask = PerformConceptSearchAsync(query, options, cancellationToken);

            await Task.WhenAll(vectorSearchTask, conceptSearchTask);

            var vectorResults = await vectorSearchTask;
            var conceptResults = await conceptSearchTask;

            if (vectorResults.IsSuccess)
            {
                results.SemanticResults = vectorResults.Value.ToList();
                _logger.LogDebug("Vector search found {Count} results", results.SemanticResults.Count);
            }
            else
            {
                _logger.LogWarning("Vector search failed: {Error}", vectorResults.Error);
            }

            if (conceptResults.IsSuccess)
            {
                results.RelationshipResults = conceptResults.Value.ToList();
                _logger.LogDebug("Graph search found {Count} results", results.RelationshipResults.Count);
            }
            else
            {
                _logger.LogWarning("Graph search failed: {Error}", conceptResults.Error);
            }

            // Combine and rank results
            results.CombinedResults = CombineAndRankResults(results.SemanticResults, results.RelationshipResults, options);

            _logger.LogInformation("Hybrid search completed. Combined {SemanticCount} semantic + {GraphCount} graph results into {CombinedCount} final results",
                results.SemanticResults.Count, results.RelationshipResults.Count, results.CombinedResults.Count);

            return Result.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform hybrid search");
            return Result.Failure<HybridSearchResults>($"Hybrid search failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Find documents related to a specific concept through graph traversal
    /// </summary>
    public async Task<Result<IEnumerable<GraphDocument>>> ExploreConceptRelationshipsAsync(
        string conceptName,
        int maxDepth = 2,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Exploring concept relationships: {ConceptName}", conceptName);

            var result = await _graphStore.FindRelatedDocumentsAsync(
                conceptName, null, maxDepth, limit, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Found {Count} documents related to concept: {ConceptName}", 
                    result.Value.Count(), conceptName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to explore concept relationships for: {ConceptName}", conceptName);
            return Result.Failure<IEnumerable<GraphDocument>>($"Concept exploration failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get comprehensive statistics from both stores
    /// </summary>
    public async Task<Result<HybridKnowledgeStats>> GetKnowledgeStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving hybrid knowledge statistics");

            var vectorStatsTask = _vectorStore.GetStatsAsync(cancellationToken);
            var graphStatsTask = _graphStore.GetStatsAsync(cancellationToken);

            await Task.WhenAll(vectorStatsTask, graphStatsTask);

            var vectorStats = await vectorStatsTask;
            var graphStats = await graphStatsTask;

            var hybridStats = new HybridKnowledgeStats
            {
                VectorStats = vectorStats.IsSuccess ? vectorStats.Value : null,
                GraphStats = graphStats.IsSuccess ? graphStats.Value : null,
                LastUpdated = DateTime.UtcNow
            };

            return Result.Success(hybridStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hybrid knowledge statistics");
            return Result.Failure<HybridKnowledgeStats>($"Stats retrieval failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Remove document from both stores
    /// </summary>
    public async Task<Result> RemoveDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentId))
                return Result.Failure("Document ID cannot be empty");

            _logger.LogInformation("Removing document from hybrid knowledge store: {DocumentId}", documentId);

            // Remove from both stores in parallel
            var vectorTask = _vectorStore.DeleteEmbeddingAsync(documentId, cancellationToken);
            var graphTask = _graphStore.DeleteDocumentAsync(documentId, cancellationToken);

            await Task.WhenAll(vectorTask, graphTask);

            var vectorResult = await vectorTask;
            var graphResult = await graphTask;

            var errors = new List<string>();
            if (vectorResult.IsFailure) errors.Add($"Vector store: {vectorResult.Error}");
            if (graphResult.IsFailure) errors.Add($"Graph store: {graphResult.Error}");

            if (errors.Any())
            {
                var errorMessage = string.Join("; ", errors);
                _logger.LogError("Partial failure removing document {DocumentId}: {Errors}", documentId, errorMessage);
                return Result.Failure($"Partial removal failure: {errorMessage}");
            }

            _logger.LogInformation("Successfully removed document from hybrid knowledge store: {DocumentId}", documentId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove document: {DocumentId}", documentId);
            return Result.Failure($"Document removal failed: {ex.Message}");
        }
    }

    private async Task<Result<IEnumerable<VectorSearchResult>>> PerformVectorSearchAsync(
        string query,
        HybridSearchOptions options,
        CancellationToken cancellationToken)
    {
        var embeddingResult = await _embeddingGenerator.GenerateEmbeddingAsync(query, cancellationToken);
        if (embeddingResult.IsFailure)
            return Result.Failure<IEnumerable<VectorSearchResult>>(embeddingResult.Error);

        return await _vectorStore.SearchSimilarAsync(
            embeddingResult.Value,
            options.MaxSemanticResults,
            options.SemanticThreshold,
            options.VectorFilter,
            cancellationToken);
    }

    private async Task<Result<IEnumerable<GraphDocument>>> PerformConceptSearchAsync(
        string query,
        HybridSearchOptions options,
        CancellationToken cancellationToken)
    {
        // Use the query as a concept name for graph traversal
        return await _graphStore.FindRelatedDocumentsAsync(
            query,
            options.RelationshipTypes,
            options.GraphTraversalDepth,
            options.MaxGraphResults,
            cancellationToken);
    }

    private static List<HybridSearchResult> CombineAndRankResults(
        List<VectorSearchResult> semanticResults,
        List<GraphDocument> relationshipResults,
        HybridSearchOptions options)
    {
        var combined = new List<HybridSearchResult>();

        // Add semantic results
        foreach (var result in semanticResults)
        {
            combined.Add(new HybridSearchResult
            {
                DocumentId = result.DocumentId,
                Content = result.Content,
                SemanticScore = result.Score,
                GraphScore = 0,
                CombinedScore = result.Score * options.SemanticWeight,
                ResultType = "Semantic",
                Metadata = result.Metadata
            });
        }

        // Add graph results
        foreach (var result in relationshipResults)
        {
            var existing = combined.FirstOrDefault(c => c.DocumentId == result.DocumentId);
            if (existing != null)
            {
                // Boost existing result with graph evidence
                existing.GraphScore = 1.0f;
                existing.CombinedScore = (existing.SemanticScore * options.SemanticWeight) + 
                                        (1.0f * options.GraphWeight);
                existing.ResultType = "Hybrid";
            }
            else
            {
                // Add as graph-only result
                combined.Add(new HybridSearchResult
                {
                    DocumentId = result.DocumentId,
                    Content = result.Content,
                    SemanticScore = 0,
                    GraphScore = 1.0f,
                    CombinedScore = 1.0f * options.GraphWeight,
                    ResultType = "Graph",
                    Metadata = result.Properties
                });
            }
        }

        // Sort by combined score and take top results
        return combined
            .OrderByDescending(r => r.CombinedScore)
            .Take(options.MaxCombinedResults)
            .ToList();
    }
}

/// <summary>
/// Document with extracted knowledge for hybrid storage
/// </summary>
public class KnowledgeDocument
{
    public string DocumentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<GraphConcept> ExtractedConcepts { get; set; } = new();
}

/// <summary>
/// Options for hybrid search
/// </summary>
public class HybridSearchOptions
{
    public int MaxSemanticResults { get; set; } = 20;
    public int MaxGraphResults { get; set; } = 20;
    public int MaxCombinedResults { get; set; } = 10;
    public float SemanticThreshold { get; set; } = 0.7f;
    public float SemanticWeight { get; set; } = 0.7f;
    public float GraphWeight { get; set; } = 0.3f;
    public int GraphTraversalDepth { get; set; } = 2;
    public Dictionary<string, object> VectorFilter { get; set; } = new();
    public IEnumerable<string> RelationshipTypes { get; set; } = null;
}

/// <summary>
/// Results from hybrid search
/// </summary>
public class HybridSearchResults
{
    public string Query { get; set; } = string.Empty;
    public List<VectorSearchResult> SemanticResults { get; set; } = new();
    public List<GraphDocument> RelationshipResults { get; set; } = new();
    public List<HybridSearchResult> CombinedResults { get; set; } = new();
    public DateTime SearchTime { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Individual hybrid search result combining semantic and graph scores
/// </summary>
public class HybridSearchResult
{
    public string DocumentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public float SemanticScore { get; set; }
    public float GraphScore { get; set; }
    public float CombinedScore { get; set; }
    public string ResultType { get; set; } = string.Empty; // Semantic, Graph, Hybrid
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Combined statistics from both knowledge stores
/// </summary>
public class HybridKnowledgeStats
{
    public VectorStoreStats VectorStats { get; set; }
    public GraphKnowledgeStats GraphStats { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}