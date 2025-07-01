using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Advanced polymorphic document processor that learns to adapt to any document type.
/// Implements multi-stage processing: Direct Read → OCR → LLM Verification → Grounding.
/// Based on proven KpiExxerpro patterns with 15+ years of financial document processing.
/// </summary>
public interface IPolymorphicDocumentProcessor
{
    /// <summary>
    /// Processes a document through the complete polymorphic pipeline.
    /// </summary>
    /// <param name="documentData">The raw document byte data.</param>
    /// <param name="metadata">Document metadata and processing options.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The complete processing result with extracted data and confidence scores.</returns>
    Task<Result<DocumentProcessingResult>> ProcessDocumentAsync(
        byte[] documentData, 
        DocumentMetadata metadata, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts specific fields from a document using adaptive patterns.
    /// </summary>
    /// <param name="document">The document to extract fields from.</param>
    /// <param name="schema">The extraction schema defining expected fields.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The field extraction results with confidence scores.</returns>
    Task<Result<ExtractionResult>> ExtractFieldsAsync(
        Document document, 
        ExtractionSchema schema, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and grounds extracted data against known patterns and business rules.
    /// </summary>
    /// <param name="data">The extracted data to validate.</param>
    /// <param name="context">The ground truth context for validation.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The validation result with grounded data.</returns>
    Task<Result<ValidationResult>> ValidateAndGroundDataAsync(
        ExtractedData data, 
        GroundTruthContext context, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adapts processing rules based on processing history and feedback.
    /// </summary>
    /// <param name="history">The processing history to learn from.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The learning result with updated processing rules.</returns>
    Task<Result<LearningResult>> AdaptProcessingRulesAsync(
        ProcessingHistory history, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Learns document schema from sample documents to improve future processing.
    /// </summary>
    /// <param name="samples">Sample documents to learn from.</param>
    /// <param name="cancellationToken">Cancellation token for operation control.</param>
    /// <returns>The learned schema definition.</returns>
    Task<Result<SchemaDefinition>> LearnDocumentSchemaAsync(
        IEnumerable<Document> samples, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the confidence score for processing a specific document type
    /// </summary>
    /// <param name="documentType">The document type to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The confidence score for processing this document type</returns>
    Task<Result<float>> GetProcessingConfidenceAsync(
        DocumentType documentType, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents document metadata and processing configuration.
/// </summary>
public class DocumentMetadata
{
    /// <summary>
    /// Gets or sets the unique document identifier.
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original filename.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type classification.
    /// </summary>
    public DocumentType DocumentType { get; set; }

    /// <summary>
    /// Gets or sets the expected schema for extraction.
    /// </summary>
    public ExtractionSchema? ExpectedSchema { get; set; }

    /// <summary>
    /// Gets or sets the source path or location.
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets processing-specific options.
    /// </summary>
    public ProcessingOptions ProcessingOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the document creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the document modification date.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the document.
    /// </summary>
    public string MimeType { get; set; } = string.Empty;
}

/// <summary>
/// Represents the complete result of document processing.
/// </summary>
public class DocumentProcessingResult
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extraction method used.
    /// </summary>
    public ExtractionMethod ExtractionMethod { get; set; }

    /// <summary>
    /// Gets or sets the extracted text content.
    /// </summary>
    public string ExtractedText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extracted fields with their values.
    /// </summary>
    public Dictionary<string, object> ExtractedFields { get; set; } = new();

    /// <summary>
    /// Gets or sets the validation results.
    /// </summary>
    public List<string> ValidationResults { get; set; } = new();

    /// <summary>
    /// Gets or sets the overall confidence score (0.0 to 1.0).
    /// </summary>
    public float OverallConfidence { get; set; }

    /// <summary>
    /// Gets or sets the processing time in milliseconds.
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the truth record identifier if stored.
    /// </summary>
    public string? TruthRecordId { get; set; }

    /// <summary>
    /// Gets or sets the data lineage identifier.
    /// </summary>
    public string? DataLineageId { get; set; }

    /// <summary>
    /// Gets or sets whether the processing was successful.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Gets or sets any error messages from processing.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Creates a failed processing result with the specified error.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed processing result.</returns>
    public static DocumentProcessingResult Failed(string error)
    {
        return new DocumentProcessingResult
        {
            IsSuccessful = false,
            Errors = new List<string> { error },
            OverallConfidence = 0.0f
        };
    }
}

/// <summary>
/// Represents the result of field extraction operations.
/// </summary>
public class ExtractionResult
{
    /// <summary>
    /// Gets or sets the extracted fields with their values and confidence scores.
    /// </summary>
    public Dictionary<string, FieldValue> ExtractedFields { get; set; } = new();

    /// <summary>
    /// Gets or sets the overall extraction confidence score.
    /// </summary>
    public float Confidence { get; set; }

    /// <summary>
    /// Gets or sets the extraction method used.
    /// </summary>
    public ExtractionMethod Method { get; set; }

    /// <summary>
    /// Gets or sets any warnings or issues encountered.
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Represents a field value with its confidence score.
/// </summary>
public class FieldValue
{
    /// <summary>
    /// Gets or sets the extracted value.
    /// </summary>
    public object Value { get; set; } = new();

    /// <summary>
    /// Gets or sets the confidence score for this field (0.0 to 1.0).
    /// </summary>
    public float Confidence { get; set; }

    /// <summary>
    /// Gets or sets the data type of the extracted value.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extraction pattern that matched this field.
    /// </summary>
    public string? ExtractedBy { get; set; }
}

/// <summary>
/// Represents extracted data from document processing.
/// </summary>
public class ExtractedData
{
    /// <summary>
    /// Gets or sets the extracted fields with their values.
    /// </summary>
    public Dictionary<string, object> Fields { get; set; } = new();

    /// <summary>
    /// Gets or sets the confidence score for the entire extraction.
    /// </summary>
    public float Confidence { get; set; }

    /// <summary>
    /// Gets or sets the extraction timestamp.
    /// </summary>
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the source document identifier.
    /// </summary>
    public string SourceDocumentId { get; set; } = string.Empty;
}

/// <summary>
/// Enumeration of document extraction methods.
/// </summary>
public enum ExtractionMethod
{
    /// <summary>
    /// Direct text extraction from digital documents.
    /// </summary>
    DirectText,

    /// <summary>
    /// Optical Character Recognition from scanned documents.
    /// </summary>
    OCR,

    /// <summary>
    /// Large Language Model assisted extraction.
    /// </summary>
    LLM,

    /// <summary>
    /// Hybrid approach using multiple methods.
    /// </summary>
    Hybrid
}

/// <summary>
/// Enumeration of supported document types.
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Unknown document type.
    /// </summary>
    Unknown,

    /// <summary>
    /// PDF document.
    /// </summary>
    PDF,

    /// <summary>
    /// Microsoft Word document.
    /// </summary>
    Word,

    /// <summary>
    /// Microsoft Excel spreadsheet.
    /// </summary>
    Excel,

    /// <summary>
    /// Plain text document.
    /// </summary>
    Text,

    /// <summary>
    /// Image document requiring OCR.
    /// </summary>
    Image,

    /// <summary>
    /// Invoice document.
    /// </summary>
    Invoice,

    /// <summary>
    /// Receipt document.
    /// </summary>
    Receipt,

    /// <summary>
    /// Contract document.
    /// </summary>
    Contract
} 