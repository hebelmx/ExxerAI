using ExxerAi.MCPServer.Application.Interfaces;
using ExxerAi.MCPServer.Application.Tools;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;
using ExxerAI.Domain;
using ExxerAi.MCPServer.Application.Services;

namespace ExxerAi.MCPServer.Tests;

/// <summary>
/// Comprehensive unit tests for GoogleDriveTools MCP layer
/// Tests all MCP tool methods and their integration with GoogleDriveService
/// </summary>
public class GoogleDriveToolsTests
{
	private readonly ILogger<GoogleDriveTools> _mockLogger;
	private readonly IGoogleDriveService _mockGoogleDriveService;
	private readonly GoogleDriveTools _sut;

	/// <summary>
	/// Initializes test fixtures with mocked dependencies
	/// </summary>
	public GoogleDriveToolsTests()
	{
		_mockLogger = Substitute.For<ILogger<GoogleDriveTools>>();
		_mockGoogleDriveService = Substitute.For<IGoogleDriveService>();
		
		_sut = new GoogleDriveTools(_mockLogger, _mockGoogleDriveService);
	}

	/// <summary>
	/// Test class for GoogleDriveTools instantiation validation
	/// </summary>
	public class Constructor : GoogleDriveToolsTests
	{
		[Fact]
		public void Should_CreateInstance_When_ValidDependenciesProvided()
		{
			// Arrange & Act & Assert
			_sut.ShouldNotBeNull();
			_sut.ShouldBeOfType<GoogleDriveTools>();
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_LoggerIsNull()
		{
			// Arrange & Act & Assert
			Should.Throw<ArgumentNullException>(() => new GoogleDriveTools(null!!!!, _mockGoogleDriveService))
				.ParamName.ShouldBe("logger");
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_GoogleDriveServiceIsNull()
		{
			// Arrange & Act & Assert
			Should.Throw<ArgumentNullException>(() => new GoogleDriveTools(_mockLogger, null!!!!))
				.ParamName.ShouldBe("googleDriveService");
		}
	}

	/// <summary>
	/// Test class for StartFolderWatchAsync MCP tool
	/// </summary>
	public class StartFolderWatchAsync : GoogleDriveToolsTests
	{
		[Fact]
		public async Task Should_CallGoogleDriveService_When_ValidParametersProvided()
		{
			// Arrange
			var folderId = "test-folder-123";
			var includeSubdirectories = true;
			var autoProcess = true;
			var pollingInterval = 60;
			
			_mockGoogleDriveService.StartFolderWatchAsync(folderId, includeSubdirectories, autoProcess, pollingInterval)
				.Returns(Result<string>.WithSuccess("✅ Watch started successfully"));

			// Act
			var result = await _sut.StartFolderWatchAsync(folderId, includeSubdirectories, autoProcess, pollingInterval);

			// Assert
			await _mockGoogleDriveService.Received(1).StartFolderWatchAsync(folderId, includeSubdirectories, autoProcess, pollingInterval);
			result.IsSuccess.ShouldBeTrue();
		}

		[Fact]
		public async Task Should_ReturnSuccess_When_GoogleDriveServiceSucceeds()
		{
			// Arrange
			var folderId = "test-folder-123";
			var expectedResult = "✅ Started watching Google Drive folder: Test Folder";
			
			_mockGoogleDriveService.StartFolderWatchAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<int>())
				.Returns(Result<string>.WithSuccess(expectedResult));

			// Act
			var result = await _sut.StartFolderWatchAsync(folderId);

			// Assert
			result.IsSuccess.ShouldBeTrue();
			result.Value!.ShouldBe(expectedResult);
		}

		[Fact]
		public async Task Should_ReturnFailure_When_GoogleDriveServiceFails()
		{
			// Arrange
			var folderId = "invalid-folder";
			var expectedError = "Folder not found or not accessible";
			
			_mockGoogleDriveService.StartFolderWatchAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<int>())
				.Returns(Result<string>.WithFailure(expectedError));

			// Act
			var result = await _sut.StartFolderWatchAsync(folderId);

			// Assert
			result.IsSuccess.ShouldBeFalse();
			result.Error!.ShouldContain(expectedError);
		}

