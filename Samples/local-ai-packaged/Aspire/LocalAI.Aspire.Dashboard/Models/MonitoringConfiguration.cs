namespace LocalAI.Aspire.Dashboard.Models;

/// <summary>
/// Configuration for monitoring LocalAI stack services
/// </summary>
public class MonitoringConfiguration
{
    public const string SectionName = "Monitoring";
    
    public ServiceEndpoints Services { get; set; } = new();
    public HealthCheckSettings HealthChecks { get; set; } = new();
}

/// <summary>
/// Service endpoint configuration for health monitoring
/// </summary>
public class ServiceEndpoints
{
    public string LocalAI { get; set; } = "http://localhost:8081";
    public string SearXNG { get; set; } = "http://localhost:8080";
    public string PostgreSQL { get; set; } = "http://localhost:5432";
    public string Redis { get; set; } = "http://localhost:6379";
    public string Qdrant { get; set; } = "http://localhost:6333";
    public string Prometheus { get; set; } = "http://localhost:9090";
    public string Grafana { get; set; } = "http://localhost:3002";
}

/// <summary>
/// Health check configuration settings
/// </summary>
public class HealthCheckSettings
{
    public int TimeoutSeconds { get; set; } = 10;
    public int CheckIntervalSeconds { get; set; } = 30;
    public bool EnableDetailedChecks { get; set; } = true;
}

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
