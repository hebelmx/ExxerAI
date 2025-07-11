using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

using RagDoc.Web.Components;
using RagDoc.Web.Extensions;
using RagDoc.Web.Models;
using RagDoc.Web.Services;
using RagDoc.Web.Services.Ingestion;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.AddOllamaApiClient("chat")
    .AddChatClient()
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());
builder.AddOllamaApiClient("embeddings")
    .AddEmbeddingGenerator();

builder.AddSeqEndpoint(connectionName: "seq");

builder.AddQdrantClient("vectordb");
builder.Services.AddQdrantCollection<Guid, IngestedChunk>("data-ragdoc-chunks");
builder.Services.AddQdrantCollection<Guid, IngestedDocument>("data-ragdoc-documents");
builder.Services.AddScoped<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();

builder.Services.Configure<PdfDocumentsSettings>(
    builder.Configuration.GetSection("PdfDocuments"));

builder.Services.AddHostedApiService<IIngestionWorker, IngestionWorker>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