		[Theory]
		[InlineData(true, true, 30)]
		[InlineData(false, false, 120)]
		[InlineData(true, false, 60)]
		[InlineData(false, true, 300)]
		public async Task Should_PassCorrectParameters_When_StartingWatch(
			bool includeSubdirectories, 
			bool autoProcess, 
			int pollingInterval)
		{
			// Arrange
			var folderId = "test-folder-123";
			
			_mockGoogleDriveService.StartFolderWatchAsync(folderId, includeSubdirectories, autoProcess, pollingInterval)
				.Returns(Result<string>.WithSuccess("Success"));

			// Act
			await _sut.StartFolderWatchAsync(folderId, includeSubdirectories, autoProcess, pollingInterval);

			// Assert
			await _mockGoogleDriveService.Received(1).StartFolderWatchAsync(folderId, includeSubdirectories, autoProcess, pollingInterval);
		}
	}

	/// <summary>
	/// Test class for DownloadDocumentAsync MCP tool
	/// </summary>
	public class DownloadDocumentAsync : GoogleDriveToolsTests
	{
		[Fact]
		public async Task Should_CallGoogleDriveService_When_ValidDocumentIdProvided()
		{
			// Arrange
			var documentId = "test-document-123";
			var sampleMetadata = GoogleDriveTestData.CreateSampleFileMetadata(documentId);
			var sampleFileData = System.Text.Encoding.UTF8.GetBytes("Sample PDF content");
			
			_mockGoogleDriveService.GetDocumentMetadataAsync(documentId)
				.Returns(Result<GoogleDriveFileMetadata>.WithSuccess(sampleMetadata));
			_mockGoogleDriveService.DownloadDocumentAsync(documentId)
				.Returns(Result<byte[]>.WithSuccess(sampleFileData));

			// Act
			var result = await _sut.DownloadDocumentAsync(documentId);

			// Assert
			await _mockGoogleDriveService.Received(1).GetDocumentMetadataAsync(documentId);
			await _mockGoogleDriveService.Received(1).DownloadDocumentAsync(documentId);
			result.IsSuccess.ShouldBeTrue();
		}

		[Fact]
		public async Task Should_ReturnSuccess_When_DocumentDownloadedSuccessfully()
		{
			// Arrange
			var documentId = "test-document-123";
			var sampleMetadata = GoogleDriveTestData.CreateSampleFileMetadata(documentId);
			var sampleFileData = System.Text.Encoding.UTF8.GetBytes("Sample PDF content");
			
			_mockGoogleDriveService.GetDocumentMetadataAsync(documentId)
				.Returns(Result<GoogleDriveFileMetadata>.WithSuccess(sampleMetadata));
			_mockGoogleDriveService.DownloadDocumentAsync(documentId)
				.Returns(Result<byte[]>.WithSuccess(sampleFileData));

			// Act
			var result = await _sut.DownloadDocumentAsync(documentId);

			// Assert
			result.IsSuccess.ShouldBeTrue();
			result.Value!.ShouldContain("✅ Downloaded:");
			result.Value!.ShouldContain(sampleMetadata.Name);
			result.Value!.ShouldContain($"{sampleFileData.Length:N0} bytes");
		}

		[Fact]
		public async Task Should_ReturnFailure_When_MetadataRetrievalFails()
		{
			// Arrange
			var documentId = "invalid-document";
			var expectedError = "Document not found";
			
			_mockGoogleDriveService.GetDocumentMetadataAsync(documentId)
				.Returns(Result<GoogleDriveFileMetadata>.WithFailure(expectedError));

			// Act
			var result = await _sut.DownloadDocumentAsync(documentId);

			// Assert
			result.IsSuccess.ShouldBeFalse();
			result.Error!.ShouldContain(expectedError);
		}
	}

	/// <summary>
	/// Test class for CheckHealthStatusAsync MCP tool
	/// </summary>
	public class CheckHealthStatusAsync : GoogleDriveToolsTests
	{
		[Fact]
		public async Task Should_CallGoogleDriveServiceInitialize_When_CheckingHealth()
		{
			// Arrange
			_mockGoogleDriveService.InitializeAsync()
				.Returns(Result<bool>.WithSuccess(true));
			_mockGoogleDriveService.GetActiveWatchesAsync()
				.Returns(Result<string>.WithSuccess("No active watches"));

			// Act
			var result = await _sut.CheckHealthStatusAsync();

			// Assert
			await _mockGoogleDriveService.Received(1).InitializeAsync();
			await _mockGoogleDriveService.Received(1).GetActiveWatchesAsync();
			result.IsSuccess.ShouldBeTrue();
		}

