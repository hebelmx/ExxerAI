using Aspire.Hosting;
using Aspire.Hosting.Lifecycle;
using Grpc.Core;
using MongoDB.Driver;
using Nextended.Aspire;
using NorthernNerds.Aspire.Hosting.Neo4j;

Console.WriteLine("🚀 Starting LocalAI Aspire Orchestrator");
Console.WriteLine("===============================================");
Console.WriteLine("🚀 Starting LocalAI Aspire Orchestrator");
Console.WriteLine("===============================================");

var builder = DistributedApplication.CreateBuilder(args);
// Web frontend project
builder.AddProject<Projects.ExxerAI_UI>("exxerai-ui");

// Redis container
var cache = builder.AddRedis("cache")
    .WithRedisInsight()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithContainerName("ExxerAI__Cache");

builder.AddContainer("redis", "redis:latest")
    .WithVolume("redis_data", "/data", isReadOnly: false);

var postgresServer = builder.AddPostgres("postgresserver")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var postgresDatabase = postgresServer.AddDatabase("postgres");

//Adding the passwor paramter for SQLServer
var password = builder.AddParameter("password", secret: true);
// SQL Server database (in progress)
var sqlServer = builder.AddSqlServer("sql-server", password)
    .WithHttpEndpoint(port: 1433, targetPort: 1433)
    .WithContainerName("ExxerAI__SqlServer")
    .WithLifetime(ContainerLifetime.Persistent)
//    .WithVolume("sqlserver_data", "/var/opt/mssql", isReadOnly: false) // Uncomment this line to use a persistent volume for SQL Server data on prod
       .WithDataBindMount(source: @"C:\SqlServer\Data") //Enable this line to bind mount a local directory for persistent data storage on dev
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
https://learn.microsoft.com/en-us/dotnet/aspire/database/sql-server-integration?tabs=dotnet-cli%2Cssms

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

// AI Inference container
builder.AddContainer("localai", "localai/localai", "v2.0.0")
    .WithHttpEndpoint(port: 8081, targetPort: 8080)
    .WithEnvironment("THREADS", "1");

// SearXNG search engine
builder.AddContainer("searxng", "searxng/searxng", "latest")
    .WithHttpEndpoint(port: 8080, targetPort: 8080);

// Qdrant vector database

var apiKey = builder.AddParameter("apiKey", secret: true);

var qdrant = builder.AddQdrant("qdrant", apiKey)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("ExxerAI_Quadrant"); // Uncomment this line to use a persistent volume for Qdrant data on prod
//.WithDataBindMount(source: @"C:\Qdrant\Data"); // Uncomment this line to bind mount a local directory for persistent data storage on dev

/*
 *The preceding code gets a parameter to pass to the AddQdrant API,
 * and internally assigns the parameter to the QDRANT__SERVICE__API_KEY
 * environment variable of the Qdrant container.
 * The apiKey parameter is usually specified as a user secret:
 *{
     "Parameters": {
       "apiKey": "Non-default-P@ssw0rd"
     }
   }
 * If you need to customize it further
 */

// Qdrant client configuration
/**
 *
 <PackageReference Include="Aspire.Qdrant.Client"
                  Version="*" />
builder.AddQdrantClient("qdrant");
public class ExampleService(QdrantClient client)
{
    // Use client...
}
builder.AddKeyedQdrantClient(name: "mainQdrant");
builder.AddKeyedQdrantClient(name: "loggingQdrant");

public class ExampleService(
    [FromKeyedServices("mainQdrant")] QdrantClient mainQdrantClient,
    [FromKeyedServices("loggingQdrant")] QdrantClient loggingQdrantClient)
{
    // Use clients...
}

Use a connection string
When using a connection string from the ConnectionStrings configuration section, you can provide the name of the connection string when calling builder.AddQdrantClient():

C#

Copy
builder.AddQdrantClient("qdrant");
Then .NET Aspire retrieves the connection string from the ConnectionStrings configuration section:

JSON

Copy
{
  "ConnectionStrings": {
    "qdrant": "Endpoint=http://localhost:6334;Key=123456!@#$%"
  }
}

The .NET Aspire Qdrant client integration supports Microsoft.Extensions.Configuration.
It loads the QdrantClientSettings from configuration by using the Aspire:Qdrant:Client key.
The following is an example of an appsettings.json that configures some of the options:
QdrantClientSettings

{
     "Aspire": {
       "Qdrant": {
         "Client": {
           "Endpoint": "http://localhost:6334/",
           "Key": "123456!@#$%"
         }
       }
     }
   }
https://learn.microsoft.com/en-us/dotnet/aspire/database/qdrant-integration?tabs=package-reference

 */

