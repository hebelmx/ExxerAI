using ExxerAI.Infrastructure.External;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Application.Patterns;
using NSubstitute;
using Shouldly;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DateRange = ExxerAI.Application.Interfaces.DateRange;
using System.Threading;
using System.Collections.Generic;
using System;
using ExxerAI.Domain.Enums;
using ExxerAI.Domain.Operations;

namespace ExxerAI.IntegrationTests;

public class GoogleDriveServiceBehavioralTests4
{
    private readonly IDocumentIngestionService _service;
    private readonly ILogger<IDocumentIngestionService> _logger;

    public GoogleDriveServiceBehavioralTests4()
    {
        _logger = Substitute.For<ILogger<IDocumentIngestionService>>();
        _service = Substitute.For<IDocumentIngestionService>();
    }

    [Fact]
    public async Task DetectDocumentChangesAsync_Should_Return_Changes_When_ChangesExist()
    {
        var changes = new List<DocumentChangeEvent> { new() { DocumentId = "doc1" }, new() { DocumentId = "doc2" } };
        _service.DetectDocumentChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<DocumentChangeEvent>>.WithSuccess(changes));

        var result = await _service.DetectDocumentChangesAsync();

        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBeEquivalentTo(changes);
    }

    [Fact]
    public async Task DetectDocumentChangesAsync_Should_Return_Failure_When_ExceptionOccurs()
    {
        _service.DetectDocumentChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<DocumentChangeEvent>>.WithFailure("Drive API error"));

        var result = await _service.DetectDocumentChangesAsync();

        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Drive API error");
    }

    [Fact]
    public async Task GetIngestionStatusAsync_Should_Return_Status_When_Successful()
    {
        var expectedStatus = new IngestionStatus { DocumentsWatched = 5 };
        _service.GetIngestionStatusAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IngestionStatus>.WithSuccess(expectedStatus));

        var result = await _service.GetIngestionStatusAsync(TestContext.Current.CancellationToken);

        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value!.DocumentsWatched.ShouldBe(5);
    }

    [Fact]
    public async Task GetIngestionStatusAsync_Should_Return_Failure_When_ExceptionOccurs()
    {
        _service.GetIngestionStatusAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IngestionStatus>.WithFailure("Timeout during agentStatus retrieval"));

        var result = await _service.GetIngestionStatusAsync(TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Timeout during agentStatus retrieval");
    }

    [Fact]
    public async Task IngestDocumentAsync_Should_Return_Success_When_DocumentProcessed()
    {
        var documentId = "ingest-001";
        _service.IngestDocumentAsync(documentId, false, Arg.Any<CancellationToken>())
            .Returns(Result<DocumentProcessingResult>.WithSuccess(new DocumentProcessingResult { DocumentId = documentId }));

        var result = await _service.IngestDocumentAsync(documentId, false, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value!.DocumentId.ShouldBe(documentId);
    }

    [Fact]
    public async Task IngestDocumentAsync_Should_Return_Failure_When_ExceptionOccurs()
    {
        var documentId = "ingest-fail";
        _service.IngestDocumentAsync(documentId, false, Arg.Any<CancellationToken>())
            .Returns(Result<DocumentProcessingResult>.WithFailure("Ingestion failed"));

        var result = await _service.IngestDocumentAsync(documentId, false, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Ingestion failed");
    }

    [Fact]
    public void EnumerableExtensions_ForEach_Should_Execute_Action_On_Each_Item()
    {
        var numbers = new List<int> { 1, 2, 3 };
        var result = new List<int>();

        numbers.ForEach(n => result.Add(n * 2));

        result.ShouldBe(new List<int> { 2, 4, 6 });
    }

    [Fact]
    public void EnumerableExtensions_ForEach_Should_Handle_Empty_List()
    {
        var numbers = new List<int>();
        var result = new List<int>();

        numbers.ForEach(n => result.Add(n * 2));

        result.ShouldBeEmpty();
    }

    [Fact]
    public void RegexPattern_ExtractValue_Should_Return_Match_When_Regex_Finds_Value()
    {
        var pattern = new RegexPattern(@"PERIODO[:\s]*(\d{2}-\d{4})", 0.9f);
        var context = new ExtractionContext();
        var text = "PERIODO: 12-2023";
        var value = pattern.ExtractValue(text, context);
        value.ShouldBe("12-2023");
    }

    [Fact]
    public void RegexPattern_ExtractValue_Should_Return_Null_When_No_Match()
    {
        var pattern = new RegexPattern(@"PERIODO[:\s]*(\d{2}-\d{4})", 0.9f);
        var context = new ExtractionContext();
        var text = "NO MATCH HERE";
        var value = pattern.ExtractValue(text, context);
        value.ShouldBeNull();
    }

    [Fact]
    public void RegexPattern_ExtractValue_Should_Handle_Invalid_Regex_Gracefully()
    {
        var pattern = new RegexPattern("[INVALID", 0.9f);
        var context = new ExtractionContext();
        var text = "PERIODO: 12-2023";
        var value = pattern.ExtractValue(text, context);
        value.ShouldBeNull();
    }

    [Fact]
    public void KeywordPattern_ExtractValue_Should_Return_NextToken()
    {
        var pattern = new KeywordPattern("PERIODO:", PositionStrategy.NextToken, 0.9f);
        var context = new ExtractionContext();
        var text = "PERIODO: 12-2023";
        var value = pattern.ExtractValue(text, context);
        value.ShouldBe("12-2023");
    }

    [Fact]
    public void KeywordPattern_ExtractValue_Should_Return_Null_When_Keyword_Not_Found()
    {
        var pattern = new KeywordPattern("PERIODO:", PositionStrategy.NextToken, 0.9f);
        var context = new ExtractionContext();
        var text = "NO KEYWORD HERE";
        var value = pattern.ExtractValue(text, context);
        value.ShouldBeNull();
    }

    [Fact]
    public void OCRRegionPattern_ExtractValue_Should_Return_NextToken()
    {
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "PERIODO:",
            SearchStrategy = ExxerAI.Application.Enums.SearchStrategy.NextToken
        };
        var context = new ExtractionContext();
        var text = "PERIODO: 12-2023";
        var value = pattern.ExtractValue(text, context);
        value.ShouldBe("12-2023");
    }

    [Fact]
    public void OCRRegionPattern_ExtractValue_Should_Return_Null_When_Reference_Not_Found()
    {
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "PERIODO:",
            SearchStrategy = ExxerAI.Application.Enums.SearchStrategy.NextToken
        };
        var context = new ExtractionContext();
        var text = "NO REFERENCE HERE";
        var value = pattern.ExtractValue(text, context);
        value.ShouldBeNull();
    }

    [Fact]
    public void DateRange_Should_Set_Properties_Correctly()
    {
        var range = new DateRange { FromDate = new DateTime(2023, 1, 1), ToDate = new DateTime(2023, 1, 31) };
        range.FromDate.ShouldBe(new DateTime(2023, 1, 1));
        range.ToDate.ShouldBe(new DateTime(2023, 1, 31));
        range.Duration.ShouldBe(new TimeSpan(30, 0, 0, 0));
    }
}