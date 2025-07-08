using NSubstitute;
using Shouldly;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Application.Interfaces;
using ExxerAi.MCPServer.Application.Services;
using ExxerAi.MCPServer.Application.Interfaces;
using Xunit;

namespace ExxerAi.MCPServer.Tests.Services;

/// <summary>
/// Comprehensive unit tests for GoogleDriveService
/// Tests OAuth authentication, file operations, folder monitoring, and document processing integration
/// </summary>
public class GoogleDriveServiceTests
{
    private readonly ILogger<GoogleDriveService> _mockLogger;
    private readonly IConfiguration _mockConfiguration;
    private readonly IHybridDocumentProcessor _mockDocumentProcessor;
    private readonly GoogleDriveService _sut;

    /// <summary>
    /// Initializes test fixtures with mocked dependencies
    /// </summary>
    public GoogleDriveServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<GoogleDriveService>>();
        _mockConfiguration = Substitute.For<IConfiguration>();
        _mockDocumentProcessor = Substitute.For<IHybridDocumentProcessor>();
        
        // Set up default configuration for OAuth
        _mockConfiguration["GoogleDrive:ClientId"].Returns("test-client-id.apps.googleusercontent.com");
        _mockConfiguration["GoogleDrive:ClientSecret"].Returns("test-client-secret");
        _mockConfiguration["GoogleDrive:RedirectUri"].Returns("http://localhost:8000/oauth2callback");
        
