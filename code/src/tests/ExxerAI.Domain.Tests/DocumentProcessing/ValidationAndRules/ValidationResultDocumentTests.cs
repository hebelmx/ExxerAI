namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for ValidationResultDocument domain entity
/// </summary>
public class ValidationResultDocumentTests
{
    [Fact]
    public void Should_InitializeWithValidState_When_ValidationResultCreated()
    {
        // Act
        var validation = new ValidationResultDocument();

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
        var validation = new ValidationResultDocument();

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
        var validation = new ValidationResultDocument();

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