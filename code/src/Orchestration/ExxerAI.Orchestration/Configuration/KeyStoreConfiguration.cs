namespace ExxerAI.Orchestration.Configuration;

/// <summary>
/// Key store configuration
/// </summary>
public class KeyStoreConfiguration
{
    public string StorePath { get; set; } = ""; // Empty means default path
    public string EncryptionKey { get; set; } = ""; // Empty means machine-based key
    public bool EnableAutoRotation { get; set; } = false;
    public TimeSpan RotationInterval { get; set; } = TimeSpan.FromDays(90);
    public bool EnableEnvironmentOverrides { get; set; } = true;
}