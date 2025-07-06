using LocalAI.Aspire.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// === CORE INFRASTRUCTURE ===
// Add Supabase stack (PostgreSQL + Auth + Realtime + Storage)
var postgres = builder.AddSupabaseStack();

// Add Redis for caching
var redis = builder.AddRedis("redis")
    .WithDataVolume();

// === AI SERVICES ===
// Add LocalAI stack (LocalAI + Open WebUI)
var localAi = builder.AddLocalAIStack();

// Add SearXNG for private search
var searxng = builder.AddContainer("searxng", "searxng/searxng")
    .WithHttpEndpoint(port: 8080, targetPort: 8080)
    .WithEnvironment("SEARXNG_BASE_URL", "http://localhost:8080/")
    .WithBindMount("../searxng", "/etc/searxng", isReadOnly: true)
    .WithVolume("searxng-logs", "/var/log/uwsgi");

// === VECTOR DATABASES ===
// Add vector databases for embeddings
var (qdrant, milvus) = builder.AddVectorDatabases();

// === MONITORING ===
// Add monitoring stack
var (prometheus, grafana) = builder.AddMonitoringStack();

// === API GATEWAY ===
// Nginx reverse proxy
var nginx = builder.AddContainer("nginx", "nginx:alpine")
    .WithHttpEndpoint(port: 80, targetPort: 80)
    .WithHttpEndpoint(port: 443, targetPort: 443, name: "https")
    .WithBindMount("../nginx/nginx.conf", "/etc/nginx/nginx.conf", isReadOnly: true)
    .WaitFor(localAi)
    .WaitFor(searxng);

builder.Build().Run();
