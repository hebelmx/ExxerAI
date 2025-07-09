using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Infrastructure.Embeddings;
using ExxerAI.Infrastructure.VectorStore;
using ExxerAI.Infrastructure.GraphStore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Neo4jClient;

namespace ExxerAI.Infrastructure.Extensions;

/// <summary>
/// Extensions for configuring vector store and semantic search services
/// </summary>
public static class VectorStoreServiceCollectionExtensions
{
    /// <summary>
    /// Add Qdrant vector store with OpenAI embeddings
    /// </summary>
    public static IServiceCollection AddQdrantVectorStore(
        this IServiceCollection services,
        string qdrantHost = "localhost",
        int qdrantPort = 6333,
        bool useHttps = false,
        string? apiKey = null,
        string collectionName = "exxerai_documents",
        int embeddingDimensions = 1536)
    {
        // Register Qdrant client
        services.AddSingleton<QdrantClient>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<QdrantClient>>();
            
            var qdrantClient = new QdrantClient(
                host: qdrantHost,
                port: qdrantPort,
                https: useHttps,
                apiKey: apiKey);

            logger.LogInformation("Qdrant client configured for {Host}:{Port}", qdrantHost, qdrantPort);
            return qdrantClient;
        });

        // Register vector store
        services.AddScoped<IVectorStore>(provider =>
        {
            var client = provider.GetRequiredService<QdrantClient>();
            var logger = provider.GetRequiredService<ILogger<QdrantVectorStore>>();
            
            return new QdrantVectorStore(client, logger, collectionName, embeddingDimensions);
        });

        return services;
    }

    /// <summary>
    /// Add OpenAI embedding generator using Microsoft.Extensions.AI
    /// </summary>
    public static IServiceCollection AddOpenAIEmbeddings(
        this IServiceCollection services,
        string apiKey,
        string modelName = "text-embedding-3-small",
        string? organizationId = null)
    {
        // Register the Microsoft.Extensions.AI embedding generator
        services.AddScoped<IEmbeddingGenerator<string, Embedding<float>>>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<IEmbeddingGenerator<string, Embedding<float>>>>();
            
            // Note: This is a placeholder - actual implementation would use the OpenAI provider
            // from Microsoft.Extensions.AI when available
            logger.LogInformation("OpenAI embedding generator configured with model: {ModelName}", modelName);
            
            // TODO: Replace with actual Microsoft.Extensions.AI OpenAI provider
            throw new NotImplementedException("OpenAI embedding provider integration pending Microsoft.Extensions.AI implementation");
        });

        // Register our wrapper
        services.AddScoped<ExxerAI.Application.Interfaces.IEmbeddingGenerator>(provider =>
        {
            var embeddingGenerator = provider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var logger = provider.GetRequiredService<ILogger<OpenAIEmbeddingGenerator>>();
            
            return new OpenAIEmbeddingGenerator(embeddingGenerator, logger, modelName);
        });

        return services;
    }

    /// <summary>
    /// Add Neo4j graph knowledge store
    /// </summary>
    public static IServiceCollection AddNeo4jGraphStore(
        this IServiceCollection services,
        string connectionString = "bolt://localhost:7687",
        string username = "neo4j",
        string password = "password")
    {
        // Register Neo4j client
        services.AddSingleton<IGraphClient>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GraphClient>>();
            
            var client = new GraphClient(new Uri(connectionString), username, password);
            logger.LogInformation("Neo4j client configured for {ConnectionString}", connectionString);
            
            return client;
        });

        // Register graph knowledge store
        services.AddScoped<IGraphKnowledgeStore>(provider =>
        {
            var client = provider.GetRequiredService<IGraphClient>();
            var logger = provider.GetRequiredService<ILogger<Neo4jGraphKnowledgeStore>>();
            
            return new Neo4jGraphKnowledgeStore(client, logger);
        });

        return services;
    }

    /// <summary>
    /// Add semantic search service with all dependencies
    /// </summary>
    public static IServiceCollection AddSemanticSearch(
        this IServiceCollection services)
    {
        services.AddScoped<SemanticSearchService>();
        return services;
    }

    /// <summary>
    /// Add hybrid knowledge service combining vector and graph stores
    /// </summary>
    public static IServiceCollection AddHybridKnowledgeService(
        this IServiceCollection services)
    {
        services.AddScoped<HybridKnowledgeService>();
        return services;
    }

    /// <summary>
    /// Add complete vector store stack with Qdrant and OpenAI
    /// </summary>
    public static IServiceCollection AddExxerAIVectorStore(
        this IServiceCollection services,
        QdrantVectorStoreOptions qdrantOptions,
        OpenAIEmbeddingOptions openAIOptions)
    {
        ArgumentNullException.ThrowIfNull(qdrantOptions);
        ArgumentNullException.ThrowIfNull(openAIOptions);

        services.AddQdrantVectorStore(
            qdrantOptions.Host,
            qdrantOptions.Port,
            qdrantOptions.UseHttps,
            qdrantOptions.ApiKey,
            qdrantOptions.CollectionName,
            qdrantOptions.EmbeddingDimensions);

        services.AddOpenAIEmbeddings(
            openAIOptions.ApiKey,
            openAIOptions.ModelName,
            openAIOptions.OrganizationId);

        services.AddSemanticSearch();

        return services;
    }

    /// <summary>
    /// Add complete hybrid knowledge stack with Qdrant, Neo4j, and OpenAI
    /// </summary>
    public static IServiceCollection AddExxerAIHybridKnowledge(
        this IServiceCollection services,
        QdrantVectorStoreOptions qdrantOptions,
        Neo4jGraphStoreOptions neo4jOptions,
        OpenAIEmbeddingOptions openAIOptions)
    {
        ArgumentNullException.ThrowIfNull(qdrantOptions);
        ArgumentNullException.ThrowIfNull(neo4jOptions);
        ArgumentNullException.ThrowIfNull(openAIOptions);

        // Add vector store
        services.AddQdrantVectorStore(
            qdrantOptions.Host,
            qdrantOptions.Port,
            qdrantOptions.UseHttps,
            qdrantOptions.ApiKey,
            qdrantOptions.CollectionName,
            qdrantOptions.EmbeddingDimensions);

        // Add graph store
        services.AddNeo4jGraphStore(
            neo4jOptions.ConnectionString,
            neo4jOptions.Username,
            neo4jOptions.Password);

        // Add embeddings
        services.AddOpenAIEmbeddings(
            openAIOptions.ApiKey,
            openAIOptions.ModelName,
            openAIOptions.OrganizationId);

        // Add services
        services.AddSemanticSearch();
        services.AddHybridKnowledgeService();

        return services;
    }
}

