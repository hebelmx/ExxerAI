using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

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