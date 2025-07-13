using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExxerAI.MCPServer.Application.Services;
using Shouldly;
using NSubstitute;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Test to verify Google Drive Service Account credential resolution
/// </summary>
public class GoogleDriveServiceAccountTest
{
    [Fact]
    public async Task GoogleDriveCredentialResolver_ShouldParseServiceAccount_FromExxerAiGdriveJson()
    {
        // Arrange
        var logger = Substitute.For<ILogger<GoogleDriveCredentialResolver>>();
        
        // Build configuration pointing to the service account file
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
        result.IsSuccess.ShouldBeTrue($"Expected success but got failure: {string.Join(", ", result.Errors)}");
        
        var credentials = result.Value;
        credentials.ShouldNotBeNull();
        credentials.Type.ShouldBe(CredentialType.ServiceAccount);
        credentials.ServiceAccountEmail.ShouldBe("exxerai@exxerai.iam.gserviceaccount.com");
        credentials.ProjectId.ShouldBe("exxerai");
        credentials.Source.ShouldBe("JSON File (Service Account)");
        credentials.ServiceAccountJson.ShouldNotBeEmpty();
        
        // Verify the JSON contains the private key (critical for service account auth)
        credentials.ServiceAccountJson.ShouldContain("\"private_key\":");
        credentials.ServiceAccountJson.ShouldContain("-----BEGIN PRIVATE KEY-----");
        
         logger.LogInformation($"✅ Service Account Parsed Successfully:");
         logger.LogInformation($"   📧 Email: {credentials.ServiceAccountEmail}");
         logger.LogInformation($"   🆔 Project: {credentials.ProjectId}");
         logger.LogInformation($"   📄 Source: {credentials.Source}");
         logger.LogInformation($"   🔑 Has Private Key: {(credentials.ServiceAccountJson.Contains("private_key") ? "Yes" : "No")}");
    }
} 