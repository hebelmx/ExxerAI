using ExxerAI.Domain.DocumentProcessing;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Comprehensive unit tests for DocumentProcessingResult domain entity
/// Tests confidence scoring, validation logic, and learning feedback generation
/// </summary>
public class DocumentProcessingResultTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_DocumentProcessingResultCreated()
    {
        // Act
        var result = new DocumentProcessingResult();

        // Assert
        result.DocumentId.ShouldBe(string.Empty);
        result.ExtractionMethod.ShouldBe(ExtractionMethod.DirectText);
        result.ExtractedText.ShouldBe(string.Empty);
        result.ExtractedFields.ShouldNotBeNull();
        result.ExtractedFields.ShouldBeEmpty();
        result.GroundedData.ShouldNotBeNull();
        result.ValidationResults.ShouldNotBeNull();
        result.OCRRegions.ShouldNotBeNull();
        result.OCRRegions.ShouldBeEmpty();
        result.Confidence.ShouldBe(0.0f);
        result.LLMConfidence.ShouldBe(0.0f);
        result.GroundingConfidence.ShouldBe(0.0f);
        result.ProcessingTimeMs.ShouldBe(0);
        result.TruthRecordId.ShouldBeNull();
        result.SchemaLearningResults.ShouldNotBeNull();
        result.DataLineageId.ShouldBeNull();
        result.ErrorMessage.ShouldBeNull();
    }

    [Theory]
    [InlineData(0.8f, 0.7f, 0.9f, 0.78f)] // (0.8*0.4 + 0.7*0.4 + 0.9*0.2) = 0.78
    [InlineData(0.9f, 0.9f, 0.9f, 0.9f)]  // Perfect confidence
    [InlineData(0.5f, 0.6f, 0.7f, 0.58f)] // Lower confidence
    [InlineData(1.0f, 1.0f, 1.0f, 1.0f)]  // Maximum confidence
    [InlineData(0.0f, 0.0f, 0.0f, 0.0f)]  // Minimum confidence
    public void Should_CalculateCorrectOverallConfidence_When_ConfidenceScoresSet(
        float confidence, float llmConfidence, float groundingConfidence, float expectedOverall)
    {
        // Arrange
        var result = new DocumentProcessingResult
        {
            Confidence = confidence,
            LLMConfidence = llmConfidence,
            GroundingConfidence = groundingConfidence
        };

        // Act & Assert
        result.OverallConfidence.ShouldBe(expectedOverall, 0.01f);
    }

    [Theory]
    [InlineData(0.6f, "", true)]   // High confidence, no error = successful
    [InlineData(0.4f, "", false)]  // Low confidence, no error = not successful
    [InlineData(0.8f, "Error occurred", false)] // High confidence but error = not successful
    [InlineData(0.3f, "Processing failed", false)] // Low confidence and error = not successful
    public void Should_DetermineCorrectIsSuccessful_When_ConfidenceAndErrorSet(
        float overallConfidence, string errorMessage, bool expectedSuccessful)
    {
        // Arrange
        var result = new DocumentProcessingResult
        {
            Confidence = overallConfidence,
            LLMConfidence = overallConfidence,
            GroundingConfidence = overallConfidence,
            ErrorMessage = string.IsNullOrEmpty(errorMessage) ? null : errorMessage
        };

        // Act & Assert
        result.IsSuccessful.ShouldBe(expectedSuccessful);
    }

    [Fact]
    public void Should_CreateFailedResult_When_FailedMethodCalled()
    {
        // Arrange
        var errorMessage = "Processing failed due to invalid document format";

        // Act
        var result = DocumentProcessingResult.Failed(errorMessage);

        // Assert
        result.ErrorMessage.ShouldBe(errorMessage);
        result.Confidence.ShouldBe(0.0f);
        result.LLMConfidence.ShouldBe(0.0f);
        result.GroundingConfidence.ShouldBe(0.0f);
        result.OverallConfidence.ShouldBe(0.0f);
        result.IsSuccessful.ShouldBeFalse();
    }

    [Fact]
    public void Should_ConvertToLearningFeedback_When_ToLearningFeedbackCalled()
    {
        // Arrange
        var result = new DocumentProcessingResult
        {
            DocumentId = "test-doc-001",
            ExtractionMethod = ExtractionMethod.Hybrid,
            ExtractedFields = new Dictionary<string, object>
            {
                ["PaymentPeriod"] = "12-2023",
                ["Amount"] = "15000.00",
                ["FailedField"] = null!, // This will be considered failed
                ["EmployerNumber"] = "REG123456"
            },
            ProcessingTimeMs = 1500,
            ValidationResults = new ValidationResult { IsValid = true }
        };

        // Set confidence scores to get a specific overall confidence
        result.Confidence = 0.9f;
        result.LLMConfidence = 0.8f;
        result.GroundingConfidence = 0.85f;

        // Act
        var feedback = result.ToLearningFeedback();

        // Assert
        feedback.DocumentId.ShouldBe("test-doc-001");
        feedback.ExtractionMethod.ShouldBe(ExtractionMethod.Hybrid);
        feedback.ProcessingTimeMs.ShouldBe(1500);
        feedback.ValidationPassed.ShouldBeTrue();
        feedback.OverallConfidence.ShouldBe(result.OverallConfidence);

        // Check successful fields (non-null values)
        feedback.SuccessfulFields.ShouldContainKey("PaymentPeriod");
        feedback.SuccessfulFields.ShouldContainKey("Amount");
        feedback.SuccessfulFields.ShouldContainKey("EmployerNumber");
        feedback.SuccessfulFields.ShouldNotContainKey("FailedField");

        // Check failed fields (null values)
        feedback.FailedFields.ShouldContain("FailedField");
        feedback.FailedFields.Count.ShouldBe(1);
    }

    [Fact]
    public void Should_HandleComplexExtractedFields_When_VariousDataTypesUsed()
    {
        // Arrange
        var result = new DocumentProcessingResult
        {
            ExtractedFields = new Dictionary<string, object>
            {
                ["StringField"] = "Sample Text",
                ["IntField"] = 42,
                ["DecimalField"] = 123.45m,
                ["DateField"] = DateTime.Parse("2024-01-15"),
                ["BoolField"] = true,
                ["ListField"] = new List<string> { "Item1", "Item2", "Item3" },
                ["NullField"] = null!
            }
        };

        // Act
        var feedback = result.ToLearningFeedback();

        // Assert
        feedback.SuccessfulFields.Count.ShouldBe(6); // All except null field
        feedback.FailedFields.Count.ShouldBe(1);     // Only null field
        feedback.FailedFields.ShouldContain("NullField");
        
        feedback.SuccessfulFields["StringField"].ShouldBe("Sample Text");
        feedback.SuccessfulFields["IntField"].ShouldBe(42);
        feedback.SuccessfulFields["DecimalField"].ShouldBe(123.45m);
        feedback.SuccessfulFields["BoolField"].ShouldBe(true);
    }
}

