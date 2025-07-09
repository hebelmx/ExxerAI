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
    public Dictionary<string, object> ExtractedFields { get; init; } = [];

    /// <summary>
    /// Gets or sets the grounded data after dictionary validation
    /// </summary>
    public ExtractedData GroundedData { get; set; } = new();

    /// <summary>
    /// Gets or sets the validation results for the extracted data
    /// </summary>
    public ValidationResultDocument ValidationResultDocument { get; set; } = new();

    /// <summary>
    /// Gets or sets the OCR regions that were processed
    /// </summary>
    public List<OCRRegion> OCRRegions { get; init; } = [];

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
            ValidationPassed = ValidationResultDocument.IsValid
        };
    }
}