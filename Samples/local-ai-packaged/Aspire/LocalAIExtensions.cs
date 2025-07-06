using Aspire.Hosting;
using LocalAI.Aspire.AppHost.Configuration;

namespace LocalAI.Aspire.AppHost.Extensions;

/// <summary>
/// Extension methods for adding LocalAI stack services to Aspire
/// Uses native Aspire extensions where possible, Docker containers where needed
/// </summary>
public static class LocalAIExtensions
{
    /// <summary>
    /// Add the complete Supabase stack using native Aspire PostgreSQL + custom containers
    /// </summary>
    public static IResourceBuilder<PostgresServerResource> AddSupabaseStack(
        this IDistributedApplicationBuilder builder, 
        DatabaseConfiguration? dbConfig = null)
    {
        dbConfig ??= new DatabaseConfiguration();
        
        // Use native Aspire PostgreSQL hosting
        var postgres = builder.AddPostgres("postgres", dbConfig.Username, dbConfig.Password)
            .WithDataVolume()
            .WithPgAdmin(); // Native Aspire PgAdmin support

        var database = postgres.AddDatabase(dbConfig.DatabaseName);

        // Supabase Auth Service (GoTrue)
        var supabaseAuth = builder.AddContainer("supabase-auth", "supabase/gotrue:v2.151.0")
            .WithHttpEndpoint(port: dbConfig.Supabase.AuthPort, targetPort: 9999)
            .WithEnvironment("GOTRUE_API_HOST", "0.0.0.0")
            .WithEnvironment("GOTRUE_API_PORT", "9999")
            .WithEnvironment("GOTRUE_DB_DRIVER", "postgres")
            .WithEnvironment("GOTRUE_DB_DATABASE_URL", database.Resource.ConnectionStringExpression)
            .WithEnvironment("GOTRUE_SITE_URL", "http://localhost:3000")
            .WithEnvironment("GOTRUE_URI_ALLOW_LIST", "*")
            .WithEnvironment("GOTRUE_JWT_ADMIN_ROLES", "service_role")
            .WithEnvironment("GOTRUE_JWT_AUD", "authenticated")
            .WithEnvironment("GOTRUE_JWT_DEFAULT_GROUP_NAME", "authenticated")
            .WithEnvironment("GOTRUE_JWT_EXP", "3600")
            .WithEnvironment("GOTRUE_JWT_SECRET", dbConfig.Supabase.JwtSecret)
            .WithEnvironment("GOTRUE_DISABLE_SIGNUP", "false")
            .WithEnvironment("GOTRUE_EXTERNAL_EMAIL_ENABLED", "true")
            .WithHealthCheck("/health")
            .WaitFor(postgres);

        // Supabase REST API (PostgREST)
        var supabaseRest = builder.AddContainer("supabase-rest", "postgrest/postgrest:v12.2.0")
            .WithHttpEndpoint(port: dbConfig.Supabase.RestPort, targetPort: 3000)
            .WithEnvironment("PGRST_DB_URI", database.Resource.ConnectionStringExpression)
            .WithEnvironment("PGRST_DB_SCHEMAS", "public")
            .WithEnvironment("PGRST_DB_ANON_ROLE", "anon")
            .WithEnvironment("PGRST_JWT_SECRET", dbConfig.Supabase.JwtSecret)
            .WithEnvironment("PGRST_DB_USE_LEGACY_GUCS", "false")
            .WithHealthCheck("/")
            .WaitFor(postgres);

        // Supabase Realtime
        var supabaseRealtime = builder.AddContainer("supabase-realtime", "supabase/realtime:v2.29.15")
            .WithHttpEndpoint(port: dbConfig.Supabase.RealtimePort, targetPort: 4000)
            .WithEnvironment("PORT", "4000")
            .WithEnvironment("DB_HOST", "postgres")
            .WithEnvironment("DB_NAME", dbConfig.DatabaseName)
            .WithEnvironment("DB_USER", dbConfig.Username)
            .WithEnvironment("DB_PASSWORD", dbConfig.Password)
            .WithEnvironment("DB_PORT", dbConfig.Port.ToString())
            .WithEnvironment("SECRET_KEY_BASE", dbConfig.Supabase.JwtSecret)
            .WithEnvironment("ERL_AFLAGS", "-proto_dist inet_tcp")
            .WithEnvironment("ENABLE_TAILSCALE", "false")
            .WithEnvironment("DNS_NODES", "'\\''[\"127.0.0.1\"]'\\''")
            .WaitFor(postgres);

        // Supabase Storage API
        var supabaseStorage = builder.AddContainer("supabase-storage", "supabase/storage-api:v1.0.6")
            .WithHttpEndpoint(port: dbConfig.Supabase.StoragePort, targetPort: 5000)
            .WithEnvironment("ANON_KEY", dbConfig.Supabase.AnonKey)
            .WithEnvironment("SERVICE_KEY", dbConfig.Supabase.ServiceRoleKey)
            .WithEnvironment("POSTGREST_URL", $"http://supabase-rest:{dbConfig.Supabase.RestPort}")
            .WithEnvironment("PGRST_JWT_SECRET", dbConfig.Supabase.JwtSecret)
            .WithEnvironment("DATABASE_URL", database.Resource.ConnectionStringExpression)
            .WithEnvironment("FILE_SIZE_LIMIT", "52428800")
            .WithEnvironment("STORAGE_BACKEND", "file")
            .WithEnvironment("FILE_STORAGE_BACKEND_PATH", "/var/lib/storage")
            .WithEnvironment("TENANT_ID", "stub")
            .WithEnvironment("REGION", "stub")
            .WithEnvironment("GLOBAL_S3_BUCKET", "stub")
            .WithVolume("supabase-storage-data", "/var/lib/storage")
            .WaitFor(supabaseRest);

        return postgres;
    }

