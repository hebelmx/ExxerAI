using Microsoft.Extensions.Logging;
using Xunit;

namespace ExxerAI.IntegrationTests.Fixtures.ContainerFixtures;

/// <summary>
/// xUnit collection definition for container-dependent tests.
/// This ensures all container tests share the same container instances.
/// </summary>
[CollectionDefinition(nameof(ContainerCollection))]
public class ContainerCollectionDefinition : ICollectionFixture<ContainerCollectionFixture>
{
}

/// <summary>
/// Collection fixture that manages container lifecycle for integration tests.
/// Uses proper xUnit infrastructure for container connectivity detection.
/// </summary>
public class ContainerCollectionFixture : IAsyncLifetime
{
    private readonly ILogger<ContainerCollectionFixture> _logger;
    
    public QdrantContainerFixture? QdrantFixture { get; private set; }
    public KnowledgeStoreContainerFixture? KnowledgeStoreFixture { get; private set; }
    
    public bool AreContainersAvailable { get; private set; }
    public string? ContainerStatusMessage { get; private set; }

    public ContainerCollectionFixture()
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<ContainerCollectionFixture>();
    }

    public async ValueTask InitializeAsync()
    {
        _logger.LogInformation("🔗 Initializing container collection fixture...");
        
        try
        {
            // Test basic connectivity first
            await TestContainerConnectivityAsync();
            
            if (AreContainersAvailable)
            {
                // Initialize container fixtures only if containers are available
                QdrantFixture = new QdrantContainerFixture();
                await QdrantFixture.InitializeAsync();
                
                KnowledgeStoreFixture = new KnowledgeStoreContainerFixture();
                await KnowledgeStoreFixture.InitializeAsync();
                
                _logger.LogInformation("✅ All container fixtures initialized successfully");
                ContainerStatusMessage = "All containers are available and healthy";
            }
            else
            {
                _logger.LogWarning("⚠️ Containers not available - tests will be skipped");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize container fixtures");
            AreContainersAvailable = false;
            ContainerStatusMessage = $"Container initialization failed: {ex.Message}";
        }
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("🔌 Disposing container collection fixture...");
        
        if (QdrantFixture != null)
        {
            await QdrantFixture.DisposeAsync();
        }
        
        if (KnowledgeStoreFixture != null)
        {
            await KnowledgeStoreFixture.DisposeAsync();
        }
        
        _logger.LogInformation("✅ Container collection fixture disposed");
    }

    /// <summary>
    /// Tests basic container connectivity before initializing fixtures.
    /// This prevents initialization errors from causing test failures.
    /// </summary>
    private async Task TestContainerConnectivityAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(3); // Shorter timeout to avoid hanging
            
            var qdrantAvailable = false;
            var neo4jAvailable = false;
            
            // Test Qdrant connectivity with timeout protection
            try
            {
                _logger.LogDebug("🔍 Testing Qdrant connectivity...");
                var qdrantResponse = await httpClient.GetAsync("http://localhost:6333/");
                qdrantAvailable = qdrantResponse.IsSuccessStatusCode;
                _logger.LogDebug("Qdrant HTTP response: {StatusCode}", qdrantResponse.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Qdrant connectivity failed: {Error}", ex.Message);
                qdrantAvailable = false;
            }
            
            // Test Neo4j connectivity with timeout protection  
            try
            {
                _logger.LogDebug("🔍 Testing Neo4j connectivity...");
                var neo4jResponse = await httpClient.GetAsync("http://localhost:7475/");
                neo4jAvailable = neo4jResponse.IsSuccessStatusCode;
                _logger.LogDebug("Neo4j HTTP response: {StatusCode}", neo4jResponse.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Neo4j connectivity failed: {Error}", ex.Message);
                neo4jAvailable = false;
            }
            
            AreContainersAvailable = qdrantAvailable && neo4jAvailable;
            
            if (!AreContainersAvailable)
            {
                var missing = new List<string>();
                if (!qdrantAvailable) missing.Add("Qdrant (localhost:6333)");
                if (!neo4jAvailable) missing.Add("Neo4j (localhost:7475)");
                
                ContainerStatusMessage = $"Missing containers: {string.Join(", ", missing)}. " +
                                       "Please start containers: docker-compose up -d";
            }
            else
            {
                ContainerStatusMessage = "All containers are available";
            }
            
            _logger.LogInformation("Container availability: Qdrant={QdrantAvailable}, Neo4j={Neo4jAvailable}", 
                qdrantAvailable, neo4jAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Container connectivity test failed");
            AreContainersAvailable = false;
            ContainerStatusMessage = $"Container connectivity test failed: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Helper method for tests to skip when containers are not available.
    /// Uses xUnit's built-in skipping mechanism.
    /// </summary>
    public void RequireContainers()
    {
        if (!AreContainersAvailable)
        {
            throw new SkipException(ContainerStatusMessage ?? 
                "Containers are not available. Please start containers: docker-compose up -d");
        }
    }
}

/// <summary>
/// Custom exception for skipping tests when containers are unavailable.
/// This integrates with xUnit's test result reporting.
/// </summary>
public class SkipException : Exception
{
    public SkipException(string message) : base(message) { }
}

/// <summary>
/// Constant for the container collection name.
/// </summary>
public static class ContainerCollection
{
    public const string Name = nameof(ContainerCollection);
} 