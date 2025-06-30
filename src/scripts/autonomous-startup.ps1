# ExxerAI Autonomous Intelligence System Startup Script
# Optimized for Dual-GPU Windows Development Machine
# PowerShell 7+ Required

param(
    [switch]$Quick,
    [switch]$Development,
    [switch]$Production,
    [switch]$SkipDocker,
    [string]$LogLevel = "Information"
)

# ===== CONFIGURATION =====
$ErrorActionPreference = "Stop"
$VerbosePreference = "Continue"

$EXXERAI_ROOT = $PSScriptRoot | Split-Path
$DOCKER_COMPOSE_FILE = "$EXXERAI_ROOT\docker-compose.autonomous.yml"
$CONFIG_DIR = "$EXXERAI_ROOT\config"
$LOGS_DIR = "$EXXERAI_ROOT\logs"

# Colors for output
$SUCCESS_COLOR = "Green"
$WARNING_COLOR = "Yellow"
$ERROR_COLOR = "Red"
$INFO_COLOR = "Cyan"

# ===== FUNCTIONS =====

function Write-StatusMessage {
    param([string]$Message, [string]$Color = $INFO_COLOR)
    $timestamp = Get-Date -Format "HH:mm:ss"
    Write-Host "[$timestamp] $Message" -ForegroundColor $Color
}

function Test-Prerequisites {
    Write-StatusMessage "🔍 Checking prerequisites..." $INFO_COLOR
    
    # Check Docker Desktop
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        throw "Docker is not installed or not in PATH"
    }
    
    # Check Docker Compose
    if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue)) {
        throw "Docker Compose is not installed or not in PATH"
    }
    
    # Check NVIDIA Docker runtime
    try {
        $dockerInfo = docker info 2>$null | Out-String
        if ($dockerInfo -notmatch "nvidia") {
            Write-StatusMessage "⚠️ NVIDIA Docker runtime not detected. GPU features may not work." $WARNING_COLOR
        }
    } catch {
        Write-StatusMessage "⚠️ Could not verify NVIDIA Docker runtime." $WARNING_COLOR
    }
    
    # Check available memory
    $memory = Get-CimInstance -ClassName Win32_PhysicalMemory | Measure-Object -Property Capacity -Sum
    $memoryGB = [math]::Round($memory.Sum / 1GB, 1)
    Write-StatusMessage "💾 Available RAM: $memoryGB GB" $INFO_COLOR
    
    if ($memoryGB -lt 16) {
        Write-StatusMessage "⚠️ Less than 16GB RAM detected. Performance may be impacted." $WARNING_COLOR
    }
    
    # Check GPU status
    try {
        $gpuInfo = nvidia-smi --query-gpu=name,memory.total --format=csv,noheader,nounits 2>$null
        if ($gpuInfo) {
            Write-StatusMessage "🎮 GPU Information:" $SUCCESS_COLOR
            $gpuInfo | ForEach-Object {
                Write-StatusMessage "  $($_)" $INFO_COLOR
            }
        }
    } catch {
        Write-StatusMessage "⚠️ NVIDIA GPU not detected or nvidia-smi not available." $WARNING_COLOR
    }
    
    Write-StatusMessage "✅ Prerequisites check completed" $SUCCESS_COLOR
}

