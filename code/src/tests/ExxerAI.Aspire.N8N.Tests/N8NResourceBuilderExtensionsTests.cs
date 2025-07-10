using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using CommunityToolkit.Aspire.Hosting.N8N;

namespace ExxerAI.Aspire.N8N.Tests;

public class N8NResourceBuilderExtensionsTests
{
    [Fact]
    public void WithTimeZone_SetsEnvironmentVariable()
    {
        // Arrange
        var builder = Substitute.For<IResourceBuilder<N8NResource>>();
        builder.WithEnvironment(Arg.Any<string>(), Arg.Any<string>()).Returns(builder);

        // Act
        var result = builder.WithTimeZone("UTC");

        // Assert
        result.ShouldBe(builder);
        builder.Received().WithEnvironment("TZ", "UTC");
    }

    [Fact]
    public void AddN8N_CreatesResourceWithCorrectConfiguration()
    {
        // Arrange
        var builder = Substitute.For<IDistributedApplicationBuilder>();
        var resourceBuilder = Substitute.For<IResourceBuilder<N8NResource>>();

        // Mock the AddResource method to avoid N8NResource constructor issues
        builder.AddResource(Arg.Do<N8NResource>(resource =>
        {
            // Verify resource properties if needed
            resource.ShouldNotBeNull();
            resource.Port.ShouldBe(5678);
        })).Returns(resourceBuilder);

        // Setup the fluent chain
        resourceBuilder.WithImage(Arg.Any<string>()).Returns(resourceBuilder);
        resourceBuilder.WithHttpEndpoint(Arg.Any<int>(), Arg.Any<int>()).Returns(resourceBuilder);
        resourceBuilder.WithEnvironment(Arg.Any<string>(), Arg.Any<string>()).Returns(resourceBuilder);
        resourceBuilder.WithVolume(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(resourceBuilder);

        // Act
        var result = builder.AddN8N("n8n", 5678);

        // Assert
        result.ShouldBe(resourceBuilder);
        builder.Received(1).AddResource(Arg.Any<N8NResource>());
        resourceBuilder.Received(1).WithImage("n8nio/n8n");
        resourceBuilder.Received(1).WithHttpEndpoint(port: 5678, targetPort: 5678);
        resourceBuilder.Received(1).WithEnvironment("TZ", Arg.Any<string>());
        resourceBuilder.Received(1).WithVolume("sqlserver_data", "/var/opt/mssql", false);
    }

    [Fact]
    public void WithTimeZone_OverridesDefaultTimezone()
    {
        // Arrange
        var resourceBuilder = Substitute.For<IResourceBuilder<N8NResource>>();
        resourceBuilder.WithEnvironment(Arg.Any<string>(), Arg.Any<string>()).Returns(resourceBuilder);

        // Act
        var result = resourceBuilder.WithTimeZone("Europe/Berlin");

        // Assert
        result.ShouldBe(resourceBuilder);
        resourceBuilder.Received(1).WithEnvironment("TZ", "Europe/Berlin");
    }

    [Fact]
    public void WithBasicAuth_SetsCorrectEnvironmentVariables()
    {
        // Arrange
        var resourceBuilder = Substitute.For<IResourceBuilder<N8NResource>>();
        resourceBuilder.WithEnvironment(Arg.Any<string>(), Arg.Any<string>()).Returns(resourceBuilder);

        // Act
        var result = resourceBuilder.WithBasicAuth("testuser", "testpass");

        // Assert
        result.ShouldBe(resourceBuilder);
        resourceBuilder.Received(1).WithEnvironment("N8N_BASIC_AUTH_ACTIVE", "true");
        resourceBuilder.Received(1).WithEnvironment("N8N_BASIC_AUTH_USER", "testuser");
        resourceBuilder.Received(1).WithEnvironment("N8N_BASIC_AUTH_PASSWORD", "testpass");
    }

    [Fact]
    public void WithWorkflowsDirectory_SetsCorrectVolume()
    {
        // Arrange
        var resourceBuilder = Substitute.For<IResourceBuilder<N8NResource>>();
        resourceBuilder.WithVolume(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(resourceBuilder);

        // Act
        var result = resourceBuilder.WithWorkflowsDirectory("/host/path", "/container/path");

        // Assert
        result.ShouldBe(resourceBuilder);
        resourceBuilder.Received(1).WithVolume("/host/path", "/container/path", false);
    }

    [Fact]
    public void WithPostgresDatabase_SetsCorrectEnvironmentVariables()
    {
        // Arrange
        var resourceBuilder = Substitute.For<IResourceBuilder<N8NResource>>();
        var mockResource = Substitute.For<N8NResource>("test", Substitute.For<ReferenceExpression>(), 5678);
        resourceBuilder.Resource.Returns(mockResource);
        resourceBuilder.WithEnvironment(Arg.Any<string>(), Arg.Any<string>()).Returns(resourceBuilder);

        // Act
        var result = resourceBuilder.WithPostgresDatabase("localhost", "user", "pass", "db", 5432);

        // Assert
        result.ShouldBe(resourceBuilder);
        resourceBuilder.Received(1).WithEnvironment("DB_TYPE", "postgresdb");
        resourceBuilder.Received(1).WithEnvironment("DB_POSTGRESDB_HOST", "localhost");
        resourceBuilder.Received(1).WithEnvironment("DB_POSTGRESDB_PORT", "5432");
        resourceBuilder.Received(1).WithEnvironment("DB_POSTGRESDB_USER", "user");
        resourceBuilder.Received(1).WithEnvironment("DB_POSTGRESDB_PASSWORD", "pass");
        resourceBuilder.Received(1).WithEnvironment("DB_POSTGRESDB_DATABASE", "db");
    }
}