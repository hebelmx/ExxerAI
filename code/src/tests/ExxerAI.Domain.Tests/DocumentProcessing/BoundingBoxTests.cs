namespace ExxerAI.Domain.Tests.DocumentProcessing;

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