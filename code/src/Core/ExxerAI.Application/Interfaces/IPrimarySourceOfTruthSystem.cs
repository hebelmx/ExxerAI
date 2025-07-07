using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Domain.Operations;

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
    Task<Result<ExxerAI.Domain.DocumentProcessing.ValidationResult>> ValidateAgainstTruthAsync(
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
    Task<Result<ExxerAI.Domain.DocumentProcessing.GroundingReport>> GenerateGroundingReportAsync(
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
    Task<Result<bool>> RequireHumanReviewAsync(
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
    Task<Result<ExxerAI.Domain.DocumentProcessing.DataQualityMetrics>> GetDataQualityMetricsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}