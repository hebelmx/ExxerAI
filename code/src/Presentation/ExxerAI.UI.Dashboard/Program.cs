using ExxerAII.Aspire.Dashboard.Models;
using ExxerAII.Aspire.Dashboard.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Configure monitoring settings
builder.Services.Configure<MonitoringConfiguration>(
    builder.Configuration.GetSection(MonitoringConfiguration.SectionName));

// Add HTTP client for health checks
builder.Services.AddHttpClient<ServiceMonitoringService>();

// Add health check services
builder.Services.AddScoped<ServiceMonitoringService>();

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

app.MapRazorPages();
app.MapHub<DashboardHub>("/dashboardHub");

app.Run();

namespace ExxerAII.Aspire.Dashboard
{
    public partial class Program
    { }
}