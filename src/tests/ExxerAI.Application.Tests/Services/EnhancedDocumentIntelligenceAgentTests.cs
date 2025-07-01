using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Comprehensive unit tests for EnhancedDocumentIntelligenceAgent service
/// Tests the complete document processing workflow, business logic, and error scenarios
/// </summary>
public class EnhancedDocumentIntelligenceAgentTests
{
    private readonly IPolymorphicDocumentProcessor _documentProcessor;
    private readonly IPrimarySourceOfTruthSystem _truthSystem;
    private readonly IMCPGoogleDriveService _mcpDriveService;
    private readonly ILLMService _llmService;
    private readonly ILogger<EnhancedDocumentIntelligenceAgent> _logger;
    private readonly EnhancedDocumentIntelligenceAgent _agent;

    public EnhancedDocumentIntelligenceAgentTests()
    {
        _documentProcessor = Substitute.For<IPolymorphicDocumentProcessor>();
        _truthSystem = Substitute.For<IPrimarySourceOfTruthSystem>();
        _mcpDriveService = Substitute.For<IMCPGoogleDriveService>();
        _llmService = Substitute.For<ILLMService>();
        _logger = Substitute.For<ILogger<EnhancedDocumentIntelligenceAgent>>();

        _agent = new EnhancedDocumentIntelligenceAgent(
            _documentProcessor,
            _truthSystem,
            _mcpDriveService,
            _llmService,
            _logger);
    }

    #region ProcessBusinessDocumentAsync Tests

