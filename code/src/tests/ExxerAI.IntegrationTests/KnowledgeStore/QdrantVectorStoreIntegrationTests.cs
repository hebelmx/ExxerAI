using ExxerAI.Application.Interfaces;
using ExxerAI.Infrastructure.VectorStore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using NSubstitute;
using Qdrant.Client;
using Shouldly;

namespace ExxerAI.IntegrationTests.KnowledgeStore;

/// <summary>
/// Integration tests for Qdrant vector store implementation
/// Tests actual behavior against Qdrant instance when orchestration is ready
/// </summary>
public class QdrantVectorStoreIntegrationTests : IDisposable
{
    private readonly ILogger<QdrantVectorStore> _logger;
    private readonly string _testCollectionName;
    private QdrantClient _qdrantClient = null!;
    private QdrantVectorStore _vectorStore = null!;

    public QdrantVectorStoreIntegrationTests()
    {
        _logger = Substitute.For<ILogger<QdrantVectorStore>>();
        _testCollectionName = $"test_collection_{Guid.NewGuid():N}";
    }

    public void Dispose()
    {
        _qdrantClient?.Dispose();
    }

    [Fact(Skip = "Integration test - requires Qdrant orchestration to be ready")]
    public async Task QdrantVectorStore_Initialize_ShouldCreateCollectionSuccessfullyAsync()
    {
        // Arrange
        SetupQdrantClient();

        // Act
        var result = await _vectorStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _logger.Received().LogInformation(Arg.Is<string>(s => s.Contains("initialized successfully")));
    }