    /// <summary>
    /// Add LocalAI stack with the latest containers
    /// </summary>
    public static IResourceBuilder<ContainerResource> AddLocalAIStack(
        this IDistributedApplicationBuilder builder,
        LocalAIConfiguration? aiConfig = null)
    {
        aiConfig ??= new LocalAIConfiguration();
        
        // LocalAI Core - using latest stable version
        var localAi = builder.AddContainer("localai", "localai/localai:latest-aio-cpu")
            .WithHttpEndpoint(port: aiConfig.ApiPort, targetPort: 8080)
            .WithEnvironment("DEBUG", "true")
            .WithEnvironment("MODELS_PATH", "/build/models")
            .WithEnvironment("THREADS", aiConfig.Workers.ToString())
            .WithEnvironment("CONTEXT_SIZE", "2048")
            .WithEnvironment("GALLERIES", "[{\"name\":\"model-gallery\", \"url\":\"github:go-skynet/model-gallery/index.yaml\"}, {\"name\":\"huggingface\", \"url\":\"github:go-skynet/model-gallery/huggingface.yaml\"}]")
            .WithVolume("localai-models", "/build/models")
            .WithVolume("localai-data", "/tmp/localai")
            .WithHealthCheck("/readyz");

        // Open WebUI - Latest version for better AI model interaction
        var openWebUi = builder.AddContainer("open-webui", "ghcr.io/open-webui/open-webui:main")
            .WithHttpEndpoint(port: aiConfig.WebUIPort, targetPort: 8080)
            .WithEnvironment("OPENAI_API_BASE_URL", $"http://localai:{aiConfig.ApiPort}/v1")
            .WithEnvironment("OPENAI_API_KEY", "not-needed")
            .WithEnvironment("WEBUI_SECRET_KEY", "your-webui-secret-key")
            .WithEnvironment("WEBUI_NAME", "LocalAI Open WebUI")
            .WithEnvironment("DEFAULT_LOCALE", "en-US")
            .WithEnvironment("ENABLE_SIGNUP", "true")
            .WithEnvironment("ENABLE_LOGIN_FORM", "true")
            .WithVolume("open-webui-data", "/app/backend/data")
            .WithHealthCheck("/health")
            .WaitFor(localAi);

        return localAi;
    }

