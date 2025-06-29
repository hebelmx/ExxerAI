using ExxerAI.Application.Interfaces;
using ExxerAI.Infrastructure.Services;
using MudBlazor.Services;
using Serilog;
using FluentValidation;

// ===============================================================================
// ExxerAI.BlazorUI - MudBlazor on Rockets! 🚀
// ===============================================================================
// Purpose: Professional AI agent management interface with Material Design 3
// Architecture: Blazor Server with real-time components
// Features: Beautiful MudBlazor UI, real-time agent interactions, responsive design
// ===============================================================================

// Configure Serilog early for startup logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/blazorui-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("🎨 Starting ExxerAI BlazorUI with MudBlazor...");

    var builder = WebApplication.CreateBuilder(args);

    // ===== LOGGING =====
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    // ===== BLAZOR SERVICES =====
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
        options.DisconnectedCircuitMaxRetained = 100;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(60);
        options.MaxBufferedUnacknowledgedRenderBatches = 10;
    });

    // ===== MUDBLAZOR UI FRAMEWORK =====
    builder.Services.AddMudServices(config =>
    {
        config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomLeft;
        config.SnackbarConfiguration.PreventDuplicates = false;
        config.SnackbarConfiguration.NewestOnTop = false;
        config.SnackbarConfiguration.ShowCloseIcon = true;
        config.SnackbarConfiguration.VisibleStateDuration = 10000;
        config.SnackbarConfiguration.HideTransitionDuration = 500;
        config.SnackbarConfiguration.ShowTransitionDuration = 500;
        config.SnackbarConfiguration.SnackbarVariant = MudBlazor.Variant.Filled;
    });

    // ===== REAL-TIME COMMUNICATION =====
    builder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    });

    // ===== VALIDATION =====
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // ===== HTTP CLIENTS =====
    builder.Services.AddHttpClient("ExxerAI.WebAPI", client =>
    {
        client.BaseAddress = new Uri("https://localhost:7002/"); // Adjust as needed
        client.DefaultRequestHeaders.Add("User-Agent", "ExxerAI-BlazorUI/1.0");
    });

    // ===== EXXER AI SERVICES =====
    builder.Services.AddScoped<IAgent, GeneralPurposeAgent>();
    builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

    // ===== APPLICATION BUILD =====
    var app = builder.Build();

    // ===== MIDDLEWARE PIPELINE =====
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseSerilogRequestLogging();
    app.UseRouting();

    // ===== ENDPOINT MAPPING =====
    app.MapRazorPages();
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    // SignalR Hub for real-time agent communication
    // app.MapHub<AgentCommunicationHub>("/hubs/agents");

    // ===== STARTUP =====
    Log.Information("🌐 ExxerAI BlazorUI starting on {Urls}", 
        string.Join(", ", app.Urls.DefaultIfEmpty("default URLs")));

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 BlazorUI terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
} 