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