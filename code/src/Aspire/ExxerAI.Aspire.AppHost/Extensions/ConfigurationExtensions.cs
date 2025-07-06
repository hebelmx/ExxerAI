using ExxerAI.Aspire.AppHost.Configuration;

namespace ExxerAI.Aspire.AppHost.Extensions;

/// <summary>
/// Extension methods for registering configuration services
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Register all LocalAI stack configurations with the DI container
    /// </summary>
    public static IServiceCollection AddLocalAIConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register the main configuration
        services.Configure<LocalAIStackConfiguration>(
            configuration.GetSection(LocalAIStackConfiguration.SectionName));
        
        // Register individual service configurations for easier injection
        services.Configure<DatabaseConfiguration>(
            configuration.GetSection($"{LocalAIStackConfiguration.SectionName}:Database"));
            
        services.Configure<LocalAIConfiguration>(
            configuration.GetSection($"{LocalAIStackConfiguration.SectionName}:LocalAI"));
            
        services.Configure<SearchConfiguration>(
            configuration.GetSection($"{LocalAIStackConfiguration.SectionName}:Search"));
            
        services.Configure<VectorDatabaseConfiguration>(
            configuration.GetSection($"{LocalAIStackConfiguration.SectionName}:VectorDatabases"));
            
        services.Configure<MonitoringConfiguration>(
            configuration.GetSection($"{LocalAIStackConfiguration.SectionName}:Monitoring"));
            
        services.Configure<NetworkConfiguration>(
            configuration.GetSection($"{LocalAIStackConfiguration.SectionName}:Network"));
            
        services.Configure<SecurityConfiguration>(
            configuration.GetSection($"{LocalAIStackConfiguration.SectionName}:Security"));

        // Add configuration validation
        services.AddSingleton<IValidateOptions<LocalAIStackConfiguration>, LocalAIStackConfigurationValidator>();
        
        return services;
    }

    /// <summary>
    /// Get typed configuration from the service provider
    /// </summary>
    public static T GetConfiguration<T>(this IServiceProvider serviceProvider) where T : class
    {
        var options = serviceProvider.GetRequiredService<IOptions<T>>();
        return options.Value;
    }

    /// <summary>
    /// Generate connection string for database
    /// </summary>
    public static string GetDatabaseConnectionString(this DatabaseConfiguration dbConfig)
    {
        return $"Host={dbConfig.Host};Port={dbConfig.Port};Database={dbConfig.DatabaseName};Username={dbConfig.Username};Password={dbConfig.Password}";
    }

    /// <summary>
    /// Generate LocalAI API base URL
    /// </summary>
    public static string GetLocalAIApiUrl(this LocalAIConfiguration aiConfig)
    {
        return $"http://localhost:{aiConfig.ApiPort}/v1";
    }

    /// <summary>
    /// Generate service URLs for display
    /// </summary>
    public static Dictionary<string, string> GetAllServiceUrls(this LocalAIStackConfiguration config)
    {
        return new Dictionary<string, string>
        {
            ["Aspire Dashboard"] = "http://localhost:15000",
            ["LocalAI API"] = $"http://localhost:{config.LocalAI.ApiPort}/v1",
            ["Open WebUI"] = $"http://localhost:{config.LocalAI.WebUIPort}",
            ["SearXNG"] = $"http://localhost:{config.Search.Port}",
            ["Supabase REST"] = $"http://localhost:{config.Database.Supabase.RestPort}",
            ["Supabase Auth"] = $"http://localhost:{config.Database.Supabase.AuthPort}",
            ["Qdrant"] = $"http://localhost:{config.VectorDatabases.Qdrant.Port}",
            ["Milvus"] = $"http://localhost:{config.VectorDatabases.Milvus.WebPort}",
            ["Prometheus"] = $"http://localhost:{config.Monitoring.Prometheus.Port}",
            ["Grafana"] = $"http://localhost:{config.Monitoring.Grafana.Port}",
            ["Nginx Gateway"] = $"http://localhost:{config.Network.Nginx.HttpPort}",
            ["Redis"] = $"redis://localhost:{config.Network.Redis.Port}"
        };
    }
}

/// <summary>
/// Configuration validator to ensure all required settings are present
/// </summary>
public class LocalAIStackConfigurationValidator : IValidateOptions<LocalAIStackConfiguration>
{
    public ValidateOptionsResult Validate(string? name, LocalAIStackConfiguration options)
    {
        var errors = new List<string>();

        // Validate database configuration
        if (string.IsNullOrWhiteSpace(options.Database.DatabaseName))
            errors.Add("Database name is required");
            
        if (string.IsNullOrWhiteSpace(options.Database.Username))
            errors.Add("Database username is required");

        // Validate ports are not conflicting
        var ports = new[]
        {
            options.Database.Port,
            options.Database.Supabase.RestPort,
            options.Database.Supabase.AuthPort,
            options.LocalAI.ApiPort,
            options.LocalAI.WebUIPort,
            options.Search.Port,
            options.VectorDatabases.Qdrant.Port,
            options.VectorDatabases.Milvus.Port,
            options.Monitoring.Prometheus.Port,
            options.Monitoring.Grafana.Port,
            options.Network.Nginx.HttpPort,
            options.Network.Redis.Port
        };

        if (ports.GroupBy(p => p).Any(g => g.Count() > 1))
            errors.Add("Port conflicts detected - each service must use a unique port");

        // Validate security settings if authentication is enabled
        if (options.Security.EnableAuthentication)
        {
            if (string.IsNullOrWhiteSpace(options.Security.EncryptionKey) || 
                options.Security.EncryptionKey.Length < 32)
            {
                errors.Add("Encryption key must be at least 32 characters when authentication is enabled");
            }
        }

        return errors.Count > 0 
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
