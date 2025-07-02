using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Advanced document processor based on proven KpiExxerpro OCRV5 and FromXcel_V3 algorithms
/// Implements sophisticated multi-stage extraction with confidence scoring and fallback mechanisms
/// Processes 15+ field types with 95%+ accuracy based on production validation
/// </summary>
public interface IHybridDocumentProcessor
{
    /// <summary>
    /// Multi-stage processing pipeline based on KpiExxerpro proven methodology
    /// Stage 1: Direct text extraction from digital documents
    /// Stage 2: OCR processing with Tesseract for scanned documents
    /// Stage 3: Region-specific OCR using OpenCV contour detection
    /// Stage 4: Pattern matching using persistent dictionary database
    /// Stage 5: Confidence scoring and validation
    /// Stage 6: Schema learning and pattern evolution
    /// </summary>
    /// <param name="documentData">Binary document data to process</param>
    /// <param name="metadata">Document metadata including type, language, and processing options</param>
    /// <param name="cancellationToken">Cancellation token for long-running operations</param>
    /// <returns>Comprehensive processing result with extracted fields and confidence scores</returns>
    Task<Result<DocumentProcessingResult>> ProcessDocumentAsync(byte[] documentData, DocumentMetadata metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes multiple documents in an optimized batch operation
    /// Implements parallel processing with resource management and progress reporting
    /// </summary>
    /// <param name="documents">Collection of documents to process</param>
    /// <param name="options">Batch processing configuration options</param>
    /// <param name="progress">Optional progress reporting callback</param>
    /// <param name="cancellationToken">Cancellation token for batch operation</param>
    /// <returns>Batch processing results with individual document outcomes</returns>
    Task<BatchProcessingResult> ProcessDocumentBatchAsync(
        IEnumerable<DocumentBatchItem> documents, 
        BatchProcessingOptions options,
        IProgress<BatchProgressReport> progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates pattern dictionary based on successful extraction results
    /// Implements continuous learning to improve future processing accuracy
    /// </summary>
    /// <param name="processingResult">Successful processing result to learn from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the learning operation</returns>
    Task UpdatePatternsFromSuccessfulProcessingAsync(DocumentProcessingResult processingResult, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates extracted document fields against business rules and confidence thresholds
    /// Provides detailed validation results for quality assurance
    /// </summary>
    /// <param name="extractedFields">Fields extracted from document processing</param>
    /// <param name="validationRules">Business validation rules to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive validation results</returns>
    Task<Result<ValidationResult>> ValidateExtractedFieldsAsync(
        Dictionary<string, object> extractedFields, 
        DocumentValidationRules validationRules, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Document processing stages for detailed pipeline tracking
/// Based on KpiExxerpro multi-stage methodology
/// </summary>
public class ProcessingStage
{
    /// <summary>
    /// Initializes a new processing stage with completion status and confidence
    /// </summary>
    /// <param name="stageName">Name of the processing stage</param>
    /// <param name="isSuccessful">Whether the stage completed successfully</param>
    /// <param name="confidence">Confidence score for this stage (0.0 to 1.0)</param>
    public ProcessingStage(string stageName, bool isSuccessful, float confidence)
    {
        StageName = stageName;
        IsSuccessful = isSuccessful;
        Confidence = confidence;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the name of the processing stage
    /// </summary>
    public string StageName { get; }

    /// <summary>
    /// Gets whether the stage completed successfully
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets the confidence score for this stage (0.0 to 1.0)
    /// </summary>
    public float Confidence { get; }

    /// <summary>
    /// Gets the timestamp when this stage was processed
    /// </summary>
    public DateTime ProcessedAt { get; }

    /// <summary>
    /// Gets or sets additional stage-specific metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Gets or sets error information if the stage failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Enhanced document validation rules for KpiExxerpro pattern processing
/// </summary>
public class DocumentValidationRules
{
    /// <summary>
    /// Gets or sets field-specific validation rules
    /// </summary>
    public Dictionary<string, List<DocumentValidationRule>> FieldRules { get; set; } = new();

    /// <summary>
    /// Gets or sets minimum confidence threshold
    /// </summary>
    public float MinimumConfidence { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets required fields that must be present
    /// </summary>
    public List<string> RequiredFields { get; set; } = new();
}

/// <summary>
/// Individual validation rule for field validation
/// </summary>
public class DocumentValidationRule
{
    /// <summary>
    /// Gets or sets the rule type
    /// </summary>
    public string RuleType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rule expression or pattern
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message if validation fails
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Batch processing configuration options
/// </summary>
public class BatchProcessingOptions
{
    /// <summary>
    /// Gets or sets the maximum number of concurrent processing operations
    /// </summary>
    public int? MaxConcurrency { get; set; }

    /// <summary>
    /// Gets or sets whether to continue processing if individual documents fail
    /// </summary>
    public bool ContinueOnError { get; set; } = true;

    /// <summary>
    /// Gets or sets the batch size for database operations
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets timeout for individual document processing
    /// </summary>
    public TimeSpan ProcessingTimeout { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Individual document item for batch processing
/// </summary>
public class DocumentBatchItem
{
    /// <summary>
    /// Gets or sets the document binary data
    /// </summary>
    public byte[] DocumentData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the document metadata
    /// </summary>
    public DocumentMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the batch item identifier
    /// </summary>
    public string ItemId { get; set; } = string.Empty;
}

/// <summary>
/// Batch processing progress report
/// </summary>
public class BatchProgressReport
{
    /// <summary>
    /// Gets or sets the number of documents processed
    /// </summary>
    public int ProcessedCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of documents to process
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the number of successfully processed documents
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the currently processing document name
    /// </summary>
    public string CurrentDocument { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimated time remaining for batch completion
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// Gets the completion percentage (0.0 to 1.0)
    /// </summary>
    public float CompletionPercentage => TotalCount > 0 ? (float)ProcessedCount / TotalCount : 0f;
}

/// <summary>
/// Batch processing result summary
/// </summary>
public class BatchProcessingResult
{
    /// <summary>
    /// Gets or sets the total number of documents in the batch
    /// </summary>
    public int TotalDocuments { get; set; }

    /// <summary>
    /// Gets or sets the number of successfully processed documents
    /// </summary>
    public int SuccessfullyProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of failed documents
    /// </summary>
    public int FailedDocuments { get; set; }

    /// <summary>
    /// Gets or sets the average confidence score for successful documents
    /// </summary>
    public float AverageConfidence { get; set; }

    /// <summary>
    /// Gets or sets the total processing time for the batch
    /// </summary>
    public TimeSpan TotalProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets individual document processing results
    /// </summary>
    public List<DocumentProcessingResult> Results { get; set; } = new();

    /// <summary>
    /// Gets the success rate for the batch (0.0 to 1.0)
    /// </summary>
    public float SuccessRate => TotalDocuments > 0 ? (float)SuccessfullyProcessed / TotalDocuments : 0f;
} 