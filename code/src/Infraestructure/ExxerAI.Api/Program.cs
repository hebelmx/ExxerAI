using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Infrastructure.Repositories;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Helpers;
using ExxerAI.Domain.Configurations;

namespace ExxerAI.Api;

/// <summary>
/// Main entry point for the ExxerAI Web API application
/// </summary>
/// <remarks>
/// Configures the web application with dependency injection, CORS policies, and middleware pipeline.
/// Sets up services for agent management, task processing, and repository implementations.
/// </remarks>
internal static class Program
{
    /// <summary>
    /// Main entry point for the API application
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllers();

        // Configure dependency injection
        builder.Services.AddScoped<IAgentService, AgentService>();

        // Register repositories - both specific interfaces and generic IRepository<T>
        builder.Services.AddScoped<IAgentRepository, InMemoryAgentRepository>();
        builder.Services.AddScoped<IRepository<Agent>, InMemoryAgentRepository>();
        builder.Services.AddScoped<ITaskRepository, InMemoryTaskRepository>();

        // Add CORS for development
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Development", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("Development");
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.MapControllers();

        app.Run();
    }
}
