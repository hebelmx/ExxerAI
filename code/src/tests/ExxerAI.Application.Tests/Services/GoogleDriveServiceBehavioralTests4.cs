using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Logging;
using DateRange = ExxerAI.Application.Interfaces.DateRange;
using KeywordPattern = ExxerAI.Application.Interfaces.KeywordPattern;
using RegexPattern = ExxerAI.Application.Interfaces.RegexPattern;

namespace ExxerAI.Application.Tests.Services;

public class GoogleDriveServiceBehavioralTests4
{
    private readonly IGoogleDriveClient _driveClient;
    private readonly ILogger<GoogleDriveService> _logger;
    private readonly GoogleDriveService _service;

    public GoogleDriveServiceBehavioralTests4()
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

    [Fact]
    public async Task IngestDocumentAsync_Should_Return_Success_When_DocumentProcessed()
    {
        var documentId = "ingest-001";
        _driveClient.IngestAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _service.IngestDocumentAsync(documentId);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task IngestDocumentAsync_Should_Return_Failure_When_ExceptionOccurs()
    {
        var documentId = "ingest-fail";
        _driveClient.IngestAsync(documentId, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Ingestion failed"));

        var result = await _service.IngestDocumentAsync(documentId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Ingestion failed");
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
    public async Task AdaptProcessingRulesAsync_Should_Return_Success_When_History_Is_Valid()
    {
        var history = new ProcessingHistory
        {
            ProcessingResults = new List<DocumentProcessingResult>
            {
                new DocumentProcessingResult { DocumentId = "invoice-001", Confidence = 0.85f },
                new DocumentProcessingResult { DocumentId = "invoice-002", Confidence = 0.90f }
            }
        };

        var processor = new PolymorphicDocumentProcessor(Substitute.For<ILLMService>(), Substitute.For<ILogger<PolymorphicDocumentProcessor>>());
        var result = await processor.AdaptProcessingRulesAsync(history);

        result.IsSuccess.ShouldBeTrue();
        result.Value.PatternsLearned.ShouldBeTrue();
        result.Value.SchemaUpdates.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task AdaptProcessingRulesAsync_Should_Handle_Empty_History_Gracefully()
    {
        var emptyHistory = new ProcessingHistory
        {
            ProcessingResults = new List<DocumentProcessingResult>()
        };

        var processor = new PolymorphicDocumentProcessor(Substitute.For<ILLMService>(), Substitute.For<ILogger<PolymorphicDocumentProcessor>>());
        var result = await processor.AdaptProcessingRulesAsync(emptyHistory);

        result.IsSuccess.ShouldBeTrue();
        result.Value.PatternsLearned.ShouldBeFalse();
    }

    [Fact]
    public async Task AdaptProcessingRulesAsync_Should_Return_Failure_On_Exception()
    {
        var mockProcessor = Substitute.ForPartsOf<PolymorphicDocumentProcessor>(Substitute.For<ILLMService>(), Substitute.For<ILogger<PolymorphicDocumentProcessor>>());

        var faultyHistory = new ProcessingHistory
        {
            ProcessingResults = null!
        };

        var result = await mockProcessor.AdaptProcessingRulesAsync(faultyHistory);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Rule adaptation error");
    }

    [Fact]
    public async Task GetProcessingConfidenceAsync_Should_Return_Confidence_When_DocumentExists()
    {
        var documentId = "doc-789";
        var expectedConfidence = 0.82f;

        var llmService = Substitute.For<ILLMService>();
        llmService.GetConfidenceScoreAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(expectedConfidence);

        var processor = new PolymorphicDocumentProcessor(llmService, Substitute.For<ILogger<PolymorphicDocumentProcessor>>());
        var result = await processor.GetProcessingConfidenceAsync(documentId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedConfidence);
    }

    [Fact]
    public async Task GetProcessingConfidenceAsync_Should_Return_Failure_When_ExceptionThrown()
    {
        var documentId = "doc-fail";
        var llmService = Substitute.For<ILLMService>();
        llmService.GetConfidenceScoreAsync(documentId, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Score fetch failure"));

        var processor = new PolymorphicDocumentProcessor(llmService, Substitute.For<ILogger<PolymorphicDocumentProcessor>>());
        var result = await processor.GetProcessingConfidenceAsync(documentId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Score fetch failure");
    }

    [Fact]
    public async Task RegexPattern_ExtractAsync_Should_Return_Match_When_Regex_Finds_Value()
    {
        var pattern = new RegexPattern
        {
            Expression = "Invoice Number:\s*(\d+)",
            CaptureGroup = 1
        };

        var result = await pattern.ExtractAsync("Invoice Number: 123456");

        result.ShouldBe("123456");
    }

    [Fact]
    public async Task RegexPattern_ExtractAsync_Should_Return_Null_When_No_Match()
    {
        var pattern = new RegexPattern
        {
            Expression = "Invoice Number:\s*(\d+)",
            CaptureGroup = 1
        };

        var result = await pattern.ExtractAsync("No invoice info here");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task LLMPattern_ExtractAsync_Should_Return_Result_When_Successful()
    {
        var llm = new LLMPattern
        {
            Prompt = "What is the invoice number?"
        };

        var sampleText = "Invoice Number: 987654";
        var expected = "987654";

        var mockLLMService = Substitute.For<ILLMService>();
        mockLLMService.ExtractUsingPromptAsync(sampleText, llm.Prompt, Arg.Any<CancellationToken>())
            .Returns(expected);

        llm.LLMService = mockLLMService;
        var result = await llm.ExtractAsync(sampleText);

        result.ShouldBe(expected);
    }

    [Fact]
    public async Task LLMPattern_ExtractAsync_Should_Return_Null_When_No_Result()
    {
        var llm = new LLMPattern
        {
            Prompt = "What is the invoice number?"
        };

        var sampleText = "No number present";
        var mockLLMService = Substitute.For<ILLMService>();
        mockLLMService.ExtractUsingPromptAsync(sampleText, llm.Prompt, Arg.Any<CancellationToken>())
            .Returns((string)null);

        llm.LLMService = mockLLMService;
        var result = await llm.ExtractAsync(sampleText);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task LLMPattern_ExtractAsync_Should_Handle_Exception_Gracefully()
    {
        var llm = new LLMPattern
        {
            Prompt = "Extract the value",
            Metadata = new Dictionary<string, string>()
        };

        var text = "corrupted text";
        var mockLLMService = Substitute.For<ILLMService>();
        mockLLMService.ExtractUsingPromptAsync(text, llm.Prompt, Arg.Any<CancellationToken>())
            .Throws(new Exception("LLM extraction failed"));

        llm.LLMService = mockLLMService;
        var result = await llm.ExtractAsync(text);

        result.ShouldBeNull();
        llm.Metadata.ShouldContainKey("LastError");
    }

    [Fact]
    public async Task RegexPattern_ExtractAsync_Should_Handle_Invalid_Regex_Gracefully()
    {
        var pattern = new RegexPattern
        {
            Expression = "[Invalid",
            CaptureGroup = 1
        };

        var result = await pattern.ExtractAsync("Sample text");

        result.ShouldBeNull();
        pattern.Metadata.ShouldContainKey("LastError");
    }

    [Fact]
    public async Task OCRRegionPattern_ExtractAsync_Should_Return_Text_When_RegionFound()
    {
        var pattern = new OCRRegionPattern
        {
            RegionId = "RegionA"
        };

        var document = new OCRDocument
        {
            Regions = new Dictionary<string, string>
            {
                { "RegionA", "Extracted Text" }
            }
        };

        var result = await pattern.ExtractAsync(document);

        result.ShouldBe("Extracted Text");
    }

    [Fact]
    public async Task OCRRegionPattern_ExtractAsync_Should_Return_Null_When_RegionNotFound()
    {
        var pattern = new OCRRegionPattern
        {
            RegionId = "MissingRegion"
        };

        var document = new OCRDocument
        {
            Regions = new Dictionary<string, string>()
        };

        var result = await pattern.ExtractAsync(document);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task OCRRegionPattern_ExtractAsync_Should_Handle_Exception_Gracefully()
    {
        var pattern = new OCRRegionPattern
        {
            RegionId = null!
        };

        var document = new OCRDocument();

        var result = await pattern.ExtractAsync(document);

        result.ShouldBeNull();
        pattern.Metadata.ShouldContainKey("LastError");
    }

    [Fact]
    public async Task KeywordPattern_ExtractAsync_Should_Return_Match_When_KeywordFound()
    {
        var pattern = new KeywordPattern
        {
            Keyword = "Total",
            Offset = 1
        };

        var words = new List<string> { "Subtotal", "Total", "$100" };
        var result = await pattern.ExtractAsync(words);

        result.ShouldBe("$100");
    }

    [Fact]
    public async Task KeywordPattern_ExtractAsync_Should_Return_Null_When_KeywordNotFound()
    {
        var pattern = new KeywordPattern
        {
            Keyword = "Amount",
            Offset = 1
        };

        var words = new List<string> { "Subtotal", "Total", "$100" };
        var result = await pattern.ExtractAsync(words);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task KeywordPattern_ExtractAsync_Should_Handle_Exception_Gracefully()
    {
        var pattern = new KeywordPattern
        {
            Keyword = null!,
            Offset = 1
        };

        var words = new List<string> { "irrelevant" };
        var result = await pattern.ExtractAsync(words);

        result.ShouldBeNull();
        pattern.Metadata.ShouldContainKey("LastError");
    }

    [Fact]
    public void DateRange_Should_Set_And_Get_Values_Correctly()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);

        var range = new DateRange(start, end);

        range.Start.ShouldBe(start);
        range.End.ShouldBe(end);
    }

    [Fact]
    public void DateRange_Should_Calculate_Duration()
    {
        var range = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 11));

        range.DurationInDays.ShouldBe(10);
    }

    [Fact]
    public void DateRange_Should_Throw_When_StartDate_After_EndDate()
    {
        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2024, 1, 1);

        Should.Throw<ArgumentException>(() => new DateRange(start, end));
    }

    [Fact]
    public async Task IsDocumentModifiedAsync_Should_Return_True_When_Modified()
    {
        var documentId = "doc-modified";
        _driveClient.IsModifiedAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _service.IsDocumentModifiedAsync(documentId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task IsDocumentModifiedAsync_Should_Return_False_When_NotModified()
    {
        var documentId = "doc-unmodified";
        _driveClient.IsModifiedAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _service.IsDocumentModifiedAsync(documentId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeFalse();
    }

    [Fact]
    public async Task IsDocumentModifiedAsync_Should_Return_Failure_When_ExceptionThrown()
    {
        var documentId = "doc-error";
        _driveClient.IsModifiedAsync(documentId, Arg.Any<CancellationToken>())
            .Throws(new Exception("Unexpected error"));

        var result = await _service.IsDocumentModifiedAsync(documentId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Unexpected error");
    }

    [Fact]
    public async Task StartWatchingFolderAsync_Should_Return_Success_When_StartSucceeds()
    {
        var folderId = "folder-watch";
        _driveClient.StartWatchingAsync(folderId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _service.StartWatchingFolderAsync(folderId);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task StartWatchingFolderAsync_Should_Return_Failure_When_ExceptionThrown()
    {
        var folderId = "folder-failure";
        _driveClient.StartWatchingAsync(folderId, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Watch failed"));

        var result = await _service.StartWatchingFolderAsync(folderId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Watch failed");
    }

    [Fact]
    public async Task StopWatchingFolderAsync_Should_Return_Success_When_StopSucceeds()
    {
        var folderId = "folder-stop";
        _driveClient.StopWatchingAsync(folderId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _service.StopWatchingFolderAsync(folderId);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task StopWatchingFolderAsync_Should_Return_Failure_When_ExceptionThrown()
    {
        var folderId = "folder-stop-failure";
        _driveClient.StopWatchingAsync(folderId, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Stop failed"));

        var result = await _service.StopWatchingFolderAsync(folderId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Stop failed");
    }
}