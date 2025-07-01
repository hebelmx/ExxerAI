using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for the central system that maintains the definitive version of all business data
/// All extracted and processed information flows through this system for validation
/// Implements audit trail, conflict resolution, and data lineage tracking
/// </summary>
public interface IPrimarySourceOfTruthSystem
{
    /// <summary>
    /// Stores extracted data as an authoritative truth record
    /// </summary>
    /// <param name="data">The extracted data to store</param>
    /// <param name="source">Information about the data source</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the storage operation containing the truth record</returns>
    Task<Result<TruthRecord>> StoreExtractedDataAsync(
        ExtractedData data, 
        DataSource source, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates extracted data against existing truth records
    /// </summary>
    /// <param name="data">The data to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the validation operation</returns>
    Task<Result<ValidationResult>> ValidateAgainstTruthAsync(
        ExtractedData data, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an authoritative truth record by its identifier
    /// </summary>
    /// <param name="recordId">The truth record identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the retrieval operation containing the truth record</returns>
    Task<Result<TruthRecord>> GetAuthoritativeRecordAsync(
        string recordId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves conflicts when multiple data sources provide conflicting information
    /// </summary>
    /// <param name="conflictingData">The conflicting data from multiple sources</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the conflict resolution operation</returns>
    Task<Result<ConflictResolution>> ResolveDataConflictAsync(
        IEnumerable<ExtractedData> conflictingData, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the data lineage for a specific truth record
    /// </summary>
    /// <param name="recordId">The truth record identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the data lineage</returns>
    Task<Result<DataLineage>> GetDataLineageAsync(
        string recordId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a grounding report for data quality assessment
    /// </summary>
    /// <param name="fromDate">The start date for the report</param>
    /// <param name="toDate">The end date for the report</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the grounding report</returns>
    Task<Result<GroundingReport>> GenerateGroundingReportAsync(
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds similar truth records based on data content
    /// </summary>
    /// <param name="data">The data to compare against</param>
    /// <param name="similarityThreshold">The minimum similarity threshold (0.0 - 1.0)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing similar truth records</returns>
    Task<Result<IEnumerable<TruthRecord>>> FindSimilarRecordsAsync(
        ExtractedData data, 
        float similarityThreshold = 0.85f, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing truth record with new information
    /// </summary>
    /// <param name="recordId">The truth record identifier</param>
    /// <param name="updatedData">The updated data</param>
    /// <param name="updatedBy">Who is updating the record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the update operation</returns>
    Task<Result<TruthRecord>> UpdateTruthRecordAsync(
        string recordId, 
        ExtractedData updatedData, 
        string updatedBy, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a truth record as requiring human review
    /// </summary>
    /// <param name="recordId">The truth record identifier</param>
    /// <param name="reason">The reason for requiring review</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<ExxerAI.Domain.Result<bool>> RequireHumanReviewAsync(
        string recordId, 
        string reason, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all truth records that require human review
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing truth records requiring review</returns>
    Task<Result<IEnumerable<TruthRecord>>> GetRecordsRequiringReviewAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets data quality metrics for the truth system
    /// </summary>
    /// <param name="fromDate">The start date for metrics</param>
    /// <param name="toDate">The end date for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing quality metrics</returns>
    Task<Result<DataQualityMetrics>> GetDataQualityMetricsAsync(
        DateTime fromDate, 
        DateTime toDate, 
        CancellationToken cancellationToken = default);
}

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

/// <summary>
/// Represents grounding statistics for a specific document type
/// </summary>
public class GroundingStatistics
{
    /// <summary>
    /// Gets or sets the document type
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the total number of documents processed
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of successful extractions
    /// </summary>
    public int SuccessfulExtractions { get; set; }

    /// <summary>
    /// Gets or sets the average processing time in milliseconds
    /// </summary>
    public long AverageProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the average confidence score
    /// </summary>
    public float AverageConfidence { get; set; }

    /// <summary>
    /// Gets or sets field-specific extraction statistics
    /// </summary>
    public Dictionary<string, FieldStatistics> FieldStats { get; init; } = new();
}

/// <summary>
/// Represents extraction statistics for a specific field
/// </summary>
public class FieldStatistics
{
    /// <summary>
    /// Gets or sets the field name
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extraction success rate for this field
    /// </summary>
    public float SuccessRate { get; set; }

    /// <summary>
    /// Gets or sets the average confidence score for this field
    /// </summary>
    public float AverageConfidence { get; set; }

    /// <summary>
    /// Gets or sets the most common extraction patterns used
    /// </summary>
    public List<string> CommonPatterns { get; init; } = new();
}

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

/// <summary>
/// Represents quality metrics for a specific field
/// </summary>
public class FieldQualityMetrics
{
    /// <summary>
    /// Gets or sets the field name
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the completeness rate for this field
    /// </summary>
    public float CompletenessRate { get; set; }

    /// <summary>
    /// Gets or sets the accuracy rate for this field
    /// </summary>
    public float AccuracyRate { get; set; }

    /// <summary>
    /// Gets or sets the number of validation errors for this field
    /// </summary>
    public int ValidationErrors { get; set; }
}

/// <summary>
/// Represents a date range for reports and queries
/// </summary>
public class DateRange
{
    /// <summary>
    /// Gets or sets the start date
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// Gets or sets the end date
    /// </summary>
    public DateTime ToDate { get; set; }

    /// <summary>
    /// Gets the duration of the date range
    /// </summary>
    public TimeSpan Duration => ToDate - FromDate;
} 