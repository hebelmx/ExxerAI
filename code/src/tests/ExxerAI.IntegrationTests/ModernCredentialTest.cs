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
        Console.WriteLine("🔍 MODERN CREDENTIAL TEST RESULTS:");
        Console.WriteLine($"   Resolution Success: {result.IsSuccess}");
        
        if (result.IsSuccess)
        {
            var creds = result.Value;
            Console.WriteLine($"   ✅ Type: {creds.Type}");
            Console.WriteLine($"   ✅ Source: {creds.Source}");
            Console.WriteLine($"   ✅ Is Scoped: {creds.IsScoped}");
            Console.WriteLine($"   ✅ Has GoogleCredential: {creds.GoogleCredential != null}");
            
            // Verify modern ADC approach
            creds.Type.ShouldBe(CredentialType.ApplicationDefault);
            creds.Source.ShouldContain("Application Default Credentials");
            creds.GoogleCredential.ShouldNotBeNull();
        }
        else
        {
            Console.WriteLine("   ❌ Errors:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"     - {error}");
            }
            
            // If ADC is not configured, that's expected in some environments
            if (result.Errors.Any(e => e.Contains("not configured")))
            {
                Console.WriteLine("   💡 This is expected if 'gcloud auth application-default login' hasn't been run");
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
            Console.WriteLine("🚀 DRIVE SERVICE TEST RESULTS:");
            Console.WriteLine($"   ✅ Service Created: {driveService != null}");
            Console.WriteLine($"   ✅ Application Name: {driveService.ApplicationName}");

            driveService.ShouldNotBeNull();
            driveService.ApplicationName.ShouldBe("ExxerAI Drive Integration");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Failed to resolve credentials"))
        {
            Console.WriteLine("   💡 Drive service creation failed - ADC not configured");
            Console.WriteLine($"   Details: {ex.Message}");
            
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
        Console.WriteLine("🧪 CREDENTIAL TEST RESULTS:");
        Console.WriteLine($"   Test Success: {testResult.IsSuccess}");
        
        if (testResult.IsSuccess)
        {
            Console.WriteLine("   ✅ Credential is working and can access Google Drive API!");
            testResult.Value.ShouldBeTrue();
        }
        else
        {
            Console.WriteLine("   ❌ Test Errors:");
            foreach (var error in testResult.Errors)
            {
                Console.WriteLine($"     - {error}");
            }
            
            // If credentials aren't configured, that's expected
            if (testResult.Errors.Any(e => e.Contains("not configured") || e.Contains("Could not load")))
            {
                Console.WriteLine("   💡 This is expected if ADC hasn't been configured");
                return; // Skip assertion
            }
        }

        // Only assert success if we have credentials configured
        testResult.IsSuccess.ShouldBeTrue("Credential test should succeed with properly configured ADC");
    }
} 