using ExxerAI.MCPServer.Application.Services;
using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        var logger = XUnitLogger.CreateLogger<GoogleDriveCredentialResolver>();
        logger.LogInformation("ServiceAccount_ShouldResolve_FromEnvironmentVariable");
        // Use minimal configuration to test environment variable priority
        var logger2 = XUnitLogger.CreateLogger();
        logger.LogInformation("Non generic logger");
        var configuration = new ConfigurationBuilder().Build();
        var resolver = new GoogleDriveCredentialResolver(configuration, logger);

        // Act
        var result = await resolver.ResolveCredentialsAsync();

        // Assert & Debug Info
        logger.LogInformation($"🔍 CREDENTIAL VERIFICATION RESULTS:");
        logger.LogInformation($"   Environment Variable Set: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT_JSON"))}");
        logger.LogInformation($"   Resolution Success: {result.IsSuccess}");

        if (result.IsSuccess)
        {
            var creds = result.Value;
            logger.LogInformation($"   Type: {creds.Type}");
            logger.LogInformation($"   Source: {creds.Source}");
            logger.LogInformation($"   Email: {creds.ServiceAccountEmail}");
            logger.LogInformation($"   Project: {creds.ProjectId}");
            logger.LogInformation($"   Has JSON: {!string.IsNullOrEmpty(creds.ServiceAccountJson)}");

            // Verify it's actually a service account
            creds.Type.ShouldBe(CredentialType.ServiceAccount);
            creds.ServiceAccountEmail.ShouldBe("exxerai@exxerai.iam.gserviceaccount.com");
            creds.ProjectId.ShouldBe("exxerai");
            creds.Source.ShouldContain("Environment Variables");
        }
        else
        {
            logger.LogInformation($"   Errors:");
            foreach (var error in result.Errors)
            {
                logger.LogInformation($"     - {error}");
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
        //var logger = XUnitLogger.CreateLogger<GoogleDriveCredentialResolver>();
        var logger = XUnitLogger.CreateLogger<GoogleDriveCredentialResolver>();
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
            logger.LogInformation($"🔍 FILE CREDENTIAL VERIFICATION:");
            logger.LogInformation($"   File Exists: {File.Exists("./exxerai.gdrive.json")}");
            logger.LogInformation($"   Resolution Success: {result.IsSuccess}");

            if (result.IsSuccess)
            {
                var creds = result.Value;
                logger.LogInformation($"   Type: {creds.Type}");
                logger.LogInformation($"   Source: {creds.Source}");

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