/// <summary>
/// Unit tests for ExtractedData domain entity
/// </summary>
public class ExtractedDataTests
{
    [Fact]
    public void Should_InitializeWithEmptyCollections_When_ExtractedDataCreated()
    {
        // Act
        var data = new ExtractedData();

        // Assert
        data.Fields.ShouldNotBeNull();
        data.Fields.ShouldBeEmpty();
        data.FieldConfidences.ShouldNotBeNull();
        data.FieldConfidences.ShouldBeEmpty();
        data.FieldSources.ShouldNotBeNull();
        data.FieldSources.ShouldBeEmpty();
        data.Metadata.ShouldNotBeNull();
        data.Metadata.ShouldBeEmpty();
    }

    [Fact]
    public void Should_CalculateCorrectOverallConfidence_When_FieldConfidencesSet()
    {
        // Arrange
        var data = new ExtractedData
        {
            FieldConfidences = new Dictionary<string, float>
            {
                ["Field1"] = 0.9f,
                ["Field2"] = 0.8f,
                ["Field3"] = 0.7f,
                ["Field4"] = 0.6f
            }
        };

        // Act
        var overallConfidence = data.OverallConfidence;

        // Assert
        // Expected: (0.9 + 0.8 + 0.7 + 0.6) / 4 = 0.75
        overallConfidence.ShouldBe(0.75f, 0.01f);
    }

    [Fact]
    public void Should_ReturnZeroConfidence_When_NoFieldConfidencesSet()
    {
        // Arrange
        var data = new ExtractedData();

        // Act
        var overallConfidence = data.OverallConfidence;

        // Assert
        overallConfidence.ShouldBe(0.0f);
    }

    [Fact]
    public void Should_AllowFieldManipulation_When_ExtractedDataCreated()
    {
        // Arrange
        var data = new ExtractedData();

        // Act
        data.Fields["PaymentPeriod"] = "12-2023";
        data.Fields["Amount"] = "15000.00";
        data.FieldConfidences["PaymentPeriod"] = 0.95f;
        data.FieldConfidences["Amount"] = 0.90f;
        data.FieldSources["PaymentPeriod"] = "Direct text extraction";
        data.FieldSources["Amount"] = "OCR extraction";

        // Assert
        data.Fields.Count.ShouldBe(2);
        data.FieldConfidences.Count.ShouldBe(2);
        data.FieldSources.Count.ShouldBe(2);
        
        data.Fields["PaymentPeriod"].ShouldBe("12-2023");
        data.Fields["Amount"].ShouldBe("15000.00");
        data.FieldConfidences["PaymentPeriod"].ShouldBe(0.95f);
        data.FieldConfidences["Amount"].ShouldBe(0.90f);
        data.FieldSources["PaymentPeriod"].ShouldBe("Direct text extraction");
        data.FieldSources["Amount"].ShouldBe("OCR extraction");
    }
}

