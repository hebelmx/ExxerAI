namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for DocumentFingerprint class
/// </summary>
public class DocumentFingerprintTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_DocumentFingerprintCreated()
    {
        // Act
        var fingerprint = new DocumentFingerprint();

        // Assert
        fingerprint.FileSize.ShouldBe(0);
        fingerprint.CreatedDate.ShouldBe(default(DateTime));
        fingerprint.MimeType.ShouldBe(string.Empty);
        fingerprint.TitlePattern.ShouldBe(string.Empty);
        fingerprint.Properties.ShouldNotBeNull();
        fingerprint.Properties.ShouldBeEmpty();
    }

    [Fact]
    public void Should_SetProperties_When_DocumentFingerprintInitializedWithValues()
    {
        // Arrange
        var creationDate = DateTime.UtcNow.AddDays(-30);

        // Act
        var fingerprint = new DocumentFingerprint
        {
            FileSize = 2048,
            CreatedDate = creationDate,
            MimeType = "application/pdf",
            TitlePattern = "sample-hash-123"
        };

        // Assert
        fingerprint.FileSize.ShouldBe(2048);
        fingerprint.CreatedDate.ShouldBe(creationDate);
        fingerprint.MimeType.ShouldBe("application/pdf");
        fingerprint.TitlePattern.ShouldBe("sample-hash-123");
    }
}