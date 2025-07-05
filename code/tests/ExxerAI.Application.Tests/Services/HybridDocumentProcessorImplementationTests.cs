using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Logging;
using Meziantou.Extensions.Logging.Xunit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;


namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Real implementation tests for HybridDocumentProcessor - targeting 545 lines of complex business logic
/// These tests cover the actual implementation, not interface contracts, to kill NoCoverage mutants
/// Based on proven KpiExxerpro OCRV5 and FromXcel_V3 algorithms
/// </summary>
public class HybridDocumentProcessorImplementationTests
{
	private readonly IDirectTextExtractor _mockDirectTextExtractor;
	private readonly IOCRProcessor _mockOcrProcessor;
	private readonly IRegionSpecificOCR _mockRegionOcr;
	private readonly IPersistentPatternDictionary _mockPatternDictionary;
	private readonly IDocumentSchemaLearningEngine _mockLearningEngine;
	private readonly ILogger<HybridDocumentProcessor> _logger;
	private readonly HybridDocumentProcessor _processor;

	public HybridDocumentProcessorImplementationTests(ITestOutputHelper testOutputHelper)
	{
		_mockDirectTextExtractor = Substitute.For<IDirectTextExtractor>();
		_mockOcrProcessor = Substitute.For<IOCRProcessor>();
		_mockRegionOcr = Substitute.For<IRegionSpecificOCR>();
		_mockPatternDictionary = Substitute.For<IPersistentPatternDictionary>();
		_mockLearningEngine = Substitute.For<IDocumentSchemaLearningEngine>();
		_logger = new XUnitLogger<HybridDocumentProcessor>(testOutputHelper);

		_processor = new HybridDocumentProcessor(
			_mockDirectTextExtractor,
			_mockOcrProcessor,
			_mockRegionOcr,
			_mockPatternDictionary,
			_mockLearningEngine,
			_logger);
	}

