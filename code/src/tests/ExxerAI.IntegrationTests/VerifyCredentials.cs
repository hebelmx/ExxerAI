using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExxerAi.MCPServer.Application.Services;

namespace ExxerAI.IntegrationTests;

public static class VerifyCredentials
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🔍 VERIFYING SERVICE ACCOUNT CREDENTIALS");
        Console.WriteLine(new string('=', 50));

        // Test 1: Environment Variable
        Console.WriteLine("\n1. Testing Environment Variable...");
        var envJson = Environment.GetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT_JSON");
        if (!string.IsNullOrEmpty(envJson))
        {
            Console.WriteLine("✅ Environment variable found");
            Console.WriteLine($"   Length: {envJson.Length} characters");
            Console.WriteLine($"   Contains 'service_account': {envJson.Contains("\"type\":\"service_account\"")}");
            Console.WriteLine($"   Contains private_key: {envJson.Contains("\"private_key\":")}");
        }
        else
        {
            Console.WriteLine("❌ Environment variable not found");
        }

        // Test 2: File Access
        Console.WriteLine("\n2. Testing File Access...");
        if (File.Exists("./exxerai.gdrive.json"))
        {
            var fileContent = await File.ReadAllTextAsync("./exxerai.gdrive.json");
            Console.WriteLine("✅ File found and readable");
            Console.WriteLine($"   Length: {fileContent.Length} characters");
            Console.WriteLine($"   Contains 'service_account': {fileContent.Contains("\"type\":\"service_account\"")}");
        }
        else
        {
            Console.WriteLine("❌ File not found");
        }

        // Test 3: Credential Resolver
        Console.WriteLine("\n3. Testing Credential Resolver...");
        try
        {
            using var loggerFactory = LoggerFactory.Create(builder => 
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            var logger = loggerFactory.CreateLogger<GoogleDriveCredentialResolver>();

            var configData = new Dictionary<string, string?>
            {
                ["GoogleDrive:CredentialsPath"] = "./exxerai.gdrive.json"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var resolver = new GoogleDriveCredentialResolver(configuration, logger);
            var result = await resolver.ResolveCredentialsAsync();

            if (result.IsSuccess)
            {
                Console.WriteLine("✅ Credential resolution SUCCESS");
                var creds = result.Value;
                Console.WriteLine($"   Type: {creds.Type}");
                Console.WriteLine($"   Source: {creds.Source}");
                Console.WriteLine($"   Email: {creds.ServiceAccountEmail}");
                Console.WriteLine($"   Project: {creds.ProjectId}");
            }
            else
            {
                Console.WriteLine("❌ Credential resolution FAILED");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   Error: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Exception: {ex.Message}");
        }

        Console.WriteLine(new string('=', 50));
        Console.WriteLine("✅ Verification complete!");
    }
} 