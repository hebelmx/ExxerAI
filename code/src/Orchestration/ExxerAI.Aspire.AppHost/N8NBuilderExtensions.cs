using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using System;
using System.Collections.Generic;

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

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using System;
using System.Collections.Generic;

namespace ExxerAI.Aspire.AppHost
{
    /// <summary>
    /// Represents a specialized container resource for the N8N automation platform,
    /// with Aspire integration and enriched configuration exposure.
    /// </summary>
    public class N8NResource : ContainerResource, IResourceWithConnectionString, IManifestExpressionProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="N8NResource"/> class.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        public N8NResource(string name) : base(name) { }

        /// <summary>
        /// Provides a standard connection string (i.e., public HTTP URL) for downstream binding.
        /// </summary>
        public string ConnectionString => $"http://{this.GetEndpoint(EndpointNames.Http)?.Url}";

        /// <summary>
        /// Exposes manifest expressions used by Aspire for service discovery and binding.
        /// </summary>
        /// <returns>List of key-value manifest expressions.</returns>
        public IEnumerable<ManifestExpression> GetExpressions()
        {
            yield return new ManifestExpression("connectionString", () => ConnectionString);
        }
    }

    /// <summary>
    /// Extension methods for registering N8N container resources in a distributed Aspire application.
    /// </summary>
    public static class N8NResourceBuilderExtensions
    {
        /// <summary>
        /// Adds a fully-integrated N8N container resource to the Aspire application builder.
        /// </summary>
        /// <param name="builder">The distributed application builder.</param>
        /// <param name="name">Resource name identifier.</param>
        /// <param name="port">Optional: external port to expose N8N's web interface.</param>
        /// <returns>A builder reference to the enriched <see cref="N8NResource"/>.</returns>
        public static IResourceBuilder<N8NResource> AddN8N(
            this IDistributedApplicationBuilder builder,
            string name = "n8n",
            int? port = 5678)
        {
            var timeZone = TimeZoneInfo.Local.Id;

            // Construct the resource object and register it to the Aspire model
            var resource = new N8NResource(name);
            var builderResource = builder.AddResource(resource)
                .WithImage("n8nio/n8n")
                .WithHttpEndpoint(port: port, targetPort: 5678)
                .WithEnvironment("TZ", timeZone)
                .WithVolume("sqlserver_data", "/var/opt/mssql", isReadOnly: false);

            return builderResource;
        }
    }
}

namespace ExxerAI.Aspire.AppHost
{
    /// <summary>
    /// Represents a specialized container resource for the N8N automation platform,
    /// with Aspire integration and enriched configuration exposure.
    /// </summary>
    public class N8NResource : ContainerResource, IResourceWithConnectionString, IManifestExpressionProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="N8NResource"/> class.
        /// </summary>
        /// <param name="name">The name of the resource.</param>
        public N8NResource(string name) : base(name) { }

        /// <summary>
        /// Provides a standard connection string (i.e., public HTTP URL) for downstream binding.
        /// </summary>
        public string ConnectionString => $"http://{this.GetEndpoint(EndpointNames.Http)?.Url}";

        /// <summary>
        /// Exposes manifest expressions used by Aspire for service discovery and binding.
        /// </summary>
        /// <returns>List of key-value manifest expressions.</returns>
        public IEnumerable<ManifestExpression> GetExpressions()
        {
            yield return new ManifestExpression("connectionString", () => ConnectionString);
        }
    }

    /// <summary>
    /// Extension methods for registering N8N container resources in a distributed Aspire application.
    /// </summary>
    public static class N8NResourceBuilderExtensions
    {
        /// <summary>
        /// Adds a fully-integrated N8N container resource to the Aspire application builder.
        /// </summary>
        /// <param name="builder">The distributed application builder.</param>
        /// <param name="name">Resource name identifier.</param>
        /// <param name="port">Optional: external port to expose N8N's web interface.</param>
        /// <returns>A builder reference to the enriched <see cref="N8NResource"/>.</returns>
        public static IResourceBuilder<N8NResource> AddN8N(
            this IDistributedApplicationBuilder builder,
            string name = "n8n",
            int? port = 5678)
        {
            var timeZone = TimeZoneInfo.Local.Id;

            // Construct the resource object and register it to the Aspire model
            var resource = new N8NResource(name);
            var builderResource = builder.AddResource(resource)
                .WithImage("n8nio/n8n")
                .WithHttpEndpoint(port: port, targetPort: 5678)
                .WithEnvironment("TZ", timeZone)
                .WithVolume("sqlserver_data", "/var/opt/mssql", isReadOnly: false);

            return builderResource;
        }
    }

    /// <summary>
    /// Fluent API extensions for configuring an N8N Aspire resource.
    /// </summary>
    public static class N8NFluentBuilderExtensions
    {
        /// <summary>
        /// Specifies the mount path for the workflows directory used by N8N.
        /// </summary>
        public static IResourceBuilder<N8NResource> WithWorkflowsDirectory(
            this IResourceBuilder<N8NResource> builder,
            string hostPath, string containerPath = "/home/node/.n8n")
        {
            return builder.WithVolume(hostPath, containerPath, isReadOnly: false);
        }

        /// <summary>
        /// Sets basic authentication credentials via environment variables.
        /// </summary>
        public static IResourceBuilder<N8NResource> WithBasicAuth(
            this IResourceBuilder<N8NResource> builder,
            string user, string password)
        {
            return builder
                .WithEnvironment("N8N_BASIC_AUTH_ACTIVE", "true")
                .WithEnvironment("N8N_BASIC_AUTH_USER", user)
                .WithEnvironment("N8N_BASIC_AUTH_PASSWORD", password);
        }

        /// <summary>
        /// Configures the timezone for the N8N container.
        /// </summary>
        public static IResourceBuilder<N8NResource> WithTimeZone(
            this IResourceBuilder<N8NResource> builder, string timeZone)
        {
            return builder.WithEnvironment("TZ", timeZone);
        }

        /// <summary>
        /// Configures a custom HTTP proxy environment variable.
        /// </summary>
        public static IResourceBuilder<N8NResource> WithProxy(
            this IResourceBuilder<N8NResource> builder, string proxyUrl)
        {
            return builder.WithEnvironment("HTTP_PROXY", proxyUrl);
        }
    }
}