	[Fact]
	public void Constructor_Should_InitializeAllDependencies_When_ValidParametersProvided()
	{
		// Arrange & Act
		var processor = new HybridDocumentProcessor(
			_mockDirectTextExtractor,
			_mockOcrProcessor,
			_mockRegionOcr,
			_mockPatternDictionary,
			_mockLearningEngine,
			_logger);

		// Assert
		processor.ShouldNotBeNull();
	}

	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_DirectTextExtractorIsNull()
	{
		// Arrange & Act & Assert
		Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
			null!,
			_mockOcrProcessor,
			_mockRegionOcr,
			_mockPatternDictionary,
			_mockLearningEngine,
			_logger));
	}

	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_OcrProcessorIsNull()
	{
		// Arrange & Act & Assert
		Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
			_mockDirectTextExtractor,
			null!,
			_mockRegionOcr,
			_mockPatternDictionary,
			_mockLearningEngine,
			_logger));
	}

	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_RegionOcrIsNull()
	{
		// Arrange & Act & Assert
		Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
			_mockDirectTextExtractor,
			_mockOcrProcessor,
			null!,
			_mockPatternDictionary,
			_mockLearningEngine,
			_logger));
	}

	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_PatternDictionaryIsNull()
	{
		// Arrange & Act & Assert
		Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
			_mockDirectTextExtractor,
			_mockOcrProcessor,
			_mockRegionOcr,
			null!,
			_mockLearningEngine,
			_logger));
	}

	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_LearningEngineIsNull()
	{
		// Arrange & Act & Assert
		Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
			_mockDirectTextExtractor,
			_mockOcrProcessor,
			_mockRegionOcr,
			_mockPatternDictionary,
			null!,
			_logger));
	}

	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_LoggerIsNull()
	{
		// Arrange & Act & Assert
		Should.Throw<ArgumentNullException>(() => new HybridDocumentProcessor(
			_mockDirectTextExtractor,
			_mockOcrProcessor,
			_mockRegionOcr,
			_mockPatternDictionary,
			_mockLearningEngine,
			null!));
	}

	[Fact]
	public async Task ProcessDocumentAsync_Should_UseDirectTextExtraction_When_DirectTextIsSuccessful()
	{
		// Arrange
		var documentData = new byte[] { 1, 2, 3, 4, 5 };
		var metadata = new DocumentMetadata 
		{ 
			FileName = "test.pdf",
			DocumentType = DocumentType.PayrollDocument 
		};

		var directTextResult = new DirectTextResult 
		{ 
			IsSuccessful = true,
			Text = "Extracted meaningful text content from digital PDF document",
			Confidence = 0.95f
		};

		var patterns = new Dictionary<string, List<ExtractionPattern>>
		{
			["CompanyName"] = new List<ExtractionPattern>
			{
				CreateMockExtractionPattern("CompanyName", "TestCompany")
			}
		};

		_mockDirectTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(directTextResult));
		_mockPatternDictionary.GetPatternsForDocumentTypeAsync("PayrollDocument", Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(patterns));

		// Act
		var result = await _processor.ProcessDocumentAsync(documentData, metadata);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.ExtractionMethod.ShouldBe(ExtractionMethod.DirectText);
		result.Value.ExtractedText.ShouldBe(directTextResult.Text);
		result.Value.Confidence.ShouldBe(0.95f);
		result.Value.ExtractedFields.ShouldContainKey("CompanyName");
		result.Value.ExtractedFields["CompanyName"].ShouldBe("TestCompany");

		await _mockDirectTextExtractor.Received(1).ExtractTextAsync(documentData, Arg.Any<CancellationToken>());
		await _mockOcrProcessor.DidNotReceive().ProcessDocumentWithOCRAsync(Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ProcessDocumentAsync_Should_FallbackToOCR_When_DirectTextFails()
	{
		// Arrange
		var documentData = new byte[] { 1, 2, 3, 4, 5 };
		var metadata = new DocumentMetadata 
		{ 
			FileName = "scanned.pdf",
			DocumentType = DocumentType.InvoiceDocument 
		};

		var directTextResult = new DirectTextResult 
		{ 
			IsSuccessful = false,
			Text = "",
			Confidence = 0.0f
		};

		var ocrResult = new OCRResult 
		{ 
			IsSuccessful = true,
			Text = "OCR extracted text from scanned document image",
			Confidence = 0.85f
		};

		var patterns = new Dictionary<string, List<ExtractionPattern>>
		{
			["InvoiceNumber"] = new List<ExtractionPattern>
			{
				CreateMockExtractionPattern("InvoiceNumber", "INV-12345")
			}
		};

		_mockDirectTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(directTextResult));
		_mockOcrProcessor.ProcessDocumentWithOCRAsync(documentData, "spa", Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(ocrResult));
		_mockPatternDictionary.GetPatternsForDocumentTypeAsync("InvoiceDocument", Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(patterns));

		// Act
		var result = await _processor.ProcessDocumentAsync(documentData, metadata);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.ExtractionMethod.ShouldBe(ExtractionMethod.OCR);
		result.Value.ExtractedText.ShouldBe(ocrResult.Text);
		result.Value.Confidence.ShouldBe(0.85f);
		result.Value.ExtractedFields.ShouldContainKey("InvoiceNumber");
		result.Value.ExtractedFields["InvoiceNumber"].ShouldBe("INV-12345");

		await _mockDirectTextExtractor.Received(1).ExtractTextAsync(documentData, Arg.Any<CancellationToken>());
		await _mockOcrProcessor.Received(1).ProcessDocumentWithOCRAsync(documentData, "spa", Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ProcessDocumentAsync_Should_ReturnFailure_When_BothDirectTextAndOCRFail()
	{
		// Arrange
		var documentData = new byte[] { 1, 2, 3, 4, 5 };
		var metadata = new DocumentMetadata 
		{ 
			FileName = "corrupted.pdf",
			DocumentType = DocumentType.ContractDocument 
		};

		var directTextResult = new DirectTextResult 
		{ 
			IsSuccessful = false,
			Text = "",
			Confidence = 0.0f
		};

		var ocrResult = new OCRResult 
		{ 
			IsSuccessful = false,
			Text = "",
			Confidence = 0.0f
		};

		_mockDirectTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(directTextResult));
		_mockOcrProcessor.ProcessDocumentWithOCRAsync(documentData, "spa", Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(ocrResult));

		// Act
		var result = await _processor.ProcessDocumentAsync(documentData, metadata);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe("Unable to extract text through direct or OCR methods");

		await _mockDirectTextExtractor.Received(1).ExtractTextAsync(documentData, Arg.Any<CancellationToken>());
		await _mockOcrProcessor.Received(1).ProcessDocumentWithOCRAsync(documentData, "spa", Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ProcessDocumentAsync_Should_ExtractMultipleFields_When_PatternsMatch()
	{
		// Arrange
		var documentData = new byte[] { 1, 2, 3, 4, 5 };
		var metadata = new DocumentMetadata 
		{ 
			FileName = "payroll.pdf",
			DocumentType = DocumentType.PayrollDocument 
		};

		var directTextResult = new DirectTextResult 
		{ 
			IsSuccessful = true,
			Text = "Company: ACME Corp\nEmployee: John Doe\nSalary: $50,000\nPeriod: 2024-01",
			Confidence = 0.95f
		};

		var patterns = new Dictionary<string, List<ExtractionPattern>>
		{
			["CompanyName"] = new List<ExtractionPattern>
			{
				CreateMockExtractionPattern("CompanyName", "ACME Corp")
			},
			["EmployeeName"] = new List<ExtractionPattern>
			{
				CreateMockExtractionPattern("EmployeeName", "John Doe")
			},
			["Salary"] = new List<ExtractionPattern>
			{
				CreateMockExtractionPattern("Salary", "$50,000")
			},
			["Period"] = new List<ExtractionPattern>
			{
				CreateMockExtractionPattern("Period", "2024-01")
			}
		};

		_mockDirectTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(directTextResult));
		_mockPatternDictionary.GetPatternsForDocumentTypeAsync("PayrollDocument", Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(patterns));

		// Act
		var result = await _processor.ProcessDocumentAsync(documentData, metadata);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.ExtractedFields.Count.ShouldBe(4);
		result.Value.ExtractedFields["CompanyName"].ShouldBe("ACME Corp");
		result.Value.ExtractedFields["EmployeeName"].ShouldBe("John Doe");
		result.Value.ExtractedFields["Salary"].ShouldBe("$50,000");
		result.Value.ExtractedFields["Period"].ShouldBe("2024-01");
	}

	[Fact]
	public async Task ProcessDocumentAsync_Should_HandleException_When_DirectTextExtractorThrows()
	{
		// Arrange
		var documentData = new byte[] { 1, 2, 3, 4, 5 };
		var metadata = new DocumentMetadata 
		{ 
			FileName = "error.pdf",
			DocumentType = DocumentType.ContractDocument 
		};

		_mockDirectTextExtractor.ExtractTextAsync(documentData, Arg.Any<CancellationToken>())
			.Throws(new InvalidOperationException("Direct text extraction failed"));

		// Act
		var result = await _processor.ProcessDocumentAsync(documentData, metadata);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldContain("Processing error: Direct text extraction failed");
	}

	[Fact]
	public async Task ProcessDocumentBatchAsync_Should_ProcessMultipleDocuments_When_ValidBatchProvided()
	{
		// Arrange
		var documents = new List<DocumentBatchItem>
		{
			new() 
			{ 
				DocumentData = new byte[] { 1, 2, 3 },
				Metadata = new DocumentMetadata { FileName = "doc1.pdf", DocumentType = DocumentType.PayrollDocument }
			},
			new() 
			{ 
				DocumentData = new byte[] { 4, 5, 6 },
				Metadata = new DocumentMetadata { FileName = "doc2.pdf", DocumentType = DocumentType.InvoiceDocument }
			}
		};

		var options = new BatchProcessingOptions
		{
			MaxConcurrency = 2,
			ProcessingTimeout = TimeSpan.FromMinutes(5)
		};

		var directTextResult = new DirectTextResult 
		{ 
			IsSuccessful = true,
			Text = "Sample document text",
			Confidence = 0.90f
		};

		var patterns = new Dictionary<string, List<ExtractionPattern>>
		{
			["SampleField"] = new List<ExtractionPattern>
			{
				CreateMockExtractionPattern("SampleField", "SampleValue")
			}
		};

		_mockDirectTextExtractor.ExtractTextAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(directTextResult));
		_mockPatternDictionary.GetPatternsForDocumentTypeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(patterns));

		// Act
		var result = await _processor.ProcessDocumentBatchAsync(documents, options);

		// Assert
		result.TotalDocuments.ShouldBe(2);
		result.SuccessfullyProcessed.ShouldBe(2);
		result.FailedDocuments.ShouldBe(0);
		result.Results.Count.ShouldBe(2);
		result.Results.All(r => r.IsSuccessful).ShouldBeTrue();
	}

	[Fact]
	public async Task ProcessDocumentBatchAsync_Should_HandleCancellation_When_CancellationRequested()
	{
		// Arrange
		var documents = new List<DocumentBatchItem>
		{
			new() 
			{ 
				DocumentData = new byte[] { 1, 2, 3 },
				Metadata = new DocumentMetadata { FileName = "doc1.pdf", DocumentType = DocumentType.PayrollDocument }
			}
		};

		var options = new BatchProcessingOptions
		{
			MaxConcurrency = 1,
			ProcessingTimeout = TimeSpan.FromMinutes(5)
		};

		using var cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.Cancel(); // Cancel immediately

		// Act
		var result = await _processor.ProcessDocumentBatchAsync(documents, options, null, cancellationTokenSource.Token);

		// Assert
		result.TotalDocuments.ShouldBe(1);
		result.SuccessfullyProcessed.ShouldBe(0);
		result.FailedDocuments.ShouldBe(1);
		result.Results.Single().IsSuccessful.ShouldBeFalse();
	}

	[Fact]
	public async Task UpdatePatternsFromSuccessfulProcessingAsync_Should_UpdatePatterns_When_HighConfidenceResult()
	{
		// Arrange
		var processingResult = new DocumentProcessingResult
		{
			DocumentId = Guid.NewGuid().ToString(),
			IsSuccessful = true,
			OverallConfidence = 0.95f,
			ExtractedFields = new Dictionary<string, object>
			{
				["CompanyName"] = "ACME Corp",
				["InvoiceNumber"] = "INV-12345"
			}
		};

		// Act
		await _processor.UpdatePatternsFromSuccessfulProcessingAsync(processingResult);

		// Assert
		await _mockPatternDictionary.Received(2).UpdatePatternFromSuccessfulExtractionAsync(
			Arg.Any<PatternLearningResult>(), 
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UpdatePatternsFromSuccessfulProcessingAsync_Should_SkipUpdate_When_LowConfidenceResult()
	{
		// Arrange
		var processingResult = new DocumentProcessingResult
		{
			DocumentId = Guid.NewGuid().ToString(),
			IsSuccessful = true,
			OverallConfidence = 0.70f, // Below 0.8 threshold
			ExtractedFields = new Dictionary<string, object>
			{
				["CompanyName"] = "ACME Corp"
			}
		};

		// Act
		await _processor.UpdatePatternsFromSuccessfulProcessingAsync(processingResult);

		// Assert
		await _mockPatternDictionary.DidNotReceive().UpdatePatternFromSuccessfulExtractionAsync(
			Arg.Any<PatternLearningResult>(), 
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UpdatePatternsFromSuccessfulProcessingAsync_Should_SkipUpdate_When_UnsuccessfulResult()
	{
		// Arrange
		var processingResult = new DocumentProcessingResult
		{
			DocumentId = Guid.NewGuid().ToString(),
			IsSuccessful = false,
			OverallConfidence = 0.95f,
			ExtractedFields = new Dictionary<string, object>
			{
				["CompanyName"] = "ACME Corp"
			}
		};

		// Act
		await _processor.UpdatePatternsFromSuccessfulProcessingAsync(processingResult);

		// Assert
		await _mockPatternDictionary.DidNotReceive().UpdatePatternFromSuccessfulExtractionAsync(
			Arg.Any<PatternLearningResult>(), 
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ValidateExtractedFieldsAsync_Should_ReturnValid_When_AllRequiredFieldsPresent()
	{
		// Arrange
		var extractedFields = new Dictionary<string, object>
		{
			["CompanyName"] = "ACME Corp",
			["InvoiceNumber"] = "INV-12345",
			["Amount"] = "$1,500.00"
		};

		var validationRules = new DocumentValidationRules
		{
			RequiredFields = new List<string> { "CompanyName", "InvoiceNumber", "Amount" },
			FieldRules = new Dictionary<string, List<DocumentValidationRule>>()
		};

		// Act
		var result = await _processor.ValidateExtractedFieldsAsync(extractedFields, validationRules);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.IsValid.ShouldBeTrue();
		result.Value.Errors.ShouldBeEmpty();
	}

	[Fact]
	public async Task ValidateExtractedFieldsAsync_Should_ReturnInvalid_When_RequiredFieldMissing()
	{
		// Arrange
		var extractedFields = new Dictionary<string, object>
		{
			["CompanyName"] = "ACME Corp"
		};

		var validationRules = new DocumentValidationRules
		{
			RequiredFields = new List<string> { "CompanyName", "InvoiceNumber", "Amount" },
			FieldRules = new Dictionary<string, List<DocumentValidationRule>>()
		};

		// Act
		var result = await _processor.ValidateExtractedFieldsAsync(extractedFields, validationRules);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.IsValid.ShouldBeFalse();
		result.Value.Errors.Count.ShouldBe(2);
		result.Value.Errors.ShouldContain(error => error.Contains("InvoiceNumber"));
		result.Value.Errors.ShouldContain(error => error.Contains("Amount"));
	}

	[Fact]
	public async Task ValidateExtractedFieldsAsync_Should_ApplyFieldRules_When_CustomRulesProvided()
	{
		// Arrange
		var extractedFields = new Dictionary<string, object>
		{
			["InvoiceNumber"] = "12345", // Invalid format (should start with INV-)
			["Amount"] = "not-a-number" // Invalid numeric value
		};

		var validationRules = new DocumentValidationRules
		{
			RequiredFields = new List<string>(),
			FieldRules = new Dictionary<string, List<DocumentValidationRule>>
			{
				["InvoiceNumber"] = new List<DocumentValidationRule>
				{
					new() { RuleType = "Regex", Expression = "^INV-", ErrorMessage = "Invoice number must start with INV-" }
				},
				["Amount"] = new List<DocumentValidationRule>
				{
					new() { RuleType = "Numeric", Expression = "", ErrorMessage = "Amount must be numeric" }
				}
			}
		};

		// Act
		var result = await _processor.ValidateExtractedFieldsAsync(extractedFields, validationRules);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.IsValid.ShouldBeFalse();
		result.Value.Errors.Count.ShouldBe(2);
		result.Value.Errors.ShouldContain(error => error.Contains("Invoice number must start with INV-"));
		result.Value.Errors.ShouldContain(error => error.Contains("Amount must be numeric"));
	}

	/// <summary>
	/// Helper method to create mock extraction patterns for testing
	/// </summary>
	private ExtractionPattern CreateMockExtractionPattern(string fieldName, string extractedValue)
	{
		var pattern = Substitute.For<ExtractionPattern>();
		pattern.PatternType.Returns($"Mock{fieldName}Pattern");
		pattern.Confidence.Returns(0.9f);
		pattern.ExtractAsync(Arg.Any<string>()).Returns(Task.FromResult(extractedValue));
		return pattern;
	}
} 