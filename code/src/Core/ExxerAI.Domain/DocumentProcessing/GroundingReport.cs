namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Contains grounding report data for document processing validation
/// </summary>
public class GroundingReport
{
    /// <summary>
    /// Gets or sets the start date of the report period
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the report period
    /// </summary>
    public DateTime ToDate { get; set; }

    /// <summary>
    /// Gets or sets the total number of records processed
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Gets or sets the number of valid records
    /// </summary>
    public int ValidRecords { get; set; }

    /// <summary>
    /// Gets or sets the number of invalid records
    /// </summary>
    public int InvalidRecords { get; set; }

    /// <summary>
    /// Gets or sets the average confidence score across all records
    /// </summary>
    public float AverageConfidence { get; set; }

    /// <summary>
    /// Gets or sets when the report was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

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