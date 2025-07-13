using ExxerAI.MCPServer.Application.Interfaces;
using ExxerAI.MCPServer.Application.Services;
using ExxerAI.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Meziantou.Extensions.Logging.Xunit;
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
        var logger = XUnitLogger.CreateLogger<GoogleDriveCredentialResolver>();
        logger.LogInformation("=== Test: GoogleDriveCredentialResolver_ShouldResolveCredentials_WithMockConfiguration ===");

        var configuration = Substitute.For<IConfiguration>();
        var configSection = Substitute.For<IConfigurationSection>();

        // Mock configuration values
        configSection.Value.Returns("test-client-id");
        configuration["GoogleDrive:ClientId"].Returns("test-client-id");
        configuration["GoogleDrive:ClientSecret"].Returns("test-client-secret");
        configuration.GetSection("GoogleDrive:ClientId").Returns(configSection);

        logger.LogInformation("Creating GoogleDriveCredentialResolver with mocked configuration");
        var resolver = new GoogleDriveCredentialResolver(configuration, logger);

        // Act
        logger.LogInformation("Resolving credentials...");
        var result = await resolver.ResolveCredentialsAsync();

        // Assert
        logger.LogInformation("Validating resolved credentials");
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ClientId.ShouldBe("test-client-id");
        result.Value.ClientSecret.ShouldBe("test-client-secret");
        result.Value.Source.ShouldBe("Configuration");

        logger.LogInformation("=== Test completed successfully ===");
    }

    /// <summary>
    /// Test that the credential resolver handles missing configuration gracefully
    /// </summary>
    [Fact]
    public async Task GoogleDriveCredentialResolver_ShouldReturnFailure_WhenCredentialsNotFound()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<GoogleDriveCredentialResolver>();
        logger.LogInformation("=== Test: GoogleDriveCredentialResolver_ShouldReturnFailure_WhenCredentialsNotFound ===");

        var configuration = Substitute.For<IConfiguration>();

        // Mock empty configuration - ensure all possible sources return null
        configuration["GoogleDrive:ClientId"].Returns((string?)null);
        configuration["GoogleDrive:ClientSecret"].Returns((string?)null);
        configuration["GoogleDrive:ApiKey"].Returns((string?)null);
        configuration["GoogleDrive:CredentialsPath"].Returns((string?)null);

        logger.LogInformation("Creating GoogleDriveCredentialResolver with empty configuration");
        var resolver = new GoogleDriveCredentialResolver(configuration, logger);

        // Act
        logger.LogInformation("Attempting to resolve credentials from empty configuration...");
        var result = await resolver.ResolveCredentialsAsync();

        // Assert
        logger.LogInformation("Validating failure response");
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(error => error.Contains("No valid Google Drive credentials found"));

        logger.LogInformation("=== Test completed successfully ===");
    }

    /// <summary>
    /// Test that the credential resolver can be registered in dependency injection
    /// </summary>
    [Fact]
    public void ServiceCollection_ShouldRegisterCredentialResolver_Successfully()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger();
        logger.LogInformation("=== Test: ServiceCollection_ShouldRegisterCredentialResolver_Successfully ===");

        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();

        // Act
        logger.LogInformation("Registering GoogleDriveCredentialResolver in ServiceCollection");
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<ILogger<GoogleDriveCredentialResolver>>(provider =>
            XUnitLogger.CreateLogger<GoogleDriveCredentialResolver>());
        services.AddScoped<IGoogleDriveCredentialResolver, GoogleDriveCredentialResolver>();

        var serviceProvider = services.BuildServiceProvider();
        var resolver = serviceProvider.GetService<IGoogleDriveCredentialResolver>();

        // Assert
        logger.LogInformation("Validating service registration");
        resolver.ShouldNotBeNull();
        resolver.ShouldBeOfType<GoogleDriveCredentialResolver>();

        logger.LogInformation("=== Test completed successfully ===");
    }
}