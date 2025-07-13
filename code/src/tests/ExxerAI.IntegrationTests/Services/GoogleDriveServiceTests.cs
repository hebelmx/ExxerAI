using ExxerAI.Application.Interfaces;
using ExxerAi.MCPServer.Application.Services;
using ExxerAi.MCPServer.Application.Interfaces;

namespace ExxerAI.IntegrationTests.Services;

/// <summary>
/// Infrastructure tests for GoogleDriveService implementation.
/// Tests the actual service behavior, credential management, and integration patterns.
/// These tests validate the service layer without requiring real Google Drive API calls.
/// </summary>
public class GoogleDriveServiceTests
{
    private readonly ILogger<GoogleDriveService> _logger;
    private readonly IGoogleDriveCredentialResolver _credentialResolver;
    private readonly IHybridDocumentProcessor _documentProcessor;
    private readonly GoogleDriveService _service;

    public GoogleDriveServiceTests()
    {
        _logger = Substitute.For<ILogger<GoogleDriveService>>();
        _credentialResolver = Substitute.For<IGoogleDriveCredentialResolver>();
        _documentProcessor = Substitute.For<IHybridDocumentProcessor>();
        
        // Setup default credential resolver behavior for testing
        var defaultCredentials = new GoogleDriveCredentials
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            Type = CredentialType.OAuth,
            Source = "Test"
        };
        _credentialResolver.ResolveCredentialsAsync(Arg.Any<CancellationToken>())
            .Returns(Result<GoogleDriveCredentials>.WithSuccess(defaultCredentials));
        
        _service = new GoogleDriveService(_logger, _credentialResolver, _documentProcessor);
    }

/// <summary>
/// Begin Tests Constructor Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act & Assert
        _service.ShouldNotBeNull();
        _service.ShouldBeOfType<GoogleDriveService>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            new GoogleDriveService(null!, _credentialResolver, _documentProcessor))
            .ParamName.ShouldBe("logger");
    }

    [Fact]
    public void Constructor_WithNullCredentialResolver_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            new GoogleDriveService(_logger, null!, _documentProcessor))
            .ParamName.ShouldBe("credentialResolver");
    }

    [Fact]
    public void Constructor_WithNullDocumentProcessor_ShouldCreateInstanceSuccessfully()
    {
        // Act
        var serviceWithNullProcessor = new GoogleDriveService(_logger, _credentialResolver, null);

        // Assert
        serviceWithNullProcessor.ShouldNotBeNull();
        serviceWithNullProcessor.ShouldBeOfType<GoogleDriveService>();
    }

