using ExxerAi.MCPServer.Application.Interfaces;
using ExxerAi.MCPServer.Application.Tools;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAi.MCPServer.Tests;

/// <summary>
/// Unit tests for GoogleDriveTools demonstrating dependency injection and mocking capabilities
/// </summary>
public class GoogleDriveToolsTests
{
	private readonly ILogger<GoogleDriveTools> _mockLogger;
	private readonly IGoogleDriveTools _googleDriveTools;

	/// <summary>
	/// Initializes a new instance of the GoogleDriveToolsTests class
	/// </summary>
	public GoogleDriveToolsTests()
	{
		_mockLogger = Substitute.For<ILogger<GoogleDriveTools>>();
		_googleDriveTools = new GoogleDriveTools(_mockLogger);
	}

	/// <summary>
	/// Tests that StartFolderWatchAsync returns a properly formatted response
	/// </summary>
	[Fact]
	public async Task StartFolderWatchAsync_Should_ReturnFormattedResponse_When_ValidParameters()
	{
		// Arrange
		const string folderId = "test-folder-123";
		const bool includeSubdirectories = true;
		const bool autoProcess = true;
		const int pollingInterval = 30;

		// Act
		var result = await _googleDriveTools.StartFolderWatchAsync(folderId, includeSubdirectories, autoProcess, pollingInterval);

		// Assert
		result.ShouldNotBeNull();
		result.ShouldContain("✅ Started watching Google Drive folder");
		result.ShouldContain(folderId);
		result.ShouldContain("Include Subdirectories: True");
		result.ShouldContain("Auto Process: True");
		result.ShouldContain("Polling Interval: 30s");
		result.ShouldContain("Watch ID: watch_");
	}

	/// <summary>
	/// Tests that StartFolderWatchAsync logs the correct information
	/// </summary>
	[Fact]
	public async Task StartFolderWatchAsync_Should_LogCorrectInformation_When_Called()
	{
		// Arrange
		const string folderId = "test-folder-456";

		// Act
		await _googleDriveTools.StartFolderWatchAsync(folderId);

		// Assert - Verify that logging methods were called the expected number of times
		_mockLogger.Received(2).Log(
			LogLevel.Information,
			Arg.Any<EventId>(),
			Arg.Any<object>(),
			Arg.Any<Exception?>(),
			Arg.Any<Func<object, Exception?, string>>());
	}

	/// <summary>
	/// Tests that GetDocumentChangesAsync returns properly formatted changes
	/// </summary>
	[Fact]
	public async Task GetDocumentChangesAsync_Should_ReturnFormattedChanges_When_ValidWatchId()
	{
		// Arrange
		const string watchId = "watch_12345678";

		// Act
		var result = await _googleDriveTools.GetDocumentChangesAsync(watchId);

		// Assert
		result.ShouldNotBeNull();
		result.ShouldContain($"📋 Document Changes for Watch {watchId}:");
		result.ShouldContain("Document 1: Updated contract.pdf");
		result.ShouldContain("Document 2: New invoice_2024.pdf");
		result.ShouldContain("Document 3: Modified report.docx");
		result.ShouldContain("Total Changes: 3 documents");
	}

	/// <summary>
	/// Tests that DownloadDocumentAsync returns download information
	/// </summary>
	[Fact]
	public async Task DownloadDocumentAsync_Should_ReturnDownloadInfo_When_ValidDocumentId()
	{
		// Arrange
		const string documentId = "doc_123456789";

		// Act
		var result = await _googleDriveTools.DownloadDocumentAsync(documentId);

		// Assert
		result.ShouldNotBeNull();
		result.ShouldContain("✅ Downloaded: Document");
		result.ShouldContain(documentId);
		result.ShouldContain("📄 Name: Document_");
		result.ShouldContain("📋 Type: application/pdf");
		result.ShouldContain("📦 Size: 256 KB");
		result.ShouldContain("✅ Status: Download completed successfully");
	}

