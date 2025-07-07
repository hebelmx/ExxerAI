using ExxerAi.MCPServer.Components;
using ModelContextProtocol.Client;

namespace ExxerAi.MCPServer.Samples;

/// <summary>
/// Sample ASP.NET Core application that demonstrates how to set up a server-side
/// </summary>
public class Sample
{
    /// <summary>
    /// Returns a task that builds the ASP.NET Core application with the necessary services and configurations.
    /// </summary>
    /// <returns></returns>
    public static Task BuilderAsync()
    {
        var builder = WebApplication.CreateBuilder();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add configuration service
        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

        // Add logging service
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
        });

        builder.Services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILogger<Sample>>());

        // add MCP client
        builder.Services.AddSingleton<IMcpClient>(sp =>
        {
            McpClientOptions mcpClientOptions = new()
            { ClientInfo = new() { Name = "AspNetCoreSseClient", Version = "1.0.0" } };

            HttpClient httpClient = new()
            {
                BaseAddress = new("https://localhost:7133/sse")  //"https +http://aspnetsseserver" + "/sse")
            };

            McpServerConfig mcpServerConfig = new()
            {
                Id = "AspNetCoreSse",
                Name = "AspNetCoreSse",
                TransportType = TransportTypes.Sse,
                Location = httpClient.BaseAddress.ToString(),
            };

            var mcpClient = McpClientFactory.CreateAsync(mcpServerConfig, mcpClientOptions).GetAwaiter().GetResult();
            return mcpClient;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
        return Task.CompletedTask;
    }
}