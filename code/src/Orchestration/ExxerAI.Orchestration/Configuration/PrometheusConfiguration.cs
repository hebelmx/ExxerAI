namespace ExxerAI.Orchestration.Configuration;

public class PrometheusConfiguration
{
    public int Port { get; set; } = 9090;
    public string ConfigPath { get; set; } = "./monitoring/prometheus.yml";
    public TimeSpan ScrapeInterval { get; set; } = TimeSpan.FromSeconds(15);
}