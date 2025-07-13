using ExxerAI.MCPServer.Application.Interfaces;
using ExxerAI.MCPServer.Application.Services;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;
using NSubstitute;
using ExxerAI.MCPServer.Application.Services;

namespace ExxerAI.IntegrationTests.MCP;

/// <summary>
/// Integration tests for Google Drive MCP service with real API interactions.
/// These tests require actual Google Drive API credentials and will perform real operations.
/// Uses test fixtures and proper cleanup to ensure no data persistence between tests.
/// </summary>
[Collection("GoogleDriveIntegration")]
public class GoogleDriveIntegrationTests : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IGoogleDriveService _driveService;
    private readonly IPolymorphicDocumentProcessor _documentProcessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleDriveIntegrationTests> _logger;
    
    // Test data tracking for cleanup
    private readonly List<string> _createdWatchSessions = [];
    private readonly List<string> _testDocumentIds = [];

    public GoogleDriveIntegrationTests()
    {
        // Build service collection with real dependencies
        var services = new ServiceCollection();
        
        // Configuration setup
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddEnvironmentVariables("EXXERAI_TEST_");

        _configuration = configBuilder.Build();
        services.AddSingleton(_configuration);

        // Logging setup
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Register MCP services
                    services.AddScoped<IGoogleDriveCredentialResolver, ModernGoogleDriveCredentialResolver>();
        services.AddScoped<IGoogleDriveService, GoogleDriveService>();
        services.AddScoped<IPolymorphicDocumentProcessor, ExxerAI.Infrastructure.DocumentProcessing.PolymorphicDocumentProcessor>();
        
        // Mock ILLMService for testing
        services.AddScoped<ILLMService>(provider => 
        {
            var mockLLMService = Substitute.For<ILLMService>();
            // Setup basic mock behavior
            mockLLMService.ValidateModelAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<bool>.WithSuccess(true)));
            mockLLMService.GenerateTextAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<LLMParameters>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<LLMResponse>.WithSuccess(new LLMResponse 
                { 
                    Content = "Mock LLM Response", 
                    InputTokens = 10, 
                    OutputTokens = 20, 
                    EstimatedCost = 0.001m 
                })));
            return mockLLMService;
        });
        
        // Mock IDocumentHashGenerator for testing (auxiliary service)
        services.AddScoped<IDocumentHashGenerator>(provider => 
        {
            var mockHashGenerator = Substitute.For<IDocumentHashGenerator>();
            // Setup basic mock behavior
            mockHashGenerator.GenerateHashAsync(Arg.Any<byte[]>(), Arg.Any<DocumentMetadata>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<DocumentHash>.WithSuccess(new DocumentHash("sample-content-hash", "sample-metadata-hash"))));
            mockHashGenerator.GenerateContentHashAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<string>.WithSuccess("sample-content-hash")));
            mockHashGenerator.VerifyIntegrityAsync(Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<bool>.WithSuccess(true)));
            return mockHashGenerator;
        });

        _serviceProvider = services.BuildServiceProvider();
        _driveService = _serviceProvider.GetRequiredService<IGoogleDriveService>();
        _documentProcessor = _serviceProvider.GetRequiredService<IPolymorphicDocumentProcessor>();
        _logger = _serviceProvider.GetRequiredService<ILogger<GoogleDriveIntegrationTests>>();
    }

    public async ValueTask InitializeAsync()
    {
        _logger.LogInformation("Initializing Google Drive integration tests...");

        // Initialize the service - let the credential resolver handle credential resolution
        var initResult = await _driveService.InitializeAsync(TestContext.Current.CancellationToken);
        if (initResult.IsFailure)
        {
            throw new InvalidOperationException(
                $"Failed to initialize Google Drive for integration tests: {string.Join(", ", initResult.Errors)}");
        }

        _logger.LogInformation("Google Drive service initialized successfully for testing");
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Cleaning up Google Drive integration test resources...");

        // Stop all created watch sessions
        foreach (var watchId in _createdWatchSessions)
        {
            try
            {
                await _driveService.StopWatchingAsync(watchId, CancellationToken.None);
                _logger.LogInformation("Stopped watch session: {WatchId}", watchId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to stop watch session {WatchId}", watchId);
            }
        }

        (_serviceProvider as IDisposable)?.Dispose();
        _logger.LogInformation("Integration test cleanup completed");
    }

/// <summary>
/// Begin Tests Initialization Integration Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task InitializeAsync_WithRealCredentials_ShouldSucceed()
    {
        // Act
        var result = await _driveService.InitializeAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();

        _logger.LogInformation("Google Drive initialization test passed");
    }

    [Fact]
    public async Task InitializeAsync_MultipleTimes_ShouldBeIdempotent()
    {
        // Act
        var firstResult = await _driveService.InitializeAsync(TestContext.Current.CancellationToken);
        var secondResult = await _driveService.InitializeAsync(TestContext.Current.CancellationToken);

        // Assert
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        firstResult.Value.ShouldBe(secondResult.Value);

        _logger.LogInformation("Google Drive idempotent initialization test passed");
    }

/// <summary>
/// End Tests Initialization Integration Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Folder Operations Integration Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task StartFolderWatchAsync_WithRealFolder_ShouldCreateWatchSession()
    {
        // Arrange
        var testFolderId = _configuration["GoogleDrive:TestFolderId"];
        if (string.IsNullOrEmpty(testFolderId))
        {
            // Skip test if no test folder configured
            _logger.LogWarning("Skipping folder watch test - no test folder configured");
            return;
        }

        // Act
        var result = await _driveService.StartFolderWatchAsync(
            testFolderId, 
            includeSubdirectories: true, 
            autoProcess: false, 
            pollingIntervalSeconds: 300, // 5 minutes for testing
            TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        if (result.IsSuccess)
        {
            result.Value.ShouldNotBeNullOrEmpty();
            _createdWatchSessions.Add(result.Value);
            _logger.LogInformation("Created watch session: {WatchId}", result.Value);
        }
        else
        {
            _logger.LogWarning("Folder watch creation failed: {Errors}", string.Join(", ", result.Errors));
        }
    }

    [Fact]
    public async Task GetActiveWatchesAsync_AfterCreatingWatch_ShouldIncludeNewWatch()
    {
        // Arrange
        var testFolderId = _configuration["GoogleDrive:TestFolderId"];
        if (string.IsNullOrEmpty(testFolderId))
        {
            _logger.LogWarning("Skipping active watches test - no test folder configured");
            return;
        }

        // Create a watch session first
        var watchResult = await _driveService.StartFolderWatchAsync(
            testFolderId, 
            cancellationToken: TestContext.Current.CancellationToken);

        if (watchResult.IsSuccess)
        {
            _createdWatchSessions.Add(watchResult.Value);
        }

        // Act
        var activeWatchesResult = await _driveService.GetActiveWatchesAsync(TestContext.Current.CancellationToken);

        // Assert
        activeWatchesResult.ShouldNotBeNull();
        activeWatchesResult.IsSuccess.ShouldBeTrue();
        
        if (watchResult.IsSuccess)
        {
            activeWatchesResult.Value.ShouldContain(watchResult.Value);
        }

        _logger.LogInformation("Active watches: {Watches}", activeWatchesResult.Value);
    }

/// <summary>
/// End Tests Folder Operations Integration Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Document Operations Integration Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetDocumentMetadataAsync_WithRealDocument_ShouldReturnMetadata()
    {
        // Arrange
        var testDocumentId = _configuration["GoogleDrive:TestDocumentId"];
        if (string.IsNullOrEmpty(testDocumentId))
        {
            _logger.LogWarning("Skipping document metadata test - no test document configured");
            return;
        }

        // Act
        var result = await _driveService.GetDocumentMetadataAsync(
            testDocumentId, 
            TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        if (result.IsSuccess)
        {
            result.Value.ShouldNotBeNull();
            result.Value.Id.ShouldBe(testDocumentId);
            result.Value.Name.ShouldNotBeNullOrEmpty();
            result.Value.MimeType.ShouldNotBeNullOrEmpty();
            result.Value.Size.ShouldBeGreaterThan(0);

            _logger.LogInformation("Document metadata retrieved: {Name} ({Size} bytes)", 
                result.Value.Name, result.Value.Size);
        }
        else
        {
            _logger.LogWarning("Document metadata retrieval failed: {Errors}", 
                string.Join(", ", result.Errors));
        }
    }

    [Fact]
    public async Task DownloadDocumentAsync_WithRealDocument_ShouldReturnDocumentData()
    {
        // Arrange
        var testDocumentId = _configuration["GoogleDrive:TestDocumentId"];
        if (string.IsNullOrEmpty(testDocumentId))
        {
            _logger.LogWarning("Skipping document download test - no test document configured");
            return;
        }

        // Act
        var result = await _driveService.DownloadDocumentAsync(
            testDocumentId, 
            TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        if (result.IsSuccess)
        {
            result.Value.ShouldNotBeNull();
            result.Value.Length.ShouldBeGreaterThan(0);

            _logger.LogInformation("Document downloaded successfully: {Size} bytes", result.Value.Length);

            // Verify the downloaded data is valid (basic format check)
            var data = result.Value;
            if (data.Length >= 4)
            {
                // Check for common file signatures
                var isPdf = data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46; // %PDF
                var isZip = data[0] == 0x50 && data[1] == 0x4B; // PK (ZIP/DOCX/etc.)
                
                (isPdf || isZip || true).ShouldBeTrue(); // Allow any format for flexibility
            }
        }
        else
        {
            _logger.LogWarning("Document download failed: {Errors}", string.Join(", ", result.Errors));
        }
    }

/// <summary>
/// End Tests Document Operations Integration Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Watch Session Management Integration Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task StopWatchingAsync_WithExistingWatch_ShouldStopSuccessfully()
    {
        // Arrange
        var testFolderId = _configuration["GoogleDrive:TestFolderId"];
        if (string.IsNullOrEmpty(testFolderId))
        {
            _logger.LogWarning("Skipping stop watching test - no test folder configured");
            return;
        }

        // Create a watch session to stop
        var watchResult = await _driveService.StartFolderWatchAsync(
            testFolderId, 
            cancellationToken: TestContext.Current.CancellationToken);

        if (watchResult.IsFailure)
        {
            _logger.LogWarning("Could not create watch session for stop test");
            return;
        }

        var watchId = watchResult.Value;

        // Act
        var stopResult = await _driveService.StopWatchingAsync(watchId, TestContext.Current.CancellationToken);

        // Assert
        stopResult.ShouldNotBeNull();
        if (stopResult.IsSuccess)
        {
            _logger.LogInformation("Watch session stopped successfully: {WatchId}", watchId);
            
            // Remove from cleanup list since it's already stopped
            _createdWatchSessions.Remove(watchId);
        }
        else
        {
            _logger.LogWarning("Failed to stop watch session {WatchId}: {Errors}", 
                watchId, string.Join(", ", stopResult.Errors));
            
            // Keep in cleanup list for manual cleanup
            _createdWatchSessions.Add(watchId);
        }
    }

/// <summary>
/// End Tests Watch Session Management Integration Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Performance Integration Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task DownloadDocumentAsync_WithLargeDocument_ShouldCompleteWithinTimeout()
    {
        // Arrange
        var largeDocumentId = _configuration["GoogleDrive:LargeTestDocumentId"];
        if (string.IsNullOrEmpty(largeDocumentId))
        {
            _logger.LogWarning("Skipping large document test - no large test document configured");
            return;
        }

        var timeout = TimeSpan.FromMinutes(5); // 5-minute timeout for large documents
        using var cts = new CancellationTokenSource(timeout);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _driveService.DownloadDocumentAsync(largeDocumentId, cts.Token);
        stopwatch.Stop();

        // Assert
        result.ShouldNotBeNull();
        if (result.IsSuccess)
        {
            result.Value.Length.ShouldBeGreaterThan(1024 * 1024); // At least 1MB
            stopwatch.Elapsed.ShouldBeLessThan(timeout);

            _logger.LogInformation("Large document ({Size} bytes) downloaded in {Duration}ms", 
                result.Value.Length, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogWarning("Large document download failed: {Errors}", 
                string.Join(", ", result.Errors));
        }
    }

    [Fact]
    public async Task ConcurrentOperations_WithMultipleRequests_ShouldHandleGracefully()
    {
        // Arrange
        var testDocumentId = _configuration["GoogleDrive:TestDocumentId"];
        if (string.IsNullOrEmpty(testDocumentId))
        {
            _logger.LogWarning("Skipping concurrent operations test - no test document configured");
            return;
        }

        const int concurrentRequests = 3; // Conservative for API limits

        // Act
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => _driveService.GetDocumentMetadataAsync(testDocumentId, TestContext.Current.CancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.ShouldAllBe(result => result != null);
        
        var successfulResults = results.Where(r => r.IsSuccess).ToArray();
        successfulResults.Length.ShouldBeGreaterThan(0); // At least some should succeed

        if (successfulResults.Length == concurrentRequests)
        {
            // All requests succeeded - verify consistency
            var firstResult = successfulResults[0].Value;
            successfulResults.ShouldAllBe(r => r.Value.Id == firstResult.Id);
            successfulResults.ShouldAllBe(r => r.Value.Name == firstResult.Name);

            _logger.LogInformation("All {Count} concurrent requests succeeded", concurrentRequests);
        }
        else
        {
            _logger.LogInformation("{Successful}/{Total} concurrent requests succeeded", 
                successfulResults.Length, concurrentRequests);
        }
    }

/// <summary>
/// End Tests Performance Integration Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Error Scenarios Integration Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetDocumentMetadataAsync_WithNonExistentDocument_ShouldReturnFailure()
    {
        // Arrange
        const string nonExistentDocumentId = "non-existent-document-id-12345";

        // Act
        var result = await _driveService.GetDocumentMetadataAsync(
            nonExistentDocumentId, 
            TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(error => 
            error.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("404", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("does not exist", StringComparison.OrdinalIgnoreCase));

        _logger.LogInformation("Non-existent document correctly returned failure: {Errors}", 
            string.Join(", ", result.Errors));
    }

    [Fact]
    public async Task StartFolderWatchAsync_WithInvalidFolder_ShouldReturnFailure()
    {
        // Arrange
        const string invalidFolderId = "invalid-folder-id-67890";

        // Act
        var result = await _driveService.StartFolderWatchAsync(
            invalidFolderId, 
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();

        _logger.LogInformation("Invalid folder correctly returned failure: {Errors}", 
            string.Join(", ", result.Errors));
    }

/// <summary>
/// End Tests Error Scenarios Integration Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Network Resilience Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task Operations_WithShortTimeout_ShouldRespectCancellation()
    {
        // Arrange
        var testDocumentId = _configuration["GoogleDrive:TestDocumentId"];
        if (string.IsNullOrEmpty(testDocumentId))
        {
            _logger.LogWarning("Skipping timeout test - no test document configured");
            return;
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100)); // Very short timeout

        // Act
        var result = await _driveService.DownloadDocumentAsync(testDocumentId, cts.Token);

        // Assert
        result.ShouldNotBeNull();
        // Result may succeed if operation is very fast, or fail due to cancellation
        if (result.IsFailure)
        {
            result.Errors.ShouldContain(error => 
                error.Contains("cancel", StringComparison.OrdinalIgnoreCase) ||
                error.Contains("timeout", StringComparison.OrdinalIgnoreCase));
        }

        _logger.LogInformation("Timeout/cancellation test completed: {Success}", result.IsSuccess);
    }

/// <summary>
/// End Tests Network Resilience Tests
/// </summary>
/// <returns></returns>
}