namespace ExxerAI.Domain.Health;

/// <summary>
/// Represents a health issue found during component monitoring
/// </summary>
public class HealthIssue
{
    /// <summary>
    /// Severity level of the issue
    /// </summary>
    public HealthIssueSeverity Severity { get; set; }

    /// <summary>
    /// Brief description of the issue
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed information about the issue
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// When the issue was detected
    /// </summary>
    public DateTime DetectedAt { get; set; }

    /// <summary>
    /// Component or subsystem where the issue occurred
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Error code or identifier if applicable
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Suggested resolution or action
    /// </summary>
    public string? SuggestedAction { get; set; }

    /// <summary>
    /// Additional metadata about the issue
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Severity levels for health issues
/// </summary>
public enum HealthIssueSeverity
{
    /// <summary>
    /// Informational message
    /// </summary>
    Info,

    /// <summary>
    /// Warning that should be monitored
    /// </summary>
    Warning,

    /// <summary>
    /// Error that affects functionality
    /// </summary>
    Error,

    /// <summary>
    /// Critical issue that requires immediate attention
    /// </summary>
    Critical
}