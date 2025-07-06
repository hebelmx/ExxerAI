#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Start the LocalAI Aspire Orchestrator

.DESCRIPTION
    This PowerShell script starts the .NET Aspire orchestrator that replaces the Python Docker Compose setup.
    It provides better tooling, observability, and management of the local AI stack.

.PARAMETER Clean
    Clean build artifacts before starting

.PARAMETER NoBrowser
    Don't automatically open the Aspire dashboard in browser

.EXAMPLE
    .\Start-AspireOrchestrator.ps1
    
.EXAMPLE
    .\Start-AspireOrchestrator.ps1 -Clean
#>

param(
    [switch]$Clean,
    [switch]$NoBrowser
)

# Set error action preference
$ErrorActionPreference = "Stop"

Write-Host "🚀 Starting LocalAI Aspire Orchestrator" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK Version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 8.0 or later." -ForegroundColor Red
    Write-Host "   Download from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Check if Docker is running
try {
    docker info | Out-Null
    Write-Host "✅ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

# Navigate to script directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Clean if requested
if ($Clean) {
    Write-Host "🧹 Cleaning build artifacts..." -ForegroundColor Yellow
    dotnet clean
}

# Restore packages
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# Build the solution
Write-Host "🔨 Building solution..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful!" -ForegroundColor Green

# Display service information
Write-Host "`n🌐 Service URLs (available after startup):" -ForegroundColor Cyan
Write-Host "  • Aspire Dashboard:   http://localhost:15000" -ForegroundColor White
Write-Host "  • LocalAI API:        http://localhost:8081/v1" -ForegroundColor White  
Write-Host "  • Open WebUI:         http://localhost:3001" -ForegroundColor White
Write-Host "  • SearXNG:            http://localhost:8080" -ForegroundColor White
Write-Host "  • Supabase REST:      http://localhost:3000" -ForegroundColor White
Write-Host "  • Supabase Auth:      http://localhost:9999" -ForegroundColor White
Write-Host "  • Qdrant:             http://localhost:6333" -ForegroundColor White
Write-Host "  • Prometheus:         http://localhost:9090" -ForegroundColor White
Write-Host "  • Grafana:            http://localhost:3002 (admin/admin)" -ForegroundColor White
Write-Host "  • Nginx Gateway:      http://localhost:80" -ForegroundColor White

Write-Host "`n⏳ Starting Aspire orchestrator..." -ForegroundColor Yellow
Write-Host "   This will download Docker images on first run (may take several minutes)" -ForegroundColor Gray

# Start the Aspire app
try {
    if (-not $NoBrowser) {
        Write-Host "`n🌐 The Aspire dashboard will open automatically in your browser" -ForegroundColor Green
    }
    
    $aspireArgs = @("run", "--project", "LocalAI.Aspire.AppHost")
    if ($NoBrowser) {
        $aspireArgs += "--no-launch-profile"
    }
    
    & dotnet $aspireArgs
} catch {
    Write-Host "❌ Failed to start Aspire orchestrator: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n👋 Aspire orchestrator stopped" -ForegroundColor Yellow
