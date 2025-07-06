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

        // Only generate recommendations if there's data to analyze
        if (!ProcessingResults.Any())
        {
            UpdatedAt = DateTime.UtcNow;
            return;
        }

        // Analyze success rates
        var successRate = (float)SuccessfulResults.Count() / ProcessingResults.Count;

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

        if (SuccessfulResults.Any() && avgConfidence < 0.7f)
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
        var avgProcessingTime = ProcessingResults.Average(r => r.ProcessingTimeMs);

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