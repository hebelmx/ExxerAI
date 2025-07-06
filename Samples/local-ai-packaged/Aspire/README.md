# LocalAI Aspire Orchestrator

This .NET Aspire application replaces the Python Docker Compose orchestrator, providing better tooling, observability, and management of your local AI stack.

## Features

- **Supabase Stack**: PostgreSQL database with Auth, Realtime, and Storage services
- **LocalAI**: OpenAI-compatible local AI model server
- **Open WebUI**: Modern web interface for AI models
- **SearXNG**: Privacy-respecting search engine
- **Vector Databases**: Qdrant and Milvus for embeddings storage
- **Monitoring**: Prometheus metrics and Grafana dashboards
- **Redis**: Caching and session storage
- **Nginx**: Reverse proxy and API gateway

## Prerequisites

- **.NET 8.0 SDK** or later
- **Docker Desktop** running
- **Visual Studio 2022** or **VS Code** with C# extension

## Getting Started

### 1. Clone and Setup

```bash
# Navigate to the Aspire directory
cd "f:\Dynamic\ExxerAi\ExxerAI\Samples\local-ai-packaged\Aspire"

# Restore packages
dotnet restore
```

### 2. Configure Services

Before running, make sure you have the required configuration directories:

- `searxng/` - SearXNG configuration files
- `monitoring/prometheus.yml` - Prometheus configuration (already created)
- `nginx/nginx.conf` - Nginx configuration (already created)

### 3. Run the Application

```bash
# Build and run
dotnet run --project LocalAI.Aspire.AppHost
```

Or use Visual Studio:
1. Open `LocalAI.Aspire.sln`
2. Set `LocalAI.Aspire.AppHost` as startup project
3. Press F5 to start

### 4. Access Services

Once running, you can access:

- **Aspire Dashboard**: `http://localhost:15000` (main orchestration dashboard)
- **LocalAI API**: `http://localhost:8081/v1` (OpenAI-compatible API)
- **Open WebUI**: `http://localhost:3001` (AI chat interface)
- **SearXNG**: `http://localhost:8080` (search interface)
- **Supabase REST API**: `http://localhost:3000` (database API)
- **Supabase Auth**: `http://localhost:9999` (authentication)
- **Qdrant**: `http://localhost:6333` (vector database)
- **Prometheus**: `http://localhost:9090` (metrics)
- **Grafana**: `http://localhost:3002` (dashboards, admin/admin)
- **Nginx Gateway**: `http://localhost:80` (unified API access)

## Architecture

### Service Dependencies

```
PostgreSQL (Supabase DB)
├── Supabase Auth (depends on PostgreSQL)
├── Supabase REST API (depends on PostgreSQL)
├── Supabase Realtime (depends on PostgreSQL)
└── Supabase Storage (depends on REST API)

LocalAI Stack
├── LocalAI Server
└── Open WebUI (depends on LocalAI)

Vector Databases
├── Qdrant (standalone)
└── Milvus Stack
    ├── Milvus etcd
    ├── Milvus MinIO
    └── Milvus (depends on etcd + MinIO)

Monitoring
├── Prometheus
└── Grafana (depends on Prometheus)

Gateway
└── Nginx (depends on all services)
```

### Data Persistence

All services use named Docker volumes for data persistence:
- `postgres-data` - PostgreSQL database
- `redis-data` - Redis cache
- `localai-models` - AI model storage
- `qdrant-storage` - Vector embeddings
- `prometheus-data` - Metrics history
- `grafana-data` - Dashboard configurations

## Configuration

### Environment Variables

Key configuration can be set via environment variables or `appsettings.json`:

```json
{
  "GOTRUE_JWT_SECRET": "your-secure-jwt-secret",
  "SUPABASE_SERVICE_KEY": "your-service-key",
  "LOCALAI_THREADS": "4",
  "LOCALAI_CONTEXT_SIZE": "512"
}
```

### Secrets Management

For production, use .NET user secrets or Azure Key Vault:

```bash
# Set user secrets for development
dotnet user-secrets set "Supabase:JwtSecret" "your-secret-key"
dotnet user-secrets set "LocalAI:ApiKey" "your-api-key"
```

## Development

### Adding New Services

To add new services, extend the `LocalAIExtensions.cs` file:

```csharp
public static IResourceBuilder<ContainerResource> AddMyService(
    this IDistributedApplicationBuilder builder)
{
    return builder.AddContainer("my-service", "my-image:latest")
        .WithHttpEndpoint(port: 8000, targetPort: 80)
        .WithEnvironment("MY_CONFIG", "value");
}
```

### Debugging

The Aspire dashboard provides:
- **Real-time logs** from all services
- **Resource health** monitoring
- **Metrics visualization**
- **Distributed tracing**
- **Configuration management**

## Comparison with Python Orchestrator

| Feature | Python Script | .NET Aspire |
|---------|---------------|-------------|
| **Service Discovery** | Manual container linking | Automatic service naming |
| **Health Checks** | None | Built-in health monitoring |
| **Logging** | Docker logs only | Centralized logging with filtering |
| **Metrics** | Manual Prometheus setup | Built-in telemetry |
| **Development Experience** | CLI-based | Rich dashboard + IDE integration |
| **Configuration** | Environment files | Structured configuration + secrets |
| **Dependencies** | Manual startup order | Automatic dependency resolution |
| **Scaling** | Manual container management | Resource-based scaling |

## Troubleshooting

### Common Issues

1. **Port Conflicts**: Check if ports are already in use
   ```bash
   netstat -an | findstr ":8080"
   ```

2. **Docker Issues**: Ensure Docker Desktop is running
   ```bash
   docker ps
   ```

3. **Build Errors**: Clean and restore packages
   ```bash
   dotnet clean
   dotnet restore
   ```

### Logs

View service logs through:
- **Aspire Dashboard**: Real-time log streaming
- **Docker Desktop**: Container log viewer
- **Command Line**: `docker logs <container-name>`

## Migration from Python Script

To migrate from the Python orchestrator:

1. **Stop existing services**:
   ```bash
   python start_services.py --stop
   ```

2. **Remove old containers** (optional):
   ```bash
   docker system prune
   ```

3. **Start Aspire orchestrator**:
   ```bash
   dotnet run --project LocalAI.Aspire.AppHost
   ```

Your data will be preserved in Docker volumes with the same names.

## Contributing

To contribute to this orchestrator:

1. Follow the coding guidelines in `.github/copilot-instructions.md`
2. Use proper Aspire resource builder patterns
3. Add comprehensive logging and health checks
4. Update this README with any new services or configuration
