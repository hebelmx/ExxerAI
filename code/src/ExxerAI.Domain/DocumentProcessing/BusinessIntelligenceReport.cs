namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a comprehensive business intelligence report generated from processed documents
/// </summary>
public class BusinessIntelligenceReport
{
    /// <summary>
    /// The reporting period for this business intelligence report
    /// </summary>
    public DateRange ReportPeriod { get; set; } = new();

    /// <summary>
    /// The grounding report containing data validation and truth verification results
    /// </summary>
    public GroundingReport GroundingReport { get; set; } = new();

    /// <summary>
    /// Value quality metrics for the reporting period
    /// </summary>
    public DataQualityMetrics QualityMetrics { get; set; } = new();

    /// <summary>
    /// Timestamp when this report was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Name of the system or service that generated this report
    /// </summary>
    public string GeneratedBy { get; set; } = string.Empty;

    /// <summary>
    /// Business insights extracted from the data analysis
    /// </summary>
    public List<string> Insights { get; set; } = new();

    /// <summary>
    /// Recommended actions based on the analysis results
    /// </summary>
    public List<string> RecommendedActions { get; set; } = new();
}

/// <summary>
/// Represents a date range for reporting periods
/// </summary>
public class DateRange
{
    /// <summary>
    /// Start date of the range (inclusive)
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// End date of the range (inclusive)
    /// </summary>
    public DateTime ToDate { get; set; }
}

/// <summary>
/// Contains grounding report data for document processing validation
/// </summary>
public class GroundingReport
{
    /// <summary>
    /// Total number of records processed during the reporting period
    /// </summary>
    public int TotalRecordsProcessed { get; set; }

    /// <summary>
    /// Success rate as a decimal (0.0 to 1.0)
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// Average confidence score across all processed documents
    /// </summary>
    public decimal AverageConfidenceScore { get; set; }

    /// <summary>
    /// Number of data conflicts that were automatically resolved
    /// </summary>
    public int ConflictsResolved { get; set; }

    /// <summary>
    /// Number of records that require manual review
    /// </summary>
    public int RecordsRequiringReview { get; set; }
}

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