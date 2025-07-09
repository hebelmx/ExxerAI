namespace ExxerAI.Orchestration.Services;

/// <summary>
/// Individual service status
/// </summary>
public class ServiceStatus
{
    public string Name { get; set; } = "";
    public ServiceHealthStatus Status { get; set; }
    public string Url { get; set; } = "";
    public ServiceCategory Category { get; set; }
    public bool IsCritical { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public int StatusCode { get; set; }
    public DateTime LastChecked { get; set; }
    public string Message { get; set; } = "";
}