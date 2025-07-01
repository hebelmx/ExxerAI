using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents the result of document processing including extracted data, validation results, and confidence scores
/// </summary>
public class DocumentProcessingResult
{
    /// <summary>
    /// Gets or sets the unique identifier for the processing result
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the method used for text extraction
    /// </summary>
    public ExtractionMethod ExtractionMethod { get; set; } = ExtractionMethod.DirectText;

    /// <summary>
    /// Gets or sets the extracted raw text content
    /// </summary>
    public string ExtractedText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extracted structured fields
    /// </summary>
    public Dictionary<string, object> ExtractedFields { get; init; } = new();

    /// <summary>
    /// Gets or sets the grounded data after dictionary validation
    /// </summary>
    public ExtractedData GroundedData { get; set; } = new();

    /// <summary>
    /// Gets or sets the validation results for the extracted data
    /// </summary>
    public ValidationResult ValidationResults { get; set; } = new();

    /// <summary>
    /// Gets or sets the OCR regions that were processed
    /// </summary>
    public List<OCRRegion> OCRRegions { get; init; } = new();

    /// <summary>
    /// Gets or sets the confidence score from direct text extraction (0.0 - 1.0)
    /// </summary>
    public float Confidence { get; set; } = 0.0f;

    /// <summary>
    /// Gets or sets the confidence score from LLM processing (0.0 - 1.0)
    /// </summary>
    public float LLMConfidence { get; set; } = 0.0f;

    /// <summary>
    /// Gets or sets the confidence score from data grounding (0.0 - 1.0)
    /// </summary>
    public float GroundingConfidence { get; set; } = 0.0f;

    /// <summary>
    /// Gets the overall confidence score (weighted average)
    /// </summary>
    public float OverallConfidence => 
        (Confidence * 0.4f + LLMConfidence * 0.4f + GroundingConfidence * 0.2f);

    /// <summary>
    /// Gets or sets the ID of the truth record if stored
    /// </summary>
    public string? TruthRecordId { get; set; }

    /// <summary>
    /// Gets or sets the schema learning results from this processing
    /// </summary>
    public LearningResult SchemaLearningResults { get; set; } = new();

    /// <summary>
    /// Gets or sets the processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the data lineage ID for audit tracking
    /// </summary>
    public string? DataLineageId { get; set; }

    /// <summary>
    /// Gets or sets error information if processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets whether the processing was successful
    /// </summary>
    public bool IsSuccessful => string.IsNullOrEmpty(ErrorMessage) && OverallConfidence > 0.5f;

    /// <summary>
    /// Creates a failed processing result with an error message
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <returns>A failed DocumentProcessingResult</returns>
    public static DocumentProcessingResult Failed(string errorMessage)
    {
        return new DocumentProcessingResult
        {
            ErrorMessage = errorMessage,
            Confidence = 0.0f,
            LLMConfidence = 0.0f,
            GroundingConfidence = 0.0f
        };
    }

    /// <summary>
    /// Converts the processing result to learning feedback for schema improvement
    /// </summary>
    /// <returns>ExtractionFeedback for schema learning</returns>
    public ExtractionFeedback ToLearningFeedback()
    {
        return new ExtractionFeedback
        {
            DocumentId = DocumentId,
            ExtractionMethod = ExtractionMethod,
            SuccessfulFields = ExtractedFields.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            FailedFields = ExtractedFields.Where(kvp => kvp.Value == null).Select(kvp => kvp.Key).ToList(),
            OverallConfidence = OverallConfidence,
            ProcessingTimeMs = ProcessingTimeMs,
            ValidationPassed = ValidationResults.IsValid
        };
    }
}

/// <summary>
/// Represents the methods available for text extraction from documents
/// </summary>
public enum ExtractionMethod
{
    /// <summary>
    /// Direct text extraction from digital documents
    /// </summary>
    DirectText,
    
    /// <summary>
    /// Optical Character Recognition for scanned documents
    /// </summary>
    OCR,
    
    /// <summary>
    /// LLM-assisted extraction
    /// </summary>
    LLMAssisted,
    
    /// <summary>
    /// Hybrid approach using multiple methods
    /// </summary>
    Hybrid
}

