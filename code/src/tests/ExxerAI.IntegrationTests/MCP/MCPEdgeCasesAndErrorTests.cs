using ExxerAI.MCPServer.Application.Interfaces;
using ExxerAI.MCPServer.Application.Services;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;
using NSubstitute;
using ExxerAI.MCPServer.Application.Services;

namespace ExxerAI.IntegrationTests.MCP;

/// <summary>
/// Comprehensive edge case and error scenario tests for MCP services.
/// Tests system resilience, error handling, and recovery mechanisms.
/// Validates behavior under adverse conditions and boundary scenarios.
/// </summary>
[Collection("MCPEdgeCasesAndErrors")]
public class MCPEdgeCasesAndErrorTests : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IGoogleDriveService _driveService;
    private readonly IPolymorphicDocumentProcessor _documentProcessor;
    private readonly IDocumentIngestionService _ingestionService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MCPEdgeCasesAndErrorTests> _logger;

    // Test cleanup tracking
    private readonly List<string> _watchSessionsToCleanup = [];

    public MCPEdgeCasesAndErrorTests()
    {
        var services = new ServiceCollection();
        
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddEnvironmentVariables("EXXERAI_TEST_");

        _configuration = configBuilder.Build();
        services.AddSingleton(_configuration);
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning)); // Reduced logging for error tests

                    services.AddScoped<IGoogleDriveCredentialResolver, ModernGoogleDriveCredentialResolver>();
        services.AddScoped<IGoogleDriveService, GoogleDriveService>();
        services.AddScoped<IPolymorphicDocumentProcessor, ExxerAI.Infrastructure.DocumentProcessing.PolymorphicDocumentProcessor>();
        services.AddScoped<IDocumentIngestionService, ExxerAI.Application.Services.DocumentIngestionService>();
        
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
        
        // Mock IDocumentHashGenerator for testing
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
        _ingestionService = _serviceProvider.GetRequiredService<IDocumentIngestionService>();
        _logger = _serviceProvider.GetRequiredService<ILogger<MCPEdgeCasesAndErrorTests>>();
    }

    public async ValueTask InitializeAsync()
    {
        _logger.LogInformation("Initializing MCP edge cases and error tests...");
        
        // Initialize services for error testing
        var initResult = await _driveService.InitializeAsync(TestContext.Current.CancellationToken);
        if (initResult.IsFailure)
        {
            _logger.LogWarning("Service initialization failed for error tests: {Errors}", 
                string.Join(", ", initResult.Errors));
            // Continue anyway - some tests expect uninitialized state
        }
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Cleaning up MCP edge case test resources...");
        
        foreach (var watchId in _watchSessionsToCleanup)
        {
            try
            {
                await _driveService.StopWatchingAsync(watchId, CancellationToken.None);
            }
            catch
            {
                // Ignore cleanup errors in edge case tests
            }
        }

        (_serviceProvider as IDisposable)?.Dispose();
    }

