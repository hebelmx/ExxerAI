namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// Raw information collected from various sources
/// </summary>
public record InformationCollection
{
    public string CollectionId { get; init; } = Guid.NewGuid().ToString();
    public DateTime CollectionTimestamp { get; init; } = DateTime.UtcNow;
    public DateTimeRange CollectionPeriod { get; init; } = new();
    public List<InformationItem> Items { get; init; } = new();
    public List<string> SourcesUsed { get; init; } = new();
    public Dictionary<string, object> CollectionMetadata { get; init; } = new();
}

/// <summary>
/// Individual piece of collected information
/// </summary>
public record InformationItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public DateTime PublishedDate { get; init; }
    public float RelevanceScore { get; init; } = 0.0f;
    public List<string> Tags { get; init; } = new();
    public List<string> RelatedCompanies { get; init; } = new();
    public string Url { get; init; } = string.Empty;
    public InformationPriority Priority { get; init; } = InformationPriority.Medium;
}

/// <summary>
/// Aggregated and processed intelligence
/// </summary>
public record AggregatedIntelligence
{
    public string AggregationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime ProcessingTimestamp { get; init; } = DateTime.UtcNow;
    public List<IntelligenceCategory> Categories { get; init; } = new();
    public List<TrendingTopic> TrendingTopics { get; init; } = new();
    public List<IntelligenceInsight> KeyInsights { get; init; } = new();
    public List<ActionableItem> ActionableItems { get; init; } = new();
    public Dictionary<string, int> CompanyMentions { get; init; } = new();
    public Dictionary<string, float> TopicRelevanceScores { get; init; } = new();
}

/// <summary>
/// Categorized intelligence information
/// </summary>
public record IntelligenceCategory
{
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryDescription { get; init; } = string.Empty;
    public List<InformationItem> Items { get; init; } = new();
    public int ItemCount { get; init; }
    public float AverageRelevance { get; init; }
    public string CategorySummary { get; init; } = string.Empty;
}

/// <summary>
/// Trending topic discovered by the aggregator
/// </summary>
public record TrendingTopic
{
    public string TopicName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public float TrendingScore { get; init; } = 0.0f;
    public int MentionCount { get; init; }
    public List<string> RelatedKeywords { get; init; } = new();
    public List<InformationItem> RelatedItems { get; init; } = new();
    public TrendDirection Direction { get; init; } = TrendDirection.Stable;
}

/// <summary>
/// Generated insight from information analysis
/// </summary>
public record IntelligenceInsight
{
    public string InsightId { get; init; } = Guid.NewGuid().ToString();
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public InsightType Type { get; init; } = InsightType.Observation;
    public float ConfidenceLevel { get; init; } = 0.0f;
    public List<string> SupportingEvidence { get; init; } = new();
    public List<string> AffectedCompanies { get; init; } = new();
    public BusinessImpact Impact { get; init; } = BusinessImpact.Low;
}

/// <summary>
/// Actionable item derived from intelligence
/// </summary>
public record ActionableItem
{
    public string ActionId { get; init; } = Guid.NewGuid().ToString();
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ActionPriority Priority { get; init; } = ActionPriority.Medium;
    public DateTime SuggestedDeadline { get; init; }
    public List<string> RelevantContacts { get; init; } = new();
    public string Category { get; init; } = string.Empty; // Follow-up, Research, Decision, etc.
}

/// <summary>
/// Personalized intelligence report
/// </summary>
public record PersonalizedReport
{
    public string ReportId { get; init; } = Guid.NewGuid().ToString();
    public string UserId { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public DateTimeRange ReportPeriod { get; init; } = new();
    public string ExecutiveSummary { get; init; } = string.Empty;
    public List<IntelligenceCategory> PrioritizedCategories { get; init; } = new();
    public List<TrendingTopic> RelevantTrends { get; init; } = new();
    public List<ActionableItem> RecommendedActions { get; init; } = new();
    public Dictionary<string, string> CompanyUpdates { get; init; } = new(); // Company -> Update
    public List<InformationSourceSuggestion> NewSourceSuggestions { get; init; } = new();
    public ReportMetrics Metrics { get; init; } = new();
}

/// <summary>
/// Date and time range specification
/// </summary>
public record DateTimeRange
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public TimeSpan Duration => EndDate - StartDate;
}

/// <summary>
/// Suggestion for new information sources
/// </summary>
public record InformationSourceSuggestion
{
    public string SourceName { get; init; } = string.Empty;
    public string SourceType { get; init; } = string.Empty; // RSS, Website, API, etc.
    public string Url { get; init; } = string.Empty;
    public string Reasoning { get; init; } = string.Empty;
    public float RelevanceScore { get; init; } = 0.0f;
    public List<string> ExpectedTopics { get; init; } = new();
}

/// <summary>
/// Report performance metrics
/// </summary>
public record ReportMetrics
{
    public int TotalItemsProcessed { get; init; }
    public int RelevantItemsIncluded { get; init; }
    public TimeSpan ProcessingTime { get; init; }
    public float AverageRelevanceScore { get; init; }
    public int NewSourcesDiscovered { get; init; }
    public int ActionItemsGenerated { get; init; }
}

// Enums for categorization
public enum InformationPriority { Low, Medium, High, Critical }
public enum TrendDirection { Declining, Stable, Rising, Emerging }
public enum InsightType { Observation, Prediction, Recommendation, Warning }
public enum BusinessImpact { Low, Medium, High, Critical }
public enum ActionPriority { Low, Medium, High, Urgent } 