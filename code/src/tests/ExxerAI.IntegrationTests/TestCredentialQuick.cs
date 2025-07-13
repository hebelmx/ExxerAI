using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExxerAi.MCPServer.Application.Services;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Quick console test for API key credentials
/// </summary>
public static class TestCredentialQuick
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🔑 Testing ExxerAI API Key Credential Resolution...");
        Console.WriteLine();

        // Create logger
        using var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var logger = loggerFactory.CreateLogger<GoogleDriveCredentialResolver>();

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

            Console.WriteLine("=== CREDENTIAL RESOLUTION RESULT ===");
            Console.WriteLine($"✅ Success: {result.IsSuccess}");
            Console.WriteLine();

            if (result.IsSuccess)
            {
                var creds = result.Value;
                Console.WriteLine($"🔑 Type: {creds.Type}");
                Console.WriteLine($"📄 Source: {creds.Source}");
                Console.WriteLine($"🔐 API Key: {creds.ApiKey[..20]}...[HIDDEN]");
                Console.WriteLine($"📧 Service Account Email: {creds.ServiceAccountEmail}");
                Console.WriteLine($"👤 Service Account Name: {creds.ServiceAccountName}");
                Console.WriteLine($"🆔 Service Account ID: {creds.ServiceAccountUniqueId}");
                Console.WriteLine();
                Console.WriteLine("🎉 API KEY CREDENTIALS SUCCESSFULLY RESOLVED!");
            }
            else
            {
                Console.WriteLine($"❌ Error: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Exception: {ex.Message}");
            Console.WriteLine($"🔍 Stack: {ex.StackTrace}");
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
} 