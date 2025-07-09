namespace ExxerAI.Orchestration.Configuration;

/// <summary>
/// Security and authentication configuration
/// Note: Encryption keys and secrets are managed by KeyStore
/// </summary>
public class SecurityConfiguration
{
    public bool EnableAuthentication { get; set; } = false;
    public string[] AllowedOrigins { get; set; } = { "http://localhost:3001", "http://localhost:8080" };
    public string EncryptionKey { get; set; } = ""; // Managed by KeyStore
    public TimeSpan TokenExpiration { get; set; } = TimeSpan.FromHours(24);
}