	/// <summary>
	/// Tests that GetDocumentMetadataAsync returns comprehensive metadata
	/// </summary>
	[Fact]
	public async Task GetDocumentMetadataAsync_Should_ReturnMetadata_When_ValidDocumentId()
	{
		// Arrange
		const string documentId = "meta_test_doc";

		// Act
		var result = await _googleDriveTools.GetDocumentMetadataAsync(documentId);

		// Assert
		result.ShouldNotBeNull();
		result.ShouldContain("📄 Document Metadata:");
		result.ShouldContain($"🆔 ID: {documentId}");
		result.ShouldContain("🏷️ Name: Document_");
		result.ShouldContain("📁 Path: /drive/documents/");
		result.ShouldContain("📋 MIME Type: application/pdf");
		result.ShouldContain("👤 Owner: user@example.com");
		result.ShouldContain("🔒 Permissions: Read/Write");
	}

	/// <summary>
	/// Tests that CheckHealthStatusAsync returns health information
	/// </summary>
	[Fact]
	public async Task CheckHealthStatusAsync_Should_ReturnHealthStatus_When_Called()
	{
		// Act
		var result = await _googleDriveTools.CheckHealthStatusAsync();

		// Assert
		result.ShouldNotBeNull();
		result.ShouldContain("🏥 Google Drive MCP Health Status:");
		result.ShouldContain("✅ Status: Healthy");
		result.ShouldContain("🔗 API Connection: Connected");
		result.ShouldContain("🔑 Authentication: Valid");
		result.ShouldContain("🔖 Version: 1.0.0");
		result.ShouldContain("📊 Active Watches: 2");
		result.ShouldContain("📈 Status: All systems operational");
	}

	/// <summary>
	/// Tests that GetActiveWatchesAsync returns watch session information
	/// </summary>
	[Fact]
	public async Task GetActiveWatchesAsync_Should_ReturnWatchSessions_When_Called()
	{
		// Act
		var result = await _googleDriveTools.GetActiveWatchesAsync();

		// Assert
		result.ShouldNotBeNull();
		result.ShouldContain("👁️ Active Google Drive Watch Sessions:");
		result.ShouldContain("🔍 Watch ID: watch_12345678");
		result.ShouldContain("📂 Folder: Documents (/drive/documents/)");
		result.ShouldContain("🔍 Watch ID: watch_87654321");
		result.ShouldContain("📂 Folder: Reports (/drive/reports/)");
		result.ShouldContain("📊 Total Active Watches: 2");
	}

	/// <summary>
	/// Tests that StopWatchingAsync returns stop confirmation
	/// </summary>
	[Fact]
	public async Task StopWatchingAsync_Should_ReturnStopConfirmation_When_ValidWatchId()
	{
		// Arrange
		const string watchId = "watch_to_stop";

		// Act
		var result = await _googleDriveTools.StopWatchingAsync(watchId);

		// Assert
		result.ShouldNotBeNull();
		result.ShouldContain($"✅ Successfully stopped watching session {watchId}");
		result.ShouldContain("🛑 Watch Status: Stopped");
		result.ShouldContain("📊 Session Duration: 2h 15m 30s");
		result.ShouldContain("📄 Documents Processed: 15");
		result.ShouldContain("💾 Resources Released: Yes");
	}

	/// <summary>
	/// Tests the interface contract - all methods should be implemented
	/// </summary>
	[Fact]
	public void GoogleDriveTools_Should_ImplementAllInterfaceMethods()
	{
		// Assert - This test verifies that GoogleDriveTools properly implements IGoogleDriveTools
		_googleDriveTools.ShouldBeAssignableTo<IGoogleDriveTools>();
		_googleDriveTools.ShouldBeOfType<GoogleDriveTools>();
	}

	/// <summary>
	/// Tests constructor with null logger should throw ArgumentNullException
	/// </summary>
	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_LoggerIsNull()
	{
		// Act & Assert
		Should.Throw<ArgumentNullException>(() => new GoogleDriveTools(null!));
	}
} 