using ExxerAI.Orchestration.Configuration;
using ExxerAI.Orchestration.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace ExxerAI.Orchestration.Services;

/// <summary>
/// Manages all API keys and secrets for the LocalAI stack
/// Provides strongly-typed access to all service credentials
/// </summary>
public class LocalAIKeyManager
{
    private readonly IKeyStore _keyStore;
    private readonly ILogger<LocalAIKeyManager> _logger;

    // Key name constants for consistent access
    public static class KeyNames
    {
        // Database keys
        public const string DatabasePassword = "database-password";

        public const string DatabaseUsername = "database-username";

        // Supabase keys
        public const string SupabaseJwtSecret = "supabase-jwt-secret";

        public const string SupabaseAnonKey = "supabase-anon-key";
        public const string SupabaseServiceRoleKey = "supabase-service-role-key";

        // LocalAI keys
        public const string LocalAIApiKey = "localai-api-key";

        public const string OpenWebUISecretKey = "open-webui-secret-key";

        // Vector database keys
        public const string QdrantApiKey = "qdrant-api-key";

        public const string MilvusUsername = "milvus-username";
        public const string MilvusPassword = "milvus-password";

        // Monitoring keys
        public const string GrafanaAdminPassword = "grafana-admin-password";

        public const string PrometheusPassword = "prometheus-password";

        // Redis keys
        public const string RedisPassword = "redis-password";

        // Security keys
        public const string EncryptionKey = "encryption-key";

        public const string JwtSigningKey = "jwt-signing-key";

        // External API keys (for integrations)
        public const string OpenAIApiKey = "openai-api-key";

        public const string AnthropicApiKey = "anthropic-api-key";
        public const string GoogleCloudApiKey = "google-cloud-api-key";
        public const string AzureOpenAIApiKey = "azure-openai-api-key";
        public const string HuggingFaceApiKey = "huggingface-api-key";
    }

    public static class Scopes
    {
        public const string Internal = "internal";
        public const string External = "external";
        public const string Development = "development";
        public const string Production = "production";
    }

    public LocalAIKeyManager(IKeyStore keyStore, ILogger<LocalAIKeyManager> logger)
    {
        _keyStore = keyStore;
        _logger = logger;
    }

    /// <summary>
    /// Initialize all required keys with secure defaults or generate new ones
    /// </summary>
    public async Task InitializeKeysAsync(LocalAIStackConfiguration config, CancellationToken cancellationToken, bool regenerateSecrets = false)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return;

        _logger.LogInformation("Initializing LocalAI key store...");

        // Initialize internal service keys
        await InitializeInternalKeysAsync(config, regenerateSecrets, cancellationToken);

        // Initialize external API keys (these will be empty until user provides them)
        await InitializeExternalKeysAsync(cancellationToken);

