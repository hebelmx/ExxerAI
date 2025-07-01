using ExxerAI.Domain.DocumentProcessing;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Comprehensive tests for Document domain class following I-TDD principles.
/// Tests cover document lifecycle, processing states, metadata handling, and business rules.
/// </summary>
public class DocumentTests
{
    /// <summary>
    /// Contract Test: Document default constructor should initialize with defaults
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults_When_Created()
    {
        // Act
        var document = new Document();

        // Assert
        document.Id.ShouldNotBeEmpty();
        Guid.TryParse(document.Id, out _).ShouldBeTrue();
        document.Content.ShouldNotBeNull();
        document.Content.ShouldBeEmpty();
        document.Metadata.ShouldNotBeNull();
        document.ExtractedText.ShouldBeNull();
        document.Status.ShouldBe(DocumentStatus.Processing);
        document.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        document.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    /// <summary>
    /// Contract Test: Document constructor with parameters should initialize correctly
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithParameters_When_ContentAndMetadataProvided()
    {
        // Arrange
        var content = System.Text.Encoding.UTF8.GetBytes("Test document content");
        var metadata = new DocumentMetadata 
        { 
            FileName = "test.pdf",
            DocumentType = DocumentType.Invoice,
            FileSize = content.Length
        };

        // Act
        var document = new Document(content, metadata);

        // Assert
        document.Content.ShouldBe(content);
        document.Metadata.ShouldBeSameAs(metadata);
        document.Status.ShouldBe(DocumentStatus.Processing);
        document.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    /// <summary>
    /// Property Test: Size should return correct content length
    /// </summary>
    [Theory]
    [InlineData("", 0)]
    [InlineData("Hello", 5)]
    [InlineData("Hello World!", 12)]
    public void Size_ShouldReturnContentLength_When_ContentSet(string content, long expectedSize)
    {
        // Arrange
        var document = new Document();
        document.Content = System.Text.Encoding.UTF8.GetBytes(content);

        // Act
        var size = document.Size;

        // Assert
        size.ShouldBe(expectedSize);
    }

    /// <summary>
    /// Property Test: Size should return zero when content is null
    /// </summary>
    [Fact]
    public void Size_ShouldReturnZero_When_ContentIsNull()
    {
        // Arrange
        var document = new Document();
        document.Content = null!;

        // Act
        var size = document.Size;

        // Assert
        size.ShouldBe(0);
    }

    /// <summary>
    /// Property Test: FileName should return metadata filename
    /// </summary>
    [Fact]
    public void FileName_ShouldReturnMetadataFileName_When_MetadataHasFileName()
    {
        // Arrange
        var document = new Document();
        document.Metadata = new DocumentMetadata { FileName = "test-document.pdf" };

        // Act
        var fileName = document.FileName;

        // Assert
        fileName.ShouldBe("test-document.pdf");
    }

    /// <summary>
    /// Property Test: FileName should return empty string when metadata is null
    /// </summary>
    [Fact]
    public void FileName_ShouldReturnEmpty_When_MetadataIsNull()
    {
        // Arrange
        var document = new Document();
        document.Metadata = null!;

        // Act
        var fileName = document.FileName;

        // Assert
        fileName.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Property Test: Type should return metadata document type
    /// </summary>
    [Theory]
    [InlineData(DocumentType.Invoice)]
    [InlineData(DocumentType.TaxDocument)]
    [InlineData(DocumentType.IMSSPayment)]
    [InlineData(DocumentType.Unknown)]
    public void Type_ShouldReturnMetadataDocumentType_When_MetadataHasType(DocumentType documentType)
    {
        // Arrange
        var document = new Document();
        document.Metadata = new DocumentMetadata { DocumentType = documentType };

        // Act
        var type = document.Type;

        // Assert
        type.ShouldBe(documentType);
    }

    /// <summary>
    /// Property Test: Type should return Unknown when metadata is null
    /// </summary>
    [Fact]
    public void Type_ShouldReturnUnknown_When_MetadataIsNull()
    {
        // Arrange
        var document = new Document();
        document.Metadata = null!;

        // Act
        var type = document.Type;

        // Assert
        type.ShouldBe(DocumentType.Unknown);
    }

    /// <summary>
    /// Property Test: HasContent should reflect content presence
    /// </summary>
    [Theory]
    [InlineData(new byte[0], false)]
    [InlineData(new byte[] { 1, 2, 3 }, true)]
    public void HasContent_ShouldReflectContentPresence_When_ContentSet(byte[] content, bool expected)
    {
        // Arrange
        var document = new Document();
        document.Content = content;

        // Act
        var hasContent = document.HasContent;

        // Assert
        hasContent.ShouldBe(expected);
    }

    /// <summary>
    /// Property Test: HasContent should return false when content is null
    /// </summary>
    [Fact]
    public void HasContent_ShouldReturnFalse_When_ContentIsNull()
    {
        // Arrange
        var document = new Document();
        document.Content = null!;

        // Act
        var hasContent = document.HasContent;

        // Assert
        hasContent.ShouldBeFalse();
    }

    /// <summary>
    /// Property Test: HasExtractedText should reflect extracted text presence
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Some extracted text", true)]
    public void HasExtractedText_ShouldReflectTextPresence_When_ExtractedTextSet(string? extractedText, bool expected)
    {
        // Arrange
        var document = new Document();
        document.ExtractedText = extractedText;

        // Act
        var hasExtractedText = document.HasExtractedText;

        // Assert
        hasExtractedText.ShouldBe(expected);
    }

    /// <summary>
    /// Behavior Test: SetExtractedText should update text and timestamp
    /// </summary>
    [Fact]
    public void SetExtractedText_ShouldUpdateTextAndTimestamp_When_Called()
    {
        // Arrange
        var document = new Document();
        var originalUpdatedAt = document.UpdatedAt;
        var extractedText = "This is extracted text content";

        // Wait to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        document.SetExtractedText(extractedText);

        // Assert
        document.ExtractedText.ShouldBe(extractedText);
        document.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
        document.HasExtractedText.ShouldBeTrue();
    }

    /// <summary>
    /// Behavior Test: UpdateStatus should update status and timestamp
    /// </summary>
    [Theory]
    [InlineData(DocumentStatus.Processing)]
    [InlineData(DocumentStatus.Active)]
    [InlineData(DocumentStatus.Error)]
    [InlineData(DocumentStatus.Archived)]
    public void UpdateStatus_ShouldUpdateStatusAndTimestamp_When_Called(DocumentStatus newStatus)
    {
        // Arrange
        var document = new Document();
        var originalUpdatedAt = document.UpdatedAt;

        // Wait to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        document.UpdateStatus(newStatus);

        // Assert
        document.Status.ShouldBe(newStatus);
        document.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
    }

    /// <summary>
    /// Integration Test: Document should maintain consistency across operations
    /// </summary>
    [Fact]
    public void Document_ShouldMaintainConsistency_When_MultipleOperationsPerformed()
    {
        // Arrange
        var content = System.Text.Encoding.UTF8.GetBytes("Test document content");
        var metadata = new DocumentMetadata 
        { 
            FileName = "integration-test.pdf",
            DocumentType = DocumentType.TaxDocument
        };
        var document = new Document(content, metadata);
        var originalCreatedAt = document.CreatedAt;

        // Act
        document.SetExtractedText("Extracted text from document");
        document.UpdateStatus(DocumentStatus.Active);

        // Assert
        document.Content.ShouldBe(content);
        document.Metadata.ShouldBeSameAs(metadata);
        document.ExtractedText.ShouldBe("Extracted text from document");
        document.Status.ShouldBe(DocumentStatus.Active);
        document.HasContent.ShouldBeTrue();
        document.HasExtractedText.ShouldBeTrue();
        document.Size.ShouldBe(content.Length);
        document.FileName.ShouldBe("integration-test.pdf");
        document.Type.ShouldBe(DocumentType.TaxDocument);
        document.CreatedAt.ShouldBe(originalCreatedAt); // Should not change
        document.UpdatedAt.ShouldBeGreaterThan(originalCreatedAt);
    }

    /// <summary>
    /// Edge Case Test: Document should handle null content gracefully
    /// </summary>
    [Fact]
    public void Document_ShouldHandleNullContent_When_ContentSetToNull()
    {
        // Arrange
        var document = new Document();

        // Act
        document.Content = null!;

        // Assert
        document.Size.ShouldBe(0);
        document.HasContent.ShouldBeFalse();
    }

    /// <summary>
    /// Edge Case Test: Document should handle null metadata gracefully
    /// </summary>
    [Fact]
    public void Document_ShouldHandleNullMetadata_When_MetadataSetToNull()
    {
        // Arrange
        var document = new Document();

        // Act
        document.Metadata = null!;

        // Assert
        document.FileName.ShouldBe(string.Empty);
        document.Type.ShouldBe(DocumentType.Unknown);
    }
}

/// <summary>
/// Comprehensive tests for DocumentMetadata class
/// </summary>
public class DocumentMetadataTests
{
    /// <summary>
    /// Contract Test: DocumentMetadata should initialize with defaults
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults_When_Created()
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
        metadata.MimeType.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Contract Test: DocumentMetadata properties should be settable
    /// </summary>
    [Fact]
    public void Properties_ShouldBeSettable_When_ValuesAssigned()
    {
        // Arrange
        var metadata = new DocumentMetadata();
        var schema = new SchemaDefinition { Name = "Test Schema" };
        var options = new ProcessingOptions { AutoProcess = true };
        var createdDate = DateTime.UtcNow.AddDays(-30);
        var modifiedDate = DateTime.UtcNow.AddDays(-1);

        // Act
        metadata.DocumentId = "DOC123";
        metadata.FileName = "test-document.pdf";
        metadata.DocumentType = DocumentType.Invoice;
        metadata.ExpectedSchema = schema;
        metadata.SourcePath = "/path/to/document.pdf";
        metadata.ProcessingOptions = options;
        metadata.CreatedDate = createdDate;
        metadata.ModifiedDate = modifiedDate;
        metadata.FileSize = 1024;
        metadata.MimeType = "application/pdf";

        // Assert
        metadata.DocumentId.ShouldBe("DOC123");
        metadata.FileName.ShouldBe("test-document.pdf");
        metadata.DocumentType.ShouldBe(DocumentType.Invoice);
        metadata.ExpectedSchema.ShouldBeSameAs(schema);
        metadata.SourcePath.ShouldBe("/path/to/document.pdf");
        metadata.ProcessingOptions.ShouldBeSameAs(options);
        metadata.CreatedDate.ShouldBe(createdDate);
        metadata.ModifiedDate.ShouldBe(modifiedDate);
        metadata.FileSize.ShouldBe(1024);
        metadata.MimeType.ShouldBe("application/pdf");
    }

    /// <summary>
    /// Validation Test: DocumentMetadata should respect string length constraints
    /// </summary>
    [Fact]
    public void StringLengths_ShouldRespectConstraints_When_MaxLengthValuesSet()
    {
        // Arrange
        var metadata = new DocumentMetadata();
        var maxFileName = new string('A', 255);
        var maxSourcePath = new string('B', 1000);
        var maxMimeType = new string('C', 100);

        // Act
        metadata.FileName = maxFileName;
        metadata.SourcePath = maxSourcePath;
        metadata.MimeType = maxMimeType;

        // Assert
        metadata.FileName.Length.ShouldBe(255);
        metadata.SourcePath.Length.ShouldBe(1000);
        metadata.MimeType.Length.ShouldBe(100);
    }
}

/// <summary>
/// Comprehensive tests for DocumentStatus enum
/// </summary>
public class DocumentStatusTests
{
    /// <summary>
    /// Contract Test: DocumentStatus should have all expected values
    /// </summary>
    [Theory]
    [InlineData(DocumentStatus.Processing)]
    [InlineData(DocumentStatus.Active)]
    [InlineData(DocumentStatus.Deleted)]
    [InlineData(DocumentStatus.Archived)]
    [InlineData(DocumentStatus.Error)]
    public void DocumentStatus_ShouldHaveExpectedValues_When_Accessed(DocumentStatus status)
    {
        // Act & Assert
        Enum.IsDefined(typeof(DocumentStatus), status).ShouldBeTrue();
    }

    /// <summary>
    /// Contract Test: DocumentStatus should have correct default value
    /// </summary>
    [Fact]
    public void DocumentStatus_ShouldHaveProcessingAsDefault_When_DefaultUsed()
    {
        // Act
        var defaultStatus = default(DocumentStatus);

        // Assert
        defaultStatus.ShouldBe(DocumentStatus.Processing);
    }

    /// <summary>
    /// Behavior Test: DocumentStatus should convert to string correctly
    /// </summary>
    [Theory]
    [InlineData(DocumentStatus.Processing, "Processing")]
    [InlineData(DocumentStatus.Active, "Active")]
    [InlineData(DocumentStatus.Deleted, "Deleted")]
    [InlineData(DocumentStatus.Archived, "Archived")]
    [InlineData(DocumentStatus.Error, "Error")]
    public void DocumentStatus_ShouldConvertToString_When_ToString(DocumentStatus status, string expected)
    {
        // Act
        var result = status.ToString();

        // Assert
        result.ShouldBe(expected);
    }

    /// <summary>
    /// Behavior Test: DocumentStatus should support parsing from string
    /// </summary>
    [Theory]
    [InlineData("Processing", DocumentStatus.Processing)]
    [InlineData("Active", DocumentStatus.Active)]
    [InlineData("Deleted", DocumentStatus.Deleted)]
    [InlineData("Archived", DocumentStatus.Archived)]
    [InlineData("Error", DocumentStatus.Error)]
    public void DocumentStatus_ShouldParseFromString_When_ValidString(string input, DocumentStatus expected)
    {
        // Act
        var parsed = Enum.Parse<DocumentStatus>(input);

        // Assert
        parsed.ShouldBe(expected);
    }
}

/// <summary>
/// Tests for DocumentType enum and classification
/// </summary>
public class DocumentTypeTests
{
    /// <summary>
    /// Contract Test: DocumentType should have expected business values
    /// </summary>
    [Theory]
    [InlineData(DocumentType.Unknown)]
    [InlineData(DocumentType.Invoice)]
    [InlineData(DocumentType.TaxDocument)]
    [InlineData(DocumentType.IMSSPayment)]
    [InlineData(DocumentType.Report)]
    [InlineData(DocumentType.Contract)]
    [InlineData(DocumentType.Spreadsheet)]
    [InlineData(DocumentType.Presentation)]
    [InlineData(DocumentType.Email)]
    [InlineData(DocumentType.Image)]
    public void DocumentType_ShouldHaveExpectedBusinessValues_When_Accessed(DocumentType documentType)
    {
        // Act & Assert
        Enum.IsDefined(typeof(DocumentType), documentType).ShouldBeTrue();
    }

    /// <summary>
    /// Contract Test: DocumentType should have Unknown as default
    /// </summary>
    [Fact]
    public void DocumentType_ShouldHaveUnknownAsDefault_When_DefaultUsed()
    {
        // Act
        var defaultType = default(DocumentType);

        // Assert
        defaultType.ShouldBe(DocumentType.Unknown);
    }

    /// <summary>
    /// Business Rule Test: DocumentType should support business document classification
    /// </summary>
    [Theory]
    [InlineData("invoice_2023.pdf", DocumentType.Invoice)]
    [InlineData("IMSS_payment_receipt.pdf", DocumentType.IMSSPayment)]
    [InlineData("tax_return_2023.pdf", DocumentType.TaxDocument)]
    [InlineData("monthly_report.xlsx", DocumentType.Report)]
    [InlineData("unknown_file.txt", DocumentType.Unknown)]
    public void DocumentType_ShouldSupportBusinessClassification_When_ClassifyingByFileName(string fileName, DocumentType expectedType)
    {
        // This test demonstrates how DocumentType would be used in business logic
        // The actual classification logic would be in a service, not the enum itself

        // Act
        var classifiedType = ClassifyDocumentByFileName(fileName);

        // Assert
        classifiedType.ShouldBe(expectedType);
    }

    /// <summary>
    /// Helper method to simulate document classification by filename
    /// This would typically be in a business service
    /// </summary>
    private static DocumentType ClassifyDocumentByFileName(string fileName)
    {
        var lower = fileName.ToLowerInvariant();
        
        if (lower.Contains("invoice") || lower.Contains("factura"))
            return DocumentType.Invoice;
        
        if (lower.Contains("imss") || lower.Contains("seguro"))
            return DocumentType.IMSSPayment;
        
        if (lower.Contains("tax") || lower.Contains("impuesto"))
            return DocumentType.TaxDocument;
        
        if (lower.Contains("report") || lower.Contains("reporte"))
            return DocumentType.Report;
        
        return DocumentType.Unknown;
    }
}

/// <summary>
/// Integration tests for Document processing workflow
/// </summary>
public class DocumentProcessingWorkflowTests
{
    /// <summary>
    /// Workflow Test: Complete document processing lifecycle
    /// </summary>
    [Fact]
    public void DocumentProcessingWorkflow_ShouldCompleteSuccessfully_When_ValidDocumentProcessed()
    {
        // Arrange - Simulate document upload
        var content = System.Text.Encoding.UTF8.GetBytes("Invoice #12345\nAmount: $1,000.00\nDate: 2023-12-01");
        var metadata = new DocumentMetadata
        {
            DocumentId = "DOC-2023-001",
            FileName = "invoice_12345.pdf",
            DocumentType = DocumentType.Invoice,
            SourcePath = "/uploads/invoice_12345.pdf",
            FileSize = content.Length,
            MimeType = "application/pdf"
        };

        // Act - Create document and simulate processing stages
        var document = new Document(content, metadata);
        
        // Stage 1: Extract text
        document.SetExtractedText("Invoice #12345 Amount: $1,000.00 Date: 2023-12-01");
        
        // Stage 2: Mark as active after successful processing
        document.UpdateStatus(DocumentStatus.Active);

        // Assert - Verify complete processing state
        document.Id.ShouldNotBeEmpty();
        document.Content.ShouldBe(content);
        document.ExtractedText.ShouldNotBeNullOrEmpty();
        document.Status.ShouldBe(DocumentStatus.Active);
        document.HasContent.ShouldBeTrue();
        document.HasExtractedText.ShouldBeTrue();
        document.Size.ShouldBe(content.Length);
        document.FileName.ShouldBe("invoice_12345.pdf");
        document.Type.ShouldBe(DocumentType.Invoice);
        document.UpdatedAt.ShouldBeGreaterThan(document.CreatedAt);
    }

    /// <summary>
    /// Error Handling Test: Document processing with errors
    /// </summary>
    [Fact]
    public void DocumentProcessingWorkflow_ShouldHandleErrors_When_ProcessingFails()
    {
        // Arrange - Simulate corrupted document
        var content = new byte[] { 0xFF, 0xFE, 0x00 }; // Invalid content
        var metadata = new DocumentMetadata
        {
            FileName = "corrupted_file.pdf",
            DocumentType = DocumentType.Unknown
        };

        // Act - Create document and simulate processing failure
        var document = new Document(content, metadata);
        document.UpdateStatus(DocumentStatus.Error);

        // Assert - Verify error state
        document.Status.ShouldBe(DocumentStatus.Error);
        document.HasContent.ShouldBeTrue(); // Still has content, just corrupted
        document.HasExtractedText.ShouldBeFalse(); // No text extracted
        document.Type.ShouldBe(DocumentType.Unknown);
    }

    /// <summary>
    /// Performance Test: Document operations should be efficient
    /// </summary>
    [Fact]
    public void DocumentOperations_ShouldBeEfficient_When_PerformingMultipleOperations()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var documents = new List<Document>();
        var content = System.Text.Encoding.UTF8.GetBytes("Test content");

        // Act - Create and process multiple documents
        for (int i = 0; i < 100; i++)
        {
            var metadata = new DocumentMetadata
            {
                DocumentId = $"DOC-{i:000}",
                FileName = $"document_{i}.pdf",
                DocumentType = DocumentType.Report
            };

            var document = new Document(content, metadata);
            document.SetExtractedText($"Extracted text for document {i}");
            document.UpdateStatus(DocumentStatus.Active);
            
            documents.Add(document);
        }

        var duration = DateTime.UtcNow - startTime;

        // Assert
        documents.Count.ShouldBe(100);
        documents.All(d => d.Status == DocumentStatus.Active).ShouldBeTrue();
        documents.All(d => d.HasContent).ShouldBeTrue();
        documents.All(d => d.HasExtractedText).ShouldBeTrue();
        duration.ShouldBeLessThan(TimeSpan.FromSeconds(1)); // Should be very fast
    }
} 