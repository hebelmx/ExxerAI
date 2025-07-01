using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents the processing history for a set of documents, used for learning and adaptation.
/// Contains historical processing results, patterns, and outcomes for analysis.
/// </summary>
public class ProcessingHistory
{
    /// <summary>
    /// Gets or sets the unique identifier for this processing history.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the document type this history applies to.
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the collection of processing results.
    /// </summary>
    public List<DocumentProcessingResult> ProcessingResults { get; set; } = new();

    /// <summary>
    /// Gets or sets the time range this history covers.
    /// </summary>
    public DateRange TimeRange { get; set; } = new();

    /// <summary>
    /// Gets or sets the learned patterns from processing.
    /// </summary>
    public List<LearnedPattern> LearnedPatterns { get; set; } = new();

    /// <summary>
    /// Gets or sets the performance metrics for this processing history.
    /// </summary>
    public ProcessingMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Gets or sets the schema evolution history.
    /// </summary>
    public List<SchemaEvolution> SchemaEvolutions { get; set; } = new();

    /// <summary>
    /// Gets or sets the adaptation recommendations based on this history.
    /// </summary>
    public List<AdaptationRecommendation> Recommendations { get; set; } = new();

    /// <summary>
    /// Gets or sets when this history was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this history was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets additional history metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the ProcessingHistory class.
    /// </summary>
    public ProcessingHistory() { }