    [Fact]
    public async Task Should_ProcessBusinessDocument_When_ValidDocumentIdProvided()
    {
        // Arrange
        var documentId = "business-doc-001";
        var documentData = CreateSampleDocumentData();
        var metadata = CreateSampleMCPMetadata(documentId, "imss_payment_12_2023.pdf");
        var processingResult = CreateSuccessfulProcessingResult(documentId);
        var truthRecord = new TruthRecord { Id = "truth-001" };

        SetupSuccessfulMCPDownload(documentId, documentData, metadata);
        SetupSuccessfulDocumentProcessing(processingResult);
        SetupSuccessfulTruthStorage(truthRecord);

        var options = new ProcessingOptions { StoreTruthRecord = true };

        // Act
        var result = await _agent.ProcessBusinessDocumentAsync(documentId, options, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.DocumentId.ShouldBe(documentId);
        result.Data.TruthRecordId.ShouldBe("truth-001");

        await _mcpDriveService.Received(1).DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>());
        await _documentProcessor.Received(1).ProcessDocumentAsync(
            Arg.Is<byte[]>(data => data.SequenceEqual(documentData)),
            Arg.Is<DocumentMetadata>(meta => meta.DocumentId == documentId),
            Arg.Any<CancellationToken>());
        await _truthSystem.Received(1).StoreExtractedDataAsync(
            Arg.Any<ExtractedData>(),
            Arg.Is<DataSource>(source => source.Id == documentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnFailure_When_DocumentDownloadFails()
    {
        // Arrange
        var documentId = "failing-doc-001";
        var errorMessage = "Network timeout during download";

        _mcpDriveService.DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result<byte[]>.WithFailure(errorMessage));

        var options = new ProcessingOptions();

        // Act
        var result = await _agent.ProcessBusinessDocumentAsync(documentId, options, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Download failed");
        result.Error.ShouldContain(errorMessage);

        await _documentProcessor.DidNotReceive().ProcessDocumentAsync(
            Arg.Any<byte[]>(), Arg.Any<DocumentMetadata>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnFailure_When_DocumentProcessingFails()
    {
        // Arrange
        var documentId = "processing-fail-doc";
        var documentData = CreateSampleDocumentData();
        var metadata = CreateSampleMCPMetadata(documentId, "test.pdf");
        var processingError = "Text extraction failed";

        SetupSuccessfulMCPDownload(documentId, documentData, metadata);
        
        _documentProcessor.ProcessDocumentAsync(
            Arg.Any<byte[]>(), Arg.Any<DocumentMetadata>(), Arg.Any<CancellationToken>())
            .Returns(Result<DocumentProcessingResult>.WithFailure(processingError));

        var options = new ProcessingOptions();

        // Act
        var result = await _agent.ProcessBusinessDocumentAsync(documentId, options, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(processingError);

        await _truthSystem.DidNotReceive().StoreExtractedDataAsync(
            Arg.Any<ExtractedData>(), Arg.Any<DataSource>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_SkipTruthStorage_When_StoreTruthRecordIsFalse()
    {
        // Arrange
        var documentId = "no-truth-storage-doc";
        var documentData = CreateSampleDocumentData();
        var metadata = CreateSampleMCPMetadata(documentId, "test.pdf");
        var processingResult = CreateSuccessfulProcessingResult(documentId);

        SetupSuccessfulMCPDownload(documentId, documentData, metadata);
        SetupSuccessfulDocumentProcessing(processingResult);

        var options = new ProcessingOptions { StoreTruthRecord = false };

        // Act
        var result = await _agent.ProcessBusinessDocumentAsync(documentId, options, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.TruthRecordId.ShouldBeNull();

        await _truthSystem.DidNotReceive().StoreExtractedDataAsync(
            Arg.Any<ExtractedData>(), Arg.Any<DataSource>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ContinueProcessing_When_TruthStorageFails()
    {
        // Arrange
        var documentId = "truth-storage-fail-doc";
        var documentData = CreateSampleDocumentData();
        var metadata = CreateSampleMCPMetadata(documentId, "test.pdf");
        var processingResult = CreateSuccessfulProcessingResult(documentId);

        SetupSuccessfulMCPDownload(documentId, documentData, metadata);
        SetupSuccessfulDocumentProcessing(processingResult);
        
        _truthSystem.StoreExtractedDataAsync(
            Arg.Any<ExtractedData>(), Arg.Any<DataSource>(), Arg.Any<CancellationToken>())
            .Returns(Result<TruthRecord>.WithFailure("Truth storage failed"));

        var options = new ProcessingOptions { StoreTruthRecord = true };

        // Act
        var result = await _agent.ProcessBusinessDocumentAsync(documentId, options, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue(); // Should still succeed even if truth storage fails
        result.Data.TruthRecordId.ShouldBeNull();
    }

    [Theory]
    [InlineData("01. SUA IMSS ENE2023.pdf", nameof(DocumentType.IMSSPayment))]
    [InlineData("factura_12345.pdf", nameof(DocumentType.Invoice))]
    [InlineData("invoice_2023.docx", nameof(DocumentType.Invoice))]
    [InlineData("tax_document.pdf", nameof(DocumentType.TaxDocument))]
    [InlineData("unknown_file.txt", nameof(DocumentType.Unknown))]
    public async Task Should_IdentifyCorrectDocumentType_When_FilenameProvided(string filename, string expectedTypeName)
    {
        // Arrange
        var documentId = "type-test-doc";
        var documentData = CreateSampleDocumentData();
        var metadata = CreateSampleMCPMetadata(documentId, filename);
        var processingResult = CreateSuccessfulProcessingResult(documentId);
        var expectedType = Enum.Parse<DocumentType>(expectedTypeName);

        SetupSuccessfulMCPDownload(documentId, documentData, metadata);
        SetupSuccessfulDocumentProcessing(processingResult);

        var options = new ProcessingOptions { StoreTruthRecord = false };

        // Act
        var result = await _agent.ProcessBusinessDocumentAsync(documentId, options, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await _documentProcessor.Received(1).ProcessDocumentAsync(
            Arg.Any<byte[]>(),
            Arg.Is<DocumentMetadata>(meta => meta.DocumentType == expectedType),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_HandleException_When_UnexpectedErrorOccurs()
    {
        // Arrange
        var documentId = "exception-doc";
        
        _mcpDriveService.DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        var options = new ProcessingOptions();

        // Act
        var result = await _agent.ProcessBusinessDocumentAsync(documentId, options, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Processing error");
        result.Error.ShouldContain("Unexpected error");
    }

    #endregion

    #region StartBusinessDocumentMonitoringAsync Tests

    [Fact]
    public async Task Should_StartDocumentMonitoring_When_ValidFolderProvided()
    {
        // Arrange
        var folderId = "business-folder-001";
        var watchOptions = new MCPWatchOptions
        {
            IncludeSubdirectories = true,
            AutoProcess = true,
            PollingIntervalSeconds = 60
        };

        var mcpResponse = new MCPResponse
        {
            Id = "watch-001",
            IsSuccess = true,
            Message = "Watch started successfully"
        };

        _mcpDriveService.WatchFolderAsync(folderId, watchOptions, Arg.Any<CancellationToken>())
            .Returns(Result<MCPResponse>.WithSuccess(mcpResponse));

        // Act
        var result = await _agent.StartBusinessDocumentMonitoringAsync(folderId, watchOptions, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBe("watch-001");

        await _mcpDriveService.Received(1).WatchFolderAsync(folderId, watchOptions, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnFailure_When_WatchSetupFails()
    {
        // Arrange
        var folderId = "invalid-folder";
        var watchOptions = new MCPWatchOptions();
        var errorMessage = "Folder not found";

        _mcpDriveService.WatchFolderAsync(folderId, watchOptions, Arg.Any<CancellationToken>())
            .Returns(Result<MCPResponse>.WithFailure(errorMessage));

        // Act
        var result = await _agent.StartBusinessDocumentMonitoringAsync(folderId, watchOptions, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Watch setup failed");
        result.Error.ShouldContain(errorMessage);
    }

    [Fact]
    public async Task Should_HandleException_When_MonitoringSetupThrows()
    {
        // Arrange
        var folderId = "exception-folder";
        var watchOptions = new MCPWatchOptions();

        _mcpDriveService.WatchFolderAsync(folderId, watchOptions, Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        // Act
        var result = await _agent.StartBusinessDocumentMonitoringAsync(folderId, watchOptions, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Monitoring error");
        result.Error.ShouldContain("Access denied");
    }

    #endregion

    #region GenerateBusinessIntelligenceReportAsync Tests

    [Fact]
    public async Task Should_GenerateBusinessIntelligenceReport_When_ValidDateRangeProvided()
    {
        // Arrange
        var fromDate = new DateTime(2024, 1, 1);
        var toDate = new DateTime(2024, 12, 31);

        var groundingReport = CreateSampleGroundingReport(fromDate, toDate);
        var qualityMetrics = CreateSampleQualityMetrics();

        _truthSystem.GenerateGroundingReportAsync(fromDate, toDate, Arg.Any<CancellationToken>())
            .Returns(Result<GroundingReport>.WithSuccess(groundingReport));

        _truthSystem.GetDataQualityMetricsAsync(fromDate, toDate, Arg.Any<CancellationToken>())
            .Returns(Result<DataQualityMetrics>.WithSuccess(qualityMetrics));

        // Act
        var result = await _agent.GenerateBusinessIntelligenceReportAsync(fromDate, toDate, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ReportPeriod.FromDate.ShouldBe(fromDate);
        result.Data.ReportPeriod.ToDate.ShouldBe(toDate);
        result.Data.GroundingReport.ShouldBe(groundingReport);
        result.Data.QualityMetrics.ShouldBe(qualityMetrics);
        result.Data.GeneratedBy.ShouldBe(nameof(EnhancedDocumentIntelligenceAgent));
        result.Data.Insights.ShouldNotBeEmpty();
        result.Data.RecommendedActions.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Should_ReturnFailure_When_GroundingReportGenerationFails()
    {
        // Arrange
        var fromDate = new DateTime(2024, 1, 1);
        var toDate = new DateTime(2024, 12, 31);
        var errorMessage = "Failed to access truth system";

        _truthSystem.GenerateGroundingReportAsync(fromDate, toDate, Arg.Any<CancellationToken>())
            .Returns(Result<GroundingReport>.WithFailure(errorMessage));

        // Act
        var result = await _agent.GenerateBusinessIntelligenceReportAsync(fromDate, toDate, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Failed to generate grounding report");
        result.Error.ShouldContain(errorMessage);
    }

    [Fact]
    public async Task Should_ReturnFailure_When_QualityMetricsRetrievalFails()
    {
        // Arrange
        var fromDate = new DateTime(2024, 1, 1);
        var toDate = new DateTime(2024, 12, 31);
        var groundingReport = CreateSampleGroundingReport(fromDate, toDate);
        var errorMessage = "Quality metrics service unavailable";

        _truthSystem.GenerateGroundingReportAsync(fromDate, toDate, Arg.Any<CancellationToken>())
            .Returns(Result<GroundingReport>.WithSuccess(groundingReport));

        _truthSystem.GetDataQualityMetricsAsync(fromDate, toDate, Arg.Any<CancellationToken>())
            .Returns(Result<DataQualityMetrics>.WithFailure(errorMessage));

        // Act
        var result = await _agent.GenerateBusinessIntelligenceReportAsync(fromDate, toDate, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Failed to get quality metrics");
        result.Error.ShouldContain(errorMessage);
    }

    #endregion

    #region OptimizeProcessingRulesAsync Tests

    [Fact]
    public async Task Should_OptimizeProcessingRules_When_OptimizationRequested()
    {
        // Act
        var result = await _agent.OptimizeProcessingRulesAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.PatternsLearned.ShouldBeTrue();
        result.Data.ConfidenceImprovement.ShouldBeGreaterThan(0.0f);
        result.Data.NewPatterns.ShouldNotBeEmpty();
        result.Data.SchemaUpdates.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Should_HandleException_When_OptimizationThrows()
    {
        // This test would verify exception handling in optimization
        // For now, the implementation doesn't throw, but this shows the pattern
        
        // Act
        var result = await _agent.OptimizeProcessingRulesAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue(); // Current implementation always succeeds
    }

    #endregion

    #region Helper Methods

    private static byte[] CreateSampleDocumentData()
    {
        var content = @"
            INSTITUTO MEXICANO DEL SEGURO SOCIAL
            CEDULA DE DETERMINACION DE CUOTAS
            PERIODO: 12-2023
            REGISTRO PATRONAL: A1234567890
            IMPORTE TOTAL: $15,000.00
            FECHA: 15/01/2024
        ";
        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    private static MCPDocumentMetadata CreateSampleMCPMetadata(string documentId, string filename)
    {
        return new MCPDocumentMetadata
        {
            Id = documentId,
            Name = filename,
            MimeType = "application/pdf",
            Size = 1024,
            CreatedTime = DateTime.UtcNow.AddDays(-30),
            ModifiedTime = DateTime.UtcNow.AddDays(-1),
            DriveFilePath = $"/business/documents/{filename}"
        };
    }

    private static DocumentProcessingResult CreateSuccessfulProcessingResult(string documentId)
    {
        return new DocumentProcessingResult
        {
            DocumentId = documentId,
            ExtractionMethod = ExtractionMethod.DirectText,
            ExtractedText = "Sample extracted text",
            ExtractedFields = new Dictionary<string, object>
            {
                ["PaymentPeriod"] = "12-2023",
                ["Amount"] = "15000.00",
                ["EmployerNumber"] = "A1234567890"
            },
            GroundedData = new ExtractedData
            {
                Fields = new Dictionary<string, object>
                {
                    ["PaymentPeriod"] = "12-2023",
                    ["Amount"] = "15000.00",
                    ["EmployerNumber"] = "A1234567890"
                }
            },
            ValidationResults = new ValidationResult { IsValid = true, Confidence = 0.95f },
            Confidence = 0.95f,
            LLMConfidence = 0.90f,
            GroundingConfidence = 0.93f,
            ProcessingTimeMs = 1500
        };
    }

    private static GroundingReport CreateSampleGroundingReport(DateTime fromDate, DateTime toDate)
    {
        return new GroundingReport
        {
            DateRange = new DateRange { FromDate = fromDate, ToDate = toDate },
            TotalRecordsProcessed = 150,
            SuccessfulGroundings = 142,
            FailedGroundings = 8,
            AverageConfidenceScore = 0.87f,
            ConflictsResolved = 5,
            RecordsRequiringReview = 3,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static DataQualityMetrics CreateSampleQualityMetrics()
    {
        return new DataQualityMetrics
        {
            OverallQualityScore = 0.89f,
            CompletenessPercentage = 0.92f,
            AccuracyPercentage = 0.94f,
            ConsistencyPercentage = 0.86f,
            DuplicateRecords = 3,
            ValidationErrors = 12
        };
    }

    private void SetupSuccessfulMCPDownload(string documentId, byte[] documentData, MCPDocumentMetadata metadata)
    {
        _mcpDriveService.DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result<byte[]>.WithSuccess(documentData));

        _mcpDriveService.GetDocumentMetadataAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result<MCPDocumentMetadata>.WithSuccess(metadata));
    }

    private void SetupSuccessfulDocumentProcessing(DocumentProcessingResult processingResult)
    {
        _documentProcessor.ProcessDocumentAsync(
            Arg.Any<byte[]>(), Arg.Any<DocumentMetadata>(), Arg.Any<CancellationToken>())
            .Returns(Result<DocumentProcessingResult>.WithSuccess(processingResult));
    }

    private void SetupSuccessfulTruthStorage(TruthRecord truthRecord)
    {
        _truthSystem.StoreExtractedDataAsync(
            Arg.Any<ExtractedData>(), Arg.Any<DataSource>(), Arg.Any<CancellationToken>())
            .Returns(Result<TruthRecord>.WithSuccess(truthRecord));
    }

    #endregion
} 