using ExxerAI.Application.Interfaces;
using ExxerAI.Infrastructure.VectorStore;
using ExxerAI.IntegrationTests.Fixtures;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using NSubstitute;
using Qdrant.Client;
using Shouldly;

namespace ExxerAI.IntegrationTests.KnowledgeStore;

/// <summary>
/// Integration tests for Qdrant vector store implementation
/// Tests actual behavior against containerized Qdrant instance
/// </summary>
public class QdrantVectorStoreIntegrationTests : IClassFixture<QdrantContainerFixture>, IAsyncLifetime
{
    private readonly ILogger<QdrantVectorStore> _logger;
    private readonly string _testCollectionName;
    private readonly QdrantContainerFixture _containerFixture;
    private QdrantClient _qdrantClient = null!;
    private QdrantVectorStore _vectorStore = null!;

    public QdrantVectorStoreIntegrationTests(QdrantContainerFixture containerFixture)
    {
        _containerFixture = containerFixture ?? throw new ArgumentNullException(nameof(containerFixture));
        _logger = XUnitLogger.CreateLogger<QdrantVectorStore>();
        _testCollectionName = $"test_collection_{Guid.NewGuid():N}";
        
        _logger.LogInformation("=== QdrantVectorStoreIntegrationTests Constructor ===");
        _logger.LogInformation($"Test collection name: {_testCollectionName}");
    }

    public async ValueTask InitializeAsync()
    {
        // Container fixture handles Qdrant startup
        _logger.LogInformation("=== InitializeAsync: Setting up Qdrant client ===");
        _containerFixture.EnsureAvailable();
        SetupQdrantClient();
        await Task.CompletedTask;
        _logger.LogInformation("=== InitializeAsync completed ===");
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("=== DisposeAsync: Cleaning up Qdrant client ===");
        _qdrantClient?.Dispose();
        await Task.CompletedTask;
        _logger.LogInformation("=== DisposeAsync completed ===");
    }

    [Fact]
    public async Task QdrantVectorStore_Initialize_ShouldCreateCollectionSuccessfullyAsync()
    {
        // Arrange
        _logger.LogInformation("=== Test: QdrantVectorStore_Initialize_ShouldCreateCollectionSuccessfullyAsync ===");
        
        SetupQdrantClient();
        _logger.LogInformation("Setting up QdrantVectorStore with test collection");

        // Act
        _logger.LogInformation("Initializing QdrantVectorStore...");
        var result = await _vectorStore.InitializeAsync(_testCollectionName, 1536, CancellationToken.None);

        // Assert
        _logger.LogInformation("Validating successful initialization");
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        
        _logger.LogInformation("=== Test completed successfully ===");
    }

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Theory]
    [InlineData(512)]   // text-embedding-ada-002 alternative
    [InlineData(1536)]  // text-embedding-3-small
    [InlineData(3072)]  // text-embedding-3-large
    public async Task QdrantVectorStore_DifferentEmbeddingDimensions_ShouldHandleCorrectlyAsync(int dimensions)
    {
        // Arrange
        var customCollectionName = $"test_dim_{dimensions}_{Guid.NewGuid():N}";
        var config = _containerFixture.GetConnectionConfig();
        var customClient = new QdrantClient(config.Host, config.Port, https: config.IsSecure);
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

    [Fact]
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
        var config = _containerFixture.GetConnectionConfig();
        _qdrantClient = new QdrantClient(config.Host, config.Port, https: config.IsSecure);
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