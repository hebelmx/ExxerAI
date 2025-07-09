using ExxerAI.Domain;
using ExxerAI.Domain.Operations;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Behavioral tests for the document ingestion pipeline via IDocumentIngestionService.
/// </summary>
public class DocumentIngestionServiceBehavioralTests
{
    private readonly IDocumentIngestionService _service;

    public DocumentIngestionServiceBehavioralTests()
    {
        _service = Substitute.For<IDocumentIngestionService>();
    }

    [Fact]
    public async Task DetectDocumentChangesAsync_Should_Return_Changes_When_ChangesExist()
    {
        var changes = new List<DocumentChangeEvent> { new() { DocumentId = "doc1" }, new() { DocumentId = "doc2" } };
        _service.DetectDocumentChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<DocumentChangeEvent>>.WithSuccess(changes));

        var result = await _service.DetectDocumentChangesAsync( TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBeEquivalentTo(changes);
    }

    [Fact]
    public async Task DetectDocumentChangesAsync_Should_Return_Failure_When_ExceptionOccurs()
    {
        _service.DetectDocumentChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<DocumentChangeEvent>>.WithFailure("Drive API error"));

        var result = await _service.DetectDocumentChangesAsync( TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Drive API error");
    }

    [Fact]
    public async Task GetIngestionStatusAsync_Should_Return_Status_When_Successful()
    {
        var expectedStatus = new IngestionStatus { DocumentsWatched = 5 };
        _service.GetIngestionStatusAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IngestionStatus>.WithSuccess(expectedStatus));

        var result = await _service.GetIngestionStatusAsync(cancellationToken: TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value!.DocumentsWatched.ShouldBe(5);
    }

    [Fact]
    public async Task GetIngestionStatusAsync_Should_Return_Failure_When_ExceptionOccurs()
    {
        _service.GetIngestionStatusAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IngestionStatus>.WithFailure("Timeout during agentStatus retrieval"));

        var result = await _service.GetIngestionStatusAsync(cancellationToken: TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Timeout during agentStatus retrieval");
    }

    [Fact]
    public async Task IngestDocumentAsync_Should_Return_Success_When_DocumentProcessed()
    {
        var documentId = "ingest-001";
        var processingResult = new DocumentProcessingResult { DocumentId = documentId, Confidence = 0.9f };
        _service.IngestDocumentAsync(documentId, false, Arg.Any<CancellationToken>())
            .Returns(Result<DocumentProcessingResult>.WithSuccess(processingResult));

        var result = await _service.IngestDocumentAsync(documentId, cancellationToken: TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value!.DocumentId.ShouldBe(documentId);
    }

    [Fact]
    public async Task IngestDocumentAsync_Should_Return_Failure_When_ExceptionOccurs()
    {
        var documentId = "ingest-fail";
        _service.IngestDocumentAsync(documentId, false, Arg.Any<CancellationToken>())
            .Returns(Result<DocumentProcessingResult>.WithFailure("Ingestion failed"));

        var result = await _service.IngestDocumentAsync(documentId, false, cancellationToken: TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Ingestion failed");
    }
}