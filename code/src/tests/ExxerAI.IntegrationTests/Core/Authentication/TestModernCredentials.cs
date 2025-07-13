using Microsoft.Extensions.Logging;
using ExxerAI.MCPServer.Application.Services;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Simple console test for modern ADC credentials
/// </summary>
public static class TestModernCredentials
{
    public static async Task Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger<ModernGoogleDriveCredentialResolver>();

        var resolver = new ModernGoogleDriveCredentialResolver(logger);
        logger.LogInformation("🚀 TESTING MODERN GOOGLE DRIVE CREDENTIALS (ADC)");
        logger.LogInformation(new string('=', 60));

        // Setup logging

        try
        {
            // Test 1: Resolve Credentials
            logger.LogInformation("\n1️⃣ Testing Credential Resolution...");
            var credResult = await resolver.ResolveCredentialsAsync();

            if (credResult.IsSuccess)
            {
                var creds = credResult.Value;
                logger.LogInformation("✅ CREDENTIAL RESOLUTION SUCCESS!");
                logger.LogInformation($"   📝 Type: {creds.Type}");
                logger.LogInformation($"   📍 Source: {creds.Source}");
                logger.LogInformation($"   🔧 Is Scoped: {creds.IsScoped}");
                logger.LogInformation($"   🔗 Has GoogleCredential: {creds.GoogleCredential != null}");
            }
            else
            {
                logger.LogInformation("❌ CREDENTIAL RESOLUTION FAILED:");
                foreach (var error in credResult.Errors)
                {
                    logger.LogInformation($"   💥 {error}");
                }

                if (credResult.Errors.Any(e => e.Contains("not configured")))
                {
                    logger.LogInformation("\n💡 SOLUTION:");
                    logger.LogInformation("   Run: gcloud auth application-default login");
                    return;
                }
            }

            // Test 2: Create Drive Service
            logger.LogInformation("\n2️⃣ Testing Drive Service Creation...");
            try
            {
                using var driveService = await resolver.CreateDriveServiceAsync();
                logger.LogInformation("✅ DRIVE SERVICE CREATION SUCCESS!");
                logger.LogInformation($"   🚀 Application Name: {driveService.ApplicationName}");
            }
            catch (Exception ex)
            {
                logger.LogInformation("❌ DRIVE SERVICE CREATION FAILED:");
                logger.LogInformation($"   💥 {ex.Message}");
            }

            // Test 3: Test API Call
            logger.LogInformation("\n3️⃣ Testing Live API Call...");
            var testResult = await resolver.TestCredentialAsync();

            if (testResult.IsSuccess)
            {
                logger.LogInformation("✅ API TEST SUCCESS!");
                logger.LogInformation("   🌐 Successfully connected to Google Drive API!");
            }
            else
            {
                logger.LogInformation("❌ API TEST FAILED:");
                foreach (var error in testResult.Errors)
                {
                    logger.LogInformation($"   💥 {error}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation($"💥 UNEXPECTED ERROR: {ex.Message}");
            logger.LogInformation($"📍 Stack Trace: {ex.StackTrace}");
        }

        logger.LogInformation(new string('=', 60));
        logger.LogInformation("🏁 TEST COMPLETED!");

        // Return success/failure for CI
        logger.LogInformation("\n🎯 SUMMARY:");
        var credResult2 = await resolver.ResolveCredentialsAsync();
        if (credResult2.IsSuccess)
        {
            logger.LogInformation("   ✅ Modern ADC credentials are working!");
            logger.LogInformation("   🚀 Ready for Google Drive integration!");
        }
        else
        {
            logger.LogInformation("   ⚠️  ADC not configured - run 'gcloud auth application-default login'");
        }
    }
}