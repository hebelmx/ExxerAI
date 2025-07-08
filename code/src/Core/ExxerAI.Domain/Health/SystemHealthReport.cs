namespace ExxerAI.Domain.Health;

/// <summary>
/// Comprehensive system health report containing status of all components
/// </summary>
public class SystemHealthReport
{
    /// <summary>
    /// Overall system health status
    /// </summary>
    public HealthStatus OverallStatus { get; set; }

    /// <summary>
    /// Timestamp when the health check was performed
    /// </summary>
    public DateTime CheckedAt { get; set; }

    /// <summary>
    /// Total time taken to perform all health checks
    /// </summary>
    public TimeSpan TotalCheckDuration { get; set; }

    /// <summary>
    /// Health reports for individual components
    /// </summary>
    public Dictionary<string, ComponentHealthReport> ComponentReports { get; set; } = new();

    /// <summary>
    /// Summary of critical issues found
    /// </summary>
    public List<string> CriticalIssues { get; set; } = new();

    /// <summary>
    /// Summary of warnings found
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// System uptime information
    /// </summary>
    public UptimeInfo Uptime { get; set; } = new();

    /// <summary>
    /// Performance metrics snapshot
    /// </summary>
    public PerformanceMetrics Performance { get; set; } = new();

    /// <summary>
    /// Gets the count of components by health status
    /// </summary>
    public Dictionary<HealthStatus, int> GetComponentStatusCounts()
    {
        return ComponentReports.Values
            .GroupBy(r => r.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Determines if the system is in a degraded state
    /// </summary>
    public bool IsDegraded => OverallStatus == HealthStatus.Degraded || OverallStatus == HealthStatus.Critical;

    /// <summary>
    /// Gets all components with non-healthy status
    /// </summary>
    public IEnumerable<ComponentHealthReport> GetUnhealthyComponents()
    {
        return ComponentReports.Values.Where(r => r.Status != HealthStatus.Healthy);
    }
}