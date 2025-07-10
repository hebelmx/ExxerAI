using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using System;
using System.Collections.Generic;

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

        public static IResourceBuilder<N8NResource> AddN8N(this IDistributedApplicationBuilder builder, string name = "n8n", int? port = 5678)
        {
            var timeZone = TimeZoneInfo.Local.Id;
            var resource = new N8NResource(name);

            var builderResource = builder.AddResource(resource)
                .WithImage("n8nio/n8n")
                .WithHttpEndpoint(port: port, targetPort: 5678)
                .WithEnvironment("TZ", timeZone)
                .WithVolume("sqlserver_data", "/var/opt/mssql", isReadOnly: false);

            return builderResource;
        }

        public static IResourceBuilder<N8NResource> WithWorkflowsDirectory(this IResourceBuilder<N8NResource> builder, string hostPath, string containerPath = "/home/node/.n8n")
        {
            return builder.WithVolume(hostPath, containerPath);
        }

        public static IResourceBuilder<N8NResource> WithBasicAuth(this IResourceBuilder<N8NResource> builder, string user, string password)
        {
            return builder
                .WithEnvironment("N8N_BASIC_AUTH_ACTIVE", "true")
                .WithEnvironment("N8N_BASIC_AUTH_USER", user)
                .WithEnvironment("N8N_BASIC_AUTH_PASSWORD", password);
        }

        public static IResourceBuilder<N8NResource> WithPostgresDatabase(this IResourceBuilder<N8NResource> builder, string host, string user, string password, string database, int port = 5432, string schema = null, string sslCa = null, bool rejectUnauthorized = true)
        {
            builder
                .WithEnvironment("DB_TYPE", "postgresdb")
                .WithEnvironment("DB_POSTGRESDB_HOST", host)
                .WithEnvironment("DB_POSTGRESDB_PORT", port.ToString())
                .WithEnvironment("DB_POSTGRESDB_USER", user)
                .WithEnvironment("DB_POSTGRESDB_PASSWORD", password)
                .WithEnvironment("DB_POSTGRESDB_DATABASE", database);

            if (!string.IsNullOrEmpty(schema))
                builder.WithEnvironment("DB_POSTGRESDB_SCHEMA", schema);

            if (!string.IsNullOrEmpty(sslCa))
                builder.WithEnvironment("DB_POSTGRESDB_SSL_CA", sslCa);

            builder.WithEnvironment("DB_POSTGRESDB_SSL_REJECT_UNAUTHORIZED", rejectUnauthorized.ToString().ToLower());

            return builder;
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

        public static IResourceBuilder<N8NResource> WithWebhookSettings(this IResourceBuilder<N8NResource> builder, string host, string protocol = "https", int port = 5678)
        {
            return builder
                .WithEnvironment("N8N_HOST", host)
                .WithEnvironment("N8N_PORT", port.ToString())
                .WithEnvironment("N8N_PROTOCOL", protocol)
                .WithEnvironment("WEBHOOK_URL", $"{protocol}://{host}/");
        }

        public static IResourceBuilder<N8NResource> WithExecutionRetention(this IResourceBuilder<N8NResource> builder, int? maxAgeDays = null, int? maxCount = null, string storage = null)
        {
            if (maxAgeDays.HasValue)
                builder.WithEnvironment("EXECUTIONS_DATA_MAX_AGE", maxAgeDays.Value.ToString());

            if (maxCount.HasValue)
                builder.WithEnvironment("EXECUTIONS_DATA_MAX_COUNT", maxCount.Value.ToString());

            if (!string.IsNullOrEmpty(storage))
                builder.WithEnvironment("EXECUTIONS_DATA_STORAGE", storage);

            return builder;
        }

        public static IResourceBuilder<N8NResource> WithNodeEnvironment(this IResourceBuilder<N8NResource> builder, string environment = "production")
        {
            return builder.WithEnvironment("NODE_ENV", environment);
        }

        public static IResourceBuilder<N8NResource> WithTraefikLabels(this IResourceBuilder<N8NResource> builder, string subdomain, string domainName)
        {
            string host = $"{subdomain}.{domainName}";
            return builder
                .WithAnnotation("traefik.enable", "true")
                .WithAnnotation("traefik.http.routers.n8n.rule", $"Host(`{host}`)")
                .WithAnnotation("traefik.http.routers.n8n.tls", "true")
                .WithAnnotation("traefik.http.routers.n8n.entrypoints", "web,websecure")
                .WithAnnotation("traefik.http.routers.n8n.tls.certresolver", "mytlschallenge")
                .WithAnnotation("traefik.http.middlewares.n8n.headers.SSLRedirect", "true")
                .WithAnnotation("traefik.http.middlewares.n8n.headers.STSSeconds", "315360000")
                .WithAnnotation("traefik.http.middlewares.n8n.headers.browserXSSFilter", "true")
                .WithAnnotation("traefik.http.middlewares.n8n.headers.contentTypeNosniff", "true")
                .WithAnnotation("traefik.http.middlewares.n8n.headers.forceSTSHeader", "true")
                .WithAnnotation("traefik.http.middlewares.n8n.headers.SSLHost", domainName)
                .WithAnnotation("traefik.http.middlewares.n8n.headers.STSIncludeSubdomains", "true")
                .WithAnnotation("traefik.http.middlewares.n8n.headers.STSPreload", "true")
                .WithAnnotation("traefik.http.routers.n8n.middlewares", "n8n@docker");
        }

        public static IResourceBuilder<N8NResource> WithHealthProbes(this IResourceBuilder<N8NResource> builder)
        {
            return builder
                .WithHttpEndpoint(name: "healthz", targetPort: 5678, path: "/healthz")
                .WithHttpEndpoint(name: "readiness", targetPort: 5678, path: "/healthz/readiness")
                .WithHttpEndpoint(name: "metrics", targetPort: 5678, path: "/metrics");
        }

        public static IResourceBuilder<N8NResource> WithHealthStatusPropagation(this IResourceBuilder<N8NResource> builder)
        {
            return builder.WithProbe("readiness", "/healthz/readiness", 5678, "http", initialDelay: 5, period: 10);
        }

        public static IResourceBuilder<N8NResource> WithInitCommand(this IResourceBuilder<N8NResource> builder, params string[] commands)
        {
            return builder.WithArgs(commands);
        }

        public static IResourceBuilder<N8NResource> WithLogging(this IResourceBuilder<N8NResource> builder, string logLevel = "info")
        {
            return builder.WithEnvironment("N8N_LOG_LEVEL", logLevel);
        }

        public static IResourceBuilder<N8NResource> WithHealthStatusPropagation(this IResourceBuilder<N8NResource> builder)
        {
            return builder.WithProbe("readiness", "/healthz/readiness", 5678, "http", initialDelay: 5, period: 10);
        }

        public static IResourceBuilder<N8NResource> WithLifecycleHooks(this IResourceBuilder<N8NResource> builder, string preStartCommand = null, string postStartCommand = null)
        {
            if (!string.IsNullOrWhiteSpace(preStartCommand))
                builder.WithEnvironment("PRE_START_COMMAND", preStartCommand);
            if (!string.IsNullOrWhiteSpace(postStartCommand))
                builder.WithEnvironment("POST_START_COMMAND", postStartCommand);
            return builder;
        }

        public static IResourceBuilder<N8NResource> WithAffinity(this IResourceBuilder<N8NResource> builder, string nodeSelector)
        {
            return builder.WithAnnotation("kubernetes.io/affinity", nodeSelector);
        }

        public static IResourceBuilder<N8NResource> WithDependency(this IResourceBuilder<N8NResource> builder, IResource dependency)
        {
            return builder.WithReference(dependency);
        }

        public static IResourceBuilder<N8NResource> ConditionalStartup(this IResourceBuilder<N8NResource> builder, Func<bool> condition)
        {
            if (condition())
                return builder;

            throw new InvalidOperationException("Condition for starting the N8N resource was not met.");
        }
    }
}