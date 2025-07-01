#!/bin/bash

# ExxerAI Ubuntu Agent Network Setup Script (Clean Version)
# Run this script on your Ubuntu machine to prepare for agent networking
# NVIDIA Container Toolkit installation skipped (already working)

set -e  # Exit on any error

echo "🚀 Setting up ExxerAI Agent Network on Ubuntu..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 1. Setup claude user and SSH access
print_status "Setting up claude user account..."

# Create claude user if it doesn't exist
if ! id "claude" &>/dev/null; then
    sudo useradd -m -s /bin/bash claude
    print_status "Created claude user account"
else
    print_status "Claude user already exists"
fi

# Setup SSH directory and permissions
sudo mkdir -p /home/claude/.ssh
sudo chmod 700 /home/claude/.ssh

# Add the SSH public key
echo "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAACAQDKxMyK/TplqhxlLMB9yHX/4S7MmT8b/EvaH+PJamYFX3wCRhrMZ0XlyaR1uCyjj7heAYf31tUQTzZbBdcCmUVrS1JE9IJxFXNCX5crwoJVGPs0X0qMnkalw8dhFxPDky54e/dbM+/outBNfMZcWL/awK+ofDo9jNsKFacRhapgW1LIvRPppNXUGqDzEbXYkOewkWyFU51vxCQPu6EmB+BKuvMJXaj25NKn2i7LL18vpI6Oui9h/Tu0PoJ1nLRgNZv4VQXbWWneVpFbAKYloCcRuCmrMUMg8CZN5uinHMO1VeJAiIJyXpxkKXdm1bIwFdhAdB+Umw+TbB54XUIWTLneqFPue94GcNVXIED7Ur6M9J9xTkfjnjlv5GlZqw9g7ZR/pd7fV6oJoZtCcLnw69o0bkmomnS6WuKhJHnNLofj8V6ns66WMWl+2SXjEtGAC6BbKfVusMX23Jvh4bQOp2khzvr4hcBZ6e4vZEkFXazeDnh+4lbg7MmDjYkK950nW1tZsirtk/2S/Xly+6JbRjPW3IpWit9KbRv8K9A7QlcwixAtj88DvQ0zl6zn2mZjra8WJHHKPMNb+2D+JNzeUHZ1glgDM/l25kw6s8m7ute1GoF7XktdwdQkR7n52RKyS56rtuNarTA2rOsw+eVg7P7VJzuQzugfqe+oJb/N156bFw== claude" | sudo tee /home/claude/.ssh/authorized_keys > /dev/null

sudo chmod 600 /home/claude/.ssh/authorized_keys
sudo chown -R claude:claude /home/claude/.ssh

print_status "SSH access configured for claude user"

# 2. Grant sudo access to claude user
echo "claude ALL=(ALL) NOPASSWD:ALL" | sudo tee /etc/sudoers.d/claude > /dev/null
print_status "Granted sudo access to claude user"

# 3. Update system and install dependencies (skip upgrade to avoid kernel issues)
print_status "Installing essential packages..."
sudo apt update
sudo apt install -y curl wget git htop neofetch build-essential python3 python3-pip nodejs npm unzip

# 4. Install .NET SDK for Linux (safer method)
print_status "Installing .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    # Download and install .NET SDK manually to avoid repository issues
    wget -q https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet
    sudo ln -sf /usr/share/dotnet/dotnet /usr/local/bin/dotnet
    rm dotnet-install.sh
    print_status ".NET SDK installed manually"
else
    print_status ".NET SDK already available"
fi

# 5. Skip NVIDIA Container Toolkit (already working)
print_status "Skipping NVIDIA Container Toolkit installation (already configured)"

# 6. Install and configure Ollama
print_status "Installing Ollama..."
if ! command -v ollama &> /dev/null; then
    curl -fsSL https://ollama.ai/install.sh | sh
    print_status "Ollama installed"
else
    print_status "Ollama already installed"
fi

# Start Ollama service
sudo systemctl enable ollama || print_warning "Could not enable ollama service"
sudo systemctl start ollama || print_warning "Could not start ollama service"

print_status "Ollama service configured"

# 7. Configure firewall
print_status "Configuring firewall..."
sudo ufw allow ssh
sudo ufw allow 22      # SSH
sudo ufw allow 11434   # Ollama API
sudo ufw allow 8080    # Web services
sudo ufw allow 3000    # Development servers
sudo ufw allow 5000    # Additional web services
sudo ufw allow 5032    # ExxerAI API port

# Enable firewall if not already enabled
sudo ufw --force enable || print_warning "Could not enable firewall"

