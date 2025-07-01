using ExxerAi.MCPServer.Application.Tools;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace ExxerAi.MCPServer.Tests.Application.Tools;

/// <summary>
/// Comprehensive unit tests for GoogleDriveTools MCP tool implementation
/// Tests all Google Drive integration functionality and error scenarios
/// </summary>
public class GoogleDriveToolsTests
{
	private readonly IMCPGoogleDriveService _driveService;
	private readonly ILogger<GoogleDriveTools> _logger;
	private readonly GoogleDriveTools _googleDriveTools;

	/// <summary>
	/// Initializes a new instance of the GoogleDriveToolsTests class
	/// </summary>
	public GoogleDriveToolsTests()
	{
		_driveService = Substitute.For<IMCPGoogleDriveService>();
		_logger = Substitute.For<ILogger<GoogleDriveTools>>();
		_googleDriveTools = new GoogleDriveTools(_driveService, _logger);
	}

	/// <summary>
	/// Tests that GoogleDriveTools can be instantiated with valid dependencies
	/// </summary>
	[Fact]
	public void Should_CreateInstance_When_ValidDependenciesProvided()
	{
		// Arrange & Act
		var instance = new GoogleDriveTools(_driveService, _logger);

		// Assert
		instance.ShouldNotBeNull();
	}

	/// <summary>
	/// Tests that constructor throws ArgumentNullException when drive service is null
	/// </summary>
	[Fact]
	public void Should_ThrowArgumentNullException_When_DriveServiceIsNull()
	{
		// Arrange, Act & Assert
		Should.Throw<ArgumentNullException>(() => new GoogleDriveTools(null!, _logger))
			.ParamName.ShouldBe("driveService");
	}

	/// <summary>
	/// Tests that constructor throws ArgumentNullException when logger is null
	/// </summary>
	[Fact]
	public void Should_ThrowArgumentNullException_When_LoggerIsNull()
	{
		// Arrange, Act & Assert
		Should.Throw<ArgumentNullException>(() => new GoogleDriveTools(_driveService, null!))
			.ParamName.ShouldBe("logger");
	}