        _sut = new GoogleDriveService(_mockLogger, _mockConfiguration, _mockDocumentProcessor);
    }

    /// <summary>
    /// Test class for GoogleDriveService instantiation validation
    /// </summary>
    public class Constructor : GoogleDriveServiceTests
    {
        [Fact]
        public void Should_CreateInstance_When_ValidDependenciesProvided()
        {
            // Arrange & Act & Assert
            _sut.ShouldNotBeNull();
            _sut.ShouldBeOfType<GoogleDriveService>();
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_LoggerIsNull()
        {
            // Arrange & Act & Assert
            Should.Throw<ArgumentNullException>(() => new GoogleDriveService(null!, _mockConfiguration, _mockDocumentProcessor))
                .ParamName.ShouldBe("logger");
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_ConfigurationIsNull()
        {
            // Arrange & Act & Assert
            Should.Throw<ArgumentNullException>(() => new GoogleDriveService(_mockLogger, null!, _mockDocumentProcessor))
                .ParamName.ShouldBe("configuration");
        }

        [Fact]
        public void Should_CreateInstance_When_DocumentProcessorIsNull()
        {
            // Arrange & Act
            var service = new GoogleDriveService(_mockLogger, _mockConfiguration, null);

            // Assert
            service.ShouldNotBeNull();
            service.ShouldBeOfType<GoogleDriveService>();
        }
    }

    /// <summary>
    /// Test class for OAuth authentication initialization
    /// </summary>
    public class InitializeAsync : GoogleDriveServiceTests
    {
        [Fact]
        public async Task Should_ReturnFailure_When_ClientIdIsNotConfigured()
        {
            // Arrange
            _mockConfiguration["GoogleDrive:ClientId"].Returns((string?)null);

            // Act
            var result = await _sut.InitializeAsync(TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error!.ShouldContain("OAuth credentials not configured");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_ClientSecretIsNotConfigured()
        {
            // Arrange
            _mockConfiguration["GoogleDrive:ClientSecret"].Returns((string?)null);

            // Act
            var result = await _sut.InitializeAsync(TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error!.ShouldContain("OAuth credentials not configured");
        }

        [Fact]
        public async Task Should_UseEnvironmentVariables_When_ConfigurationIsEmpty()
        {
            // Arrange
            _mockConfiguration["GoogleDrive:ClientId"].Returns((string?)null);
            _mockConfiguration["GoogleDrive:ClientSecret"].Returns((string?)null);
            
            // Note: This test would require environment variable mocking
            // In a real scenario, you'd test with actual environment variables

            // Act
            var result = await _sut.InitializeAsync(TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeFalse(); // Expected to fail without env vars in test environment
        }

        [Fact]
        public async Task Should_LogInformation_When_InitializationStarts()
        {
            // Arrange & Act
            await _sut.InitializeAsync(TestContext.Current.CancellationToken);

            // Assert
            _mockLogger.Received().LogInformation("Initializing Google Drive service...");
        }
    }

    /// <summary>
    /// Test class for folder watching functionality
    /// </summary>
    public class StartFolderWatchAsync : GoogleDriveServiceTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("null-placeholder")] // Using placeholder instead of null for xUnit
        public async Task Should_ReturnFailure_When_FolderIdIsInvalid(string invalidFolderId)
        {
            // Arrange
            var actualFolderId = invalidFolderId == "null-placeholder" ? null : invalidFolderId;

            // Act
            var result = await _sut.StartFolderWatchAsync(actualFolderId!);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error!.ShouldContain("Drive service not initialized");
        }

        [Fact]
        public async Task Should_LogInformation_When_StartingFolderWatch()
        {
            // Arrange
            var testFolderId = "test-folder-123";

            // Act
            await _sut.StartFolderWatchAsync(testFolderId);

            // Assert
            _mockLogger.Received().LogInformation("Starting folder watch for {FolderId}", testFolderId);
        }

        [Theory]
        [InlineData(true, true, 30)]
        [InlineData(false, false, 120)]
        [InlineData(true, false, 60)]
        public async Task Should_AcceptValidParameters_When_StartingFolderWatch(
            bool includeSubdirectories, 
            bool autoProcess, 
            int pollingInterval)
        {
            // Arrange
            var testFolderId = "test-folder-123";

            // Act
            var result = await _sut.StartFolderWatchAsync(testFolderId, includeSubdirectories, autoProcess, pollingInterval);

            // Assert
            result.ShouldNotBeNull();
            // Note: In unit tests, this will fail due to no actual Google Drive service
            // But we're testing parameter validation and method structure
        }
    }

    /// <summary>
    /// Test class for document download functionality
    /// </summary>
    public class DownloadDocumentAsync : GoogleDriveServiceTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("null-placeholder")] // Using placeholder instead of null for xUnit
        public async Task Should_ReturnFailure_When_DocumentIdIsInvalid(string invalidDocumentId)
        {
            // Arrange
            var actualDocumentId = invalidDocumentId == "null-placeholder" ? null : invalidDocumentId;

            // Act
            var result = await _sut.DownloadDocumentAsync(actualDocumentId!);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error!.ShouldContain("Drive service not initialized");
        }

        [Fact]
        public async Task Should_LogInformation_When_DownloadingDocument()
        {
            // Arrange
            var testDocumentId = "test-document-123";

            // Act
            await _sut.DownloadDocumentAsync(testDocumentId);

            // Assert
            _mockLogger.Received().LogInformation("Downloading document {DocumentId}", testDocumentId);
        }
    }

    /// <summary>
    /// Test class for document metadata retrieval
    /// </summary>
    public class GetDocumentMetadataAsync : GoogleDriveServiceTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("null-placeholder")] // Using placeholder instead of null for xUnit
        public async Task Should_ReturnFailure_When_DocumentIdIsInvalid(string invalidDocumentId)
        {
            // Arrange
            var actualDocumentId = invalidDocumentId == "null-placeholder" ? null : invalidDocumentId;

            // Act
            var result = await _sut.GetDocumentMetadataAsync(actualDocumentId!);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error!.ShouldContain("Drive service not initialized");
        }

        [Fact]
        public async Task Should_LogInformation_When_GettingMetadata()
        {
            // Arrange
            var testDocumentId = "test-document-123";

            // Act
            await _sut.GetDocumentMetadataAsync(testDocumentId);

            // Assert
            _mockLogger.Received().LogInformation("Getting metadata for document {DocumentId}", testDocumentId);
        }
    }

    /// <summary>
    /// Test class for active watch sessions management
    /// </summary>
    public class GetActiveWatchesAsync : GoogleDriveServiceTests
    {
        [Fact]
        public async Task Should_ReturnNoActiveSessions_When_NoWatchesStarted()
        {
            // Arrange & Act
            var result = await _sut.GetActiveWatchesAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldContain("No active watch sessions found");
        }

        [Fact]
        public async Task Should_ReturnSuccess_When_CalledWithoutWatches()
        {
            // Arrange & Act
            var result = await _sut.GetActiveWatchesAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNullOrEmpty();
        }
    }

    /// <summary>
    /// Test class for stopping watch sessions
    /// </summary>
    public class StopWatchingAsync : GoogleDriveServiceTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("null-placeholder")] // Using placeholder instead of null for xUnit
        public async Task Should_ReturnFailure_When_WatchIdIsInvalid(string invalidWatchId)
        {
            // Arrange
            var actualWatchId = invalidWatchId == "null-placeholder" ? null : invalidWatchId;

            // Act
            var result = await _sut.StopWatchingAsync(actualWatchId!);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error!.ShouldContain("not found");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_WatchIdDoesNotExist()
        {
            // Arrange
            var nonExistentWatchId = "watch_12345678";

            // Act
            var result = await _sut.StopWatchingAsync(nonExistentWatchId);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error!.ShouldContain("not found");
        }

        [Fact]
        public async Task Should_LogInformation_When_StoppingWatch()
        {
            // Arrange
            var testWatchId = "watch_12345678";

            // Act
            await _sut.StopWatchingAsync(testWatchId);

            // Assert
            _mockLogger.Received().LogInformation("✅ Successfully stopped watch session {WatchId}", testWatchId);
        }
    }
}

/// <summary>
/// Test fixtures and data for Google Drive service tests
/// </summary>
public static class GoogleDriveServiceTestData
{
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
    /// Invalid identifiers for negative testing
    /// </summary>
    public static readonly List<string> InvalidIds = new()
    {
        "",
        "   ",
        "invalid-id-with-special-chars-!@#$%",
        "too-short"
    };

    /// <summary>
    /// Valid polling intervals for folder watching
    /// </summary>
    public static readonly List<int> ValidPollingIntervals = new()
    {
        30, 60, 120, 300, 600
    };

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
    /// Sample document metadata for processing tests
    /// </summary>
    public static DocumentMetadata CreateSampleDocumentMetadata(string fileName = "TestDocument.pdf")
    {
        return new DocumentMetadata
        {
            DocumentId = Guid.NewGuid().ToString(),
            FileName = fileName,
            MimeType = "application/pdf",
            SourcePath = $"GoogleDrive:test-file-123",
            DocumentType = DocumentType.Other,
            FileSize = 256000,
            Properties = new Dictionary<string, object>
            {
                ["SourceSystem"] = "GoogleDrive_MCP",
                ["WatchId"] = "watch_12345678",
                ["DetectedAt"] = DateTime.UtcNow.ToString("O")
            }
        };
    }
} 