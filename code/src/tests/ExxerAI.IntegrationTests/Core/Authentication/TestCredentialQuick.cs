using ExxerAI.MCPServer.Application.Services;
using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Quick console test for API key credentials
/// </summary>
public static class TestCredentialQuick
{
    public static async Task Main(string[] args)
    {
        var logger = XUnitLogger.CreateLogger<GoogleDriveCredentialResolver>();

        logger.LogInformation("🔑 Testing ExxerAI API Key Credential Resolution...");
        logger.LogInformation("TestCredentialQuick");

        // Create logger
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        // Build configuration
        var configData = new Dictionary<string, string?>
        {
            ["GoogleDrive:CredentialsPath"] = "./GDrive.Api.json"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var resolver = new GoogleDriveCredentialResolver(configuration, logger);

        try
        {
            // Test credential resolution
            var result = await resolver.ResolveCredentialsAsync();

            logger.LogInformation("=== CREDENTIAL RESOLUTION RESULT ===");
            logger.LogInformation($"✅ Success: {result.IsSuccess}");
            logger.LogInformation("GoogleDriveCredentialResolver");

            if (result.IsSuccess)
            {
                var creds = result.Value;
                logger.LogInformation($"🔑 Type: {creds.Type}");
                logger.LogInformation($"📄 Source: {creds.Source}");
                logger.LogInformation($"🔐 API Key: {creds.ApiKey[..20]}...[HIDDEN]");
                logger.LogInformation($"📧 Service Account Email: {creds.ServiceAccountEmail}");
                logger.LogInformation($"👤 Service Account Name: {creds.ServiceAccountName}");
                logger.LogInformation($"🆔 Service Account ID: {creds.ServiceAccountUniqueId}");
                logger.LogInformation("GoogleDriveCredentialResolver");
                logger.LogInformation("🎉 API KEY CREDENTIALS SUCCESSFULLY RESOLVED!");
            }
            else
            {
                logger.LogInformation($"❌ Error: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation($"💥 Exception: {ex.Message}");
            logger.LogInformation($"🔍 Stack: {ex.StackTrace}");
        }

        logger.LogInformation("=== CREDENTIAL RESOLUTION RESULT ===");
        logger.LogInformation("Press any key to exit...");
        Console.ReadKey();
    }
}