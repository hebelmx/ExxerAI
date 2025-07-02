using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Implementation tests for DocumentIngestionService - tests the real service implementation with mocked dependencies
/// </summary>
public class DocumentIngestionServiceImplementationTests
{
private readonly IPolymorphicDocumentProcessor _mockDocumentProcessor;
private readonly IDocumentHashGenerator _mockHashGenerator;
private readonly ILogger<DocumentIngestionService> _mockLogger;
private readonly DocumentIngestionService _service;

public DocumentIngestionServiceImplementationTests()
{
_mockDocumentProcessor = Substitute.For<IPolymorphicDocumentProcessor>();
_mockHashGenerator = Substitute.For<IDocumentHashGenerator>();
_mockLogger = Substitute.For<ILogger<DocumentIngestionService>>();
_service = new DocumentIngestionService(_mockDocumentProcessor, _mockHashGenerator, _mockLogger);
}

[Fact]
public void Constructor_Should_ThrowArgumentNullException_When_DocumentProcessorIsNull()
{
// Act & Assert
Should.Throw<ArgumentNullException>(() => new DocumentIngestionService(null!, _mockHashGenerator, _mockLogger))
.ParamName.ShouldBe("documentProcessor");
}

[Fact]
public void Constructor_Should_ThrowArgumentNullException_When_HashGeneratorIsNull()
{
// Act & Assert
Should.Throw<ArgumentNullException>(() => new DocumentIngestionService(_mockDocumentProcessor, null!, _mockLogger))
.ParamName.ShouldBe("hashGenerator");
}

[Fact]
public void Constructor_Should_ThrowArgumentNullException_When_LoggerIsNull()
{
// Act & Assert
Should.Throw<ArgumentNullException>(() => new DocumentIngestionService(_mockDocumentProcessor, _mockHashGenerator, null!))
.ParamName.ShouldBe("logger");
}

[Fact]
public async Task StartWatchingFolderAsync_Should_ReturnFailure_When_FolderIdIsEmpty()
{
// Act
var result = await _service.StartWatchingFolderAsync("");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe("Folder ID cannot be empty");
}

[Fact]
public async Task StartWatchingFolderAsync_Should_ReturnSuccess_When_ValidFolderId()
{
// Act
var result = await _service.StartWatchingFolderAsync("test-folder-id");

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldNotBeNull();
result.Value.ShouldNotBeEmpty();
Guid.TryParse(result.Value, out _).ShouldBeTrue(); // Should be a valid GUID
}

[Fact]
public async Task StartWatchingFolderAsync_Should_ReturnFailure_When_FolderIdIsWhitespace()
{
// Act
var result = await _service.StartWatchingFolderAsync("   ");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe("Folder ID cannot be empty");
}

[Fact]
public async Task StopWatchingFolderAsync_Should_ReturnFailure_When_WatchIdNotFound()
{
// Act
var result = await _service.StopWatchingFolderAsync("nonexistent-watch-id");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe("Watch session nonexistent-watch-id not found");
}

[Fact]
public async Task StopWatchingFolderAsync_Should_ReturnSuccess_When_ValidWatchId()
{
// Arrange
var startResult = await _service.StartWatchingFolderAsync("test-folder");
var watchId = startResult.Value!;

// Act
var result = await _service.StopWatchingFolderAsync(watchId);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBeTrue();
}

[Fact]
public async Task DetectDocumentChangesAsync_Should_ReturnSuccess_When_NoPendingChanges()
{
// Act
var result = await _service.DetectDocumentChangesAsync();

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldNotBeNull();
result.Value.ShouldBeEmpty();
}

[Fact]
public async Task ProcessDocumentChangeAsync_Should_ReturnFailure_When_ChangeEventIsNull()
{
// Act
var result = await _service.ProcessDocumentChangeAsync(null!);

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe("Change event cannot be null");
}

[Fact]
public async Task ProcessDocumentChangeAsync_Should_ReturnSuccess_When_RequiresProcessingIsFalse()
{
// Arrange - create a change event that does not require processing (Renamed or PermissionsChanged)
var changeEvent = new DocumentChangeEvent
{
EventId = Guid.NewGuid().ToString(),
DocumentId = "test-doc-id",
ChangeType = DocumentChangeType.Renamed, // Renamed does not require processing
WatchSessionId = "test-session"
};

// Act
var result = await _service.ProcessDocumentChangeAsync(changeEvent);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldNotBeNull();
result.Value.DocumentId.ShouldBe(changeEvent.DocumentId);
result.Value.Confidence.ShouldBe(1.0f);
}

[Fact]
public async Task ProcessDocumentChangeAsync_Should_HandleDeletedDocuments()
{
// Arrange
var changeEvent = new DocumentChangeEvent
{
EventId = Guid.NewGuid().ToString(),
DocumentId = "deleted-doc-id",
ChangeType = DocumentChangeType.Deleted,
WatchSessionId = "test-session"
};

// Act
var result = await _service.ProcessDocumentChangeAsync(changeEvent);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldNotBeNull();
result.Value.DocumentId.ShouldBe(changeEvent.DocumentId);
result.Value.Confidence.ShouldBe(1.0f);
result.Value.ProcessingTimeMs.ShouldBeGreaterThan(0);
}

[Fact]
public async Task ProcessDocumentChangeAsync_Should_ProcessValidChangeEvent()
{
// Arrange
var changeEvent = new DocumentChangeEvent
{
EventId = Guid.NewGuid().ToString(),
DocumentId = "test-doc-id",
ChangeType = DocumentChangeType.Modified,
WatchSessionId = "test-session",
Metadata = new DocumentMetadata { DocumentId = "test-doc-id" }
};

var expectedResult = new DocumentProcessingResult
{
DocumentId = "test-doc-id",
Confidence = 0.95f,
LLMConfidence = 0.95f,
GroundingConfidence = 0.95f
};

_mockDocumentProcessor.ProcessDocumentAsync(
Arg.Any<byte[]>(),
Arg.Any<DocumentMetadata>(),
Arg.Any<CancellationToken>())
.Returns(Result<DocumentProcessingResult>.WithSuccess(expectedResult));

// Act
var result = await _service.ProcessDocumentChangeAsync(changeEvent);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBe(expectedResult);
}

[Fact]
public async Task IngestDocumentAsync_Should_ReturnFailure_When_DocumentIdIsEmpty()
{
// Act
var result = await _service.IngestDocumentAsync("");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe("Document ID cannot be empty");
}

[Fact]
public async Task IngestDocumentAsync_Should_ReturnFailure_When_DocumentIdIsWhitespace()
{
// Act
var result = await _service.IngestDocumentAsync("   ");

// Assert
result.IsFailure.ShouldBeTrue();
result.Error.ShouldBe("Document ID cannot be empty");
}

[Fact]
public async Task IngestDocumentAsync_Should_ProcessValidDocument()
{
// Arrange
var documentId = "test-document-id";
var expectedResult = new DocumentProcessingResult
{
DocumentId = documentId,
Confidence = 0.90f,
LLMConfidence = 0.90f,
GroundingConfidence = 0.90f
};

_mockDocumentProcessor.ProcessDocumentAsync(
Arg.Any<byte[]>(),
Arg.Any<DocumentMetadata>(),
Arg.Any<CancellationToken>())
.Returns(Result<DocumentProcessingResult>.WithSuccess(expectedResult));

// Act
var result = await _service.IngestDocumentAsync(documentId);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBe(expectedResult);
}

[Fact]
public async Task IngestDocumentAsync_Should_ForceReprocessWhenRequested()
{
// Arrange
var documentId = "existing-document-id";
var expectedResult = new DocumentProcessingResult
{
DocumentId = documentId,
Confidence = 0.85f,
LLMConfidence = 0.85f,
GroundingConfidence = 0.85f
};

_mockDocumentProcessor.ProcessDocumentAsync(
Arg.Any<byte[]>(),
Arg.Any<DocumentMetadata>(),
Arg.Any<CancellationToken>())
.Returns(Result<DocumentProcessingResult>.WithSuccess(expectedResult));

// Act
var result = await _service.IngestDocumentAsync(documentId, forceReprocess: true);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBe(expectedResult);
}

[Fact]
public async Task IsDocumentModifiedAsync_Should_ReturnTrueForModifiedDocument()
{
// Arrange
var documentId = "test-doc-id";
var lastProcessed = DateTime.UtcNow.AddDays(-1);

// Act
var result = await _service.IsDocumentModifiedAsync(documentId, lastProcessed);

// Assert
result.IsSuccess.ShouldBeTrue();
// The implementation simulates recent modification, so it should return true
result.Value.ShouldBeTrue();
}

[Fact]
public async Task GetIngestionStatusAsync_Should_ReturnValidStatus()
{
// Act
var result = await _service.GetIngestionStatusAsync();

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldNotBeNull();
result.Value!.DocumentsWatched.ShouldBeGreaterThanOrEqualTo(0);
result.Value.ActiveWatchSessions.ShouldBeGreaterThanOrEqualTo(0);
result.Value.PendingChanges.ShouldBeGreaterThanOrEqualTo(0);
}

[Fact]
public async Task GetIngestionStatusAsync_Should_IncludeActiveSessionsInStatus()
{
// Arrange
var startResult = await _service.StartWatchingFolderAsync("test-folder");
startResult.IsSuccess.ShouldBeTrue();

// Act
var result = await _service.GetIngestionStatusAsync();

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value!.ActiveWatchSessions.ShouldBe(1);
}

[Fact]
public async Task Integration_StartWatch_ProcessChange_StopWatch_Should_WorkCorrectly()
{
// Arrange
var folderId = "integration-test-folder";

// Act 1: Start watching
var watchResult = await _service.StartWatchingFolderAsync(folderId);
watchResult.IsSuccess.ShouldBeTrue();

// Act 2: Get status
var statusResult = await _service.GetIngestionStatusAsync();
statusResult.IsSuccess.ShouldBeTrue();
statusResult.Value!.ActiveWatchSessions.ShouldBe(1);

// Act 3: Stop watching
var stopResult = await _service.StopWatchingFolderAsync(watchResult.Value!);
stopResult.IsSuccess.ShouldBeTrue();

// Act 4: Verify status updated
var finalStatusResult = await _service.GetIngestionStatusAsync();
finalStatusResult.IsSuccess.ShouldBeTrue();
finalStatusResult.Value!.ActiveWatchSessions.ShouldBe(0);

// Assert
watchResult.IsSuccess.ShouldBeTrue();
stopResult.IsSuccess.ShouldBeTrue();
}

[Fact]
public async Task ProcessDocumentChangeAsync_Should_HandleModifiedDocuments()
{
// Arrange
var changeEvent = new DocumentChangeEvent
{
EventId = Guid.NewGuid().ToString(),
DocumentId = "modified-doc-id",
ChangeType = DocumentChangeType.Modified,
WatchSessionId = "test-session",
Metadata = new DocumentMetadata { DocumentId = "modified-doc-id" }
};

var expectedResult = new DocumentProcessingResult
{
DocumentId = "modified-doc-id",
Confidence = 0.88f,
LLMConfidence = 0.88f,
GroundingConfidence = 0.88f
};

_mockDocumentProcessor.ProcessDocumentAsync(
Arg.Any<byte[]>(),
Arg.Any<DocumentMetadata>(),
Arg.Any<CancellationToken>())
.Returns(Result<DocumentProcessingResult>.WithSuccess(expectedResult));

// Act
var result = await _service.ProcessDocumentChangeAsync(changeEvent);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBe(expectedResult);
}
}
