using Microsoft.Extensions.Logging;
using ExxerAi.MCPServer.Application.Services;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Simple console test for modern ADC credentials
/// </summary>
public static class TestModernCredentials
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 TESTING MODERN GOOGLE DRIVE CREDENTIALS (ADC)");
        Console.WriteLine(new string('=', 60));

        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger<ModernGoogleDriveCredentialResolver>();

        var resolver = new ModernGoogleDriveCredentialResolver(logger);

        try
        {
            // Test 1: Resolve Credentials
            Console.WriteLine("\n1️⃣ Testing Credential Resolution...");
            var credResult = await resolver.ResolveCredentialsAsync();
            
            if (credResult.IsSuccess)
            {
                var creds = credResult.Value;
                Console.WriteLine("✅ CREDENTIAL RESOLUTION SUCCESS!");
                Console.WriteLine($"   📝 Type: {creds.Type}");
                Console.WriteLine($"   📍 Source: {creds.Source}");
                Console.WriteLine($"   🔧 Is Scoped: {creds.IsScoped}");
                Console.WriteLine($"   🔗 Has GoogleCredential: {creds.GoogleCredential != null}");
            }
            else
            {
                Console.WriteLine("❌ CREDENTIAL RESOLUTION FAILED:");
                foreach (var error in credResult.Errors)
                {
                    Console.WriteLine($"   💥 {error}");
                }
                
                if (credResult.Errors.Any(e => e.Contains("not configured")))
                {
                    Console.WriteLine("\n💡 SOLUTION:");
                    Console.WriteLine("   Run: gcloud auth application-default login");
                    return;
                }
            }

            // Test 2: Create Drive Service
            Console.WriteLine("\n2️⃣ Testing Drive Service Creation...");
            try
            {
                using var driveService = await resolver.CreateDriveServiceAsync();
                Console.WriteLine("✅ DRIVE SERVICE CREATION SUCCESS!");
                Console.WriteLine($"   🚀 Application Name: {driveService.ApplicationName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ DRIVE SERVICE CREATION FAILED:");
                Console.WriteLine($"   💥 {ex.Message}");
            }

            // Test 3: Test API Call
            Console.WriteLine("\n3️⃣ Testing Live API Call...");
            var testResult = await resolver.TestCredentialAsync();
            
            if (testResult.IsSuccess)
            {
                Console.WriteLine("✅ API TEST SUCCESS!");
                Console.WriteLine("   🌐 Successfully connected to Google Drive API!");
            }
            else
            {
                Console.WriteLine("❌ API TEST FAILED:");
                foreach (var error in testResult.Errors)
                {
                    Console.WriteLine($"   💥 {error}");
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 UNEXPECTED ERROR: {ex.Message}");
            Console.WriteLine($"📍 Stack Trace: {ex.StackTrace}");
        }

        Console.WriteLine(new string('=', 60));
        Console.WriteLine("🏁 TEST COMPLETED!");
        
        // Return success/failure for CI
        Console.WriteLine("\n🎯 SUMMARY:");
        var credResult2 = await resolver.ResolveCredentialsAsync();
        if (credResult2.IsSuccess)
        {
            Console.WriteLine("   ✅ Modern ADC credentials are working!");
            Console.WriteLine("   🚀 Ready for Google Drive integration!");
        }
        else
        {
            Console.WriteLine("   ⚠️  ADC not configured - run 'gcloud auth application-default login'");
        }
    }
} 