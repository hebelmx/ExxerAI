namespace ExxerAI.Aspire.AppHost;

/// <summary>
/// Provides extension methods for adding N8N container resources to the application model.
/// This allows for easy integration of N8N workflows into the distributed application architecture.
/// </summary>
public static class N8NBuilderExtensions
{
    /// <summary>
    /// Adds a N8N container to the application model. The default image is "n8nio/n8n".
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="port">The host port for N8N.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{ContainerResource}"/>.</returns>
    public static IResourceBuilder<ContainerResource> AddN8NContainer(
        this IDistributedApplicationBuilder builder,
        string name = "n8n",
        int? port = 5678)
    {
        var timeZone = GetTimeZone();

        return builder
            .AddContainer(name, "n8nio/n8n")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithVolume("sqlserver_data", "/var/opt/mssql",
                isReadOnly: false) // Uncomment this line to use a persistent volume for SQL Server data on prod
            .WithEnvironment("TZ", timeZone)
            .WithHttpEndpoint(port: port, targetPort: 5678);
    }

    private static string GetTimeZone()
    {
        var timeZone = TimeZoneInfo.Local;
        return timeZone.Id;
    }
}