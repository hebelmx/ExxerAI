namespace ExxerAI.Orchestration.Configuration;

/// <summary>
/// Network and gateway configuration
/// </summary>
public class NetworkConfiguration
{
    public NginxConfiguration Nginx { get; set; } = new();
    public RedisConfiguration Redis { get; set; } = new();
}