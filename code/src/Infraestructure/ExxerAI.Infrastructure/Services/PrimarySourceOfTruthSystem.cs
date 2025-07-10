using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ExxerAI.Infrastructure.Services;

/// <summary>
/// Implementation of the primary source of truth system for maintaining authoritative business data
/// Implements audit trail, conflict resolution, and data lineage tracking using modern C# patterns
/// </summary>
public class PrimarySourceOfTruthSystem : IPrimarySourceOfTruthSystem
{
    private readonly ILogger<PrimarySourceOfTruthSystem> _logger;
    private readonly ConcurrentDictionary<string, TruthRecord> _truthRecords;
    private readonly ConcurrentDictionary<string, DataLineage> _dataLineages;
    private readonly ConcurrentDictionary<string, ConflictResolution> _conflictResolutions;
    private readonly SemaphoreSlim _writeLock;

    /// <summary>
    /// Initializes a new instance of the PrimarySourceOfTruthSystem
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public PrimarySourceOfTruthSystem(ILogger<PrimarySourceOfTruthSystem> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _truthRecords = new ConcurrentDictionary<string, TruthRecord>();
        _dataLineages = new ConcurrentDictionary<string, DataLineage>();
        _conflictResolutions = new ConcurrentDictionary<string, ConflictResolution>();
        _writeLock = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Stores extracted data as an authoritative truth record
    /// </summary>
    /// <param name="data">The extracted data to store</param>
    /// <param name="source">Information about the data source</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the storage operation containing the truth record</returns>
    public async Task<Result<TruthRecord>> StoreExtractedDataAsync(
        ExtractedData data,
        DataSource source,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<TruthRecord>();

        try
        {
            if (data is null)
            {
                _logger.LogWarning("Attempted to store null extracted data");
                return Result<TruthRecord>.WithFailure("Value cannot be null");
            }

            if (source is null)
            {
                _logger.LogWarning("Attempted to store data with null data source");
                return Result<TruthRecord>.WithFailure("Value source cannot be null");
            }

            await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var truthRecord = await CreateTruthRecordAsync(data, source, cancellationToken).ConfigureAwait(false);
                var lineage = CreateDataLineage(truthRecord, source);

                _truthRecords.TryAdd(truthRecord.Id, truthRecord);
                _dataLineages.TryAdd(lineage.Id, lineage);

                _logger.LogInformation("Successfully stored truth record {TruthRecordId} from source {SourceType}:{SourceId}",
                    truthRecord.Id, source.Type, source.Id);

                return Result<TruthRecord>.Success(truthRecord);
            }
            finally
            {
                _writeLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Store operation was cancelled");
            return ResultExtensions.Cancelled<TruthRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing extracted data from source {SourceType}:{SourceId}",
                source?.Type, source?.Id);
            return Result<TruthRecord>.WithFailure($"Failed to store data: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates extracted data against existing truth records
    /// </summary>
    /// <param name="data">The data to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the validation operation</returns>
    public async Task<Result<ExxerAI.Domain.DocumentProcessing.ValidationResult>> ValidateAgainstTruthAsync(
        ExtractedData data,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<ExxerAI.Domain.DocumentProcessing.ValidationResult>();

        try
        {
            if (data is null)
            {
                _logger.LogWarning("Attempted to validate null extracted data");
                return Result<ExxerAI.Domain.DocumentProcessing.ValidationResult>.WithFailure("Value cannot be null");
            }

            var validationResult = await PerformValidationAsync(data, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Validation completed for document {DocumentId} with result: {IsValid}",
                data.DocumentId, validationResult.IsValid);

            return Result<ExxerAI.Domain.DocumentProcessing.ValidationResult>.Success(validationResult);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Validation operation was cancelled");
            return ResultExtensions.Cancelled<ExxerAI.Domain.DocumentProcessing.ValidationResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating data for document {DocumentId}", data?.DocumentId);
            return Result<ExxerAI.Domain.DocumentProcessing.ValidationResult>.WithFailure($"Validation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets an authoritative truth record by its identifier
    /// </summary>
    /// <param name="recordId">The truth record identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the retrieval operation containing the truth record</returns>
    public async Task<Result<TruthRecord>> GetAuthoritativeRecordAsync(
        string recordId,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<TruthRecord>();

        try
        {
            if (string.IsNullOrWhiteSpace(recordId))
            {
                _logger.LogWarning("Attempted to get truth record with empty ID");
                return Result<TruthRecord>.WithFailure("Record ID cannot be empty");
            }

            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            if (_truthRecords.TryGetValue(recordId, out var record))
            {
                _logger.LogInformation("Retrieved truth record {RecordId}", recordId);
                return Result<TruthRecord>.Success(record);
            }

            _logger.LogWarning("Truth record {RecordId} not found", recordId);
            return Result<TruthRecord>.WithFailure("Record not found");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get record operation was cancelled");
            return ResultExtensions.Cancelled<TruthRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving truth record {RecordId}", recordId);
            return Result<TruthRecord>.WithFailure($"Failed to retrieve record: {ex.Message}");
        }
    }

    /// <summary>
    /// Resolves conflicts when multiple data sources provide conflicting information
    /// </summary>
    /// <param name="conflictingData">The conflicting data from multiple sources</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the conflict resolution operation</returns>
    public async Task<Result<ConflictResolution>> ResolveDataConflictAsync(
        IEnumerable<ExtractedData> conflictingData,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<ConflictResolution>();

        try
        {
            if (conflictingData is null || !conflictingData.Any())
            {
                _logger.LogWarning("Attempted to resolve conflicts with no conflicting data");
                return Result<ConflictResolution>.WithFailure("No conflicting data provided");
            }

            var resolution = await PerformConflictResolutionAsync(conflictingData, cancellationToken).ConfigureAwait(false);
            
            _conflictResolutions.TryAdd(resolution.Id, resolution);

            _logger.LogInformation("Resolved conflict {ConflictId} using strategy {Strategy}",
                resolution.Id, resolution.Strategy);

            return Result<ConflictResolution>.Success(resolution);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Conflict resolution operation was cancelled");
            return ResultExtensions.Cancelled<ConflictResolution>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving data conflict");
            return Result<ConflictResolution>.WithFailure($"Conflict resolution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the data lineage for a specific truth record
    /// </summary>
    /// <param name="recordId">The truth record identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the data lineage</returns>
    public async Task<Result<DataLineage>> GetDataLineageAsync(
        string recordId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recordId))
            {
                _logger.LogWarning("Attempted to get data lineage with empty record ID");
                return Result<DataLineage>.WithFailure("Record ID cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            var lineage = _dataLineages.Values.FirstOrDefault(l => l.RecordId == recordId);
            if (lineage is not null)
            {
                _logger.LogInformation("Retrieved data lineage for record {RecordId}", recordId);
                return Result<DataLineage>.Success(lineage);
            }

            _logger.LogWarning("Value lineage for record {RecordId} not found", recordId);
            return Result<DataLineage>.WithFailure("Value lineage not found");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get lineage operation was cancelled");
            return ResultExtensions.Cancelled<DataLineage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data lineage for record {RecordId}", recordId);
            return Result<DataLineage>.WithFailure($"Failed to retrieve lineage: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a grounding report for data quality assessment
    /// </summary>
    /// <param name="fromDate">The start date for the report</param>
    /// <param name="toDate">The end date for the report</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing the grounding report</returns>
    public async Task<Result<ExxerAI.Domain.DocumentProcessing.GroundingReport>> GenerateGroundingReportAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (toDate <= fromDate)
            {
                _logger.LogWarning("Invalid date range: from {FromDate} to {ToDate}", fromDate, toDate);
                return Result<ExxerAI.Domain.DocumentProcessing.GroundingReport>.WithFailure("Invalid date range");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var report = await GenerateReportAsync(fromDate, toDate, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Generated grounding report for period {FromDate} to {ToDate} with {TotalRecords} records",
                fromDate, toDate, report.TotalRecords);

            return Result<ExxerAI.Domain.DocumentProcessing.GroundingReport>.Success(report);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Report generation operation was cancelled");
            return Result<ExxerAI.Domain.DocumentProcessing.GroundingReport>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating grounding report for period {FromDate} to {ToDate}", fromDate, toDate);
            return Result<ExxerAI.Domain.DocumentProcessing.GroundingReport>.WithFailure($"Report generation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Finds similar truth records based on data content
    /// </summary>
    /// <param name="data">The data to compare against</param>
    /// <param name="similarityThreshold">The minimum similarity threshold (0.0 - 1.0)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing similar truth records</returns>
    public async Task<Result<IEnumerable<TruthRecord>>> FindSimilarRecordsAsync(
        ExtractedData data,
        float similarityThreshold = 0.85f,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (data is null)
            {
                _logger.LogWarning("Attempted to find similar records with null data");
                return Result<IEnumerable<TruthRecord>>.WithFailure("Value cannot be null");
            }

            if (similarityThreshold is < 0.0f or > 1.0f || float.IsNaN(similarityThreshold))
            {
                _logger.LogWarning("Invalid similarity threshold: {Threshold}", similarityThreshold);
                return Result<IEnumerable<TruthRecord>>.WithFailure("Invalid similarity threshold");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var similarRecords = await FindSimilarRecordsInternalAsync(data, similarityThreshold, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Found {Count} similar records for document {DocumentId} with threshold {Threshold}",
                similarRecords.Count(), data.DocumentId, similarityThreshold);

            return Result<IEnumerable<TruthRecord>>.Success(similarRecords);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Find similar records operation was cancelled");
            return Result<IEnumerable<TruthRecord>>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar records for document {DocumentId}", data?.DocumentId);
            return Result<IEnumerable<TruthRecord>>.WithFailure($"Failed to find similar records: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an existing truth record with new information
    /// </summary>
    /// <param name="recordId">The truth record identifier</param>
    /// <param name="updatedData">The updated data</param>
    /// <param name="updatedBy">Who is updating the record</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the update operation</returns>
    public async Task<Result<TruthRecord>> UpdateTruthRecordAsync(
        string recordId,
        ExtractedData updatedData,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recordId))
            {
                _logger.LogWarning("Attempted to update truth record with empty ID");
                return Result<TruthRecord>.WithFailure("Record ID cannot be empty");
            }

            if (updatedData is null)
            {
                _logger.LogWarning("Attempted to update truth record {RecordId} with null data", recordId);
                return Result<TruthRecord>.WithFailure("Updated data cannot be null");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_truthRecords.TryGetValue(recordId, out var existingRecord))
                {
                    _logger.LogWarning("Truth record {RecordId} not found for update", recordId);
                    return Result<TruthRecord>.WithFailure("Record not found");
                }

                var updatedRecord = await UpdateRecordAsync(existingRecord, updatedData, updatedBy, cancellationToken).ConfigureAwait(false);
                _truthRecords.TryUpdate(recordId, updatedRecord, existingRecord);

                _logger.LogInformation("Updated truth record {RecordId} by {UpdatedBy}", recordId, updatedBy);
                return Result<TruthRecord>.Success(updatedRecord);
            }
            finally
            {
                _writeLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Update record operation was cancelled");
            return Result<TruthRecord>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating truth record {RecordId}", recordId);
            return Result<TruthRecord>.WithFailure($"Failed to update record: {ex.Message}");
        }
    }

    /// <summary>
    /// Marks a truth record as requiring human review
    /// </summary>
    /// <param name="recordId">The truth record identifier</param>
    /// <param name="reason">The reason for requiring review</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> RequireHumanReviewAsync(
        string recordId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recordId))
            {
                _logger.LogWarning("Attempted to require human review with empty record ID");
                return Result<bool>.WithFailure("Record ID cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                _logger.LogWarning("Attempted to require human review without reason for record {RecordId}", recordId);
                return Result<bool>.WithFailure("Review reason cannot be empty");
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!_truthRecords.TryGetValue(recordId, out var record))
                {
                    _logger.LogWarning("Truth record {RecordId} not found for human review", recordId);
                    return Result<bool>.WithFailure("Record not found");
                }

                record.RequireHumanReview(reason);
                _truthRecords.TryUpdate(recordId, record, record);

                _logger.LogInformation("Marked truth record {RecordId} for human review: {Reason}", recordId, reason);
                return Result<bool>.Success(true);
            }
            finally
            {
                _writeLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Require human review operation was cancelled");
            return Result<bool>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requiring human review for record {RecordId}", recordId);
            return Result<bool>.WithFailure($"Failed to require human review: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all truth records that require human review
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing truth records requiring review</returns>
    public async Task<Result<IEnumerable<TruthRecord>>> GetRecordsRequiringReviewAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

            var reviewRecords = _truthRecords.Values
                .Where(r => r.Status == "RequiresReview")
                .ToList();

            _logger.LogInformation("Found {Count} records requiring human review", reviewRecords.Count);
            return Result<IEnumerable<TruthRecord>>.Success(reviewRecords);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get records requiring review operation was cancelled");
            return Result<IEnumerable<TruthRecord>>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting records requiring review");
            return Result<IEnumerable<TruthRecord>>.WithFailure($"Failed to get review records: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets data quality metrics for the truth system
    /// </summary>
    /// <param name="fromDate">The start date for metrics</param>
    /// <param name="toDate">The end date for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation containing quality metrics</returns>
    public async Task<Result<ExxerAI.Domain.DocumentProcessing.DataQualityMetrics>> GetDataQualityMetricsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (toDate <= fromDate)
            {
                _logger.LogWarning("Invalid date range for quality metrics: from {FromDate} to {ToDate}", fromDate, toDate);
                return Result<ExxerAI.Domain.DocumentProcessing.DataQualityMetrics>.WithFailure("Invalid date range");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var metrics = await CalculateQualityMetricsAsync(fromDate, toDate, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Calculated quality metrics for period {FromDate} to {ToDate}", fromDate, toDate);
            return Result<ExxerAI.Domain.DocumentProcessing.DataQualityMetrics>.Success(metrics);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get quality metrics operation was cancelled");
            return Result<ExxerAI.Domain.DocumentProcessing.DataQualityMetrics>.WithFailure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating quality metrics for period {FromDate} to {ToDate}", fromDate, toDate);
            return Result<ExxerAI.Domain.DocumentProcessing.DataQualityMetrics>.WithFailure($"Failed to calculate metrics: {ex.Message}");
        }
    }

    // Private helper methods

    private async Task<TruthRecord> CreateTruthRecordAsync(ExtractedData data, DataSource source, CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate async operation

        var dataHash = GenerateDataHash(data);
        
        return new TruthRecord
        {
            Id = Guid.NewGuid().ToString(),
            Data = data,
            Source = source,
            DataHash = dataHash,
            ConfidenceScore = data.ConfidenceScore,
            CreatedAt = DateTime.UtcNow,
            Status = "Active",
            Version = 1,
            CreatedBy = source.ProcessedBy
        };
    }

    private DataLineage CreateDataLineage(TruthRecord truthRecord, DataSource source)
    {
        return new DataLineage
        {
            Id = Guid.NewGuid().ToString(),
            RecordId = truthRecord.Id,
            SourceDocuments = new[] { source.Id },
            ProcessingHistory = new[] { "Extraction", "Validation", "Storage" },
            CreatedAt = DateTime.UtcNow,
            OriginalSource = source,
            FinalTruthRecordId = truthRecord.Id
        };
    }

    private async Task<ExxerAI.Domain.DocumentProcessing.ValidationResult> PerformValidationAsync(ExtractedData data, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken).ConfigureAwait(false); // Simulate validation work

        // Simple validation logic - in real implementation, this would be more sophisticated
        var errors = new List<string>();
        var warnings = new List<string>();

        if (data.ConfidenceScore < 0.5f)
        {
            errors.Add("Confidence score too low");
        }
        else if (data.ConfidenceScore < 0.8f)
        {
            warnings.Add("Confidence score below recommended threshold");
        }

        if (!data.ExtractedFields.Any())
        {
            errors.Add("No fields extracted");
        }

        return new ExxerAI.Domain.DocumentProcessing.ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors,
            Warnings = warnings,
            ValidatedAt = DateTime.UtcNow,
            ConfidenceScore = data.ConfidenceScore
        };
    }

    private async Task<ConflictResolution> PerformConflictResolutionAsync(IEnumerable<ExtractedData> conflictingData, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken).ConfigureAwait(false); // Simulate resolution work

        // Use highest confidence as resolution strategy
        var dataList = conflictingData.ToList();
        var resolvedData = dataList.OrderByDescending(d => d.ConfidenceScore).First();

        return new ConflictResolution
        {
            Id = Guid.NewGuid().ToString(),
            Strategy = ConflictResolutionStrategy.MostConfident,
            ResolvedData = resolvedData,
            ConflictingSourcesData = dataList,
            ResolutionConfidence = resolvedData.ConfidenceScore,
            ResolvedAt = DateTime.UtcNow,
            ResolvedBy = "System"
        };
    }

    private async Task<ExxerAI.Domain.DocumentProcessing.GroundingReport> GenerateReportAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken).ConfigureAwait(false); // Simulate report generation

        var recordsInPeriod = _truthRecords.Values
            .Where(r => r.CreatedAt >= fromDate && r.CreatedAt <= toDate)
            .ToList();

        var validRecords = recordsInPeriod.Count(r => r.ConfidenceScore >= 0.8f);
        var invalidRecords = recordsInPeriod.Count - validRecords;

        return new ExxerAI.Domain.DocumentProcessing.GroundingReport
        {
            FromDate = fromDate,
            ToDate = toDate,
            TotalRecords = recordsInPeriod.Count,
            ValidRecords = validRecords,
            InvalidRecords = invalidRecords,
            AverageConfidence = recordsInPeriod.Any() ? recordsInPeriod.Average(r => r.ConfidenceScore) : 0f,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private async Task<IEnumerable<TruthRecord>> FindSimilarRecordsInternalAsync(ExtractedData data, float threshold, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken).ConfigureAwait(false); // Simulate similarity calculation

        // Simple similarity based on document ID and confidence - in real implementation, would use more sophisticated algorithms
        return _truthRecords.Values
            .Where(r => r.Data.DocumentId != data.DocumentId && 
                       Math.Abs(r.Data.ConfidenceScore - data.ConfidenceScore) <= (1.0f - threshold))
            .ToList();
    }

    private async Task<TruthRecord> UpdateRecordAsync(TruthRecord existingRecord, ExtractedData updatedData, string updatedBy, CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulate update work

        existingRecord.Data = updatedData;
        existingRecord.LastModified = DateTime.UtcNow;
        existingRecord.ModifiedBy = updatedBy;
        existingRecord.Version++;
        existingRecord.DataHash = GenerateDataHash(updatedData);

        return existingRecord;
    }

    private async Task<ExxerAI.Domain.DocumentProcessing.DataQualityMetrics> CalculateQualityMetricsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken).ConfigureAwait(false); // Simulate metrics calculation

        var recordsInPeriod = _truthRecords.Values
            .Where(r => r.CreatedAt >= fromDate && r.CreatedAt <= toDate)
            .ToList();

        var highQuality = recordsInPeriod.Count(r => r.ConfidenceScore >= 0.9f);
        var mediumQuality = recordsInPeriod.Count(r => r.ConfidenceScore >= 0.7f && r.ConfidenceScore < 0.9f);
        var lowQuality = recordsInPeriod.Count - highQuality - mediumQuality;

        return new ExxerAI.Domain.DocumentProcessing.DataQualityMetrics
        {
            FromDate = fromDate,
            ToDate = toDate,
            TotalRecords = recordsInPeriod.Count,
            HighQualityRecords = highQuality,
            MediumQualityRecords = mediumQuality,
            LowQualityRecords = lowQuality,
            AverageConfidence = recordsInPeriod.Any() ? recordsInPeriod.Average(r => r.ConfidenceScore) : 0f,
            DataCompleteness = recordsInPeriod.Any() ? recordsInPeriod.Count(r => r.Data.ExtractedFields.Any()) / (float)recordsInPeriod.Count : 0f,
            DataAccuracy = recordsInPeriod.Any() ? recordsInPeriod.Average(r => r.ConfidenceScore) : 0f
        };
    }

    private static string GenerateDataHash(ExtractedData data)
    {
        var dataJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = false });
        var dataBytes = Encoding.UTF8.GetBytes(dataJson);
        var hashBytes = SHA256.HashData(dataBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Disposes the resources used by the PrimarySourceOfTruthSystem
    /// </summary>
    public void Dispose()
    {
        _writeLock?.Dispose();
        GC.SuppressFinalize(this);
    }
} 