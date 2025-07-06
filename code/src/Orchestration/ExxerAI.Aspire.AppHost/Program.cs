var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ExxerAI_UI>("exxerai-ui");

builder.Build().Run();
