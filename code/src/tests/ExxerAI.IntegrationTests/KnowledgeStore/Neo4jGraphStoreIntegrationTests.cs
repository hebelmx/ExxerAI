using ExxerAI.Application.Interfaces;
using ExxerAI.Infrastructure.GraphStore;
using ExxerAI.IntegrationTests.Fixtures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Neo4jClient;
using Shouldly;
using Xunit;

namespace ExxerAI.IntegrationTests.KnowledgeStore;

/// <summary>
/// Integration tests for Neo4j graph knowledge store implementation
/// Tests actual behavior against containerized Neo4j instance
/// </summary>
public class Neo4jGraphStoreIntegrationTests : IClassFixture<Neo4jContainerFixture>, IAsyncLifetime
{
    private readonly Neo4jContainerFixture _containerFixture;
    private readonly ILogger<Neo4jGraphKnowledgeStore> _logger;
    private IGraphClient _graphClient = null!;
    private Neo4jGraphKnowledgeStore _graphStore = null!;

    public Neo4jGraphStoreIntegrationTests(Neo4jContainerFixture containerFixture)
    {
        _containerFixture = containerFixture;
        _logger = Substitute.For<ILogger<Neo4jGraphKnowledgeStore>>();
    }

    public async ValueTask InitializeAsync()
    {
        // Ensure container is available before running tests
        if (_containerFixture.IsAvailable)
        {
            await _containerFixture.CleanDatabaseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _graphClient?.Dispose();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Helper method to check if Neo4j container is available for testing
    /// </summary>
    private void EnsureContainerAvailable()
    {
        if (!_containerFixture.IsAvailable)
        {
            throw new SkipException("Neo4j container not available - install Docker and try again");
        }
    }

    [Fact]
    public async Task Neo4jGraphStore_Initialize_ShouldCreateConstraintsAndIndexesAsync()
    {
        // Skip if container is not available
        EnsureContainerAvailable();

        // Arrange
        SetupGraphClient();

        // Act
        var result = await _graphStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _logger.Received().LogInformation(Arg.Is<string>(s => s.Contains("initialized successfully")));
    }

    [Fact]
    public async Task Neo4jGraphStore_StoreDocument_ShouldCreateDocumentNodeWithPropertiesAsync()
    {
        // Skip if container is not available
        EnsureContainerAvailable();

        // Arrange
        SetupGraphClient();
        await _graphStore.InitializeAsync(TestContext.Current.CancellationToken);

        var document = new GraphDocument
        {
            DocumentId = "test-doc-001",
            Title = "Test Document",
            Content = "This is a comprehensive test document about machine learning.",
            DocumentType = "research-paper",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            Tags = ["AI", "ML", "research"],
            Properties = new Dictionary<string, object>
            {
                ["author"] = "Dr. Test Author",
                ["word_count"] = 5000,
                ["language"] = "en"
            }
        };

        // Act
        var result = await _graphStore.StoreDocumentAsync(document, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify document was stored by querying
        var queryResult = await _graphStore.ExecuteQueryAsync(
            "MATCH (d:Document {documentId: $docId}) RETURN d.title as title, d.documentType as type",
            new Dictionary<string, object> { ["docId"] = document.DocumentId }, TestContext.Current.CancellationToken);

        queryResult.IsSuccess.ShouldBeTrue();
        var results = queryResult.Value!.ToList();
        results.Count.ShouldBe(1);
        results.First()["title"].ShouldBe(document.Title);
        results.First()["type"].ShouldBe(document.DocumentType);
    }

    [Fact]
    public async Task Neo4jGraphStore_StoreConcepts_ShouldCreateConceptNodesWithMetadataAsync()
    {
        // Skip if container is not available
        EnsureContainerAvailable();

        // Arrange
        SetupGraphClient();
        await _graphStore.InitializeAsync(TestContext.Current.CancellationToken);

        var concepts = new List<GraphConcept>
        {
            new GraphConcept
            {
                ConceptId = "concept-ml",
                Name = "Machine Learning",
                Type = "technology",
                Description = "A subset of artificial intelligence focusing on algorithms that learn from data",
                Confidence = 0.95f,
                Aliases = ["ML", "Statistical Learning"],
                Properties = new Dictionary<string, object>
                {
                    ["domain"] = "computer_science",
                    ["complexity"] = "high"
                }
            },
            new GraphConcept
            {
                ConceptId = "concept-ai",
                Name = "Artificial Intelligence",
                Type = "technology",
                Description = "Intelligence demonstrated by machines",
                Confidence = 0.98f,
                Aliases = ["AI"]
            }
        };

        // Act
        var result = await _graphStore.StoreConceptsAsync(concepts, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify concepts were stored
        var queryResult = await _graphStore.ExecuteQueryAsync(
            "MATCH (c:Concept) WHERE c.conceptId IN [$id1, $id2] RETURN c.name as name, c.confidence as confidence",
            new Dictionary<string, object>
            {
                ["id1"] = "concept-ml",
                ["id2"] = "concept-ai"
            }, TestContext.Current.CancellationToken);

        queryResult.IsSuccess.ShouldBeTrue();
        var results = queryResult.Value!.ToList();
        results.Count.ShouldBe(2);
        results.Any(r => r["name"].ToString() == "Machine Learning").ShouldBeTrue();
        results.Any(r => r["name"].ToString() == "Artificial Intelligence").ShouldBeTrue();
    }

    [Fact]
    public async Task Neo4jGraphStore_CreateRelationships_ShouldLinkDocumentsAndConceptsAsync()
    {
        // Skip if container is not available
        EnsureContainerAvailable();

        // Arrange
        SetupGraphClient();
        await _graphStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        // First create document and concept
        var document = CreateTestDocument("rel-doc-001", "Document about AI");
        var concept = CreateTestConcept("rel-concept-ai", "Artificial Intelligence");

        await _graphStore.StoreDocumentAsync(document, cancellationToken: TestContext.Current.CancellationToken);
        await _graphStore.StoreConceptsAsync(new[] { concept }, TestContext.Current.CancellationToken);

        var relationships = new List<GraphRelationship>
        {
            new GraphRelationship
            {
                FromNodeId = document.DocumentId,
                ToNodeId = concept.ConceptId,
                RelationshipType = "DISCUSSES",
                Weight = 0.8f,
                Properties = new Dictionary<string, object>
                {
                    ["mentions"] = 15,
                    ["context"] = "primary_topic"
                }
            }
        };

        // Act
        var result = await _graphStore.CreateRelationshipsAsync(relationships, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify relationship was created
        var queryResult = await _graphStore.ExecuteQueryAsync(
            "MATCH (d:Document {documentId: $docId})-[r:DISCUSSES]->(c:Concept {conceptId: $conceptId}) RETURN r.weight as weight",
            new Dictionary<string, object>
            {
                ["docId"] = document.DocumentId,
                ["conceptId"] = concept.ConceptId
            }, TestContext.Current.CancellationToken);

        queryResult.IsSuccess.ShouldBeTrue();
        var results = queryResult.Value!.ToList();
        results.Count.ShouldBe(1);
        Convert.ToSingle(results.First()["weight"]).ShouldBe(0.8f, tolerance: 0.01f);
    }

    [Fact]
    public async Task Neo4jGraphStore_FindRelatedDocuments_ShouldTraverseGraphCorrectlyAsync()
    {
        // Skip if container is not available
        EnsureContainerAvailable();

        // Arrange
        SetupGraphClient();
        await _graphStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        await SetupTestGraphDataAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _graphStore.FindRelatedDocumentsAsync(
            "Machine Learning",
            relationshipTypes: new[] { "DISCUSSES", "MENTIONS" },
            maxDepth: 2,
            limit: 10, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var documents = result.Value!.ToList();
        documents.Count.ShouldBeGreaterThan(0);
        documents.All(d => !string.IsNullOrEmpty(d.DocumentId)).ShouldBeTrue();
        documents.All(d => !string.IsNullOrEmpty(d.Content)).ShouldBeTrue();
    }

    [Fact]
    public async Task Neo4jGraphStore_FindRelatedConcepts_ShouldDiscoverConceptualConnectionsAsync()
    {
        // Skip if container is not available
        EnsureContainerAvailable();

        // Arrange
        SetupGraphClient();
        await _graphStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        await SetupTestGraphDataAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _graphStore.FindRelatedConceptsAsync(
            "ml-doc-001",
            relationshipTypes: new[] { "DISCUSSES" },
            maxDepth: 1,
            limit: 5, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var concepts = result.Value!.ToList();
        concepts.Count.ShouldBeGreaterThan(0);
        concepts.All(c => !string.IsNullOrEmpty(c.ConceptId)).ShouldBeTrue();
        concepts.All(c => !string.IsNullOrEmpty(c.Name)).ShouldBeTrue();
    }

    [Fact]
    public async Task Neo4jGraphStore_FindShortestPath_ShouldDiscoverConnectionPathsAsync()
    {
        // Skip if container is not available
        EnsureContainerAvailable();

        // Arrange
        SetupGraphClient();
        await _graphStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        await SetupTestGraphDataAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _graphStore.FindShortestPathAsync(
            "ml-doc-001",
            "ai-doc-002",
            relationshipTypes: new[] { "DISCUSSES", "RELATED_TO" },
            maxLength: 5, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var path = result.Value;

        if (path!.Length > 0) // Path exists
        {
            path!.Length.ShouldBeGreaterThan(0);
            path!.Length.ShouldBeLessThanOrEqualTo(5);
        }
        else // No path found
        {
            path.Length.ShouldBe(-1);
        }
    }

    [Fact]
    public async Task Neo4jGraphStore_BatchOperations_ShouldHandleLargeDataSetsEfficientlyAsync()
    {
        // Skip if container is not available
        EnsureContainerAvailable();

        // Arrange
        SetupGraphClient();
        await _graphStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var documents = GenerateTestDocuments(50);
        var concepts = GenerateTestConcepts(20);
        var relationships = GenerateTestRelationships(documents, concepts, 100);

        // Act
        var result = await _graphStore.StoreBatchAsync(documents, concepts, relationships, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify data was stored
        var stats = await _graphStore.GetStatsAsync(cancellationToken: TestContext.Current.CancellationToken);
        stats.IsSuccess.ShouldBeTrue();
        stats.Value!.DocumentNodes.ShouldBeGreaterThanOrEqualTo(50);
        stats.Value!.ConceptNodes.ShouldBeGreaterThanOrEqualTo(20);
        stats.Value!.TotalRelationships.ShouldBeGreaterThanOrEqualTo(100);
    }

    [Fact]
    public async Task Neo4jGraphStore_DeleteDocument_ShouldRemoveNodeAndRelationshipsAsync()
    {
        // Skip if container is not available
        EnsureContainerAvailable();

        // Arrange
        SetupGraphClient();
        await _graphStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var document = CreateTestDocument("deletable-doc", "Document to be deleted");
        await _graphStore.StoreDocumentAsync(document, TestContext.Current.CancellationToken);

        // Act
        var result = await _graphStore.DeleteDocumentAsync(document.DocumentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify deletion
        var queryResult = await _graphStore.ExecuteQueryAsync(
            "MATCH (d:Document {documentId: $docId}) RETURN count(d) as count",
            new Dictionary<string, object> { ["docId"] = document.DocumentId }, TestContext.Current.CancellationToken);

        queryResult.IsSuccess.ShouldBeTrue();
        var count = Convert.ToInt32(queryResult.Value!.First()["count"]);
        count.ShouldBe(0);
    }

    [Fact]
    public async Task Neo4jGraphStore_ConcurrentOperations_ShouldMaintainDataIntegrityAsync()
    {
        // Skip if container is not available
        EnsureContainerAvailable();

        // Arrange
        SetupGraphClient();
        await _graphStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var concurrentTasks = new List<Task>();
        var documentCount = 30;

        // Act - Concurrent document creation
        for (int i = 0; i < documentCount; i++)
        {
            var doc = CreateTestDocument($"concurrent-doc-{i}", $"Concurrent document {i}");
            concurrentTasks.Add(_graphStore.StoreDocumentAsync(doc, TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(concurrentTasks);

        // Assert
        var stats = await _graphStore.GetStatsAsync(cancellationToken: TestContext.Current.CancellationToken);
        stats.IsSuccess.ShouldBeTrue();
        stats.Value!.DocumentNodes.ShouldBeGreaterThanOrEqualTo(documentCount);
    }

    [Theory(Skip = "Integration test - requires Neo4j orchestration to be ready")]
    [InlineData("MATCH (d:Document) RETURN count(d) as documentCount")]
    [InlineData("MATCH (c:Concept) RETURN count(c) as conceptCount")]
    [InlineData("MATCH ()-[r]->() RETURN count(r) as relationshipCount")]
    public async Task Neo4jGraphStore_ExecuteQuery_ShouldHandleVariousCypherQueriesAsync(string cypherQuery)
    {
        // Skip if container is not available
        EnsureContainerAvailable();

        // Arrange
        SetupGraphClient();
        await _graphStore.InitializeAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _graphStore.ExecuteQueryAsync(cypherQuery, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var results = result.Value!.ToList();
        results.Count.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Neo4jGraphStore_PerformanceTest_ShouldMeetResponseTimeRequirementsAsync()
    {
        // Skip if container is not available
        EnsureContainerAvailable();

        // Arrange
        SetupGraphClient();
        await _graphStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var documents = GenerateTestDocuments(200);
        var concepts = GenerateTestConcepts(50);
        var relationships = GenerateTestRelationships(documents, concepts, 500);

        // Act & Assert - Batch storage performance
        var storageStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var batchResult = await _graphStore.StoreBatchAsync(documents, concepts, relationships, cancellationToken: TestContext.Current.CancellationToken);
        storageStopwatch.Stop();

        batchResult.IsSuccess.ShouldBeTrue();
        storageStopwatch.ElapsedMilliseconds.ShouldBeLessThan(60000); // < 60 seconds for large batch

        // Act & Assert - Query performance
        var queryStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var queryResult = await _graphStore.FindRelatedDocumentsAsync("Technology", maxDepth: 2, limit: 20, cancellationToken: TestContext.Current.CancellationToken);
        queryStopwatch.Stop();

        queryResult.IsSuccess.ShouldBeTrue();
        queryStopwatch.ElapsedMilliseconds.ShouldBeLessThan(5000); // < 5 seconds for graph traversal
    }

    private void SetupGraphClient()
    {
        _graphClient = new GraphClient(new Uri("bolt://localhost:7687"), "neo4j", "password");
        _graphStore = new Neo4jGraphKnowledgeStore(_graphClient, _logger);
    }

    private async Task SetupTestGraphDataAsync(CancellationToken cancellationToken = default)
    {
        // Create test documents
        var documents = new[]
        {
            CreateTestDocument("ml-doc-001", "Document about machine learning algorithms"),
            CreateTestDocument("ai-doc-002", "Document about artificial intelligence applications"),
            CreateTestDocument("tech-doc-003", "General technology document")
        };

        // Create test concepts
        var concepts = new[]
        {
            CreateTestConcept("concept-ml", "Machine Learning"),
            CreateTestConcept("concept-ai", "Artificial Intelligence"),
            CreateTestConcept("concept-tech", "Technology")
        };

        // Create relationships
        var relationships = new[]
        {
            new GraphRelationship
            {
                FromNodeId = "ml-doc-001",
                ToNodeId = "concept-ml",
                RelationshipType = "DISCUSSES",
                Weight = 0.9f
            },
            new GraphRelationship
            {
                FromNodeId = "ai-doc-002",
                ToNodeId = "concept-ai",
                RelationshipType = "DISCUSSES",
                Weight = 0.8f
            },
            new GraphRelationship
            {
                FromNodeId = "concept-ml",
                ToNodeId = "concept-ai",
                RelationshipType = "RELATED_TO",
                Weight = 0.7f
            }
        };

        await _graphStore.StoreBatchAsync(documents, concepts, relationships, cancellationToken: TestContext.Current.CancellationToken);

        //Simulate waiting for batch operation to complete
        await Task.Delay(0, TestContext.Current.CancellationToken);
        return;
    }

    private static GraphDocument CreateTestDocument(string id, string content)
    {
        return new GraphDocument
        {
            DocumentId = id,
            Title = $"Test Document {id}",
            Content = content,
            DocumentType = "test-document",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            Tags = ["test", "integration"]
        };
    }

    private static GraphConcept CreateTestConcept(string id, string name)
    {
        return new GraphConcept
        {
            ConceptId = id,
            Name = name,
            Type = "test-concept",
            Description = $"Test concept for {name}",
            Confidence = 0.85f
        };
    }

    private static List<GraphDocument> GenerateTestDocuments(int count)
    {
        var documents = new List<GraphDocument>();
        for (int i = 0; i < count; i++)
        {
            documents.Add(CreateTestDocument($"batch-doc-{i:D3}", $"Batch document {i} content"));
        }
        return documents;
    }

    private static List<GraphConcept> GenerateTestConcepts(int count)
    {
        var concepts = new List<GraphConcept>();
        var conceptTypes = new[] { "Technology", "Science", "Business", "Research", "Development" };

        for (int i = 0; i < count; i++)
        {
            concepts.Add(new GraphConcept
            {
                ConceptId = $"batch-concept-{i:D3}",
                Name = $"{conceptTypes[i % conceptTypes.Length]} {i}",
                Type = conceptTypes[i % conceptTypes.Length].ToLower(),
                Description = $"Generated concept {i}",
                Confidence = 0.7f + (i % 3) * 0.1f
            });
        }
        return concepts;
    }

    private static List<GraphRelationship> GenerateTestRelationships(
        List<GraphDocument> documents,
        List<GraphConcept> concepts,
        int count)
    {
        var relationships = new List<GraphRelationship>();
        var relationshipTypes = new[] { "DISCUSSES", "MENTIONS", "RELATES_TO", "REFERENCES" };
        var random = new Random(42);

        for (int i = 0; i < count; i++)
        {
            var doc = documents[random.Next(documents.Count)];
            var concept = concepts[random.Next(concepts.Count)];

            relationships.Add(new GraphRelationship
            {
                FromNodeId = doc.DocumentId,
                ToNodeId = concept.ConceptId,
                RelationshipType = relationshipTypes[i % relationshipTypes.Length],
                Weight = 0.5f + (float)(random.NextDouble() * 0.5),
                Properties = new Dictionary<string, object>
                {
                    ["generated"] = true,
                    ["batch_id"] = i
                }
            });
        }

        return relationships;
    }
}