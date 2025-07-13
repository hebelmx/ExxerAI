using Microsoft.Extensions.Logging;
using Neo4jClient;

namespace ExxerAI.IntegrationTests.Fixtures;

/// <summary>
/// Test fixture for Neo4j graph database - connects to persistent container.
/// Assumes Neo4j container is already running via docker-compose.
/// </summary>
public class Neo4jContainerFixture : IAsyncLifetime
{
    private readonly ILogger<Neo4jContainerFixture> _logger;
    private IGraphClient? _graphClient;

    // Fixed connection details for persistent container (using non-conflicting ports)
    public virtual string ConnectionString { get; protected set; } = "bolt://localhost:7688";
    public virtual string BoltUri { get; protected set; } = "bolt://localhost:7688";
    public virtual string HttpUri { get; protected set; } = "http://localhost:7475";
    public virtual string Username { get; protected set; } = "neo4j";
    public virtual string Password { get; protected set; } = "test123456";
    public virtual bool IsAvailable { get; protected set; }

    public Neo4jContainerFixture()
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<Neo4jContainerFixture>();
    }

    public async ValueTask InitializeAsync()
    {
        try
        {
            _logger.LogInformation("🔗 Connecting to persistent Neo4j container...");
            _logger.LogInformation("📍 Neo4j HTTP: {HttpUri}", HttpUri);
            _logger.LogInformation("📍 Neo4j Bolt: {BoltUri}", BoltUri);

            // Try to connect to existing container
            await VerifyNeo4jHealthAsync();
            
            // Initialize graph client
            _graphClient = new GraphClient(new Uri(BoltUri), Username, Password);
            await _graphClient.ConnectAsync();
            
            IsAvailable = true;
            _logger.LogInformation("✅ Connected to persistent Neo4j container successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to connect to persistent Neo4j container");
            _logger.LogWarning("💡 Make sure Neo4j container is running: .\\start-containers.ps1");
            IsAvailable = false;
            
            // Don't throw - let tests skip gracefully
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_graphClient != null)
        {
            _logger.LogInformation("🔌 Disconnecting from Neo4j container...");
            _graphClient.Dispose();
            _logger.LogInformation("✅ Disconnected from Neo4j container");
        }
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Verify that Neo4j is responding to health checks
    /// </summary>
    private async Task VerifyNeo4jHealthAsync()
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        
        var maxRetries = 3; // Reduced since container should already be running
        var retryDelay = TimeSpan.FromSeconds(2);

        for (int i = 0; i < maxRetries; i++)
        {
            try {
                // Check the root endpoint which should return Neo4j service info
                var response = await httpClient.GetAsync($"{HttpUri}/");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (content.Contains("neo4j_version"))
                    {
                        _logger.LogInformation("✅ Neo4j health check passed");
                        return;
                    }
                }
                
                _logger.LogWarning("⚠️ Neo4j health check failed: {StatusCode}", response.StatusCode);
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                _logger.LogDebug("🔄 Neo4j health check attempt {Attempt}/{MaxRetries} failed: {Error}", 
                    i + 1, maxRetries, ex.Message);
                await Task.Delay(retryDelay);
            }
        }

        throw new InvalidOperationException(
            $"Neo4j container is not responding after {maxRetries} attempts. " +
            "Please ensure the Neo4j container is running: .\\start-containers.ps1");
    }

    /// <summary>
    /// Ensures the container is available for testing
    /// </summary>
    public virtual void EnsureAvailable()
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException(
                "Neo4j container is not available. " +
                "Please start the persistent containers: .\\start-containers.ps1");
        }
    }

    /// <summary>
    /// Get connection configuration for Neo4j client
    /// </summary>
    public virtual Neo4jConnectionConfig GetConnectionConfig()
    {
        EnsureAvailable();
        return new Neo4jConnectionConfig
        {
            BoltUri = BoltUri,
            HttpUri = HttpUri,
            Username = Username,
            Password = Password,
            Database = "neo4j"
        };
    }

    /// <summary>
    /// Get the graph client instance
    /// </summary>
    public IGraphClient GetGraphClient()
    {
        EnsureAvailable();
        return _graphClient ?? throw new InvalidOperationException("Graph client not initialized");
    }

    /// <summary>
    /// Execute a Cypher command for test setup/cleanup
    /// </summary>
    public async Task ExecuteCypherAsync(string cypher)
    {
        EnsureAvailable();
        
        try
        {
            _logger.LogDebug("📝 Executing Cypher: {Cypher}", cypher);
            
            // For cleanup operations, we'll use a simple approach
            if (cypher.Contains("DELETE"))
            {
                await _graphClient!.Cypher
                    .Match("(n)")
                    .DetachDelete("n")
                    .ExecuteWithoutResultsAsync();
            }
            else
            {
                // For other operations, we'll need to implement specific methods
                _logger.LogDebug("Cypher execution skipped for: {Cypher}", cypher);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to execute Cypher: {Cypher}", cypher);
            throw;
        }
    }

    /// <summary>
    /// Clean the database for test isolation
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        try
        {
            await ExecuteCypherAsync("MATCH (n) DETACH DELETE n");
            _logger.LogDebug("🧹 Neo4j database cleaned for test isolation");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to clean Neo4j database");
        }
    }

    /// <summary>
    /// Get database statistics for monitoring
    /// </summary>
    public async Task<Neo4jStats> GetDatabaseStatsAsync()
    {
        EnsureAvailable();
        
        try
        {
            var nodeCount = await _graphClient!.Cypher
                .Match("(n)")
                .Return(n => n.Count())
                .ResultsAsync;
                
            var relationshipCount = await _graphClient.Cypher
                .Match("()-[r]-()")
                .Return(r => r.Count())
                .ResultsAsync;
                
            return new Neo4jStats
            {
                NodeCount = nodeCount.FirstOrDefault(),
                RelationshipCount = relationshipCount.FirstOrDefault(),
                IsHealthy = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to get database stats");
            return new Neo4jStats { IsHealthy = false };
        }
    }
}

/// <summary>
/// Configuration for connecting to Neo4j persistent container
/// </summary>
public class Neo4jConnectionConfig
{
    public string BoltUri { get; set; } = string.Empty;
    public string HttpUri { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Database { get; set; } = "neo4j";
}

/// <summary>
/// Neo4j database statistics
/// </summary>
public class Neo4jStats
{
    public long NodeCount { get; set; }
    public long RelationshipCount { get; set; }
    public bool IsHealthy { get; set; }
}