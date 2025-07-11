using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Comprehensive unit tests for DocumentMetadata domain entity
/// Tests document classification, processing options, and metadata handling
/// </summary>
public class DocumentMetadataTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_DocumentMetadataCreated()
    {
        // Act
        var metadata = new DocumentMetadata();

        // Assert
        metadata.DocumentId.ShouldBe(string.Empty);
        metadata.FileName.ShouldBe(string.Empty);
        metadata.DocumentType.ShouldBe(DocumentType.Unknown);
        metadata.ExpectedSchema.ShouldBeNull();
        metadata.SourcePath.ShouldBe(string.Empty);
        metadata.ProcessingOptions.ShouldNotBeNull();
        metadata.CreatedDate.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        metadata.ModifiedDate.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        metadata.FileSize.ShouldBe(0);
        metadata.Properties.ShouldNotBeNull();
        metadata.Properties.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("01. SUA IMSS ENE2023.pdf", nameof(DocumentType.IMSSPayment))]
    [InlineData("factura_12345.pdf", nameof(DocumentType.Invoice))]
    [InlineData("invoice_2023.docx", nameof(DocumentType.Invoice))]
    [InlineData("tax_document_2023.pdf", nameof(DocumentType.TaxDocument))]
    [InlineData("random_file.txt", nameof(DocumentType.Unknown))]
    public void Should_SetDocumentProperties_When_DocumentMetadataInitializedWithValues(string fileName, string documentTypeName)
    {
        // Arrange
        var documentType = Enum.Parse<DocumentType>(documentTypeName);
        var createdDate = DateTime.UtcNow.AddDays(-30);
        var modifiedDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var metadata = new DocumentMetadata
        {
            DocumentId = "doc-123",
            FileName = fileName,
            DocumentType = documentType,
            SourcePath = $"/documents/{fileName}",
            CreatedDate = createdDate,
            ModifiedDate = modifiedDate,
            FileSize = 2048
        };

        // Assert
        metadata.DocumentId.ShouldBe("doc-123");
        metadata.FileName.ShouldBe(fileName);
        metadata.DocumentType.ShouldBe(documentType);
        metadata.SourcePath.ShouldBe($"/documents/{fileName}");
        metadata.CreatedDate.ShouldBe(createdDate);
        metadata.ModifiedDate.ShouldBe(modifiedDate);
        metadata.FileSize.ShouldBe(2048);
    }

    [Fact]
    public void Should_AllowPropertiesManipulation_When_DocumentMetadataCreated()
    {
        // Arrange
        var metadata = new DocumentMetadata();

        // Act
        metadata.Properties["Author"] = "John Doe";
        metadata.Properties["Department"] = "Finance";
        metadata.Properties["Classification"] = "Confidential";
        metadata.Properties["ProcessingPriority"] = 1;

        // Assert
        metadata.Properties.Count.ShouldBe(4);
        metadata.Properties["Author"].ShouldBe("John Doe");
        metadata.Properties["Department"].ShouldBe("Finance");
        metadata.Properties["Classification"].ShouldBe("Confidential");
        metadata.Properties["ProcessingPriority"].ShouldBe(1);
    }
} 