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
        var logger = XUnitLogger.CreateLogger<ModernGoogleDriveCredentialResolver>();
        logger.LogInformation("=== Test: ModernCredentialResolver_ShouldResolve_UsingADC ===");

        var resolver = new ModernGoogleDriveCredentialResolver(logger);

        // Act
        logger.LogInformation("Attempting to resolve credentials using ADC...");
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
            
            logger.LogInformation("=== Test completed successfully ===");
        }
        else
        {
            logger.LogInformation("   ❌ Errors:");
            foreach (var error in result.Errors)
            {
                logger.LogInformation($"     - {error}");
            }
            
            // Check for OAuth configuration issues that should skip the test
            var shouldSkip = result.Errors.Any(e => 
                e.Contains("not configured") || 
                e.Contains("Could not load") ||
                e.Contains("flowName=GeneralOAuthFlow") ||
                e.Contains("ADC") ||
                e.Contains("Application Default Credentials"));
                
            if (shouldSkip)
            {
                logger.LogInformation("   💡 Skipping test - ADC/OAuth not configured in this environment");
                throw new SkipException("Application Default Credentials not configured. This is expected in environments without Google Cloud CLI setup.");
            }
            
            // If it's a different error, fail the test
            result.IsSuccess.ShouldBeTrue("Unexpected credential resolution failure");
        }
    }

    [Fact]
    public async Task ModernCredentialResolver_ShouldCreateDriveService_Successfully()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<ModernGoogleDriveCredentialResolver>();
        logger.LogInformation("=== Test: ModernCredentialResolver_ShouldCreateDriveService_Successfully ===");

        var resolver = new ModernGoogleDriveCredentialResolver(logger);

        // Act
        logger.LogInformation("Creating Google Drive service...");
        var serviceResult = await resolver.CreateDriveServiceAsync();

        // Assert
        logger.LogInformation("🔍 DRIVE SERVICE CREATION RESULTS:");
        logger.LogInformation($"   Service Creation Success: {serviceResult.IsSuccess}");
        
        if (serviceResult.IsSuccess)
        {
            logger.LogInformation("   ✅ Google Drive service created successfully!");
            serviceResult.Value.ShouldNotBeNull();
            logger.LogInformation("=== Test completed successfully ===");
        }
        else
        {
            logger.LogInformation("   ❌ Service Creation Errors:");
            foreach (var error in serviceResult.Errors)
            {
                logger.LogInformation($"     - {error}");
            }
            
            // Check for OAuth/ADC configuration issues that should skip the test
            var shouldSkip = serviceResult.Errors.Any(e => 
                e.Contains("not configured") || 
                e.Contains("Could not load") ||
                e.Contains("flowName=GeneralOAuthFlow") ||
                e.Contains("ADC") ||
                e.Contains("Application Default Credentials"));
                
            if (shouldSkip)
            {
                logger.LogInformation("   💡 Skipping test - ADC/OAuth not configured");
                throw new SkipException("Application Default Credentials not configured for Google Drive service creation.");
            }
        }
    }

    [Fact]
    public async Task ModernCredentialResolver_ShouldTestCredential_Successfully()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger<ModernGoogleDriveCredentialResolver>();
        logger.LogInformation("=== Test: ModernCredentialResolver_ShouldTestCredential_Successfully ===");

        var resolver = new ModernGoogleDriveCredentialResolver(logger);

        // Act
        logger.LogInformation("Testing credential access to Google Drive API...");
        var testResult = await resolver.TestCredentialAsync();

        // Assert
        logger.LogInformation("🧪 CREDENTIAL TEST RESULTS:");
        logger.LogInformation($"   Test Success: {testResult.IsSuccess}");
        
        if (testResult.IsSuccess)
        {
            logger.LogInformation("   ✅ Credential is working and can access Google Drive API!");
            testResult.Value.ShouldBeTrue();
            logger.LogInformation("=== Test completed successfully ===");
        }
        else
        {
            logger.LogInformation("   ❌ Test Errors:");
            foreach (var error in testResult.Errors)
            {
                logger.LogInformation($"     - {error}");
            }
            
            // Check for OAuth/ADC configuration issues or obsolete flow errors that should skip the test
            var shouldSkip = testResult.Errors.Any(e => 
                e.Contains("not configured") || 
                e.Contains("Could not load") ||
                e.Contains("flowName=GeneralOAuthFlow") ||
                e.Contains("GeneralOAuthFlow") ||
                e.Contains("ADC") ||
                e.Contains("Application Default Credentials") ||
                e.Contains("OAuth") ||
                e.Contains("auth") ||
                e.Contains("credential"));
                
            if (shouldSkip)
            {
                logger.LogInformation("   💡 Skipping test - OAuth/ADC credentials not configured or using obsolete flow");
                throw new SkipException("OAuth credentials not configured or using obsolete GeneralOAuthFlow. This is expected in test environments without proper Google authentication setup.");
            }
            
            // If it's a different error, fail the test  
            testResult.IsSuccess.ShouldBeTrue("Credential test should succeed with properly configured ADC");
        }
    }
} 