function Initialize-Environment {
    Write-StatusMessage "🚀 Initializing ExxerAI environment..." $INFO_COLOR
    
    # Create necessary directories
    $directories = @($LOGS_DIR, "$CONFIG_DIR\nginx", "$CONFIG_DIR\grafana\dashboards", "$CONFIG_DIR\grafana\datasources", "$EXXERAI_ROOT\data", "$EXXERAI_ROOT\credentials")
    
    foreach ($dir in $directories) {
        if (-not (Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            Write-StatusMessage "📁 Created directory: $dir" $INFO_COLOR
        }
    }
    
    # Initialize configuration files if they don't exist
    $configTemplate = "$CONFIG_DIR\appsettings.Autonomous.json"
    $productionConfig = "$CONFIG_DIR\appsettings.Production.json"
    
    if ((Test-Path $configTemplate) -and (-not (Test-Path $productionConfig))) {
        Copy-Item $configTemplate $productionConfig
        Write-StatusMessage "📄 Created production configuration from template" $INFO_COLOR
    }
    
    # Check for Google Drive credentials
    $credentialsFile = "$EXXERAI_ROOT\credentials\google-credentials.json"
    if (-not (Test-Path $credentialsFile)) {
        Write-StatusMessage "⚠️ Google Drive credentials not found at: $credentialsFile" $WARNING_COLOR
        Write-StatusMessage "   Business intelligence features will be limited without Google Drive access." $WARNING_COLOR
    }
    
    Write-StatusMessage "✅ Environment initialization completed" $SUCCESS_COLOR
}

function Start-DockerServices {
    param([bool]$QuickStart = $false)
    
    if ($SkipDocker) {
        Write-StatusMessage "⏭️ Skipping Docker services (--SkipDocker specified)" $WARNING_COLOR
        return
    }
    
    Write-StatusMessage "🐳 Starting ExxerAI Docker services..." $INFO_COLOR
    
    # Pull latest images first (unless quick start)
    if (-not $QuickStart) {
        Write-StatusMessage "📦 Pulling latest Docker images..." $INFO_COLOR
        docker-compose -f $DOCKER_COMPOSE_FILE pull
    }
    
    # Start core infrastructure first
    Write-StatusMessage "🔧 Starting core infrastructure..." $INFO_COLOR
    docker-compose -f $DOCKER_COMPOSE_FILE up -d postgres redis seq
    
    # Wait for databases to be ready
    Write-StatusMessage "⏳ Waiting for databases to initialize..." $INFO_COLOR
    Start-Sleep -Seconds 10
    
    # Start AI services
    Write-StatusMessage "🤖 Starting AI services..." $INFO_COLOR
    docker-compose -f $DOCKER_COMPOSE_FILE up -d ollama qdrant
    
    # Wait for AI services
    Write-StatusMessage "⏳ Waiting for AI services to load..." $INFO_COLOR
    Start-Sleep -Seconds 15
    
    # Start application services
    Write-StatusMessage "🎯 Starting ExxerAI application services..." $INFO_COLOR
    docker-compose -f $DOCKER_COMPOSE_FILE up -d exxerai-webapi exxerai-autonomous-worker exxerai-blazorui
    
    # Start monitoring and utilities
    Write-StatusMessage "📊 Starting monitoring services..." $INFO_COLOR
    docker-compose -f $DOCKER_COMPOSE_FILE up -d grafana nginx portainer
    
    Write-StatusMessage "✅ All Docker services started" $SUCCESS_COLOR
}

function Test-ServiceHealth {
    Write-StatusMessage "🏥 Checking service health..." $INFO_COLOR
    
    $services = @(
        @{ Name = "Ollama"; Url = "http://localhost:11434/api/version"; Expected = "llama" },
        @{ Name = "Qdrant"; Url = "http://localhost:6333/collections"; Expected = "result" },
        @{ Name = "PostgreSQL"; Port = 5432 },
        @{ Name = "Redis"; Port = 6379 },
        @{ Name = "ExxerAI WebAPI"; Url = "http://localhost:7001/health"; Expected = "Healthy" },
        @{ Name = "Seq Logging"; Url = "http://localhost:5341"; Expected = "Seq" },
        @{ Name = "Grafana"; Url = "http://localhost:3000"; Expected = "Grafana" },
        @{ Name = "Blazor UI"; Url = "http://localhost:7003"; Expected = "ExxerAI" }
    )
    
    $healthyCount = 0
    $totalCount = $services.Count
    
    foreach ($service in $services) {
        try {
            if ($service.Url) {
                $response = Invoke-WebRequest -Uri $service.Url -UseBasicParsing -TimeoutSec 5 2>$null
                if ($response.Content -match $service.Expected) {
                    Write-StatusMessage "✅ $($service.Name): Healthy" $SUCCESS_COLOR
                    $healthyCount++
                } else {
                    Write-StatusMessage "⚠️ $($service.Name): Responding but unexpected content" $WARNING_COLOR
                }
            } elseif ($service.Port) {
                $connection = Test-NetConnection -ComputerName localhost -Port $service.Port -WarningAction SilentlyContinue
                if ($connection.TcpTestSucceeded) {
                    Write-StatusMessage "✅ $($service.Name): Port $($service.Port) open" $SUCCESS_COLOR
                    $healthyCount++
                } else {
                    Write-StatusMessage "❌ $($service.Name): Port $($service.Port) not responding" $ERROR_COLOR
                }
            }
        } catch {
            Write-StatusMessage "❌ $($service.Name): Health check failed - $($_.Exception.Message)" $ERROR_COLOR
        }
    }
    
    $healthPercent = [math]::Round(($healthyCount / $totalCount) * 100, 1)
    Write-StatusMessage "📊 System Health: $healthyCount/$totalCount services healthy ($healthPercent%)" $INFO_COLOR
    
    if ($healthPercent -ge 80) {
        Write-StatusMessage "🎉 System is ready for autonomous operation!" $SUCCESS_COLOR
    } elseif ($healthPercent -ge 60) {
        Write-StatusMessage "⚠️ System partially ready. Some features may be limited." $WARNING_COLOR
    } else {
        Write-StatusMessage "❌ System not ready. Check service logs for issues." $ERROR_COLOR
    }
}

function Show-AccessInformation {
    Write-StatusMessage "🌐 ExxerAI Autonomous Intelligence System Access Information:" $SUCCESS_COLOR
    Write-Host ""
    Write-Host "  🎯 Main Application:" -ForegroundColor $SUCCESS_COLOR
    Write-Host "     Blazor UI:          http://localhost:7003" -ForegroundColor White
    Write-Host "     Web API:            http://localhost:7001" -ForegroundColor White
    Write-Host "     API Documentation:  http://localhost:7001/swagger" -ForegroundColor White
    Write-Host ""
    Write-Host "  🤖 AI Services:" -ForegroundColor $SUCCESS_COLOR
    Write-Host "     Ollama LLM:         http://localhost:11434" -ForegroundColor White
    Write-Host "     Qdrant Vector DB:   http://localhost:6333" -ForegroundColor White
    Write-Host ""
    Write-Host "  📊 Monitoring & Management:" -ForegroundColor $SUCCESS_COLOR
    Write-Host "     Seq Logging:        http://localhost:5341" -ForegroundColor White
    Write-Host "     Grafana Dashboards: http://localhost:3000 (admin/autonomous2024)" -ForegroundColor White
    Write-Host "     Portainer:          http://localhost:9000" -ForegroundColor White
    Write-Host ""
    Write-Host "  💾 Databases:" -ForegroundColor $SUCCESS_COLOR
    Write-Host "     PostgreSQL:         localhost:5432 (exxerai_intelligence)" -ForegroundColor White
    Write-Host "     Redis:              localhost:6379" -ForegroundColor White
    Write-Host ""
    Write-Host "  📁 Important Paths:" -ForegroundColor $SUCCESS_COLOR
    Write-Host "     Logs:               $LOGS_DIR" -ForegroundColor White
    Write-Host "     Configuration:      $CONFIG_DIR" -ForegroundColor White
    Write-Host "     Credentials:        $EXXERAI_ROOT\credentials" -ForegroundColor White
}

function Start-AutonomousMode {
    Write-StatusMessage "🤖 AUTONOMOUS INTELLIGENCE MODE ACTIVATED" $SUCCESS_COLOR
    Write-Host ""
    Write-Host "  🎯 Business Intelligence Targets:" -ForegroundColor $SUCCESS_COLOR
    Write-Host "     • Siemens, Rockwell, ABB (Partners)" -ForegroundColor White
    Write-Host "     • GM, Ford, VW, Tesla (Automotive Clients)" -ForegroundColor White
    Write-Host "     • Tremec, Valeo (Tier-1 Suppliers)" -ForegroundColor White
    Write-Host "     • Microsoft Ecosystem Monitoring" -ForegroundColor White
    Write-Host "     • Querétaro & El Bajío Manufacturing" -ForegroundColor White
    Write-Host ""
    Write-Host "  🔄 Autonomous Operations:" -ForegroundColor $SUCCESS_COLOR
    Write-Host "     • Hourly information collection cycles" -ForegroundColor White
    Write-Host "     • Google Drive document monitoring" -ForegroundColor White
    Write-Host "     • Personalized weekly report generation" -ForegroundColor White
    Write-Host "     • Learning from user interactions" -ForegroundColor White
    Write-Host "     • Company relationship tracking" -ForegroundColor White
    Write-Host ""
    Write-Host "  📈 Performance Optimization:" -ForegroundColor $SUCCESS_COLOR
    Write-Host "     • GPU 1: Ollama LLM Processing" -ForegroundColor White
    Write-Host "     • GPU 2: Qdrant Vector Operations" -ForegroundColor White
    Write-Host "     • Multi-threaded agent orchestration" -ForegroundColor White
    Write-Host "     • Real-time monitoring & logging" -ForegroundColor White
    Write-Host ""
    Write-StatusMessage "🚨 IMPORTANT: MCP calls may soon exceed human requests!" $WARNING_COLOR
    Write-StatusMessage "   System designed for full autonomous operation." $INFO_COLOR
}

# ===== MAIN EXECUTION =====

try {
    Write-Host ""
    Write-Host "🚀 ExxerAI Autonomous Intelligence System" -ForegroundColor $SUCCESS_COLOR
    Write-Host "   Dual-GPU Optimized Startup Script" -ForegroundColor $INFO_COLOR
    Write-Host "   $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor $INFO_COLOR
    Write-Host ""
    
    # Check prerequisites
    Test-Prerequisites
    
    # Initialize environment
    Initialize-Environment
    
    # Start Docker services
    Start-DockerServices -QuickStart:$Quick
    
    # Wait for services to stabilize
    if (-not $SkipDocker) {
        Write-StatusMessage "⏳ Allowing services to stabilize..." $INFO_COLOR
        Start-Sleep -Seconds 20
        
        # Test service health
        Test-ServiceHealth
    }
    
    # Show access information
    Show-AccessInformation
    
    # Activate autonomous mode
    Start-AutonomousMode
    
    Write-Host ""
    Write-StatusMessage "🎉 ExxerAI Autonomous Intelligence System is READY!" $SUCCESS_COLOR
    Write-StatusMessage "   System operating autonomously for business intelligence collection." $INFO_COLOR
    Write-StatusMessage "   Monitor progress via Seq (http://localhost:5341) and Grafana (http://localhost:3000)" $INFO_COLOR
    
} catch {
    Write-StatusMessage "❌ Startup failed: $($_.Exception.Message)" $ERROR_COLOR
    Write-StatusMessage "   Check the logs and ensure all prerequisites are met." $ERROR_COLOR
    exit 1
} 