    [Fact(Skip = "Integration test - requires Qdrant orchestration to be ready")]
    public async Task QdrantVectorStore_StoreAndRetrieve_ShouldMaintainEmbeddingIntegrityAsync()
    {
        // Arrange
        SetupQdrantClient();
        await _vectorStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var documentId = "test-doc-001";
        var content = "This is a test document about machine learning and artificial intelligence.";
        var embeddings = GenerateTestEmbedding(1536);
        var metadata = new Dictionary<string, object>
        {
            ["category"] = "technology",
            ["author"] = "test-author",
            ["created_date"] = DateTime.UtcNow.ToString("O")
        };

        // Act - Store
        var storeResult = await _vectorStore.StoreEmbeddingAsync(documentId, content, embeddings, metadata, cancellationToken: TestContext.Current.CancellationToken);

        // Act - Search
        var searchResult = await _vectorStore.SearchSimilarAsync(
            embeddings, limit: 5, threshold: 0.9f, null!, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        storeResult.IsSuccess.ShouldBeTrue();
        searchResult.IsSuccess.ShouldBeTrue();

        var results = searchResult.Value!.ToList();
        results.Count.ShouldBeGreaterThan(0);
        results.First().DocumentId.ShouldBe(documentId);
        results.First().Content.ShouldBe(content);
        results.First().Score.ShouldBeGreaterThan(0.9f);
        results.First().Metadata["category"].ShouldBe("technology");
    }

    [Fact(Skip = "Integration test - requires Qdrant orchestration to be ready")]
    public async Task QdrantVectorStore_BatchOperations_ShouldHandleLargeDatasetsAsync()
    {
        // Arrange
        SetupQdrantClient();
        await _vectorStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var batchSize = 100;
        var batchItems = GenerateTestBatch(batchSize);

        // Act
        var result = await _vectorStore.StoreBatchAsync(batchItems, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Verify batch was stored
        var searchResult = await _vectorStore.SearchSimilarAsync(
            batchItems.First().Embeddings, limit: batchSize, 0.7f,
            null!, cancellationToken: TestContext.Current.CancellationToken);

        searchResult.IsSuccess.ShouldBeTrue();
        searchResult.Value!.Count().ShouldBeGreaterThan(50); // At least half should be similar
    }

    [Fact(Skip = "Integration test - requires Qdrant orchestration to be ready")]
    public async Task QdrantVectorStore_DeleteOperation_ShouldRemoveDocumentCompletelyAsync()
    {
        // Arrange
        SetupQdrantClient();
        await _vectorStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var documentId = "deletable-doc";
        var content = "Document to be deleted";
        var embeddings = GenerateTestEmbedding(1536);

        await _vectorStore.StoreEmbeddingAsync(documentId, content, embeddings, null!, cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var deleteResult = await _vectorStore.DeleteEmbeddingAsync(documentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        deleteResult.IsSuccess.ShouldBeTrue();

        // Verify deletion by searching
        var searchResult = await _vectorStore.SearchSimilarAsync(embeddings, threshold: 0.95f, cancellationToken: TestContext.Current.CancellationToken);
        searchResult.IsSuccess.ShouldBeTrue();
        searchResult.Value!.Any(r => r.DocumentId == documentId).ShouldBeFalse();
    }

    [Fact(Skip = "Integration test - requires Qdrant orchestration to be ready")]
    public async Task QdrantVectorStore_ConcurrentOperations_ShouldMaintainDataConsistencyAsync()
    {
        // Arrange
        SetupQdrantClient();
        await _vectorStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var concurrentTasks = new List<Task>();
        var documentCount = 50;

        // Act - Concurrent writes
        for (int i = 0; i < documentCount; i++)
        {
            var docId = $"concurrent-doc-{i}";
            var content = $"Concurrent document {i} with unique content";
            var embeddings = GenerateTestEmbedding(1536);

            concurrentTasks.Add(_vectorStore.StoreEmbeddingAsync(docId, content, embeddings, cancellationToken: TestContext.Current.CancellationToken));
        }

        await Task.WhenAll(concurrentTasks);

        // Assert - Verify all documents were stored
        var stats = await _vectorStore.GetStatsAsync(cancellationToken: TestContext.Current.CancellationToken);
        stats.IsSuccess.ShouldBeTrue();
        stats.Value!.TotalVectors.ShouldBeGreaterThanOrEqualTo(documentCount);
    }

    [Fact(Skip = "Integration test - requires Qdrant orchestration to be ready")]
    public async Task QdrantVectorStore_MetadataFiltering_ShouldReturnFilteredResultsAsync()
    {
        // Arrange
        SetupQdrantClient();
        await _vectorStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Store documents with different categories
        var techDoc = await StoreTestDocumentAsync("tech-doc", "Technical content", new { category = "technology" }, cancellationToken: TestContext.Current.CancellationToken);
        var scienceDoc = await StoreTestDocumentAsync("science-doc", "Scientific content", new { category = "science" }, cancellationToken: TestContext.Current.CancellationToken);

        var queryEmbedding = GenerateTestEmbedding(1536);

        // Act - Search with metadata filter
        var filteredResult = await _vectorStore.SearchSimilarAsync(
            queryEmbedding, limit: 10, threshold: 0.1f,
            filter: new Dictionary<string, object> { ["category"] = "technology" }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        filteredResult.IsSuccess.ShouldBeTrue();
        var results = filteredResult.Value!.ToList();
        results.All(r => r.Metadata.ContainsKey("meta_category") &&
                        r.Metadata["meta_category"].ToString() == "technology").ShouldBeTrue();
    }

    [Theory(Skip = "Integration test - requires Qdrant orchestration to be ready")]
    [InlineData(512)]   // text-embedding-ada-002 alternative
    [InlineData(1536)]  // text-embedding-3-small
    [InlineData(3072)]  // text-embedding-3-large
    public async Task QdrantVectorStore_DifferentEmbeddingDimensions_ShouldHandleCorrectlyAsync(int dimensions)
    {
        // Arrange
        var customCollectionName = $"test_dim_{dimensions}_{Guid.NewGuid():N}";
        var customClient = new QdrantClient("localhost", 6333, https: false);
        var customStore = new QdrantVectorStore(customClient, _logger, customCollectionName, dimensions);

        // Act
        var initResult = await customStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);
        var embeddings = GenerateTestEmbedding(dimensions);
        var storeResult = await customStore.StoreEmbeddingAsync("test-doc", "content", embeddings, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        initResult.IsSuccess.ShouldBeTrue();
        storeResult.IsSuccess.ShouldBeTrue();

        // Cleanup
        customClient.Dispose();
    }

    [Fact(Skip = "Integration test - requires Qdrant orchestration to be ready")]
    public async Task QdrantVectorStore_PerformanceTest_ShouldMeetResponseTimeRequirementsAsync()
    {
        // Arrange
        SetupQdrantClient();
        await _vectorStore.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var documentsToIndex = 1000;
        var batchItems = GenerateTestBatch(documentsToIndex);

        // Act & Assert - Batch indexing performance
        var indexingStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var batchResult = await _vectorStore.StoreBatchAsync(batchItems, cancellationToken: TestContext.Current.CancellationToken);
        indexingStopwatch.Stop();

        batchResult.IsSuccess.ShouldBeTrue();
        indexingStopwatch.ElapsedMilliseconds.ShouldBeLessThan(30000); // < 30 seconds for 1000 docs

        // Act & Assert - Search performance
        var searchStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var searchResult = await _vectorStore.SearchSimilarAsync(
            batchItems.First().Embeddings, limit: 50, cancellationToken: TestContext.Current.CancellationToken);
        searchStopwatch.Stop();

        searchResult.IsSuccess.ShouldBeTrue();
        searchStopwatch.ElapsedMilliseconds.ShouldBeLessThan(1000); // < 1 second for search
    }

    private void SetupQdrantClient()
    {
        _qdrantClient = new QdrantClient("localhost", 6333, https: false);
        _vectorStore = new QdrantVectorStore(_qdrantClient, _logger, _testCollectionName, 1536);
    }

    private async Task<bool> StoreTestDocumentAsync(string docId, string content, object metadata, CancellationToken cancellationToken)
    {
        var embeddings = GenerateTestEmbedding(1536);
        var metadataDict = ConvertToMetadataDictionary(metadata);
        var result = await _vectorStore.StoreEmbeddingAsync(docId, content, embeddings, metadataDict, cancellationToken);
        return result.IsSuccess;
    }

    private static float[] GenerateTestEmbedding(int dimensions)
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var embedding = new float[dimensions];
        for (int i = 0; i < dimensions; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2.0 - 1.0); // Range [-1, 1]
        }
        return embedding;
    }

    private static IEnumerable<VectorStoreItem> GenerateTestBatch(int count)
    {
        var items = new List<VectorStoreItem>();
        var random = new Random(42);

        for (int i = 0; i < count; i++)
        {
            items.Add(new VectorStoreItem
            {
                DocumentId = $"batch-doc-{i:D4}",
                Content = $"Batch document {i} with random content {random.Next(1000, 9999)}",
                Embeddings = GenerateTestEmbedding(1536),
                Metadata = new Dictionary<string, object>
                {
                    ["batch_id"] = "test-batch-001",
                    ["index"] = i,
                    ["category"] = i % 2 == 0 ? "even" : "odd"
                }
            });
        }

        return items;
    }

    private static Dictionary<string, object> ConvertToMetadataDictionary(object metadata)
    {
        var dict = new Dictionary<string, object>();
        foreach (var prop in metadata.GetType().GetProperties())
        {
            dict[prop.Name] ??= prop.GetValue(metadata)!;
        }
        return dict;
    }
}