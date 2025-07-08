namespace ExxerAI.Orchestration.Configuration;

/// <summary>
/// Monitoring stack configuration
/// </summary>
public class MonitoringConfiguration
{
    public PrometheusConfiguration Prometheus { get; set; } = new();
    public GrafanaConfiguration Grafana { get; set; } = new();
}