using Microsoft.Extensions.Logging;

namespace ExxerAI.Application.Tests.Services;

public class GoogleDriveServiceBehavioralTests24
{
    private readonly IGoogleDriveClient _driveClient;
    private readonly ILogger<GoogleDriveService> _logger;
    private readonly GoogleDriveService _service;

    public GoogleDriveServiceBehavioralTests2()
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
}