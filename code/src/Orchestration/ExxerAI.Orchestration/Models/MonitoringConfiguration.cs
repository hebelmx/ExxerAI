namespace ExxerAI.Orchestration.Models;

/// <summary>
/// Configuration for monitoring LocalAI stack services
/// </summary>
public class MonitoringConfiguration
{
    public const string SectionName = "Monitoring";
    
    public ServiceEndpoints Services { get; set; } = new();
    public HealthCheckSettings HealthChecks { get; set; } = new();
}