		[Fact]
		public async Task Should_ReturnHealthyStatus_When_InitializationSucceeds()
		{
			// Arrange
			_mockGoogleDriveService.InitializeAsync()
				.Returns(Result<bool>.WithSuccess(true));
			_mockGoogleDriveService.GetActiveWatchesAsync()
				.Returns(Result<string>.WithSuccess("No active watches"));

			// Act
			var result = await _sut.CheckHealthStatusAsync();

			// Assert
			result.IsSuccess.ShouldBeTrue();
			result.Value!.ShouldContain("✅ Status: Healthy");
			result.Value!.ShouldContain("🔗 API Connection: ✅ Connected");
			result.Value!.ShouldContain("🔑 Authentication: ✅ Valid");
		}

		[Fact]
		public async Task Should_ReturnUnhealthyStatus_When_InitializationFails()
		{
			// Arrange
			var expectedError = "OAuth credentials not configured";
			_mockGoogleDriveService.InitializeAsync()
				.Returns(Result<bool>.WithFailure(expectedError));
			_mockGoogleDriveService.GetActiveWatchesAsync()
				.Returns(Result<string>.WithSuccess("No active watches"));

			// Act
			var result = await _sut.CheckHealthStatusAsync();

			// Assert
			result.IsSuccess.ShouldBeTrue(); // Health check itself succeeds, but reports unhealthy status
			result.Value!.ShouldContain("✅ Status: Unhealthy");
			result.Value!.ShouldContain("🔗 API Connection: ❌ Failed");
			result.Value!.ShouldContain("🔑 Authentication: ❌ Invalid");
			result.Value!.ShouldContain(expectedError);
		}
	}

	/// <summary>
	/// Test class for GetActiveWatchesAsync MCP tool
	/// </summary>
	public class GetActiveWatchesAsync : GoogleDriveToolsTests
	{
		[Fact]
		public async Task Should_CallGoogleDriveService_When_GettingActiveWatches()
		{
			// Arrange
			var expectedResult = "👁️ Active Google Drive Watch Sessions:\n📊 Total Active Watches: 0";
			
			_mockGoogleDriveService.GetActiveWatchesAsync()
				.Returns(Result<string>.WithSuccess(expectedResult));

			// Act
			var result = await _sut.GetActiveWatchesAsync();

			// Assert
			await _mockGoogleDriveService.Received(1).GetActiveWatchesAsync();
			result.IsSuccess.ShouldBeTrue();
		}

		[Fact]
		public async Task Should_ReturnSuccess_When_ActiveWatchesRetrievedSuccessfully()
		{
			// Arrange
			var expectedResult = "👁️ Active Google Drive Watch Sessions:\n📊 Total Active Watches: 2";
			
			_mockGoogleDriveService.GetActiveWatchesAsync()
				.Returns(Result<string>.WithSuccess(expectedResult));

			// Act
			var result = await _sut.GetActiveWatchesAsync();

			// Assert
			result.IsSuccess.ShouldBeTrue();
			result.Value!.ShouldBe(expectedResult);
		}

		[Fact]
		public async Task Should_ReturnFailure_When_ActiveWatchesRetrievalFails()
		{
			// Arrange
			var expectedError = "Failed to retrieve active watches";
			
			_mockGoogleDriveService.GetActiveWatchesAsync()
				.Returns(Result<string>.WithFailure(expectedError));

			// Act
			var result = await _sut.GetActiveWatchesAsync();

			// Assert
			result.IsSuccess.ShouldBeFalse();
			result.Error!.ShouldContain(expectedError);
		}
	}

	/// <summary>
	/// Test class for StopWatchingAsync MCP tool
	/// </summary>
	public class StopWatchingAsync : GoogleDriveToolsTests
	{
		[Fact]
		public async Task Should_CallGoogleDriveService_When_ValidWatchIdProvided()
		{
			// Arrange
			var watchId = "watch_12345678";
			var expectedResult = "✅ Successfully stopped watching session";
			
			_mockGoogleDriveService.StopWatchingAsync(watchId)
				.Returns(Result<string>.WithSuccess(expectedResult));

			// Act
			var result = await _sut.StopWatchingAsync(watchId);

			// Assert
			await _mockGoogleDriveService.Received(1).StopWatchingAsync(watchId);
			result.IsSuccess.ShouldBeTrue();
		}