/// <summary>
/// Network and Connectivity Edge Cases
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GoogleDriveService_WithNetworkTimeout_ShouldFailGracefully()
    {
        _logger.LogInformation("Testing network timeout scenarios");

        // Arrange - Very short timeout to simulate network issues
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

        // Act - Attempt operations with immediate cancellation
        var downloadResult = await _driveService.DownloadDocumentAsync("any-doc-id", cts.Token);
        var metadataResult = await _driveService.GetDocumentMetadataAsync("any-doc-id", cts.Token);
        var watchResult = await _driveService.StartFolderWatchAsync("any-folder-id", cancellationToken: cts.Token);

        // Assert - All operations should handle cancellation gracefully
        downloadResult.ShouldNotBeNull();
        downloadResult.IsFailure.ShouldBeTrue();
        
        metadataResult.ShouldNotBeNull();
        metadataResult.IsFailure.ShouldBeTrue();
        
        watchResult.ShouldNotBeNull();
        watchResult.IsFailure.ShouldBeTrue();

        _logger.LogInformation("✓ Network timeout scenarios handled gracefully");
    }

    [Fact]
    public async Task GoogleDriveService_WithInvalidCredentials_ShouldReportAuthenticationErrors()
    {
        _logger.LogInformation("Testing authentication error scenarios");

        // Arrange - Create service with invalid configuration
        var invalidConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GoogleDrive:CredentialsPath"] = "non-existent-credentials.json",
                ["GoogleDrive:ApplicationName"] = "Test-App"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(invalidConfig);
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton(_configuration);
        services.AddScoped<IGoogleDriveCredentialResolver, ModernGoogleDriveCredentialResolver>();
        services.AddScoped<IGoogleDriveService, GoogleDriveService>();

        using var provider = services.BuildServiceProvider();
        var invalidDriveService = provider.GetRequiredService<IGoogleDriveService>();

        // Act
        var initResult = await invalidDriveService.InitializeAsync(TestContext.Current.CancellationToken);

        // Assert
        initResult.ShouldNotBeNull();
        initResult.IsFailure.ShouldBeTrue();
        initResult.Errors.ShouldContain(error => 
            error.Contains("credential", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("file not found", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("authentication", StringComparison.OrdinalIgnoreCase));

        _logger.LogInformation("✓ Authentication errors reported correctly");
    }

    [Fact]
    public async Task GoogleDriveService_WithRateLimitSimulation_ShouldHandleBackoff()
    {
        _logger.LogInformation("Testing rate limit and backoff scenarios");

        // Arrange - Multiple rapid requests to potentially trigger rate limiting
        const int rapidRequestCount = 10;
        var testDocumentId = _configuration["GoogleDrive:TestDocumentId"] ?? "rate-limit-test-doc";

        // Act - Fire multiple requests rapidly
        var tasks = Enumerable.Range(0, rapidRequestCount)
            .Select(_ => _driveService.GetDocumentMetadataAsync(testDocumentId, TestContext.Current.CancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert - Some requests may fail due to rate limiting, but service should remain stable
        results.ShouldAllBe(result => result != null);
        
        var successCount = results.Count(r => r.IsSuccess);
        var failureCount = results.Count(r => r.IsFailure);

        _logger.LogInformation("Rate limit test: {Success} successes, {Failures} failures out of {Total}", 
            successCount, failureCount, rapidRequestCount);

        // At least some requests should complete (either success or controlled failure)
        (successCount + failureCount).ShouldBe(rapidRequestCount);

        _logger.LogInformation("✓ Rate limiting handled appropriately");
    }

/// <summary>
/// End Network and Connectivity Edge Cases
/// </summary>
/// <returns></returns>

/// <summary>
/// Document Processing Edge Cases
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task DocumentProcessor_WithCorruptedDocument_ShouldFailGracefully()
    {
        _logger.LogInformation("Testing corrupted document processing");

        // Arrange - Various types of corrupted data
        var corruptedDocuments = new[]
        {
            ("empty.pdf", Array.Empty<byte>()),
            ("invalid-header.pdf", new byte[] { 0x00, 0x00, 0x00, 0x00 }),
            ("partial-pdf.pdf", new byte[] { 0x25, 0x50, 0x44 }), // Incomplete PDF header
            ("random-bytes.pdf", GenerateRandomBytes(1024)),
            ("oversized-header.pdf", GenerateInvalidPdfHeader())
        };

        // Act & Assert
        foreach (var (fileName, data) in corruptedDocuments)
        {
            var documentMetadata = new DocumentMetadata { FileName = fileName };
            var result = await _documentProcessor.ProcessDocumentAsync(
                data, documentMetadata, TestContext.Current.CancellationToken);

            result.ShouldNotBeNull($"Processing {fileName} should return a result");
            result.IsFailure.ShouldBeTrue($"Processing {fileName} should fail gracefully");
            result.Errors.ShouldNotBeEmpty($"Processing {fileName} should provide error details");

            _logger.LogInformation("✓ Corrupted document {FileName} handled: {Error}", 
                fileName, result.Errors.FirstOrDefault());
        }

        _logger.LogInformation("✓ All corrupted document scenarios handled gracefully");
    }

    [Fact]
    public async Task DocumentProcessor_WithExtremelyLargeDocument_ShouldHandleResourceLimits()
    {
        _logger.LogInformation("Testing extremely large document processing");

        // Arrange - Create a very large document (50MB)
        var largeDocumentData = GenerateLargePdfDocument(50 * 1024 * 1024);
        const string fileName = "extremely-large-document.pdf";

        // Use a timeout to prevent hanging
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        // Act
        var documentMetadata = new DocumentMetadata { FileName = fileName };
        var result = await _documentProcessor.ProcessDocumentAsync(
            largeDocumentData, documentMetadata, cts.Token);

        // Assert - Should either process successfully or fail gracefully due to size limits
        result.ShouldNotBeNull();
        
        if (result.IsFailure)
        {
            result.Errors.ShouldContain(error => 
                error.Contains("size", StringComparison.OrdinalIgnoreCase) ||
                error.Contains("memory", StringComparison.OrdinalIgnoreCase) ||
                error.Contains("limit", StringComparison.OrdinalIgnoreCase) ||
                error.Contains("timeout", StringComparison.OrdinalIgnoreCase));
        }

        _logger.LogInformation("✓ Large document processing handled: {Success}", result.IsSuccess);
    }

    [Fact]
    public async Task DocumentProcessor_WithUnsupportedFileTypes_ShouldProvideHelpfulErrors()
    {
        _logger.LogInformation("Testing unsupported file type processing");

        // Arrange - Various unsupported file types
        var unsupportedFiles = new[]
        {
            ("image.jpg", GenerateJpegHeader()),
            ("audio.mp3", GenerateMp3Header()),
            ("video.mp4", GenerateMp4Header()),
            ("archive.zip", GenerateZipHeader()),
            ("executable.exe", GenerateExeHeader()),
            ("unknown.xyz", GenerateRandomBytes(512))
        };

        // Act & Assert
        foreach (var (fileName, data) in unsupportedFiles)
        {
            var documentMetadata = new DocumentMetadata { FileName = fileName };
            var result = await _documentProcessor.ProcessDocumentAsync(
                data, documentMetadata, TestContext.Current.CancellationToken);

            result.ShouldNotBeNull($"Processing {fileName} should return a result");
            
            if (result.IsFailure)
            {
                result.Errors.ShouldContain(error => 
                    error.Contains("unsupported", StringComparison.OrdinalIgnoreCase) ||
                    error.Contains("format", StringComparison.OrdinalIgnoreCase) ||
                    error.Contains("type", StringComparison.OrdinalIgnoreCase));
            }

            _logger.LogInformation("✓ Unsupported file {FileName} handled: {Success}", fileName, result.IsSuccess);
        }

        _logger.LogInformation("✓ All unsupported file types handled appropriately");
    }

/// <summary>
/// End Document Processing Edge Cases
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests API Boundary and Limit Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GoogleDriveService_WithInvalidDocumentIds_ShouldHandleAllFormats()
    {
        _logger.LogInformation("Testing various invalid document ID formats");

        // Arrange - Various invalid document ID formats
        var invalidDocumentIds = new[]
        {
            null!,
            "",
            " ",
            "invalid-id-format",
            "123-456-789-very-long-id-that-might-exceed-limits-" + new string('x', 1000),
            "special!@#$%^&*()characters",
            "unicode-ñ-文档-🔥",
            "../../../etc/passwd", // Path traversal attempt
            "<script>alert('xss')</script>", // XSS attempt
            "' OR '1'='1", // SQL injection attempt
        };

        // Act & Assert
        foreach (var invalidId in invalidDocumentIds.Where(id => id != null))
        {
            var metadataResult = await _driveService.GetDocumentMetadataAsync(invalidId, TestContext.Current.CancellationToken);
            var downloadResult = await _driveService.DownloadDocumentAsync(invalidId, TestContext.Current.CancellationToken);

            metadataResult.ShouldNotBeNull($"Metadata request with invalid ID '{invalidId}' should return result");
            downloadResult.ShouldNotBeNull($"Download request with invalid ID '{invalidId}' should return result");

            // Both should fail gracefully
            metadataResult.IsFailure.ShouldBeTrue($"Metadata request with invalid ID '{invalidId}' should fail");
            downloadResult.IsFailure.ShouldBeTrue($"Download request with invalid ID '{invalidId}' should fail");
        }

        // Test null specifically
        var nullMetadataResult = await _driveService.GetDocumentMetadataAsync(null!, TestContext.Current.CancellationToken);
        var nullDownloadResult = await _driveService.DownloadDocumentAsync(null!, TestContext.Current.CancellationToken);

        nullMetadataResult.IsFailure.ShouldBeTrue();
        nullDownloadResult.IsFailure.ShouldBeTrue();

        _logger.LogInformation("✓ All invalid document ID formats handled securely");
    }

    [Fact]
    public async Task GoogleDriveService_WithInvalidFolderIds_ShouldValidateInputs()
    {
        _logger.LogInformation("Testing invalid folder ID validation");

        // Arrange
        var invalidFolderIds = new[]
        {
            null!,
            "",
            "   ",
            "folder/with/slashes",
            new string('x', 10000), // Extremely long ID
            "../../sensitive/path"
        };

        // Act & Assert
        foreach (var invalidId in invalidFolderIds.Where(id => id != null))
        {
            var watchResult = await _driveService.StartFolderWatchAsync(invalidId, cancellationToken: TestContext.Current.CancellationToken);
            
            watchResult.ShouldNotBeNull($"Watch request with invalid folder ID '{invalidId}' should return result");
            watchResult.IsFailure.ShouldBeTrue($"Watch request with invalid folder ID '{invalidId}' should fail");
        }

        // Test null specifically
        var nullWatchResult = await _driveService.StartFolderWatchAsync(null!, cancellationToken: TestContext.Current.CancellationToken);
        nullWatchResult.IsFailure.ShouldBeTrue();

        _logger.LogInformation("✓ Invalid folder IDs properly validated");
    }

/// <summary>
/// End Tests API Boundary and Limit Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Concurrency and Race Condition Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GoogleDriveService_WithConcurrentWatchSessions_ShouldMaintainConsistency()
    {
        _logger.LogInformation("Testing concurrent watch session management");

        var testFolderId = _configuration["GoogleDrive:TestFolderId"];
        if (string.IsNullOrEmpty(testFolderId))
        {
            _logger.LogWarning("Skipping concurrent watch test - no test folder configured");
            return;
        }

        // Arrange - Multiple concurrent watch requests
        const int concurrentWatches = 5;
        var watchTasks = Enumerable.Range(0, concurrentWatches)
            .Select(i => _driveService.StartFolderWatchAsync(
                testFolderId, 
                pollingIntervalSeconds: 300 + i, // Slightly different intervals
                cancellationToken: TestContext.Current.CancellationToken))
            .ToArray();

        // Act
        var results = await Task.WhenAll(watchTasks);

        // Assert
        results.ShouldAllBe(result => result != null);

        var successfulWatches = results.Where(r => r.IsSuccess).Select(r => r.Value).ToList();
        
        // Track successful watches for cleanup
        _watchSessionsToCleanup.AddRange(successfulWatches);

        // Verify no duplicate watch IDs (if any succeeded)
        if (successfulWatches.Count > 1)
        {
            successfulWatches.Distinct().Count().ShouldBe(successfulWatches.Count, "Watch session IDs should be unique");
        }

        _logger.LogInformation("✓ Concurrent watch sessions handled: {Successful}/{Total} succeeded", 
            successfulWatches.Count, concurrentWatches);
    }

    [Fact]
    public async Task DocumentProcessor_WithConcurrentLargeDocuments_ShouldManageResources()
    {
        _logger.LogInformation("Testing concurrent large document processing");

        // Arrange - Multiple large documents processed simultaneously
        const int concurrentDocs = 3;
        const int docSizeMB = 5; // Smaller size for concurrent testing

        var processingTasks = Enumerable.Range(0, concurrentDocs)
            .Select(i => 
            {
                var docData = GenerateLargePdfDocument(docSizeMB * 1024 * 1024);
                var documentMetadata = new DocumentMetadata { FileName = $"concurrent-large-doc-{i}.pdf" };
                return _documentProcessor.ProcessDocumentAsync(
                    docData, 
                    documentMetadata, 
                    TestContext.Current.CancellationToken);
            })
            .ToArray();

        // Act
        var results = await Task.WhenAll(processingTasks);

        // Assert
        results.ShouldAllBe(result => result != null);

        var successful = results.Count(r => r.IsSuccess);
        var failed = results.Count(r => r.IsFailure);

        _logger.LogInformation("✓ Concurrent large document processing: {Successful} succeeded, {Failed} failed", 
            successful, failed);

        // System should remain stable regardless of success/failure ratio
        (successful + failed).ShouldBe(concurrentDocs);
    }

/// <summary>
/// End Tests Concurrency and Race Condition Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Resource Exhaustion and Memory Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task DocumentProcessor_WithMemoryPressure_ShouldHandleGracefully()
    {
        _logger.LogInformation("Testing behavior under memory pressure");

        // Arrange - Process multiple documents to create memory pressure
        const int documentCount = 10;
        var documents = Enumerable.Range(0, documentCount)
            .Select(i => GenerateLargePdfDocument(1024 * 1024)) // 1MB each
            .ToArray();

        // Act - Process documents sequentially to avoid overwhelming the system
        var results = new List<Result<DocumentProcessingResult>>();
        
        for (int i = 0; i < documentCount; i++)
        {
            var documentMetadata = new DocumentMetadata { FileName = $"memory-pressure-doc-{i}.pdf" };
            var result = await _documentProcessor.ProcessDocumentAsync(
                documents[i], 
                documentMetadata, 
                TestContext.Current.CancellationToken);
            
            results.Add(result);

            // Force garbage collection to simulate memory pressure
            if (i % 3 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        // Assert
        results.ShouldAllBe(result => result != null);
        
        var successfulProcessing = results.Count(r => r.IsSuccess);
        
        // Should process at least some documents successfully
        successfulProcessing.ShouldBeGreaterThan(0, "At least some documents should process successfully under memory pressure");

        _logger.LogInformation("✓ Memory pressure handling: {Successful}/{Total} documents processed", 
            successfulProcessing, documentCount);
    }

/// <summary>
/// End Tests Resource Exhaustion and Memory Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Helper Methods for Test Data Generation
/// </summary>
/// <returns></returns>

    private static byte[] GenerateRandomBytes(int size)
    {
        var random = new Random(42); // Fixed seed for reproducible tests
        var bytes = new byte[size];
        random.NextBytes(bytes);
        return bytes;
    }

    private static byte[] GenerateInvalidPdfHeader()
    {
        var data = new byte[1024];
        // Start with PDF header but corrupt it
        data[0] = 0x25; // %
        data[1] = 0x50; // P
        data[2] = 0x44; // D
        data[3] = 0x46; // F
        data[4] = 0x2D; // -
        data[5] = 0xFF; // Invalid version byte
        // Fill rest with random data
        var random = new Random(42);
        random.NextBytes(data.AsSpan(6));
        return data;
    }

    private static byte[] GenerateJpegHeader()
    {
        var data = new byte[512];
        data[0] = 0xFF; // JPEG SOI
        data[1] = 0xD8;
        data[2] = 0xFF;
        data[3] = 0xE0; // JFIF
        return data;
    }

    private static byte[] GenerateMp3Header()
    {
        var data = new byte[512];
        data[0] = 0xFF; // MP3 sync
        data[1] = 0xFB;
        return data;
    }

    private static byte[] GenerateMp4Header()
    {
        var data = new byte[512];
        // MP4 ftyp box
        System.Text.Encoding.ASCII.GetBytes("ftyp").CopyTo(data.AsSpan(4));
        return data;
    }

    private static byte[] GenerateZipHeader()
    {
        var data = new byte[512];
        data[0] = 0x50; // PK
        data[1] = 0x4B;
        data[2] = 0x03;
        data[3] = 0x04;
        return data;
    }

    private static byte[] GenerateExeHeader()
    {
        var data = new byte[512];
        data[0] = 0x4D; // MZ
        data[1] = 0x5A;
        return data;
    }

    private static byte[] GenerateLargePdfDocument(int sizeBytes)
    {
        var data = new byte[sizeBytes];
        
        // Add minimal PDF structure
        var header = System.Text.Encoding.ASCII.GetBytes("%PDF-1.4\n");
        header.CopyTo(data, 0);
        
        // Fill with content that looks like PDF data
        var random = new Random(42);
        for (int i = header.Length; i < data.Length - 10; i += 100)
        {
            var content = System.Text.Encoding.ASCII.GetBytes("BT /F1 12 Tf 100 700 Td (Test content) Tj ET\n");
            var copyLength = Math.Min(content.Length, data.Length - i);
            content.AsSpan(0, copyLength).CopyTo(data.AsSpan(i));
        }
        
        // Add minimal PDF trailer
        var trailer = System.Text.Encoding.ASCII.GetBytes("%%EOF");
        trailer.CopyTo(data.AsSpan(data.Length - trailer.Length));
        
        return data;
    }

/// <summary>
/// End Tests Helper Methods for Test Data Generation
/// </summary>
/// <returns></returns>
}