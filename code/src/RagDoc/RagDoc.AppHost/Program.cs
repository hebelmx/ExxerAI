using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Minimal configuration for DCP testing
var webApp = builder.AddProject<Projects.RagDoc_Web>("webapi");

builder.Build().Run();