/// <summary>
/// Configuration options for Qdrant vector store
/// </summary>
public class QdrantVectorStoreOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6333;
    public bool UseHttps { get; set; } = false;
    public string? ApiKey { get; set; } = null;
    public string CollectionName { get; set; } = "exxerai_documents";
    public int EmbeddingDimensions { get; set; } = 1536;
}

/// <summary>
/// Configuration options for OpenAI embeddings
/// </summary>
public class OpenAIEmbeddingOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = "text-embedding-3-small";
    public string? OrganizationId { get; set; } = null;
}

/// <summary>
/// Configuration options for Neo4j graph store
/// </summary>
public class Neo4jGraphStoreOptions
{
    public string ConnectionString { get; set; } = "bolt://localhost:7687";
    public string Username { get; set; } = "neo4j";
    public string Password { get; set; } = "password";
    public string Database { get; set; } = "neo4j";
}

/// <summary>
/// Extension methods for IServiceProvider to get semantic search components
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Get the semantic search service from DI container
    /// </summary>
    public static SemanticSearchService GetSemanticSearchService(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<SemanticSearchService>();
    }

    /// <summary>
    /// Get the vector store from DI container
    /// </summary>
    public static IVectorStore GetVectorStore(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<IVectorStore>();
    }

    /// <summary>
    /// Get the embedding generator from DI container
    /// </summary>
    public static ExxerAI.Application.Interfaces.IEmbeddingGenerator GetEmbeddingGenerator(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<ExxerAI.Application.Interfaces.IEmbeddingGenerator>();
    }

    /// <summary>
    /// Get the graph knowledge store from DI container
    /// </summary>
    public static IGraphKnowledgeStore GetGraphKnowledgeStore(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<IGraphKnowledgeStore>();
    }

    /// <summary>
    /// Get the hybrid knowledge service from DI container
    /// </summary>
    public static HybridKnowledgeService GetHybridKnowledgeService(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<HybridKnowledgeService>();
    }
}