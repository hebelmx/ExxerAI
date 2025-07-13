using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExxerAi.MCPServer.Application.Services;
using Shouldly;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Simple verification test for credential resolution
/// </summary>
public class CredentialVerificationTest
{
    [Fact]
    public async Task ServiceAccount_ShouldResolve_FromEnvironmentVariable()
    {
        // Arrange
        using var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var logger = loggerFactory.CreateLogger<GoogleDriveCredentialResolver>();

        // Use minimal configuration to test environment variable priority
        var configuration = new ConfigurationBuilder().Build();
        var resolver = new GoogleDriveCredentialResolver(configuration, logger);

        // Act
        var result = await resolver.ResolveCredentialsAsync();

        // Assert & Debug Info
        Console.WriteLine($"🔍 CREDENTIAL VERIFICATION RESULTS:");
        Console.WriteLine($"   Environment Variable Set: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT_JSON"))}");
        Console.WriteLine($"   Resolution Success: {result.IsSuccess}");
        
        if (result.IsSuccess)
        {
            var creds = result.Value;
            Console.WriteLine($"   Type: {creds.Type}");
            Console.WriteLine($"   Source: {creds.Source}");
            Console.WriteLine($"   Email: {creds.ServiceAccountEmail}");
            Console.WriteLine($"   Project: {creds.ProjectId}");
            Console.WriteLine($"   Has JSON: {!string.IsNullOrEmpty(creds.ServiceAccountJson)}");
            
            // Verify it's actually a service account
            creds.Type.ShouldBe(CredentialType.ServiceAccount);
            creds.ServiceAccountEmail.ShouldBe("exxerai@exxerai.iam.gserviceaccount.com");
            creds.ProjectId.ShouldBe("exxerai");
            creds.Source.ShouldContain("Environment Variables");
        }
        else
        {
            Console.WriteLine($"   Errors:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"     - {error}");
            }
        }

        // This should pass if our environment variable is working
        result.IsSuccess.ShouldBeTrue("Service account credentials should resolve from environment variable");
    }

    [Fact]
    public async Task ServiceAccount_ShouldResolve_FromFile()
    {
        // Arrange
        using var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var logger = loggerFactory.CreateLogger<GoogleDriveCredentialResolver>();

        // Clear environment variable to test file resolution
        var originalEnvVar = Environment.GetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT_JSON");
        Environment.SetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT_JSON", null);

        try
        {
            var configData = new Dictionary<string, string?>
            {
                ["GoogleDrive:CredentialsPath"] = "./exxerai.gdrive.json"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var resolver = new GoogleDriveCredentialResolver(configuration, logger);

            // Act
            var result = await resolver.ResolveCredentialsAsync();

            // Assert
            Console.WriteLine($"🔍 FILE CREDENTIAL VERIFICATION:");
            Console.WriteLine($"   File Exists: {File.Exists("./exxerai.gdrive.json")}");
            Console.WriteLine($"   Resolution Success: {result.IsSuccess}");

            if (result.IsSuccess)
            {
                var creds = result.Value;
                Console.WriteLine($"   Type: {creds.Type}");
                Console.WriteLine($"   Source: {creds.Source}");
                
                creds.Type.ShouldBe(CredentialType.ServiceAccount);
                creds.Source.ShouldContain("JSON File");
            }

            result.IsSuccess.ShouldBeTrue("Service account credentials should resolve from file");
        }
        finally
        {
            // Restore environment variable
            Environment.SetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT_JSON", originalEnvVar);
        }
    }
} 