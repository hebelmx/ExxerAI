namespace ExxerAI.Orchestration.Models;

/// <summary>
/// Service health status
/// </summary>
public class ServiceHealth
{
    public string ServiceName { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = "";
    public TimeSpan ResponseTime { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime LastChecked { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}