/// <summary>
/// Unit tests for ValidationResult domain entity
/// </summary>
public class ValidationResultTests
{
    [Fact]
    public void Should_InitializeWithValidState_When_ValidationResultCreated()
    {
        // Act
        var validation = new ValidationResult();

        // Assert
        validation.IsValid.ShouldBeTrue();
        validation.Errors.ShouldNotBeNull();
        validation.Errors.ShouldBeEmpty();
        validation.Warnings.ShouldNotBeNull();
        validation.Warnings.ShouldBeEmpty();
        validation.FieldResults.ShouldNotBeNull();
        validation.FieldResults.ShouldBeEmpty();
        validation.Confidence.ShouldBe(1.0f);
    }

    [Fact]
    public void Should_HandleValidationErrors_When_ErrorsAdded()
    {
        // Arrange
        var validation = new ValidationResult();

        // Act
        validation.IsValid = false;
        validation.Errors.Add("Field 'Amount' is required but missing");
        validation.Errors.Add("Field 'Date' has invalid format");
        validation.Confidence = 0.3f;

        // Assert
        validation.IsValid.ShouldBeFalse();
        validation.Errors.Count.ShouldBe(2);
        validation.Errors.ShouldContain("Field 'Amount' is required but missing");
        validation.Errors.ShouldContain("Field 'Date' has invalid format");
        validation.Confidence.ShouldBe(0.3f);
    }

    [Fact]
    public void Should_HandleFieldValidationResults_When_FieldResultsAdded()
    {
        // Arrange
        var validation = new ValidationResult();

        // Act
        validation.FieldResults["PaymentPeriod"] = new FieldValidationResult
        {
            IsValid = true,
            Confidence = 0.95f
        };
        validation.FieldResults["Amount"] = new FieldValidationResult
        {
            IsValid = false,
            ErrorMessage = "Invalid currency format",
            Confidence = 0.2f,
            SuggestedCorrection = "Use format: $0,000.00"
        };

        // Assert
        validation.FieldResults.Count.ShouldBe(2);
        
        var paymentPeriodResult = validation.FieldResults["PaymentPeriod"];
        paymentPeriodResult.IsValid.ShouldBeTrue();
        paymentPeriodResult.Confidence.ShouldBe(0.95f);
        paymentPeriodResult.ErrorMessage.ShouldBeNull();
        
        var amountResult = validation.FieldResults["Amount"];
        amountResult.IsValid.ShouldBeFalse();
        amountResult.ErrorMessage.ShouldBe("Invalid currency format");
        amountResult.Confidence.ShouldBe(0.2f);
        amountResult.SuggestedCorrection.ShouldBe("Use format: $0,000.00");
    }
}

/// <summary>
/// Unit tests for OCRRegion domain entity
/// </summary>
public class OCRRegionTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_OCRRegionCreated()
    {
        // Act
        var region = new OCRRegion();

        // Assert
        region.RegionId.ShouldBe(string.Empty);
        region.Text.ShouldBe(string.Empty);
        region.Confidence.ShouldBe(0.0f);
        region.BoundingBox.ShouldNotBeNull();
        region.FieldsFound.ShouldNotBeNull();
        region.FieldsFound.ShouldBeEmpty();
    }

    [Fact]
    public void Should_SetProperties_When_OCRRegionInitializedWithValues()
    {
        // Arrange & Act
        var region = new OCRRegion
        {
            RegionId = "region-001",
            Text = "PAYMENT PERIOD: 12-2023",
            Confidence = 0.87f,
            BoundingBox = new BoundingBox { X = 100, Y = 200, Width = 300, Height = 50 }
        };
        region.FieldsFound.Add("PaymentPeriod");

        // Assert
        region.RegionId.ShouldBe("region-001");
        region.Text.ShouldBe("PAYMENT PERIOD: 12-2023");
        region.Confidence.ShouldBe(0.87f);
        region.BoundingBox.X.ShouldBe(100);
        region.BoundingBox.Y.ShouldBe(200);
        region.BoundingBox.Width.ShouldBe(300);
        region.BoundingBox.Height.ShouldBe(50);
        region.FieldsFound.ShouldContain("PaymentPeriod");
    }
}

/// <summary>
/// Unit tests for BoundingBox domain entity
/// </summary>
public class BoundingBoxTests
{
    [Fact]
    public void Should_InitializeWithZeroValues_When_BoundingBoxCreated()
    {
        // Act
        var boundingBox = new BoundingBox();

        // Assert
        boundingBox.X.ShouldBe(0);
        boundingBox.Y.ShouldBe(0);
        boundingBox.Width.ShouldBe(0);
        boundingBox.Height.ShouldBe(0);
    }

