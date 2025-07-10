using Aspire.Hosting;
using ExxerAI.Aspire.AppHost;
using Aspire.Hosting.Lifecycle;
using Grpc.Core;
using MongoDB.Driver;
using Nextended.Aspire;
using NorthernNerds.Aspire.Hosting.Neo4j;
using CommunityToolkit.Aspire.Hosting.Ollama;
using CommunityToolkit.Aspire.Hosting.RavenDB;
using CommunityToolkit.Aspire.Hosting.N8N;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Starting LocalAI Aspire Orchestrator");
        Console.WriteLine("===============================================");
        Console.WriteLine("🚀 Starting LocalAI Aspire Orchestrator");
        Console.WriteLine("===============================================");

        var builder = DistributedApplication.CreateBuilder(args);
        // Web frontend project
        builder.AddProject<Projects.ExxerAI_UI>("exxerai-ui");

        //Add N8N Resource
        var n8n = builder.AddN8N()
            .WithBasicAuth("user", "pwd");

        // Dashboard Project (references cache)
        builder.AddProject<Projects.ExxerAI_Aspire_Dashboard>("Dashboard")
            .WithReference(n8n)
            .WaitFor(n8n);

        var app = builder.Build();
        await app.RunAsync();

        Console.WriteLine("===============================================");

        Console.WriteLine();
        Console.WriteLine("✅ LocalAI Stack Services:");
        Console.WriteLine($"   🗄️  PostgreSQL: Available via service discovery");
        Console.WriteLine($"   🔄  Redis: Available via service discovery");
        Console.WriteLine($"   🤖  LocalAI API: http://localhost:8081");
        Console.WriteLine($"   🔍  SearXNG: http://localhost:8080");
        Console.WriteLine($"   📊  Qdrant: http://localhost:6333");
        Console.WriteLine($"   �  Prometheus: http://localhost:9090");
        Console.WriteLine($"   📊  Grafana: http://localhost:3002 (admin/admin)");
        Console.WriteLine();
        Console.WriteLine("🚀 Starting services...");
    }
}