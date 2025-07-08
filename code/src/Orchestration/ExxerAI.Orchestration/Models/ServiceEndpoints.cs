namespace ExxerAI.Orchestration.Models;

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