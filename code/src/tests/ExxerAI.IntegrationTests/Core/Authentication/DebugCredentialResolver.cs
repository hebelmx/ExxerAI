using ExxerAi.MCPServer.Application.Interfaces;
using ExxerAi.MCPServer.Application.Services;
using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Debug test to understand credential resolution issues
/// </summary>
public class DebugCredentialResolver
{
    [Fact]
    public async Task Debug_CredentialResolver_ShouldShowWhatSourcesItChecks()
    {
        var logger = XUnitLogger.CreateLogger<DebugCredentialResolver>();
        // Arrange
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddEnvironmentVariables("EXXERAI_TEST_");

        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddScoped<IGoogleDriveCredentialResolver, GoogleDriveCredentialResolver>();

        using var serviceProvider = services.BuildServiceProvider();
        var credentialResolver = serviceProvider.GetRequiredService<IGoogleDriveCredentialResolver>();

        // Act
        var result = await credentialResolver.ResolveCredentialsAsync();

        // Debug output
        logger.LogInformation("=== CREDENTIAL RESOLUTION DEBUG ===");
        logger.LogInformation($"Result Success: {result.IsSuccess}");
        logger.LogInformation($"Result Error: {result.Error}");

        // Check individual configuration sources
        logger.LogInformation("\n=== CONFIGURATION VALUES ===");
        logger.LogInformation($"GoogleDrive:ClientId: '{configuration["GoogleDrive:ClientId"]}'");
        logger.LogInformation($"GoogleDrive:ClientSecret: '{configuration["GoogleDrive:ClientSecret"]}'");
        logger.LogInformation($"GoogleDrive:CredentialsPath: '{configuration["GoogleDrive:CredentialsPath"]}'");

        logger.LogInformation("\n=== ENVIRONMENT VARIABLES ===");
        logger.LogInformation($"GOOGLE_OAUTH_CLIENT_ID: '{Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID")}'");
        logger.LogInformation($"GOOGLE_OAUTH_CLIENT_SECRET: '{Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET")}'");

        logger.LogInformation("\n=== FILE EXISTENCE ===");
        var credentialsPath = configuration["GoogleDrive:CredentialsPath"];
        if (!string.IsNullOrEmpty(credentialsPath))
        {
            logger.LogInformation($"Credentials file path: {credentialsPath}");
            logger.LogInformation($"File exists: {File.Exists(credentialsPath)}");
            if (File.Exists(credentialsPath))
            {
                var content = await File.ReadAllTextAsync(credentialsPath);
                logger.LogInformation($"File content length: {content.Length}");
                logger.LogInformation($"File content preview: {content[..Math.Min(200, content.Length)]}...");
            }
        }

        // This test is for debugging, so we don't assert success
        // The goal is to understand what's happening
        logger.LogInformation("=== DEBUG COMPLETE ===");
    }
}