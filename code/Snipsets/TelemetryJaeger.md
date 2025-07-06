Minimal OpenTelemetry Setup for .NET Console App (Traces & Logs via OTLP/HTTP)
This guide outlines the minimum code and configuration to send OpenTelemetry traces and from Semantic Kernel in a .NET console application to an OTLP-compatible backend (e.g., Langfuse, Jaeger) using the HTTP/protobuf protocol, including proxy support and custom SSL certificate validation if you want to inspect traffic using Fiddler or a similar proxy.

1. Project Setup (NuGet Packages)
Ensure your .NET console project (.csproj) includes the following OpenTelemetry packages:

<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Microsoft.SemanticKernel" Version="1.10.0" />
    <PackageReference Include="OpenTelemetry" Version="1.7.0" />
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.7.0" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.7.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
  </ItemGroup>
</Project>
2. Environment Variables (.env file)
I'm using DotnetEnv, but you can use whatever you want. If you follow my suggestion, create a .env file inside your entry point project and make sure it's being copied to the build directory:

# --- OTLP Endpoint Configuration ---
# Example with Langfuse
LANGFUSE_OTLP_ENDPOINT=https://us.cloud.langfuse.com/api/public/otel/v1/traces
LANGFUSE_PUBLIC_KEY=your_langfuse_public_key
LANGFUSE_SECRET_KEY=your_langfuse_secret_key

# --- Proxy Configuration (Optional) ---
# If your application needs to go through a proxy to reach the OTLP endpoint or other services
PROXY_URL=http://your.proxy.server:port # e.g., http://127.0.0.1:8888 for Fiddler
3. C# Code (Program.cs)
using DotNetEnv;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using System.Net;
using System.Net.Http;

namespace MinimalOtelExample
{
    class Program
    {
        private static readonly ActivitySource MyActivitySource = new("MinimalOtelExample.App");

        // This method is completely optional - It's cool if you want to inspect it through a proxy like Fiddler
        static HttpClient CreateCustomHttpClient()
        {
            var handler = new HttpClientHandler();

            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            string? proxyUrl = Env.GetString("PROXY_URL");
            if (!string.IsNullOrEmpty(proxyUrl))
            {
                var webProxy = new WebProxy(new Uri(proxyUrl));
                if (!string.IsNullOrEmpty(proxyUsername) && !string.IsNullOrEmpty(proxyPassword))
                {
                    webProxy.Credentials = new NetworkCredential(proxyUsername, proxyPassword);
                }
                handler.Proxy = webProxy;
                handler.UseProxy = true;
            }
            else
            {
                handler.UseProxy = false;
            }
            return new HttpClient(handler);
        }

        // Configures OTLP Exporter options (shared for traces and logs)
        static void ConfigureOtlpExporter(OtlpExporterOptions options)
        {
            string? otlpEndpoint = Env.GetString("LANGFUSE_OTLP_ENDPOINT");
            string? publicKey = Env.GetString("LANGFUSE_PUBLIC_KEY");
            string? secretKey = Env.GetString("LANGFUSE_SECRET_KEY");
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes($"{publicKey}:{secretKey}");
            string base64EncodedAuth = Convert.ToBase64String(plainTextBytes);

            options.Endpoint = new Uri(otlpEndpoint);
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
            options.Headers = $"Authorization=Basic {base64EncodedAuth}";                
            options.HttpClientFactory = CreateCustomHttpClient;
        }

        static ILoggerFactory CreateAppLoggerFactory(ResourceBuilder resourceBuilder)
        {
            return LoggerFactory.Create(builder =>
            {
                builder
                    .AddOpenTelemetry(options =>
                    {
                        options.SetResourceBuilder(resourceBuilder);
                        options.AddOtlpExporter(ConfigureOtlpExporter);
                        options.IncludeFormattedMessage = true;
                        options.IncludeScopes = true;
                        
                    })
                    .AddConsole() // Also log to console for local debugging
                    .SetMinimumLevel(LogLevel.Debug);
            });
        }

        static TracerProvider CreateAppTracerProvider(ResourceBuilder resourceBuilder)
        {
            var tracerProviderBuilder = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddSource(MyActivitySource.Name)                
                .AddConsoleExporter() // Also export traces to console for local debugging
                .AddOtlpExporter(ConfigureOtlpExporter);

            return tracerProviderBuilder.Build()!;
        }

        static async Task Main(string[] args)
        {
            Env.Load();

            
            AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnostics", true);
            AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: "MyMinimalOtelApp", serviceVersion: "1.0.0");

            using var loggerFactory = CreateAppLoggerFactory(resourceBuilder);
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("Application starting...");

            using var tracerProvider = CreateAppTracerProvider(resourceBuilder);

            /* DO ANYTHING WITH SEMANTIC KERNEL HERE AND TRACES WILL BE PICKED UP */

            // --- Manually trigger a custom trace (Activity) ---
            using (var activity = MyActivitySource.StartActivity("SampleOperation"))
            {
                logger.LogInformation("Inside SampleOperation Activity.");
                activity?.SetTag("my.custom.tag", "TagValue123");
                activity?.AddEvent(new ActivityEvent("Something interesting happened here!"));
                
                try
                {
                    // Simulate some work
                    await Task.Delay(150);
                    
                    activity?.SetStatus(ActivityStatusCode.Ok, "Operation completed successfully");
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, $"Operation failed: {ex.Message}");
                    activity?.RecordException(ex); // Record exception details
                    logger.LogError(ex, "Error during SampleOperation");
                    throw;
                }
            }
            logger.LogInformation("SampleOperation finished.");
            // --- End of custom trace ---


            logger.LogInformation("Application finished. Flushing telemetry...");

            // Explicitly flush telemetry data before application exits
            // This is crucial for console apps to ensure data is sent.
            tracerProvider.ForceFlush();
            // For logs, flushing is often handled by disposing the logger factory or specific log processors,
            // but ForceFlush on tracerProvider is the most direct for traces.
            // You might need to wait briefly for asynchronous export:
            // await Task.Delay(5000); // Adjust delay as needed if flushing seems incomplete

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
