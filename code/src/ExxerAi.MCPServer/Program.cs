using ExxerAi.MCPServer.Components;
using ExxerAi.MCPServer.Components.Account;
using ExxerAi.MCPServer.Data;
using ExxerAi.MCPServer.Application.Interfaces;
using ExxerAi.MCPServer.Application.Tools;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using ModelContextProtocol.AspNetCore;

namespace ExxerAi.MCPServer;

/// <summary>
/// Main program entry point for the ExxerAI MCP Server.
/// Configures MCP protocol integration with dependency injection and testable implementations.
/// </summary>
public class Program
{
	/// <summary>
	/// Main entry point for the ExxerAI MCP Server application.
	/// </summary>
	/// <param name="args">Command line arguments</param>
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Configure Serilog
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Information()
			.WriteTo.Console()
			.Enrich.FromLogContext()
			.CreateLogger();

		builder.Host.UseSerilog();

		// Add MudBlazor services
		builder.Services.AddMudServices();

		// Add services to the container.
		builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents();

		// Add Identity services
		builder.Services.AddCascadingAuthenticationState();
		builder.Services.AddScoped<IdentityUserAccessor>();
		builder.Services.AddScoped<IdentityRedirectManager>();
		builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

		builder.Services.AddAuthentication(options =>
			{
				options.DefaultScheme = IdentityConstants.ApplicationScheme;
				options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
			})
			.AddIdentityCookies();

		// Configure Entity Framework - TEMPORARY: Using in-memory database to bypass LocalDB issues
		// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
		//	?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
		
		builder.Services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase("TempMCPServerDb"));
		builder.Services.AddDatabaseDeveloperPageExceptionFilter();

		builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddSignInManager()
			.AddDefaultTokenProviders();

		builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

		// Register MCP Tool interfaces and implementations
		builder.Services.AddScoped<IGoogleDriveTools, GoogleDriveTools>();
		builder.Services.AddScoped<IDocumentProcessingTools, DocumentProcessingTools>();
		builder.Services.AddScoped<ISystemTools, SystemTools>();

		// Register MCP Server services with automatic tool discovery
		builder.Services.AddMcpServer()
			.WithHttpTransport()
			.WithToolsFromAssembly();

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseMigrationsEndPoint();
		}
		else
		{
			app.UseExceptionHandler("/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		app.UseHttpsRedirection();
		app.UseAntiforgery();

		app.MapStaticAssets();
		app.MapRazorComponents<App>()
			.AddInteractiveServerRenderMode();

		// Add additional endpoints required by the Identity /Account Razor components.
		app.MapAdditionalIdentityEndpoints();

		// Configure MCP endpoints
		app.MapMcp();

		Log.Information("🚀 ExxerAI MCP Server starting...");
		Log.Information("📡 MCP tools registered: GoogleDrive, DocumentProcessing, System");
		Log.Information("🏗️ MCP Server configured with dependency injection pattern");

		app.Run();
	}
}
