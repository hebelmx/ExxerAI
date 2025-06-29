# ExxerAI Intelligence System

A C#/.NET-based orchestration framework for managing contextual and persona-based interactions with Large Language Models (LLMs).

## Quick Start
1. Clone the repository
2. Run: `dotnet restore`
3. Configure appsettings.json with your API keys
4. Run: `dotnet run --project src/ExxerAI.WebAPI`

## Architecture
- **Layered Architecture** with clean separation of concerns
- **Domain-Driven Design** with rich domain models
- **Dependency Injection** throughout all layers
- **Interface-First Design** for testability and extensibility

## Project Structure
- **src/ExxerAI.Domain/** - Core business logic and entities
- **src/ExxerAI.Application/** - Use cases and application services
- **src/ExxerAI.Infrastructure/** - External adapters and implementations
- **src/ExxerAI.WebAPI/** - REST API presentation layer
- **src/ExxerAI.BlazorUI/** - Blazor web application
- **src/ExxerAI.CLI/** - Command line interface

## Technology Stack
- .NET 8
- PostgreSQL with vector support
- Redis for caching
- OpenAI API
- Blazor for web UI