    /// <summary>
    /// Add SearXNG privacy-focused search engine
    /// </summary>
    public static IResourceBuilder<ContainerResource> AddSearXNG(
        this IDistributedApplicationBuilder builder,
        SearchConfiguration? searchConfig = null)
    {
        searchConfig ??= new SearchConfiguration();
        
        // SearXNG - Privacy-respecting search engine
        var searxng = builder.AddContainer("searxng", "searxng/searxng:latest")
            .WithHttpEndpoint(port: searchConfig.Port, targetPort: 8080)
            .WithEnvironment("SEARXNG_BASE_URL", searchConfig.BaseUrl)
            .WithEnvironment("SEARXNG_REDIS_URL", "redis://redis:6379/0")
            .WithBindMount("../searxng", "/etc/searxng", isReadOnly: true)
            .WithVolume("searxng-logs", "/var/log/uwsgi")
            .WithHealthCheck("/stats");

        return searxng;
    }

    /// <summary>
    /// Add vector databases with full ecosystem
    /// </summary>
    public static (IResourceBuilder<ContainerResource> qdrant, IResourceBuilder<ContainerResource> milvus, IResourceBuilder<ContainerResource> chroma) 
        AddVectorDatabases(this IDistributedApplicationBuilder builder, VectorDatabaseConfiguration? vectorConfig = null)
    {
        vectorConfig ??= new VectorDatabaseConfiguration();
        
        // Qdrant Vector Database - Fast and efficient
        var qdrant = builder.AddContainer("qdrant", "qdrant/qdrant:latest")
            .WithHttpEndpoint(port: vectorConfig.Qdrant.Port, targetPort: 6333)
            .WithHttpEndpoint(port: vectorConfig.Qdrant.GrpcPort, targetPort: 6334, name: "grpc")
            .WithEnvironment("QDRANT__SERVICE__HTTP_PORT", "6333")
            .WithEnvironment("QDRANT__SERVICE__GRPC_PORT", "6334")
            .WithVolume("qdrant-storage", "/qdrant/storage")
            .WithHealthCheck("/health");

        // Milvus Vector Database - Simplified single container approach
        var milvus = builder.AddContainer("milvus", "milvusdb/milvus:latest")
            .WithHttpEndpoint(port: vectorConfig.Milvus.Port, targetPort: 19530)
            .WithEnvironment("ETCD_USE_EMBED", "true")
            .WithEnvironment("MINIO_USE_EMBED", "true")
            .WithEnvironment("COMMON_STORAGETYPE", "local")
            .WithVolume("milvus-data", "/var/lib/milvus")
            .WithHealthCheck("/health");

        // ChromaDB - Simple and easy to use
        var chroma = builder.AddContainer("chroma", "chromadb/chroma:latest")
            .WithHttpEndpoint(port: 8000, targetPort: 8000)
            .WithEnvironment("IS_PERSISTENT", "TRUE")
            .WithEnvironment("PERSIST_DIRECTORY", "/chroma/chroma")
            .WithEnvironment("ANONYMIZED_TELEMETRY", "TRUE")
            .WithVolume("chroma-data", "/chroma/chroma")
            .WithHealthCheck("/api/v1/heartbeat");

        return (qdrant, milvus, chroma);
    }

