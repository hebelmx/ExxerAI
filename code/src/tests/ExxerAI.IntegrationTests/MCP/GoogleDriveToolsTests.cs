using ExxerAI.MCPServer.Application.Interfaces;
using ExxerAI.MCPServer.Application.Tools;
using ExxerAI.MCPServer.Application.Services;

namespace ExxerAI.IntegrationTests.MCP;

/// <summary>
/// Infrastructure tests for GoogleDriveTools MCP implementation.
/// Tests the MCP protocol tools layer that wraps the GoogleDriveService.
/// Validates tool registration, parameter handling, and result transformation.
/// </summary>
public class GoogleDriveToolsTests
{
    private readonly ILogger<GoogleDriveTools> _logger;
    private readonly IGoogleDriveService _driveService;
    private readonly GoogleDriveTools _tools;

    public GoogleDriveToolsTests()
    {
        _logger = XUnitLogger.CreateLogger<GoogleDriveTools>();
        _driveService = Substitute.For<IGoogleDriveService>();
        _tools = new GoogleDriveTools(_logger, _driveService);
    }

/// <summary>
/// Begin Tests Constructor Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        _logger.LogInformation("=== Test: Constructor_WithValidParameters_ShouldCreateInstance ===");
        
        // Assert
        _logger.LogInformation("Validating GoogleDriveTools instance creation");
        _tools.ShouldNotBeNull();
        _tools.ShouldBeOfType<GoogleDriveTools>();
        
        _logger.LogInformation("=== Test completed successfully ===");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger();
        logger.LogInformation("=== Test: Constructor_WithNullLogger_ShouldThrowArgumentNullException ===");
        
        // Act & Assert
        logger.LogInformation("Validating null logger throws ArgumentNullException");
        var exception = Should.Throw<ArgumentNullException>(() => 
            new GoogleDriveTools(null!, _driveService));
        exception.ParamName.ShouldBe("logger");
        
        logger.LogInformation("=== Test completed successfully ===");
    }

    [Fact]
    public void Constructor_WithNullDriveService_ShouldThrowArgumentNullException()
    {
        // Arrange
        var logger = XUnitLogger.CreateLogger();
        logger.LogInformation("=== Test: Constructor_WithNullDriveService_ShouldThrowArgumentNullException ===");
        
        // Act & Assert
        logger.LogInformation("Validating null drive service throws ArgumentNullException");
        var exception = Should.Throw<ArgumentNullException>(() => 
            new GoogleDriveTools(_logger, null!));
        exception.ParamName.ShouldBe("driveService");
        
        logger.LogInformation("=== Test completed successfully ===");
    }

