using Microsoft.Extensions.Logging;

namespace ExxerAI.Application.Tests.Services;

public class GoogleDriveServiceBehavioralTests3
{
    private readonly IGoogleDriveClient _driveClient;
    private readonly ILogger<GoogleDriveService> _logger;
    private readonly GoogleDriveService _service;

    public GoogleDriveServiceBehavioralTests3()
    {
        _driveClient = Substitute.For<IGoogleDriveClient>();
        _logger = Substitute.For<ILogger<GoogleDriveService>>();
        _service = new GoogleDriveService(_driveClient, _logger);
    }

    [Fact]
    public async Task DetectDocumentChangesAsync_Should_Return_Changes_When_ChangesExist()
    {
        var folderId = "folder123";
        var changes = new List<string> { "doc1", "doc2" };

        _driveClient.DetectChangesAsync(folderId, Arg.Any<CancellationToken>())
            .Returns(changes);

        var result = await _service.DetectDocumentChangesAsync(folderId);

        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEquivalentTo(changes);
    }

    [Fact]
    public async Task DetectDocumentChangesAsync_Should_Return_Failure_When_ExceptionOccurs()
    {
        var folderId = "invalid-folder";
        _driveClient.DetectChangesAsync(folderId, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Drive API error"));

        var result = await _service.DetectDocumentChangesAsync(folderId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Drive API error");
    }

    [Fact]
    public async Task GetIngestionStatusAsync_Should_Return_Status_When_Successful()
    {
        var documentId = "doc-123";
        var expectedStatus = new IngestionStatus { State = "Processed" };

        _driveClient.GetStatusAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(expectedStatus);

        var result = await _service.GetIngestionStatusAsync(documentId);

        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.State.ShouldBe("Processed");
    }

    [Fact]
    public async Task GetIngestionStatusAsync_Should_Return_Failure_When_ExceptionOccurs()
    {
        var documentId = "error-doc";
        _driveClient.GetStatusAsync(documentId, Arg.Any<CancellationToken>())
            .Throws(new TimeoutException("Timeout during status retrieval"));

        var result = await _service.GetIngestionStatusAsync(documentId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Timeout during status retrieval");
    }
}