	/// <summary>
	/// Tests successful folder watch start
	/// </summary>
	[Fact]
	public async Task Should_StartFolderWatch_When_ValidFolderIdProvided()
	{
		// Arrange
		var folderId = "test-folder-123";
		var watchResponse = new MCPResponse
		{
			Id = "watch-123",
			IsSuccess = true,
			Message = "Watch started successfully"
		};

		_driveService
			.WatchFolderAsync(Arg.Any<string>(), Arg.Any<MCPWatchOptions>(), Arg.Any<CancellationToken>())
			.Returns(Result<MCPResponse>.WithSuccess(watchResponse));

		// Act
		var result = await _googleDriveTools.StartFolderWatchAsync(folderId);

		// Assert
		result.ShouldContain("✅ Started watching folder");
		result.ShouldContain(folderId);
		result.ShouldContain("watch-123");

		await _driveService.Received(1)
			.WatchFolderAsync(
				Arg.Is<string>(f => f == folderId),
				Arg.Is<MCPWatchOptions>(o => o.IncludeSubdirectories && o.AutoProcess && o.PollingIntervalSeconds == 60),
				Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Tests folder watch with custom parameters
	/// </summary>
	[Fact]
	public async Task Should_StartFolderWatchWithCustomOptions_When_ParametersProvided()
	{
		// Arrange
		var folderId = "test-folder-456";
		var watchResponse = new MCPResponse { Id = "watch-456", IsSuccess = true };

		_driveService
			.WatchFolderAsync(Arg.Any<string>(), Arg.Any<MCPWatchOptions>(), Arg.Any<CancellationToken>())
			.Returns(Result<MCPResponse>.WithSuccess(watchResponse));

		// Act
		var result = await _googleDriveTools.StartFolderWatchAsync(
			folderId, 
			includeSubdirectories: false, 
			autoProcess: false, 
			pollingIntervalSeconds: 120);

		// Assert
		result.ShouldContain("✅ Started watching folder");

		await _driveService.Received(1)
			.WatchFolderAsync(
				Arg.Is<string>(f => f == folderId),
				Arg.Is<MCPWatchOptions>(o => !o.IncludeSubdirectories && !o.AutoProcess && o.PollingIntervalSeconds == 120),
				Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Tests folder watch failure handling
	/// </summary>
	[Fact]
	public async Task Should_ReturnFailureMessage_When_FolderWatchFails()
	{
		// Arrange
		var folderId = "invalid-folder";
		var errorMessage = "Folder not found";

		_driveService
			.WatchFolderAsync(Arg.Any<string>(), Arg.Any<MCPWatchOptions>(), Arg.Any<CancellationToken>())
			.Returns(Result<MCPResponse>.WithFailure(errorMessage));

		// Act
		var result = await _googleDriveTools.StartFolderWatchAsync(folderId);

		// Assert
		result.ShouldContain("❌ Failed to start watching folder");
		result.ShouldContain(errorMessage);
	}

	/// <summary>
	/// Tests successful document changes retrieval
	/// </summary>
	[Fact]
	public async Task Should_GetDocumentChanges_When_ValidWatchIdProvided()
	{
		// Arrange
		var watchId = "watch-123";
		var changes = new List<DocumentChange>
		{
			new()
			{
				Metadata = new MCPDocumentMetadata { Name = "document1.pdf" },
				ChangeType = ChangeType.Created,
				DetectedAt = DateTime.UtcNow
			},
			new()
			{
				Metadata = new MCPDocumentMetadata { Name = "document2.docx" },
				ChangeType = ChangeType.Modified,
				DetectedAt = DateTime.UtcNow.AddMinutes(-5)
			}
		};

		_driveService
			.GetDocumentChangesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<DocumentChange>>.WithSuccess(changes));

		// Act
		var result = await _googleDriveTools.GetDocumentChangesAsync(watchId);

		// Assert
		result.ShouldContain("📋 Document Changes (2):");
		result.ShouldContain("document1.pdf (Created)");
		result.ShouldContain("document2.docx (Modified)");

		await _driveService.Received(1)
			.GetDocumentChangesAsync(watchId, Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Tests empty document changes retrieval
	/// </summary>
	[Fact]
	public async Task Should_ReturnNoChangesMessage_When_NoDocumentChangesFound()
	{
		// Arrange
		var watchId = "watch-empty";
		var emptyChanges = new List<DocumentChange>();

		_driveService
			.GetDocumentChangesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<DocumentChange>>.WithSuccess(emptyChanges));

		// Act
		var result = await _googleDriveTools.GetDocumentChangesAsync(watchId);

		// Assert
		result.ShouldContain("📭 No document changes detected.");
	}

	/// <summary>
	/// Tests successful document download
	/// </summary>
	[Fact]
	public async Task Should_DownloadDocument_When_ValidDocumentIdProvided()
	{
		// Arrange
		var documentId = "doc-123";
		var metadata = new MCPDocumentMetadata
		{
			Name = "test-document.pdf",
			MimeType = "application/pdf",
			ModifiedTime = DateTime.UtcNow.AddDays(-1)
		};
		var documentData = new byte[2048]; // 2KB

		_driveService
			.GetDocumentMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Result<MCPDocumentMetadata>.WithSuccess(metadata));

		_driveService
			.DownloadDocumentAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Result<byte[]>.WithSuccess(documentData));

		// Act
		var result = await _googleDriveTools.DownloadDocumentAsync(documentId);

		// Assert
		result.ShouldContain("✅ Downloaded: test-document.pdf");
		result.ShouldContain("📄 Type: application/pdf");
		result.ShouldContain("📦 Size: 2.00 KB");

		await _driveService.Received(1)
			.GetDocumentMetadataAsync(documentId, Arg.Any<CancellationToken>());
		await _driveService.Received(1)
			.DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Tests document metadata retrieval failure
	/// </summary>
	[Fact]
	public async Task Should_ReturnFailureMessage_When_DocumentMetadataRetrievalFails()
	{
		// Arrange
		var documentId = "invalid-doc";
		var errorMessage = "Document not found";

		_driveService
			.GetDocumentMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Result<MCPDocumentMetadata>.WithFailure(errorMessage));

		// Act
		var result = await _googleDriveTools.DownloadDocumentAsync(documentId);

		// Assert
		result.ShouldContain("❌ Failed to get document metadata");
		result.ShouldContain(errorMessage);
	}

	/// <summary>
	/// Tests successful health status check
	/// </summary>
	[Fact]
	public async Task Should_CheckHealthStatus_When_ServiceIsHealthy()
	{
		// Arrange
		var healthStatus = new MCPHealthStatus
		{
			IsHealthy = true,
			Version = "1.0.0",
			LastCheckTime = DateTime.UtcNow,
			StatusInfo = new Dictionary<string, object>
			{
				["ActiveConnections"] = 5,
				["QueueSize"] = 0
			}
		};

		_driveService
			.CheckMCPServerHealthAsync(Arg.Any<CancellationToken>())
			.Returns(Result<MCPHealthStatus>.WithSuccess(healthStatus));

		// Act
		var result = await _googleDriveTools.CheckHealthStatusAsync();

		// Assert
		result.ShouldContain("🏥 MCP Server Health Status:");
		result.ShouldContain("✅ Status: Healthy");
		result.ShouldContain("🔖 Version: 1.0.0");
		result.ShouldContain("ActiveConnections: 5");
	}

	/// <summary>
	/// Tests active watches retrieval
	/// </summary>
	[Fact]
	public async Task Should_GetActiveWatches_When_WatchSessionsExist()
	{
		// Arrange
		var watches = new List<MCPWatchSession>
		{
			new()
			{
				WatchId = "watch-1",
				FolderId = "folder-1",
				StartedAt = DateTime.UtcNow.AddHours(-2)
			},
			new()
			{
				WatchId = "watch-2",
				FolderId = "folder-2",
				StartedAt = DateTime.UtcNow.AddHours(-1)
			}
		};

		_driveService
			.GetActiveWatchesAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<MCPWatchSession>>.WithSuccess(watches));

		// Act
		var result = await _googleDriveTools.GetActiveWatchesAsync();

		// Assert
		result.ShouldContain("👁️ Active Watch Sessions (2):");
		result.ShouldContain("watch-1 - Folder: folder-1");
		result.ShouldContain("watch-2 - Folder: folder-2");
	}

	/// <summary>
	/// Tests successful watch session stop
	/// </summary>
	[Fact]
	public async Task Should_StopWatching_When_ValidWatchIdProvided()
	{
		// Arrange
		var watchId = "watch-to-stop";

		_driveService
			.StopWatchingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		// Act
		var result = await _googleDriveTools.StopWatchingAsync(watchId);

		// Assert
		result.ShouldContain("✅ Successfully stopped watching session");
		result.ShouldContain(watchId);

		await _driveService.Received(1)
			.StopWatchingAsync(watchId, Arg.Any<CancellationToken>());
	}

	/// <summary>
	/// Tests exception handling in folder watch
	/// </summary>
	[Fact]
	public async Task Should_HandleException_When_FolderWatchThrowsException()
	{
		// Arrange
		var folderId = "problematic-folder";
		var exceptionMessage = "Network timeout";

		_driveService
			.WatchFolderAsync(Arg.Any<string>(), Arg.Any<MCPWatchOptions>(), Arg.Any<CancellationToken>())
			.Throws(new InvalidOperationException(exceptionMessage));

		// Act
		var result = await _googleDriveTools.StartFolderWatchAsync(folderId);

		// Assert
		result.ShouldContain("❌ Error starting folder watch:");
		result.ShouldContain(exceptionMessage);
	}

	/// <summary>
	/// Tests proper logging during successful operations
	/// </summary>
	[Fact]
	public async Task Should_LogInformation_When_OperationsSucceed()
	{
		// Arrange
		var folderId = "test-folder";
		var watchResponse = new MCPResponse { Id = "watch-123", IsSuccess = true };

		_driveService
			.WatchFolderAsync(Arg.Any<string>(), Arg.Any<MCPWatchOptions>(), Arg.Any<CancellationToken>())
			.Returns(Result<MCPResponse>.WithSuccess(watchResponse));

		// Act
		await _googleDriveTools.StartFolderWatchAsync(folderId);

		// Assert
		_logger.Received(1).LogInformation("Starting folder watch for folder {FolderId}", folderId);
		_logger.Received(1).LogInformation("Successfully started watching folder {FolderId}", folderId);
	}

	/// <summary>
	/// Tests proper error logging during failures
	/// </summary>
	[Fact]
	public async Task Should_LogErrors_When_OperationsFail()
	{
		// Arrange
		var folderId = "failing-folder";
		var errorMessage = "Access denied";

		_driveService
			.WatchFolderAsync(Arg.Any<string>(), Arg.Any<MCPWatchOptions>(), Arg.Any<CancellationToken>())
			.Returns(Result<MCPResponse>.WithFailure(errorMessage));

		// Act
		await _googleDriveTools.StartFolderWatchAsync(folderId);

		// Assert
		_logger.Received(1).LogError("Failed to start watching folder {FolderId}: {Error}", folderId, errorMessage);
	}
} 