var seq = builder.AddSeq("seq")
    .ExcludeFromManifest()
    .WithDataVolume("ExerAI_SEQ") // Uncomment this line to use a persistent volume for Seq data on prod
                                  //.WithDataBindMount(source: @"C:\Data")  // Uncomment this line to bind mount a local directory for persistent data storage on dev

    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("ACCEPT_EULA", "Y");
;

//Configration details of SEQ
//https://github.com/dotnet/docs-aspire/blob/main/docs/logging/seq-integration.md

/*
 *
 *
 * https://learn.microsoft.com/en-us/dotnet/aspire/logging/seq-integration?tabs=dotnet-cli
 */

//Adding neo 4j container

var neo4jUser = builder.AddParameter("neo4jUser", "neo4j");
var neo4jPass = builder.AddParameter("neo4jPass", "secret");

//Basic version of neo4J only support one datbase for user
//So no need at this time to add more databases
//At least until the app grow and need this or the app is becoming multittenant
var neo4jDb = builder.AddNeo4j("graph-db", neo4jUser, neo4jPass);

// Client Side configuration for Neo4j
/*NorthernNerds.Aspire.Neo4j	NuGet	Downloads
   NorthernNerds.Aspire.Hosting.Neo4j
 *
 *

   using NorthernNerds.Aspire.Neo4j;

   var builder = WebApplication.CreateBuilder(args);
   builder.AddNeo4jClient("graph-db");n your service projects:

   using NorthernNerds.Aspire.Neo4j;

   var builder = WebApplication.CreateBuilder(args);
   builder.AddNeo4jClient("graph-db");
or

   builder.AddKeyedNeo4jClient("graph-db");
   builder.AddKeyedNeo4jClient("graph-db-logging");

Configure parameters in appsettings.json:

{
     "Parameters": {
       "neo4j-pass": "Password",
       "neo4j-user": "neo4j"
     }
   }
https://github.com/terle/aspire-neo4j?tab=readme-ov-file
https://github.com/terle/aspire-neo4j/blob/main/example/README.md
 *
 */

var ollama = builder.AddOllama("Ollama", 1342);

// Monitoring - PrometheusVAR
var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "latest")
    .WithHttpEndpoint(port: 9090, targetPort: 9090)
    .WithBindMount("./monitoring/prometheus.yml", "/etc/prometheus/prometheus.yml", isReadOnly: true);

// Monitoring - Grafana
var grafana = builder.AddContainer("grafana", "grafana/grafana", "latest")
    .WithHttpEndpoint(port: 3002, targetPort: 3000)
    .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
    .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin");

// Dashboard Project (references cache)
builder.AddProject<Projects.ExxerAI_Aspire_Dashboard>("Dashboard")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(sqlServerDb)
    .WaitFor(sqlServerDb)
    .WithReference(qdrant)
    .WaitFor(qdrant)
    .WithReference(seq)
    .WaitFor(seq)
    .WithReference(neo4jDb)
    .WaitFor(neo4jDb)
    .WithReference(ollama)
    .WaitFor(ollama)
    .WithReference(postgresServer)
    .WaitFor(postgresServer)
    //.WithReference(prometheus)
    //.WaitFor(prometheus)
    //.WithReference(grafana)
    //.WaitFor(grafana)
    .WithReference(seq)
    .WaitFor(seq)

// keep adding references to other services as needed
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