print_status "Firewall configured"

# 8. Create workspace directory for ExxerAI
print_status "Setting up ExxerAI workspace..."
sudo mkdir -p /opt/exxerai
sudo chown claude:claude /opt/exxerai
sudo -u claude mkdir -p /home/claude/exxerai-workspace

# 9. Start useful containers with Podman
print_status "Starting agent infrastructure containers..."

# Check if podman is available
if command -v podman &> /dev/null; then
    # Ollama WebUI (Open WebUI)
    print_status "Pulling Open WebUI container..."
    sudo -u claude podman pull ghcr.io/open-webui/open-webui:main || print_warning "Could not pull Open WebUI image"
    
    # Stop existing container if running
    sudo -u claude podman stop open-webui 2>/dev/null || true
    sudo -u claude podman rm open-webui 2>/dev/null || true
    
    # Start new container
    sudo -u claude podman run -d --name open-webui \
        -p 8080:8080 \
        --add-host=host.containers.internal:host-gateway \
        -v open-webui:/app/backend/data \
        --restart unless-stopped \
        ghcr.io/open-webui/open-webui:main || print_warning "Could not start Open WebUI container"
    
    print_status "Open WebUI container started (or attempted)"
else
    print_warning "Podman not available, skipping container setup"
fi

# 10. Download some basic Ollama models
print_status "Downloading basic Ollama models..."
sudo -u claude ollama pull llama3.2:3b || print_warning "Could not download llama3.2:3b model"
sudo -u claude ollama pull qwen2.5:3b || print_warning "Could not download qwen2.5:3b model"

# 11. Create basic agent scripts directory
print_status "Setting up agent scripts directory..."
sudo -u claude mkdir -p /home/claude/agent-scripts
sudo -u claude cat > /home/claude/agent-scripts/test-gpu.sh << 'EOF'
#!/bin/bash
echo "=== GPU Status ==="
nvidia-smi 2>/dev/null || echo "nvidia-smi not available"

echo "=== Container GPU Access ==="
podman run --rm --device nvidia.com/gpu=all nvidia/cuda:12.0-base-ubuntu20.04 nvidia-smi 2>/dev/null || echo "GPU container test failed"

echo "=== Ollama Status ==="
systemctl status ollama

echo "=== Ollama Models ==="
ollama list
EOF

sudo -u claude chmod +x /home/claude/agent-scripts/test-gpu.sh

# 12. Display system information
print_status "System setup complete! Here's your configuration:"

echo -e "\n${BLUE}=== SYSTEM INFO ===${NC}"
neofetch --stdout 2>/dev/null || echo "System: $(uname -a)"

echo -e "\n${BLUE}=== GPU INFO ===${NC}"
if command -v nvidia-smi &> /dev/null; then
    nvidia-smi --query-gpu=index,name,memory.total,memory.used --format=csv,noheader,nounits || echo "GPU query failed"
else
    echo "nvidia-smi not available"
fi

echo -e "\n${BLUE}=== NETWORK INFO ===${NC}"
IP_ADDRESS=$(hostname -I | awk '{print $1}')
echo "SSH Access: ssh claude@${IP_ADDRESS}"
echo "Ollama API: http://${IP_ADDRESS}:11434"
echo "Open WebUI: http://${IP_ADDRESS}:8080"

echo -e "\n${BLUE}=== SERVICES STATUS ===${NC}"
sudo systemctl is-active --quiet ollama && echo "✅ Ollama: Running" || echo "❌ Ollama: Not running"

if command -v podman &> /dev/null; then
    echo -e "\n${BLUE}=== CONTAINERS ===${NC}"
    sudo -u claude podman ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>/dev/null || echo "No containers running"
fi

echo -e "\n${BLUE}=== OLLAMA MODELS ===${NC}"
sudo -u claude ollama list 2>/dev/null || echo "No models available"

echo -e "\n${GREEN}🎉 Ubuntu Agent Network setup complete!${NC}"
echo -e "${GREEN}Ready for remote agent management and multi-machine orchestration!${NC}"

print_status "You can now SSH to this machine as: ssh claude@${IP_ADDRESS}"
print_status "Test GPU setup with: /home/claude/agent-scripts/test-gpu.sh"

echo -e "\n${YELLOW}=== NEXT STEPS ===${NC}"
echo "1. Test SSH connection from Windows"
echo "2. Start agent development and deployment"
echo "3. Configure agent-to-agent communication"
echo "4. Deploy ExxerAI API to Ubuntu"

print_status "Setup script completed successfully! 🚀" 