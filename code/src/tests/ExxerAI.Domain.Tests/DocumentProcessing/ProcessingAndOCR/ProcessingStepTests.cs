namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for ProcessingStep domain entity
/// </summary>
public class ProcessingStepTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_ProcessingStepCreated()
    {
        // Act
        var step = new ProcessingStep();

        // Assert
        step.StepId.ShouldNotBeNullOrEmpty();
        step.StepName.ShouldBe(string.Empty);
        step.ExecutedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        step.ProcessedBy.ShouldBe(string.Empty);
        step.Input.ShouldNotBeNull();
        step.Input.ShouldBeEmpty();
        step.Output.ShouldNotBeNull();
        step.Output.ShouldBeEmpty();
        step.Duration.ShouldBe(TimeSpan.Zero);
        step.IsSuccessful.ShouldBeTrue();
        step.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void Should_SetProcessingStepProperties_When_ProcessingStepInitializedWithValues()
    {
        // Arrange
        var executionTime = DateTime.UtcNow.AddMinutes(-5);
        var duration = TimeSpan.FromMilliseconds(1500);

        // Act
        var step = new ProcessingStep
        {
            StepName = "Document OCR Processing",
            ExecutedAt = executionTime,
            ProcessedBy = "TesseractOCREngine",
            Duration = duration,
            IsSuccessful = false,
            ErrorMessage = "OCR processing failed - low image quality"
        };

        step.Input["ImageData"] = "base64imagedata...";
        step.Input["Language"] = "spa,eng";
        step.Output["ErrorCode"] = "OCR_001";
        step.Output["PartialText"] = "PARTIAL EXTRACTED TEXT";

        // Assert
        step.StepName.ShouldBe("Document OCR Processing");
        step.ExecutedAt.ShouldBe(executionTime);
        step.ProcessedBy.ShouldBe("TesseractOCREngine");
        step.Duration.ShouldBe(duration);
        step.IsSuccessful.ShouldBeFalse();
        step.ErrorMessage.ShouldBe("OCR processing failed - low image quality");
        
        step.Input.Count.ShouldBe(2);
        step.Input["ImageData"].ShouldBe("base64imagedata...");
        step.Input["Language"].ShouldBe("spa,eng");
        
        step.Output.Count.ShouldBe(2);
        step.Output["ErrorCode"].ShouldBe("OCR_001");
        step.Output["PartialText"].ShouldBe("PARTIAL EXTRACTED TEXT");
    }

    [Fact]
    public void Should_HandleSuccessfulProcessingStep_When_ProcessingStepCompleted()
    {
        // Arrange
        var step = new ProcessingStep();

        // Act
        step.StepName = "Field Extraction";
        step.ProcessedBy = "FieldExtractionEngine";
        step.Duration = TimeSpan.FromMilliseconds(800);
        step.IsSuccessful = true;
        
        step.Input["ExtractedText"] = "PAYMENT PERIOD: 12-2023\nAMOUNT: $15,000.00";
        step.Input["SchemaDefinition"] = "IMSS_Payment_Schema";
        
        step.Output["PaymentPeriod"] = "12-2023";
        step.Output["Amount"] = "15000.00";
        step.Output["ConfidenceScore"] = 0.95f;

        // Assert
        step.IsSuccessful.ShouldBeTrue();
        step.ErrorMessage.ShouldBeNull();
        step.Duration.TotalMilliseconds.ShouldBe(800);
        
        step.Input.ShouldContainKey("ExtractedText");
        step.Input.ShouldContainKey("SchemaDefinition");
        
        step.Output.ShouldContainKey("PaymentPeriod");
        step.Output.ShouldContainKey("Amount");
        step.Output.ShouldContainKey("ConfidenceScore");
        step.Output["ConfidenceScore"].ShouldBe(0.95f);
    }
}