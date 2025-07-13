using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExxerAi.MCPServer.Application.Services;
using Meziantou.Extensions.Logging.Xunit.v3;

namespace ExxerAI.IntegrationTests;

public static class VerifyCredentials
{
    public static async Task Main(string[] args)
    {
        var logger = XUnitLogger.CreateLogger();

        logger.LogInformation("🔍 VERIFYING SERVICE ACCOUNT CREDENTIALS");
        logger.LogInformation(new string('=', 50));

        // Test 1: Environment Variable
        logger.LogInformation("\n1. Testing Environment Variable...");
        var envJson = Environment.GetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT_JSON");
        if (!string.IsNullOrEmpty(envJson))
        {
            logger.LogInformation("✅ Environment variable found");
            logger.LogInformation($"   Length: {envJson.Length} characters");
            logger.LogInformation($"   Contains 'service_account': {envJson.Contains("\"type\":\"service_account\"")}");
            logger.LogInformation($"   Contains private_key: {envJson.Contains("\"private_key\":")}");
        }
        else
        {
            logger.LogInformation("❌ Environment variable not found");
        }

        // Test 2: File Access
        logger.LogInformation("\n2. Testing File Access...");
        if (File.Exists("./exxerai.gdrive.json"))
        {
            var fileContent = await File.ReadAllTextAsync("./exxerai.gdrive.json");
            logger.LogInformation("✅ File found and readable");
            logger.LogInformation($"   Length: {fileContent.Length} characters");
            logger.LogInformation($"   Contains 'service_account': {fileContent.Contains("\"type\":\"service_account\"")}");
        }
        else
        {
            logger.LogInformation("❌ File not found");
        }

        // Test 3: Credential Resolver
        logger.LogInformation("\n3. Testing Credential Resolver...");
        try
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            var logger2 = XUnitLogger.CreateLogger<GoogleDriveCredentialResolver>();

            var configData = new Dictionary<string, string?>
            {
                ["GoogleDrive:CredentialsPath"] = "./exxerai.gdrive.json"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var resolver = new GoogleDriveCredentialResolver(configuration, logger2);
            var result = await resolver.ResolveCredentialsAsync();

            if (result.IsSuccess)
            {
                logger.LogInformation("✅ Credential resolution SUCCESS");
                var creds = result.Value;
                logger.LogInformation($"   Type: {creds.Type}");
                logger.LogInformation($"   Source: {creds.Source}");
                logger.LogInformation($"   Email: {creds.ServiceAccountEmail}");
                logger.LogInformation($"   Project: {creds.ProjectId}");
            }
            else
            {
                logger.LogInformation("❌ Credential resolution FAILED");
                foreach (var error in result.Errors)
                {
                    logger.LogInformation($"   Error: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation($"💥 Exception: {ex.Message}");
        }

        logger.LogInformation(new string('=', 50));
        logger.LogInformation("✅ Verification complete!");
    }
}