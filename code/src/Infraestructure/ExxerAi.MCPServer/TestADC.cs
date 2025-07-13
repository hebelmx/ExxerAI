using Microsoft.Extensions.Logging;
using ExxerAi.MCPServer.Application.Services;

namespace ExxerAi.MCPServer;

/// <summary>
/// Simple console test for ADC credentials
/// </summary>
public class TestADC
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 TESTING MODERN ADC CREDENTIALS");
        Console.WriteLine(new string('=', 50));

        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger<ModernGoogleDriveCredentialResolver>();

        var resolver = new ModernGoogleDriveCredentialResolver(logger);

        try
        {
            Console.WriteLine("\n🔍 Testing credential resolution...");
            var credResult = await resolver.ResolveCredentialsAsync();
            
            if (credResult.IsSuccess)
            {
                var creds = credResult.Value;
                Console.WriteLine("✅ SUCCESS - Credentials resolved!");
                Console.WriteLine($"   📝 Type: {creds.Type}");
                Console.WriteLine($"   📍 Source: {creds.Source}");
                Console.WriteLine($"   🔧 Is Scoped: {creds.IsScoped}");
            }
            else
            {
                Console.WriteLine("❌ FAILED - Credential resolution failed:");
                foreach (var error in credResult.Errors)
                {
                    Console.WriteLine($"   💥 {error}");
                }
                return;
            }

            Console.WriteLine("\n🧪 Testing API call...");
            var testResult = await resolver.TestCredentialAsync();
            
            if (testResult.IsSuccess)
            {
                Console.WriteLine("✅ SUCCESS - API call worked!");
                Console.WriteLine("   🌐 Connected to Google Drive API successfully!");
            }
            else
            {
                Console.WriteLine("❌ FAILED - API test failed:");
                foreach (var error in testResult.Errors)
                {
                    Console.WriteLine($"   💥 {error}");
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 UNEXPECTED ERROR: {ex.Message}");
        }

        Console.WriteLine(new string('=', 50));
        Console.WriteLine("🏁 TEST COMPLETED");
    }
} 