using ExxerAI.Domain.DocumentProcessing;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Comprehensive unit tests for DocumentAsset domain entity
/// Tests document lifecycle, status transitions, and business rules
/// </summary>
public class DocumentAssetTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_DocumentAssetCreated()
    {
        // Act
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");

        // Assert
        document.Id.ShouldNotBe(string.Empty);
        document.OriginalFileName.ShouldBe("test.pdf");
        document.ContentHash.ShouldBe(string.Empty);
        document.Fingerprint.ShouldNotBeNull();
        document.Content.ShouldBe(new byte[] { 1, 2, 3 });
        document.Embeddings.ShouldBeEmpty();
        document.Status.ShouldBe(DocumentStatus.Processing);
        document.ProcessedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        document.Version.ShouldNotBeNull();
        document.Version.Number.ShouldBe(1);
        document.RelatedDocuments.ShouldBeEmpty();
        document.Metadata.ShouldBeEmpty();
        document.IsDeleted().ShouldBeFalse();
    }

    [Theory]
    [InlineData("test-document.pdf", "application/pdf", 1024)]
    [InlineData("sample-invoice.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 2048)]
    [InlineData("financial-report.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 4096)]
    public void Should_SetProperties_When_DocumentAssetInitializedWithValues(string fileName, string mimeType, long size)
    {
        // Arrange & Act
        var document = new DocumentAsset(fileName, new byte[size], $"/documents/{fileName}")
        {
            MimeType = mimeType,
            SourcePath = $"/documents/{fileName}"
        };

        // Assert
        document.OriginalFileName.ShouldBe(fileName);
        document.MimeType.ShouldBe(mimeType);
        document.FileSize.ShouldBe(size);
        document.SourcePath.ShouldBe($"/documents/{fileName}");
    }

    [Fact]
    public void Should_MarkAsActive_When_MarkAsActiveCalled()
    {
        // Arrange
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");

        // Act
        document.MarkAsActive();

        // Assert
        document.Status.ShouldBe(DocumentStatus.Active);
        document.IsDeleted().ShouldBeFalse();
    }

    [Fact]
    public void Should_MarkAsDeleted_When_MarkAsDeletedCalled()
    {
        // Arrange
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");

        // Act
        document.MarkAsDeleted();

        // Assert
        document.Status.ShouldBe(DocumentStatus.Deleted);
        document.IsDeleted().ShouldBeTrue();
    }

    [Fact]
    public void Should_AddRelatedDocument_When_ValidGuidProvided()
    {
        // Arrange
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");
        var relatedDocumentId = Guid.NewGuid().ToString();

        // Act
        document.AddRelatedDocument(relatedDocumentId);

        // Assert
        document.RelatedDocuments.ShouldContain(relatedDocumentId);
        document.RelatedDocuments.Count.ShouldBe(1);
    }

    [Fact]
    public void Should_NotAddDuplicateRelatedDocument_When_SameGuidAddedTwice()
    {
        // Arrange
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");
        var relatedDocumentId = Guid.NewGuid().ToString();

        // Act
        document.AddRelatedDocument(relatedDocumentId);
        document.AddRelatedDocument(relatedDocumentId); // Add same ID again

        // Assert
        document.RelatedDocuments.ShouldContain(relatedDocumentId);
        document.RelatedDocuments.Count.ShouldBe(1); // Should not duplicate
    }

    [Fact]
    public void Should_SetContentHash_When_ValidHashProvided()
    {
        // Arrange
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");
        var validHash = "a1b2c3d4e5f6789012345678901234567890123456789012345678901234567890123456";

        // Act
        document.SetContentHash(validHash);

        // Assert
        document.ContentHash.ShouldBe(validHash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowArgumentException_When_InvalidHashProvided(string? invalidHash)
    {
        // Arrange
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");

        // Act & Assert - Focus on behavior: should throw ArgumentException, not exact message
        Should.Throw<ArgumentException>(() => document.SetContentHash(invalidHash!));
    }

    [Fact]
    public void Should_SetEmbeddings_When_ValidEmbeddingsProvided()
    {
        // Arrange
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");
        var embeddings = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

        // Act
        document.SetEmbeddings(embeddings);

        // Assert
        document.Embeddings.ShouldBe(embeddings);
        document.Embeddings.Length.ShouldBe(5);
    }

    [Fact]
    public void Should_SetEmptyEmbeddings_When_NullEmbeddingsProvided()
    {
        // Arrange
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");

        // Act
        document.SetEmbeddings(null!);

        // Assert
        document.Embeddings.ShouldNotBeNull();
        document.Embeddings.ShouldBeEmpty();
    }

    [Fact]
    public void Should_ReturnCorrectIsDeleted_When_StatusIsDeleted()
    {
        // Arrange
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");

        // Act & Assert - Initially not deleted
        document.IsDeleted().ShouldBeFalse();

        // Act - Mark as deleted
        document.MarkAsDeleted();

        // Assert - Now deleted
        document.IsDeleted().ShouldBeTrue();
    }

    [Theory]
    [InlineData(nameof(DocumentStatus.Processing))]
    [InlineData(nameof(DocumentStatus.Active))]
    [InlineData(nameof(DocumentStatus.Archived))]
    [InlineData(nameof(DocumentStatus.Error))]
    public void Should_ReturnFalseForIsDeleted_When_StatusIsNotDeleted(string statusName)
    {
        // Arrange
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");
        var status = Enum.Parse<DocumentStatus>(statusName);

        // Act
        // Cannot directly set Status as it's private set, use methods instead
        if (status == DocumentStatus.Active)
            document.MarkAsActive();
        else if (status == DocumentStatus.Archived)
            document.MarkAsArchived();
        else if (status == DocumentStatus.Error)
            document.MarkAsFailed("Test error");

        // Assert
        document.IsDeleted().ShouldBeFalse();
    }

    [Fact]
    public void Should_InitializeDocumentFingerprint_When_DocumentAssetCreated()
    {
        // Act
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");

        // Assert
        document.Fingerprint.ShouldNotBeNull();
        document.Fingerprint.Properties.ShouldNotBeNull();
        document.Fingerprint.Properties.ShouldBeEmpty();
        document.Fingerprint.TitlePattern.ShouldBe(string.Empty);
    }

    [Fact]
    public void Should_InitializeDocumentVersion_When_DocumentAssetCreated()
    {
        // Act
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");

        // Assert
        document.Version.ShouldNotBeNull();
        document.Version.Number.ShouldBe(1);
        document.Version.Label.ShouldBe(string.Empty);
        document.Version.PreviousVersionId.ShouldBeNull();
        document.Version.Notes.ShouldBe(string.Empty);
    }

    [Fact]
    public void Should_AllowMetadataManipulation_When_DocumentAssetCreated()
    {
        // Arrange
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");

        // Act
        document.Metadata["author"] = "John Doe";
        document.Metadata["department"] = "Finance";
        document.Metadata["classification"] = "Confidential";

        // Assert
        document.Metadata.Count.ShouldBe(3);
        document.Metadata["author"].ShouldBe("John Doe");
        document.Metadata["department"].ShouldBe("Finance");
        document.Metadata["classification"].ShouldBe("Confidential");
    }

    [Fact]
    public void Should_MaintainUniqueIds_When_MultipleDocumentsCreated()
    {
        // Act
        var document1 = new DocumentAsset("test1.pdf", new byte[] { 1 }, "/test/path1");
        var document2 = new DocumentAsset("test2.pdf", new byte[] { 2 }, "/test/path2");
        var document3 = new DocumentAsset("test3.pdf", new byte[] { 3 }, "/test/path3");

        // Assert
        document1.Id.ShouldNotBe(document2.Id);
        document1.Id.ShouldNotBe(document3.Id);
        document2.Id.ShouldNotBe(document3.Id);
    }

    [Fact]
    public void Should_HandleLargeContent_When_ContentSet()
    {
        // Arrange
        var largeContent = new byte[1024 * 1024]; // 1MB
        for (int i = 0; i < largeContent.Length; i++)
        {
            largeContent[i] = (byte)(i % 256);
        }

        // Act
        var document = new DocumentAsset("large.pdf", largeContent, "/test/path");

        // Assert
        document.Content.Length.ShouldBe(1024 * 1024);
        document.FileSize.ShouldBe(1024 * 1024);
        document.Content[0].ShouldBe((byte)0);
        document.Content[255].ShouldBe((byte)255);
        document.Content[256].ShouldBe((byte)0); // Wraps around
    }

    [Fact]
    public void Should_HandleComplexRelatedDocuments_When_MultipleRelatedDocumentsAdded()
    {
        // Arrange
        var document = new DocumentAsset("test.pdf", new byte[] { 1, 2, 3 }, "/test/path");
        var relatedIds = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid().ToString()).ToList();

        // Act
        foreach (var id in relatedIds)
        {
            document.AddRelatedDocument(id);
        }

        // Assert
        document.RelatedDocuments.Count.ShouldBe(10);
        foreach (var id in relatedIds)
        {
            document.RelatedDocuments.ShouldContain(id);
        }
    }
}

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