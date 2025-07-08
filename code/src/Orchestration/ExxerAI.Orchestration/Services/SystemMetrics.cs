namespace ExxerAI.Orchestration.Services;

/// <summary>
/// System performance metrics
/// </summary>
public class SystemMetrics
{
    public double CpuUsage { get; set; }
    public long MemoryUsage { get; set; }
    public long DiskSpace { get; set; }
    public int NetworkConnections { get; set; }
    public TimeSpan Uptime { get; set; }
    public int ProcessCount { get; set; }
    public int ThreadCount { get; set; }
    public DateTime Timestamp { get; set; }
}