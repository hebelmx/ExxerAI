using Aspire.Hosting;

namespace LocalAI.Aspire.AppHost;

public static class LocalAIExtensions
{
    /// <summary>
    /// Adds the complete Supabase stack to the application
    /// </summary>
    public static IResourceBuilder<PostgresServerResource> AddSupabaseStack(
        this IDistributedApplicationBuilder builder,
        string name = "supabase")
    {
        // PostgreSQL for Supabase
        var postgres = builder.AddPostgres(name)
            .WithPgAdmin()
            .WithDataVolume();

        var postgresDb = postgres.AddDatabase("postgres");

        // Supabase Auth Service
        var supabaseAuth = builder.AddContainer("supabase-auth", "supabase/gotrue")
            .WithHttpEndpoint(port: 9999, targetPort: 9999)
            .WithEnvironment("GOTRUE_API_HOST", "0.0.0.0")
            .WithEnvironment("GOTRUE_API_PORT", "9999")
            .WithEnvironment("GOTRUE_DB_DRIVER", "postgres")
            .WithEnvironment("GOTRUE_DB_DATABASE_URL", postgresDb.Resource.ConnectionStringExpression)
            .WithEnvironment("GOTRUE_SITE_URL", "http://localhost:3000")
            .WithEnvironment("GOTRUE_URI_ALLOW_LIST", "*")
            .WithEnvironment("GOTRUE_JWT_ADMIN_ROLES", "service_role")
            .WithEnvironment("GOTRUE_JWT_AUD", "authenticated")
            .WithEnvironment("GOTRUE_JWT_DEFAULT_GROUP_NAME", "authenticated")
            .WithEnvironment("GOTRUE_JWT_EXP", "3600")
            .WithEnvironment("GOTRUE_JWT_SECRET", "your-jwt-secret-key")
            .WaitFor(postgres);

        // Supabase Rest API
        var supabaseRest = builder.AddContainer("supabase-rest", "postgrest/postgrest")
            .WithHttpEndpoint(port: 3000, targetPort: 3000)
            .WithEnvironment("PGRST_DB_URI", postgresDb.Resource.ConnectionStringExpression)
            .WithEnvironment("PGRST_DB_SCHEMAS", "public")
            .WithEnvironment("PGRST_DB_ANON_ROLE", "anon")
            .WithEnvironment("PGRST_JWT_SECRET", "your-jwt-secret-key")
            .WaitFor(postgres);

        // Supabase Realtime
        var supabaseRealtime = builder.AddContainer("supabase-realtime", "supabase/realtime")
            .WithHttpEndpoint(port: 4000, targetPort: 4000)
            .WithEnvironment("PORT", "4000")
            .WithEnvironment("DB_HOST", "postgres")
            .WithEnvironment("DB_NAME", "postgres")
            .WithEnvironment("DB_USER", "postgres")
            .WithEnvironment("DB_PASSWORD", "postgres")
            .WithEnvironment("DB_PORT", "5432")
            .WithEnvironment("SECRET_KEY_BASE", "your-secret-key-base")
            .WithEnvironment("ERL_AFLAGS", "-proto_dist inet_tcp")
            .WaitFor(postgres);

        // Supabase Storage
        var supabaseStorage = builder.AddContainer("supabase-storage", "supabase/storage-api")
            .WithHttpEndpoint(port: 5000, targetPort: 5000)
            .WithEnvironment("ANON_KEY", "your-anon-key")
            .WithEnvironment("SERVICE_KEY", "your-service-key")
            .WithEnvironment("POSTGREST_URL", "http://supabase-rest:3000")
            .WithEnvironment("PGRST_JWT_SECRET", "your-jwt-secret-key")
            .WithEnvironment("DATABASE_URL", postgresDb.Resource.ConnectionStringExpression)
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
    /// Adds the LocalAI stack with models and UI
    /// </summary>
    public static IResourceBuilder<ContainerResource> AddLocalAIStack(
        this IDistributedApplicationBuilder builder,
        string name = "localai")
    {
        // LocalAI Core
        var localAi = builder.AddContainer(name, "localai/localai")
            .WithHttpEndpoint(port: 8081, targetPort: 8080)
            .WithEnvironment("DEBUG", "true")
            .WithEnvironment("MODELS_PATH", "/models")
            .WithEnvironment("THREADS", "4")
            .WithEnvironment("CONTEXT_SIZE", "512")
            .WithVolume("localai-models", "/models")
            .WithVolume("localai-tmp", "/tmp/localai");

        // Open WebUI (Frontend for AI models)
        var openWebUi = builder.AddContainer("open-webui", "ghcr.io/open-webui/open-webui")
            .WithHttpEndpoint(port: 3001, targetPort: 8080)
            .WithEnvironment("OLLAMA_BASE_URL", "http://localai:8080/v1")
            .WithEnvironment("WEBUI_SECRET_KEY", "your-webui-secret-key")
            .WithVolume("open-webui-data", "/app/backend/data")
            .WaitFor(localAi);

        return localAi;
    }

    /// <summary>
    /// Adds vector database options (Qdrant and Milvus)
    /// </summary>
    public static (IResourceBuilder<ContainerResource> Qdrant, IResourceBuilder<ContainerResource> Milvus) AddVectorDatabases(
        this IDistributedApplicationBuilder builder)
    {
        // Qdrant Vector Database
        var qdrant = builder.AddContainer("qdrant", "qdrant/qdrant")
            .WithHttpEndpoint(port: 6333, targetPort: 6333)
            .WithHttpEndpoint(port: 6334, targetPort: 6334, name: "grpc")
            .WithVolume("qdrant-storage", "/qdrant/storage");

        // Milvus Vector Database (alternative to Qdrant)
        var milvusEtcd = builder.AddContainer("milvus-etcd", "quay.io/coreos/etcd")
            .WithEnvironment("ETCD_AUTO_COMPACTION_MODE", "revision")
            .WithEnvironment("ETCD_AUTO_COMPACTION_RETENTION", "1000")
            .WithEnvironment("ETCD_QUOTA_BACKEND_BYTES", "4294967296")
            .WithEnvironment("ETCD_SNAPSHOT_COUNT", "50000")
            .WithArgs("etcd", "-advertise-client-urls=http://127.0.0.1:2379", "-listen-client-urls=http://0.0.0.0:2379", "--data-dir", "/etcd")
            .WithVolume("milvus-etcd-data", "/etcd");

        var milvusMinIO = builder.AddContainer("milvus-minio", "minio/minio")
            .WithHttpEndpoint(port: 9001, targetPort: 9001)
            .WithHttpEndpoint(port: 9000, targetPort: 9000, name: "api")
            .WithEnvironment("MINIO_ACCESS_KEY", "minioadmin")
            .WithEnvironment("MINIO_SECRET_KEY", "minioadmin")
            .WithArgs("server", "/minio_data", "--console-address", ":9001")
            .WithVolume("milvus-minio-data", "/minio_data");

        var milvus = builder.AddContainer("milvus-standalone", "milvusdb/milvus")
            .WithHttpEndpoint(port: 19530, targetPort: 19530)
            .WithHttpEndpoint(port: 9091, targetPort: 9091, name: "metrics")
            .WithEnvironment("ETCD_ENDPOINTS", "milvus-etcd:2379")
            .WithEnvironment("MINIO_ADDRESS", "milvus-minio:9000")
            .WithArgs("milvus", "run", "standalone")
            .WithVolume("milvus-data", "/var/lib/milvus")
            .WaitFor(milvusEtcd)
            .WaitFor(milvusMinIO);

        return (qdrant, milvus);
    }

    /// <summary>
    /// Adds monitoring stack with Prometheus and Grafana
    /// </summary>
    public static (IResourceBuilder<ContainerResource> Prometheus, IResourceBuilder<ContainerResource> Grafana) AddMonitoringStack(
        this IDistributedApplicationBuilder builder)
    {
        // Prometheus for metrics
        var prometheus = builder.AddContainer("prometheus", "prom/prometheus")
            .WithHttpEndpoint(port: 9090, targetPort: 9090)
            .WithBindMount("../monitoring/prometheus.yml", "/etc/prometheus/prometheus.yml", isReadOnly: true)
            .WithVolume("prometheus-data", "/prometheus");

        // Grafana for dashboards
        var grafana = builder.AddContainer("grafana", "grafana/grafana")
            .WithHttpEndpoint(port: 3002, targetPort: 3000)
            .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin")
            .WithVolume("grafana-data", "/var/lib/grafana")
            .WaitFor(prometheus);

        return (prometheus, grafana);
    }
}
