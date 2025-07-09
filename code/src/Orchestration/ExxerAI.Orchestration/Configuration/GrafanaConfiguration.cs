namespace ExxerAI.Orchestration.Configuration;

public class GrafanaConfiguration
{
    public int Port { get; set; } = 3002;
    public string AdminUsername { get; set; } = "admin";
    public string AdminPassword { get; set; } = "admin";
    public bool EnableAnonymousAccess { get; set; } = false;
}