        _logger.LogInformation("Key store initialization completed");
    }

    /// <summary>
    /// Get a database connection string with secure credentials
    /// </summary>
    public async Task<string> GetDatabaseConnectionStringAsync(DatabaseConfiguration config, CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return string.Empty;

        var username = await _keyStore.GetKeyAsync(KeyNames.DatabaseUsername, Scopes.Internal, cancellationToken) ?? config.Username;
        var password = await _keyStore.GetKeyAsync(KeyNames.DatabasePassword, Scopes.Internal, cancellationToken) ?? config.Password;

        return $"Host={config.Host};Port={config.Port};Database={config.DatabaseName};Username={username};Password={password}";
    }

    /// <summary>
    /// Get Supabase configuration with secure keys
    /// </summary>
    public async Task<(string jwtSecret, string anonKey, string serviceRoleKey)> GetSupabaseKeysAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return (string.Empty, string.Empty, string.Empty);

        var jwtSecret = await _keyStore.GetKeyAsync(KeyNames.SupabaseJwtSecret, Scopes.Internal, cancellationToken);
        var anonKey = await _keyStore.GetKeyAsync(KeyNames.SupabaseAnonKey, Scopes.Internal, cancellationToken);
        var serviceRoleKey = await _keyStore.GetKeyAsync(KeyNames.SupabaseServiceRoleKey, Scopes.Internal, cancellationToken);

        return (jwtSecret!, anonKey!, serviceRoleKey!);
    }

    /// <summary>
    /// Get LocalAI API key
    /// </summary>
    public async Task<string?> GetLocalAIApiKeyAsync(CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return null;

        return await _keyStore.GetKeyAsync(KeyNames.LocalAIApiKey, Scopes.Internal, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get vector database credentials
    /// </summary>
    public async Task<(string? qdrantApiKey, string? milvusUsername, string? milvusPassword)> GetVectorDatabaseKeysAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return (null, null, null);

        var qdrantKey = await _keyStore.GetKeyAsync(KeyNames.QdrantApiKey, Scopes.Internal, cancellationToken);
        var milvusUser = await _keyStore.GetKeyAsync(KeyNames.MilvusUsername, Scopes.Internal, cancellationToken);
        var milvusPass = await _keyStore.GetKeyAsync(KeyNames.MilvusPassword, Scopes.Internal, cancellationToken);

        return (qdrantKey, milvusUser, milvusPass);
    }

    /// <summary>
    /// Get monitoring service credentials
    /// </summary>
    public async Task<(string grafanaPassword, string? prometheusPassword)> GetMonitoringKeysAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return (string.Empty, null);

        var grafanaPassword = await _keyStore.GetKeyAsync(KeyNames.GrafanaAdminPassword, Scopes.Internal, cancellationToken);
        var prometheusPassword = await _keyStore.GetKeyAsync(KeyNames.PrometheusPassword, Scopes.Internal, cancellationToken);

        return (grafanaPassword!, prometheusPassword);
    }

    /// <summary>
    /// Get external API key for third-party integrations
    /// </summary>
    public async Task<string?> GetExternalApiKeyAsync(string provider, CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return null;

        var keyName = provider.ToLowerInvariant() switch
        {
            "openai" => KeyNames.OpenAIApiKey,
            "anthropic" => KeyNames.AnthropicApiKey,
            "google" or "google-cloud" => KeyNames.GoogleCloudApiKey,
            "azure" or "azure-openai" => KeyNames.AzureOpenAIApiKey,
            "huggingface" => KeyNames.HuggingFaceApiKey,
            _ => null
        };

        if (keyName == null)
        {
            _logger.LogWarning("Unknown external API provider: {Provider}", provider);
            return null;
        }

        return await _keyStore.GetKeyAsync(keyName, Scopes.External, cancellationToken);
    }

    /// <summary>
    /// Set external API key for third-party integrations
    /// </summary>
    public async Task SetExternalApiKeyAsync(string provider, string apiKey, CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return;

        var keyName = provider.ToLowerInvariant() switch
        {
            "openai" => KeyNames.OpenAIApiKey,
            "anthropic" => KeyNames.AnthropicApiKey,
            "google" or "google-cloud" => KeyNames.GoogleCloudApiKey,
            "azure" or "azure-openai" => KeyNames.AzureOpenAIApiKey,
            "huggingface" => KeyNames.HuggingFaceApiKey,
            _ => null
        };

        if (keyName == null)
        {
            throw new ArgumentException($"Unknown external API provider: {provider}");
        }

        await _keyStore.SetKeyAsync(keyName, apiKey, Scopes.External, cancellationToken: cancellationToken);
        _logger.LogInformation("Set external API key for provider: {Provider}", provider);
    }

    /// <summary>
    /// Rotate all internal secrets (useful for security maintenance)
    /// </summary>
    public async Task RotateInternalSecretsAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return;

        _logger.LogInformation("Rotating internal secrets...");

        var secretsToRotate = new[]
        {
            KeyNames.SupabaseJwtSecret,
            KeyNames.LocalAIApiKey,
            KeyNames.OpenWebUISecretKey,
            KeyNames.EncryptionKey,
            KeyNames.JwtSigningKey
        };

        foreach (var secret in secretsToRotate)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var newValue = await _keyStore.GenerateApiKeyAsync($"{secret}-new", Scopes.Internal, 64, cancellationToken);
            await _keyStore.RotateKeyAsync(secret, newValue, Scopes.Internal, cancellationToken);
        }

        _logger.LogInformation("Internal secrets rotation completed");
    }

    /// <summary>
    /// List all available keys (for management/debugging)
    /// </summary>
    public async Task<Dictionary<string, IEnumerable<string>>> ListAllKeysAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return new Dictionary<string, IEnumerable<string>>();

        return new Dictionary<string, IEnumerable<string>>
        {
            [Scopes.Internal] = await _keyStore.ListKeysAsync(Scopes.Internal, cancellationToken),
            [Scopes.External] = await _keyStore.ListKeysAsync(Scopes.External, cancellationToken),
            [Scopes.Development] = await _keyStore.ListKeysAsync(Scopes.Development, cancellationToken),
            [Scopes.Production] = await _keyStore.ListKeysAsync(Scopes.Production, cancellationToken)
        };
    }

    private async Task InitializeInternalKeysAsync(LocalAIStackConfiguration config, bool regenerateSecrets, CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return;

        // Database credentials
        await EnsureKeyExistsAsync(KeyNames.DatabaseUsername, config.Database.Username, Scopes.Internal, regenerateSecrets, cancellationToken);
        await EnsureKeyExistsAsync(KeyNames.DatabasePassword, config.Database.Password, Scopes.Internal, regenerateSecrets, cancellationToken);

        // Supabase keys
        await EnsureKeyExistsAsync(KeyNames.SupabaseJwtSecret, config.Database.Supabase.JwtSecret, Scopes.Internal, regenerateSecrets, cancellationToken);
        await EnsureKeyExistsAsync(KeyNames.SupabaseAnonKey, config.Database.Supabase.AnonKey, Scopes.Internal, regenerateSecrets, cancellationToken);
        await EnsureKeyExistsAsync(KeyNames.SupabaseServiceRoleKey, config.Database.Supabase.ServiceRoleKey, Scopes.Internal, regenerateSecrets, cancellationToken);

        // Generate LocalAI API key if not set
        if (!await _keyStore.KeyExistsAsync(KeyNames.LocalAIApiKey, Scopes.Internal, cancellationToken) || regenerateSecrets)
        {
            await _keyStore.GenerateApiKeyAsync(KeyNames.LocalAIApiKey, Scopes.Internal, cancellationToken: cancellationToken);
        }

        // Generate Open WebUI secret key
        if (!await _keyStore.KeyExistsAsync(KeyNames.OpenWebUISecretKey, Scopes.Internal, cancellationToken) || regenerateSecrets)
        {
            await _keyStore.GenerateApiKeyAsync(KeyNames.OpenWebUISecretKey, Scopes.Internal, 64, cancellationToken);
        }

        // Monitoring credentials
        await EnsureKeyExistsAsync(KeyNames.GrafanaAdminPassword, config.Monitoring.Grafana.AdminPassword, Scopes.Internal, regenerateSecrets, cancellationToken);

        // Security keys
        if (!await _keyStore.KeyExistsAsync(KeyNames.EncryptionKey, Scopes.Internal, cancellationToken) || regenerateSecrets)
        {
            await _keyStore.GenerateApiKeyAsync(KeyNames.EncryptionKey, Scopes.Internal, 32, cancellationToken);
        }

        if (!await _keyStore.KeyExistsAsync(KeyNames.JwtSigningKey, Scopes.Internal, cancellationToken) || regenerateSecrets)
        {
            await _keyStore.GenerateApiKeyAsync(KeyNames.JwtSigningKey, Scopes.Internal, 64, cancellationToken);
        }
    }

    private async Task InitializeExternalKeysAsync(CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return;

        // Initialize external API key placeholders (empty until user provides them)
        var externalKeys = new[]
        {
            KeyNames.OpenAIApiKey,
            KeyNames.AnthropicApiKey,
            KeyNames.GoogleCloudApiKey,
            KeyNames.AzureOpenAIApiKey,
            KeyNames.HuggingFaceApiKey
        };

        foreach (var key in externalKeys)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (!await _keyStore.KeyExistsAsync(key, Scopes.External, cancellationToken))
            {
                await _keyStore.SetKeyAsync(key, "", Scopes.External, cancellationToken: cancellationToken);
            }
        }
    }

    private async Task EnsureKeyExistsAsync(string keyName, string defaultValue, string scope, bool regenerate, CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return;

        if (!await _keyStore.KeyExistsAsync(keyName, scope, cancellationToken) || regenerate)
        {
            await _keyStore.SetKeyAsync(keyName, defaultValue, scope, cancellationToken: cancellationToken);
        }
    }
}