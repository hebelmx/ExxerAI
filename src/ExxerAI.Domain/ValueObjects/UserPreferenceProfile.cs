namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// Represents a user's learned preference profile for personalized intelligence
/// </summary>
public record UserPreferenceProfile
{
    /// <summary>
    /// User identifier
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// User's business role and context
    /// </summary>
    public UserBusinessContext BusinessContext { get; init; } = new();

    /// <summary>
    /// Preferred information categories with relevance weights
    /// </summary>
    public Dictionary<string, float> CategoryPreferences { get; init; } = new();

    /// <summary>
    /// Specific companies and organizations of interest
    /// </summary>
    public List<CompanyInterest> CompaniesOfInterest { get; init; } = new();

    /// <summary>
    /// Technology topics and their importance scores
    /// </summary>
    public Dictionary<string, float> TechnologyInterests { get; init; } = new();

    /// <summary>
    /// Geographic regions of business interest
    /// </summary>
    public List<string> RegionsOfInterest { get; init; } = new();

    /// <summary>
    /// Preferred information sources and their credibility scores
    /// </summary>
    public Dictionary<string, float> PreferredSources { get; init; } = new();

    /// <summary>
    /// Information consumption patterns (time, frequency, format)
    /// </summary>
    public ConsumptionPreferences ConsumptionPattern { get; init; } = new();

    /// <summary>
    /// Topics to filter out or minimize
    /// </summary>
    public List<string> FilteredTopics { get; init; } = new();

    /// <summary>
    /// Learning confidence score (how well we understand user preferences)
    /// </summary>
    public float LearningConfidence { get; init; } = 0.0f;

    /// <summary>
    /// Last time preferences were updated
    /// </summary>
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// User feedback history for learning improvement
    /// </summary>
    public List<UserFeedbackSummary> FeedbackHistory { get; init; } = new();
}

/// <summary>
/// User's business context and role information
/// </summary>
public record UserBusinessContext
{
    public string JobTitle { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Industry { get; init; } = string.Empty;
    public List<string> Responsibilities { get; init; } = new();
    public List<string> DecisionAreas { get; init; } = new();
}

/// <summary>
/// Company or organization of interest to the user
/// </summary>
public record CompanyInterest
{
    public string CompanyName { get; init; } = string.Empty;
    public string Relationship { get; init; } = string.Empty; // Client, Partner, Competitor, etc.
    public float ImportanceLevel { get; init; } = 1.0f;
    public List<string> SpecificInterests { get; init; } = new(); // M&A, Products, Leadership, etc.
}

/// <summary>
/// User's information consumption preferences
/// </summary>
public record ConsumptionPreferences
{
    public string PreferredReportFormat { get; init; } = "Executive Summary"; // Detailed, Brief, Visual
    public TimeSpan PreferredReadingTime { get; init; } = TimeSpan.FromMinutes(15);
    public DayOfWeek PreferredDeliveryDay { get; init; } = DayOfWeek.Monday;
    public TimeOnly PreferredDeliveryTime { get; init; } = new(8, 0); // 8 AM
    public string PreferredLanguage { get; init; } = "English";
    public bool IncludeActionItems { get; init; } = true;
    public bool IncludeTrendAnalysis { get; init; } = true;
}

/// <summary>
/// Summary of user feedback for learning purposes
/// </summary>
public record UserFeedbackSummary
{
    public DateTime FeedbackDate { get; init; }
    public string ContentCategory { get; init; } = string.Empty;
    public float RelevanceScore { get; init; } // 1-5 rating
    public string FeedbackType { get; init; } = string.Empty; // "More", "Less", "Different"
    public string SpecificFeedback { get; init; } = string.Empty;
} 