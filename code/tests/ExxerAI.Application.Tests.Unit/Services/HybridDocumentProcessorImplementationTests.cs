using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.Unit.Services;

/// <summary>
/// Implementation tests for HybridDocumentProcessor service
/// Tests real implementation with mocked dependencies to achieve mutation coverage
/// </summary>
public class HybridDocumentProcessorImplementationTests
{
    private readonly IDirectTextExtractor _directTextExtractor;
    private readonly IOCRProcessor _ocrProcessor;
    private readonly IRegionSpecificOCR _regionOCR;
    private readonly IPersistentPatternDictionary _patternDictionary;
    private readonly IDocumentSchemaLearningEngine _learningEngine;
    private readonly ILogger<HybridDocumentProcessor> _logger;
    private readonly HybridDocumentProcessor _processor;

    public HybridDocumentProcessorImplementationTests()
    {
        _directTextExtractor = Substitute.For<IDirectTextExtractor>();
        _ocrProcessor = Substitute.For<IOCRProcessor>();
        _regionOCR = Substitute.For<IRegionSpecificOCR>();
        _patternDictionary = Substitute.For<IPersistentPatternDictionary>();
        _learningEngine = Substitute.For<IDocumentSchemaLearningEngine>();
        _logger = Substitute.For<ILogger<HybridDocumentProcessor>>();

        _processor = new HybridDocumentProcessor(
            _directTextExtractor,
            _ocrProcessor,
            _regionOCR,
            _patternDictionary,
            _learningEngine,
            _logger);
    }

    public class ConstructorTests : HybridDocumentProcessorImplementationTests
    {
        [Fact]
        public void Should_ThrowArgumentNullException_When_DirectTextExtractorIsNull()
        {
            // Arrange & Act & Assert
            Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
                null!,
                _ocrProcessor,
                _regionOCR,
                _patternDictionary,
                _learningEngine,
                _logger))
                .ParamName.ShouldBe("directTextExtractor");
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_OcrProcessorIsNull()
        {
            // Arrange & Act & Assert
            Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
                _directTextExtractor,
                null!,
                _regionOCR,
                _patternDictionary,
                _learningEngine,
                _logger))
                .ParamName.ShouldBe("ocrProcessor");
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_PatternDictionaryIsNull()
        {
            // Arrange & Act & Assert
            Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
                _directTextExtractor,
                _ocrProcessor,
                _regionOCR,
                null!,
                _learningEngine,
                _logger))
                .ParamName.ShouldBe("patternDictionary");
        }

        [Fact]
        public void Should_CreateInstance_When_AllDependenciesProvided()
        {
            // Arrange & Act
            var processor = new HybridDocumentProcessor(
                _directTextExtractor,
                _ocrProcessor,
                _regionOCR,
                _patternDictionary,
                _learningEngine,
                _logger);

            // Assert
            processor.ShouldNotBeNull();
        }
    }

    public class ProcessDocumentAsyncTests : HybridDocumentProcessorImplementationTests
    {
        [Fact]
        public async Task Should_UseDirectTextExtraction_When_DirectTextIsSuccessful()
        {
            // Arrange
            var documentData = new byte[] { 1, 2, 3, 4, 5 };
            var metadata = new DocumentMetadata 
            { 
                FileName = "test.pdf",
                DocumentType = DocumentType.Invoice
            };

            var directResult = new DirectTextResult
            {
                IsSuccessful = true,
                Text = "Sample extracted text with meaningful content that is longer than 50 characters",
                Confidence = 0.95f
            };

            var patterns = new Dictionary<string, List<ExtractionPattern>>();

            _directTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
                .Returns(directResult);

            _patternDictionary.GetPatternsForDocumentTypeAsync(metadata.DocumentType.ToString(), Arg.Any<CancellationToken>())
                .Returns(patterns);

            // Act
            var result = await _processor.ProcessDocumentAsync(documentData, metadata);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ExtractionMethod.ShouldBe(ExtractionMethod.DirectText);
            result.Value.ExtractedText.ShouldBe(directResult.Text);
            result.Value.Confidence.ShouldBe(0.95f);

            await _directTextExtractor.Received(1).ExtractTextAsync(documentData, Arg.Any<CancellationToken>());
            await _ocrProcessor.DidNotReceive().ProcessDocumentWithOCRAsync(Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_FallbackToOCR_When_DirectTextFails()
        {
            // Arrange
            var documentData = new byte[] { 1, 2, 3, 4, 5 };
            var metadata = new DocumentMetadata 
            { 
                FileName = "scanned.pdf",
                DocumentType = DocumentType.Receipt
            };

            var directResult = new DirectTextResult
            {
                IsSuccessful = false,
                Text = "",
                Confidence = 0f
            };

            var ocrResult = new OCRResult
            {
                IsSuccessful = true,
                Text = "OCR extracted text with meaningful content that is longer than 50 characters",
                Confidence = 0.85f
            };

            var patterns = new Dictionary<string, List<ExtractionPattern>>();

            _directTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
                .Returns(directResult);

            _ocrProcessor.ProcessDocumentWithOCRAsync(documentData, "spa", Arg.Any<CancellationToken>())
                .Returns(ocrResult);

            _patternDictionary.GetPatternsForDocumentTypeAsync(metadata.DocumentType.ToString(), Arg.Any<CancellationToken>())
                .Returns(patterns);

            // Act
            var result = await _processor.ProcessDocumentAsync(documentData, metadata);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ExtractionMethod.ShouldBe(ExtractionMethod.OCR);
            result.Value.ExtractedText.ShouldBe(ocrResult.Text);
            result.Value.Confidence.ShouldBe(0.85f);

            await _directTextExtractor.Received(1).ExtractTextAsync(documentData, Arg.Any<CancellationToken>());
            await _ocrProcessor.Received(1).ProcessDocumentWithOCRAsync(documentData, "spa", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_ReturnFailure_When_BothDirectTextAndOCRFail()
        {
            // Arrange
            var documentData = new byte[] { 1, 2, 3, 4, 5 };
            var metadata = new DocumentMetadata 
            { 
                FileName = "corrupted.pdf",
                DocumentType = DocumentType.Contract
            };

            var directResult = new DirectTextResult
            {
                IsSuccessful = false,
                Text = "",
                Confidence = 0f
            };

            var ocrResult = new OCRResult
            {
                IsSuccessful = false,
                Text = "",
                Confidence = 0f
            };

            _directTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
                .Returns(directResult);

            _ocrProcessor.ProcessDocumentWithOCRAsync(documentData, "spa", Arg.Any<CancellationToken>())
                .Returns(ocrResult);

            // Act
            var result = await _processor.ProcessDocumentAsync(documentData, metadata);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBe("Unable to extract text through direct or OCR methods");

            await _directTextExtractor.Received(1).ExtractTextAsync(documentData, Arg.Any<CancellationToken>());
            await _ocrProcessor.Received(1).ProcessDocumentWithOCRAsync(documentData, "spa", Arg.Any<CancellationToken>());
        }
    }
} 