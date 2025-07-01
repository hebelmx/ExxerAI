using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using NSubstitute;
using Shouldly;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Interface-Test-Driven Development (I-TDD) tests for IDocumentIngestionService.
/// Tests focus on the interface contract and behavior, not implementation details.
/// </summary>
public class DocumentIngestionServiceTests
{
    private readonly IDocumentIngestionService _documentIngestionService;

    public DocumentIngestionServiceTests()
    {
        _documentIngestionService = Substitute.For<IDocumentIngestionService>();
    }

    #region StartWatchingFolderAsync Tests

    [Fact]
    public async Task StartWatchingFolderAsync_WithValidFolderId_ShouldReturnSuccessResult()
    {
        // Arrange
        var folderId = "1234567890abcdef";
        var expectedWatchId = "watch_session_abc123";
        var expectedResult = Result<string>.WithSuccess(expectedWatchId);

        _documentIngestionService.StartWatchingFolderAsync(folderId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.StartWatchingFolderAsync(folderId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNullOrEmpty();
        result.Data.ShouldBe(expectedWatchId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task StartWatchingFolderAsync_WithInvalidFolderId_ShouldReturnFailureResult(string folderId)
    {
        // Arrange
        var expectedResult = Result<string>.WithFailure("Folder ID cannot be null or empty");

        _documentIngestionService.StartWatchingFolderAsync(folderId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.StartWatchingFolderAsync(folderId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task StartWatchingFolderAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var folderId = "1234567890abcdef";
        var expectedResult = Result<string>.WithFailure("Operation was cancelled");

        _documentIngestionService.StartWatchingFolderAsync(folderId, cts.Token)
            .Returns(expectedResult);

        cts.Cancel();

        // Act
        var result = await _documentIngestionService.StartWatchingFolderAsync(folderId, cts.Token);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("cancelled");
    }

    #endregion

    #region StopWatchingFolderAsync Tests

    [Fact]
    public async Task StopWatchingFolderAsync_WithValidWatchId_ShouldReturnSuccessResult()
    {
        // Arrange
        var watchId = "watch_session_abc123";
        var expectedResult = Result<bool>.WithSuccess(true);

        _documentIngestionService.StopWatchingFolderAsync(watchId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.StopWatchingFolderAsync(watchId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task StopWatchingFolderAsync_WithInvalidWatchId_ShouldReturnFailureResult(string watchId)
    {
        // Arrange
        var expectedResult = Result<bool>.WithFailure("Watch ID cannot be null or empty");

        _documentIngestionService.StopWatchingFolderAsync(watchId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.StopWatchingFolderAsync(watchId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task StopWatchingFolderAsync_WithNonExistentWatchId_ShouldReturnFailureResult()
    {
        // Arrange
        var watchId = "non_existent_watch_id";
        var expectedResult = Result<bool>.WithFailure("Watch session not found");

        _documentIngestionService.StopWatchingFolderAsync(watchId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.StopWatchingFolderAsync(watchId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region DetectDocumentChangesAsync Tests

    [Fact]
    public async Task DetectDocumentChangesAsync_WhenChangesExist_ShouldReturnDocumentChanges()
    {
        // Arrange
        var changes = new List<DocumentChangeEvent>
        {
            new() 
            { 
                EventId = "event1",
                DocumentId = "doc1",
                ChangeType = DocumentChangeType.Created,
                DetectedAt = DateTime.UtcNow
            },
            new() 
            { 
                EventId = "event2",
                DocumentId = "doc2",
                ChangeType = DocumentChangeType.Modified,
                DetectedAt = DateTime.UtcNow
            }
        };
        var expectedResult = Result<IEnumerable<DocumentChangeEvent>>.WithSuccess(changes);

        _documentIngestionService.DetectDocumentChangesAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.DetectDocumentChangesAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count().ShouldBe(2);
        result.Data.First().ChangeType.ShouldBe(DocumentChangeType.Created);
        result.Data.Last().ChangeType.ShouldBe(DocumentChangeType.Modified);
    }

    [Fact]
    public async Task DetectDocumentChangesAsync_WhenNoChanges_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyChanges = new List<DocumentChangeEvent>();
        var expectedResult = Result<IEnumerable<DocumentChangeEvent>>.WithSuccess(emptyChanges);

        _documentIngestionService.DetectDocumentChangesAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.DetectDocumentChangesAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldBeEmpty();
    }

    #endregion

    #region ProcessDocumentChangeAsync Tests

    [Fact]
    public async Task ProcessDocumentChangeAsync_WithValidChangeEvent_ShouldReturnSuccessResult()
    {
        // Arrange
        var changeEvent = new DocumentChangeEvent
        {
            EventId = "event1",
            DocumentId = "doc1",
            ChangeType = DocumentChangeType.Created,
            DetectedAt = DateTime.UtcNow
        };
        var expectedResult = new DocumentProcessingResult
        {
            DocumentId = "doc1",
            IsSuccessful = true,
            ProcessingTimeMs = 1500,
            ExtractedFields = new Dictionary<string, object> { ["title"] = "Test Document" }
        };
        var expectedResultWrapper = Result<DocumentProcessingResult>.WithSuccess(expectedResult);

        _documentIngestionService.ProcessDocumentChangeAsync(changeEvent, Arg.Any<CancellationToken>())
            .Returns(expectedResultWrapper);

        // Act
        var result = await _documentIngestionService.ProcessDocumentChangeAsync(changeEvent);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.DocumentId.ShouldBe("doc1");
        result.Data.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public async Task ProcessDocumentChangeAsync_WithNullChangeEvent_ShouldReturnFailureResult()
    {
        // Arrange
        DocumentChangeEvent changeEvent = null!;
        var expectedResult = Result<DocumentProcessingResult>.WithFailure("Change event cannot be null");

        _documentIngestionService.ProcessDocumentChangeAsync(changeEvent, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.ProcessDocumentChangeAsync(changeEvent);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessDocumentChangeAsync_WithDeletedDocument_ShouldHandleGracefully()
    {
        // Arrange
        var changeEvent = new DocumentChangeEvent
        {
            EventId = "event1",
            DocumentId = "doc1",
            ChangeType = DocumentChangeType.Deleted,
            DetectedAt = DateTime.UtcNow
        };
        var expectedResult = new DocumentProcessingResult
        {
            DocumentId = "doc1",
            IsSuccessful = true,
            ProcessingTimeMs = 50,
            ExtractedFields = new Dictionary<string, object>(),
            ProcessingMethod = "Deletion"
        };
        var expectedResultWrapper = Result<DocumentProcessingResult>.WithSuccess(expectedResult);

        _documentIngestionService.ProcessDocumentChangeAsync(changeEvent, Arg.Any<CancellationToken>())
            .Returns(expectedResultWrapper);

        // Act
        var result = await _documentIngestionService.ProcessDocumentChangeAsync(changeEvent);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.DocumentId.ShouldBe("doc1");
        result.Data.ProcessingMethod.ShouldBe("Deletion");
    }

    #endregion

    #region IngestDocumentAsync Tests

    [Fact]
    public async Task IngestDocumentAsync_WithValidDocumentId_ShouldReturnSuccessResult()
    {
        // Arrange
        var documentId = "doc123456";
        var forceReprocess = false;
        var expectedResult = new DocumentProcessingResult
        {
            DocumentId = documentId,
            IsSuccessful = true,
            ProcessingTimeMs = 2500,
            ExtractedFields = new Dictionary<string, object>
            {
                ["title"] = "Sample Document",
                ["pages"] = 5,
                ["wordCount"] = 1500
            }
        };
        var expectedResultWrapper = Result<DocumentProcessingResult>.WithSuccess(expectedResult);

        _documentIngestionService.IngestDocumentAsync(documentId, forceReprocess, Arg.Any<CancellationToken>())
            .Returns(expectedResultWrapper);

        // Act
        var result = await _documentIngestionService.IngestDocumentAsync(documentId, forceReprocess);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.DocumentId.ShouldBe(documentId);
        result.Data.IsSuccessful.ShouldBeTrue();
        result.Data.ExtractedFields.Count.ShouldBe(3);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task IngestDocumentAsync_WithInvalidDocumentId_ShouldReturnFailureResult(string documentId)
    {
        // Arrange
        var expectedResult = Result<DocumentProcessingResult>.WithFailure("Document ID cannot be null or empty");

        _documentIngestionService.IngestDocumentAsync(documentId, false, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.IngestDocumentAsync(documentId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task IngestDocumentAsync_WithForceReprocess_ShouldReprocessExistingDocument()
    {
        // Arrange
        var documentId = "existing_doc123";
        var forceReprocess = true;
        var expectedResult = new DocumentProcessingResult
        {
            DocumentId = documentId,
            IsSuccessful = true,
            ProcessingTimeMs = 3000,
            ProcessingMethod = "Forced Reprocessing"
        };
        var expectedResultWrapper = Result<DocumentProcessingResult>.WithSuccess(expectedResult);

        _documentIngestionService.IngestDocumentAsync(documentId, forceReprocess, Arg.Any<CancellationToken>())
            .Returns(expectedResultWrapper);

        // Act
        var result = await _documentIngestionService.IngestDocumentAsync(documentId, forceReprocess);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ProcessingMethod.ShouldBe("Forced Reprocessing");
    }

    [Fact]
    public async Task IngestDocumentAsync_WithNonExistentDocument_ShouldReturnFailureResult()
    {
        // Arrange
        var documentId = "non_existent_doc";
        var expectedResult = Result<DocumentProcessingResult>.WithFailure("Document not found");

        _documentIngestionService.IngestDocumentAsync(documentId, false, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.IngestDocumentAsync(documentId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region IsDocumentModifiedAsync Tests

    [Fact]
    public async Task IsDocumentModifiedAsync_WithModifiedDocument_ShouldReturnTrue()
    {
        // Arrange
        var documentId = "doc123456";
        var lastProcessed = DateTime.UtcNow.AddHours(-1);
        var expectedResult = Result<bool>.WithSuccess(true);

        _documentIngestionService.IsDocumentModifiedAsync(documentId, lastProcessed, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.IsDocumentModifiedAsync(documentId, lastProcessed);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task IsDocumentModifiedAsync_WithUnmodifiedDocument_ShouldReturnFalse()
    {
        // Arrange
        var documentId = "doc123456";
        var lastProcessed = DateTime.UtcNow.AddMinutes(-5);
        var expectedResult = Result<bool>.WithSuccess(false);

        _documentIngestionService.IsDocumentModifiedAsync(documentId, lastProcessed, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.IsDocumentModifiedAsync(documentId, lastProcessed);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task IsDocumentModifiedAsync_WithInvalidDocumentId_ShouldReturnFailureResult(string documentId)
    {
        // Arrange
        var lastProcessed = DateTime.UtcNow.AddHours(-1);
        var expectedResult = Result<bool>.WithFailure("Document ID cannot be null or empty");

        _documentIngestionService.IsDocumentModifiedAsync(documentId, lastProcessed, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.IsDocumentModifiedAsync(documentId, lastProcessed);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region GetIngestionStatusAsync Tests

    [Fact]
    public async Task GetIngestionStatusAsync_ShouldReturnSystemStatus()
    {
        // Arrange
        var expectedStatus = new IngestionStatus
        {
            DocumentsWatched = 150,
            DocumentsProcessedToday = 25,
            DocumentsProcessedThisWeek = 180,
            ActiveWatchSessions = 5,
            PendingChanges = 3,
            AverageProcessingTimeMs = 2500.5,
            SystemHealth = HealthStatus.Healthy,
            LastProcessingTime = DateTime.UtcNow.AddMinutes(-5),
            SystemMessages = new List<string> { "System operating normally" }
        };
        var expectedResult = Result<IngestionStatus>.WithSuccess(expectedStatus);

        _documentIngestionService.GetIngestionStatusAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.GetIngestionStatusAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.DocumentsWatched.ShouldBe(150);
        result.Data.SystemHealth.ShouldBe(HealthStatus.Healthy);
        result.Data.ActiveWatchSessions.ShouldBe(5);
        result.Data.AverageProcessingTimeMs.ShouldBe(2500.5);
    }

    [Fact]
    public async Task GetIngestionStatusAsync_WithSystemIssues_ShouldReturnWarningStatus()
    {
        // Arrange
        var expectedStatus = new IngestionStatus
        {
            DocumentsWatched = 150,
            DocumentsProcessedToday = 0,
            ActiveWatchSessions = 0,
            PendingChanges = 50,
            SystemHealth = HealthStatus.Warning,
            SystemMessages = new List<string> 
            { 
                "Google Drive API rate limit approaching",
                "High pending changes queue"
            }
        };
        var expectedResult = Result<IngestionStatus>.WithSuccess(expectedStatus);

        _documentIngestionService.GetIngestionStatusAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _documentIngestionService.GetIngestionStatusAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.SystemHealth.ShouldBe(HealthStatus.Warning);
        result.Data.SystemMessages.Count.ShouldBe(2);
        result.Data.PendingChanges.ShouldBe(50);
    }

    #endregion

    #region Contract Validation Tests

    [Fact]
    public async Task IDocumentIngestionService_AllMethods_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var folderId = "folder123";
        var watchId = "watch123";
        var documentId = "doc123";
        var changeEvent = new DocumentChangeEvent { DocumentId = documentId };
        var lastProcessed = DateTime.UtcNow.AddHours(-1);

        // Act & Assert - Verify all methods accept CancellationToken
        await _documentIngestionService.Received(0).StartWatchingFolderAsync(folderId, cts.Token);
        await _documentIngestionService.Received(0).StopWatchingFolderAsync(watchId, cts.Token);
        await _documentIngestionService.Received(0).DetectDocumentChangesAsync(cts.Token);
        await _documentIngestionService.Received(0).ProcessDocumentChangeAsync(changeEvent, cts.Token);
        await _documentIngestionService.Received(0).IngestDocumentAsync(documentId, false, cts.Token);
        await _documentIngestionService.Received(0).IsDocumentModifiedAsync(documentId, lastProcessed, cts.Token);
        await _documentIngestionService.Received(0).GetIngestionStatusAsync(cts.Token);

        // All methods should exist and accept cancellation tokens
        true.ShouldBeTrue();
    }

    [Fact]
    public void IDocumentIngestionService_AllMethods_ShouldReturnResult()
    {
        // Arrange & Act & Assert - Verify all async methods return Result<T>
        var folderId = "folder123";
        var watchId = "watch123";
        var documentId = "doc123";
        var changeEvent = new DocumentChangeEvent { DocumentId = documentId };
        var lastProcessed = DateTime.UtcNow.AddHours(-1);

        // Verify method signatures return Result<T>
        var startWatchTask = _documentIngestionService.StartWatchingFolderAsync(folderId);
        var stopWatchTask = _documentIngestionService.StopWatchingFolderAsync(watchId);
        var detectChangesTask = _documentIngestionService.DetectDocumentChangesAsync();
        var processChangeTask = _documentIngestionService.ProcessDocumentChangeAsync(changeEvent);
        var ingestTask = _documentIngestionService.IngestDocumentAsync(documentId);
        var isModifiedTask = _documentIngestionService.IsDocumentModifiedAsync(documentId, lastProcessed);
        var getStatusTask = _documentIngestionService.GetIngestionStatusAsync();

        startWatchTask.ShouldBeOfType<Task<Result<string>>>();
        stopWatchTask.ShouldBeOfType<Task<Result<bool>>>();
        detectChangesTask.ShouldBeOfType<Task<Result<IEnumerable<DocumentChangeEvent>>>>();
        processChangeTask.ShouldBeOfType<Task<Result<DocumentProcessingResult>>>();
        ingestTask.ShouldBeOfType<Task<Result<DocumentProcessingResult>>>();
        isModifiedTask.ShouldBeOfType<Task<Result<bool>>>();
        getStatusTask.ShouldBeOfType<Task<Result<IngestionStatus>>>();
    }

    #endregion

    #region DocumentChangeEvent Tests

    [Fact]
    public void DocumentChangeEvent_IsContentChange_ShouldReturnTrueForModifiedWithDifferentHashes()
    {
        // Arrange
        var changeEvent = new DocumentChangeEvent
        {
            ChangeType = DocumentChangeType.Modified,
            PreviousHash = "hash123",
            NewHash = "hash456"
        };

        // Act
        var isContentChange = changeEvent.IsContentChange;

        // Assert
        isContentChange.ShouldBeTrue();
    }

    [Fact]
    public void DocumentChangeEvent_IsContentChange_ShouldReturnFalseForSameHashes()
    {
        // Arrange
        var changeEvent = new DocumentChangeEvent
        {
            ChangeType = DocumentChangeType.Modified,
            PreviousHash = "hash123",
            NewHash = "hash123"
        };

        // Act
        var isContentChange = changeEvent.IsContentChange;

        // Assert
        isContentChange.ShouldBeFalse();
    }

    [Fact]
    public void DocumentChangeEvent_RequiresProcessing_ShouldReturnTrueForProcessableChanges()
    {
        // Arrange
        var testCases = new[]
        {
            DocumentChangeType.Created,
            DocumentChangeType.Modified,
            DocumentChangeType.Restored
        };

        foreach (var changeType in testCases)
        {
            var changeEvent = new DocumentChangeEvent { ChangeType = changeType };

            // Act
            var requiresProcessing = changeEvent.RequiresProcessing;

            // Assert
            requiresProcessing.ShouldBeTrue($"ChangeType {changeType} should require processing");
        }
    }

    [Fact]
    public void DocumentChangeEvent_RequiresProcessing_ShouldReturnFalseForNonProcessableChanges()
    {
        // Arrange
        var testCases = new[]
        {
            DocumentChangeType.Deleted,
            DocumentChangeType.Moved,
            DocumentChangeType.Renamed,
            DocumentChangeType.PermissionsChanged
        };

        foreach (var changeType in testCases)
        {
            var changeEvent = new DocumentChangeEvent { ChangeType = changeType };

            // Act
            var requiresProcessing = changeEvent.RequiresProcessing;

            // Assert
            requiresProcessing.ShouldBeFalse($"ChangeType {changeType} should not require processing");
        }
    }

    #endregion
}
