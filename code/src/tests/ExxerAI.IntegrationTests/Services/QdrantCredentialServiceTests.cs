using ExxerAI.IntegrationTests.Fixtures;
using ExxerAI.IntegrationTests.Services;
using Qdrant.Client;
using NSubstitute;
using Shouldly;

namespace ExxerAI.IntegrationTests.Services;

/// <summary>
/// Unit tests for QdrantCredentialService
/// Tests credential resolution, connection creation, and availability checks
/// </summary>
public class QdrantCredentialServiceTests
{
    [Fact]
    public void Constructor_WithContainerFixture_ShouldInitializeCorrectly()
    {
        // Arrange
        var containerFixture = Substitute.For<QdrantContainerFixture>();

        // Act
        var service = new QdrantCredentialService(containerFixture);

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithStaticConfig_ShouldInitializeCorrectly()
    {
        // Arrange
        var config = new QdrantConnectionConfig
        {
            Host = "localhost",
            Port = 6333,
            GrpcPort = 6334,
            HttpUrl = "http://localhost:6333",
            IsSecure = false
        };

        // Act
        var service = new QdrantCredentialService(config);

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void GetConnectionConfig_WithAvailableContainerFixture_ShouldReturnFixtureConfig()
    {
        // Arrange
        var expectedConfig = new QdrantConnectionConfig
        {
            Host = "localhost",
            Port = 6333,
            GrpcPort = 6334,
            HttpUrl = "http://localhost:6333",
            IsSecure = false
        };

        var containerFixture = Substitute.For<QdrantContainerFixture>();
        containerFixture.IsAvailable.Returns(true);
        containerFixture.GetConnectionConfig().Returns(expectedConfig);

        var service = new QdrantCredentialService(containerFixture);

        // Act
        var result = service.GetConnectionConfig();

        // Assert
        result.ShouldNotBeNull();
        result.Host.ShouldBe(expectedConfig.Host);
        result.Port.ShouldBe(expectedConfig.Port);
        result.GrpcPort.ShouldBe(expectedConfig.GrpcPort);
        result.HttpUrl.ShouldBe(expectedConfig.HttpUrl);
        result.IsSecure.ShouldBe(expectedConfig.IsSecure);
    }

    [Fact]
    public void GetConnectionConfig_WithUnavailableContainerFixture_ShouldReturnFallbackConfig()
    {
        // Arrange
        var containerFixture = Substitute.For<QdrantContainerFixture>();
        containerFixture.IsAvailable.Returns(false);

        var service = new QdrantCredentialService(containerFixture);

        // Act
        var result = service.GetConnectionConfig();

        // Assert
        result.ShouldNotBeNull();
        result.Host.ShouldBe("localhost");
        result.Port.ShouldBe(6333);
        result.GrpcPort.ShouldBe(6334);
        result.HttpUrl.ShouldBe("http://localhost:6333");
        result.IsSecure.ShouldBeFalse();
    }

    [Fact]
    public void GetConnectionConfig_WithStaticConfig_ShouldReturnStaticConfig()
    {
        // Arrange
        var staticConfig = new QdrantConnectionConfig
        {
            Host = "custom-host",
            Port = 7333,
            GrpcPort = 7334,
            HttpUrl = "https://custom-host:7333",
            IsSecure = true
        };

        var service = new QdrantCredentialService(staticConfig);

        // Act
        var result = service.GetConnectionConfig();

        // Assert
        result.ShouldNotBeNull();
        result.Host.ShouldBe(staticConfig.Host);
        result.Port.ShouldBe(staticConfig.Port);
        result.GrpcPort.ShouldBe(staticConfig.GrpcPort);
        result.HttpUrl.ShouldBe(staticConfig.HttpUrl);
        result.IsSecure.ShouldBe(staticConfig.IsSecure);
    }

    [Fact]
    public void GetConnectionConfig_WithNoFixtureOrConfig_ShouldReturnFallbackConfig()
    {
        // Arrange
        var service = new QdrantCredentialService((QdrantContainerFixture)null!);

        // Act
        var result = service.GetConnectionConfig();

        // Assert
        result.ShouldNotBeNull();
        result.Host.ShouldBe("localhost");
        result.Port.ShouldBe(6333);
        result.GrpcPort.ShouldBe(6334);
        result.HttpUrl.ShouldBe("http://localhost:6333");
        result.IsSecure.ShouldBeFalse();
    }

    [Fact]
    public void CreateQdrantClient_ShouldReturnConfiguredClient()
    {
        // Arrange
        var config = new QdrantConnectionConfig
        {
            Host = "localhost",
            Port = 6333,
            IsSecure = false
        };

        var service = new QdrantCredentialService(config);

        // Act
        var client = service.CreateQdrantClient();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeOfType<QdrantClient>();
    }

    [Fact]
    public void IsAvailable_WithAvailableContainerFixture_ShouldReturnTrue()
    {
        // Arrange
        var containerFixture = Substitute.For<QdrantContainerFixture>();
        containerFixture.IsAvailable.Returns(true);

        var service = new QdrantCredentialService(containerFixture);

        // Act
        var result = service.IsAvailable;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAvailable_WithUnavailableContainerFixture_ShouldReturnFalse()
    {
        // Arrange
        var containerFixture = Substitute.For<QdrantContainerFixture>();
        containerFixture.IsAvailable.Returns(false);

        var service = new QdrantCredentialService(containerFixture);

        // Act
        var result = service.IsAvailable;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsAvailable_WithStaticConfig_ShouldReturnTrue()
    {
        // Arrange
        var config = new QdrantConnectionConfig
        {
            Host = "localhost",
            Port = 6333,
            IsSecure = false
        };

        var service = new QdrantCredentialService(config);

        // Act
        var result = service.IsAvailable;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void EnsureAvailable_WithAvailableContainerFixture_ShouldNotThrow()
    {
        // Arrange
        var containerFixture = Substitute.For<QdrantContainerFixture>();
        containerFixture.IsAvailable.Returns(true);

        var service = new QdrantCredentialService(containerFixture);

        // Act & Assert
        Should.NotThrow(() => service.EnsureAvailable());
    }

    [Fact]
    public void EnsureAvailable_WithUnavailableContainerFixture_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var containerFixture = Substitute.For<QdrantContainerFixture>();
        containerFixture.IsAvailable.Returns(false);
        containerFixture.When(x => x.EnsureAvailable()).Do(x => throw new InvalidOperationException("Container not available"));

        var service = new QdrantCredentialService(containerFixture);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => service.EnsureAvailable())
            .Message.ShouldContain("Container not available");
    }

    [Fact]
    public void EnsureAvailable_WithStaticConfig_ShouldNotThrow()
    {
        // Arrange
        var config = new QdrantConnectionConfig
        {
            Host = "localhost",
            Port = 6333,
            IsSecure = false
        };

        var service = new QdrantCredentialService(config);

        // Act & Assert
        Should.NotThrow(() => service.EnsureAvailable());
    }

    [Fact]
    public void GetConnectionString_ShouldReturnFormattedString()
    {
        // Arrange
        var config = new QdrantConnectionConfig
        {
            HttpUrl = "http://localhost:6333",
            Host = "localhost",
            GrpcPort = 6334
        };

        var service = new QdrantCredentialService(config);

        // Act
        var result = service.GetConnectionString();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe("http://localhost:6333 (gRPC: localhost:6334)");
    }

    [Theory]
    [InlineData("localhost", 6333, 6334, false)]
    [InlineData("production", 7333, 7334, true)]
    [InlineData("dev", 8333, 8334, false)]
    public void GetConnectionConfig_WithVariousConfigurations_ShouldReturnCorrectValues(
        string host, int port, int grpcPort, bool isSecure)
    {
        // Arrange
        var httpUrl = isSecure ? $"https://{host}:{port}" : $"http://{host}:{port}";
        var config = new QdrantConnectionConfig
        {
            Host = host,
            Port = port,
            GrpcPort = grpcPort,
            HttpUrl = httpUrl,
            IsSecure = isSecure
        };

        var service = new QdrantCredentialService(config);

        // Act
        var result = service.GetConnectionConfig();

        // Assert
        result.Host.ShouldBe(host);
        result.Port.ShouldBe(port);
        result.GrpcPort.ShouldBe(grpcPort);
        result.HttpUrl.ShouldBe(httpUrl);
        result.IsSecure.ShouldBe(isSecure);
    }

    [Fact]
    public async Task CleanCollectionsAsync_WithContainerFixture_ShouldCallFixtureMethod()
    {
        // Arrange
        var containerFixture = Substitute.For<QdrantContainerFixture>();
        containerFixture.IsAvailable.Returns(true);

        var service = new QdrantCredentialService(containerFixture);

        // Act
        await service.CleanCollectionsAsync();

        // Assert
        await containerFixture.Received(1).CleanCollectionsAsync();
    }

    [Fact]
    public async Task CleanCollectionsAsync_WithoutContainerFixture_ShouldNotThrow()
    {
        // Arrange
        var config = new QdrantConnectionConfig
        {
            Host = "localhost",
            Port = 6333,
            IsSecure = false
        };

        var service = new QdrantCredentialService(config);

        // Act & Assert
        await Should.NotThrowAsync(async () => await service.CleanCollectionsAsync());
    }

    [Fact]
    public async Task CreateTestCollectionAsync_WithContainerFixture_ShouldCallFixtureMethod()
    {
        // Arrange
        var containerFixture = Substitute.For<QdrantContainerFixture>();
        containerFixture.IsAvailable.Returns(true);
        containerFixture.CreateTestCollectionAsync("test-collection", 1536).Returns(true);

        var service = new QdrantCredentialService(containerFixture);

        // Act
        var result = await service.CreateTestCollectionAsync("test-collection", 1536);

        // Assert
        result.ShouldBeTrue();
        await containerFixture.Received(1).CreateTestCollectionAsync("test-collection", 1536);
    }

    [Fact]
    public async Task CreateTestCollectionAsync_WithStaticConfig_ShouldAttemptCreation()
    {
        // Arrange
        var config = new QdrantConnectionConfig
        {
            Host = "localhost",
            Port = 6333,
            IsSecure = false
        };

        var service = new QdrantCredentialService(config);

        // Act
        var result = await service.CreateTestCollectionAsync("test-collection", 1536);

        // Assert
        // Should return false since we can't actually connect to Qdrant in unit tests
        result.ShouldBeFalse();
    }

    [Fact]
    public void GetConnectionString_WithDifferentConfigurations_ShouldFormatCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            new { HttpUrl = "http://localhost:6333", Host = "localhost", GrpcPort = 6334, Expected = "http://localhost:6333 (gRPC: localhost:6334)" },
            new { HttpUrl = "https://prod:7333", Host = "prod", GrpcPort = 7334, Expected = "https://prod:7333 (gRPC: prod:7334)" },
            new { HttpUrl = "http://dev:8333", Host = "dev", GrpcPort = 8334, Expected = "http://dev:8333 (gRPC: dev:8334)" }
        };

        foreach (var testCase in testCases)
        {
            // Arrange
            var config = new QdrantConnectionConfig
            {
                HttpUrl = testCase.HttpUrl,
                Host = testCase.Host,
                GrpcPort = testCase.GrpcPort
            };

            var service = new QdrantCredentialService(config);

            // Act
            var result = service.GetConnectionString();

            // Assert
            result.ShouldBe(testCase.Expected, $"Failed for {testCase.HttpUrl} with gRPC {testCase.Host}:{testCase.GrpcPort}");
        }
    }

    [Theory]
    [InlineData(512)]
    [InlineData(1536)]
    [InlineData(3072)]
    public async Task CreateTestCollectionAsync_WithDifferentVectorSizes_ShouldPassCorrectParameters(uint vectorSize)
    {
        // Arrange
        var containerFixture = Substitute.For<QdrantContainerFixture>();
        containerFixture.IsAvailable.Returns(true);
        containerFixture.CreateTestCollectionAsync(Arg.Any<string>(), vectorSize).Returns(true);

        var service = new QdrantCredentialService(containerFixture);

        // Act
        var result = await service.CreateTestCollectionAsync("test-collection", vectorSize);

        // Assert
        result.ShouldBeTrue();
        await containerFixture.Received(1).CreateTestCollectionAsync("test-collection", vectorSize);
    }
} 