    [Theory]
    [InlineData(10, 20, 100, 50)]
    [InlineData(0, 0, 800, 600)]
    [InlineData(250, 300, 150, 75)]
    public void Should_SetCoordinates_When_BoundingBoxInitializedWithValues(int x, int y, int width, int height)
    {
        // Act
        var boundingBox = new BoundingBox
        {
            X = x,
            Y = y,
            Width = width,
            Height = height
        };

        // Assert
        boundingBox.X.ShouldBe(x);
        boundingBox.Y.ShouldBe(y);
        boundingBox.Width.ShouldBe(width);
        boundingBox.Height.ShouldBe(height);
    }
}

/// <summary>
/// Unit tests for LearningResult domain entity
/// </summary>
public class LearningResultTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_LearningResultCreated()
    {
        // Act
        var learning = new LearningResult();

        // Assert
        learning.PatternsLearned.ShouldBeFalse();
        learning.NewPatterns.ShouldNotBeNull();
        learning.NewPatterns.ShouldBeEmpty();
        learning.ConfidenceImprovement.ShouldBe(0.0f);
        learning.SchemaUpdates.ShouldNotBeNull();
        learning.SchemaUpdates.ShouldBeEmpty();
    }

    [Fact]
    public void Should_SetLearningResults_When_LearningResultInitializedWithValues()
    {
        // Act
        var learning = new LearningResult
        {
            PatternsLearned = true,
            ConfidenceImprovement = 0.15f
        };
        learning.NewPatterns.Add("Enhanced IMSS date pattern recognition");
        learning.NewPatterns.Add("Improved currency format detection");
        learning.SchemaUpdates.Add("Updated PaymentPeriod field pattern");
        learning.SchemaUpdates.Add("Enhanced Amount validation rules");

        // Assert
        learning.PatternsLearned.ShouldBeTrue();
        learning.ConfidenceImprovement.ShouldBe(0.15f);
        learning.NewPatterns.Count.ShouldBe(2);
        learning.NewPatterns.ShouldContain("Enhanced IMSS date pattern recognition");
        learning.NewPatterns.ShouldContain("Improved currency format detection");
        learning.SchemaUpdates.Count.ShouldBe(2);
        learning.SchemaUpdates.ShouldContain("Updated PaymentPeriod field pattern");
        learning.SchemaUpdates.ShouldContain("Enhanced Amount validation rules");
    }
}

/// <summary>
/// Unit tests for ExtractionFeedback domain entity
/// </summary>
public class ExtractionFeedbackTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_ExtractionFeedbackCreated()
    {
        // Act
        var feedback = new ExtractionFeedback();

        // Assert
        feedback.DocumentId.ShouldBe(string.Empty);
        feedback.ExtractionMethod.ShouldBe(ExtractionMethod.DirectText);
        feedback.SuccessfulFields.ShouldNotBeNull();
        feedback.SuccessfulFields.ShouldBeEmpty();
        feedback.FailedFields.ShouldNotBeNull();
        feedback.FailedFields.ShouldBeEmpty();
        feedback.OverallConfidence.ShouldBe(0.0f);
        feedback.ProcessingTimeMs.ShouldBe(0);
        feedback.ValidationPassed.ShouldBeTrue();
    }

    [Fact]
    public void Should_SetFeedbackProperties_When_ExtractionFeedbackInitializedWithValues()
    {
        // Arrange & Act
        var feedback = new ExtractionFeedback
        {
            DocumentId = "feedback-test-001",
            ExtractionMethod = ExtractionMethod.Hybrid,
            OverallConfidence = 0.85f,
            ProcessingTimeMs = 2500,
            ValidationPassed = false
        };
        
        feedback.SuccessfulFields["PaymentPeriod"] = "12-2023";
        feedback.SuccessfulFields["Amount"] = "15000.00";
        feedback.FailedFields.Add("InvalidField");
        feedback.FailedFields.Add("MissingField");

        // Assert
        feedback.DocumentId.ShouldBe("feedback-test-001");
        feedback.ExtractionMethod.ShouldBe(ExtractionMethod.Hybrid);
        feedback.OverallConfidence.ShouldBe(0.85f);
        feedback.ProcessingTimeMs.ShouldBe(2500);
        feedback.ValidationPassed.ShouldBeFalse();
        
        feedback.SuccessfulFields.Count.ShouldBe(2);
        feedback.SuccessfulFields.ShouldContainKey("PaymentPeriod");
        feedback.SuccessfulFields.ShouldContainKey("Amount");
        
        feedback.FailedFields.Count.ShouldBe(2);
        feedback.FailedFields.ShouldContain("InvalidField");
        feedback.FailedFields.ShouldContain("MissingField");
    }
} 