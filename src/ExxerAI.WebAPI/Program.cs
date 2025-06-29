using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using FluentValidation;
using Serilog;
using System.Text.Json;

// ===============================================================================
// ExxerAI.WebAPI - REST API Powerhouse 🚀
// ===============================================================================
// Purpose: High-performance API endpoints for AI agent orchestration
// Architecture: Minimal APIs with modern .NET 10 patterns
// Features: OpenAPI, real-time SignalR, structured logging, validation
// ===============================================================================

// Configure Serilog early for startup logging
Log.Logger = new LoggerConfiguration()
.WriteTo.Console()
.WriteTo.File("logs/startup-.log", rollingInterval: RollingInterval.Day)
.CreateBootstrapLogger();

try
{
    Log.Information("🚀 Starting ExxerAI WebAPI...");

    // Add the required using directive for SwaggerGen

    // Ensure the Swashbuckle.AspNetCore NuGet package is installed in your project.
    // You can install it using the following command in the terminal:
    // dotnet add package Swashbuckle.AspNetCore
    var builder = WebApplication.CreateBuilder(args);

    // ===== LOGGING =====
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    // ===== CORE SERVICES =====
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title = "ExxerAI API",
            Version = "v1",
            Description = "AI Agent Orchestration API"
        });
    });

    // ===== JSON CONFIGURATION =====
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.SerializerOptions.WriteIndented = true;
    });

    // ===== CORS =====
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("https://localhost:7001", "http://localhost:5000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // ===== REAL-TIME COMMUNICATION =====
    builder.Services.AddSignalR();

    // ===== VALIDATION =====
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // ===== EXXER AI SERVICES =====
    builder.Services.AddScoped<IAgent, GeneralPurposeAgent>();
    builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

    // ===== APPLICATION BUILD =====
    var app = builder.Build();

    // ===== MIDDLEWARE PIPELINE =====
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExxerAI API v1");
            c.RoutePrefix = string.Empty; // Swagger at root
        });
    }

    app.UseCors();
    app.UseRouting();

    // ===== API ENDPOINTS =====

    // Health Check
    app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow })
       .WithName("HealthCheck")
       .WithTags("System");

    // Agents
    var agents = app.MapGroup("/api/agents").WithTags("Agents");

    agents.MapPost("/execute", async (
        AgentContext context,
        IAgent agent,
        ILogger<Program> logger) =>
    {
        logger.LogInformation("🤖 Executing agent with context: {Context}", context);

        var result = await agent.ExecuteAsync(context, CancellationToken.None);

        logger.LogInformation("Agent execution result: {Result}", result);

        return result.IsSuccessful
            ? Results.Ok(result)
            : Results.BadRequest(result);
    })
    .WithName("ExecuteAgent")
    .WithSummary("Execute an AI agent with given context");

    agents.MapGet("/status", (ILogger<Program> logger) =>
    {
        logger.LogInformation("📊 Agent status requested");
        return Results.Ok(new { Status = "Ready", AgentsCount = 1 });
    })
    .WithName("GetAgentStatus")
    .WithSummary("Get current agent status");

    // Orchestration
    var orchestration = app.MapGroup("/api/orchestration").WithTags("Orchestration");

    orchestration.MapPost("/coordinate", async (
        AgentContext[] contexts,
        IAgentOrchestrator orchestrator,
        ILogger<Program> logger) =>
    {
        logger.LogInformation("🎭 Coordinating {Count} agents", contexts.Length);

        var results = new List<AgentResult>();

        foreach (var context in contexts)
        {
            // This would be implemented based on your orchestrator interface
            logger.LogInformation("Processing context for coordination...");
            var result = await orchestrator.ExecuteAsync(context, CancellationToken.None);
            results.Add(result);
        }

        return Results.Ok(results);
    })
    .WithName("CoordinateAgents")
    .WithSummary("Coordinate multiple AI agents");

    // SignalR Hub (placeholder for real-time features)
    // app.MapHub<AgentHub>("/hubs/agents");

    // ===== STARTUP =====
    Log.Information("🌐 ExxerAI WebAPI starting on {Urls}",
        string.Join(", ", app.Urls.DefaultIfEmpty("default URLs")));

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 WebAPI terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}