# LocalAI Aspire Configuration Guide

This document explains how to use the strongly-typed configuration system in the LocalAI Aspire orchestrator.

## Overview

The LocalAI Aspire orchestrator uses a comprehensive configuration system that allows you to:

1. **Configure all services** through strongly-typed classes
2. **Override settings** using environment variables 
3. **Inject configurations** into client applications
4. **Validate configurations** at startup
5. **Provide sensible defaults** while allowing customization

## Configuration Structure

The configuration is organized into logical groups:

```
LocalAIStack
├── Database (PostgreSQL/Supabase)
├── LocalAI (AI Models & Web UI)  
├── Search (SearXNG)
├── VectorDatabases (Qdrant & Milvus)
├── Monitoring (Prometheus & Grafana)
├── Network (Nginx & Redis)
└── Security (Authentication & CORS)
```

## Configuration Files

### appsettings.json
Contains all default configuration values with sensible defaults for development.

### Environment Variables
Override sensitive settings in production:

```bash
# Database
LOCALAI_DB_PASSWORD=your-secure-password
LOCALAI_DB_USERNAME=postgres

# Supabase
SUPABASE_JWT_SECRET=your-jwt-secret-32-chars-min
SUPABASE_ANON_KEY=your-anon-key

# LocalAI
LOCALAI_API_KEY=your-api-key

# Vector Databases
QDRANT_API_KEY=your-qdrant-key

# Security
LOCALAI_ENCRYPTION_KEY=your-32-char-encryption-key
```

## Usage Examples

### In Client Applications

```csharp
// Inject configuration into your client apps
public class MyService
{
    private readonly LocalAIStackConfiguration _config;
    
    public MyService(IOptions<LocalAIStackConfiguration> config)
    {
        _config = config.Value;
    }
    
    public async Task ConnectToLocalAI()
    {
        var apiUrl = $"http://localhost:{_config.LocalAI.ApiPort}/v1";
        // Use the configured URL to connect
    }
    
    public string GetDatabaseConnectionString()
    {
        var db = _config.Database;
        return $"Host={db.Host};Port={db.Port};Database={db.DatabaseName};Username={db.Username};Password={db.Password}";
    }
}
```

### Dynamic Configuration

```csharp
// Access configuration at runtime
public class ConfigurationController : ControllerBase
{
    private readonly ConfigurationService _configService;
    
    public ConfigurationController(ConfigurationService configService)
    {
        _configService = configService;
    }
    
    [HttpGet("services")]
    public IActionResult GetServiceUrls()
    {
        return Ok(_configService.GetServiceUrls());
    }
    
    [HttpGet("database-connection")]
    public IActionResult GetDatabaseConnection()
    {
        return Ok(_configService.GetDatabaseConnectionString());
    }
}
```

## Port Management

All ports are configured and validated to prevent conflicts:

- **Database**: 5432 (PostgreSQL)
- **Supabase REST**: 3000
- **Supabase Auth**: 9999
- **LocalAI API**: 8081
- **Open WebUI**: 3001
- **SearXNG**: 8080
- **Qdrant**: 6333
- **Milvus**: 19530, 9091 (web)
- **Prometheus**: 9090
- **Grafana**: 3002
- **Nginx**: 80, 443
- **Redis**: 6379

## Benefits of This Approach

1. **Type Safety**: Compile-time checking of configuration values
2. **IntelliSense**: Full IDE support for configuration properties
3. **Validation**: Automatic validation of required settings and port conflicts
4. **Environment Separation**: Easy configuration for dev/staging/production
5. **Service Discovery**: Aspire automatically handles service-to-service communication
6. **Observability**: Built-in integration with Aspire dashboard and telemetry
7. **Self-Contained**: No external configuration files needed for basic operation

## Migration from PowerShell Script

The new .NET AppHost eliminates the need for:
- ❌ PowerShell script for orchestration
- ❌ Manual Docker Compose management
- ❌ Separate environment variable files
- ❌ Manual dependency checking

And provides:
- ✅ Self-contained executable with embedded .NET runtime
- ✅ Automatic environment validation and build process
- ✅ Strongly-typed configuration with validation
- ✅ Built-in service discovery and health checks
- ✅ Rich observability through Aspire dashboard
- ✅ Single executable deployment

## Command Line Usage

```bash
# Basic startup
LocalAI.Aspire.AppHost.exe

# Clean build before startup
LocalAI.Aspire.AppHost.exe --clean

# Run without opening browser
LocalAI.Aspire.AppHost.exe --no-browser

# Production environment
LocalAI.Aspire.AppHost.exe --environment Production

# Help
LocalAI.Aspire.AppHost.exe --help
```
