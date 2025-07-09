using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents a grounding report for data quality assessment
/// </summary>
public class GroundingReport
{
    /// <summary>
    /// Gets or sets the report identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the date range for the report
    /// </summary>
    public DateRange DateRange { get; set; } = new();

    /// <summary>
    /// Gets or sets the total number of records processed
    /// </summary>
    public int TotalRecordsProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of successful groundings
    /// </summary>
    public int SuccessfulGroundings { get; set; }

    /// <summary>
    /// Gets or sets the number of failed groundings
    /// </summary>
    public int FailedGroundings { get; set; }

    /// <summary>
    /// Gets or sets the average confidence score
    /// </summary>
    public float AverageConfidenceScore { get; set; }

    /// <summary>
    /// Gets or sets the number of conflicts resolved
    /// </summary>
    public int ConflictsResolved { get; set; }

    /// <summary>
    /// Gets or sets the number of records requiring human review
    /// </summary>
    public int RecordsRequiringReview { get; set; }

    /// <summary>
    /// Gets or sets detailed grounding statistics by document type
    /// </summary>
    public Dictionary<DocumentType, GroundingStatistics> StatsByDocumentType { get; init; } = new();

    /// <summary>
    /// Gets or sets when the report was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the overall success rate for groundings
    /// </summary>
    public float SuccessRate => TotalRecordsProcessed > 0 
        ? (float)SuccessfulGroundings / TotalRecordsProcessed 
        : 0.0f;
}