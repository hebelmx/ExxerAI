using Microsoft.Extensions.Logging;
using Qdrant.Client;
using ExxerAI.Infrastructure.Services;

namespace ExxerAI.IntegrationTests.Fixtures.ContainerFixtures;

/// <summary>
/// Test fixture for Qdrant vector database - connects to persistent container.
/// Assumes Qdrant container is already running via docker-compose.
/// </summary>
public class QdrantContainerFixture : IAsyncLifetime
{
    private readonly ILogger<QdrantContainerFixture> _logger;
    private QdrantClient? _qdrantClient;

    // Fixed connection details for persistent container
    public virtual string QdrantUrl { get; protected set; } = "http://localhost:6333";
    public virtual int QdrantPort { get; protected set; } = 6333;
    public virtual int QdrantGrpcPort { get; protected set; } = 6334;
    public virtual bool IsAvailable { get; protected set; }

    public QdrantContainerFixture()
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<QdrantContainerFixture>();
    }

    public async ValueTask InitializeAsync()
    {
        try
        {
            _logger.LogInformation("🔗 Connecting to persistent Qdrant container...");
            _logger.LogInformation("📍 Qdrant HTTP: {QdrantUrl}", QdrantUrl);
            _logger.LogInformation("📍 Qdrant gRPC: localhost:{QdrantGrpcPort}", QdrantGrpcPort);

            // Try to connect to existing container
            await VerifyQdrantHealthAsync();

            // Initialize Qdrant client using the SAME pattern that works in production code
            _logger.LogInformation("🔄 Using production-tested QdrantClient configuration...");
            try
            {
                // Use the exact same constructor pattern from VectorStoreServiceCollectionExtensions.cs
                _qdrantClient = new QdrantClient(
                    host: "localhost",
                    port: QdrantPort,
                    https: false,
                    apiKey: null);
                
                // Test connection by listing collections  
                await _qdrantClient.ListCollectionsAsync();
                _logger.LogInformation("✅ Qdrant client connected successfully using production pattern");
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Qdrant connection failed: {Error}", ex.Message);
                _logger.LogInformation("ℹ️ Skipping Qdrant container - service will be marked as unavailable");
                _qdrantClient = null;
                IsAvailable = false;
                return; // Don't throw - just mark as unavailable
            }
            
            IsAvailable = true;
            _logger.LogInformation("✅ Connected to persistent Qdrant container successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to connect to persistent Qdrant container");
            _logger.LogWarning("💡 Make sure Qdrant container is running: .\\start-containers.ps1");
            IsAvailable = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_qdrantClient != null)
        {
            _logger.LogInformation("🔌 Disconnecting from Qdrant container...");
            _qdrantClient.Dispose();
            _logger.LogInformation("✅ Disconnected from Qdrant container");
        }
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Verify that Qdrant is responding to health checks
    /// </summary>
    private async Task VerifyQdrantHealthAsync()
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        
        var maxRetries = 3; // Reduced since container should already be running
        var retryDelay = TimeSpan.FromSeconds(2);

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await httpClient.GetAsync($"{QdrantUrl}/");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Qdrant health check passed");
                    return;
                }
                
                _logger.LogWarning("⚠️ Qdrant health check failed: {StatusCode}", response.StatusCode);
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                _logger.LogDebug("🔄 Qdrant health check attempt {Attempt}/{MaxRetries} failed: {Error}", 
                    i + 1, maxRetries, ex.Message);
                await Task.Delay(retryDelay);
            }
        }

        throw new InvalidOperationException(
            $"Qdrant container is not responding after {maxRetries} attempts. " +
            "Please ensure the Qdrant container is running: .\\start-containers.ps1");
    }

    /// <summary>
    /// Ensures the container is available for testing
    /// </summary>
    public virtual void EnsureAvailable()
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException(
                "Qdrant container is not available. " +
                "Please start the persistent containers: .\\start-containers.ps1");
        }
    }

    /// <summary>
    /// Get connection configuration for Qdrant client
    /// </summary>
    public virtual QdrantConnectionConfig GetConnectionConfig()
    {
        EnsureAvailable();
        return new QdrantConnectionConfig
        {
            Host = "localhost",
            Port = QdrantPort,
            GrpcPort = QdrantGrpcPort,
            HttpUrl = QdrantUrl,
            IsSecure = false
        };
    }

    /// <summary>
    /// Get the Qdrant client instance
    /// </summary>
    public QdrantClient GetQdrantClient()
    {
        EnsureAvailable();
        return _qdrantClient ?? throw new InvalidOperationException("Qdrant client not initialized");
    }

    /// <summary>
    /// Get all collections for cleanup/monitoring
    /// </summary>
    public async Task<List<string>> GetCollectionsAsync()
    {
        EnsureAvailable();
        
        try
        {
            var collections = await _qdrantClient!.ListCollectionsAsync();
            return collections.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get collections");
            return new List<string>();
        }
    }

    /// <summary>
    /// Clean all collections for test isolation
    /// </summary>
    public virtual async Task CleanCollectionsAsync()
    {
        try
        {
            var collections = await GetCollectionsAsync();
            _logger.LogDebug("🧹 Cleaning {Count} Qdrant collections", collections.Count);
            
            foreach (var collection in collections)
            {
                try
                {
                    await _qdrantClient!.DeleteCollectionAsync(collection);
                    _logger.LogDebug("🗑️ Deleted collection: {Collection}", collection);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Failed to delete collection: {Collection}", collection);
                }
            }
            
            _logger.LogDebug("✅ Qdrant collections cleaned for test isolation");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to clean Qdrant collections");
        }
    }

    /// <summary>
    /// Get database statistics for monitoring
    /// </summary>
    public async Task<QdrantStats> GetDatabaseStatsAsync()
    {
        EnsureAvailable();
        
        try
        {
            var collections = await GetCollectionsAsync();
            var totalVectors = 0L;
            
            foreach (var collection in collections)
            {
                try
                {
                    var info = await _qdrantClient!.GetCollectionInfoAsync(collection);
                    totalVectors += (long)info.VectorsCount;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to get info for collection: {Collection}", collection);
                }
            }
            
            return new QdrantStats
            {
                CollectionCount = collections.Count,
                TotalVectors = totalVectors,
                IsHealthy = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get database stats");
            return new QdrantStats { IsHealthy = false };
        }
    }

    /// <summary>
    /// Create a test collection with specified parameters
    /// </summary>
    public virtual async Task<bool> CreateTestCollectionAsync(string collectionName, uint vectorSize = 1536)
    {
        EnsureAvailable();
        
        try
        {
            await _qdrantClient!.CreateCollectionAsync(collectionName, new Qdrant.Client.Grpc.VectorParams
            {
                Size = vectorSize,
                Distance = Qdrant.Client.Grpc.Distance.Cosine
            });
            
            _logger.LogDebug("✅ Created test collection: {Collection} (size: {Size})", collectionName, vectorSize);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create test collection: {Collection}", collectionName);
            return false;
        }
    }
}



/// <summary>
/// Qdrant database statistics
/// </summary>
public class QdrantStats
{
    public int CollectionCount { get; set; }
    public long TotalVectors { get; set; }
    public bool IsHealthy { get; set; }
}