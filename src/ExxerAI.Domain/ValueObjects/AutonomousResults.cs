namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// Result of autonomous cycle execution
/// </summary>
public record AutonomousCycleResult
{
    public string UserId { get; init; } = string.Empty;
    public DateTime CycleStartTime { get; init; }
    public DateTime CycleEndTime { get; init; }
    public bool IsSuccessful { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public int ItemsProcessed { get; init; }
    public int CategoriesCreated { get; init; }
    public int InsightsGenerated { get; init; }
    public int ActionItemsCreated { get; init; }
    public TimeSpan TotalProcessingTime => CycleEndTime - CycleStartTime;
    public Dictionary<string, object> CycleMetadata { get; init; } = new();
}

/// <summary>
/// User interaction for learning purposes
/// </summary>
public record UserInteraction
{
    public string InteractionId { get; init; } = Guid.NewGuid().ToString();
    public string UserId { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public UserInteractionType Type { get; init; } = UserInteractionType.Query;
    public string Content { get; init; } = string.Empty;
    public string Response { get; init; } = string.Empty;
    public float SatisfactionScore { get; init; } = 0.0f; // 1-5 rating
    public Dictionary<string, object> Context { get; init; } = new();
    public List<string> Topics { get; init; } = new();
    public TimeSpan ResponseTime { get; init; }
}

/// <summary>
/// Learning adjustment result from user feedback
/// </summary>
public record LearningAdjustment
{
    public string UserId { get; init; } = string.Empty;
    public DateTime AdjustmentTime { get; init; } = DateTime.UtcNow;
    public Dictionary<string, float> PreferenceChanges { get; init; } = new();
    public List<string> AddedSources { get; init; } = new();
    public List<string> RemovedSources { get; init; } = new();
    public float ConfidenceChange { get; init; } = 0.0f;
    public string AdjustmentReason { get; init; } = string.Empty;
    public bool RequiresReprocessing { get; init; }
}

/// <summary>
/// User feedback on report relevance
/// </summary>
public record ReportFeedback
{
    public string ReportId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public DateTime FeedbackTime { get; init; } = DateTime.UtcNow;
    public float OverallRating { get; init; } = 0.0f; // 1-5
    public Dictionary<string, float> CategoryRatings { get; init; } = new();
    public Dictionary<string, float> ItemRatings { get; init; } = new();
    public List<string> WantMoreOf { get; init; } = new();
    public List<string> WantLessOf { get; init; } = new();
    public string FreeformFeedback { get; init; } = string.Empty;
    public bool RecommendToOthers { get; init; }
}

/// <summary>
/// Report template configuration
/// </summary>
public record ReportTemplate
{
    public string TemplateId { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ReportFormat Format { get; init; } = ReportFormat.ExecutiveSummary;
    public List<string> IncludedSections { get; init; } = new();
    public Dictionary<string, object> FormatSettings { get; init; } = new();
    public int MaxItems { get; init; } = 50;
    public TimeSpan EstimatedReadTime { get; init; } = TimeSpan.FromMinutes(15);
    public string Language { get; init; } = "English";
}

/// <summary>
/// Information source definition
/// </summary>
public record InformationSource
{
    public string SourceId { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public InformationSourceType Type { get; init; } = InformationSourceType.Website;
    public List<string> Categories { get; init; } = new();
    public float CredibilityScore { get; init; } = 1.0f;
    public int CheckFrequencyMinutes { get; init; } = 60;
    public DateTime LastChecked { get; init; }
    public bool IsActive { get; init; } = true;
    public Dictionary<string, string> AuthCredentials { get; init; } = new();
    public Dictionary<string, object> SourceMetadata { get; init; } = new();
}

/// <summary>
/// Source monitoring result
/// </summary>
public record SourceMonitoringResult
{
    public string SourceId { get; init; } = string.Empty;
    public DateTime MonitoringTime { get; init; } = DateTime.UtcNow;
    public bool IsSuccessful { get; init; }
    public int NewItemsFound { get; init; }
    public int UpdatedItemsFound { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public TimeSpan ResponseTime { get; init; }
    public List<InformationItem> NewItems { get; init; } = new();
    public bool SourceAvailable { get; init; } = true;
}

/// <summary>
/// Vector storage result
/// </summary>
public record VectorStorageResult
{
    public bool IsSuccessful { get; init; }
    public string DocumentId { get; init; } = string.Empty;
    public int VectorDimensions { get; init; }
    public string CollectionName { get; init; } = string.Empty;
    public DateTime StoredAt { get; init; } = DateTime.UtcNow;
    public string ErrorMessage { get; init; } = string.Empty;
    public Dictionary<string, object> StorageMetadata { get; init; } = new();
}

/// <summary>
/// Vector search result
/// </summary>
public record VectorSearchResult
{
    public string DocumentId { get; init; } = string.Empty;
    public float SimilarityScore { get; init; } = 0.0f;
    public Dictionary<string, object> Metadata { get; init; } = new();
    public string ContentPreview { get; init; } = string.Empty;
    public DateTime LastUpdated { get; init; }
    public List<string> MatchedKeywords { get; init; } = new();
}

/// <summary>
/// Conversation storage result
/// </summary>
public record ConversationStorageResult
{
    public bool IsSuccessful { get; init; }
    public string MessageId { get; init; } = string.Empty;
    public string SessionId { get; init; } = string.Empty;
    public DateTime StoredAt { get; init; } = DateTime.UtcNow;
    public string ErrorMessage { get; init; } = string.Empty;
}

/// <summary>
/// Conversation history
/// </summary>
public record ConversationHistory
{
    public string SessionId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public List<ChatMessage> Messages { get; init; } = new();
    public DateTime SessionStarted { get; init; }
    public DateTime LastActivity { get; init; }
    public int TotalMessages { get; init; }
    public Dictionary<string, object> SessionMetadata { get; init; } = new();
}

/// <summary>
/// Chat message for conversation memory
/// </summary>
public record ChatMessage
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string SessionId { get; init; } = string.Empty;
    public string SenderId { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public MessageType Type { get; init; } = MessageType.User;
    public Dictionary<string, object> Metadata { get; init; } = new();
    public List<string> Attachments { get; init; } = new();
}

/// <summary>
/// Conversation context for agents
/// </summary>
public record ConversationContext
{
    public string SessionId { get; init; } = string.Empty;
    public List<ChatMessage> RecentMessages { get; init; } = new();
    public string ConversationSummary { get; init; } = string.Empty;
    public List<string> TopicHistory { get; init; } = new();
    public Dictionary<string, object> ContextVariables { get; init; } = new();
    public DateTime ContextGeneratedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Conversation session information
/// </summary>
public record ConversationSession
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public string UserId { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; init; }
    public string SessionType { get; init; } = "Standard";
    public Dictionary<string, object> SessionMetadata { get; init; } = new();
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Conversation summary
/// </summary>
public record ConversationSummary
{
    public string SessionId { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public List<string> KeyTopics { get; init; } = new();
    public List<string> ActionItems { get; init; } = new();
    public int MessageCount { get; init; }
    public TimeSpan Duration { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public float SatisfactionScore { get; init; } = 0.0f;
}

/// <summary>
/// Conversation search result
/// </summary>
public record ConversationSearchResult
{
    public string MessageId { get; init; } = string.Empty;
    public string SessionId { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public float RelevanceScore { get; init; } = 0.0f;
    public string Context { get; init; } = string.Empty;
    public List<string> MatchedKeywords { get; init; } = new();
}

// Enums for supporting types
public enum UserInteractionType { Query, Feedback, Command, Browse, Search }
public enum ReportFormat { ExecutiveSummary, Detailed, Brief, Visual, Custom }
public enum InformationSourceType { Website, RSS, API, Database, Email, SocialMedia }
public enum MessageType { User, Agent, System, Notification } 