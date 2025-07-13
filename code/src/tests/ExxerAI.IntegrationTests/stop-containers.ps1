#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Stops persistent integration test containers for ExxerAI

.DESCRIPTION
    This script stops all integration test containers:
    - LocalAI (LLM processing)
    - Neo4j (graph database)
    - Qdrant (vector database)
    - Redis (caching)

.PARAMETER Service
    Stop only specific service(s). Valid values: localai, neo4j, qdrant, redis

.PARAMETER Clean
    Also remove containers and volumes (destructive operation)

.EXAMPLE
    .\stop-containers.ps1
    Stops all integration test containers

.EXAMPLE
    .\stop-containers.ps1 -Service neo4j,qdrant
    Stops only Neo4j and Qdrant containers

.EXAMPLE
    .\stop-containers.ps1 -Clean
    Stops containers and removes volumes (data loss!)
#>

param(
    [string[]]$Service = @(),
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

# Container configuration
$ComposeFile = "docker-compose.yml"
$ProjectName = "exxerai-integration"

Write-Host "🛑 Stopping ExxerAI Integration Test Containers" -ForegroundColor Red
Write-Host "=================================================" -ForegroundColor Red

# Check if Docker is running
try {
    docker info | Out-Null
    Write-Host "✅ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not running or not accessible" -ForegroundColor Red
    Write-Host "Please start Docker Desktop and try again" -ForegroundColor Yellow
    exit 1
}

# Check if docker-compose file exists
if (!(Test-Path $ComposeFile)) {
    Write-Host "❌ docker-compose.yml not found in current directory" -ForegroundColor Red
    exit 1
}

# Stop containers
try {
    if ($Service.Count -gt 0) {
        Write-Host "🎯 Stopping specific services: $($Service -join ', ')" -ForegroundColor Cyan
        docker-compose -f $ComposeFile -p $ProjectName stop $Service
    } else {
        Write-Host "🎯 Stopping all integration test containers..." -ForegroundColor Cyan
        docker-compose -f $ComposeFile -p $ProjectName stop
    }
    
    Write-Host "✅ Containers stopped successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to stop containers: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Clean up if requested
if ($Clean) {
    Write-Host "🧹 Cleaning up containers and volumes..." -ForegroundColor Yellow
    Write-Host "⚠️  WARNING: This will remove all data in the containers!" -ForegroundColor Red
    
    $confirmation = Read-Host "Are you sure you want to continue? (y/N)"
    if ($confirmation.ToLower() -eq 'y') {
        try {
            docker-compose -f $ComposeFile -p $ProjectName down --volumes --remove-orphans
            Write-Host "✅ Containers and volumes cleaned up" -ForegroundColor Green
        } catch {
            Write-Host "❌ Failed to clean up: $($_.Exception.Message)" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "❌ Cleanup cancelled" -ForegroundColor Yellow
    }
}

# Display container status
Write-Host "`n📊 Container Status:" -ForegroundColor Cyan
docker-compose -f $ComposeFile -p $ProjectName ps

Write-Host "`n✅ Integration test containers stopped!" -ForegroundColor Green
Write-Host "💡 Start containers with: .\start-containers.ps1" -ForegroundColor Cyan

# Show volume information
if (!$Clean) {
    Write-Host "`n💾 Data volumes are preserved:" -ForegroundColor Yellow
    Write-Host "- exxerai-localai-models" -ForegroundColor White
    Write-Host "- exxerai-localai-data" -ForegroundColor White
    Write-Host "- exxerai-neo4j-data" -ForegroundColor White
    Write-Host "- exxerai-qdrant-data" -ForegroundColor White
    Write-Host "- exxerai-redis-data" -ForegroundColor White
    Write-Host "🗑️  To remove volumes: .\stop-containers.ps1 -Clean" -ForegroundColor Yellow
} 