namespace ExxerAI.Domain.Health;

/// <summary>
/// System performance metrics snapshot
/// </summary>
public class PerformanceMetrics
{
    /// <summary>
    /// CPU usage percentage (0-100)
    /// </summary>
    public double? CpuUsagePercent { get; set; }

    /// <summary>
    /// Memory usage in bytes
    /// </summary>
    public long? MemoryUsageBytes { get; set; }

    /// <summary>
    /// Total available memory in bytes
    /// </summary>
    public long? TotalMemoryBytes { get; set; }

    /// <summary>
    /// Memory usage percentage (0-100)
    /// </summary>
    public double? MemoryUsagePercent => 
        TotalMemoryBytes.HasValue && MemoryUsageBytes.HasValue && TotalMemoryBytes > 0
            ? (double)MemoryUsageBytes.Value / TotalMemoryBytes.Value * 100
            : null;

    /// <summary>
    /// Disk usage information
    /// </summary>
    public Dictionary<string, DiskUsage> DiskUsage { get; set; } = new();

    /// <summary>
    /// Active thread count
    /// </summary>
    public int? ThreadCount { get; set; }

    /// <summary>
    /// Handle count (Windows) or file descriptor count (Unix)
    /// </summary>
    public int? HandleCount { get; set; }

    /// <summary>
    /// Network bytes sent per second
    /// </summary>
    public long? NetworkBytesSentPerSecond { get; set; }

    /// <summary>
    /// Network bytes received per second
    /// </summary>
    public long? NetworkBytesReceivedPerSecond { get; set; }

    /// <summary>
    /// Average response time for requests (milliseconds)
    /// </summary>
    public double? AverageResponseTimeMs { get; set; }

    /// <summary>
    /// Request rate per second
    /// </summary>
    public double? RequestsPerSecond { get; set; }

    /// <summary>
    /// Error rate percentage
    /// </summary>
    public double? ErrorRatePercent { get; set; }

    /// <summary>
    /// Garbage collection information
    /// </summary>
    public GarbageCollectionMetrics? GCMetrics { get; set; }
}

/// <summary>
/// Disk usage information for a specific drive
/// </summary>
public class DiskUsage
{
    /// <summary>
    /// Drive or mount point name
    /// </summary>
    public string Drive { get; set; } = string.Empty;

    /// <summary>
    /// Total disk space in bytes
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// Available free space in bytes
    /// </summary>
    public long FreeBytes { get; set; }

    /// <summary>
    /// Used space in bytes
    /// </summary>
    public long UsedBytes => TotalBytes - FreeBytes;

    /// <summary>
    /// Usage percentage (0-100)
    /// </summary>
    public double UsagePercent => TotalBytes > 0 ? (double)UsedBytes / TotalBytes * 100 : 0;
}

/// <summary>
/// Garbage collection metrics
/// </summary>
public class GarbageCollectionMetrics
{
    /// <summary>
    /// Number of Gen 0 collections
    /// </summary>
    public int Gen0Collections { get; set; }

    /// <summary>
    /// Number of Gen 1 collections
    /// </summary>
    public int Gen1Collections { get; set; }

    /// <summary>
    /// Number of Gen 2 collections
    /// </summary>
    public int Gen2Collections { get; set; }

    /// <summary>
    /// Total allocated memory in bytes
    /// </summary>
    public long TotalAllocatedBytes { get; set; }

    /// <summary>
    /// Time spent in GC as percentage
    /// </summary>
    public double? TimeInGCPercent { get; set; }
}