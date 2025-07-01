#!/bin/bash

# Ubuntu Setup Diagnostic Script
# Run this to check current status and identify issues

set +e  # Don't exit on errors during diagnosis

echo "🔍 Diagnosing Ubuntu Agent Network Setup..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "\n${BLUE}=== $1 ===${NC}"
}

# 1. Basic System Info
print_header "SYSTEM INFORMATION"
echo "Hostname: $(hostname)"
echo "Kernel: $(uname -r)"
echo "OS: $(cat /etc/os-release | grep PRETTY_NAME | cut -d'"' -f2)"
echo "Uptime: $(uptime -p)"
echo "Current User: $(whoami)"
echo "IP Address: $(hostname -I | awk '{print $1}')"

# 2. User Account Status
print_header "USER ACCOUNTS"
if id claude &>/dev/null; then
    print_status "✅ Claude user exists"
    echo "Groups: $(groups claude)"
else
    print_error "❌ Claude user not found"
fi

# 3. SSH Configuration
print_header "SSH CONFIGURATION"
if [ -d /home/claude/.ssh ]; then
    print_status "✅ SSH directory exists"
    echo "Permissions: $(ls -ld /home/claude/.ssh)"
    if [ -f /home/claude/.ssh/authorized_keys ]; then
        print_status "✅ Authorized keys exist"
        echo "Key count: $(wc -l < /home/claude/.ssh/authorized_keys)"
    else
        print_warning "❌ No authorized keys found"
    fi
else
    print_error "❌ SSH directory missing"
fi

# 4. Sudo Access
print_header "SUDO ACCESS"
if sudo -l -U claude 2>/dev/null | grep -q NOPASSWD; then
    print_status "✅ Claude has sudo access"
else
    print_warning "❌ Claude sudo access unclear"
fi

# 5. System Updates Status
print_header "SYSTEM UPDATES"
if command -v apt &> /dev/null; then
    echo "Last update: $(stat -c %y /var/cache/apt/pkgcache.bin 2>/dev/null || echo 'Unknown')"
    UPDATES=$(apt list --upgradable 2>/dev/null | wc -l)
    echo "Available updates: $((UPDATES-1))"
fi

# 6. Essential Packages
print_header "INSTALLED PACKAGES"
packages=("curl" "wget" "git" "build-essential" "python3" "nodejs" "unzip")
for pkg in "${packages[@]}"; do
    if command -v $pkg &> /dev/null; then
        print_status "✅ $pkg installed"
    else
        print_warning "❌ $pkg missing"
    fi
done

# 7. .NET SDK Status
print_header ".NET SDK STATUS"
if command -v dotnet &> /dev/null; then
    print_status "✅ .NET SDK available"
    echo "Version: $(dotnet --version)"
    echo "Location: $(which dotnet)"
else
    print_error "❌ .NET SDK not found"
fi

# 8. GPU Status
print_header "GPU STATUS"
if command -v nvidia-smi &> /dev/null; then
    print_status "✅ NVIDIA tools available"
    nvidia-smi --query-gpu=index,name,memory.total --format=csv,noheader,nounits || print_error "GPU query failed"
else
    print_warning "❌ nvidia-smi not available"
fi

# 9. Container System
print_header "CONTAINER SYSTEM"
if command -v podman &> /dev/null; then
    print_status "✅ Podman available"
    echo "Version: $(podman --version)"
    echo "Running containers:"
    podman ps --format "table {{.Names}}\t{{.Status}}" 2>/dev/null || echo "No containers"
elif command -v docker &> /dev/null; then
    print_status "✅ Docker available"
    echo "Version: $(docker --version)"
else
    print_warning "❌ No container system found"
fi

# 10. NVIDIA Container Toolkit
print_header "NVIDIA CONTAINER TOOLKIT"
if nvidia-container-cli --version &>/dev/null; then
    print_status "✅ NVIDIA Container Toolkit available"
    nvidia-container-cli --version
else
    print_warning "❌ NVIDIA Container Toolkit not found"
fi

# 11. Ollama Status
print_header "OLLAMA STATUS"
if command -v ollama &> /dev/null; then
    print_status "✅ Ollama binary found"
    echo "Location: $(which ollama)"
    
    # Check service status
    if systemctl is-active --quiet ollama; then
        print_status "✅ Ollama service running"
    else
        print_warning "❌ Ollama service not running"
        echo "Service status: $(systemctl is-active ollama)"
    fi
    
    # Check API accessibility
    if curl -s http://localhost:11434/api/version >/dev/null; then
        print_status "✅ Ollama API responding"
        curl -s http://localhost:11434/api/version | head -1
    else
        print_warning "❌ Ollama API not responding"
    fi
    
    # Check models
    echo "Available models:"
    ollama list 2>/dev/null || echo "No models or permission issue"
    
else
    print_error "❌ Ollama not installed"
fi

# 12. Firewall Status
print_header "FIREWALL STATUS"
if command -v ufw &> /dev/null; then
    echo "UFW Status: $(sudo ufw status | head -1)"
    echo "Open ports:"
    sudo ufw status numbered | grep ALLOW || echo "No explicit rules"
else
    print_warning "❌ UFW not available"
fi

# 13. Network Services
print_header "NETWORK SERVICES"
services=("ssh:22" "ollama:11434" "webui:8080")
for service in "${services[@]}"; do
    name=$(echo $service | cut -d: -f1)
    port=$(echo $service | cut -d: -f2)
    if netstat -tuln 2>/dev/null | grep -q ":$port "; then
        print_status "✅ $name listening on port $port"
    else
        print_warning "❌ $name not listening on port $port"
    fi
done

# 14. Disk Space
print_header "DISK SPACE"
df -h / | tail -1
echo "Available space in /tmp: $(df -h /tmp | tail -1 | awk '{print $4}')"

# 15. Memory Usage
print_header "MEMORY USAGE"
free -h

# 16. Recent Errors
print_header "RECENT SYSTEM ERRORS"
echo "Last 5 system errors:"
journalctl --since "1 hour ago" -p err -n 5 --no-pager 2>/dev/null || echo "Cannot access journal"

# 17. Last Script Execution
print_header "LAST SCRIPT ISSUES"
if [ -f /tmp/setup_script.log ]; then
    echo "Last setup script log:"
    tail -20 /tmp/setup_script.log
else
    echo "No previous script log found"
fi

print_header "DIAGNOSIS COMPLETE"
echo "Current time: $(date)"
echo "For detailed logs run: journalctl -xe | tail -50"
echo "To check Ollama specifically: sudo journalctl -u ollama -f" 