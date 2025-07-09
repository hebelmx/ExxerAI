namespace ExxerAI.Orchestration.Configuration;

/// <summary>
/// Root configuration for the entire LocalAI stack
/// Note: Sensitive values (passwords, API keys) are managed by the SecureKeyStore
/// </summary>
public class LocalAIStackConfiguration
{
    public const string SectionName = "LocalAIStack";
    
    public DatabaseConfiguration Database { get; set; } = new();
    public LocalAIConfiguration LocalAI { get; set; } = new();
    public SearchConfiguration Search { get; set; } = new();
    public VectorDatabaseConfiguration VectorDatabases { get; set; } = new();
    public MonitoringConfiguration Monitoring { get; set; } = new();
    public NetworkConfiguration Network { get; set; } = new();
    public SecurityConfiguration Security { get; set; } = new();
    public KeyStoreConfiguration KeyStore { get; set; } = new();
}