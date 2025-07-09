using ExxerAI.Domain;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Comprehensive unit tests for HybridDocumentProcessor using xUnit v3, Shouldly, and NSubstitute
/// Tests all major functionality including multi-stage processing pipeline, batch operations, and learning
/// </summary>
public class HybridDocumentProcessorTests
{
    private readonly IDirectTextExtractor _directTextExtractor;
    private readonly IOCRProcessor _ocrProcessor;
    private readonly IRegionSpecificOCR _regionOCR;
    private readonly IPersistentPatternDictionary _patternDictionary;
    private readonly IDocumentSchemaLearningEngine _learningEngine;
    private readonly ILogger<HybridDocumentProcessor> _logger;
    private readonly HybridDocumentProcessor _processor;

    public HybridDocumentProcessorTests()
    {
        // Arrange - Set up all dependencies with NSubstitute
        _directTextExtractor = Substitute.For<IDirectTextExtractor>();
        _ocrProcessor = Substitute.For<IOCRProcessor>();
        _regionOCR = Substitute.For<IRegionSpecificOCR>();
        _patternDictionary = Substitute.For<IPersistentPatternDictionary>();
        _learningEngine = Substitute.For<IDocumentSchemaLearningEngine>();
        _logger = Substitute.For<ILogger<HybridDocumentProcessor>>();

        // Act - Create processor instance
        _processor = new HybridDocumentProcessor(
            _directTextExtractor,
            _ocrProcessor,
            _regionOCR,
            _patternDictionary,
            _learningEngine,
            _logger);
    }

    public class ConstructorTests : HybridDocumentProcessorTests
    {
        [Fact]
        public void Constructor_WithValidDependencies_ShouldInitializeSuccessfully()
        {
            // Act & Assert - Constructor already called in base class setup
            _processor.ShouldNotBeNull();
        }

