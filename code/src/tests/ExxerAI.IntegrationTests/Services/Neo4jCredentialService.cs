using Neo4jClient;
using ExxerAI.IntegrationTests.Fixtures;

namespace ExxerAI.IntegrationTests.Services;

/// <summary>
/// Service for managing Neo4j credentials and connections in integration tests
/// </summary>
public class Neo4jCredentialService
{
    private readonly Neo4jContainerFixture? _containerFixture;
    private readonly Neo4jConnectionConfig? _staticConfig;

    /// <summary>
    /// Initialize with a container fixture (preferred for integration tests)
    /// </summary>
    public Neo4jCredentialService(Neo4jContainerFixture containerFixture)
    {
        _containerFixture = containerFixture;
    }

    /// <summary>
    /// Initialize with static configuration (fallback)
    /// </summary>
    public Neo4jCredentialService(Neo4jConnectionConfig config)
    {
        _staticConfig = config;
    }

    /// <summary>
    /// Get the connection configuration, preferring container fixture over static config
    /// </summary>
    public Neo4jConnectionConfig GetConnectionConfig()
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
        return new Neo4jConnectionConfig
        {
            BoltUri = "bolt://localhost:7688",  // Non-conflicting port
            HttpUri = "http://localhost:7475",  // Non-conflicting port  
            Username = "neo4j",
            Password = "test123456",  // Container password
            Database = "neo4j"
        };
    }

    /// <summary>
    /// Create a configured GraphClient instance
    /// </summary>
    public IGraphClient CreateGraphClient()
    {
        var config = GetConnectionConfig();
        return new GraphClient(new Uri(config.BoltUri), config.Username, config.Password);
    }

    /// <summary>
    /// Check if Neo4j is available for testing
    /// </summary>
    public bool IsAvailable => _containerFixture?.IsAvailable ?? true;

    /// <summary>
    /// Ensure Neo4j is available or throw appropriate exception
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
                "Neo4j is not available. Please start the persistent containers: .\\start-containers.ps1");
        }
    }

    /// <summary>
    /// Get connection string for logging/display purposes
    /// </summary>
    public string GetConnectionString()
    {
        var config = GetConnectionConfig();
        return $"{config.BoltUri} (user: {config.Username})";
    }
} 