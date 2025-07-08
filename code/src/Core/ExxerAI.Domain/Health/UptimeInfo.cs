namespace ExxerAI.Domain.Health;

/// <summary>
/// System uptime and availability information
/// </summary>
public class UptimeInfo
{
    /// <summary>
    /// When the system was started
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Total uptime duration
    /// </summary>
    public TimeSpan Uptime => DateTime.UtcNow - StartedAt;

    /// <summary>
    /// Application version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Environment name (Development, Staging, Production)
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Host machine information
    /// </summary>
    public string HostName { get; set; } = string.Empty;

    /// <summary>
    /// Process ID
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    /// Number of times the system has restarted
    /// </summary>
    public int RestartCount { get; set; }

    /// <summary>
    /// Last restart timestamp
    /// </summary>
    public DateTime? LastRestartAt { get; set; }

    /// <summary>
    /// Uptime percentage over the last 24 hours
    /// </summary>
    public double? UptimePercentage24h { get; set; }

    /// <summary>
    /// Uptime percentage over the last 7 days
    /// </summary>
    public double? UptimePercentage7d { get; set; }
}