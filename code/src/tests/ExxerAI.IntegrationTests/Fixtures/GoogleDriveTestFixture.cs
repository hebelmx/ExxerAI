using ExxerAi.MCPServer.Application.Interfaces;
using ExxerAi.MCPServer.Application.Services;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExxerAI.IntegrationTests.Fixtures;

/// <summary>
/// Shared test fixture for Google Drive integration tests.
/// Provides initialized services and test data for all MCP integration tests.
/// Implements proper resource management and cleanup for test isolation.
/// </summary>
public class GoogleDriveTestFixture : IAsyncLifetime
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;
    public IConfiguration Configuration { get; private set; } = null!;
    public IGoogleDriveService DriveService { get; private set; } = null!;
    public IPolymorphicDocumentProcessor DocumentProcessor { get; private set; } = null!;
    public IDocumentIngestionService IngestionService { get; private set; } = null!;

    // Test data properties
    public string? TestDocumentId { get; private set; }
    public string? TestFolderId { get; private set; }
    public string? LargeTestDocumentId { get; private set; }
    public bool IsConfigured { get; private set; }

    // Track resources for cleanup
    private readonly List<string> _createdWatchSessions = [];
    private readonly List<string> _processedDocuments = [];

    public async ValueTask InitializeAsync()
    {
        // Build configuration
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddEnvironmentVariables("EXXERAI_TEST_");

        Configuration = configBuilder.Build();

        // Validate Google Drive configuration
        var credentialsPath = Configuration["GoogleDrive:CredentialsPath"];
        TestDocumentId = Configuration["GoogleDrive:TestDocumentId"];
        TestFolderId = Configuration["GoogleDrive:TestFolderId"];
        LargeTestDocumentId = Configuration["GoogleDrive:LargeTestDocumentId"];

        IsConfigured = !string.IsNullOrEmpty(credentialsPath) && 
                      File.Exists(credentialsPath) &&
                      !string.IsNullOrEmpty(TestDocumentId) &&
                      !string.IsNullOrEmpty(TestFolderId);

        if (!IsConfigured)
        {
            // Don't fail completely - just mark as not configured for conditional tests
            return;
        }

        // Build service container
        var services = new ServiceCollection();
        services.AddSingleton(Configuration);
        services.AddLogging(builder => 
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Register MCP services
                    services.AddScoped<IGoogleDriveCredentialResolver, ModernGoogleDriveCredentialResolver>();
        services.AddScoped<IGoogleDriveService, GoogleDriveService>();
        services.AddScoped<IPolymorphicDocumentProcessor, ExxerAI.Infrastructure.DocumentProcessing.PolymorphicDocumentProcessor>();
        services.AddScoped<IDocumentIngestionService, ExxerAI.Application.Services.DocumentIngestionService>();

        ServiceProvider = services.BuildServiceProvider();

        // Get services
        DriveService = ServiceProvider.GetRequiredService<IGoogleDriveService>();
        DocumentProcessor = ServiceProvider.GetRequiredService<IPolymorphicDocumentProcessor>();
        IngestionService = ServiceProvider.GetRequiredService<IDocumentIngestionService>();

        // Initialize Google Drive service
        var initResult = await DriveService.InitializeAsync(CancellationToken.None);
        if (initResult.IsFailure)
        {
            throw new InvalidOperationException(
                $"Failed to initialize Google Drive service: {string.Join(", ", initResult.Errors)}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!IsConfigured || ServiceProvider == null)
            return;

        // Clean up watch sessions
        foreach (var watchId in _createdWatchSessions)
        {
            try
            {
                await DriveService.StopWatchingAsync(watchId, CancellationToken.None);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        (ServiceProvider as IDisposable)?.Dispose();
    }

    /// <summary>
    /// Registers a watch session for cleanup
    /// </summary>
    public void RegisterWatchSession(string watchId)
    {
        if (!string.IsNullOrEmpty(watchId))
        {
            _createdWatchSessions.Add(watchId);
        }
    }

    /// <summary>
    /// Registers a processed document for tracking
    /// </summary>
    public void RegisterProcessedDocument(string documentId)
    {
        if (!string.IsNullOrEmpty(documentId))
        {
            _processedDocuments.Add(documentId);
        }
    }

    /// <summary>
    /// Validates that the fixture is properly configured for testing
    /// </summary>
    public void EnsureConfigured()
    {
        if (!IsConfigured)
        {
            throw new SkipException("Google Drive integration tests are not configured. " +
                                   "Please set up credentials and test document/folder IDs in appsettings.test.json");
        }
    }
}

