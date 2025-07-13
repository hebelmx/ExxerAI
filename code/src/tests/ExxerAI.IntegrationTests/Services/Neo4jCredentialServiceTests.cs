using ExxerAI.IntegrationTests.Fixtures;
using ExxerAI.IntegrationTests.Services;
using Neo4jClient;
using NSubstitute;
using Shouldly;

namespace ExxerAI.IntegrationTests.Services;

/// <summary>
/// Unit tests for Neo4jCredentialService
/// Tests credential resolution, connection creation, and availability checks
/// </summary>
public class Neo4jCredentialServiceTests
{
    [Fact]
    public void Constructor_WithContainerFixture_ShouldInitializeCorrectly()
    {
        // Arrange
        var containerFixture = Substitute.For<Neo4jContainerFixture>();

        // Act
        var service = new Neo4jCredentialService(containerFixture);

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithStaticConfig_ShouldInitializeCorrectly()
    {
        // Arrange
        var config = new Neo4jConnectionConfig
        {
            BoltUri = "bolt://localhost:7688",
            HttpUri = "http://localhost:7475",
            Username = "neo4j",
            Password = "test123456",
            Database = "neo4j"
        };

        // Act
        var service = new Neo4jCredentialService(config);

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void GetConnectionConfig_WithAvailableContainerFixture_ShouldReturnFixtureConfig()
    {
        // Arrange
        var expectedConfig = new Neo4jConnectionConfig
        {
            BoltUri = "bolt://localhost:7688",
            HttpUri = "http://localhost:7475",
            Username = "neo4j",
            Password = "test123456",
            Database = "neo4j"
        };

        var containerFixture = Substitute.For<Neo4jContainerFixture>();
        containerFixture.IsAvailable.Returns(true);
        containerFixture.GetConnectionConfig().Returns(expectedConfig);

        var service = new Neo4jCredentialService(containerFixture);

        // Act
        var result = service.GetConnectionConfig();

        // Assert
        result.ShouldNotBeNull();
        result.BoltUri.ShouldBe(expectedConfig.BoltUri);
        result.HttpUri.ShouldBe(expectedConfig.HttpUri);
        result.Username.ShouldBe(expectedConfig.Username);
        result.Password.ShouldBe(expectedConfig.Password);
        result.Database.ShouldBe(expectedConfig.Database);
    }

    [Fact]
    public void GetConnectionConfig_WithUnavailableContainerFixture_ShouldReturnFallbackConfig()
    {
        // Arrange
        var containerFixture = Substitute.For<Neo4jContainerFixture>();
        containerFixture.IsAvailable.Returns(false);

        var service = new Neo4jCredentialService(containerFixture);

        // Act
        var result = service.GetConnectionConfig();

        // Assert
        result.ShouldNotBeNull();
        result.BoltUri.ShouldBe("bolt://localhost:7688");
        result.HttpUri.ShouldBe("http://localhost:7475");
        result.Username.ShouldBe("neo4j");
        result.Password.ShouldBe("test123456");
        result.Database.ShouldBe("neo4j");
    }

    [Fact]
    public void GetConnectionConfig_WithStaticConfig_ShouldReturnStaticConfig()
    {
        // Arrange
        var staticConfig = new Neo4jConnectionConfig
        {
            BoltUri = "bolt://custom:7687",
            HttpUri = "http://custom:7474",
            Username = "customuser",
            Password = "custompass",
            Database = "customdb"
        };

        var service = new Neo4jCredentialService(staticConfig);

        // Act
        var result = service.GetConnectionConfig();

        // Assert
        result.ShouldNotBeNull();
        result.BoltUri.ShouldBe(staticConfig.BoltUri);
        result.HttpUri.ShouldBe(staticConfig.HttpUri);
        result.Username.ShouldBe(staticConfig.Username);
        result.Password.ShouldBe(staticConfig.Password);
        result.Database.ShouldBe(staticConfig.Database);
    }

    [Fact]
    public void GetConnectionConfig_WithNoFixtureOrConfig_ShouldReturnFallbackConfig()
    {
        // Arrange
        var service = new Neo4jCredentialService((Neo4jContainerFixture)null!);

        // Act
        var result = service.GetConnectionConfig();

        // Assert
        result.ShouldNotBeNull();
        result.BoltUri.ShouldBe("bolt://localhost:7688");
        result.HttpUri.ShouldBe("http://localhost:7475");
        result.Username.ShouldBe("neo4j");
        result.Password.ShouldBe("test123456");
        result.Database.ShouldBe("neo4j");
    }

    [Fact]
    public void CreateGraphClient_ShouldReturnConfiguredClient()
    {
        // Arrange
        var config = new Neo4jConnectionConfig
        {
            BoltUri = "bolt://localhost:7688",
            Username = "neo4j",
            Password = "test123456"
        };

        var service = new Neo4jCredentialService(config);

        // Act
        var client = service.CreateGraphClient();

        // Assert
        client.ShouldNotBeNull();
        client.ShouldBeOfType<GraphClient>();
    }

    [Fact]
    public void IsAvailable_WithAvailableContainerFixture_ShouldReturnTrue()
    {
        // Arrange
        var containerFixture = Substitute.For<Neo4jContainerFixture>();
        containerFixture.IsAvailable.Returns(true);

        var service = new Neo4jCredentialService(containerFixture);

        // Act
        var result = service.IsAvailable;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAvailable_WithUnavailableContainerFixture_ShouldReturnFalse()
    {
        // Arrange
        var containerFixture = Substitute.For<Neo4jContainerFixture>();
        containerFixture.IsAvailable.Returns(false);

        var service = new Neo4jCredentialService(containerFixture);

        // Act
        var result = service.IsAvailable;

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsAvailable_WithStaticConfig_ShouldReturnTrue()
    {
        // Arrange
        var config = new Neo4jConnectionConfig
        {
            BoltUri = "bolt://localhost:7688",
            Username = "neo4j",
            Password = "test123456"
        };

        var service = new Neo4jCredentialService(config);

        // Act
        var result = service.IsAvailable;

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void EnsureAvailable_WithAvailableContainerFixture_ShouldNotThrow()
    {
        // Arrange
        var containerFixture = Substitute.For<Neo4jContainerFixture>();
        containerFixture.IsAvailable.Returns(true);

        var service = new Neo4jCredentialService(containerFixture);

        // Act & Assert
        Should.NotThrow(() => service.EnsureAvailable());
    }

    [Fact]
    public void EnsureAvailable_WithUnavailableContainerFixture_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var containerFixture = Substitute.For<Neo4jContainerFixture>();
        containerFixture.IsAvailable.Returns(false);
        containerFixture.When(x => x.EnsureAvailable()).Do(x => throw new InvalidOperationException("Container not available"));

        var service = new Neo4jCredentialService(containerFixture);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => service.EnsureAvailable())
            .Message.ShouldContain("Container not available");
    }

    [Fact]
    public void EnsureAvailable_WithStaticConfig_ShouldNotThrow()
    {
        // Arrange
        var config = new Neo4jConnectionConfig
        {
            BoltUri = "bolt://localhost:7688",
            Username = "neo4j",
            Password = "test123456"
        };

        var service = new Neo4jCredentialService(config);

        // Act & Assert
        Should.NotThrow(() => service.EnsureAvailable());
    }

    [Fact]
    public void GetConnectionString_ShouldReturnFormattedString()
    {
        // Arrange
        var config = new Neo4jConnectionConfig
        {
            BoltUri = "bolt://localhost:7688",
            Username = "testuser"
        };

        var service = new Neo4jCredentialService(config);

        // Act
        var result = service.GetConnectionString();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe("bolt://localhost:7688 (user: testuser)");
    }

    [Theory]
    [InlineData("bolt://localhost:7687", "neo4j", "password")]
    [InlineData("bolt://production:7687", "produser", "prodpass")]
    [InlineData("bolt://dev:7688", "devuser", "devpass")]
    public void GetConnectionConfig_WithVariousConfigurations_ShouldReturnCorrectValues(
        string boltUri, string username, string password)
    {
        // Arrange
        var config = new Neo4jConnectionConfig
        {
            BoltUri = boltUri,
            Username = username,
            Password = password
        };

        var service = new Neo4jCredentialService(config);

        // Act
        var result = service.GetConnectionConfig();

        // Assert
        result.BoltUri.ShouldBe(boltUri);
        result.Username.ShouldBe(username);
        result.Password.ShouldBe(password);
    }

    [Fact]
    public void GetConnectionString_WithDifferentConfigurations_ShouldFormatCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            new { BoltUri = "bolt://localhost:7687", Username = "neo4j", Expected = "bolt://localhost:7687 (user: neo4j)" },
            new { BoltUri = "bolt://prod:7688", Username = "admin", Expected = "bolt://prod:7688 (user: admin)" },
            new { BoltUri = "bolt://dev:7689", Username = "developer", Expected = "bolt://dev:7689 (user: developer)" }
        };

        foreach (var testCase in testCases)
        {
            // Arrange
            var config = new Neo4jConnectionConfig
            {
                BoltUri = testCase.BoltUri,
                Username = testCase.Username
            };

            var service = new Neo4jCredentialService(config);

            // Act
            var result = service.GetConnectionString();

            // Assert
            result.ShouldBe(testCase.Expected, $"Failed for {testCase.BoltUri} with user {testCase.Username}");
        }
    }
} 