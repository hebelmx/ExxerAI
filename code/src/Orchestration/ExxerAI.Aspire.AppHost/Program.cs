using Aspire.Hosting;
using MongoDB.Driver;

Console.WriteLine("🚀 Starting LocalAI Aspire Orchestrator");
Console.WriteLine("===============================================");

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ExxerAI_UI>("exxerai-ui");

Console.WriteLine("🚀 Starting LocalAI Aspire Orchestrator");
Console.WriteLine("===============================================");

// Web frontend project
builder.AddProject<Projects.ExxerAI_UI>("exxerai-ui");

// Redis container
var cache = builder.AddRedis("cache")
    .WithRedisInsight()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithContainerName("ExxerAI__Cache");

builder.AddContainer("redis", "redis:latest")
    .WithVolume("redis_data", "/data", isReadOnly: false);

//Adding the passwor paramter for SQLServer
var password = builder.AddParameter("password", secret: true);
// SQL Server database (in progress)
var sqlServer = builder.AddSqlServer("sql-server", password)
    .WithHttpEndpoint(port: 1433, targetPort: 1433)
    .WithContainerName("ExxerAI__SqlServer")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithVolume("sqlserver_data", "/var/opt/mssql", isReadOnly: false) // Uncomment this line to use a persistent volume for SQL Server data on prod
                                                                       //.WithDataBindMount(source: @"C:\SqlServer\Data"); //Enable this line to bind mount a local directory for persistent data storage on dev
    .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_PID", "Developer")
    .WithEnvironment("MSSQL_AGENT_ENABLED", "true")
    .WithEnvironment("MSSQL_SA_PASSWORD", "Exxer123!");

//** Client Side configuration for SQL Server **//
//dotnet add package Aspire.Microsoft.Data.SqlClient
//<PackageReference Include="Aspire.Microsoft.Data.SqlClient"
// Version="*" />
/*
builder.AddSqlServerClient(connectionName: "database");

Using the connection string in your application:
public class ExampleService(SqlConnection connection)
   {
       // Use connection...
   }

o keyed connections for multiple databases
builder.AddKeyedSqlServerClient(name: "mainDb");
   builder.AddKeyedSqlServerClient(name: "loggingDb");
public class ExampleService(
       [FromKeyedServices("mainDb")] SqlConnection mainDbConnection,
       [FromKeyedServices("loggingDb")] SqlConnection loggingDbConnection)
   {
       // Use connections...
   }

or to  add a json configuration file for SQL Server connections
builder.AddSqlServerClient(connectionName: "sql");
{
     "ConnectionStrings": {
       "database": "Data Source=myserver;Initial Catalog=master"
     }
   }

e Microsoft.Extensions.Configuration.

{
     "Aspire": {
       "Microsoft": {
         "Data": {
           "SqlClient": {
             "ConnectionString": "YOUR_CONNECTIONSTRING",
             "DisableHealthChecks": false,
             "DisableMetrics": true
           }
         }
       }
     }
   }
builder.AddSqlServerClient(
   "database",
   static settings => settings.DisableHealthChecks = true);

*/

var databaseName = "app-db";
var creationScript = $$"""
                       IF DB_ID('{{databaseName}}') IS NULL
                           CREATE DATABASE [{{databaseName}}];
                       GO

                       -- Use the database
                       USE [{{databaseName}}];
                       GO

                       -- Create the todos table
                       CREATE TABLE todos (
                           id INT PRIMARY KEY IDENTITY(1,1),        -- Unique ID for each todo
                           title VARCHAR(255) NOT NULL,             -- Short description of the task
                           description TEXT,                        -- Optional detailed description
                           is_completed BIT DEFAULT 0,              -- Completion status
                           due_date DATE,                           -- Optional due date
                           created_at DATETIME DEFAULT GETDATE()    -- Creation timestamp
                       );
                       GO

                       """;

var sqlServerDb = sqlServer.AddDatabase(databaseName)
    .WithCreationScript(creationScript);

// PostgreSQL for LocalAI
builder.AddPostgres("localai-postgres");

// AI Inference container
builder.AddContainer("localai", "localai/localai", "v2.0.0")
    .WithHttpEndpoint(port: 8081, targetPort: 8080)
    .WithEnvironment("THREADS", "1");

// SearXNG search engine
builder.AddContainer("searxng", "searxng/searxng", "latest")
    .WithHttpEndpoint(port: 8080, targetPort: 8080);

// Qdrant vector database
builder.AddContainer("qdrant", "qdrant/qdrant", "latest")
    .WithHttpEndpoint(port: 6333, targetPort: 6333);

// Monitoring - Prometheus
builder.AddContainer("prometheus", "prom/prometheus", "latest")
    .WithHttpEndpoint(port: 9090, targetPort: 9090)
    .WithBindMount("./monitoring/prometheus.yml", "/etc/prometheus/prometheus.yml", isReadOnly: true);

// Monitoring - Grafana
builder.AddContainer("grafana", "grafana/grafana", "latest")
    .WithHttpEndpoint(port: 3002, targetPort: 3000)
    .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
    .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin");

// Dashboard Project (references cache)
builder.AddProject<Projects.ExxerAI_Aspire_Dashboard>("Dashboard")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(sqlServerDb)
    .WaitFor(sqlServerDb);
;

// Build and run the application
var app = builder.Build();
await app.RunAsync();

Console.WriteLine("===============================================");

Console.WriteLine();
Console.WriteLine("✅ LocalAI Stack Services:");
Console.WriteLine($"   🗄️  PostgreSQL: Available via service discovery");
Console.WriteLine($"   🔄  Redis: Available via service discovery");
Console.WriteLine($"   🤖  LocalAI API: http://localhost:8081");
Console.WriteLine($"   🔍  SearXNG: http://localhost:8080");
Console.WriteLine($"   📊  Qdrant: http://localhost:6333");
Console.WriteLine($"   �  Prometheus: http://localhost:9090");
Console.WriteLine($"   📊  Grafana: http://localhost:3002 (admin/admin)");
Console.WriteLine();
Console.WriteLine("🚀 Starting services...");