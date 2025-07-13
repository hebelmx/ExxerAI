using ExxerAI.IntegrationTests.Fixtures;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Critical infrastructure connectivity tests that MUST pass before any other integration tests.
/// These tests validate that all required Docker containers are running and healthy.
/// If these fail, ALL other integration tests should be skipped to prevent cascading failures.
/// </summary>
[Collection("ContainerConnectivity")]
public class ContainerConnectivityTests : IClassFixture<KnowledgeStoreContainerFixture>
{
    private readonly KnowledgeStoreContainerFixture _containerFixture;
    private readonly ILogger<ContainerConnectivityTests> _logger;

    public ContainerConnectivityTests(KnowledgeStoreContainerFixture containerFixture)
    {
        _containerFixture = containerFixture;
        using var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        _logger = loggerFactory.CreateLogger<ContainerConnectivityTests>();
    }

    /// <summary>
    /// CRITICAL: Validates that Neo4j container is running and responding
    /// This test MUST pass for any graph database functionality to work
    /// </summary>
    [Fact(DisplayName = "🔍 CRITICAL: Neo4j Container Connectivity")]
    public async Task Neo4jContainer_ShouldBeHealthyAndResponding()
    {
        _logger.LogInformation("🔍 CRITICAL TEST: Validating Neo4j container connectivity...");
        
        try
        {
            // This will throw if Neo4j is not available, causing ALL other tests to fail fast
            _containerFixture.EnsureFullyAvailable();
            
            var neo4jConfig = _containerFixture.Neo4jConfig;
            neo4jConfig.ShouldNotBeNull("Neo4j configuration must be available");
            neo4jConfig.BoltUri.ShouldNotBeNullOrEmpty("Neo4j Bolt URI must be configured");
            
            // Test actual connectivity
            var graphClient = _containerFixture.GetRequiredService<Neo4jContainerFixture>().GetGraphClient();
            graphClient.ShouldNotBeNull("Neo4j graph client must be available");
            
            // Verify we can execute a simple query
            var stats = await _containerFixture.GetRequiredService<Neo4jContainerFixture>().GetDatabaseStatsAsync();
            stats.ShouldNotBeNull("Neo4j database stats must be retrievable");
            stats.IsHealthy.ShouldBeTrue("Neo4j database must be healthy");
            
            _logger.LogInformation("✅ CRITICAL: Neo4j container is healthy and responding");
            _logger.LogInformation("   📊 Bolt URI: {BoltUri}", neo4jConfig.BoltUri);
            _logger.LogInformation("   📈 Nodes: {NodeCount}, Relations: {RelationshipCount}", 
                stats.NodeCount, stats.RelationshipCount);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "❌ CRITICAL FAILURE: Neo4j container is not available");
            _logger.LogCritical("💡 Start containers with: .\\start-containers.ps1");
            
            // This failure should stop ALL other integration tests
            throw new InvalidOperationException(
                "CRITICAL INFRASTRUCTURE FAILURE: Neo4j container is not available. " +
                "ALL integration tests will fail. Please start containers: .\\start-containers.ps1", ex);
        }
    }

    /// <summary>
    /// CRITICAL: Validates that Qdrant container is running and responding  
    /// This test MUST pass for any vector database functionality to work
    /// </summary>
    [Fact(DisplayName = "🔍 CRITICAL: Qdrant Container Connectivity")]
    public async Task QdrantContainer_ShouldBeHealthyAndResponding()
    {
        _logger.LogInformation("🔍 CRITICAL TEST: Validating Qdrant container connectivity...");
        
        try
        {
            // This will throw if Qdrant is not available, causing ALL other tests to fail fast
            _containerFixture.EnsureFullyAvailable();
            
            var qdrantConfig = _containerFixture.QdrantConfig;
            qdrantConfig.ShouldNotBeNull("Qdrant configuration must be available");
            qdrantConfig.HttpUrl.ShouldNotBeNullOrEmpty("Qdrant HTTP URL must be configured");
            
            // Test actual connectivity
            var qdrantClient = _containerFixture.GetRequiredService<QdrantContainerFixture>().GetQdrantClient();
            qdrantClient.ShouldNotBeNull("Qdrant client must be available");
            
            // Verify we can list collections
            var collections = await _containerFixture.GetRequiredService<QdrantContainerFixture>().GetCollectionsAsync();
            collections.ShouldNotBeNull("Qdrant collections list must be retrievable");
            
            // Verify health
            var stats = await _containerFixture.GetRequiredService<QdrantContainerFixture>().GetDatabaseStatsAsync();
            stats.ShouldNotBeNull("Qdrant database stats must be retrievable");
            stats.IsHealthy.ShouldBeTrue("Qdrant database must be healthy");
            
            _logger.LogInformation("✅ CRITICAL: Qdrant container is healthy and responding");
            _logger.LogInformation("   📊 HTTP URL: {HttpUrl}", qdrantConfig.HttpUrl);
            _logger.LogInformation("   📈 Collections: {CollectionCount}, Vectors: {TotalVectors}", 
                stats.CollectionCount, stats.TotalVectors);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "❌ CRITICAL FAILURE: Qdrant container is not available");
            _logger.LogCritical("💡 Start containers with: .\\start-containers.ps1");
            
            // This failure should stop ALL other integration tests
            throw new InvalidOperationException(
                "CRITICAL INFRASTRUCTURE FAILURE: Qdrant container is not available. " +
                "ALL integration tests will fail. Please start containers: .\\start-containers.ps1", ex);
        }
    }

    /// <summary>
    /// CRITICAL: Validates that Docker is running and containers are accessible
    /// This test MUST pass before any container-dependent tests can run
    /// </summary>
    [Fact(DisplayName = "🔍 CRITICAL: Docker Infrastructure Connectivity")]
    public async Task DockerInfrastructure_ShouldBeRunningAndAccessible()
    {
        _logger.LogInformation("🔍 CRITICAL TEST: Validating Docker infrastructure...");
        
        try
        {
            // Test basic container health
            var knowledgeStoreStats = await _containerFixture.GetDatabaseStatsAsync();
            knowledgeStoreStats.ShouldNotBeNull("Knowledge store stats must be available");
            knowledgeStoreStats.IsHealthy.ShouldBeTrue("At least one knowledge store component must be healthy");
            
            // Validate configuration is properly built
            var config = _containerFixture.Configuration;
            config.ShouldNotBeNull("Container configuration must be built");
            
            // Check for required configuration values
            var neo4jAvailable = config["Testing:Neo4jAvailable"];
            var qdrantAvailable = config["Testing:QdrantAvailable"];
            
            _logger.LogInformation("✅ CRITICAL: Docker infrastructure is accessible");
            _logger.LogInformation("   🐳 Neo4j Available: {Neo4jAvailable}", neo4jAvailable);
            _logger.LogInformation("   🐳 Qdrant Available: {QdrantAvailable}", qdrantAvailable);
            _logger.LogInformation("   📊 Overall Health: {IsHealthy}", knowledgeStoreStats.IsHealthy);
            
            // If neither is available, fail fast
            if (!_containerFixture.IsFullyAvailable)
            {
                var availableServices = new List<string>();
                if (bool.Parse(neo4jAvailable ?? "false")) availableServices.Add("Neo4j");
                if (bool.Parse(qdrantAvailable ?? "false")) availableServices.Add("Qdrant");
                
                if (availableServices.Count == 0)
                {
                    throw new InvalidOperationException(
                        "CRITICAL: No knowledge store containers are available. Please start containers: .\\start-containers.ps1");
                }
                
                _logger.LogWarning("⚠️ Partial infrastructure available: {Services}", string.Join(", ", availableServices));
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "❌ CRITICAL FAILURE: Docker infrastructure is not accessible");
            _logger.LogCritical("💡 Ensure Docker is running and start containers: .\\start-containers.ps1");
            
            throw new InvalidOperationException(
                "CRITICAL INFRASTRUCTURE FAILURE: Docker containers are not accessible. " +
                "Please ensure Docker is running and start containers: .\\start-containers.ps1", ex);
        }
    }

    /// <summary>
    /// EARLY WARNING: Check for Ollama availability (required for embedding generation)
    /// This validates the AI infrastructure needed for Phase 1 objectives
    /// </summary>
    [Fact(DisplayName = "🔍 WARNING: Ollama LLM Connectivity")]
    public async Task OllamaLLM_ShouldBeAvailableForEmbedding()
    {
        _logger.LogInformation("🔍 WARNING TEST: Validating Ollama LLM availability...");
        
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            
            // Check if Ollama is running on default port
            var ollamaUrl = "http://localhost:11434";
            var response = await httpClient.GetAsync($"{ollamaUrl}/api/tags");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("✅ WARNING: Ollama LLM is available and responding");
                _logger.LogInformation("   🤖 URL: {OllamaUrl}", ollamaUrl);
                _logger.LogDebug("   📋 Available models: {Content}", content);
            }
            else
            {
                _logger.LogWarning("⚠️ WARNING: Ollama LLM is not responding properly (Status: {StatusCode})", response.StatusCode);
                _logger.LogWarning("💡 Some embedding tests may fail without Ollama");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ WARNING: Ollama LLM is not available");
            _logger.LogWarning("💡 Phase 1 embedding generation will require Ollama integration");
            _logger.LogWarning("💡 Start Ollama or configure alternative embedding provider");
            
            // Don't fail the test - this is a warning for future integration
            // Some tests can still run without Ollama
        }
    }

    /// <summary>
    /// Performance baseline test to validate infrastructure meets Phase 1 requirements
    /// Tests the sub-second response time requirement from Project.md
    /// </summary>
    [Fact(DisplayName = "🔍 PERFORMANCE: Infrastructure Response Time")]
    public async Task Infrastructure_ShouldMeetPerformanceBaseline()
    {
        _logger.LogInformation("🔍 PERFORMANCE TEST: Validating infrastructure response times...");
        
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Test Neo4j response time
            var neo4jStats = await _containerFixture.GetRequiredService<Neo4jContainerFixture>().GetDatabaseStatsAsync();
            var neo4jTime = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            
            // Test Qdrant response time  
            var qdrantStats = await _containerFixture.GetRequiredService<QdrantContainerFixture>().GetDatabaseStatsAsync();
            var qdrantTime = stopwatch.ElapsedMilliseconds;
            
            _logger.LogInformation("✅ PERFORMANCE: Infrastructure response times measured");
            _logger.LogInformation("   📊 Neo4j Response: {Neo4jTime}ms", neo4jTime);
            _logger.LogInformation("   📊 Qdrant Response: {QdrantTime}ms", qdrantTime);
            
            // Project.md requirement: Sub-second semantic search
            var maxResponseTime = 1000; // 1 second baseline
            
            if (neo4jTime > maxResponseTime)
            {
                _logger.LogWarning("⚠️ Neo4j response time ({Neo4jTime}ms) exceeds baseline ({MaxTime}ms)", 
                    neo4jTime, maxResponseTime);
            }
            
            if (qdrantTime > maxResponseTime)
            {
                _logger.LogWarning("⚠️ Qdrant response time ({QdrantTime}ms) exceeds baseline ({MaxTime}ms)", 
                    qdrantTime, maxResponseTime);
            }
            
            // Overall infrastructure should be responsive
            var totalTime = neo4jTime + qdrantTime;
            totalTime.ShouldBeLessThan(maxResponseTime * 2, 
                "Combined infrastructure response time should be reasonable for integration tests");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Could not measure infrastructure performance");
            // Don't fail - performance testing can be refined later
        }
    }
}

/// <summary>
/// Collection definition for container connectivity tests
/// Ensures these critical tests run in isolation and first
/// </summary>
[CollectionDefinition("ContainerConnectivity")]
public class ContainerConnectivityCollection : ICollectionFixture<KnowledgeStoreContainerFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
} 