namespace ExxerAI.Domain.Health;

/// <summary>
/// Resource utilization information for a component
/// </summary>
public class ResourceUtilization
{
    /// <summary>
    /// CPU usage specific to this component (percentage)
    /// </summary>
    public double? CpuUsagePercent { get; set; }

    /// <summary>
    /// Memory usage specific to this component (bytes)
    /// </summary>
    public long? MemoryUsageBytes { get; set; }

    /// <summary>
    /// Number of active connections or sessions
    /// </summary>
    public int? ActiveConnections { get; set; }

    /// <summary>
    /// Queue depth or backlog size
    /// </summary>
    public int? QueueDepth { get; set; }

    /// <summary>
    /// Throughput metrics (operations per second)
    /// </summary>
    public double? ThroughputPerSecond { get; set; }

    /// <summary>
    /// Cache hit ratio (percentage)
    /// </summary>
    public double? CacheHitRatio { get; set; }

    /// <summary>
    /// Disk I/O operations per second
    /// </summary>
    public double? DiskIOPS { get; set; }

    /// <summary>
    /// Network bandwidth utilization (bytes per second)
    /// </summary>
    public long? NetworkBytesPerSecond { get; set; }

    /// <summary>
    /// Database connection pool usage
    /// </summary>
    public DatabasePoolUsage? DatabasePool { get; set; }

    /// <summary>
    /// Custom resource metrics specific to the component
    /// </summary>
    public Dictionary<string, object> CustomMetrics { get; set; } = new();
}

/// <summary>
/// Database connection pool utilization metrics
/// </summary>
public class DatabasePoolUsage
{
    /// <summary>
    /// Total number of connections in the pool
    /// </summary>
    public int TotalConnections { get; set; }

    /// <summary>
    /// Number of active/busy connections
    /// </summary>
    public int ActiveConnections { get; set; }

    /// <summary>
    /// Number of idle connections
    /// </summary>
    public int IdleConnections { get; set; }

    /// <summary>
    /// Pool utilization percentage
    /// </summary>
    public double UtilizationPercent => TotalConnections > 0 
        ? (double)ActiveConnections / TotalConnections * 100 
        : 0;

    /// <summary>
    /// Average wait time for connections (milliseconds)
    /// </summary>
    public double? AverageWaitTimeMs { get; set; }

    /// <summary>
    /// Connection timeout count
    /// </summary>
    public int TimeoutCount { get; set; }
}