namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents extracted data with field mappings and confidence scores
/// </summary>
public class ExtractedData
{
    /// <summary>
    /// Gets or sets the document identifier
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extracted field values
    /// </summary>
    public Dictionary<string, object> ExtractedFields { get; set; } = new();

    /// <summary>
    /// Gets or sets the overall confidence score for the extraction
    /// </summary>
    public float ConfidenceScore { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets when the data was processed
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the extracted field values (legacy property)
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
        FieldConfidences.Values.DefaultIfEmpty(ConfidenceScore).Average();
}