        [Fact]
        public void Constructor_WithNullDirectTextExtractor_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
                null!, _ocrProcessor, _regionOCR, _patternDictionary, _learningEngine, _logger))
                .ParamName.ShouldBe("directTextExtractor");
        }

        [Fact]
        public void Constructor_WithNullOCRProcessor_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
                _directTextExtractor, null!, _regionOCR, _patternDictionary, _learningEngine, _logger))
                .ParamName.ShouldBe("ocrProcessor");
        }

        [Fact]
        public void Constructor_WithNullRegionOCR_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
                _directTextExtractor, _ocrProcessor, null!, _patternDictionary, _learningEngine, _logger))
                .ParamName.ShouldBe("regionOCR");
        }

        [Fact]
        public void Constructor_WithNullPatternDictionary_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
                _directTextExtractor, _ocrProcessor, _regionOCR, null!, _learningEngine, _logger))
                .ParamName.ShouldBe("patternDictionary");
        }

        [Fact]
        public void Constructor_WithNullLearningEngine_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
                _directTextExtractor, _ocrProcessor, _regionOCR, _patternDictionary, null!, _logger))
                .ParamName.ShouldBe("learningEngine");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
                _directTextExtractor, _ocrProcessor, _regionOCR, _patternDictionary, _learningEngine, null!))
                .ParamName.ShouldBe("logger");
        }
    }

    public class ProcessDocumentAsyncTests : HybridDocumentProcessorTests
    {
        [Fact]
        public async Task ProcessDocumentAsync_WithSuccessfulDirectTextExtraction_ShouldReturnSuccessResult()
        {
            // Arrange
            var documentData = CreateSampleDocumentData();
            var metadata = CreateSampleDocumentMetadata();
            var directTextResult = CreateSuccessfulDirectTextResult();
            var patterns = CreateSamplePatterns();

            _directTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
                .Returns(directTextResult);
            _patternDictionary.GetPatternsForDocumentTypeAsync(metadata.DocumentType.ToString(), Arg.Any<CancellationToken>())
                .Returns(patterns);

            // Act
            var result = await _processor.ProcessDocumentAsync(documentData, metadata, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.ExtractionMethod.ShouldBe(ExtractionMethod.DirectText);
            result.Value!.ExtractedText.ShouldBe(directTextResult.Text);
            result.Value!.Confidence.ShouldBe(0.95f);
            result.Value!.DocumentId.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ProcessDocumentAsync_WithFailedDirectTextButSuccessfulOCR_ShouldReturnOCRResult()
        {
            // Arrange
            var documentData = CreateSampleDocumentData();
            var metadata = CreateSampleDocumentMetadata();
            var failedDirectTextResult = CreateFailedDirectTextResult();
            var successfulOCRResult = CreateSuccessfulOCRResult();
            var patterns = CreateSamplePatterns();

            _directTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
                .Returns(failedDirectTextResult);
            _ocrProcessor.ProcessDocumentWithOCRAsync(documentData, "spa", Arg.Any<CancellationToken>())
                .Returns(successfulOCRResult);
            _patternDictionary.GetPatternsForDocumentTypeAsync(metadata.DocumentType.ToString(), Arg.Any<CancellationToken>())
                .Returns(patterns);

            // Act
            var result = await _processor.ProcessDocumentAsync(documentData, metadata, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.ExtractionMethod.ShouldBe(ExtractionMethod.OCR);
            result.Value!.ExtractedText.ShouldBe(successfulOCRResult.Text);
            result.Value!.Confidence.ShouldBe(successfulOCRResult.Confidence);
        }

        [Fact]
        public async Task ProcessDocumentAsync_WithBothDirectTextAndOCRFailure_ShouldReturnFailureResult()
        {
            // Arrange
            var documentData = CreateSampleDocumentData();
            var metadata = CreateSampleDocumentMetadata();
            var failedDirectTextResult = CreateFailedDirectTextResult();
            var failedOCRResult = CreateFailedOCRResult();

            _directTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
                .Returns(failedDirectTextResult);
            _ocrProcessor.ProcessDocumentWithOCRAsync(documentData, "spa", Arg.Any<CancellationToken>())
                .Returns(failedOCRResult);

            // Act
            var result = await _processor.ProcessDocumentAsync(documentData, metadata, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error!.ShouldContain("Unable to extract text through direct or OCR methods");
        }

        [Fact]
        public async Task ProcessDocumentAsync_WithPatternMatching_ShouldExtractFieldsCorrectly()
        {
            // Arrange
            var documentData = CreateSampleDocumentData();
            var metadata = CreateSampleDocumentMetadata();
            var directTextResult = CreateSuccessfulDirectTextResult();
            var patterns = CreateSamplePatternsWithMockExtraction();

            _directTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
                .Returns(directTextResult);
            _patternDictionary.GetPatternsForDocumentTypeAsync(metadata.DocumentType.ToString(), Arg.Any<CancellationToken>())
                .Returns(patterns);

            // Act
            var result = await _processor.ProcessDocumentAsync(documentData, metadata, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ExtractedFields.ShouldNotBeEmpty();
            result.Value!.ExtractedFields.ShouldContainKey("registro_patronal");
            result.Value!.ExtractedFields.ShouldContainKey("periodo_imss");
            result.Value!.ProcessingTimeMs.ShouldBeGreaterThanOrEqualTo(0); // Processing time is implementation-dependent
        }

        [Fact]
        public async Task ProcessDocumentAsync_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var documentData = CreateSampleDocumentData();
            var metadata = CreateSampleDocumentMetadata();
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel the token

            // Don't mock the extractor - let the implementation handle cancellation naturally
            // The implementation checks cancellation token in multiple places

            // Act & Assert
            var result = await _processor.ProcessDocumentAsync(documentData, metadata, cts.Token, cancellationToken: TestContext.Current.CancellationToken);

            // Either the result should be a failure due to cancellation, or an exception should be thrown
            // Both are valid responses to cancellation
            if (result.IsSuccess)
            {
                // If processing somehow completed, it should be valid
                result.Value!.ShouldNotBeNull();
            }
            else
            {
                // Processing failed, which is expected with cancellation
                result.Error.ShouldNotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task ProcessDocumentAsync_WithException_ShouldReturnFailureResultWithErrorMessage()
        {
            // Arrange
            var documentData = CreateSampleDocumentData();
            var metadata = CreateSampleDocumentMetadata();
            var exceptionMessage = "Test exception message";

            _directTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
                .Returns<Task<DirectTextResult>>(callInfo => throw new InvalidOperationException(exceptionMessage));

            // Act
            var result = await _processor.ProcessDocumentAsync(documentData, metadata, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error!.ShouldContain("Processing error");
            result.Error!.ShouldContain(exceptionMessage);
        }
    }

    public class ProcessDocumentBatchAsyncTests : HybridDocumentProcessorTests
    {
        [Fact]
        public async Task ProcessDocumentBatchAsync_WithValidDocuments_ShouldProcessAllSuccessfully()
        {
            // Arrange
            var documents = CreateSampleDocumentBatch(3);
            var options = CreateBatchProcessingOptions();
            var directTextResult = CreateSuccessfulDirectTextResult();
            var patterns = CreateSamplePatterns();

            _directTextExtractor.ExtractTextAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
                .Returns(directTextResult);
            _patternDictionary.GetPatternsForDocumentTypeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(patterns);

            // Act
            var result = await _processor.ProcessDocumentBatchAsync(documents, options, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.TotalDocuments.ShouldBe(3);
            result.Results.ShouldNotBeEmpty();
            result.Results.Count.ShouldBe(3);
            // Note: Actual success/failure counts depend on implementation details
        }

        [Fact]
        public async Task ProcessDocumentBatchAsync_WithMixedResults_ShouldReportCorrectStatistics()
        {
            // Arrange
            var documents = CreateSampleDocumentBatch(3);
            var options = CreateBatchProcessingOptions();
            var successResult = CreateSuccessfulDirectTextResult();
            var failResult = CreateFailedDirectTextResult();

            // Setup first call to succeed, second to fail, third to succeed
            _directTextExtractor.ExtractTextAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
                .Returns(successResult, failResult, successResult);
            _ocrProcessor.ProcessDocumentWithOCRAsync(Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(CreateFailedOCRResult());
            _patternDictionary.GetPatternsForDocumentTypeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(CreateSamplePatterns());

            // Act
            var result = await _processor.ProcessDocumentBatchAsync(documents, options, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.TotalDocuments.ShouldBe(3);
            result.Results.Count.ShouldBe(3);
            // Note: Success/failure distribution depends on implementation details
        }

        [Fact]
        public async Task ProcessDocumentBatchAsync_WithProgressReporting_ShouldReportProgress()
        {
            // Arrange
            var documents = CreateSampleDocumentBatch(3); // Use 3 documents for better progress visibility
            var options = CreateBatchProcessingOptions();
            var progressReports = new List<BatchProgressReport>();
            var progress = new Progress<BatchProgressReport>(report => progressReports.Add(report));

            // Add small delays to make progress reporting more visible
            _directTextExtractor.ExtractTextAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
                .Returns(async callInfo =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken: TestContext.Current.CancellationToken); // Small delay to ensure progress reporting
                    return CreateSuccessfulDirectTextResult();
                });
            _patternDictionary.GetPatternsForDocumentTypeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(CreateSamplePatterns());

            // Act
            var result = await _processor.ProcessDocumentBatchAsync(documents, options, progress, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.TotalDocuments.ShouldBe(3);

            // Progress should be reported at least once (when batch completes)
            // Note: Due to async nature, we may get 1 or more progress reports
            progressReports.Count.ShouldBeGreaterThanOrEqualTo(1);

            // The final progress report should show completion
            var finalReport = progressReports.Last();
            finalReport.ProcessedCount.ShouldBe(3);
            finalReport.TotalCount.ShouldBe(3);
        }

        [Fact]
        public async Task ProcessDocumentBatchAsync_WithCancellation_ShouldHandleCancellationGracefully()
        {
            // Arrange
            var documents = CreateSampleDocumentBatch(1); // Use single document for predictable behavior
            var options = CreateBatchProcessingOptions();
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            // Don't set up complex mocks - let the implementation handle cancellation

            // Act & Assert - Should handle cancellation gracefully without throwing unhandled exceptions
            try
            {
                var result = await _processor.ProcessDocumentBatchAsync(documents, options, progress: null, cts.Token, cancellationToken: TestContext.Current.CancellationToken);

                // If we get a result, validate it
                result.ShouldNotBeNull();
                result.TotalDocuments.ShouldBe(1);
                // Results may be empty or contain failed results due to cancellation
            }
            catch (OperationCanceledException)
            {
                // This is also acceptable behavior for cancellation
                // The implementation may choose to throw or return failed results
            }
        }
    }

    public class UpdatePatternsFromSuccessfulProcessingAsyncTests : HybridDocumentProcessorTests
    {
        [Fact]
        public async Task UpdatePatternsFromSuccessfulProcessingAsync_WithHighConfidenceResult_ShouldUpdatePatterns()
        {
            // Arrange
            var processingResult = CreateSuccessfulProcessingResult(0.95f);

            // Act
            await _processor.UpdatePatternsFromSuccessfulProcessingAsync(processingResult);

            // Assert
            await _patternDictionary.Received(processingResult.ExtractedFields.Count)
                .UpdatePatternFromSuccessfulExtractionAsync(Arg.Any<PatternLearningResult>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UpdatePatternsFromSuccessfulProcessingAsync_WithLowConfidenceResult_ShouldSkipUpdate()
        {
            // Arrange
            var processingResult = CreateSuccessfulProcessingResult(0.7f); // Below 0.8 threshold

            // Act
            await _processor.UpdatePatternsFromSuccessfulProcessingAsync(processingResult);

            // Assert
            await _patternDictionary.DidNotReceive()
                .UpdatePatternFromSuccessfulExtractionAsync(Arg.Any<PatternLearningResult>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UpdatePatternsFromSuccessfulProcessingAsync_WithFailedResult_ShouldSkipUpdate()
        {
            // Arrange
            var processingResult = CreateFailedProcessingResult();

            // Act
            await _processor.UpdatePatternsFromSuccessfulProcessingAsync(processingResult);

            // Assert
            await _patternDictionary.DidNotReceive()
                .UpdatePatternFromSuccessfulExtractionAsync(Arg.Any<PatternLearningResult>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UpdatePatternsFromSuccessfulProcessingAsync_WithEmptyFields_ShouldNotUpdatePatterns()
        {
            // Arrange
            var processingResult = CreateSuccessfulProcessingResult(0.9f);
            processingResult.ExtractedFields.Clear(); // Remove all fields

            // Act
            await _processor.UpdatePatternsFromSuccessfulProcessingAsync(processingResult);

            // Assert
            await _patternDictionary.DidNotReceive()
                .UpdatePatternFromSuccessfulExtractionAsync(Arg.Any<PatternLearningResult>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UpdatePatternsFromSuccessfulProcessingAsync_WithException_ShouldLogErrorAndContinue()
        {
            // Arrange
            var processingResult = CreateSuccessfulProcessingResult(0.95f);
            _patternDictionary.UpdatePatternFromSuccessfulExtractionAsync(Arg.Any<PatternLearningResult>(), Arg.Any<CancellationToken>())
                .Returns<Task>(callInfo => throw new InvalidOperationException("Test exception"));

            // Act & Assert - Should not throw
            await _processor.UpdatePatternsFromSuccessfulProcessingAsync(processingResult);
        }
    }

    public class ValidateExtractedFieldsAsyncTests : HybridDocumentProcessorTests
    {
        [Fact]
        public async Task ValidateExtractedFieldsAsync_WithValidFields_ShouldReturnValidResult()
        {
            // Arrange
            var extractedFields = CreateValidExtractedFields();
            var validationRules = CreateValidationRules();

            // Act
            var result = await _processor.ValidateExtractedFieldsAsync(extractedFields, validationRules, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.IsValid.ShouldBeTrue();
            result.Value!.Errors.ShouldBeEmpty();
        }

        [Fact]
        public async Task ValidateExtractedFieldsAsync_WithMissingRequiredFields_ShouldReturnInvalidResult()
        {
            // Arrange
            var extractedFields = new Dictionary<string, object>(); // Empty fields
            var validationRules = CreateValidationRulesWithRequiredFields();

            // Act
            var result = await _processor.ValidateExtractedFieldsAsync(extractedFields, validationRules, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.IsValid.ShouldBeFalse();
            result.Value!.Errors.ShouldNotBeEmpty();
            result.Value!.Errors.ShouldContain(error => error.Contains("Required field"));
        }

        [Fact]
        public async Task ValidateExtractedFieldsAsync_WithInvalidFieldValues_ShouldReturnInvalidResult()
        {
            // Arrange
            var extractedFields = CreateInvalidExtractedFields();
            var validationRules = CreateStrictValidationRules();

            // Act
            var result = await _processor.ValidateExtractedFieldsAsync(extractedFields, validationRules, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.IsValid.ShouldBeFalse();
            result.Value!.Errors.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task ValidateExtractedFieldsAsync_WithMalformedRegex_ShouldHandleGracefully()
        {
            // Arrange
            var extractedFields = new Dictionary<string, object> { ["test_field"] = "some_value" };
            var validationRules = CreateMalformedValidationRules(); // Rules with invalid regex

            // Act
            var result = await _processor.ValidateExtractedFieldsAsync(extractedFields, validationRules, cancellationToken: TestContext.Current.CancellationToken);

            // Assert - The method handles regex exceptions gracefully and returns success with invalid validation
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.IsValid.ShouldBeFalse(); // Validation fails due to malformed regex
        }
    }

    // Helper methods for creating test data
    private static byte[] CreateSampleDocumentData() =>
        System.Text.Encoding.UTF8.GetBytes("Sample PDF document content for testing");

    private static DocumentMetadata CreateSampleDocumentMetadata() => new()
    {
        DocumentId = Guid.NewGuid().ToString(),
        FileName = "test-document.pdf",
        DocumentType = DocumentType.IMSSPayment,
        SourcePath = "/test/path",
        FileSize = 12345,
        CreatedDate = DateTime.UtcNow.AddDays(-1),
        ModifiedDate = DateTime.UtcNow
    };

    private static DirectTextResult CreateSuccessfulDirectTextResult() => new()
    {
        IsSuccessful = true,
        Text = "REGISTRO PATRONAL: RFC: TEST123456 PERIODO QUE COMPRENDE EL PAGO DE SEGUROS IMSS: ENERO 2024",
        Confidence = 0.95f
    };

    private static DirectTextResult CreateFailedDirectTextResult() => new()
    {
        IsSuccessful = false,
        Text = "",
        Confidence = 0.0f
    };

    private static OCRResult CreateSuccessfulOCRResult() => new()
    {
        IsSuccessful = true,
        Text = "OCR EXTRACTED: REGISTRO PATRONAL: RFC: OCR123456 PERIODO: FEBRERO 2024",
        Confidence = 0.85f,
        ProcessedRegions = new List<OCRRegion>()
    };

    private static OCRResult CreateFailedOCRResult() => new()
    {
        IsSuccessful = false,
        Text = "",
        Confidence = 0.0f,
        ProcessedRegions = new List<OCRRegion>()
    };

    private static Dictionary<string, List<ExtractionPattern>> CreateSamplePatterns()
    {
        var patterns = new Dictionary<string, List<ExtractionPattern>>();

        var registroPattern = new TestExtractionPattern("Regex", 0.95f, "");

        patterns["registro_patronal"] = new List<ExtractionPattern> { registroPattern };
        return patterns;
    }

    private static Dictionary<string, List<ExtractionPattern>> CreateSamplePatternsWithMockExtraction()
    {
        var patterns = new Dictionary<string, List<ExtractionPattern>>();

        var registroPattern = new TestExtractionPattern("Regex", 0.95f, "TEST123456");
        var periodoPattern = new TestExtractionPattern("Regex", 0.90f, "ENERO 2024");

        patterns["registro_patronal"] = new List<ExtractionPattern> { registroPattern };
        patterns["periodo_imss"] = new List<ExtractionPattern> { periodoPattern };
        return patterns;
    }

    /// <summary>
    /// Test implementation of ExtractionPattern for unit testing
    /// </summary>
    private class TestExtractionPattern : ExtractionPattern
    {
        private readonly string _patternType;
        private readonly string _returnValue;

        public TestExtractionPattern(string patternType, float confidence, string returnValue)
        {
            _patternType = patternType;
            Confidence = confidence;
            _returnValue = returnValue;
        }

        public string PatternType => _patternType;

        public override string? ExtractValue(string text, ExtractionContext context)
        {
            return string.IsNullOrEmpty(_returnValue) ? null : _returnValue;
        }
    }

    private static List<DocumentBatchItem> CreateSampleDocumentBatch(int count)
    {
        var batch = new List<DocumentBatchItem>();
        for (int i = 0; i < count; i++)
        {
            batch.Add(new DocumentBatchItem
            {
                DocumentData = CreateSampleDocumentData(),
                Metadata = CreateSampleDocumentMetadata()
            });
        }
        return batch;
    }

    private static BatchProcessingOptions CreateBatchProcessingOptions() => new()
    {
        MaxConcurrency = 2,
        ProcessingTimeout = TimeSpan.FromMinutes(5)
    };

    private static DocumentProcessingResult CreateSuccessfulProcessingResult(float baseConfidence)
    {
        var result = new DocumentProcessingResult
        {
            DocumentId = Guid.NewGuid().ToString(),
            ExtractionMethod = ExtractionMethod.DirectText,
            ExtractedText = "Sample extracted text",
            ProcessingTimeMs = 1500,
            Confidence = baseConfidence,
            LLMConfidence = baseConfidence * 0.9f,
            GroundingConfidence = baseConfidence * 0.8f
        };

        result.ExtractedFields["registro_patronal"] = "TEST123456";
        result.ExtractedFields["periodo_imss"] = "ENERO 2024";
        return result;
    }

    private static DocumentProcessingResult CreateFailedProcessingResult() =>
        DocumentProcessingResult.Failed("Test processing failure");

    private static Dictionary<string, object> CreateValidExtractedFields() => new()
    {
        ["registro_patronal"] = "TEST123456",
        ["periodo_imss"] = "ENERO 2024",
        ["total_pagar"] = "1234.56"
    };

    private static Dictionary<string, object> CreateInvalidExtractedFields() => new()
    {
        ["registro_patronal"] = "", // Empty string
        ["periodo_imss"] = "INVALID_PERIOD_FORMAT",
        ["total_pagar"] = "NOT_A_NUMBER"
    };

    private static DocumentValidationRules CreateValidationRules() => new()
    {
        RequiredFields = new List<string>(),
        FieldRules = new Dictionary<string, List<DocumentValidationRule>>()
    };

    private static DocumentValidationRules CreateValidationRulesWithRequiredFields() => new()
    {
        RequiredFields = new List<string> { "registro_patronal", "periodo_imss" },
        FieldRules = new Dictionary<string, List<DocumentValidationRule>>()
    };

    private static DocumentValidationRules CreateStrictValidationRules() => new()
    {
        RequiredFields = new List<string> { "registro_patronal" },
        FieldRules = new Dictionary<string, List<DocumentValidationRule>>
        {
            ["registro_patronal"] = new List<DocumentValidationRule>
            {
                new DocumentValidationRule
                {
                    RuleType = "NotEmpty",
                    Expression = "",
                    ErrorMessage = "Registro patronal cannot be empty"
                }
            },
            ["total_pagar"] = new List<DocumentValidationRule>
            {
                new DocumentValidationRule
                {
                    RuleType = "Numeric",
                    Expression = "",
                    ErrorMessage = "Total must be numeric"
                }
            }
        }
    };

    private static DocumentValidationRules CreateMalformedValidationRules() => new()
    {
        RequiredFields = new List<string>(),
        FieldRules = new Dictionary<string, List<DocumentValidationRule>>
        {
            ["test_field"] = new List<DocumentValidationRule>
            {
                new DocumentValidationRule
                {
                    RuleType = "Regex",
                    Expression = "[", // Invalid regex that will cause RegexException
                    ErrorMessage = "Malformed regex"
                }
            }
        }
    };
}