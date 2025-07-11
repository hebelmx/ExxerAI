namespace ExxerAI.Domain.Tests.DocumentProcessing.ProcessingAndOCR;

/// <summary>
/// Unit tests for ProcessingOptions domain entity
/// </summary>
public class ProcessingOptionsTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_ProcessingOptionsCreated()
    {
        // Act
        var options = new ProcessingOptions();

        // Assert
        options.UseOCRFallback.ShouldBeTrue();
        options.UseLLMExtraction.ShouldBeTrue();
        options.EnableSchemaLearning.ShouldBeTrue();
        options.MinimumConfidenceThreshold.ShouldBe(0.7f);
        options.OCRLanguages.ShouldNotBeNull();
        options.OCRLanguages.Count.ShouldBe(2);
        options.OCRLanguages.ShouldContain("spa");
        options.OCRLanguages.ShouldContain("eng");
        options.CustomParameters.ShouldNotBeNull();
        options.CustomParameters.ShouldBeEmpty();
    }

    [Fact]
    public void Should_SetProcessingOptions_When_ProcessingOptionsInitializedWithValues()
    {
        // Act
        var options = new ProcessingOptions
        {
            UseOCRFallback = false,
            UseLLMExtraction = false,
            EnableSchemaLearning = false,
            MinimumConfidenceThreshold = 0.9f
        };
        
        options.OCRLanguages.Clear();
        options.OCRLanguages.Add("fra");
        options.OCRLanguages.Add("deu");
        
        options.CustomParameters["MaxProcessingTime"] = 5000;
        options.CustomParameters["EnableDebugMode"] = true;

        // Assert
        options.UseOCRFallback.ShouldBeFalse();
        options.UseLLMExtraction.ShouldBeFalse();
        options.EnableSchemaLearning.ShouldBeFalse();
        options.MinimumConfidenceThreshold.ShouldBe(0.9f);
        options.OCRLanguages.Count.ShouldBe(2);
        options.OCRLanguages.ShouldContain("fra");
        options.OCRLanguages.ShouldContain("deu");
        options.CustomParameters.Count.ShouldBe(2);
        options.CustomParameters["MaxProcessingTime"].ShouldBe(5000);
        options.CustomParameters["EnableDebugMode"].ShouldBe(true);
    }
} 