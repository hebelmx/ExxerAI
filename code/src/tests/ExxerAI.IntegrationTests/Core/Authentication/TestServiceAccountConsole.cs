using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExxerAi.MCPServer.Application.Services;
using Meziantou.Extensions.Logging.Xunit.v3;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Simple console test for service account credentials
/// </summary>
public static class TestServiceAccountConsole
{
    public static async Task Main(string[] args)
    {
        var logger = XUnitLogger.CreateLogger<GoogleDriveCredentialResolver>();
        logger.LogInformation("🔑 Testing ExxerAI Service Account Credential Resolution...");
        logger.LogInformation(new string('=', 60));

        try
        {
            // Create logger
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

            // Build configuration
            var configData = new Dictionary<string, string?>
            {
                ["GoogleDrive:CredentialsPath"] = "./exxerai.gdrive.json"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Test the resolver
            var resolver = new GoogleDriveCredentialResolver(configuration, logger);

            logger.LogInformation("🔍 Resolving credentials...");
            var result = await resolver.ResolveCredentialsAsync();

            if (result.IsSuccess)
            {
                var creds = result.Value;
                logger.LogInformation("✅ SUCCESS! Service Account credentials resolved:");
                logger.LogInformation($"   📧 Email: {creds.ServiceAccountEmail}");
                logger.LogInformation($"   🆔 Project: {creds.ProjectId}");
                logger.LogInformation($"   📄 Source: {creds.Source}");
                logger.LogInformation($"   🔐 Type: {creds.Type}");
                logger.LogInformation($"   🔑 Has JSON: {(!string.IsNullOrEmpty(creds.ServiceAccountJson) ? "Yes" : "No")}");

                if (!string.IsNullOrEmpty(creds.ServiceAccountJson))
                {
                    logger.LogInformation($"   🗝️ Has Private Key: {(creds.ServiceAccountJson.Contains("private_key") ? "Yes" : "No")}");
                }
            }
            else
            {
                logger.LogInformation("❌ FAILED to resolve credentials:");
                foreach (var error in result.Errors)
                {
                    logger.LogInformation($"   💥 {error}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation($"💥 Exception occurred: {ex.Message}");
            logger.LogInformation($"📍 Stack trace: {ex.StackTrace}");
        }

        logger.LogInformation(new string('=', 60));
        logger.LogInformation("🏁 Test completed.");
    }
}