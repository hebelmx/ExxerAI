using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Operations;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace ExxerAI.Infrastructure.GraphStore;

/// <summary>
/// Neo4j implementation of graph knowledge store
/// Manages document-concept relationships and knowledge discovery
/// </summary>
public class Neo4jGraphKnowledgeStore : IGraphKnowledgeStore
{
    private readonly IGraphClient _graphClient;
    private readonly ILogger<Neo4jGraphKnowledgeStore> _logger;
    
    private static readonly object _initLock = new();
    private bool _isInitialized = false;

    public Neo4jGraphKnowledgeStore(
        IGraphClient graphClient,
        ILogger<Neo4jGraphKnowledgeStore> logger)
    {
        _graphClient = graphClient ?? throw new ArgumentNullException(nameof(graphClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initialize Neo4j database with constraints and indexes
    /// </summary>
    public async Task<Result> InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_initLock)
            {
                if (_isInitialized) return Result.Success();
            }

            _logger.LogInformation("Initializing Neo4j graph knowledge store");

            // Connect to Neo4j if not already connected
            if (!_graphClient.IsConnected)
            {
                await _graphClient.ConnectAsync();
                _logger.LogInformation("Connected to Neo4j database");
            }

            // Create unique constraints
            await CreateConstraintsAsync(cancellationToken);
            
            // Create indexes for performance
            await CreateIndexesAsync(cancellationToken);

            lock (_initLock)
            {
                _isInitialized = true;
            }

            _logger.LogInformation("Neo4j graph knowledge store initialized successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Neo4j graph knowledge store");
            return Result.Failure($"Graph store initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Store document in the knowledge graph
    /// </summary>
    public async Task<Result> StoreDocumentAsync(GraphDocument document, CancellationToken cancellationToken = default)
    {
        try
        {
            if (document == null) return Result.Failure("Document cannot be null");
            if (string.IsNullOrWhiteSpace(document.DocumentId)) return Result.Failure("Document ID cannot be empty");

            await EnsureInitializedAsync(cancellationToken);

            _logger.LogDebug("Storing document in graph: {DocumentId}", document.DocumentId);

            var query = _graphClient.Cypher
                .Merge("(d:Document {documentId: $documentId})")
                .Set("d.title = $title")
                .Set("d.content = $content")
                .Set("d.documentType = $documentType")
                .Set("d.createdAt = $createdAt")
                .Set("d.modifiedAt = $modifiedAt")
                .Set("d.tags = $tags")
                .WithParams(new
                {
                    documentId = document.DocumentId,
                    title = document.Title,
                    content = document.Content,
                    documentType = document.DocumentType,
                    createdAt = document.CreatedAt.ToString("O"),
                    modifiedAt = document.ModifiedAt.ToString("O"),
                    tags = document.Tags.ToArray()
                });

            // Add custom properties
            foreach (var prop in document.Properties)
            {
                query = query.Set($"d.{SanitizePropertyName(prop.Key)} = ${prop.Key}")
                    .WithParam(prop.Key, prop.Value);
            }

            await query.ExecuteWithoutResultsAsync();

            _logger.LogDebug("Successfully stored document: {DocumentId}", document.DocumentId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store document: {DocumentId}", document?.DocumentId);
            return Result.Failure($"Failed to store document: {ex.Message}");
        }
    }

    /// <summary>
    /// Store concepts in the knowledge graph
    /// </summary>
    public async Task<Result> StoreConceptsAsync(IEnumerable<GraphConcept> concepts, CancellationToken cancellationToken = default)
    {
        try
        {
            var conceptList = concepts?.ToList() ?? new List<GraphConcept>();
            if (!conceptList.Any()) return Result.Success();

            await EnsureInitializedAsync(cancellationToken);

            _logger.LogDebug("Storing {Count} concepts in graph", conceptList.Count);

            foreach (var concept in conceptList)
            {
                if (string.IsNullOrWhiteSpace(concept.ConceptId)) continue;

                var query = _graphClient.Cypher
                    .Merge("(c:Concept {conceptId: $conceptId})")
                    .Set("c.name = $name")
                    .Set("c.type = $type")
                    .Set("c.description = $description")
                    .Set("c.confidence = $confidence")
                    .Set("c.aliases = $aliases")
                    .WithParams(new
                    {
                        conceptId = concept.ConceptId,
                        name = concept.Name,
                        type = concept.Type,
                        description = concept.Description,
                        confidence = concept.Confidence,
                        aliases = concept.Aliases.ToArray()
                    });

                // Add custom properties
                foreach (var prop in concept.Properties)
                {
                    query = query.Set($"c.{SanitizePropertyName(prop.Key)} = ${prop.Key}")
                        .WithParam(prop.Key, prop.Value);
                }

                await query.ExecuteWithoutResultsAsync();
            }

            _logger.LogInformation("Successfully stored {Count} concepts", conceptList.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store concepts");
            return Result.Failure($"Failed to store concepts: {ex.Message}");
        }
    }

    /// <summary>
    /// Create relationships between nodes
    /// </summary>
    public async Task<Result> CreateRelationshipsAsync(IEnumerable<GraphRelationship> relationships, CancellationToken cancellationToken = default)
    {
        try
        {
            var relationshipList = relationships?.ToList() ?? new List<GraphRelationship>();
            if (!relationshipList.Any()) return Result.Success();

            await EnsureInitializedAsync(cancellationToken);

            _logger.LogDebug("Creating {Count} relationships in graph", relationshipList.Count);

            foreach (var relationship in relationshipList)
            {
                if (string.IsNullOrWhiteSpace(relationship.FromNodeId) || 
                    string.IsNullOrWhiteSpace(relationship.ToNodeId)) continue;

                var relationshipType = SanitizeRelationshipType(relationship.RelationshipType);

                var query = _graphClient.Cypher
                    .Match("(from {nodeId: $fromId})")
                    .Match("(to {nodeId: $toId})")
                    .Merge($"(from)-[r:{relationshipType}]->(to)")
                    .Set("r.weight = $weight")
                    .Set("r.createdAt = $createdAt")
                    .WithParams(new
                    {
                        fromId = relationship.FromNodeId,
                        toId = relationship.ToNodeId,
                        weight = relationship.Weight,
                        createdAt = relationship.CreatedAt.ToString("O")
                    });

                // Add custom properties
                foreach (var prop in relationship.Properties)
                {
                    query = query.Set($"r.{SanitizePropertyName(prop.Key)} = ${prop.Key}")
                        .WithParam(prop.Key, prop.Value);
                }

                await query.ExecuteWithoutResultsAsync();
            }

            _logger.LogInformation("Successfully created {Count} relationships", relationshipList.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create relationships");
            return Result.Failure($"Failed to create relationships: {ex.Message}");
        }
    }

    /// <summary>
    /// Find documents related to a concept
    /// </summary>
    public async Task<Result<IEnumerable<GraphDocument>>> FindRelatedDocumentsAsync(
        string conceptName,
        IEnumerable<string> relationshipTypes = null,
        int maxDepth = 2,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(conceptName))
                return Result.Failure<IEnumerable<GraphDocument>>("Concept name cannot be empty");

            await EnsureInitializedAsync(cancellationToken);

            _logger.LogDebug("Finding documents related to concept: {ConceptName}", conceptName);

            var relationshipFilter = relationshipTypes?.Any() == true 
                ? $":{string.Join("|", relationshipTypes.Select(SanitizeRelationshipType))}"
                : "";

            var cypherQuery = $@"
                MATCH (c:Concept {{name: $conceptName}})
                MATCH (c)-[{relationshipFilter}*1..{maxDepth}]-(d:Document)
                RETURN DISTINCT d
                ORDER BY d.modifiedAt DESC
                LIMIT {limit}";

            var results = await _graphClient.Cypher
                .Match(cypherQuery)
                .WithParam("conceptName", conceptName)
                .Return<Neo4jDocumentResult>("d")
                .ResultsAsync;

            var documents = results.Select(r => new GraphDocument
            {
                DocumentId = r.documentId,
                Title = r.title ?? string.Empty,
                Content = r.content ?? string.Empty,
                DocumentType = r.documentType ?? string.Empty,
                CreatedAt = DateTime.TryParse(r.createdAt, out var created) ? created : DateTime.UtcNow,
                ModifiedAt = DateTime.TryParse(r.modifiedAt, out var modified) ? modified : DateTime.UtcNow,
                Tags = r.tags?.ToList() ?? new List<string>()
            });

            _logger.LogInformation("Found {Count} documents related to concept: {ConceptName}", 
                documents.Count(), conceptName);

            return Result.Success(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find related documents for concept: {ConceptName}", conceptName);
            return Result.Failure<IEnumerable<GraphDocument>>($"Failed to find related documents: {ex.Message}");
        }
    }

    /// <summary>
    /// Find concepts related to a document
    /// </summary>
    public async Task<Result<IEnumerable<GraphConcept>>> FindRelatedConceptsAsync(
        string documentId,
        IEnumerable<string> relationshipTypes = null,
        int maxDepth = 2,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentId))
                return Result.Failure<IEnumerable<GraphConcept>>("Document ID cannot be empty");

            await EnsureInitializedAsync(cancellationToken);

            _logger.LogDebug("Finding concepts related to document: {DocumentId}", documentId);

            var relationshipFilter = relationshipTypes?.Any() == true 
                ? $":{string.Join("|", relationshipTypes.Select(SanitizeRelationshipType))}"
                : "";

            var cypherQuery = $@"
                MATCH (d:Document {{documentId: $documentId}})
                MATCH (d)-[{relationshipFilter}*1..{maxDepth}]-(c:Concept)
                RETURN DISTINCT c
                ORDER BY c.confidence DESC
                LIMIT {limit}";

            var results = await _graphClient.Cypher
                .Match(cypherQuery)
                .WithParam("documentId", documentId)
                .Return<Neo4jConceptResult>("c")
                .ResultsAsync;

            var concepts = results.Select(r => new GraphConcept
            {
                ConceptId = r.conceptId,
                Name = r.name ?? string.Empty,
                Type = r.type ?? string.Empty,
                Description = r.description ?? string.Empty,
                Confidence = r.confidence,
                Aliases = r.aliases?.ToList() ?? new List<string>()
            });

            _logger.LogInformation("Found {Count} concepts related to document: {DocumentId}", 
                concepts.Count(), documentId);

            return Result.Success(concepts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find related concepts for document: {DocumentId}", documentId);
            return Result.Failure<IEnumerable<GraphConcept>>($"Failed to find related concepts: {ex.Message}");
        }
    }

    /// <summary>
    /// Execute custom Cypher query
    /// </summary>
    public async Task<Result<IEnumerable<Dictionary<string, object>>>> ExecuteQueryAsync(
        string cypherQuery,
        Dictionary<string, object> parameters = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cypherQuery))
                return Result.Failure<IEnumerable<Dictionary<string, object>>>("Cypher query cannot be empty");

            await EnsureInitializedAsync(cancellationToken);

            _logger.LogDebug("Executing custom Cypher query");

            var query = _graphClient.Cypher.Match(cypherQuery);

            if (parameters?.Any() == true)
            {
                foreach (var param in parameters)
                {
                    query = query.WithParam(param.Key, param.Value);
                }
            }

            var results = await query.Return<Dictionary<string, object>>("*").ResultsAsync;

            _logger.LogDebug("Custom query returned {Count} results", results.Count());
            return Result.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute custom Cypher query");
            return Result.Failure<IEnumerable<Dictionary<string, object>>>($"Query execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Find shortest path between entities
    /// </summary>
    public async Task<Result<GraphPath>> FindShortestPathAsync(
        string fromEntityId,
        string toEntityId,
        IEnumerable<string> relationshipTypes = null,
        int maxLength = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fromEntityId) || string.IsNullOrWhiteSpace(toEntityId))
                return Result.Failure<GraphPath>("Entity IDs cannot be empty");

            await EnsureInitializedAsync(cancellationToken);

            _logger.LogDebug("Finding shortest path from {From} to {To}", fromEntityId, toEntityId);

            var relationshipFilter = relationshipTypes?.Any() == true 
                ? $":{string.Join("|", relationshipTypes.Select(SanitizeRelationshipType))}"
                : "";

            var cypherQuery = $@"
                MATCH (from {{nodeId: $fromId}})
                MATCH (to {{nodeId: $toId}})
                MATCH path = shortestPath((from)-[{relationshipFilter}*1..{maxLength}]-(to))
                RETURN path";

            var results = await _graphClient.Cypher
                .Match(cypherQuery)
                .WithParams(new { fromId = fromEntityId, toId = toEntityId })
                .Return<IPath>("path")
                .ResultsAsync;

            var path = results.FirstOrDefault();
            if (path == null)
            {
                return Result.Success(new GraphPath { Length = -1 }); // No path found
            }

            var graphPath = ConvertToGraphPath(path);
            
            _logger.LogInformation("Found shortest path of length {Length} from {From} to {To}", 
                graphPath.Length, fromEntityId, toEntityId);

            return Result.Success(graphPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find shortest path from {From} to {To}", fromEntityId, toEntityId);
            return Result.Failure<GraphPath>($"Shortest path search failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get graph statistics
    /// </summary>
    public async Task<Result<GraphKnowledgeStats>> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureInitializedAsync(cancellationToken);

            _logger.LogDebug("Retrieving graph statistics");

            var nodeCountQuery = await _graphClient.Cypher
                .Match("(n)")
                .Return(n => n.Count())
                .ResultsAsync;

            var relationshipCountQuery = await _graphClient.Cypher
                .Match("()-[r]->()")
                .Return(r => r.Count())
                .ResultsAsync;

            var documentCountQuery = await _graphClient.Cypher
                .Match("(d:Document)")
                .Return(d => d.Count())
                .ResultsAsync;

            var conceptCountQuery = await _graphClient.Cypher
                .Match("(c:Concept)")
                .Return(c => c.Count())
                .ResultsAsync;

            var stats = new GraphKnowledgeStats
            {
                TotalNodes = nodeCountQuery.FirstOrDefault(),
                TotalRelationships = relationshipCountQuery.FirstOrDefault(),
                DocumentNodes = documentCountQuery.FirstOrDefault(),
                ConceptNodes = conceptCountQuery.FirstOrDefault(),
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogInformation("Graph stats: {TotalNodes} nodes, {TotalRelationships} relationships", 
                stats.TotalNodes, stats.TotalRelationships);

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get graph statistics");
            return Result.Failure<GraphKnowledgeStats>($"Stats retrieval failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete document and its relationships
    /// </summary>
    public async Task<Result> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentId))
                return Result.Failure("Document ID cannot be empty");

            await EnsureInitializedAsync(cancellationToken);

            _logger.LogDebug("Deleting document from graph: {DocumentId}", documentId);

            await _graphClient.Cypher
                .Match("(d:Document {documentId: $documentId})")
                .DetachDelete("d")
                .WithParam("documentId", documentId)
                .ExecuteWithoutResultsAsync();

            _logger.LogInformation("Successfully deleted document: {DocumentId}", documentId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document: {DocumentId}", documentId);
            return Result.Failure($"Failed to delete document: {ex.Message}");
        }
    }

    /// <summary>
    /// Batch store operation
    /// </summary>
    public async Task<Result> StoreBatchAsync(
        IEnumerable<GraphDocument> documents,
        IEnumerable<GraphConcept> concepts,
        IEnumerable<GraphRelationship> relationships,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureInitializedAsync(cancellationToken);

            _logger.LogInformation("Starting batch store operation");

            // Store documents
            if (documents?.Any() == true)
            {
                foreach (var doc in documents)
                {
                    var docResult = await StoreDocumentAsync(doc, cancellationToken);
                    if (docResult.IsFailure)
                        return docResult;
                }
            }

            // Store concepts
            if (concepts?.Any() == true)
            {
                var conceptResult = await StoreConceptsAsync(concepts, cancellationToken);
                if (conceptResult.IsFailure)
                    return conceptResult;
            }

            // Create relationships
            if (relationships?.Any() == true)
            {
                var relationshipResult = await CreateRelationshipsAsync(relationships, cancellationToken);
                if (relationshipResult.IsFailure)
                    return relationshipResult;
            }

            _logger.LogInformation("Batch store operation completed successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete batch store operation");
            return Result.Failure($"Batch store failed: {ex.Message}");
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (!_isInitialized)
        {
            var result = await InitializeAsync(cancellationToken);
            if (result.IsFailure)
                throw new InvalidOperationException($"Graph store initialization failed: {result.Error}");
        }
    }

    private async Task CreateConstraintsAsync(CancellationToken cancellationToken)
    {
        var constraints = new[]
        {
            "CREATE CONSTRAINT document_id_unique IF NOT EXISTS FOR (d:Document) REQUIRE d.documentId IS UNIQUE",
            "CREATE CONSTRAINT concept_id_unique IF NOT EXISTS FOR (c:Concept) REQUIRE c.conceptId IS UNIQUE"
        };

        foreach (var constraint in constraints)
        {
            try
            {
                await _graphClient.Cypher.Match(constraint).ExecuteWithoutResultsAsync();
                _logger.LogDebug("Created constraint: {Constraint}", constraint);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create constraint: {Constraint}", constraint);
            }
        }
    }

    private async Task CreateIndexesAsync(CancellationToken cancellationToken)
    {
        var indexes = new[]
        {
            "CREATE INDEX document_title_index IF NOT EXISTS FOR (d:Document) ON (d.title)",
            "CREATE INDEX concept_name_index IF NOT EXISTS FOR (c:Concept) ON (c.name)",
            "CREATE INDEX concept_type_index IF NOT EXISTS FOR (c:Concept) ON (c.type)"
        };

        foreach (var index in indexes)
        {
            try
            {
                await _graphClient.Cypher.Match(index).ExecuteWithoutResultsAsync();
                _logger.LogDebug("Created index: {Index}", index);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create index: {Index}", index);
            }
        }
    }

    private static string SanitizePropertyName(string propertyName)
    {
        return propertyName?.Replace("-", "_").Replace(" ", "_") ?? "unknown";
    }

    private static string SanitizeRelationshipType(string relationshipType)
    {
        return relationshipType?.ToUpperInvariant().Replace("-", "_").Replace(" ", "_") ?? "RELATED_TO";
    }

    private static GraphPath ConvertToGraphPath(IPath path)
    {
        // Note: This is a simplified conversion
        // In a real implementation, you'd extract nodes and relationships from the Neo4j path
        return new GraphPath
        {
            Length = path?.Length ?? 0,
            Nodes = new List<GraphNode>(),
            Relationships = new List<GraphRelationship>(),
            TotalWeight = 0
        };
    }
}

// Helper classes for Neo4j result mapping
internal class Neo4jDocumentResult
{
    public string documentId { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
    public string documentType { get; set; } = string.Empty;
    public string createdAt { get; set; } = string.Empty;
    public string modifiedAt { get; set; } = string.Empty;
    public string[] tags { get; set; } = Array.Empty<string>();
}

internal class Neo4jConceptResult
{
    public string conceptId { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public float confidence { get; set; } = 1.0f;
    public string[] aliases { get; set; } = Array.Empty<string>();
}