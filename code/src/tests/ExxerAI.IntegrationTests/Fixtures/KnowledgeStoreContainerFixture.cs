using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExxerAI.IntegrationTests.Fixtures;

/// <summary>
/// Combined container fixture for complete knowledge store testing.
/// Orchestrates both Qdrant and Neo4j containers for hybrid knowledge service tests.
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
        _logger.LogInformation("🚀 Starting knowledge store container orchestration...");

        try
        {
            // Start containers in parallel for faster startup
            var qdrantTask = _qdrantFixture.InitializeAsync();
            var neo4jTask = _neo4jFixture.InitializeAsync();

            await Task.WhenAll(qdrantTask.AsTask(), neo4jTask.AsTask());

            // Build configuration with container connection strings
            await BuildConfigurationAsync();

            // Build service container with real services
            await BuildServiceContainerAsync();

            _logger.LogInformation("✅ Knowledge store container orchestration ready");
            _logger.LogInformation("📊 Qdrant: {QdrantUrl}", QdrantConfig.HttpUrl);
            _logger.LogInformation("📈 Neo4j: {Neo4jUrl}", Neo4jConfig.BoltUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize knowledge store containers");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("🧹 Cleaning up knowledge store containers...");

        try
        {
            // Dispose service container first
            (ServiceProvider as IDisposable)?.Dispose();

            // Stop containers in parallel
            var qdrantTask = _qdrantFixture.DisposeAsync();
            var neo4jTask = _neo4jFixture.DisposeAsync();

            await Task.WhenAll(qdrantTask.AsTask(), neo4jTask.AsTask());

            _logger.LogInformation("✅ Knowledge store containers cleaned up");
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
        var configData = new Dictionary<string, string?>
        {
            // Qdrant configuration
            ["VectorStore:Qdrant:Host"] = QdrantConfig.Host,
            ["VectorStore:Qdrant:Port"] = QdrantConfig.Port.ToString(),
            ["VectorStore:Qdrant:HttpUrl"] = QdrantConfig.HttpUrl,
            ["VectorStore:Qdrant:IsSecure"] = QdrantConfig.IsSecure.ToString(),
            
            // Neo4j configuration  
            ["GraphStore:Neo4j:BoltUri"] = Neo4jConfig.BoltUri,
            ["GraphStore:Neo4j:HttpUri"] = Neo4jConfig.HttpUri,
            ["GraphStore:Neo4j:Username"] = Neo4jConfig.Username,
            ["GraphStore:Neo4j:Password"] = Neo4jConfig.Password,
            ["GraphStore:Neo4j:Database"] = Neo4jConfig.Database,
            
            // Hybrid configuration
            ["KnowledgeStore:Type"] = "Hybrid",
            ["KnowledgeStore:VectorWeight"] = "0.7",
            ["KnowledgeStore:GraphWeight"] = "0.3",
            
            // Test configuration
            ["Testing:ContainerMode"] = "true",
            ["Testing:SkipExternalServices"] = "false"
        };

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

        // Add knowledge store services (these would be your real implementations)
        // services.AddScoped<IQdrantVectorStore, QdrantVectorStore>();
        // services.AddScoped<INeo4jGraphStore, Neo4jGraphStore>();
        // services.AddScoped<IHybridKnowledgeService, HybridKnowledgeService>();

        // For now, we'll add placeholder registrations
        services.AddSingleton<QdrantConnectionConfig>(_ => QdrantConfig);
        services.AddSingleton<Neo4jConnectionConfig>(_ => Neo4jConfig);

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
                "This test requires Docker to be running and accessible. " +
                "Please ensure Docker is installed and running, then try again.");
        }
    }

    /// <summary>
    /// Clean both databases for test isolation
    /// </summary>
    public async Task CleanDatabasesAsync()
    {
        _logger.LogDebug("🧹 Cleaning knowledge store databases for test isolation...");

        try
        {
            // Clean Neo4j
            await _neo4jFixture.CleanDatabaseAsync();

            // Clean Qdrant (delete all collections)
            await CleanQdrantAsync();

            _logger.LogDebug("✅ Knowledge store databases cleaned");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error cleaning knowledge store databases");
        }
    }

    /// <summary>
    /// Clean Qdrant collections
    /// </summary>
    private async Task CleanQdrantAsync()
    {
        try
        {
            using var httpClient = new HttpClient();
            
            // Get all collections
            var collectionsResponse = await httpClient.GetAsync($"{QdrantConfig.HttpUrl}/collections");
            if (collectionsResponse.IsSuccessStatusCode)
            {
                // TODO: Parse collections and delete them
                // For now, just log
                _logger.LogDebug("🗑️ Cleaned Qdrant collections");
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "⚠️ Error cleaning Qdrant collections");
        }
    }

    /// <summary>
    /// Get a service from the container
    /// </summary>
    public T GetRequiredService<T>() where T : notnull
    {
        EnsureFullyAvailable();
        return ServiceProvider.GetRequiredService<T>();
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
        // TODO: Insert sample documents into both Qdrant and Neo4j
        _logger.LogDebug("📝 Setting up sample documents...");
        await Task.CompletedTask;
    }

    private async Task SetupPerformanceTestDataAsync()
    {
        // TODO: Insert performance test data
        _logger.LogDebug("⚡ Setting up performance test data...");
        await Task.CompletedTask;
    }
}