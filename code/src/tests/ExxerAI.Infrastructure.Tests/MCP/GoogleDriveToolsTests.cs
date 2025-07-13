using ExxerAi.MCPServer.Application.Interfaces;
using ExxerAi.MCPServer.Application.Tools;
using ExxerAI.Domain.Operations;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Infrastructure.Tests.MCP;

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
        _logger = Substitute.For<ILogger<GoogleDriveTools>>();
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
        // Act & Assert
        _tools.ShouldNotBeNull();
        _tools.ShouldBeOfType<GoogleDriveTools>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            new GoogleDriveTools(null!, _driveService))
            .ParamName.ShouldBe("logger");
    }

    [Fact]
    public void Constructor_WithNullDriveService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            new GoogleDriveTools(_logger, null!))
            .ParamName.ShouldBe("driveService");
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
    public async Task GetAvailableToolsAsync_ShouldReturnExpectedTools()
    {
        // Act
        var result = await _tools.GetAvailableToolsAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldNotBeEmpty();

        // Verify essential tools are present
        var toolNames = result.Value.Select(t => t.Name).ToList();
        toolNames.ShouldContain("initialize_drive");
        toolNames.ShouldContain("start_folder_watch");
        toolNames.ShouldContain("download_document");
        toolNames.ShouldContain("get_document_metadata");
        toolNames.ShouldContain("get_active_watches");
        toolNames.ShouldContain("stop_watching");
    }

    [Fact]
    public async Task GetAvailableToolsAsync_WithCancellation_ShouldRespectCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _tools.GetAvailableToolsAsync(cts.Token);

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
    public async Task ExecuteToolAsync_InitializeDrive_WithValidRequest_ShouldCallService()
    {
        // Arrange
        _driveService.InitializeAsync(Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(true));

        var request = new
        {
            tool = "initialize_drive",
            parameters = new { }
        };

        // Act
        var result = await _tools.ExecuteToolAsync("initialize_drive", request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await _driveService.Received(1).InitializeAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteToolAsync_InitializeDrive_WhenServiceFails_ShouldReturnFailure()
    {
        // Arrange
        _driveService.InitializeAsync(Arg.Any<CancellationToken>())
            .Returns(Result<bool>.WithFailure("Authentication failed"));

        var request = new
        {
            tool = "initialize_drive",
            parameters = new { }
        };

        // Act
        var result = await _tools.ExecuteToolAsync("initialize_drive", request, CancellationToken.None);

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
    public async Task ExecuteToolAsync_StartFolderWatch_WithValidParameters_ShouldCallService()
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
            .Returns(Result<string>.Success("watch-session-123"));

        var request = new
        {
            tool = "start_folder_watch",
            parameters = new
            {
                folder_id = folderId,
                include_subdirectories = includeSubdirectories,
                auto_process = autoProcess,
                polling_interval_seconds = pollingInterval
            }
        };

        // Act
        var result = await _tools.ExecuteToolAsync("start_folder_watch", request, CancellationToken.None);

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
    public async Task ExecuteToolAsync_StartFolderWatch_WithMissingFolderId_ShouldReturnFailure()
    {
        // Arrange
        var request = new
        {
            tool = "start_folder_watch",
            parameters = new
            {
                include_subdirectories = true,
                auto_process = false
            }
        };

        // Act
        var result = await _tools.ExecuteToolAsync("start_folder_watch", request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("folder_id", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteToolAsync_StartFolderWatch_WithDefaultParameters_ShouldUseDefaults()
    {
        // Arrange
        const string folderId = "test-folder-456";

        _driveService.StartFolderWatchAsync(
                folderId, 
                true,  // default include_subdirectories
                true,  // default auto_process
                60,    // default polling_interval_seconds
                Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success("watch-session-456"));

        var request = new
        {
            tool = "start_folder_watch",
            parameters = new
            {
                folder_id = folderId
            }
        };

        // Act
        var result = await _tools.ExecuteToolAsync("start_folder_watch", request, CancellationToken.None);

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
    public async Task ExecuteToolAsync_DownloadDocument_WithValidDocumentId_ShouldCallService()
    {
        // Arrange
        const string documentId = "test-doc-789";
        var documentData = new byte[] { 1, 2, 3, 4, 5 };

        _driveService.DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result<byte[]>.Success(documentData));

        var request = new
        {
            tool = "download_document",
            parameters = new
            {
                document_id = documentId
            }
        };

        // Act
        var result = await _tools.ExecuteToolAsync("download_document", request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await _driveService.Received(1).DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteToolAsync_DownloadDocument_WithMissingDocumentId_ShouldReturnFailure()
    {
        // Arrange
        var request = new
        {
            tool = "download_document",
            parameters = new { }
        };

        // Act
        var result = await _tools.ExecuteToolAsync("download_document", request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("document_id", StringComparison.OrdinalIgnoreCase));
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
    public async Task ExecuteToolAsync_GetDocumentMetadata_WithValidDocumentId_ShouldCallService()
    {
        // Arrange
        const string documentId = "metadata-test-doc";
        var metadata = new GoogleDriveFileMetadata
        {
            Id = documentId,
            Name = "Test Document.pdf",
            MimeType = "application/pdf",
            Size = 1024,
            CreatedTime = DateTimeOffset.UtcNow.AddDays(-1),
            ModifiedTime = DateTimeOffset.UtcNow
        };

        _driveService.GetDocumentMetadataAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result<GoogleDriveFileMetadata>.Success(metadata));

        var request = new
        {
            tool = "get_document_metadata",
            parameters = new
            {
                document_id = documentId
            }
        };

        // Act
        var result = await _tools.ExecuteToolAsync("get_document_metadata", request, CancellationToken.None);

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
    public async Task ExecuteToolAsync_GetActiveWatches_ShouldCallService()
    {
        // Arrange
        const string watchInfo = "Active watches: watch-123, watch-456";
        _driveService.GetActiveWatchesAsync(Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success(watchInfo));

        var request = new
        {
            tool = "get_active_watches",
            parameters = new { }
        };

        // Act
        var result = await _tools.ExecuteToolAsync("get_active_watches", request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await _driveService.Received(1).GetActiveWatchesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteToolAsync_StopWatching_WithValidWatchId_ShouldCallService()
    {
        // Arrange
        const string watchId = "watch-to-stop-123";
        _driveService.StopWatchingAsync(watchId, Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success("Watch stopped successfully"));

        var request = new
        {
            tool = "stop_watching",
            parameters = new
            {
                watch_id = watchId
            }
        };

        // Act
        var result = await _tools.ExecuteToolAsync("stop_watching", request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        await _driveService.Received(1).StopWatchingAsync(watchId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteToolAsync_StopWatching_WithMissingWatchId_ShouldReturnFailure()
    {
        // Arrange
        var request = new
        {
            tool = "stop_watching",
            parameters = new { }
        };

        // Act
        var result = await _tools.ExecuteToolAsync("stop_watching", request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("watch_id", StringComparison.OrdinalIgnoreCase));
    }

/// <summary>
/// End Tests Watch Management Tool Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Error Handling Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task ExecuteToolAsync_WithUnknownTool_ShouldReturnFailure()
    {
        // Arrange
        var request = new
        {
            tool = "unknown_tool",
            parameters = new { }
        };

        // Act
        var result = await _tools.ExecuteToolAsync("unknown_tool", request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("unknown", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("not found", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteToolAsync_WithNullRequest_ShouldReturnFailure()
    {
        // Act
        var result = await _tools.ExecuteToolAsync("initialize_drive", null!, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("request", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteToolAsync_WithMalformedParameters_ShouldReturnFailure()
    {
        // Arrange
        var request = "invalid-json-request";

        // Act
        var result = await _tools.ExecuteToolAsync("start_folder_watch", request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("parameter", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("format", StringComparison.OrdinalIgnoreCase));
    }

/// <summary>
/// End Tests Error Handling Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Concurrent Execution Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task ExecuteToolAsync_WithConcurrentRequests_ShouldHandleGracefully()
    {
        // Arrange
        const int concurrentRequests = 5;
        _driveService.GetActiveWatchesAsync(Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success("Mock watch list"));

        var request = new
        {
            tool = "get_active_watches",
            parameters = new { }
        };

        // Act
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => _tools.ExecuteToolAsync("get_active_watches", request, CancellationToken.None))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.ShouldAllBe(result => result is not null);
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
    public async Task ExecuteToolAsync_WithHighFrequencyRequests_ShouldMaintainPerformance()
    {
        // Arrange
        const int requestCount = 100;
        _driveService.GetActiveWatchesAsync(Arg.Any<CancellationToken>())
            .Returns(Result<string>.Success("Performance test response"));

        var request = new
        {
            tool = "get_active_watches",
            parameters = new { }
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var tasks = new List<Task<Result<object>>>();
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(_tools.ExecuteToolAsync("get_active_watches", request, CancellationToken.None));
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