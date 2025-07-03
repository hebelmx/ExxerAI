namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents the agentStatus and statistics of the document ingestion system.
/// </summary>
public class IngestionStatus
{
    /// <summary>
    /// Gets or sets the total number of documents being watched.
    /// </summary>
    public int DocumentsWatched { get; set; }

    /// <summary>
    /// Gets or sets the number of documents processed today.
    /// </summary>
    public int DocumentsProcessedToday { get; set; }

    /// <summary>
    /// Gets or sets the number of documents processed this week.
    /// </summary>
    public int DocumentsProcessedThisWeek { get; set; }

    /// <summary>
    /// Gets or sets the number of active watch sessions.
    /// </summary>
    public int ActiveWatchSessions { get; set; }

    /// <summary>
    /// Gets or sets the number of pending changes to process.
    /// </summary>
    public int PendingChanges { get; set; }

    /// <summary>
    /// Gets or sets the average processing time in milliseconds.
    /// </summary>
    public double AverageProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the overall system health agentStatus.
    /// </summary>
    public HealthStatus SystemHealth { get; set; } = HealthStatus.Healthy;

    /// <summary>
    /// Gets or sets the last time the system processed changes.
    /// </summary>
    public DateTime LastProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets any current system errors or warnings.
    /// </summary>
    public List<string> SystemMessages { get; set; } = new();

    /// <summary>
    /// Gets or sets performance metrics for the ingestion system.
    /// </summary>
    public Dictionary<string, object> Metrics { get; set; } = new();
}