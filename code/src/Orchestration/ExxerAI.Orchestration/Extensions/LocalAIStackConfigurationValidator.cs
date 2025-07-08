using ExxerAI.Orchestration.Configuration;

namespace ExxerAI.Orchestration.Extensions;

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