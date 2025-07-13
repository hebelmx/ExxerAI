using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExxerAi.MCPServer.Application.Services;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Simple console test for service account credentials
/// </summary>
public static class TestServiceAccountConsole
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🔑 Testing ExxerAI Service Account Credential Resolution...");
        Console.WriteLine(new string('=', 60));

        try
        {
            // Create logger
            using var loggerFactory = LoggerFactory.Create(builder => 
                builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var logger = loggerFactory.CreateLogger<GoogleDriveCredentialResolver>();

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
            
            Console.WriteLine("🔍 Resolving credentials...");
            var result = await resolver.ResolveCredentialsAsync();

            if (result.IsSuccess)
            {
                var creds = result.Value;
                Console.WriteLine("✅ SUCCESS! Service Account credentials resolved:");
                Console.WriteLine($"   📧 Email: {creds.ServiceAccountEmail}");
                Console.WriteLine($"   🆔 Project: {creds.ProjectId}");
                Console.WriteLine($"   📄 Source: {creds.Source}");
                Console.WriteLine($"   🔐 Type: {creds.Type}");
                Console.WriteLine($"   🔑 Has JSON: {(!string.IsNullOrEmpty(creds.ServiceAccountJson) ? "Yes" : "No")}");
                
                if (!string.IsNullOrEmpty(creds.ServiceAccountJson))
                {
                    Console.WriteLine($"   🗝️ Has Private Key: {(creds.ServiceAccountJson.Contains("private_key") ? "Yes" : "No")}");
                }
            }
            else
            {
                Console.WriteLine("❌ FAILED to resolve credentials:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   💥 {error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Exception occurred: {ex.Message}");
            Console.WriteLine($"📍 Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine(new string('=', 60));
        Console.WriteLine("🏁 Test completed.");
    }
} 