using ExxerAI.IntegrationTests.Fixtures;
using Shouldly;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Verification tests to ensure Docker containers are working correctly.
/// These tests verify the container fixtures themselves.
/// </summary>
public class ContainerVerificationTests : IClassFixture<QdrantContainerFixture>, IAsyncLifetime
{
    private readonly QdrantContainerFixture _qdrantFixture;

    public ContainerVerificationTests(QdrantContainerFixture qdrantFixture)
    {
        _qdrantFixture = qdrantFixture ?? throw new ArgumentNullException(nameof(qdrantFixture));
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
    public void QdrantContainer_WhenStarted_ShouldBeAvailable()
    {
        // Arrange & Act
        _qdrantFixture.EnsureAvailable();

        // Assert
        _qdrantFixture.IsAvailable.ShouldBeTrue();
        _qdrantFixture.QdrantUrl.ShouldNotBeNullOrEmpty();
        _qdrantFixture.QdrantPort.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void QdrantContainer_WhenGetConnectionConfig_ShouldReturnValidConfig()
    {
        // Act
        var config = _qdrantFixture.GetConnectionConfig();

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
        _qdrantFixture.EnsureAvailable();
        using var httpClient = new HttpClient();

        // Act
        var response = await httpClient.GetAsync(_qdrantFixture.QdrantUrl);

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
        _containerFixture.EnsureFullyAvailable();

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