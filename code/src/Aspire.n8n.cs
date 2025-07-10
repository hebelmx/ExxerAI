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
        public N8NResource(string name) : base(name) { }

        public string ConnectionString => $"http://{this.GetEndpoint(EndpointNames.Http)?.Url}";

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

        public static IResourceBuilder<N8NResource> WithPersistentVolume(this IResourceBuilder<N8NResource> builder, string volumeName = "n8n_data", string containerPath = "/home/node/.n8n")
        {
            return builder.WithVolume(volumeName, containerPath);
        }

        public static IResourceBuilder<N8NResource> WithSecret(this IResourceBuilder<N8NResource> builder, string key, string secretReference)
        {
            return builder.WithEnvironment(key, secretReference);
        }

        public static IResourceBuilder<N8NResource> WithInitCommand(this IResourceBuilder<N8NResource> builder, params string[] commands)
        {
            return builder.WithArgs(commands);
        }

        public static IResourceBuilder<N8NResource> WithHealthStatusPropagation(this IResourceBuilder<N8NResource> builder)
        {
            return builder.WithProbe("readiness", "/healthz/readiness", 5678, "http", initialDelay: 5, period: 10);
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
    }
}




using Aspire.Hosting;
using Microsoft.AspNetCore.Http;
using Aspire.Hosting.ApplicationModel;
using System;
using System.Collections.Generic;

namespace ExxerAI.Aspire.AppHost
{
    /// <summary>
    /// Represents the N8N container resource for Aspire, with connection string expression support and additional configuration.
    /// </summary>
    public sealed class N8NResource : ContainerResource, IResourceWithConnectionString
    {
        private ReferenceExpression _connectionStringExpression;

        /// <summary>
        /// Gets or sets the plain connection string for the N8N resource.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the connection string expression for the N8N resource.
        /// </summary>
        public ReferenceExpression ConnectionStringExpression
        {
            get => _connectionStringExpression;
            set => _connectionStringExpression = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="N8NResource"/> class with the specified name, port, and optional connection string expression.
        /// </summary>
        /// <param name="name">The name of the N8N resource.</param>
        /// <param name="port">The port the N8N service will listen on. Default is 5678.</param>
        /// <param name="connectionStringExpression">The connection string expression for the N8N resource.</param>
        public N8NResource(string name, int port = 5678, ReferenceExpression connectionStringExpression = null) : base(name)
        {
            Port = port;
            _connectionStringExpression = connectionStringExpression;
        }

        /// <summary>
        /// Gets the port the N8N service will listen on.
        /// </summary>
        public int Port { get; }
    }

    /// <summary>
    /// Extension methods for registering N8N container resources in a distributed Aspire application.
    /// </summary>
    public static class N8NResourceBuilderExtensions
    {
        public static IResourceBuilder<N8NResource> WithWorkflowsDirectory(this IResourceBuilder<N8NResource> builder, string hostPath, string containerPath = "/home/node/.n8n")
        {
            return builder.WithVolume(hostPath, containerPath, isReadOnly: false);
        }

        public static IResourceBuilder<N8NResource> WithBasicAuth(this IResourceBuilder<N8NResource> builder, string user, string password)
        {
            return builder
                .WithEnvironment("N8N_BASIC_AUTH_ACTIVE", "true")
                .WithEnvironment("N8N_BASIC_AUTH_USER", user)
                .WithEnvironment("N8N_BASIC_AUTH_PASSWORD", password);
        }

        public static IResourceBuilder<N8NResource> AddN8N(this IDistributedApplicationBuilder builder, string name = "n8n", int? port = 5678)
        {
            var timeZone = TimeZoneInfo.Local.Id;
            var resource = new N8NResource(name, port.Value);

            var builderResource = builder.AddResource(resource)
                .WithImage("n8nio/n8n")
                .WithHttpEndpoint(port: port, targetPort: 5678)
                .WithEnvironment("TZ", timeZone)
                .WithVolume("sqlserver_data", "/var/opt/mssql", isReadOnly: false);

            return builderResource;
        }

        public static IResourceBuilder<N8NResource> WithPostgresDatabase(this IResourceBuilder<N8NResource> builder, string host, string user, string password, string database, int port = 5432, string schema = null, string sslCa = null, bool rejectUnauthorized = true)
        {
            host = string.IsNullOrEmpty(host) ? "localhost" : host;
            user = string.IsNullOrEmpty(user) ? "postgres" : user;
            password = string.IsNullOrEmpty(password) ? "postgres" : password;
            database = string.IsNullOrEmpty(database) ? "postgres" : database;

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

            // Build the connection string and set it on the resource's ConnectionString property
            string connStr = $"Host={host};Port={port};Username={user};Password={password};Database={database}";
            if (!string.IsNullOrEmpty(schema))
                connStr += $";Search Path={schema}";
            if (!string.IsNullOrEmpty(sslCa))
                connStr += $";SSL Mode=Require;SSL Certificate={sslCa}";
            builder.Resource.ConnectionString = connStr;

            return builder;
        }
    }
}
