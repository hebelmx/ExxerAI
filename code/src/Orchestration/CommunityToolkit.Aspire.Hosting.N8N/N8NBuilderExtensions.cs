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
        /// Initializes a new instance of the <see cref="N8NResource"/> class with the specified name, port, and optional connection string expression.
        /// </summary>
        /// <param name="name">The name of the N8N resource.</param>
        /// <param name="port">The port the N8N service will listen on. Default is 5678.</param>
        /// <param name="connectionStringExpression">The connection string expression for the resource (optional).</param>
        public N8NResource(string name, int port = 5678, ReferenceExpression connectionStringExpression = null) : base(name)
        {
            Port = port;
            _connectionStringExpression = connectionStringExpression;
        }

        /// <summary>
        /// Gets or sets the connection string expression for the N8N resource.
        /// </summary>
        public ReferenceExpression ConnectionStringExpression
        {
            get => _connectionStringExpression;
            set => _connectionStringExpression = value;
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
            // Use defaults if values are not provided
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

            // Build the connection string and set it on the resource's ConnectionStringExpression
            string connStr = $"Host={host};Port={port};Username={user};Password={password};Database={database}";
            if (!string.IsNullOrEmpty(schema))
                connStr += $";Search Path={schema}";
            if (!string.IsNullOrEmpty(sslCa))
                connStr += $";SSL Mode=Require;SSL Certificate={sslCa}";
            builder.Resource.ConnectionStringExpression = ReferenceExpression.Literal(connStr);

            return builder;
        }
    }
}