		[Fact]
		public async Task Should_ReturnSuccess_When_WatchStoppedSuccessfully()
		{
			// Arrange
			var watchId = "watch_12345678";
			var expectedResult = "✅ Successfully stopped watching session watch_12345678";
			
			_mockGoogleDriveService.StopWatchingAsync(watchId)
				.Returns(Result<string>.WithSuccess(expectedResult));

			// Act
			var result = await _sut.StopWatchingAsync(watchId);

			// Assert
			result.IsSuccess.ShouldBeTrue();
			result.Value!.ShouldBe(expectedResult);
		}

		[Fact]
		public async Task Should_ReturnFailure_When_WatchNotFound()
		{
			// Arrange
			var watchId = "watch_nonexistent";
			var expectedError = "Watch session not found";
			
			_mockGoogleDriveService.StopWatchingAsync(watchId)
				.Returns(Result<string>.WithFailure(expectedError));

			// Act
			var result = await _sut.StopWatchingAsync(watchId);

			// Assert
			result.IsSuccess.ShouldBeFalse();
			result.Error!.ShouldContain(expectedError);
		}
	}
}

/// <summary>
/// Test fixtures and data for Google Drive testing
/// </summary>
public static class GoogleDriveTestData
{
	/// <summary>
	/// Sample Google Drive file metadata for testing
	/// </summary>
	public static GoogleDriveFileMetadata CreateSampleFileMetadata(string id = "test-file-123")
	{
		return new GoogleDriveFileMetadata
		{
			Id = id,
			Name = $"TestDocument_{id}.pdf",
			MimeType = "application/pdf",
			Size = 256000, // 256 KB
			CreatedTime = DateTime.UtcNow.AddDays(-30),
			ModifiedTime = DateTime.UtcNow.AddDays(-1),
			WebViewLink = $"https://drive.google.com/file/d/{id}/view",
			DownloadUrl = $"https://drive.google.com/uc?id={id}"
		};
	}

	/// <summary>
	/// Valid Google Drive folder IDs for testing
	/// </summary>
	public static readonly List<string> ValidFolderIds = new()
	{
		"1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms",
		"1mGcks_2AB3De5Fg6HIjKLMnOpQrstu7vW",
		"root"
	};

	/// <summary>
	/// Valid Google Drive document IDs for testing
	/// </summary>
	public static readonly List<string> ValidDocumentIds = new()
	{
		"1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms",
		"1234567890abcdefghijklmnopqrstuvwxyz",
		"test-doc-id-12345"
	};

	/// <summary>
	/// Sample MCP tool responses for testing
	/// </summary>
	public static class SampleResponses
	{
		public const string FolderWatchStarted = "✅ Started watching Google Drive folder: Test Folder\n📂 Folder ID: test-folder-123\n🔍 Include Subdirectories: true\n⚙️ Auto Process: true\n⏱️ Polling Interval: 60s\n🆔 Watch ID: watch_12345678\n🕐 Started: 2024-01-01 12:00:00 UTC";
		
		public const string NoActiveWatches = "📋 No active watch sessions found.";
		
		public const string HealthyStatus = "🏥 Google Drive MCP Health Status:\n✅ Status: Healthy\n🔗 API Connection: ✅ Connected\n🔑 Authentication: ✅ Valid\n🔖 Version: 1.0.0 (Native C# Implementation)\n📊 Active Watches: Available\n🕐 Last Check: 2024-01-01 12:00:00 UTC\n📈 Status: All systems operational\n🔧 Implementation: Native Google APIs for .NET";
	}

	/// <summary>
	/// Sample error messages for testing
	/// </summary>
	public static class SampleErrors
	{
		public const string FolderNotFound = "Folder test-folder-123 not found or not accessible";
		public const string DocumentNotFound = "File test-document-123 not found";
		public const string WatchNotFound = "Watch session watch_12345678 not found";
		public const string OAuthNotConfigured = "Google Drive OAuth credentials not configured. Set GOOGLE_OAUTH_CLIENT_ID and GOOGLE_OAUTH_CLIENT_SECRET environment variables.";
	}
} 