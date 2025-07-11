namespace ExxerAI.Domain.Tests.DocumentProcessing;

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