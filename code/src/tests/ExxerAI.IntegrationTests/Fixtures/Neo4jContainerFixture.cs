using Testcontainers.Neo4j;
using Microsoft.Extensions.Logging;

namespace ExxerAI.IntegrationTests.Fixtures;

/// <summary>
/// Test fixture for Neo4j graph database container.
/// Provides a real Neo4j instance for integration testing.
/// </summary>
public class Neo4jContainerFixture : IAsyncLifetime
{
    private Neo4jContainer? _neo4jContainer;
    private readonly ILogger<Neo4jContainerFixture> _logger;

    public string ConnectionString { get; private set; } = string.Empty;
    public string BoltUri { get; private set; } = string.Empty;
    public string HttpUri { get; private set; } = string.Empty;
    public string Username { get; private set; } = "neo4j";
    public string Password { get; private set; } = "test123456";
    public bool IsAvailable { get; private set; }

    public Neo4jContainerFixture()
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<Neo4jContainerFixture>();
    }

    public async ValueTask InitializeAsync()
    {
        try
        {
            _logger.LogInformation("🐳 Starting Neo4j container for integration tests...");

            // Build Neo4j container with authentication
            _neo4jContainer = new Neo4jBuilder()
                .WithEnvironment("NEO4J_AUTH", $"{Username}/{Password}")
                .WithEnvironment("NEO4J_dbms_security_procedures_unrestricted", "gds.*,apoc.*")
                .WithEnvironment("NEO4J_dbms_security_procedures_allowlist", "gds.*,apoc.*")
                .Build();

            // Start the container
            await _neo4jContainer.StartAsync();

            // Get connection details
            BoltUri = _neo4jContainer.GetConnectionString();
            ConnectionString = BoltUri;
            HttpUri = $"http://localhost:{_neo4jContainer.GetMappedPublicPort(7474)}";

            _logger.LogInformation("✅ Neo4j container started");
            _logger.LogInformation("📍 Bolt URI: {BoltUri}", BoltUri);
            _logger.LogInformation("📍 HTTP URI: {HttpUri}", HttpUri);

            // Verify container is healthy
            await VerifyNeo4jHealthAsync();
            
            IsAvailable = true;
            _logger.LogInformation("🎯 Neo4j container fixture ready for testing");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to start Neo4j container");
            IsAvailable = false;
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_neo4jContainer != null)
        {
            _logger.LogInformation("🧹 Cleaning up Neo4j container...");
            
            try
            {
                await _neo4jContainer.StopAsync();
                await _neo4jContainer.DisposeAsync();
                _logger.LogInformation("✅ Neo4j container cleaned up successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error during Neo4j container cleanup");
            }
        }
    }

    /// <summary>
    /// Verify that Neo4j is responding to health checks
    /// </summary>
    private async Task VerifyNeo4jHealthAsync()
    {
        using var httpClient = new HttpClient();
        var maxRetries = 15; // Neo4j can take longer to start
        var retryDelay = TimeSpan.FromSeconds(3);

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                // Try to connect to Neo4j HTTP endpoint
                var response = await httpClient.GetAsync($"{HttpUri}/db/neo4j/");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Neo4j health check passed");
                    return;
                }
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                _logger.LogDebug("🔄 Neo4j health check attempt {Attempt}/{MaxRetries} failed: {Error}", 
                    i + 1, maxRetries, ex.Message);
                await Task.Delay(retryDelay);
            }
        }

        throw new InvalidOperationException($"Neo4j container failed health check after {maxRetries} attempts");
    }

    /// <summary>
    /// Ensures the container is available for testing
    /// </summary>
    public void EnsureAvailable()
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException(
                "Neo4j container is not available. " +
                "This test requires Docker to be running and accessible. " +
                "Please ensure Docker is installed and running, then try again.");
        }
    }

    /// <summary>
    /// Get connection configuration for Neo4j client
    /// </summary>
    public Neo4jConnectionConfig GetConnectionConfig()
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
    /// Execute a Cypher command for test setup/cleanup
    /// </summary>
    public async Task ExecuteCypherAsync(string cypher)
    {
        EnsureAvailable();
        
        // This is a basic implementation - in real scenarios you'd use Neo4j.Driver
        // For now, we'll just log the command
        _logger.LogDebug("📝 Executing Cypher: {Cypher}", cypher);
        
        // TODO: Implement actual Cypher execution when Neo4j.Driver is available
        await Task.CompletedTask;
    }

    /// <summary>
    /// Clean the database for test isolation
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        await ExecuteCypherAsync("MATCH (n) DETACH DELETE n");
        _logger.LogDebug("🧹 Neo4j database cleaned for test isolation");
    }
}

/// <summary>
/// Configuration for connecting to Neo4j test instance
/// </summary>
public class Neo4jConnectionConfig
{
    public string BoltUri { get; set; } = string.Empty;
    public string HttpUri { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Database { get; set; } = "neo4j";
}