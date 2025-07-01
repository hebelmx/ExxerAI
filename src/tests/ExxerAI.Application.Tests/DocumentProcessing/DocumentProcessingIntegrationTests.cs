using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.DocumentProcessing;

/// <summary>
/// Comprehensive integration tests for the Enhanced Document Intelligence Pipeline
/// Tests the complete end-to-end workflow: Document Input → Processing → Truth Storage
/// Validates all major components working together
/// </summary>
public class DocumentProcessingIntegrationTests
{
    private readonly IPolymorphicDocumentProcessor _documentProcessor;
    private readonly IPrimarySourceOfTruthSystem _truthSystem;
    private readonly IMCPGoogleDriveService _mcpDriveService;
    private readonly ILLMService _llmService;
    private readonly ILogger<EnhancedDocumentIntelligenceAgent> _logger;
    private readonly EnhancedDocumentIntelligenceAgent _agent;

    public DocumentProcessingIntegrationTests()
    {
        // Setup mocks for dependencies
        _documentProcessor = Substitute.For<IPolymorphicDocumentProcessor>();
        _truthSystem = Substitute.For<IPrimarySourceOfTruthSystem>();
        _mcpDriveService = Substitute.For<IMCPGoogleDriveService>();
        _llmService = Substitute.For<ILLMService>();
        _logger = Substitute.For<ILogger<EnhancedDocumentIntelligenceAgent>>();

        // Create agent under test
        _agent = new EnhancedDocumentIntelligenceAgent(
            _documentProcessor,
            _truthSystem,
            _mcpDriveService,
            _llmService,
            _logger);
    }

    [Fact]
    public async Task Should_ProcessBusinessDocument_When_ValidDocumentProvided()
    {
        // Arrange
        var documentId = "test-imss-document-001";
        var documentData = CreateSampleIMSSDocument();
        var expectedResult = CreateSuccessfulProcessingResult(documentId);

        // Setup MCP service to return document data
        _mcpDriveService.DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result<byte[]>.WithSuccess(documentData));

