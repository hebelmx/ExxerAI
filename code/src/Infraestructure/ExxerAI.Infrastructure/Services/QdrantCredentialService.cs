using Qdrant.Client;

namespace ExxerAI.Infrastructure.Services;

/// <summary>
/// Service for managing Qdrant credentials and connections
/// </summary>
public class QdrantCredentialService
{
    private readonly object? _containerFixture;
    private readonly QdrantConnectionConfig? _staticConfig;

    /// <summary>
    /// Initialize with a container fixture (for integration tests)
    /// </summary>
    public QdrantCredentialService(object containerFixture)
    {
        _containerFixture = containerFixture;
    }

    /// <summary>
    /// Initialize with static configuration
    /// </summary>
    public QdrantCredentialService(QdrantConnectionConfig config)
    {
        _staticConfig = config;
    }

    /// <summary>
    /// Initialize with default configuration (fallback)
    /// </summary>
    public QdrantCredentialService()
    {
        // Default constructor for dependency injection
    }

    /// <summary>
    /// Get the connection configuration, preferring container fixture over static config
    /// </summary>
    public QdrantConnectionConfig GetConnectionConfig()
    {
        if (_containerFixture != null)
        {
            // Use reflection to check if container fixture is available and get config
            var isAvailableProperty = _containerFixture.GetType().GetProperty("IsAvailable");
            var isAvailable = isAvailableProperty?.GetValue(_containerFixture) as bool? ?? false;
            
            if (isAvailable)
            {
                var getConfigMethod = _containerFixture.GetType().GetMethod("GetConnectionConfig");
                var config = getConfigMethod?.Invoke(_containerFixture, null) as QdrantConnectionConfig;
                if (config != null)
                {
                    return config;
                }
            }
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
    public bool IsAvailable => _containerFixture != null ? 
        (_containerFixture.GetType().GetProperty("IsAvailable")?.GetValue(_containerFixture) as bool? ?? false) : 
        true;

    /// <summary>
    /// Ensure Qdrant is available or throw appropriate exception
    /// </summary>
    public void EnsureAvailable()
    {
        if (_containerFixture != null)
        {
            var ensureAvailableMethod = _containerFixture.GetType().GetMethod("EnsureAvailable");
            ensureAvailableMethod?.Invoke(_containerFixture, null);
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
            var cleanMethod = _containerFixture.GetType().GetMethod("CleanCollectionsAsync");
            if (cleanMethod != null)
            {
                var task = cleanMethod.Invoke(_containerFixture, null) as Task;
                if (task != null)
                {
                    await task;
                }
            }
        }
    }

    /// <summary>
    /// Create a test collection with specified parameters
    /// </summary>
    public async Task<bool> CreateTestCollectionAsync(string collectionName, uint vectorSize = 1536)
    {
        if (_containerFixture != null)
        {
            var createMethod = _containerFixture.GetType().GetMethod("CreateTestCollectionAsync");
            if (createMethod != null)
            {
                var task = createMethod.Invoke(_containerFixture, new object[] { collectionName, vectorSize }) as Task<bool>;
                if (task != null)
                {
                    return await task;
                }
            }
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