using ExxerAI.IntegrationTests.Fixtures.ContainerFixtures;
using Shouldly;
using Xunit;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Verification tests to ensure Docker containers are working correctly.
/// These tests verify the container fixtures themselves.
/// </summary>
[Collection(ContainerCollection.Name)]
public class ContainerVerificationTests : IAsyncLifetime
{
    private readonly ContainerCollectionFixture _containerFixture;

    public ContainerVerificationTests(ContainerCollectionFixture containerFixture)
    {
        _containerFixture = containerFixture ?? throw new ArgumentNullException(nameof(containerFixture));
    }

    public async ValueTask InitializeAsync()
    {
        // Container collection fixture handles container startup
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        // Container collection fixture handles cleanup
        await Task.CompletedTask;
    }

    [Fact]
    public void QdrantContainer_WhenStarted_ShouldBeAvailable()
    {
        // Arrange & Act - Use collection fixture to check container availability
        _containerFixture.RequireContainers();

        // Assert
        _containerFixture.AreContainersAvailable.ShouldBeTrue();
        _containerFixture.QdrantFixture.ShouldNotBeNull();
        _containerFixture.QdrantFixture!.IsAvailable.ShouldBeTrue();
        _containerFixture.QdrantFixture.QdrantUrl.ShouldNotBeNullOrEmpty();
        _containerFixture.QdrantFixture.QdrantPort.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void QdrantContainer_WhenGetConnectionConfig_ShouldReturnValidConfig()
    {
        // Arrange
        _containerFixture.RequireContainers();
        
        // Act
        var config = _containerFixture.QdrantFixture!.GetConnectionConfig();

        // Assert
        config.ShouldNotBeNull();
        config.Host.ShouldBe("localhost");
        config.Port.ShouldBeGreaterThan(0);
        config.HttpUrl.ShouldStartWith("http://localhost:");
        config.IsSecure.ShouldBeFalse();
    }

    [Fact]
    public async Task QdrantContainer_WhenHealthCheck_ShouldRespond()
    {
        // Arrange
        _containerFixture.RequireContainers();
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync(_containerFixture.QdrantFixture!.QdrantUrl);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
    }
}

/// <summary>
/// Combined container verification tests for the full knowledge store stack.
/// </summary>
public class KnowledgeStoreContainerVerificationTests : IClassFixture<KnowledgeStoreContainerFixture>, IAsyncLifetime
{
    private readonly KnowledgeStoreContainerFixture _containerFixture;

    public KnowledgeStoreContainerVerificationTests(KnowledgeStoreContainerFixture containerFixture)
    {
        _containerFixture = containerFixture ?? throw new ArgumentNullException(nameof(containerFixture));
    }

    public async ValueTask InitializeAsync()
    {
        // Container fixture handles startup
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        // Container fixture handles cleanup
        await Task.CompletedTask;
    }

    [Fact]
    public void KnowledgeStoreContainers_WhenStarted_ShouldBeFullyAvailable()
    {
        // Arrange & Act
        if (!_containerFixture.IsFullyAvailable)
        {
            throw new SkipException("Knowledge store containers (Qdrant & Neo4j) are not available. Please start containers: .\\start-containers.ps1");
        }

        // Assert
        _containerFixture.IsFullyAvailable.ShouldBeTrue();
        _containerFixture.QdrantConfig.HttpUrl.ShouldNotBeNullOrEmpty();
        _containerFixture.Neo4jConfig.BoltUri.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void KnowledgeStoreContainers_WhenGetConfigurations_ShouldReturnValidConfigs()
    {
        // Act
        var qdrantConfig = _containerFixture.QdrantConfig;
        var neo4jConfig = _containerFixture.Neo4jConfig;

        // Assert
        qdrantConfig.ShouldNotBeNull();
        qdrantConfig.HttpUrl.ShouldStartWith("http://localhost:");

        neo4jConfig.ShouldNotBeNull();
        neo4jConfig.BoltUri.ShouldStartWith("bolt://localhost:");
        neo4jConfig.Username.ShouldBe("neo4j");
        neo4jConfig.Password.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task KnowledgeStoreContainers_WhenHealthChecks_ShouldBothRespond()
    {
        // Arrange
        _containerFixture.EnsureFullyAvailable();
        using var httpClient = new HttpClient();

        // Act & Assert - Qdrant
        var qdrantResponse = await httpClient.GetAsync(_containerFixture.QdrantConfig.HttpUrl);
        qdrantResponse.IsSuccessStatusCode.ShouldBeTrue();

        // Act & Assert - Neo4j  
        var neo4jResponse = await httpClient.GetAsync(_containerFixture.Neo4jConfig.HttpUri);
        neo4jResponse.IsSuccessStatusCode.ShouldBeTrue();
    }

    [Fact]
    public async Task KnowledgeStoreContainers_WhenSetupTestScenario_ShouldPrepareCleanState()
    {
        // Arrange
        _containerFixture.EnsureFullyAvailable();

        // Act
        await _containerFixture.SetupTestScenarioAsync("empty");

        // Assert - Should complete without errors
        _containerFixture.IsFullyAvailable.ShouldBeTrue();
    }
}