    /// <summary>
    /// Add comprehensive monitoring stack
    /// </summary>
    public static (IResourceBuilder<ContainerResource> prometheus, IResourceBuilder<ContainerResource> grafana, IResourceBuilder<ContainerResource> jaeger) 
        AddMonitoringStack(this IDistributedApplicationBuilder builder, MonitoringConfiguration? monitoringConfig = null)
    {
        monitoringConfig ??= new MonitoringConfiguration();
        
        // Prometheus for metrics collection
        var prometheus = builder.AddContainer("prometheus", "prom/prometheus:latest")
            .WithHttpEndpoint(port: monitoringConfig.Prometheus.Port, targetPort: 9090)
            .WithBindMount(monitoringConfig.Prometheus.ConfigPath, "/etc/prometheus/prometheus.yml", isReadOnly: true)
            .WithArgs("--config.file=/etc/prometheus/prometheus.yml", 
                     "--storage.tsdb.path=/prometheus", 
                     "--web.console.libraries=/etc/prometheus/console_libraries", 
                     "--web.console.templates=/etc/prometheus/consoles", 
                     "--storage.tsdb.retention.time=200h", 
                     "--web.enable-lifecycle")
            .WithVolume("prometheus-data", "/prometheus")
            .WithHealthCheck("/-/healthy");

        // Grafana for visualization
        var grafana = builder.AddContainer("grafana", "grafana/grafana:latest")
            .WithHttpEndpoint(port: monitoringConfig.Grafana.Port, targetPort: 3000)
            .WithEnvironment("GF_SECURITY_ADMIN_USER", monitoringConfig.Grafana.AdminUsername)
            .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", monitoringConfig.Grafana.AdminPassword)
            .WithEnvironment("GF_USERS_ALLOW_SIGN_UP", "false")
            .WithEnvironment("GF_INSTALL_PLUGINS", "grafana-clock-panel,grafana-simple-json-datasource")
            .WithVolume("grafana-data", "/var/lib/grafana")
            .WithHealthCheck("/api/health")
            .WaitFor(prometheus);

        // Jaeger for distributed tracing
        var jaeger = builder.AddContainer("jaeger", "jaegertracing/all-in-one:latest")
            .WithHttpEndpoint(port: 16686, targetPort: 16686, name: "ui")
            .WithHttpEndpoint(port: 14268, targetPort: 14268, name: "collector")
            .WithEnvironment("COLLECTOR_ZIPKIN_HOST_PORT", ":9411")
            .WithEnvironment("COLLECTOR_OTLP_ENABLED", "true")
            .WithVolume("jaeger-data", "/tmp")
            .WithHealthCheck("/");

        return (prometheus, grafana, jaeger);
    }

    /// <summary>
    /// Add API Gateway with Nginx
    /// </summary>
    public static IResourceBuilder<ContainerResource> AddNginxGateway(
        this IDistributedApplicationBuilder builder,
        NetworkConfiguration? networkConfig = null)
    {
        networkConfig ??= new NetworkConfiguration();
        
        var nginx = builder.AddContainer("nginx", "nginx:alpine")
            .WithHttpEndpoint(port: networkConfig.Nginx.HttpPort, targetPort: 80)
            .WithHttpEndpoint(port: networkConfig.Nginx.HttpsPort, targetPort: 443, name: "https")
            .WithBindMount(networkConfig.Nginx.ConfigPath, "/etc/nginx/nginx.conf", isReadOnly: true)
            .WithVolume("nginx-logs", "/var/log/nginx")
            .WithHealthCheck("/nginx_status");

        return nginx;
    }

    /// <summary>
    /// Add Redis using native Aspire support
    /// </summary>
    public static IResourceBuilder<RedisResource> AddRedisStack(
        this IDistributedApplicationBuilder builder,
        NetworkConfiguration? networkConfig = null)
    {
        networkConfig ??= new NetworkConfiguration();
        
        // Use native Aspire Redis hosting
        var redis = builder.AddRedis("redis")
            .WithDataVolume()
            .WithRedisCommander(); // Native Aspire Redis Commander support

        return redis;
    }

