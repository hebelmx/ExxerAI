namespace ExxerAI.Orchestration.Services;

/// <summary>
/// Overall status of the service stack
/// </summary>
public class ServiceStackStatus
{
    public OverallHealthStatus OverallStatus { get; set; }
    public bool CoreServicesHealthy { get; set; }
    public bool OptionalServicesHealthy { get; set; }
    public int TotalServices { get; set; }
    public int HealthyServices { get; set; }
    public int UnhealthyServices { get; set; }
    public int UnknownServices { get; set; }
    public List<ServiceStatus> Services { get; set; } = [];
    public DateTime LastChecked { get; set; }
}