namespace ExxerAI.Domain.DocumentProcessing;

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