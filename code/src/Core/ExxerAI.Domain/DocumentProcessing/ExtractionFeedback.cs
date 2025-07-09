namespace ExxerAI.Domain.DocumentProcessing;

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