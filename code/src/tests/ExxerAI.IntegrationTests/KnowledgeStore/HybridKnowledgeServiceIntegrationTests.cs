using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Infrastructure.Embeddings;
using ExxerAI.Infrastructure.GraphStore;
using ExxerAI.Infrastructure.VectorStore;
using ExxerAI.Domain.Operations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Neo4jClient;
using Qdrant.Client;
using Shouldly;

namespace ExxerAI.IntegrationTests.KnowledgeStore;

/// <summary>
/// Integration tests for hybrid knowledge service combining vector and graph stores
/// Tests end-to-end behavior when orchestration is ready
/// </summary>
public class HybridKnowledgeServiceIntegrationTests : IDisposable
{
    private readonly ILogger<HybridKnowledgeService> _hybridLogger;
    private readonly ILogger<QdrantVectorStore> _vectorLogger;
    private readonly ILogger<Neo4jGraphKnowledgeStore> _graphLogger;
    private readonly ILogger<OpenAIEmbeddingGenerator> _embeddingLogger;

    private QdrantClient _qdrantClient;
    private IGraphClient _neo4jClient;
    private HybridKnowledgeService _hybridService;
    private readonly string _testCollectionName;

    public HybridKnowledgeServiceIntegrationTests()
    {
        _hybridLogger = Substitute.For<ILogger<HybridKnowledgeService>>();
        _vectorLogger = Substitute.For<ILogger<QdrantVectorStore>>();
        _graphLogger = Substitute.For<ILogger<Neo4jGraphKnowledgeStore>>();
        _embeddingLogger = Substitute.For<ILogger<OpenAIEmbeddingGenerator>>();
        _testCollectionName = $"hybrid_test_{Guid.NewGuid():N}";
    }

    public void Dispose()
    {
        _qdrantClient?.Dispose();
        _neo4jClient?.Dispose();
    }

    [Fact(Skip = "Integration test - requires full orchestration (Qdrant + Neo4j + OpenAI) to be ready")]
    public async Task HybridKnowledgeService_Initialize_ShouldSetupBothStores()
    {
        // Arrange
        SetupHybridService();

        // Act
        var result = await _hybridService.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _hybridLogger.Received().LogInformation(Arg.Is<string>(s => s.Contains("initialized successfully")));
    }

