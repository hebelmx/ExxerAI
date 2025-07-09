namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Contains data quality metrics for processed documents
/// </summary>
public class DataQualityMetrics
{
    /// <summary>
    /// Gets or sets the start date of the metrics period
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the metrics period
    /// </summary>
    public DateTime ToDate { get; set; }

    /// <summary>
    /// Gets or sets the total number of records
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Gets or sets the number of high quality records
    /// </summary>
    public int HighQualityRecords { get; set; }

    /// <summary>
    /// Gets or sets the number of medium quality records
    /// </summary>
    public int MediumQualityRecords { get; set; }

    /// <summary>
    /// Gets or sets the number of low quality records
    /// </summary>
    public int LowQualityRecords { get; set; }

    /// <summary>
    /// Gets or sets the average confidence score
    /// </summary>
    public float AverageConfidence { get; set; }

    /// <summary>
    /// Gets or sets the data completeness score (0.0 - 1.0)
    /// </summary>
    public float DataCompleteness { get; set; }

    /// <summary>
    /// Gets or sets the data accuracy score (0.0 - 1.0)
    /// </summary>
    public float DataAccuracy { get; set; }

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