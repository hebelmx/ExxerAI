namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Contains data quality metrics for processed documents
/// </summary>
public class DataQualityMetrics
{
    /// <summary>
    /// Overall data quality score as a decimal (0.0 to 1.0)
    /// </summary>
    public decimal OverallQualityScore { get; set; }

    /// <summary>
    /// Percentage of complete records (all required fields present)
    /// </summary>
    public decimal CompletenessScore { get; set; }

    /// <summary>
    /// Percentage of accurate extractions based on validation rules
    /// </summary>
    public decimal AccuracyScore { get; set; }

    /// <summary>
    /// Percentage of consistent data across related documents
    /// </summary>
    public decimal ConsistencyScore { get; set; }
}