    /// <summary>
    /// Add additional AI/ML tools including MCP servers and development tools
    /// </summary>
    public static (IResourceBuilder<ContainerResource> jupyter, IResourceBuilder<ContainerResource> mlflow, IResourceBuilder<ContainerResource> ollama, IResourceBuilder<ContainerResource> mcpServer) 
        AddAIMLToolsStack(this IDistributedApplicationBuilder builder)
    {
        // Jupyter Lab for data science workflows
        var jupyter = builder.AddContainer("jupyter", "jupyter/scipy-notebook:latest")
            .WithHttpEndpoint(port: 8888, targetPort: 8888)
            .WithEnvironment("JUPYTER_ENABLE_LAB", "yes")
            .WithEnvironment("JUPYTER_TOKEN", "localai")
            .WithVolume("jupyter-work", "/home/jovyan/work")
            .WithHealthCheck("/api");

        // MLflow for ML experiment tracking
        var mlflow = builder.AddContainer("mlflow", "python:3.9-slim")
            .WithHttpEndpoint(port: 5000, targetPort: 5000)
            .WithArgs("sh", "-c", "pip install mlflow && mlflow ui --host 0.0.0.0 --port 5000")
            .WithVolume("mlflow-artifacts", "/mlflow");

        // Ollama for additional LLM support
        var ollama = builder.AddContainer("ollama", "ollama/ollama:latest")
            .WithHttpEndpoint(port: 11434, targetPort: 11434)
            .WithEnvironment("OLLAMA_ORIGINS", "*")
            .WithVolume("ollama-data", "/root/.ollama")
            .WithHealthCheck("/api/tags");

        // MCP (Model Context Protocol) Server for enhanced AI interactions
        var mcpServer = builder.AddContainer("mcp-server", "python:3.11-slim")
            .WithHttpEndpoint(port: 3333, targetPort: 8000)
            .WithArgs("sh", "-c", "pip install fastapi uvicorn && python -c \"" +
                "from fastapi import FastAPI; " +
                "import uvicorn; " +
                "app = FastAPI(title='LocalAI MCP Server'); " +
                "@app.get('/health'); " +
                "def health(): return {'status': 'healthy'}; " +
                "uvicorn.run(app, host='0.0.0.0', port=8000)\"")
            .WithHealthCheck("/health");

        return (jupyter, mlflow, ollama, mcpServer);
    }

    /// <summary>
    /// Add additional enterprise services for production deployments
    /// </summary>
    public static (IResourceBuilder<ContainerResource> elasticsearch, IResourceBuilder<ContainerResource> kibana, IResourceBuilder<ContainerResource> rabbitmq, IResourceBuilder<ContainerResource> minio) 
        AddEnterpriseServicesStack(this IDistributedApplicationBuilder builder)
    {
        // Elasticsearch for advanced search and analytics
        var elasticsearch = builder.AddContainer("elasticsearch", "docker.elastic.co/elasticsearch/elasticsearch:8.11.0")
            .WithHttpEndpoint(port: 9200, targetPort: 9200)
            .WithEnvironment("discovery.type", "single-node")
            .WithEnvironment("xpack.security.enabled", "false")
            .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
            .WithVolume("elasticsearch-data", "/usr/share/elasticsearch/data")
            .WithHealthCheck("/_cluster/health");

        // Kibana for Elasticsearch visualization
        var kibana = builder.AddContainer("kibana", "docker.elastic.co/kibana/kibana:8.11.0")
            .WithHttpEndpoint(port: 5601, targetPort: 5601)
            .WithEnvironment("ELASTICSEARCH_HOSTS", "http://elasticsearch:9200")
            .WaitFor(elasticsearch)
            .WithHealthCheck("/api/status");

        // RabbitMQ for message queuing
        var rabbitmq = builder.AddContainer("rabbitmq", "rabbitmq:3-management")
            .WithHttpEndpoint(port: 15672, targetPort: 15672, name: "management")
            .WithHttpEndpoint(port: 5672, targetPort: 5672, name: "amqp")
            .WithEnvironment("RABBITMQ_DEFAULT_USER", "admin")
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", "admin")
            .WithVolume("rabbitmq-data", "/var/lib/rabbitmq")
            .WithHealthCheck("/api/aliveness-test/%2F");

        // MinIO for S3-compatible object storage
        var minio = builder.AddContainer("minio", "minio/minio:latest")
            .WithHttpEndpoint(port: 9000, targetPort: 9000, name: "api")
            .WithHttpEndpoint(port: 9001, targetPort: 9001, name: "console")
            .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
            .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
            .WithArgs("server", "/data", "--console-address", ":9001")
            .WithVolume("minio-data", "/data")
            .WithHealthCheck("/minio/health/live");

        return (elasticsearch, kibana, rabbitmq, minio);
    }
}
