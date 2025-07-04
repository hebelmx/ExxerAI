namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for DocumentVersion class
/// </summary>
public class DocumentVersionTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_DocumentVersionCreated()
    {
        // Act
        var version = new DocumentVersion();

        // Assert
        version.Number.ShouldBe(1);
        version.Label.ShouldBe(string.Empty);
        version.PreviousVersionId.ShouldBeNull();
        version.NextVersionId.ShouldBeNull();
        version.Notes.ShouldBe(string.Empty);
    }

    [Fact]
    public void Should_SetProperties_When_DocumentVersionInitializedWithValues()
    {
        // Arrange
        var previousId = Guid.NewGuid().ToString();

        // Act
        var version = new DocumentVersion
        {
            Number = 3,
            Label = "Final",
            PreviousVersionId = previousId,
            Notes = "Updated financial figures"
        };

        // Assert
        version.Number.ShouldBe(3);
        version.Label.ShouldBe("Final");
        version.PreviousVersionId.ShouldBe(previousId);
        version.Notes.ShouldBe("Updated financial figures");
    }
}