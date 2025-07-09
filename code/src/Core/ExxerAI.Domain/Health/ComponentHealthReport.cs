namespace ExxerAI.Domain.Health;

/// <summary>
/// Health report for an individual system component
/// </summary>
public class ComponentHealthReport
{
    /// <summary>
    /// Name of the component
    /// </summary>
    public string ComponentName { get; set; } = string.Empty;

    /// <summary>
    /// Current health status of the component
    /// </summary>
    public HealthStatus Status { get; set; }

    /// <summary>
    /// Timestamp when the health check was performed
    /// </summary>
    public DateTime CheckedAt { get; set; }

    /// <summary>
    /// Time taken to perform the health check
    /// </summary>
    public TimeSpan CheckDuration { get; set; }

    /// <summary>
    /// Detailed status message
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>
    /// Component version information
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Component uptime
    /// </summary>
    public TimeSpan? Uptime { get; set; }

    /// <summary>
    /// List of issues or errors found
    /// </summary>
    public List<HealthIssue> Issues { get; set; } = [];

    /// <summary>
    /// Component-specific metrics
    /// </summary>
    public Dictionary<string, object> Metrics { get; set; } = [];

    /// <summary>
    /// Dependencies that this component relies on
    /// </summary>
    public List<ComponentDependency> Dependencies { get; set; } = [];

    /// <summary>
    /// Resource utilization information
    /// </summary>
    public ResourceUtilization? ResourceUsage { get; set; }

    /// <summary>
    /// Adds an issue to the component health report
    /// </summary>
    /// <param name="severity">Severity level of the issue</param>
    /// <param name="message">Description of the issue</param>
    /// <param name="details">Additional details about the issue</param>
    public void AddIssue(HealthIssueSeverity severity, string message, string? details = null)
    {
        Issues.Add(new HealthIssue
        {
            Severity = severity,
            Message = message,
            Details = details,
            DetectedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets critical issues for this component
    /// </summary>
    public IEnumerable<HealthIssue> GetCriticalIssues()
    {
        return Issues.Where(i => i.Severity == HealthIssueSeverity.Critical);
    }

    /// <summary>
    /// Gets warning issues for this component
    /// </summary>
    public IEnumerable<HealthIssue> GetWarningIssues()
    {
        return Issues.Where(i => i.Severity == HealthIssueSeverity.Warning);
    }
}