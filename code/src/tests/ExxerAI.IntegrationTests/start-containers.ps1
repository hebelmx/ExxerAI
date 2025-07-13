#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Starts persistent integration test containers for ExxerAI

.DESCRIPTION
    This script starts all required containers for integration testing:
    - LocalAI (LLM processing)
    - Neo4j (graph database)
    - Qdrant (vector database)
    - Redis (caching)

.PARAMETER Force
    Force recreate containers even if they already exist

.PARAMETER Service
    Start only specific service(s). Valid values: localai, neo4j, qdrant, redis

.EXAMPLE
    .\start-containers.ps1
    Starts all integration test containers

.EXAMPLE
    .\start-containers.ps1 -Service neo4j,qdrant
    Starts only Neo4j and Qdrant containers
#>

param(
    [switch]$Force,
    [string[]]$Service = @()
)

$ErrorActionPreference = "Stop"

# Container configuration
$ComposeFile = "docker-compose.yml"
$ProjectName = "exxerai-integration"

Write-Host "🚀 Starting ExxerAI Integration Test Containers" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

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

# Stop existing containers if force flag is used
if ($Force) {
    Write-Host "🔄 Force flag detected - stopping existing containers..." -ForegroundColor Yellow
    docker-compose -f $ComposeFile -p $ProjectName down --remove-orphans
}

# Start containers
try {
    if ($Service.Count -gt 0) {
        Write-Host "🎯 Starting specific services: $($Service -join ', ')" -ForegroundColor Cyan
        docker-compose -f $ComposeFile -p $ProjectName up -d $Service
    } else {
        Write-Host "🎯 Starting all integration test containers..." -ForegroundColor Cyan
        docker-compose -f $ComposeFile -p $ProjectName up -d
    }
    
    Write-Host "✅ Containers started successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to start containers: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Wait for containers to be healthy
Write-Host "🔍 Checking container health..." -ForegroundColor Cyan

$services = if ($Service.Count -gt 0) { $Service } else { @('localai', 'neo4j', 'qdrant', 'redis') }
$maxWait = 180  # 3 minutes
$checkInterval = 5  # 5 seconds

foreach ($serviceName in $services) {
    $containerName = "exxerai-$serviceName-integration"
    Write-Host "⏳ Waiting for $serviceName to be healthy..." -ForegroundColor Yellow
    
    $elapsed = 0
    while ($elapsed -lt $maxWait) {
        $health = docker inspect --format='{{.State.Health.Status}}' $containerName 2>$null
        
        if ($health -eq "healthy") {
            Write-Host "✅ $serviceName is healthy" -ForegroundColor Green
            break
        } elseif ($health -eq "unhealthy") {
            Write-Host "❌ $serviceName is unhealthy" -ForegroundColor Red
            docker logs $containerName --tail 20
            break
        } else {
            Write-Host "⏳ $serviceName status: $health (waiting...)" -ForegroundColor Yellow
            Start-Sleep $checkInterval
            $elapsed += $checkInterval
        }
    }
    
    if ($elapsed -ge $maxWait) {
        Write-Host "⚠️ $serviceName health check timed out after $maxWait seconds" -ForegroundColor Yellow
    }
}

# Display container status
Write-Host "`n📊 Container Status:" -ForegroundColor Cyan
docker-compose -f $ComposeFile -p $ProjectName ps

# Display connection information
Write-Host "`n🔗 Connection Information:" -ForegroundColor Cyan
Write-Host "Neo4j HTTP:   http://localhost:7475 (neo4j/test123456)" -ForegroundColor White
Write-Host "Neo4j Bolt:   bolt://localhost:7688 (neo4j/test123456)" -ForegroundColor White
Write-Host "Qdrant HTTP:  http://localhost:6333" -ForegroundColor White
Write-Host "Qdrant gRPC:  localhost:6334" -ForegroundColor White
Write-Host "LocalAI API:  http://localhost:8080" -ForegroundColor White
Write-Host "Redis:        localhost:6379 (password: test123456)" -ForegroundColor White

Write-Host "`n✅ Integration test containers are ready!" -ForegroundColor Green
Write-Host "💡 Run tests with: dotnet test" -ForegroundColor Cyan
Write-Host "🛑 Stop containers with: .\stop-containers.ps1" -ForegroundColor Yellow 