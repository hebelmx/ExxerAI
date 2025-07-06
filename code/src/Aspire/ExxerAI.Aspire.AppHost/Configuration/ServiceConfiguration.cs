using System.ComponentModel.DataAnnotations;

namespace ExxerAII.Aspire.AppHost.Configuration;

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

/// <summary>
/// Database (Supabase/PostgreSQL) configuration
/// Note: Username and Password are stored securely in KeyStore
/// </summary>
public class DatabaseConfiguration
{
    [Required]
    public string DatabaseName { get; set; } = "localai_db";
    
    [Required]
    public string Username { get; set; } = "postgres"; // Default fallback, actual value from KeyStore
    
    [Required]
    public string Password { get; set; } = "postgres"; // Default fallback, actual value from KeyStore
    
    public int Port { get; set; } = 5432;
    
    public string Host { get; set; } = "localhost";
    
    // Supabase specific settings
    public SupabaseConfiguration Supabase { get; set; } = new();
}

/// <summary>
/// Supabase service configuration
/// Note: All secrets (JWT, API keys) are managed by KeyStore
/// </summary>
public class SupabaseConfiguration
{
    // These are fallback values; actual secrets stored in KeyStore
    public string JwtSecret { get; set; } = "your-super-secret-jwt-token-with-at-least-32-characters-long";
    public string AnonKey { get; set; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6ImFub24iLCJleHAiOjE5ODM4MTI5OTZ9.CRXP1A7WOeoJeXxjNni43kdQwgnWNReilDMblYTn_I0";
    public string ServiceRoleKey { get; set; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImV4cCI6MTk4MzgxMjk5Nn0.EGIM96RAZx35lJzdJsyH-qQwv8Hdp7fsn3W0YpN81IU";
    public int RestPort { get; set; } = 3000;
    public int AuthPort { get; set; } = 9999;
    public int RealtimePort { get; set; } = 4000;
    public int StoragePort { get; set; } = 5000;
}

/// <summary>
/// LocalAI and Open WebUI configuration
/// Note: API keys are managed by KeyStore
/// </summary>
public class LocalAIConfiguration
{
    public int ApiPort { get; set; } = 8081;
    public int WebUIPort { get; set; } = 3001;
    public string ModelsPath { get; set; } = "./models";
    public string ApiKey { get; set; } = ""; // Managed by KeyStore
    public bool EnableCors { get; set; } = true;
    public int Workers { get; set; } = 1;
    public string DefaultModel { get; set; } = "llama-3.2-3b-instruct";
    public ExternalAPIConfiguration ExternalAPIs { get; set; } = new();
}

/// <summary>
/// Search engine (SearXNG) configuration
/// </summary>
public class SearchConfiguration
{
    public int Port { get; set; } = 8080;
    public string BaseUrl { get; set; } = "http://localhost:8080/";
    public bool EnableSafeSearch { get; set; } = true;
    public string[] DefaultEngines { get; set; } = { "google", "bing", "duckduckgo" };
}

/// <summary>
/// Vector databases configuration
/// </summary>
public class VectorDatabaseConfiguration
{
    public QdrantConfiguration Qdrant { get; set; } = new();
    public MilvusConfiguration Milvus { get; set; } = new();
}

public class QdrantConfiguration
{
    public int Port { get; set; } = 6333;
    public int GrpcPort { get; set; } = 6334;
    public string ApiKey { get; set; } = "";
    public string StoragePath { get; set; } = "./qdrant_storage";
}

public class MilvusConfiguration
{
    public int Port { get; set; } = 19530;
    public int WebPort { get; set; } = 9091;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string StoragePath { get; set; } = "./milvus_data";
}

/// <summary>
/// Monitoring stack configuration
/// </summary>
public class MonitoringConfiguration
{
    public PrometheusConfiguration Prometheus { get; set; } = new();
    public GrafanaConfiguration Grafana { get; set; } = new();
}

public class PrometheusConfiguration
{
    public int Port { get; set; } = 9090;
    public string ConfigPath { get; set; } = "./monitoring/prometheus.yml";
    public TimeSpan ScrapeInterval { get; set; } = TimeSpan.FromSeconds(15);
}

public class GrafanaConfiguration
{
    public int Port { get; set; } = 3002;
    public string AdminUsername { get; set; } = "admin";
    public string AdminPassword { get; set; } = "admin";
    public bool EnableAnonymousAccess { get; set; } = false;
}

/// <summary>
/// Network and gateway configuration
/// </summary>
public class NetworkConfiguration
{
    public NginxConfiguration Nginx { get; set; } = new();
    public RedisConfiguration Redis { get; set; } = new();
}

public class NginxConfiguration
{
    public int HttpPort { get; set; } = 80;
    public int HttpsPort { get; set; } = 443;
    public string ConfigPath { get; set; } = "./nginx/nginx.conf";
    public bool EnableSsl { get; set; } = false;
}

public class RedisConfiguration
{
    public int Port { get; set; } = 6379;
    public string Password { get; set; } = "";
    public int Database { get; set; } = 0;
}

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

/// <summary>
/// External API integrations configuration
/// Note: All API keys are managed by KeyStore
/// </summary>
public class ExternalAPIConfiguration
{
    public bool EnableOpenAI { get; set; } = false;
    public bool EnableAnthropic { get; set; } = false;
    public bool EnableGoogleCloud { get; set; } = false;
    public bool EnableAzureOpenAI { get; set; } = false;
    public bool EnableHuggingFace { get; set; } = false;
    
    // Configuration endpoints (keys stored in KeyStore)
    public string OpenAIEndpoint { get; set; } = "https://api.openai.com/v1";
    public string AnthropicEndpoint { get; set; } = "https://api.anthropic.com";
    public string AzureOpenAIEndpoint { get; set; } = "";
    public string HuggingFaceEndpoint { get; set; } = "https://api-inference.huggingface.co";
}

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
