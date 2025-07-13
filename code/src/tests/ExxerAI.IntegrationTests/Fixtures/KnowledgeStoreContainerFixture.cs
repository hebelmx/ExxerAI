using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ExxerAI.Infrastructure.Services;

namespace ExxerAI.IntegrationTests.Fixtures;

/// <summary>
/// Combined container fixture for complete knowledge store testing.
/// Connects to persistent Qdrant and Neo4j containers for hybrid knowledge service tests.
/// </summary>
public class KnowledgeStoreContainerFixture : IAsyncLifetime
{
    private readonly QdrantContainerFixture _qdrantFixture;
    private readonly Neo4jContainerFixture _neo4jFixture;
    private readonly ILogger<KnowledgeStoreContainerFixture> _logger;

    public IServiceProvider ServiceProvider { get; private set; } = null!;
    public IConfiguration Configuration { get; private set; } = null!;
    public bool IsFullyAvailable => _qdrantFixture.IsAvailable && _neo4jFixture.IsAvailable;

    // Service accessors
    public QdrantConnectionConfig QdrantConfig => _qdrantFixture.GetConnectionConfig();
    public Neo4jConnectionConfig Neo4jConfig => _neo4jFixture.GetConnectionConfig();

    public KnowledgeStoreContainerFixture()
    {
        _qdrantFixture = new QdrantContainerFixture();
        _neo4jFixture = new Neo4jContainerFixture();
        
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<KnowledgeStoreContainerFixture>();
    }

