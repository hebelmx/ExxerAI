namespace ExxerAI.Orchestration.Models;

/// <summary>
/// Health check configuration settings
/// </summary>
public class HealthCheckSettings
{
    public int TimeoutSeconds { get; set; } = 10;
    public int CheckIntervalSeconds { get; set; } = 30;
    public bool EnableDetailedChecks { get; set; } = true;
}