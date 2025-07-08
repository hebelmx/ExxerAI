#!/bin/bash

# Build script that excludes ExxerAI.Aspire.AppHost project
# This is useful when working on the AppHost project separately

echo "Building ExxerAI solution excluding AppHost project..."

# Build all projects except AppHost
dotnet build --project "Core/ExxerAI.Domain/ExxerAI.Domain.csproj" "$@"
dotnet build --project "Core/ExxerAI.Application/ExxerAI.Application.csproj" "$@"
dotnet build --project "Orchestration/ExxerAI.Orchestration/ExxerAI.Orchestration.csproj" "$@"
dotnet build --project "Orchestration/ExxerAI.Aspire.ServiceDefaults/ExxerAI.Aspire.ServiceDefaults.csproj" "$@"
dotnet build --project "Infraestructure/ExxerAI.Infrastructure/ExxerAI.Infrastructure.csproj" "$@"
dotnet build --project "Infraestructure/ExxerAI.API/ExxerAI.API.csproj" "$@"
dotnet build --project "Infraestructure/ExxerAI.CLI/ExxerAI.CLI.csproj" "$@"
dotnet build --project "Infraestructure/ExxerAi.MCPServer/ExxerAi.MCPServer.csproj" "$@"
dotnet build --project "Presentation/ExxerAI.UI/ExxerAI.UI.csproj" "$@"
dotnet build --project "Presentation/ExxerAI.UI.Dashboard/ExxerAI.Aspire.Dashboard.csproj" "$@"
dotnet build --project "Presentation/ExxerAI.UI.Library/ExxerAI.UI.Library.csproj" "$@"

# Build test projects
dotnet build --project "tests/ExxerAI.Domain.Tests/ExxerAI.Domain.Tests.csproj" "$@"
dotnet build --project "tests/ExxerAI.Application.Tests/ExxerAI.Application.Tests.csproj" "$@"
dotnet build --project "tests/ExxerAI.Infrastructure.Tests/ExxerAI.Infrastructure.Tests.csproj" "$@"
dotnet build --project "tests/ExxerAI.Orchestration.Tests/ExxerAI.Orchestration.Tests.csproj" "$@"
dotnet build --project "tests/ExxerAI.API.Tests/ExxerAI.API.Tests.csproj" "$@"
dotnet build --project "tests/ExxerAI.CLI.Tests/ExxerAI.CLI.Tests.csproj" "$@"
dotnet build --project "tests/ExxerAI.IntegrationTests/ExxerAI.IntegrationTests.csproj" "$@"
dotnet build --project "tests/ExxerAI.Architecture.Tests/ExxerAI.Architecture.Tests.csproj" "$@"

echo "Build completed (excluding AppHost project)"