using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a document in the processing pipeline
/// </summary>
public class Document
{
    /// <summary>
    /// Gets or sets the unique document identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the document content as byte array
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the document metadata
    /// </summary>
    public DocumentMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the extracted text content if available
    /// </summary>
    public string? ExtractedText { get; set; }

    /// <summary>
    /// Gets or sets the document processing status
    /// </summary>
    public DocumentStatus Status { get; set; } = DocumentStatus.Processing;

    /// <summary>
    /// Gets or sets when the document was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the document was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the file size of the document content
    /// </summary>
    public long Size => Content?.Length ?? 0;

    /// <summary>
    /// Gets whether the document has content
    /// </summary>
    public bool HasContent => Content?.Length > 0;

    /// <summary>
    /// Gets whether the document has extracted text
    /// </summary>
    public bool HasExtractedText => !string.IsNullOrEmpty(ExtractedText);

    /// <summary>
    /// Initializes a new instance of the Document class
    /// </summary>
    public Document() { }

    /// <summary>
    /// Initializes a new instance of the Document class with content and metadata
    /// </summary>
    /// <param name="content">The document content</param>
    /// <param name="metadata">The document metadata</param>
    public Document(byte[] content, DocumentMetadata metadata)
    {
        Content = content;
        Metadata = metadata;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the extracted text content
    /// </summary>
    /// <param name="text">The extracted text</param>
    public void SetExtractedText(string text)
    {
        ExtractedText = text;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the document status
    /// </summary>
    /// <param name="status">The new status</param>
    public void UpdateStatus(DocumentStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Represents a document for processing operations
/// </summary>
public class Document
{
    /// <summary>
    /// Gets or sets the document identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document content as byte array
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the document type
    /// </summary>
    public DocumentType Type { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the MIME type of the document
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the document was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents an extraction schema for document processing
/// </summary>
public class ExtractionSchema
{
    /// <summary>
    /// Gets or sets the schema name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type this schema applies to
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the field extraction patterns
    /// </summary>
    public List<ExtractionField> Fields { get; set; } = new();

    /// <summary>
    /// Gets or sets when this schema was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a field extraction pattern
/// </summary>
public class ExtractionField
{
    /// <summary>
    /// Gets or sets the field name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extraction pattern (regex or other)
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the required confidence threshold
    /// </summary>
    public float RequiredConfidence { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets whether this field is required
    /// </summary>
    public bool IsRequired { get; set; } = false;
}

/// <summary>
/// Represents the result of field extraction
/// </summary>
public class ExtractionResult
{
    /// <summary>
    /// Gets or sets the extracted fields
    /// </summary>
    public Dictionary<string, object> Fields { get; set; } = new();

    /// <summary>
    /// Gets or sets the overall extraction confidence
    /// </summary>
    public float Confidence { get; set; } = 0.0f;

    /// <summary>
    /// Gets or sets when the extraction was performed
    /// </summary>
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets any errors encountered during extraction
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Represents ground truth context for validation
/// </summary>
public class GroundTruthContext
{
    /// <summary>
    /// Gets or sets the context properties
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Gets or sets known valid values for validation
    /// </summary>
    public Dictionary<string, List<object>> ValidValues { get; set; } = new();

    /// <summary>
    /// Gets or sets business rules for validation
    /// </summary>
    public List<ValidationRule> Rules { get; set; } = new();
}

/// <summary>
/// Represents a validation rule
/// </summary>
public class ValidationRule
{
    /// <summary>
    /// Gets or sets the rule name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field this rule applies to
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation pattern or expression
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message if validation fails
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Represents processing history for learning
/// </summary>
public class ProcessingHistory
{
    /// <summary>
    /// Gets or sets the processing results
    /// </summary>
    public List<DocumentProcessingResult> Results { get; set; } = new();

    /// <summary>
    /// Gets or sets when this history was compiled
    /// </summary>
    public DateTime CompiledAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets metadata about the processing sessions
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
} 