using Qdrant.Client;
using ExxerAI.IntegrationTests.Fixtures;

namespace ExxerAI.IntegrationTests.Services;

/// <summary>
/// Service for managing Qdrant credentials and connections in integration tests
/// </summary>
public class QdrantCredentialService
{
    private readonly QdrantContainerFixture? _containerFixture;
    private readonly QdrantConnectionConfig? _staticConfig;

    /// <summary>
    /// Initialize with a container fixture (preferred for integration tests)
    /// </summary>
    public QdrantCredentialService(QdrantContainerFixture containerFixture)
    {
        _containerFixture = containerFixture;
    }

    /// <summary>
    /// Initialize with static configuration (fallback)
    /// </summary>
    public QdrantCredentialService(QdrantConnectionConfig config)
    {
        _staticConfig = config;
    }

    /// <summary>
    /// Get the connection configuration, preferring container fixture over static config
    /// </summary>
    public QdrantConnectionConfig GetConnectionConfig()
    {
        if (_containerFixture?.IsAvailable == true)
        {
            return _containerFixture.GetConnectionConfig();
        }

        if (_staticConfig != null)
        {
            return _staticConfig;
        }

        // Fallback configuration for development
        return new QdrantConnectionConfig
        {
            Host = "localhost",
            Port = 6333,
            GrpcPort = 6334,
            HttpUrl = "http://localhost:6333",
            IsSecure = false
        };
    }

    /// <summary>
    /// Create a configured QdrantClient instance
    /// </summary>
    public QdrantClient CreateQdrantClient()
    {
        var config = GetConnectionConfig();
        return new QdrantClient(config.Host, config.Port, https: config.IsSecure);
    }

    /// <summary>
    /// Check if Qdrant is available for testing
    /// </summary>
    public bool IsAvailable => _containerFixture?.IsAvailable ?? true;

    /// <summary>
    /// Ensure Qdrant is available or throw appropriate exception
    /// </summary>
    public void EnsureAvailable()
    {
        if (_containerFixture != null)
        {
            _containerFixture.EnsureAvailable();
        }
        else if (!IsAvailable)
        {
            throw new InvalidOperationException(
                "Qdrant is not available. Please start the persistent containers: .\\start-containers.ps1");
        }
    }

    /// <summary>
    /// Get connection string for logging/display purposes
    /// </summary>
    public string GetConnectionString()
    {
        var config = GetConnectionConfig();
        return $"{config.HttpUrl} (gRPC: {config.Host}:{config.GrpcPort})";
    }

    /// <summary>
    /// Clean collections for test isolation
    /// </summary>
    public async Task CleanCollectionsAsync()
    {
        if (_containerFixture != null)
        {
            await _containerFixture.CleanCollectionsAsync();
        }
    }

    /// <summary>
    /// Create a test collection with specified parameters
    /// </summary>
    public async Task<bool> CreateTestCollectionAsync(string collectionName, uint vectorSize = 1536)
    {
        if (_containerFixture != null)
        {
            return await _containerFixture.CreateTestCollectionAsync(collectionName, vectorSize);
        }

        // Fallback implementation
        try
        {
            var client = CreateQdrantClient();
            await client.CreateCollectionAsync(collectionName, new Qdrant.Client.Grpc.VectorParams
            {
                Size = vectorSize,
                Distance = Qdrant.Client.Grpc.Distance.Cosine
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
} 