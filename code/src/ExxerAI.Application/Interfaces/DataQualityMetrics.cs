namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents data quality metrics for the truth system
/// </summary>
public class DataQualityMetrics
{
    /// <summary>
    /// Gets or sets the overall data quality score (0.0 - 1.0)
    /// </summary>
    public float OverallQualityScore { get; set; }

    /// <summary>
    /// Gets or sets the data completeness percentage
    /// </summary>
    public float CompletenessPercentage { get; set; }

    /// <summary>
    /// Gets or sets the data accuracy percentage
    /// </summary>
    public float AccuracyPercentage { get; set; }

    /// <summary>
    /// Gets or sets the data consistency percentage
    /// </summary>
    public float ConsistencyPercentage { get; set; }

    /// <summary>
    /// Gets or sets the number of duplicate records detected
    /// </summary>
    public int DuplicateRecords { get; set; }

    /// <summary>
    /// Gets or sets the number of validation errors
    /// </summary>
    public int ValidationErrors { get; set; }

    /// <summary>
    /// Gets or sets quality metrics by field
    /// </summary>
    public Dictionary<string, FieldQualityMetrics> FieldMetrics { get; init; } = new();
}