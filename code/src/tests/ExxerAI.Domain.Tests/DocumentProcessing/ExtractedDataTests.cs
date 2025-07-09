namespace ExxerAI.Domain.Tests.DocumentProcessing;

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