/// <summary>
/// Represents extracted data with field mappings and confidence scores
/// </summary>
public class ExtractedData
{
    /// <summary>
    /// Gets or sets the extracted field values
    /// </summary>
    public Dictionary<string, object> Fields { get; init; } = new();

    /// <summary>
    /// Gets or sets the confidence scores for each field
    /// </summary>
    public Dictionary<string, float> FieldConfidences { get; init; } = new();

    /// <summary>
    /// Gets or sets the source regions where each field was found
    /// </summary>
    public Dictionary<string, string> FieldSources { get; init; } = new();

    /// <summary>
    /// Gets or sets additional metadata about the extraction
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Gets the overall confidence for all extracted fields
    /// </summary>
    public float OverallConfidence => 
        FieldConfidences.Values.DefaultIfEmpty(0.0f).Average();
}

/// <summary>
/// Represents validation results for extracted data
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets whether the data passed validation
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of validation errors
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Gets or sets the list of validation warnings
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Gets or sets field-specific validation results
    /// </summary>
    public Dictionary<string, FieldValidationResult> FieldResults { get; init; } = new();

    /// <summary>
    /// Gets or sets the validation confidence score
    /// </summary>
    public float Confidence { get; set; } = 1.0f;
}

/// <summary>
/// Represents validation results for a specific field
/// </summary>
public class FieldValidationResult
{
    /// <summary>
    /// Gets or sets whether the field passed validation
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Gets or sets the field-specific error message
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the confidence score for this field validation
    /// </summary>
    public float Confidence { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the suggested correction for invalid fields
    /// </summary>
    public string? SuggestedCorrection { get; set; }
}

/// <summary>
/// Represents an OCR region that was processed
/// </summary>
public class OCRRegion
{
    /// <summary>
    /// Gets or sets the region identifier
    /// </summary>
    public string RegionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extracted text from this region
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence score for this region
    /// </summary>
    public float Confidence { get; set; } = 0.0f;

    /// <summary>
    /// Gets or sets the bounding box coordinates
    /// </summary>
    public BoundingBox BoundingBox { get; set; } = new();

    /// <summary>
    /// Gets or sets the fields found in this region
    /// </summary>
    public List<string> FieldsFound { get; init; } = new();
}

/// <summary>
/// Represents a bounding box for OCR regions
/// </summary>
public class BoundingBox
{
    /// <summary>
    /// Gets or sets the X coordinate
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets the width
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height
    /// </summary>
    public int Height { get; set; }
}

/// <summary>
/// Represents learning results from document processing
/// </summary>
public class LearningResult
{
    /// <summary>
    /// Gets or sets whether new patterns were learned
    /// </summary>
    public bool PatternsLearned { get; set; } = false;

    /// <summary>
    /// Gets or sets the list of new patterns discovered
    /// </summary>
    public List<string> NewPatterns { get; init; } = new();

    /// <summary>
    /// Gets or sets the confidence improvement from learning
    /// </summary>
    public float ConfidenceImprovement { get; set; } = 0.0f;

    /// <summary>
    /// Gets or sets the schema updates made
    /// </summary>
    public List<string> SchemaUpdates { get; init; } = new();
}

/// <summary>
/// Represents feedback for schema learning from processing results
/// </summary>
public class ExtractionFeedback
{
    /// <summary>
    /// Gets or sets the document ID
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extraction method used
    /// </summary>
    public ExtractionMethod ExtractionMethod { get; set; } = ExtractionMethod.DirectText;

    /// <summary>
    /// Gets or sets the successfully extracted fields
    /// </summary>
    public Dictionary<string, object> SuccessfulFields { get; init; } = new();

    /// <summary>
    /// Gets or sets the fields that failed extraction
    /// </summary>
    public List<string> FailedFields { get; init; } = new();

    /// <summary>
    /// Gets or sets the overall confidence of the extraction
    /// </summary>
    public float OverallConfidence { get; set; } = 0.0f;

    /// <summary>
    /// Gets or sets the processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets whether validation passed
    /// </summary>
    public bool ValidationPassed { get; set; } = true;
} 