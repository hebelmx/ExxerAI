using LocalAI.Aspire.AppHost.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LocalAI.Aspire.AppHost.Services;

/// <summary>
/// Service to provide strongly-typed configuration to the orchestrator
/// Integrates with SecureKeyStore for sensitive data management
/// </summary>
public class ConfigurationService
{
    public LocalAIStackConfiguration Configuration { get; }
    private readonly LocalAIKeyManager _keyManager;
    private readonly ILogger<ConfigurationService>? _logger;

    public ConfigurationService(IConfiguration configuration, LocalAIKeyManager? keyManager = null, ILogger<ConfigurationService>? logger = null)
    {
        Configuration = new LocalAIStackConfiguration();
        configuration.GetSection(LocalAIStackConfiguration.SectionName).Bind(Configuration);
        _keyManager = keyManager!;
        _logger = logger;
        
        // Apply environment variable overrides if key manager is not available
        if (_keyManager == null)
        {
            ApplyEnvironmentOverrides();
        }
    }

    /// <summary>
    /// Initialize the service with key management
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_keyManager != null)
        {
            await _keyManager.InitializeKeysAsync(Configuration);
            _logger?.LogInformation("Configuration service initialized with secure key management");
        }
        else
        {
            _logger?.LogWarning("Configuration service initialized without key management - using fallback values");
        }
    }

    /// <summary>
    /// Apply environment variable overrides for sensitive data
    /// </summary>
    private void ApplyEnvironmentOverrides()
    {
        // Database overrides
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOCALAI_DB_PASSWORD")))
            Configuration.Database.Password = Environment.GetEnvironmentVariable("LOCALAI_DB_PASSWORD")!;
            
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOCALAI_DB_USERNAME")))
            Configuration.Database.Username = Environment.GetEnvironmentVariable("LOCALAI_DB_USERNAME")!;

        // Supabase overrides
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET")))
            Configuration.Database.Supabase.JwtSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET")!;
            
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY")))
            Configuration.Database.Supabase.AnonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY")!;

        // LocalAI overrides
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOCALAI_API_KEY")))
            Configuration.LocalAI.ApiKey = Environment.GetEnvironmentVariable("LOCALAI_API_KEY")!;

        // Vector DB overrides
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("QDRANT_API_KEY")))
            Configuration.VectorDatabases.Qdrant.ApiKey = Environment.GetEnvironmentVariable("QDRANT_API_KEY")!;

        // Security overrides
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOCALAI_ENCRYPTION_KEY")))
            Configuration.Security.EncryptionKey = Environment.GetEnvironmentVariable("LOCALAI_ENCRYPTION_KEY")!;
    }

    /// <summary>
    /// Get all service URLs for display
    /// </summary>
    public Dictionary<string, string> GetServiceUrls()
    {
        return new Dictionary<string, string>
        {
            ["Aspire Dashboard"] = "http://localhost:15000",
            ["Health Dashboard"] = "http://localhost:5555",
            ["LocalAI API"] = $"http://localhost:{Configuration.LocalAI.ApiPort}/v1",
            ["Open WebUI"] = $"http://localhost:{Configuration.LocalAI.WebUIPort}",
            ["SearXNG"] = $"http://localhost:{Configuration.Search.Port}",
            ["Supabase REST"] = $"http://localhost:{Configuration.Database.Supabase.RestPort}",
            ["Supabase Auth"] = $"http://localhost:{Configuration.Database.Supabase.AuthPort}",
            ["Qdrant"] = $"http://localhost:{Configuration.VectorDatabases.Qdrant.Port}",
            ["Milvus"] = $"http://localhost:{Configuration.VectorDatabases.Milvus.WebPort}",
            ["Prometheus"] = $"http://localhost:{Configuration.Monitoring.Prometheus.Port}",
            ["Grafana"] = $"http://localhost:{Configuration.Monitoring.Grafana.Port}",
            ["Nginx Gateway"] = $"http://localhost:{Configuration.Network.Nginx.HttpPort}",
            ["Redis"] = $"redis://localhost:{Configuration.Network.Redis.Port}"
        };
    }

    /// <summary>
    /// Display service URLs in a formatted way
    /// </summary>
    public void DisplayServiceUrls()
    {
        Console.WriteLine();
        Console.WriteLine("🌐 Service URLs (available after startup):");
        
        foreach (var service in GetServiceUrls())
        {
            Console.WriteLine($"  • {service.Key,-20}: {service.Value}");
        }
        
        Console.WriteLine();
        Console.WriteLine("⏳ Starting Aspire orchestrator...");
        Console.WriteLine("   This will download Docker images on first run (may take several minutes)");
    }

    /// <summary>
    /// Get secure database connection string using KeyStore
    /// </summary>
    public async Task<string> GetSecureDatabaseConnectionStringAsync()
    {
        if (_keyManager != null)
        {
            return await _keyManager.GetDatabaseConnectionStringAsync(Configuration.Database);
        }
        
        // Fallback to configuration values
        return GetDatabaseConnectionString();
    }

    /// <summary>
    /// Get LocalAI API key from secure store
    /// </summary>
    public async Task<string?> GetSecureLocalAIApiKeyAsync()
    {
        if (_keyManager != null)
        {
            return await _keyManager.GetLocalAIApiKeyAsync();
        }
        
        return Configuration.LocalAI.ApiKey;
    }

    /// <summary>
    /// Get external API key for third-party integrations
    /// </summary>
    public async Task<string?> GetExternalApiKeyAsync(string provider)
    {
        if (_keyManager != null)
        {
            return await _keyManager.GetExternalApiKeyAsync(provider);
        }
        
        // Fallback to environment variables
        var envVarName = $"LOCALAI_EXTERNAL_{provider.ToUpperInvariant()}_API_KEY";
        return Environment.GetEnvironmentVariable(envVarName);
    }

    /// <summary>
    /// Set external API key for third-party integrations
    /// </summary>
    public async Task SetExternalApiKeyAsync(string provider, string apiKey)
    {
        if (_keyManager != null)
        {
            await _keyManager.SetExternalApiKeyAsync(provider, apiKey);
        }
        else
        {
            _logger?.LogWarning("Cannot set external API key - key manager not available");
        }
    }

    /// <summary>
    /// Generate connection string for database (fallback method)
    /// </summary>
    public string GetDatabaseConnectionString()
    {
        var db = Configuration.Database;
        return $"Host={db.Host};Port={db.Port};Database={db.DatabaseName};Username={db.Username};Password={db.Password}";
    }

    /// <summary>
    /// Get LocalAI API URL
    /// </summary>
    public string GetLocalAIApiUrl()
    {
        return $"http://localhost:{Configuration.LocalAI.ApiPort}/v1";
    }
}
