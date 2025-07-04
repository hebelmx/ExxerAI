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
        result.ValidationResultDocument.ShouldNotBeNull();
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
            ValidationResultDocument = new ValidationResultDocument { IsValid = true }
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