/// <summary>
/// End Tests Constructor Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Tool Registration Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task StartFolderWatchAsync_WithValidParameters_ShouldReturnWatchSession()
    {
        // Arrange
        const string folderId = "test-folder-123";
        _driveService.StartFolderWatchAsync(folderId, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Result<string>.WithSuccess("Watch session started: watch-123"));

        // Act
        var result = await _tools.StartFolderWatchAsync(folderId, true, true, 60, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldContain("Watch session started");
        
        // Verify service was called
        await _driveService.Received(1).StartFolderWatchAsync(folderId, Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DownloadDocumentAsync_WithCancellation_ShouldRespectCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        const string documentId = "test-doc-123";

        // Act
        var result = await _tools.DownloadDocumentAsync(documentId, cts.Token);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("cancel", StringComparison.OrdinalIgnoreCase));
    }

/// <summary>
/// End Tests Tool Registration Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Initialize Drive Tool Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task CheckHealthStatusAsync_WithValidRequest_ShouldCallService()
    {
        // Arrange
        _driveService.InitializeAsync(Arg.Any<CancellationToken>())
            .Returns(Result<bool>.WithSuccess(true));

        // Act
        var result = await _tools.CheckHealthStatusAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await _driveService.Received(1).InitializeAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckHealthStatusAsync_WhenServiceFails_ShouldReturnFailure()
    {
        // Arrange
        _driveService.InitializeAsync(Arg.Any<CancellationToken>())
            .Returns(Result<bool>.WithFailure("Authentication failed"));

        // Act
        var result = await _tools.CheckHealthStatusAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Authentication failed");
    }

/// <summary>
/// End Tests Initialize Drive Tool Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Start Folder Watch Tool Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task StartFolderWatchAsync_WithSpecificParameters_ShouldCallService()
    {
        // Arrange
        const string folderId = "test-folder-123";
        const bool includeSubdirectories = true;
        const bool autoProcess = false;
        const int pollingInterval = 120;

        _driveService.StartFolderWatchAsync(
                folderId, 
                includeSubdirectories, 
                autoProcess, 
                pollingInterval, 
                Arg.Any<CancellationToken>())
            .Returns(Result<string>.WithSuccess("watch-session-123"));

        // Act
        var result = await _tools.StartFolderWatchAsync(folderId, includeSubdirectories, autoProcess, pollingInterval, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await _driveService.Received(1).StartFolderWatchAsync(
            folderId, 
            includeSubdirectories, 
            autoProcess, 
            pollingInterval, 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartFolderWatchAsync_WithNullFolderId_ShouldReturnFailure()
    {
        // Arrange
        const string? folderId = null;

        // Act
        var result = await _tools.StartFolderWatchAsync(folderId!, true, false, 60, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("folder", StringComparison.OrdinalIgnoreCase) || 
                                             error.Contains("null", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task StartFolderWatchAsync_WithDefaultParameters_ShouldUseDefaults()
    {
        // Arrange
        const string folderId = "test-folder-456";

        _driveService.StartFolderWatchAsync(
                folderId, 
                true,  // default include_subdirectories
                true,  // default auto_process
                60,    // default polling_interval_seconds
                Arg.Any<CancellationToken>())
            .Returns(Result<string>.WithSuccess("watch-session-456"));

        // Act (using default parameters)
        var result = await _tools.StartFolderWatchAsync(folderId, cancellationToken: CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await _driveService.Received(1).StartFolderWatchAsync(
            folderId, 
            true, 
            true, 
            60, 
            Arg.Any<CancellationToken>());
    }

/// <summary>
/// End Tests Start Folder Watch Tool Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Download Document Tool Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task DownloadDocumentAsync_WithValidDocumentId_ShouldCallService()
    {
        // Arrange
        const string documentId = "test-doc-789";
        var documentData = new byte[] { 1, 2, 3, 4, 5 };

        _driveService.DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result<byte[]>.WithSuccess(documentData));

        // Act
        var result = await _tools.DownloadDocumentAsync(documentId, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await _driveService.Received(1).DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DownloadDocumentAsync_WithNullDocumentId_ShouldReturnFailure()
    {
        // Arrange
        const string? documentId = null;

        // Act
        var result = await _tools.DownloadDocumentAsync(documentId!, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("document", StringComparison.OrdinalIgnoreCase) ||
                                             error.Contains("null", StringComparison.OrdinalIgnoreCase));
    }

/// <summary>
/// End Tests Download Document Tool Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Get Document Metadata Tool Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetDocumentMetadataAsync_WithValidDocumentId_ShouldCallService()
    {
        // Arrange
        const string documentId = "metadata-test-doc";
        var metadata = new GoogleDriveFileMetadata
        {
            Id = documentId,
            Name = "Test Document.pdf",
            MimeType = "application/pdf",
            Size = 1024,
            CreatedTime = DateTimeOffset.UtcNow.AddDays(-1).DateTime,
            ModifiedTime = DateTimeOffset.UtcNow.DateTime
        };

        _driveService.GetDocumentMetadataAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result<GoogleDriveFileMetadata>.WithSuccess(metadata));

        // Act
        var result = await _tools.GetDocumentMetadataAsync(documentId, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await _driveService.Received(1).GetDocumentMetadataAsync(documentId, Arg.Any<CancellationToken>());
    }

/// <summary>
/// End Tests Get Document Metadata Tool Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Watch Management Tool Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetActiveWatchesAsync_ShouldCallService()
    {
        // Arrange
        const string watchInfo = "Active watches: watch-123, watch-456";
        _driveService.GetActiveWatchesAsync(Arg.Any<CancellationToken>())
            .Returns(Result<string>.WithSuccess(watchInfo));

        // Act
        var result = await _tools.GetActiveWatchesAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await _driveService.Received(1).GetActiveWatchesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StopWatchingAsync_WithValidWatchId_ShouldCallService()
    {
        // Arrange
        const string watchId = "watch-to-stop-123";
        _driveService.StopWatchingAsync(watchId, Arg.Any<CancellationToken>())
            .Returns(Result<string>.WithSuccess("Watch stopped successfully"));

        // Act
        var result = await _tools.StopWatchingAsync(watchId, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await _driveService.Received(1).StopWatchingAsync(watchId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StopWatchingAsync_WithNullWatchId_ShouldReturnFailure()
    {
        // Arrange
        const string? watchId = null;

        // Act
        var result = await _tools.StopWatchingAsync(watchId!, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("watch", StringComparison.OrdinalIgnoreCase) ||
                                             error.Contains("null", StringComparison.OrdinalIgnoreCase));
    }

/// <summary>
/// End Tests Watch Management Tool Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Error Handling Tests
/// </summary>
/// <returns></returns>







/// <summary>
/// End Tests Error Handling Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Concurrent Execution Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetActiveWatchesAsync_WithConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        const int concurrentRequests = 5;
        _driveService.GetActiveWatchesAsync(Arg.Any<CancellationToken>())
            .Returns(Result<string>.WithSuccess("Mock watch list"));

        // Act
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => _tools.GetActiveWatchesAsync(CancellationToken.None))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.ShouldAllBe(result => result != null);
        results.ShouldAllBe(result => result.IsSuccess);
        await _driveService.Received(concurrentRequests).GetActiveWatchesAsync(Arg.Any<CancellationToken>());
    }

/// <summary>
/// End Tests Concurrent Execution Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Performance Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetActiveWatchesAsync_WithHighFrequencyRequests_ShouldMaintainPerformance()
    {
        // Arrange
        const int requestCount = 100;
        _driveService.GetActiveWatchesAsync(Arg.Any<CancellationToken>())
            .Returns(Result<string>.WithSuccess("Performance test response"));

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var tasks = new List<Task<Result<string>>>();
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(_tools.GetActiveWatchesAsync(CancellationToken.None));
        }

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        results.Length.ShouldBe(requestCount);
        results.ShouldAllBe(result => result.IsSuccess);
        
        // Performance assertion - should complete reasonably quickly
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(10000); // 10 seconds max for 100 requests
    }

/// <summary>
/// End Tests Performance Tests
/// </summary>
/// <returns></returns>
}