        _mcpDriveService.GetDocumentMetadataAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result<MCPDocumentMetadata>.WithSuccess(CreateSampleMetadata(documentId)));

        // Setup document processor to return successful processing
        _documentProcessor.ProcessDocumentAsync(
            Arg.Any<byte[]>(), 
            Arg.Any<DocumentMetadata>(), 
            Arg.Any<CancellationToken>())
            .Returns(Result<DocumentProcessingResult>.WithSuccess(expectedResult));

        // Setup truth system to accept storage
        _truthSystem.StoreExtractedDataAsync(
            Arg.Any<ExtractedData>(), 
            Arg.Any<DataSource>(), 
            Arg.Any<CancellationToken>())
            .Returns(Result<TruthRecord>.WithSuccess(new TruthRecord { Id = "truth-001" }));

        var processingOptions = new ProcessingOptions
        {
            UseOCRFallback = true,
            UseLLMExtraction = true,
            EnableSchemaLearning = true,
            StoreTruthRecord = true
        };

        // Act
        var result = await _agent.ProcessBusinessDocumentAsync(documentId, processingOptions, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.DocumentId.ShouldBe(documentId);
        result.Data.IsSuccessful.ShouldBeTrue();
        result.Data.TruthRecordId.ShouldBe("truth-001");

        // Verify all services were called in correct sequence
        await _mcpDriveService.Received(1).DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>());
        await _mcpDriveService.Received(1).GetDocumentMetadataAsync(documentId, Arg.Any<CancellationToken>());
        await _documentProcessor.Received(1).ProcessDocumentAsync(
            Arg.Any<byte[]>(), 
            Arg.Any<DocumentMetadata>(), 
            Arg.Any<CancellationToken>());
        await _truthSystem.Received(1).StoreExtractedDataAsync(
            Arg.Any<ExtractedData>(), 
            Arg.Any<DataSource>(), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_StartBusinessDocumentMonitoring_When_ValidFolderProvided()
    {
        // Arrange
        var folderId = "test-folder-001";
        var watchOptions = new MCPWatchOptions
        {
            IncludeSubdirectories = true,
            FileTypes = new List<string> { ".pdf", ".docx" },
            AutoProcess = true,
            PollingIntervalSeconds = 30
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

        // Verify watch was set up
        await _mcpDriveService.Received(1).WatchFolderAsync(folderId, watchOptions, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(nameof(DocumentType.IMSSPayment), "01. SUA IMSS ENE2023.pdf")]
    [InlineData(nameof(DocumentType.Invoice), "Factura_12345.pdf")]
    [InlineData(nameof(DocumentType.TaxDocument), "RFC_Impuestos_2023.pdf")]
    public async Task Should_IdentifyDocumentType_When_FilenameProvided(string expectedTypeName, string filename)
    {
        // Arrange
        var expectedType = Enum.Parse<DocumentType>(expectedTypeName);
        var documentData = CreateSampleDocument(expectedType);
        var metadata = new MCPDocumentMetadata
        {
            Id = "test-doc",
            Name = filename,
            MimeType = "application/pdf",
            Size = documentData.Length
        };

        var processingResult = new DocumentProcessingResult
        {
            DocumentId = "test-doc",
            IsSuccessful = true,
            OverallConfidence = 0.95f
        };

        _mcpDriveService.DownloadDocumentAsync("test-doc", Arg.Any<CancellationToken>())
            .Returns(Result<byte[]>.WithSuccess(documentData));

        _mcpDriveService.GetDocumentMetadataAsync("test-doc", Arg.Any<CancellationToken>())
            .Returns(Result<MCPDocumentMetadata>.WithSuccess(metadata));

        _documentProcessor.ProcessDocumentAsync(
            Arg.Any<byte[]>(), 
            Arg.Is<DocumentMetadata>(m => m.DocumentType == expectedType), 
            Arg.Any<CancellationToken>())
            .Returns(Result<DocumentProcessingResult>.WithSuccess(processingResult));

        // Act
        var result = await _agent.ProcessBusinessDocumentAsync("test-doc", new ProcessingOptions(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        
        // Verify the correct document type was passed to the processor
        await _documentProcessor.Received(1).ProcessDocumentAsync(
            Arg.Any<byte[]>(),
            Arg.Is<DocumentMetadata>(m => m.DocumentType == expectedType),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_GenerateBusinessIntelligenceReport_When_ValidDateRangeProvided()
    {
        // Arrange
        var fromDate = new DateTime(2024, 1, 1);
        var toDate = new DateTime(2024, 12, 31);

        var groundingReport = new GroundingReport
        {
            TotalRecordsProcessed = 150,
            SuccessfulGroundings = 142,
            FailedGroundings = 8,
            AverageConfidenceScore = 0.87f,
            DateRange = new DateRange { FromDate = fromDate, ToDate = toDate }
        };

        var qualityMetrics = new DataQualityMetrics
        {
            OverallQualityScore = 0.89f,
            CompletenessPercentage = 0.92f,
            AccuracyPercentage = 0.94f,
            ConsistencyPercentage = 0.86f,
            DuplicateRecords = 3,
            ValidationErrors = 12
        };

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
        result.Data.GroundingReport.SuccessRate.ShouldBe(142f / 150f, 0.01f);

        // Verify insights were generated
        result.Data.Insights.ShouldNotBeEmpty();
        result.Data.RecommendedActions.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Should_HandleMCPServiceFailure_When_DownloadFails()
    {
        // Arrange
        var documentId = "failing-document";
        var errorMessage = "Network timeout during document download";

        _mcpDriveService.DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result<byte[]>.WithFailure(errorMessage));

        // Act
        var result = await _agent.ProcessBusinessDocumentAsync(documentId, new ProcessingOptions(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Download failed");
        result.Error.ShouldContain(errorMessage);

        // Verify no further processing was attempted
        await _documentProcessor.DidNotReceive().ProcessDocumentAsync(
            Arg.Any<byte[]>(), 
            Arg.Any<DocumentMetadata>(), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_StoreInTruthSystem_When_ConfidenceThresholdMet()
    {
        // Arrange
        var documentId = "high-confidence-doc";
        var documentData = CreateSampleIMSSDocument();
        var highConfidenceResult = new DocumentProcessingResult
        {
            DocumentId = documentId,
            Confidence = 0.95f,
            LLMConfidence = 0.93f,
            GroundingConfidence = 0.90f,
            GroundedData = new ExtractedData
            {
                Fields = new Dictionary<string, object>
                {
                    ["PaymentPeriod"] = "12-2023",
                    ["Amount"] = "15000.00",
                    ["EmployerNumber"] = "REG123456"
                }
            }
        };

        _mcpDriveService.DownloadDocumentAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result<byte[]>.WithSuccess(documentData));

        _mcpDriveService.GetDocumentMetadataAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(Result<MCPDocumentMetadata>.WithSuccess(CreateSampleMetadata(documentId)));

        _documentProcessor.ProcessDocumentAsync(
            Arg.Any<byte[]>(), 
            Arg.Any<DocumentMetadata>(), 
            Arg.Any<CancellationToken>())
            .Returns(Result<DocumentProcessingResult>.WithSuccess(highConfidenceResult));

        var truthRecord = new TruthRecord { Id = "truth-high-conf-001" };
        _truthSystem.StoreExtractedDataAsync(
            Arg.Any<ExtractedData>(), 
            Arg.Any<DataSource>(), 
            Arg.Any<CancellationToken>())
            .Returns(Result<TruthRecord>.WithSuccess(truthRecord));

        var options = new ProcessingOptions
        {
            MinimumConfidenceThreshold = 0.8f,
            StoreTruthRecord = true
        };

        // Act
        var result = await _agent.ProcessBusinessDocumentAsync(documentId, options, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.TruthRecordId.ShouldBe("truth-high-conf-001");

        // Verify truth storage was called with correct data
        await _truthSystem.Received(1).StoreExtractedDataAsync(
            Arg.Is<ExtractedData>(data => 
                data.Fields.ContainsKey("PaymentPeriod") && 
                data.Fields.ContainsKey("Amount") && 
                data.Fields.ContainsKey("EmployerNumber")),
            Arg.Is<DataSource>(source => 
                source.Type == "GoogleDrive" && 
                source.Id == documentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_OptimizeProcessingRules_When_HistoricalDataAvailable()
    {
        // Arrange
        var expectedLearningResult = new LearningResult
        {
            PatternsLearned = true,
            NewPatterns = new List<string> { "Enhanced IMSS pattern recognition" },
            ConfidenceImprovement = 0.12f,
            SchemaUpdates = new List<string> { "Updated field extraction patterns" }
        };

        // Act
        var result = await _agent.OptimizeProcessingRulesAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.PatternsLearned.ShouldBeTrue();
        result.Data.ConfidenceImprovement.ShouldBeGreaterThan(0.0f);
    }

    #region Test Data Helpers

    private static byte[] CreateSampleIMSSDocument()
    {
        var sampleContent = @"
            INSTITUTO MEXICANO DEL SEGURO SOCIAL
            CEDULA DE DETERMINACION DE CUOTAS
            PERIODO: 12-2023
            REGISTRO PATRONAL: A1234567890
            IMPORTE TOTAL: $15,000.00
            FECHA: 15/01/2024
        ";
        return System.Text.Encoding.UTF8.GetBytes(sampleContent);
    }

    private static byte[] CreateSampleDocument(DocumentType documentType)
    {
        var content = documentType switch
        {
            DocumentType.IMSSPayment => "IMSS PAYMENT DOCUMENT - PERIODO: 01-2024 - TOTAL: $10,000.00",
            DocumentType.Invoice => "FACTURA #12345 - TOTAL: $5,000.00 - RFC: ABC123456789",
            DocumentType.TaxDocument => "DOCUMENTO FISCAL - RFC: XYZ987654321 - IMPUESTO: $2,000.00",
            _ => "GENERIC DOCUMENT CONTENT"
        };
        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    private static MCPDocumentMetadata CreateSampleMetadata(string documentId)
    {
        return new MCPDocumentMetadata
        {
            Id = documentId,
            Name = "sample_imss_document.pdf",
            MimeType = "application/pdf",
            Size = 1024,
            CreatedTime = DateTime.UtcNow.AddDays(-30),
            ModifiedTime = DateTime.UtcNow.AddDays(-30),
            DriveFilePath = $"/business_documents/{documentId}.pdf"
        };
    }

    private static DocumentProcessingResult CreateSuccessfulProcessingResult(string documentId)
    {
        return new DocumentProcessingResult
        {
            DocumentId = documentId,
            ExtractionMethod = ExtractionMethod.DirectText,
            ExtractedText = "Sample extracted text from IMSS document",
            ExtractedFields = new Dictionary<string, object>
            {
                ["PaymentPeriod"] = "12-2023",
                ["Amount"] = "15000.00",
                ["EmployerNumber"] = "A1234567890",
                ["PaymentDate"] = "15/01/2024"
            },
            GroundedData = new ExtractedData
            {
                Fields = new Dictionary<string, object>
                {
                    ["PaymentPeriod"] = "12-2023",
                    ["Amount"] = "15000.00",
                    ["EmployerNumber"] = "A1234567890"
                },
                FieldConfidences = new Dictionary<string, float>
                {
                    ["PaymentPeriod"] = 0.95f,
                    ["Amount"] = 0.92f,
                    ["EmployerNumber"] = 0.98f
                }
            },
            ValidationResults = new ValidationResult
            {
                IsValid = true,
                Confidence = 0.95f
            },
            Confidence = 0.95f,
            LLMConfidence = 0.90f,
            GroundingConfidence = 0.93f,
            ProcessingTimeMs = 1500
        };
    }

    #endregion
} 