    [Fact(Skip = "Integration test - requires full orchestration (Qdrant + Neo4j + OpenAI) to be ready")]
    public async Task HybridKnowledgeService_StoreDocumentWithKnowledge_ShouldIndexInBothStores()
    {
        // Arrange
        SetupHybridService();
        await _hybridService.InitializeAsync(TestContext.Current.CancellationToken);

        var knowledgeDocument = new KnowledgeDocument
        {
            DocumentId = "hybrid-doc-001",
            Title = "Machine Learning in Healthcare",
            Content = @"Machine learning is revolutionizing healthcare by enabling predictive analytics,
                       diagnostic assistance, and personalized treatment plans. AI algorithms can analyze
                       medical images, predict patient outcomes, and optimize treatment protocols.",
            DocumentType = "research-article",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            Tags = new List<string> { "healthcare", "ML", "AI", "predictive-analytics" },
            Metadata = new Dictionary<string, object>
            {
                ["category"] = "healthcare-tech",
                ["author"] = "Dr. Integration Test",
                ["word_count"] = 250,
                ["publication_year"] = 2024
            },
            ExtractedConcepts = new List<GraphConcept>
            {
                new GraphConcept
                {
                    ConceptId = "concept-ml-healthcare",
                    Name = "Machine Learning in Healthcare",
                    Type = "application_domain",
                    Description = "Application of ML techniques in medical field",
                    Confidence = 0.95f
                },
                new GraphConcept
                {
                    ConceptId = "concept-predictive-analytics",
                    Name = "Predictive Analytics",
                    Type = "technique",
                    Description = "Using data to predict future outcomes",
                    Confidence = 0.88f
                },
                new GraphConcept
                {
                    ConceptId = "concept-diagnostic-ai",
                    Name = "Diagnostic AI",
                    Type = "application",
                    Description = "AI systems that assist in medical diagnosis",
                    Confidence = 0.92f
                }
            }
        };

        // Act
        var result = await _hybridService.StoreDocumentWithKnowledgeAsync(knowledgeDocument, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify document is in both stores by performing searches
        var searchResult = await _hybridService.SearchHybridAsync(
            "machine learning healthcare",
            new HybridSearchOptions { MaxCombinedResults = 5 }, cancellationToken: TestContext.Current.CancellationToken);

        searchResult.IsSuccess.ShouldBeTrue();
        var hybridResults = searchResult.Value;

        hybridResults.CombinedResults.Any(r => r.DocumentId == knowledgeDocument.DocumentId).ShouldBeTrue();
        hybridResults.SemanticResults.Any(r => r.DocumentId == knowledgeDocument.DocumentId).ShouldBeTrue();
    }

    [Fact(Skip = "Integration test - requires full orchestration (Qdrant + Neo4j + OpenAI) to be ready")]
    public async Task HybridKnowledgeService_SearchHybrid_ShouldCombineSemanticAndGraphResults()
    {
        // Arrange
        SetupHybridService();
        await _hybridService.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Store multiple related documents
        await StoreTestKnowledgeBase();

        var searchOptions = new HybridSearchOptions
        {
            MaxSemanticResults = 10,
            MaxGraphResults = 10,
            MaxCombinedResults = 15,
            SemanticWeight = 0.6f,
            GraphWeight = 0.4f,
            SemanticThreshold = 0.7f,
            GraphTraversalDepth = 2
        };

        // Act
        var result = await _hybridService.SearchHybridAsync(
            "artificial intelligence applications",
            searchOptions, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var hybridResults = result.Value;

        // Should have results from both semantic and graph searches
        hybridResults.SemanticResults.Count.ShouldBeGreaterThan(0);
        hybridResults.RelationshipResults.Count.ShouldBeGreaterThan(0);
        hybridResults.CombinedResults.Count.ShouldBeGreaterThan(0);

        // Combined results should be ranked by combined score
        var combinedScores = hybridResults.CombinedResults.Select(r => r.CombinedScore).ToList();
        combinedScores.ShouldBe(combinedScores.OrderByDescending(s => s));

        // Should have different result types
        var resultTypes = hybridResults.CombinedResults.Select(r => r.ResultType).Distinct().ToList();
        resultTypes.Count.ShouldBeGreaterThan(0);
    }

    [Fact(Skip = "Integration test - requires full orchestration (Qdrant + Neo4j + OpenAI) to be ready")]
    public async Task HybridKnowledgeService_ExploreConceptRelationships_ShouldTraverseKnowledgeGraph()
    {
        // Arrange
        SetupHybridService();
        await _hybridService.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);
        await StoreTestKnowledgeBase();

        // Act
        var result = await _hybridService.ExploreConceptRelationshipsAsync(
            "Machine Learning",
            maxDepth: 3,
            limit: 20, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var relatedDocuments = result.Value!.ToList();

        relatedDocuments.Count.ShouldBeGreaterThan(0);
        relatedDocuments.All(d => !string.IsNullOrEmpty(d.DocumentId)).ShouldBeTrue();
        relatedDocuments.All(d => !string.IsNullOrEmpty(d.Content)).ShouldBeTrue();

        // Should find documents that discuss ML concepts
        relatedDocuments.Any(d => d.Content.Contains("machine learning") ||
                                 d.Content.Contains("artificial intelligence")).ShouldBeTrue();
    }

    [Fact(Skip = "Integration test - requires full orchestration (Qdrant + Neo4j + OpenAI) to be ready")]
    public async Task HybridKnowledgeService_GetKnowledgeStats_ShouldReturnCombinedStatistics()
    {
        // Arrange
        SetupHybridService();
        await _hybridService.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);
        await StoreTestKnowledgeBase();

        // Act
        var result = await _hybridService.GetKnowledgeStatsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var stats = result.Value;

        stats.VectorStats.ShouldNotBeNull();
        stats.GraphStats.ShouldNotBeNull();
        stats.VectorStats.TotalVectors.ShouldBeGreaterThan(0);
        stats.GraphStats.TotalNodes.ShouldBeGreaterThan(0);
        stats.GraphStats.TotalRelationships.ShouldBeGreaterThan(0);
    }

    [Fact(Skip = "Integration test - requires full orchestration (Qdrant + Neo4j + OpenAI) to be ready")]
    public async Task HybridKnowledgeService_RemoveDocument_ShouldDeleteFromBothStores()
    {
        // Arrange
        SetupHybridService();
        await _hybridService.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var documentId = "removable-doc-001";
        var knowledgeDocument = CreateTestKnowledgeDocument(documentId, "Document to be removed");

        await _hybridService.StoreDocumentWithKnowledgeAsync(knowledgeDocument);

        // Act
        var result = await _hybridService.RemoveDocumentAsync(documentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify removal by searching
        var searchResult = await _hybridService.SearchHybridAsync(
            knowledgeDocument.Content,
            new HybridSearchOptions { SemanticThreshold = 0.5f }, cancellationToken: TestContext.Current.CancellationToken);

        searchResult.IsSuccess.ShouldBeTrue();
        searchResult.Value!.CombinedResults.Any(r => r.DocumentId == documentId).ShouldBeFalse();
    }

    [Fact(Skip = "Integration test - requires full orchestration (Qdrant + Neo4j + OpenAI) to be ready")]
    public async Task HybridKnowledgeService_ConcurrentOperations_ShouldMaintainConsistency()
    {
        // Arrange
        SetupHybridService();
        await _hybridService.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var concurrentTasks = new List<Task>();
        var documentCount = 20;

        // Act - Concurrent document storage
        for (int i = 0; i < documentCount; i++)
        {
            var doc = CreateTestKnowledgeDocument($"concurrent-hybrid-{i}", $"Concurrent document {i} about AI technology");
            concurrentTasks.Add(_hybridService.StoreDocumentWithKnowledgeAsync(doc));
        }

        await Task.WhenAll(concurrentTasks);

        // Assert
        var stats = await _hybridService.GetKnowledgeStatsAsync(TestContext.Current.CancellationToken);
        stats.IsSuccess.ShouldBeTrue();
        stats.Value.VectorStats.TotalVectors.ShouldBeGreaterThanOrEqualTo(documentCount);
        stats.Value.GraphStats.DocumentNodes.ShouldBeGreaterThanOrEqualTo(documentCount);
    }

    [Theory(Skip = "Integration test - requires full orchestration (Qdrant + Neo4j + OpenAI) to be ready")]
    [InlineData(0.8f, 0.2f)] // Vector-heavy weighting
    [InlineData(0.5f, 0.5f)] // Balanced weighting
    [InlineData(0.3f, 0.7f)] // Graph-heavy weighting
    public async Task HybridKnowledgeService_DifferentWeightings_ShouldProduceDifferentRankings(
        float semanticWeight, float graphWeight)
    {
        // Arrange
        SetupHybridService();
        await _hybridService.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);
        await StoreTestKnowledgeBase();

        var searchOptions = new HybridSearchOptions
        {
            SemanticWeight = semanticWeight,
            GraphWeight = graphWeight,
            MaxCombinedResults = 10
        };

        // Act
        var result = await _hybridService.SearchHybridAsync("machine learning", searchOptions, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var results = result.Value!.CombinedResults;

        results.Count.ShouldBeGreaterThan(0);

        // Verify that combined scores reflect the weighting
        foreach (var hybridResult in results)
        {
            var expectedScore = (hybridResult.SemanticScore * semanticWeight) +
                               (hybridResult.GraphScore * graphWeight);
            hybridResult.CombinedScore.ShouldBe(expectedScore, tolerance: 0.01f);
        }
    }

    [Fact(Skip = "Integration test - requires full orchestration (Qdrant + Neo4j + OpenAI) to be ready")]
    public async Task HybridKnowledgeService_PerformanceTest_ShouldMeetResponseTimeRequirements()
    {
        // Arrange
        SetupHybridService();
        await _hybridService.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var documents = GenerateLargeKnowledgeBase(100);

        // Act & Assert - Batch storage performance
        var storageStopwatch = System.Diagnostics.Stopwatch.StartNew();
        foreach (var doc in documents)
        {
            await _hybridService.StoreDocumentWithKnowledgeAsync(doc, cancellationToken: TestContext.Current.CancellationToken);
        }
        storageStopwatch.Stop();

        storageStopwatch.ElapsedMilliseconds.ShouldBeLessThan(120000); // < 2 minutes for 100 docs

        // Act & Assert - Hybrid search performance
        var searchStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var searchResult = await _hybridService.SearchHybridAsync(
            "artificial intelligence machine learning",
            new HybridSearchOptions { MaxCombinedResults = 20 }, cancellationToken: TestContext.Current.CancellationToken);
        searchStopwatch.Stop();

        searchResult.IsSuccess.ShouldBeTrue();
        searchStopwatch.ElapsedMilliseconds.ShouldBeLessThan(3000); // < 3 seconds for hybrid search
    }

    [Fact(Skip = "Integration test - requires full orchestration (Qdrant + Neo4j + OpenAI) to be ready")]
    public async Task HybridKnowledgeService_DataConsistency_ShouldMaintainVectorGraphAlignment()
    {
        // Arrange
        SetupHybridService();
        await _hybridService.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var documents = new[]
        {
            CreateTestKnowledgeDocument("consistency-doc-1", "Document about neural networks and deep learning"),
            CreateTestKnowledgeDocument("consistency-doc-2", "Document about natural language processing"),
            CreateTestKnowledgeDocument("consistency-doc-3", "Document about computer vision applications")
        };

        // Act - Store documents
        foreach (var doc in documents)
        {
            var result = await _hybridService.StoreDocumentWithKnowledgeAsync(doc, cancellationToken: TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        // Assert - Check consistency between stores
        foreach (var doc in documents)
        {
            // Search by exact content should find the document in vector store
            var vectorSearch = await _hybridService.SearchHybridAsync(doc.Content.Substring(0, 50));
            vectorSearch.IsSuccess.ShouldBeTrue();
            vectorSearch.Value.SemanticResults.Any(r => r.DocumentId == doc.DocumentId).ShouldBeTrue();

            // Explore concepts should find the document in graph store
            if (doc.ExtractedConcepts.Any())
            {
                var conceptExploration = await _hybridService.ExploreConceptRelationshipsAsync(
                    doc.ExtractedConcepts.First().Name);
                conceptExploration.IsSuccess.ShouldBeTrue();
            }
        }
    }

    private void SetupHybridService()
    {
        // Setup Qdrant client
        _qdrantClient = new QdrantClient("localhost", 6333, https: false);
        var vectorStore = new QdrantVectorStore(_qdrantClient, _vectorLogger, _testCollectionName, 1536);

        // Setup Neo4j client
        _neo4jClient = new GraphClient(new Uri("bolt://localhost:7687"), "neo4j", "password");
        var graphStore = new Neo4jGraphKnowledgeStore(_neo4jClient, _graphLogger);

        // Setup mock embedding generator (would be real OpenAI in actual integration)
        var mockEmbeddingGenerator = Substitute.For<ExxerAI.Application.Interfaces.IEmbeddingGenerator>();
        mockEmbeddingGenerator.EmbeddingDimensions.Returns(1536);
        mockEmbeddingGenerator.ModelName.Returns("text-embedding-3-small");
        mockEmbeddingGenerator.GenerateEmbeddingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var text = callInfo.Arg<string>();
                var embedding = GenerateTestEmbedding(text, 1536);
                return Task.FromResult(Result<float[]>.Success(embedding));
            });

        // Create hybrid service
        _hybridService = new HybridKnowledgeService(vectorStore, graphStore, mockEmbeddingGenerator, _hybridLogger);
    }

    private async Task StoreTestKnowledgeBase()
    {
        var knowledgeBase = new[]
        {
            CreateTestKnowledgeDocument("kb-doc-1", "Artificial intelligence and machine learning are transforming industries"),
            CreateTestKnowledgeDocument("kb-doc-2", "Deep learning neural networks enable complex pattern recognition"),
            CreateTestKnowledgeDocument("kb-doc-3", "Natural language processing helps computers understand human language"),
            CreateTestKnowledgeDocument("kb-doc-4", "Computer vision applications in autonomous vehicles and medical imaging"),
            CreateTestKnowledgeDocument("kb-doc-5", "Predictive analytics using statistical models and machine learning algorithms")
        };

        foreach (var doc in knowledgeBase)
        {
            await _hybridService.StoreDocumentWithKnowledgeAsync(doc, cancellationToken: TestContext.Current.CancellationToken);
        }
    }

    private static KnowledgeDocument CreateTestKnowledgeDocument(string id, string content)
    {
        return new KnowledgeDocument
        {
            DocumentId = id,
            Title = $"Test Document {id}",
            Content = content,
            DocumentType = "test-article",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            Tags = new List<string> { "test", "AI", "technology" },
            Metadata = new Dictionary<string, object>
            {
                ["category"] = "test-category",
                ["author"] = "Integration Test",
                ["word_count"] = content.Length
            },
            ExtractedConcepts = new List<GraphConcept>
            {
                new GraphConcept
                {
                    ConceptId = $"concept-{id}",
                    Name = "Technology",
                    Type = "domain",
                    Description = "Technology domain concept",
                    Confidence = 0.8f
                }
            }
        };
    }

    private static List<KnowledgeDocument> GenerateLargeKnowledgeBase(int count)
    {
        var documents = new List<KnowledgeDocument>();
        var topics = new[] { "AI", "ML", "robotics", "automation", "data science", "blockchain", "cloud computing" };
        var random = new Random(42);

        for (int i = 0; i < count; i++)
        {
            var topic = topics[i % topics.Length];
            documents.Add(new KnowledgeDocument
            {
                DocumentId = $"large-kb-doc-{i:D3}",
                Title = $"Document about {topic} - {i}",
                Content = $"This is a comprehensive document about {topic} and its applications in modern technology. " +
                         $"Document number {i} covers various aspects including implementation, challenges, and future prospects.",
                DocumentType = "article",
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(30)),
                ModifiedAt = DateTime.UtcNow,
                Tags = new List<string> { topic, "technology", "research" },
                Metadata = new Dictionary<string, object>
                {
                    ["category"] = topic,
                    ["batch"] = "large-test",
                    ["index"] = i
                },
                ExtractedConcepts = new List<GraphConcept>
                {
                    new GraphConcept
                    {
                        ConceptId = $"concept-{topic}-{i}",
                        Name = topic,
                        Type = "technology",
                        Description = $"Concept related to {topic}",
                        Confidence = 0.7f + (float)(random.NextDouble() * 0.3)
                    }
                }
            });
        }

        return documents;
    }

    private static float[] GenerateTestEmbedding(string text, int dimensions)
    {
        // Simple hash-based embedding generation for testing
        var hash = text.GetHashCode();
        var random = new Random(hash);
        var embedding = new float[dimensions];

        for (int i = 0; i < dimensions; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2.0 - 1.0);
        }

        return embedding;
    }
}