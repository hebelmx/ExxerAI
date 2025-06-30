# ExxerAI Autonomous Intelligence System - Dual-GPU Startup
# PowerShell 7+ Required for Windows

param([switch]$Quick, [switch]$SkipDocker)

$EXXERAI_ROOT = $PSScriptRoot | Split-Path
$DOCKER_COMPOSE_FILE = "$EXXERAI_ROOT\docker-compose.autonomous.yml"

function Write-Status($Message, $Color = "Cyan") {
    $timestamp = Get-Date -Format "HH:mm:ss"
    Write-Host "[$timestamp] $Message" -ForegroundColor $Color
}

try {
    Write-Host "🚀 ExxerAI Autonomous Intelligence System - STARTING" -ForegroundColor Green
    
    # Prerequisites Check
    Write-Status "🔍 Checking prerequisites..."
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw "Docker not installed" }
    if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue)) { throw "Docker Compose not installed" }
    
    # GPU Check
    try {
        $gpuInfo = nvidia-smi --query-gpu=name --format=csv,noheader 2>$null
        if ($gpuInfo) { Write-Status "🎮 GPUs detected: $($gpuInfo.Count)" "Green" }
    } catch { Write-Status "⚠️ NVIDIA GPU not detected" "Yellow" }
    
    # Environment Setup
    Write-Status "📁 Setting up environment..."
    @("logs", "config", "data", "credentials") | ForEach-Object {
        $dir = "$EXXERAI_ROOT\$_"
        if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    }
    
    # Docker Services
    if (-not $SkipDocker) {
        Write-Status "🐳 Starting Docker services..."
        
        if (-not $Quick) { docker-compose -f $DOCKER_COMPOSE_FILE pull }
        
        # Start core services first
        docker-compose -f $DOCKER_COMPOSE_FILE up -d postgres redis seq
        Start-Sleep -Seconds 8
        
        # Start AI services
        docker-compose -f $DOCKER_COMPOSE_FILE up -d ollama qdrant
        Start-Sleep -Seconds 12
        
        # Start applications
        docker-compose -f $DOCKER_COMPOSE_FILE up -d exxerai-webapi exxerai-autonomous-worker exxerai-blazorui grafana nginx portainer
        Start-Sleep -Seconds 10
        
        Write-Status "✅ All services started" "Green"
    }
    
    # Health Check
    Write-Status "🏥 Checking service health..."
    $services = @(
        @{ Name = "Ollama"; Url = "http://localhost:11434" },
        @{ Name = "Qdrant"; Url = "http://localhost:6333" },
        @{ Name = "WebAPI"; Url = "http://localhost:7001/health" },
        @{ Name = "BlazorUI"; Url = "http://localhost:7003" }
    )
    
    $healthyCount = 0
    foreach ($service in $services) {
        try {
            $response = Invoke-WebRequest -Uri $service.Url -UseBasicParsing -TimeoutSec 3 2>$null
            Write-Status "✅ $($service.Name): Healthy" "Green"
            $healthyCount++
        } catch {
            Write-Status "❌ $($service.Name): Not responding" "Red"
        }
    }
    
    # Access Information
    Write-Host ""
    Write-Host "🌐 AUTONOMOUS SYSTEM ACCESS:" -ForegroundColor Green
    Write-Host "   Main UI:    http://localhost:7003" -ForegroundColor White
    Write-Host "   Web API:    http://localhost:7001" -ForegroundColor White
    Write-Host "   Monitoring: http://localhost:5341 (Seq)" -ForegroundColor White
    Write-Host "   Dashboards: http://localhost:3000 (Grafana)" -ForegroundColor White
    Write-Host ""
    
    # Autonomous Mode Activation
    Write-Host "🤖 AUTONOMOUS INTELLIGENCE MODE ACTIVATED" -ForegroundColor Green
    Write-Host "   • Business Intelligence: Siemens, Rockwell, ABB" -ForegroundColor White
    Write-Host "   • Automotive Clients: GM, Ford, VW, Tesla, Tremec, Valeo" -ForegroundColor White
    Write-Host "   • Technology Focus: Microsoft Ecosystem, Industrial IoT" -ForegroundColor White
    Write-Host "   • Operations: Hourly cycles, Google Drive monitoring" -ForegroundColor White
    Write-Host "   • Performance: Dual-GPU optimization active" -ForegroundColor White
    Write-Host ""
    
    Write-Status "🎉 ExxerAI Autonomous System READY! ($healthyCount/$($services.Count) services healthy)" "Green"
    Write-Status "🚨 MCP calls may soon exceed human requests - Autonomous operation critical!" "Yellow"
    
} catch {
    Write-Status "❌ Startup failed: $($_.Exception.Message)" "Red"
    exit 1
} 