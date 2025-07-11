using System.Xml.Schema;

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

using Google.Protobuf.Compiler;

var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithGPUSupport()
    .WithOpenWebUI()
    .WithEnvironment("OLLAMA_ORIGINS", "chrome-extension://*")
    .WithEndpoint("ollama", e =>
    {
        e.Name = "Ollama";
        e.Port = 11434;
        e.TargetPort = 11434;
    })
    .WithLifetime(ContainerLifetime.Persistent);

var sqlserver = builder.AddSqlServer("sqlserver")
    .WithDataVolume()
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEndpoint("sqlserver", e =>
    {
        e.Name = "SQL Server";
        e.Port = 1433;
        e.TargetPort = 1433;
    })
    .WithLifetime(ContainerLifetime.Persistent);

var db = sqlserver.AddDatabase("comprobantes");

var redis = builder.ExecutionContext.IsRunMode
    ? builder.AddRedis("redis")
    : builder.AddConnectionString("redis");

//var dbUser = db.AddUser("sa")
//    .WithPassword("Aspire1234!")
//    .WithRole("db_owner");

//var efcore = builder.AddEfCore("efcore")
//    .WithDataVolume()
//    .WithEnvironment("ACCEPT_EULA", "Y")
//    .WithEndpoint("efcore", e =>
//    {
//        e.Name = "EF Core";
//        e.Port = 11435;
//        e.TargetPort = 11435;
//    })
//    .WithLifetime(ContainerLifetime.Persistent);

var chat = ollama.AddModel("chat", "llama3.2")
    .WithUrlForEndpoint("OllamaUrl", url => url.Url = "Ollama");

var planer = ollama.AddModel("planner", "phi3:mini");
var navigator = ollama.AddModel("navigator", "qwen2.5-coder:14b");
var validator = ollama.AddModel("validator", "mistral-small:24b");

/* other moderl
    "falcon3:10b"
    "qwen2.5-coder:14b"
 */

var embeddings = ollama.AddModel("embeddings", "all-minilm");

var vectorDB = builder.AddQdrant("vectordb")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var seq = builder.AddSeq("seq")
    .ExcludeFromManifest()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("ACCEPT_EULA", "Y");

var webApp = builder.AddProject<Projects.RagDoc_Web>("aichatweb-app");
webApp
    .WithReference(chat)
    .WithReference(planer)
    //.WithReference(navigator)
    // .WithReference(validator)
    .WithReference(embeddings)
    .WithReference(seq)
    .WaitFor(chat)
    .WaitFor(planer)
    //.WaitFor(navigator)
    //.WaitFor(validator)
    .WaitFor(embeddings);
webApp
    .WithReference(vectorDB)
    .WaitFor(vectorDB)
    .WaitFor(seq);

builder.Build().Run();