    public async ValueTask InitializeAsync()
    {
        _logger.LogInformation("🚀 Initializing knowledge store container orchestration...");
        _logger.LogInformation("💡 Connecting to persistent containers (no startup delay)");

        try
        {
            // Connect to containers in parallel for faster initialization
            var qdrantTask = _qdrantFixture.InitializeAsync();
            var neo4jTask = _neo4jFixture.InitializeAsync();

            await Task.WhenAll(qdrantTask.AsTask(), neo4jTask.AsTask());

            // Check availability
            if (!IsFullyAvailable)
            {
                var missing = new List<string>();
                if (!_qdrantFixture.IsAvailable) missing.Add("Qdrant");
                if (!_neo4jFixture.IsAvailable) missing.Add("Neo4j");
                
                _logger.LogWarning("⚠️ Some containers are not available: {Missing}", string.Join(", ", missing));
                _logger.LogWarning("💡 Start containers with: .\\start-containers.ps1");
            }

            // Build configuration with container connection strings
            await BuildConfigurationAsync();

            // Build service container with real services
            await BuildServiceContainerAsync();

            if (IsFullyAvailable)
            {
                _logger.LogInformation("✅ Knowledge store container orchestration ready");
                _logger.LogInformation("📊 Qdrant: {QdrantUrl}", QdrantConfig.HttpUrl);
                _logger.LogInformation("📈 Neo4j: {Neo4jUrl}", Neo4jConfig.BoltUri);
            }
            else
            {
                _logger.LogWarning("⚠️ Knowledge store partially available - some tests may be skipped");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize knowledge store containers");
            // Don't throw - let tests handle gracefully
        }
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("🧹 Cleaning up knowledge store container connections...");

        try
        {
            // Dispose service container first
            (ServiceProvider as IDisposable)?.Dispose();

            // Disconnect from containers in parallel
            var qdrantTask = _qdrantFixture.DisposeAsync();
            var neo4jTask = _neo4jFixture.DisposeAsync();

            await Task.WhenAll(qdrantTask.AsTask(), neo4jTask.AsTask());

            _logger.LogInformation("✅ Knowledge store container connections cleaned up");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error during knowledge store cleanup");
        }
    }

    /// <summary>
    /// Build configuration with container connection strings
    /// </summary>
    private async Task BuildConfigurationAsync()
    {
        var configData = new Dictionary<string, string?>();

        // Add Qdrant configuration if available
        if (_qdrantFixture.IsAvailable)
        {
            var qdrantConfig = QdrantConfig;
            configData.AddRange(new Dictionary<string, string?>
            {
                ["VectorStore:Qdrant:Host"] = qdrantConfig.Host,
                ["VectorStore:Qdrant:Port"] = qdrantConfig.Port.ToString(),
                ["VectorStore:Qdrant:GrpcPort"] = qdrantConfig.GrpcPort.ToString(),
                ["VectorStore:Qdrant:HttpUrl"] = qdrantConfig.HttpUrl,
                ["VectorStore:Qdrant:IsSecure"] = qdrantConfig.IsSecure.ToString(),
            });
        }

        // Add Neo4j configuration if available
        if (_neo4jFixture.IsAvailable)
        {
            var neo4jConfig = Neo4jConfig;
            configData.AddRange(new Dictionary<string, string?>
            {
                ["GraphStore:Neo4j:BoltUri"] = neo4jConfig.BoltUri,
                ["GraphStore:Neo4j:HttpUri"] = neo4jConfig.HttpUri,
                ["GraphStore:Neo4j:Username"] = neo4jConfig.Username,
                ["GraphStore:Neo4j:Password"] = neo4jConfig.Password,
                ["GraphStore:Neo4j:Database"] = neo4jConfig.Database,
            });
        }

        // Add common configuration
        configData.AddRange(new Dictionary<string, string?>
        {
            // LocalAI configuration
            ["LocalAI:BaseUrl"] = "http://localhost:8080",
            ["LocalAI:ApiKey"] = "test-key",
            
            // Redis configuration
            ["Redis:ConnectionString"] = "localhost:6379",
            ["Redis:Password"] = "test123456",
            
            // Hybrid configuration
            ["KnowledgeStore:Type"] = "Hybrid",
            ["KnowledgeStore:VectorWeight"] = "0.7",
            ["KnowledgeStore:GraphWeight"] = "0.3",
            
            // Test configuration
            ["Testing:ContainerMode"] = "Persistent",
            ["Testing:SkipExternalServices"] = "false",
            ["Testing:QdrantAvailable"] = _qdrantFixture.IsAvailable.ToString(),
            ["Testing:Neo4jAvailable"] = _neo4jFixture.IsAvailable.ToString(),
        });

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddEnvironmentVariables("EXXERAI_TEST_")
            .Build();

        await Task.CompletedTask;
    }

    /// <summary>
    /// Build service container with real knowledge store implementations
    /// </summary>
    private async Task BuildServiceContainerAsync()
    {
        var services = new ServiceCollection();

        // Add configuration
        services.AddSingleton(Configuration);

        // Add logging
        services.AddLogging(builder => builder
            .AddConsole()
            .SetMinimumLevel(LogLevel.Information));

        // Add connection configs
        if (_qdrantFixture.IsAvailable)
        {
            services.AddSingleton(QdrantConfig);
            services.AddSingleton(_qdrantFixture.GetQdrantClient);
        }

        if (_neo4jFixture.IsAvailable)
        {
            services.AddSingleton(Neo4jConfig);
            services.AddSingleton(_neo4jFixture.GetGraphClient);
        }

        // Add fixtures for direct access
        services.AddSingleton(_qdrantFixture);
        services.AddSingleton(_neo4jFixture);

        // Add your real knowledge store services here
        // services.AddScoped<IQdrantVectorStore, QdrantVectorStore>();
        // services.AddScoped<INeo4jGraphStore, Neo4jGraphStore>();
        // services.AddScoped<IHybridKnowledgeService, HybridKnowledgeService>();

        ServiceProvider = services.BuildServiceProvider();

        await Task.CompletedTask;
    }

    /// <summary>
    /// Ensures both containers are available for testing
    /// </summary>
    public void EnsureFullyAvailable()
    {
        if (!IsFullyAvailable)
        {
            var missingServices = new List<string>();
            if (!_qdrantFixture.IsAvailable) missingServices.Add("Qdrant");
            if (!_neo4jFixture.IsAvailable) missingServices.Add("Neo4j");

            throw new InvalidOperationException(
                $"Knowledge store containers are not fully available. Missing: {string.Join(", ", missingServices)}. " +
                "Please start the persistent containers: .\\start-containers.ps1");
        }
    }

    /// <summary>
    /// Ensures at least one container is available for testing
    /// </summary>
    public void EnsurePartiallyAvailable()
    {
        if (!_qdrantFixture.IsAvailable && !_neo4jFixture.IsAvailable)
        {
            throw new InvalidOperationException(
                "No knowledge store containers are available. " +
                "Please start the persistent containers: .\\start-containers.ps1");
        }
    }

    /// <summary>
    /// Clean both databases for test isolation
    /// </summary>
    public async Task CleanDatabasesAsync()
    {
        _logger.LogDebug("🧹 Cleaning knowledge store databases for test isolation...");

        var cleanupTasks = new List<Task>();

        if (_qdrantFixture.IsAvailable)
        {
            cleanupTasks.Add(_qdrantFixture.CleanCollectionsAsync());
        }

        if (_neo4jFixture.IsAvailable)
        {
            cleanupTasks.Add(_neo4jFixture.CleanDatabaseAsync());
        }

        try
        {
            await Task.WhenAll(cleanupTasks);
            _logger.LogDebug("✅ Knowledge store databases cleaned");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error cleaning knowledge store databases");
        }
    }

    /// <summary>
    /// Get database statistics for monitoring
    /// </summary>
    public async Task<KnowledgeStoreStats> GetDatabaseStatsAsync()
    {
        var stats = new KnowledgeStoreStats();

        if (_qdrantFixture.IsAvailable)
        {
            stats.QdrantStats = await _qdrantFixture.GetDatabaseStatsAsync();
        }

        if (_neo4jFixture.IsAvailable)
        {
            stats.Neo4jStats = await _neo4jFixture.GetDatabaseStatsAsync();
        }

        return stats;
    }

    /// <summary>
    /// Get a service from the container
    /// </summary>
    public T GetRequiredService<T>() where T : notnull
    {
        EnsurePartiallyAvailable();
        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Try to get a service from the container
    /// </summary>
    public T? GetService<T>()
    {
        return ServiceProvider.GetService<T>();
    }

    /// <summary>
    /// Execute setup operations for test scenarios
    /// </summary>
    public async Task SetupTestScenarioAsync(string scenarioName)
    {
        _logger.LogInformation("🎬 Setting up test scenario: {ScenarioName}", scenarioName);

        // Clean databases first
        await CleanDatabasesAsync();

        // Setup scenario-specific data
        switch (scenarioName.ToLowerInvariant())
        {
            case "empty":
                // Already clean, nothing to do
                break;
                
            case "sample_documents":
                await SetupSampleDocumentsAsync();
                break;
                
            case "performance_test":
                await SetupPerformanceTestDataAsync();
                break;
                
            default:
                _logger.LogWarning("⚠️ Unknown test scenario: {ScenarioName}", scenarioName);
                break;
        }

        _logger.LogInformation("✅ Test scenario ready: {ScenarioName}", scenarioName);
    }

    private async Task SetupSampleDocumentsAsync()
    {
        _logger.LogDebug("📝 Setting up sample documents...");
        
        // Create sample collections/data
        if (_qdrantFixture.IsAvailable)
        {
            await _qdrantFixture.CreateTestCollectionAsync("sample_documents", 1536);
        }
        
        // TODO: Add sample data to both stores
        await Task.CompletedTask;
    }

    private async Task SetupPerformanceTestDataAsync()
    {
        _logger.LogDebug("⚡ Setting up performance test data...");
        
        // Create performance test collections
        if (_qdrantFixture.IsAvailable)
        {
            await _qdrantFixture.CreateTestCollectionAsync("performance_test", 1536);
        }
        
        // TODO: Add performance test data
        await Task.CompletedTask;
    }
}

/// <summary>
/// Combined statistics for knowledge store containers
/// </summary>
public class KnowledgeStoreStats
{
    public QdrantStats? QdrantStats { get; set; }
    public Neo4jStats? Neo4jStats { get; set; }
    public bool IsHealthy => (QdrantStats?.IsHealthy ?? false) || (Neo4jStats?.IsHealthy ?? false);
}

/// <summary>
/// Extension methods for Dictionary to make configuration building easier
/// </summary>
internal static class DictionaryExtensions
{
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> items)
    {
        foreach (var item in items)
        {
            dictionary[item.Key] = item.Value;
        }
    }
}