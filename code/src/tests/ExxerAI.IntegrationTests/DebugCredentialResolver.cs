using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ExxerAi.MCPServer.Application.Services;
using ExxerAi.MCPServer.Application.Interfaces;
using Xunit;
using Shouldly;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Debug test to understand credential resolution issues
/// </summary>
public class DebugCredentialResolver
{
    [Fact]
    public async Task Debug_CredentialResolver_ShouldShowWhatSourcesItChecks()
    {
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
        Console.WriteLine("=== CREDENTIAL RESOLUTION DEBUG ===");
        Console.WriteLine($"Result Success: {result.IsSuccess}");
        Console.WriteLine($"Result Error: {result.Error}");

        // Check individual configuration sources
        Console.WriteLine("\n=== CONFIGURATION VALUES ===");
        Console.WriteLine($"GoogleDrive:ClientId: '{configuration["GoogleDrive:ClientId"]}'");
        Console.WriteLine($"GoogleDrive:ClientSecret: '{configuration["GoogleDrive:ClientSecret"]}'");
        Console.WriteLine($"GoogleDrive:CredentialsPath: '{configuration["GoogleDrive:CredentialsPath"]}'");

        Console.WriteLine("\n=== ENVIRONMENT VARIABLES ===");
        Console.WriteLine($"GOOGLE_OAUTH_CLIENT_ID: '{Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID")}'");
        Console.WriteLine($"GOOGLE_OAUTH_CLIENT_SECRET: '{Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET")}'");

        Console.WriteLine("\n=== FILE EXISTENCE ===");
        var credentialsPath = configuration["GoogleDrive:CredentialsPath"];
        if (!string.IsNullOrEmpty(credentialsPath))
        {
            Console.WriteLine($"Credentials file path: {credentialsPath}");
            Console.WriteLine($"File exists: {File.Exists(credentialsPath)}");
            if (File.Exists(credentialsPath))
            {
                var content = await File.ReadAllTextAsync(credentialsPath);
                Console.WriteLine($"File content length: {content.Length}");
                Console.WriteLine($"File content preview: {content[..Math.Min(200, content.Length)]}...");
            }
        }

        // This test is for debugging, so we don't assert success
        // The goal is to understand what's happening
        Console.WriteLine("=== DEBUG COMPLETE ===");
    }
} 