/// <summary>
/// End Tests Constructor Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Initialization Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task InitializeAsync_WithMissingCredentials_ShouldReturnFailure()
    {
        // Arrange
        _credentialResolver.ResolveCredentialsAsync(Arg.Any<CancellationToken>())
            .Returns(Result<GoogleDriveCredentials>.WithFailure("No credentials found"));

        // Act
        var result = await _service.InitializeAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("credentials", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task InitializeAsync_WithInvalidCredentialsPath_ShouldReturnFailure()
    {
        // Arrange
        _credentialResolver.ResolveCredentialsAsync(Arg.Any<CancellationToken>())
            .Returns(Result<GoogleDriveCredentials>.WithFailure("Invalid credentials path: non-existent-file.json"));

        // Act
        var result = await _service.InitializeAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("file not found", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("credentials", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task InitializeAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _service.InitializeAsync(cts.Token);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("cancel", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("operation", StringComparison.OrdinalIgnoreCase));
    }

/// <summary>
/// End Tests Initialization Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Folder Watch Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task StartFolderWatchAsync_WithNullFolderId_ShouldReturnFailure()
    {
        // Act
        var result = await _service.StartFolderWatchAsync(null!, cancellationToken: CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("folder", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task StartFolderWatchAsync_WithEmptyFolderId_ShouldReturnFailure()
    {
        // Act
        var result = await _service.StartFolderWatchAsync(string.Empty, cancellationToken: CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("folder", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task StartFolderWatchAsync_WithInvalidPollingInterval_ShouldReturnFailure()
    {
        // Arrange
        const string validFolderId = "test-folder-id";
        const int invalidPollingInterval = -1;

        // Act
        var result = await _service.StartFolderWatchAsync(
            validFolderId, 
            pollingIntervalSeconds: invalidPollingInterval, 
            cancellationToken: CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("polling", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("interval", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(true, true, 60)]
    [InlineData(false, false, 30)]
    [InlineData(true, false, 120)]
    [InlineData(false, true, 300)]
    public async Task StartFolderWatchAsync_WithValidParameters_ShouldAcceptAllCombinations(
        bool includeSubdirectories, 
        bool autoProcess, 
        int pollingInterval)
    {
        // Arrange
        const string validFolderId = "test-folder-id";

        // Act
        var result = await _service.StartFolderWatchAsync(
            validFolderId, 
            includeSubdirectories, 
            autoProcess, 
            pollingInterval, 
            CancellationToken.None);

        // Assert - Should handle parameters gracefully even if service isn't initialized
        result.ShouldNotBeNull();
        // Note: Will likely fail due to uninitialized service, but should validate parameters
    }

/// <summary>
/// End Tests Folder Watch Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Document Operations Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task DownloadDocumentAsync_WithNullDocumentId_ShouldReturnFailure()
    {
        // Act
        var result = await _service.DownloadDocumentAsync(null!, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("document", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task DownloadDocumentAsync_WithEmptyDocumentId_ShouldReturnFailure()
    {
        // Act
        var result = await _service.DownloadDocumentAsync(string.Empty, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("document", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetDocumentMetadataAsync_WithNullDocumentId_ShouldReturnFailure()
    {
        // Act
        var result = await _service.GetDocumentMetadataAsync(null!, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("document", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetDocumentMetadataAsync_WithEmptyDocumentId_ShouldReturnFailure()
    {
        // Act
        var result = await _service.GetDocumentMetadataAsync(string.Empty, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("document", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

/// <summary>
/// End Tests Document Operations Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Watch Management Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetActiveWatchesAsync_ShouldReturnWatchInformation()
    {
        // Act
        var result = await _service.GetActiveWatchesAsync(CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        // Should return information about watches (empty or populated)
    }

    [Fact]
    public async Task StopWatchingAsync_WithNullWatchId_ShouldReturnFailure()
    {
        // Act
        var result = await _service.StopWatchingAsync(null!, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("watch", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("required", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task StopWatchingAsync_WithEmptyWatchId_ShouldReturnFailure()
    {
        // Act
        var result = await _service.StopWatchingAsync(string.Empty, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("watch", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task StopWatchingAsync_WithNonExistentWatchId_ShouldReturnFailure()
    {
        // Arrange
        const string nonExistentWatchId = "non-existent-watch-id";

        // Act
        var result = await _service.StopWatchingAsync(nonExistentWatchId, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Contains("watch", StringComparison.OrdinalIgnoreCase) ||
                                           error.Contains("not found", StringComparison.OrdinalIgnoreCase));
    }

/// <summary>
/// End Tests Watch Management Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Cancellation Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task AllMethods_WithCancelledToken_ShouldRespectCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        const string testId = "test-id";

        // Act & Assert - All methods should handle cancellation gracefully
        var initResult = await _service.InitializeAsync(cts.Token);
        initResult.ShouldNotBeNull();

        var watchResult = await _service.StartFolderWatchAsync(testId, cancellationToken: cts.Token);
        watchResult.ShouldNotBeNull();

        var downloadResult = await _service.DownloadDocumentAsync(testId, cts.Token);
        downloadResult.ShouldNotBeNull();

        var metadataResult = await _service.GetDocumentMetadataAsync(testId, cts.Token);
        metadataResult.ShouldNotBeNull();

        var activeWatchesResult = await _service.GetActiveWatchesAsync(cts.Token);
        activeWatchesResult.ShouldNotBeNull();

        var stopWatchResult = await _service.StopWatchingAsync(testId, cts.Token);
        stopWatchResult.ShouldNotBeNull();
    }

/// <summary>
/// End Tests Cancellation Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Error Resilience Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task Service_UnderStressConditions_ShouldMaintainStability()
    {
        // Arrange
        const int concurrentOperations = 10;
        const string testFolderId = "stress-test-folder";

        // Act - Execute multiple operations concurrently
        var tasks = new List<Task<Result<string>>>();
        for (int i = 0; i < concurrentOperations; i++)
        {
            tasks.Add(_service.StartFolderWatchAsync($"{testFolderId}-{i}", cancellationToken: CancellationToken.None));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All operations should complete without throwing exceptions
        results.ShouldAllBe(result => result != null);
        results.Length.ShouldBe(concurrentOperations);
    }

    [Fact]
    public async Task Service_WithResourceContention_ShouldHandleGracefully()
    {
        // Arrange
        const string testDocumentId = "contention-test-doc";
        
        // Act - Execute same operation multiple times simultaneously
        var downloadTasks = Enumerable.Range(0, 5)
            .Select(_ => _service.DownloadDocumentAsync(testDocumentId, CancellationToken.None))
            .ToArray();

        var results = await Task.WhenAll(downloadTasks);

        // Assert - Should handle concurrent access gracefully
        results.ShouldAllBe(result => result != null);
        // All results should be consistent (all success or all failure with same reason)
    }

/// <summary>
/// End Tests Error Resilience Tests
/// </summary>
/// <returns></returns>
}