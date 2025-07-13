using Microsoft.Extensions.Logging;
using ExxerAi.MCPServer.Application.Services;
using Shouldly;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Test the modern Google-recommended Application Default Credentials approach
/// </summary>
public class ModernCredentialTest
{
    [Fact]
    public async Task ModernCredentialResolver_ShouldResolve_UsingADC()
    {
        // Arrange
        using var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger<ModernGoogleDriveCredentialResolver>();

        var resolver = new ModernGoogleDriveCredentialResolver(logger);

        // Act
        var result = await resolver.ResolveCredentialsAsync();

        // Assert & Debug Info
         logger.LogInformation("🔍 MODERN CREDENTIAL TEST RESULTS:");
         logger.LogInformation($"   Resolution Success: {result.IsSuccess}");
        
        if (result.IsSuccess)
        {
            var creds = result.Value;
             logger.LogInformation($"   ✅ Type: {creds.Type}");
             logger.LogInformation($"   ✅ Source: {creds.Source}");
             logger.LogInformation($"   ✅ Is Scoped: {creds.IsScoped}");
             logger.LogInformation($"   ✅ Has GoogleCredential: {creds.GoogleCredential != null}");
            
            // Verify modern ADC approach
            creds.Type.ShouldBe(CredentialType.ApplicationDefault);
            creds.Source.ShouldContain("Application Default Credentials");
            creds.GoogleCredential.ShouldNotBeNull();
        }
        else
        {
             logger.LogInformation("   ❌ Errors:");
            foreach (var error in result.Errors)
            {
                 logger.LogInformation($"     - {error}");
            }
            
            // If ADC is not configured, that's expected in some environments
            if (result.Errors.Any(e => e.Contains("not configured")))
            {
                 logger.LogInformation("   💡 This is expected if 'gcloud auth application-default login' hasn't been run");
                return; // Skip assertion for environments without ADC
            }
        }

        // This should pass if ADC is properly configured
        result.IsSuccess.ShouldBeTrue("Modern ADC credentials should resolve successfully");
    }

    [Fact]
    public async Task ModernCredentialResolver_ShouldCreateDriveService_Successfully()
    {
        // Arrange
        using var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger<ModernGoogleDriveCredentialResolver>();

        var resolver = new ModernGoogleDriveCredentialResolver(logger);

        try
        {
            // Act
            using var driveService = await resolver.CreateDriveServiceAsync();

            // Assert
             logger.LogInformation("🚀 DRIVE SERVICE TEST RESULTS:");
             logger.LogInformation($"   ✅ Service Created: {driveService != null}");
             logger.LogInformation($"   ✅ Application Name: {driveService.ApplicationName}");

            driveService.ShouldNotBeNull();
            driveService.ApplicationName.ShouldBe("ExxerAI Drive Integration");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Failed to resolve credentials"))
        {
             logger.LogInformation("   💡 Drive service creation failed - ADC not configured");
             logger.LogInformation($"   Details: {ex.Message}");
            
            // Skip test if ADC is not configured
            return;
        }
    }

    [Fact]
    public async Task ModernCredentialResolver_ShouldTestCredential_Successfully()
    {
        // Arrange
        using var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger<ModernGoogleDriveCredentialResolver>();

        var resolver = new ModernGoogleDriveCredentialResolver(logger);

        // Act
        var testResult = await resolver.TestCredentialAsync();

        // Assert
         logger.LogInformation("🧪 CREDENTIAL TEST RESULTS:");
         logger.LogInformation($"   Test Success: {testResult.IsSuccess}");
        
        if (testResult.IsSuccess)
        {
             logger.LogInformation("   ✅ Credential is working and can access Google Drive API!");
            testResult.Value.ShouldBeTrue();
        }
        else
        {
             logger.LogInformation("   ❌ Test Errors:");
            foreach (var error in testResult.Errors)
            {
                 logger.LogInformation($"     - {error}");
            }
            
            // If credentials aren't configured, that's expected
            if (testResult.Errors.Any(e => e.Contains("not configured") || e.Contains("Could not load")))
            {
                 logger.LogInformation("   💡 This is expected if ADC hasn't been configured");
                return; // Skip assertion
            }
        }

        // Only assert success if we have credentials configured
        testResult.IsSuccess.ShouldBeTrue("Credential test should succeed with properly configured ADC");
    }
} 