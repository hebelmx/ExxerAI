# LocalAI Aspire Orchestrator

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

This is a .NET Aspire application that orchestrates a local AI stack including:

## Stack Components
- **Supabase** - Backend-as-a-Service with PostgreSQL, Auth, Realtime, and Storage
- **LocalAI** - Local AI model server compatible with OpenAI API
- **SearXNG** - Privacy-respecting search engine
- **Vector Databases** - Qdrant and Milvus for embeddings storage
- **Monitoring** - Prometheus and Grafana for observability
- **Redis** - Caching and session storage

## Architecture Guidelines
- Use proper Aspire resource builder patterns
- Implement health checks for all services
- Use named volumes for persistent data
- Configure proper service dependencies with `WaitFor()`
- Use environment variables for configuration
- Implement proper logging and telemetry

## Aspire Best Practices
- Use the correct resource builders (AddPostgres, AddRedis, AddContainer)
- Configure volumes using `WithVolume()` for named volumes
- Use `WithBindMount()` for configuration files
- Implement proper service discovery through Aspire's built-in networking
- Use connection string expressions for database connectivity
- Configure health checks for all external services

## Configuration
- Environment-specific configurations should use appsettings files
- Secrets should be managed through user secrets or Azure Key Vault
- Use Aspire's configuration providers for service discovery
