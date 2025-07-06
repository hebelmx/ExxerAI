using LocalAI.Aspire.Dashboard.Services;
using LocalAI.Aspire.AppHost.Configuration;
using LocalAI.Aspire.AppHost.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Register configuration
var configService = new ConfigurationService(builder.Configuration);
builder.Services.AddSingleton(configService);

// Add health check services
builder.Services.AddScoped<HealthCheckService>();
builder.Services.AddScoped<ServiceMonitoringService>();

// Configure health checks for all services
builder.Services.AddHealthChecks()
    // Database health checks
    .AddNpgSql(
        configService.GetDatabaseConnectionString(),
        name: "postgresql",
        tags: new[] { "database", "core" })
    
    // Redis health check
    .AddRedis(
        $"localhost:{configService.Configuration.Network.Redis.Port}",
        name: "redis",
        tags: new[] { "cache", "core" })
    
    // LocalAI API health check
    .AddUrlGroup(
        new Uri($"http://localhost:{configService.Configuration.LocalAI.ApiPort}/health"),
        name: "localai-api",
        tags: new[] { "ai", "core" })
    
    // Open WebUI health check
    .AddUrlGroup(
        new Uri($"http://localhost:{configService.Configuration.LocalAI.WebUIPort}"),
        name: "open-webui",
        tags: new[] { "ai", "ui" })
    
    // SearXNG health check
    .AddUrlGroup(
        new Uri($"http://localhost:{configService.Configuration.Search.Port}"),
        name: "searxng",
        tags: new[] { "search", "optional" })
    
    // Qdrant health check
    .AddUrlGroup(
        new Uri($"http://localhost:{configService.Configuration.VectorDatabases.Qdrant.Port}"),
        name: "qdrant",
        tags: new[] { "vector", "ai" })
    
    // Milvus health check
    .AddUrlGroup(
        new Uri($"http://localhost:{configService.Configuration.VectorDatabases.Milvus.WebPort}"),
        name: "milvus",
        tags: new[] { "vector", "ai" })
    
    // Prometheus health check
    .AddUrlGroup(
        new Uri($"http://localhost:{configService.Configuration.Monitoring.Prometheus.Port}/-/healthy"),
        name: "prometheus",
        tags: new[] { "monitoring", "optional" })
    
    // Grafana health check
    .AddUrlGroup(
        new Uri($"http://localhost:{configService.Configuration.Monitoring.Grafana.Port}/api/health"),
        name: "grafana",
        tags: new[] { "monitoring", "optional" });

// Add Health Checks UI
builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(30);
    options.MaximumHistoryEntriesPerEndpoint(60);
    options.SetApiMaxActiveRequests(1);
    options.AddHealthCheckEndpoint("LocalAI Stack", "/health");
})
.AddInMemoryStorage();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions()
{
    Predicate = check => check.Tags.Contains("core")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions()
{
    Predicate = _ => true
});

// Health Checks UI
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/dashboard";
    options.ApiPath = "/dashboard-api";
});

// Custom dashboard endpoints
app.MapGet("/", () => Results.Redirect("/dashboard"));
app.MapGet("/api/services", async (ServiceMonitoringService monitoring) => 
    await monitoring.GetServiceStatusAsync());
app.MapGet("/api/metrics", async (ServiceMonitoringService monitoring) => 
    await monitoring.GetSystemMetricsAsync());

app.MapRazorPages();
app.MapHub<DashboardHub>("/dashboardHub");

app.Run();

public partial class Program { }
