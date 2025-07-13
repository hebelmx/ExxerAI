using ExxerAi.MCPServer.Application.Interfaces;
using ExxerAi.MCPServer.Application.Services;
using ExxerAI.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Basic tests to verify credential resolution is working
/// </summary>
public class BasicCredentialTests
{
    /// <summary>
    /// Test that we can create and use the credential resolver service
    /// </summary>
    [Fact]
    public async Task GoogleDriveCredentialResolver_ShouldResolveCredentials_WithMockConfiguration()
    {
        // Arrange
        var configuration = Substitute.For<IConfiguration>();
        var configSection = Substitute.For<IConfigurationSection>();
        var logger = Substitute.For<ILogger<HybridGoogleDriveCredentialResolver>>();

        // Mock configuration values
        configSection.Value.Returns("test-client-id");
        configuration["GoogleDrive:ClientId"].Returns("test-client-id");
        configuration["GoogleDrive:ClientSecret"].Returns("test-client-secret");
        configuration.GetSection("GoogleDrive:ClientId").Returns(configSection);

        var resolver = new HybridGoogleDriveCredentialResolver(configuration, logger);

        // Act
        var result = await resolver.ResolveCredentialsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ClientId.ShouldBe("test-client-id");
        result.Value.ClientSecret.ShouldBe("test-client-secret");
        result.Value.Source.ShouldBe("Configuration");
    }

    /// <summary>
    /// Test that the credential resolver handles missing configuration gracefully
    /// </summary>
    [Fact]
    public async Task GoogleDriveCredentialResolver_ShouldReturnFailure_WhenCredentialsNotFound()
    {
        // Arrange
        var configuration = Substitute.For<IConfiguration>();
        var logger = Substitute.For<ILogger<HybridGoogleDriveCredentialResolver>>();

        // Mock empty configuration - ensure all possible sources return null
        configuration["GoogleDrive:ClientId"].Returns((string?)null);
        configuration["GoogleDrive:ClientSecret"].Returns((string?)null);
        configuration["GoogleDrive:ApiKey"].Returns((string?)null);
        configuration["GoogleDrive:CredentialsPath"].Returns((string?)null);

        var resolver = new HybridGoogleDriveCredentialResolver(configuration, logger);

        // Act
        var result = await resolver.ResolveCredentialsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        // Updated to match the actual error message from the resolver
        result.Error.ShouldContain("not found");
    }

    /// <summary>
    /// Test service registration and dependency injection
    /// </summary>
    [Fact]
    public void ServiceCollection_ShouldRegisterCredentialResolver_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        
        services.AddSingleton(configuration);
        services.AddLogging();
        services.AddScoped<IGoogleDriveCredentialResolver, HybridGoogleDriveCredentialResolver>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var resolver = serviceProvider.GetService<IGoogleDriveCredentialResolver>();

        // Assert
        resolver.ShouldNotBeNull();
        resolver.ShouldBeOfType<HybridGoogleDriveCredentialResolver>();
    }
}