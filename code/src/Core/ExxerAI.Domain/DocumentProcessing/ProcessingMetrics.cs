namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents processing metrics for a history period.
/// </summary>
public class ProcessingMetrics
{
    /// <summary>
    /// Gets or sets the total number of documents processed.
    /// </summary>
    public int TotalDocuments { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of successfully processed documents.
    /// </summary>
    public int SuccessfulDocuments { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of failed document processing attempts.
    /// </summary>
    public int FailedDocuments { get; set; } = 0;

    /// <summary>
    /// Gets or sets the overall success rate (0.0 to 1.0).
    /// </summary>
    public float SuccessRate { get; set; } = 0f;

    /// <summary>
    /// Gets or sets the average confidence score.
    /// </summary>
    public float AverageConfidence { get; set; } = 0f;

    /// <summary>
    /// Gets or sets the average processing time in milliseconds.
    /// </summary>
    public double AverageProcessingTime { get; set; } = 0;

    /// <summary>
    /// Gets or sets the distribution of confidence scores.
    /// </summary>
    public Dictionary<string, int> ConfidenceDistribution { get; set; } = new();

    /// <summary>
    /// Gets or sets additional metric properties.
    /// </summary>
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
}