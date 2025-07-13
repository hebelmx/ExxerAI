using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExxerAi.MCPServer.Application.Services;
using Shouldly;
using NSubstitute;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Test to verify API key credential resolution works with GDrive.Api.json
/// </summary>
public class TestApiKeyCredentials
{
    [Fact]
    public async Task GoogleDriveCredentialResolver_ShouldParseExxerAiApiFormat_FromGDriveApiJson()
    {
        // Arrange
        var logger = Substitute.For<ILogger<GoogleDriveCredentialResolver>>();
        
        // Build configuration pointing to the actual GDrive.Api.json
        var configData = new Dictionary<string, string?>
        {
            ["GoogleDrive:CredentialsPath"] = "./GDrive.Api.json"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var resolver = new GoogleDriveCredentialResolver(configuration, logger);

        // Act
        var result = await resolver.ResolveCredentialsAsync();

        // Assert
        Console.WriteLine("=== API KEY CREDENTIAL TEST ===");
        Console.WriteLine($"Success: {result.IsSuccess}");
        
        if (result.IsSuccess)
        {
            Console.WriteLine($"API Key: {result.Value.ApiKey[..20]}..."); // Show first 20 chars
            Console.WriteLine($"Type: {result.Value.Type}");
            Console.WriteLine($"Source: {result.Value.Source}");
            Console.WriteLine($"Service Account Email: {result.Value.ServiceAccountEmail}");
            Console.WriteLine($"Service Account Name: {result.Value.ServiceAccountName}");
            
            result.Value.Type.ShouldBe(CredentialType.ApiKey);
            result.Value.ApiKey.ShouldNotBeNullOrEmpty();
            result.Value.Source.ShouldBe("JSON File (ExxerAI API Key)");
            result.Value.ServiceAccountEmail.ShouldBe("exxerai@exxerai.iam.gserviceaccount.com");
        }
        else
        {
            Console.WriteLine($"Error: {result.Error}");
        }

        // The test should succeed if the file exists and is properly formatted
        result.IsSuccess.ShouldBeTrue("API key credentials should be resolved from GDrive.Api.json");
    }

    [Fact]
    public void GoogleDriveCredentials_ShouldSupportApiKeyType()
    {
        // Arrange & Act
        var credentials = new GoogleDriveCredentials
        {
            ApiKey = "test-api-key",
            Type = CredentialType.ApiKey,
            Source = "Test"
        };

        // Assert
        credentials.Type.ShouldBe(CredentialType.ApiKey);
        credentials.ApiKey.ShouldBe("test-api-key");
    }
} 