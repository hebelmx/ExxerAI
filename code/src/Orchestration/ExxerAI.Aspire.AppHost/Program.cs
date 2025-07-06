Console.WriteLine("🚀 Starting LocalAI Aspire Orchestrator");
Console.WriteLine("===============================================");

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ExxerAI_UI>("exxerai-ui");

// Define a container resource
var redisContainer = builder.AddContainer("redis", "redis:latest")
    .WithVolume("redis_data", "/data", isReadOnly: false);

// Example of connecting a service to the container
builder.AddProject<Projects.ExxerAI_Aspire_Dashboard>("Dashboard")
    .WithReference("redis_data", new Uri("ip"));

// Add PostgreSQL database
var postgres = builder.AddSqlServer("SqlServer");
//AddPostgres("postgres");

//var postgres = builder.AddPostgres("localai-postgres")
//    .WithHttpEndpoint(port: 5432, targetPort: 5432)
//    .WithEnvironment("POSTGRES_USER", "localai");

var database = postgres.AddDatabase("localai-db");

//// Add Redis for caching
//var redis = builder.AddRedis("redis");

// Add LocalAI container
var localai = builder.AddContainer("localai", "localai/localai", "v2.0.0")
    .WithHttpEndpoint(port: 8081, targetPort: 8080)
    .WithEnvironment("THREADS", "1");

// Add SearXNG search container
var searxng = builder.AddContainer("searxng", "searxng/searxng", "latest")
    .WithHttpEndpoint(port: 8080, targetPort: 8080);

// Add Qdrant vector database
var qdrant = builder.AddContainer("qdrant", "qdrant/qdrant", "latest")
    .WithHttpEndpoint(port: 6333, targetPort: 6333);

// Add Prometheus monitoring
var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "latest")
    .WithHttpEndpoint(port: 9090, targetPort: 9090)
    .WithBindMount("./monitoring/prometheus.yml", "/etc/prometheus/prometheus.yml", isReadOnly: true);

// Add Grafana dashboard
var grafana = builder.AddContainer("grafana", "grafana/grafana", "latest")
    .WithHttpEndpoint(port: 3002, targetPort: 3000)
    .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
    .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin");

// Build and run the application
var app = builder.Build();

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

await app.RunAsync();

builder.Build().Run();