    /// <summary>
    /// Initializes a new instance of the ProcessingHistory class for a specific document type.
    /// </summary>
    /// <param name="documentType">The document type this history applies to.</param>
    public ProcessingHistory(DocumentType documentType)
    {
        DocumentType = documentType;
        TimeRange = new DateRange
        {
            FromDate = DateTime.UtcNow.AddMonths(-1),
            ToDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adds a processing result to the history.
    /// </summary>
    /// <param name="result">The processing result to add.</param>
    public void AddProcessingResult(DocumentProcessingResult result)
    {
        if (result != null)
        {
            ProcessingResults.Add(result);
            UpdateMetrics();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Adds a learned pattern to the history.
    /// </summary>
    /// <param name="pattern">The learned pattern to add.</param>
    public void AddLearnedPattern(LearnedPattern pattern)
    {
        if (pattern != null)
        {
            LearnedPatterns.Add(pattern);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Adds a schema evolution record.
    /// </summary>
    /// <param name="evolution">The schema evolution to record.</param>
    public void AddSchemaEvolution(SchemaEvolution evolution)
    {
        if (evolution != null)
        {
            SchemaEvolutions.Add(evolution);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Gets the successful processing results.
    /// </summary>
    public IEnumerable<DocumentProcessingResult> SuccessfulResults =>
        ProcessingResults.Where(r => r.IsSuccessful);

    /// <summary>
    /// Gets the failed processing results.
    /// </summary>
    public IEnumerable<DocumentProcessingResult> FailedResults =>
        ProcessingResults.Where(r => !r.IsSuccessful);

    /// <summary>
    /// Gets the processing results within a specific confidence range.
    /// </summary>
    /// <param name="minConfidence">The minimum confidence threshold.</param>
    /// <param name="maxConfidence">The maximum confidence threshold.</param>
    /// <returns>Processing results within the confidence range.</returns>
    public IEnumerable<DocumentProcessingResult> GetResultsByConfidence(float minConfidence, float maxConfidence)
    {
        return ProcessingResults.Where(r => r.OverallConfidence >= minConfidence && r.OverallConfidence <= maxConfidence);
    }

    /// <summary>
    /// Gets the most common extraction patterns for a specific field.
    /// </summary>
    /// <param name="fieldName">The field name to analyze.</param>
    /// <returns>The most common patterns for the field.</returns>
    public IEnumerable<string> GetCommonPatternsForField(string fieldName)
    {
        return LearnedPatterns
            .Where(p => p.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.SuccessCount)
            .Select(p => p.Pattern)
            .Take(5);
    }

    /// <summary>
    /// Analyzes the processing history to generate adaptation recommendations.
    /// </summary>
    public void AnalyzeAndGenerateRecommendations()
    {
        Recommendations.Clear();

        // Analyze success rates
        var successRate = ProcessingResults.Any() ? 
            (float)SuccessfulResults.Count() / ProcessingResults.Count : 0f;

        if (successRate < 0.8f)
        {
            Recommendations.Add(new AdaptationRecommendation
            {
                Type = RecommendationType.SchemaImprovement,
                Priority = RecommendationPriority.High,
                Description = $"Success rate is low ({successRate:P0}). Consider improving extraction patterns.",
                Confidence = 0.9f
            });
        }

        // Analyze confidence trends
        var avgConfidence = SuccessfulResults.Any() ? 
            SuccessfulResults.Average(r => r.OverallConfidence) : 0f;

        if (avgConfidence < 0.7f)
        {
            Recommendations.Add(new AdaptationRecommendation
            {
                Type = RecommendationType.ConfidenceImprovement,
                Priority = RecommendationPriority.Medium,
                Description = $"Average confidence is low ({avgConfidence:P0}). Consider adding more validation patterns.",
                Confidence = 0.8f
            });
        }

        // Analyze processing time trends
        var avgProcessingTime = ProcessingResults.Any() ? 
            ProcessingResults.Average(r => r.ProcessingTimeMs) : 0;

        if (avgProcessingTime > 10000) // More than 10 seconds
        {
            Recommendations.Add(new AdaptationRecommendation
            {
                Type = RecommendationType.PerformanceOptimization,
                Priority = RecommendationPriority.Low,
                Description = $"Average processing time is high ({avgProcessingTime:F0}ms). Consider optimizing extraction algorithms.",
                Confidence = 0.7f
            });
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the processing metrics based on current results.
    /// </summary>
    private void UpdateMetrics()
    {
        if (!ProcessingResults.Any())
        {
            Metrics = new ProcessingMetrics();
            return;
        }

        Metrics = new ProcessingMetrics
        {
            TotalDocuments = ProcessingResults.Count,
            SuccessfulDocuments = SuccessfulResults.Count(),
            FailedDocuments = FailedResults.Count(),
            SuccessRate = (float)SuccessfulResults.Count() / ProcessingResults.Count,
            AverageConfidence = SuccessfulResults.Any() ? SuccessfulResults.Average(r => r.OverallConfidence) : 0f,
            AverageProcessingTime = ProcessingResults.Average(r => r.ProcessingTimeMs),
            ConfidenceDistribution = CalculateConfidenceDistribution()
        };
    }

    /// <summary>
    /// Calculates the distribution of confidence scores.
    /// </summary>
    /// <returns>A dictionary mapping confidence ranges to counts.</returns>
    private Dictionary<string, int> CalculateConfidenceDistribution()
    {
        var distribution = new Dictionary<string, int>
        {
            ["0.0-0.2"] = 0,
            ["0.2-0.4"] = 0,
            ["0.4-0.6"] = 0,
            ["0.6-0.8"] = 0,
            ["0.8-1.0"] = 0
        };

        foreach (var result in ProcessingResults)
        {
            var confidence = result.OverallConfidence;
            var range = confidence switch
            {
                < 0.2f => "0.0-0.2",
                < 0.4f => "0.2-0.4",
                < 0.6f => "0.4-0.6",
                < 0.8f => "0.6-0.8",
                _ => "0.8-1.0"
            };
            distribution[range]++;
        }

        return distribution;
    }

    /// <summary>
    /// Gets a summary of this processing history.
    /// </summary>
    /// <returns>A formatted summary string.</returns>
    public string GetSummary()
    {
        return $"ProcessingHistory[{DocumentType}, {ProcessingResults.Count} documents, {Metrics.SuccessRate:P0} success rate]";
    }
}

/// <summary>
/// Represents a learned pattern from processing history.
/// </summary>
public class LearnedPattern
{
    /// <summary>
    /// Gets or sets the unique identifier for this pattern.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the field name this pattern applies to.
    /// </summary>
    [StringLength(255)]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extraction pattern.
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pattern type (Regex, Keyword, etc.).
    /// </summary>
    public string PatternType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how many times this pattern was successful.
    /// </summary>
    public int SuccessCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets how many times this pattern failed.
    /// </summary>
    public int FailureCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the confidence score for this pattern.
    /// </summary>
    public float Confidence { get; set; } = 0f;

    /// <summary>
    /// Gets or sets when this pattern was first learned.
    /// </summary>
    public DateTime LearnedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this pattern was last used successfully.
    /// </summary>
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the success rate for this pattern.
    /// </summary>
    public float SuccessRate => (SuccessCount + FailureCount) > 0 ? 
        (float)SuccessCount / (SuccessCount + FailureCount) : 0f;
}

/// <summary>
/// Represents processing metrics for a history period.
/// </summary>
public class ProcessingMetrics
{
    /// <summary>
    /// Gets or sets the total number of documents processed.
    /// </summary>
    public int TotalDocuments { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of successfully processed documents.
    /// </summary>
    public int SuccessfulDocuments { get; set; } = 0;

    /// <summary>
    /// Gets or sets the number of failed document processing attempts.
    /// </summary>
    public int FailedDocuments { get; set; } = 0;

    /// <summary>
    /// Gets or sets the overall success rate (0.0 to 1.0).
    /// </summary>
    public float SuccessRate { get; set; } = 0f;

    /// <summary>
    /// Gets or sets the average confidence score.
    /// </summary>
    public float AverageConfidence { get; set; } = 0f;

    /// <summary>
    /// Gets or sets the average processing time in milliseconds.
    /// </summary>
    public double AverageProcessingTime { get; set; } = 0;

    /// <summary>
    /// Gets or sets the distribution of confidence scores.
    /// </summary>
    public Dictionary<string, int> ConfidenceDistribution { get; set; } = new();

    /// <summary>
    /// Gets or sets additional metric properties.
    /// </summary>
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
}

/// <summary>
/// Represents the evolution of a schema over time.
/// </summary>
public class SchemaEvolution
{
    /// <summary>
    /// Gets or sets the unique identifier for this evolution record.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the schema ID that evolved.
    /// </summary>
    [StringLength(255)]
    public string SchemaId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the previous version number.
    /// </summary>
    public int PreviousVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the new version number.
    /// </summary>
    public int NewVersion { get; set; } = 2;

    /// <summary>
    /// Gets or sets the changes made in this evolution.
    /// </summary>
    public List<string> Changes { get; set; } = new();

    /// <summary>
    /// Gets or sets the reason for the evolution.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this evolution occurred.
    /// </summary>
    public DateTime EvolvedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the impact of this evolution on processing accuracy.
    /// </summary>
    public float AccuracyImprovement { get; set; } = 0f;
}

/// <summary>
/// Represents an adaptation recommendation based on processing history analysis.
/// </summary>
public class AdaptationRecommendation
{
    /// <summary>
    /// Gets or sets the unique identifier for this recommendation.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the type of recommendation.
    /// </summary>
    public RecommendationType Type { get; set; } = RecommendationType.SchemaImprovement;

    /// <summary>
    /// Gets or sets the priority of this recommendation.
    /// </summary>
    public RecommendationPriority Priority { get; set; } = RecommendationPriority.Medium;

    /// <summary>
    /// Gets or sets the description of the recommendation.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence in this recommendation.
    /// </summary>
    public float Confidence { get; set; } = 0f;

    /// <summary>
    /// Gets or sets the expected impact of implementing this recommendation.
    /// </summary>
    public string ExpectedImpact { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this recommendation was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether this recommendation has been implemented.
    /// </summary>
    public bool IsImplemented { get; set; } = false;

    /// <summary>
    /// Gets or sets when this recommendation was implemented.
    /// </summary>
    public DateTime? ImplementedAt { get; set; }
}

/// <summary>
/// Enumeration of recommendation types.
/// </summary>
public enum RecommendationType
{
    /// <summary>
    /// Recommendation to improve schema patterns.
    /// </summary>
    SchemaImprovement,

    /// <summary>
    /// Recommendation to improve confidence scoring.
    /// </summary>
    ConfidenceImprovement,

    /// <summary>
    /// Recommendation to optimize processing performance.
    /// </summary>
    PerformanceOptimization,

    /// <summary>
    /// Recommendation to add new validation rules.
    /// </summary>
    ValidationEnhancement,

    /// <summary>
    /// Recommendation to improve error handling.
    /// </summary>
    ErrorHandlingImprovement
}

/// <summary>
/// Enumeration of recommendation priorities.
/// </summary>
public enum RecommendationPriority
{
    /// <summary>
    /// Low priority recommendation.
    /// </summary>
    Low,

    /// <summary>
    /// Medium priority recommendation.
    /// </summary>
    Medium,

    /// <summary>
    /// High priority recommendation.
    /// </summary>
    High,

    /// <summary>
    /// Critical priority recommendation.
    /// </summary>
    Critical
} 