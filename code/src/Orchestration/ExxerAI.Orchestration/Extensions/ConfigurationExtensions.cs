using ExxerAI.Orchestration.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ExxerAI.Orchestration.Extensions;

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