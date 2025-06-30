#!/bin/bash

# ExxerAI Ubuntu Agent Network Setup Script
# Run this script on your Ubuntu machine to prepare for agent networking

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

# 3. Update system and install dependencies
print_status "Updating system packages..."
sudo apt update && sudo apt upgrade -y

print_status "Installing essential packages..."
sudo apt install -y curl wget git htop neofetch build-essential python3 python3-pip nodejs npm

# 4. Install .NET SDK for Linux
print_status "Installing .NET SDK..."
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-8.0

# 5. Setup NVIDIA drivers and container toolkit (if GPUs present)
if command -v nvidia-smi &> /dev/null; then
    print_status "NVIDIA GPU detected, setting up container toolkit..."
    
    # Install NVIDIA Container Toolkit
    distribution=$(. /etc/os-release;echo $ID$VERSION_ID)
    curl -fsSL https://nvidia.github.io/libnvidia-container/gpgkey | sudo gpg --dearmor -o /usr/share/keyrings/nvidia-container-toolkit-keyring.gpg
    curl -s -L https://nvidia.github.io/libnvidia-container/$distribution/libnvidia-container.list | \
        sed 's#deb https://#deb [signed-by=/usr/share/keyrings/nvidia-container-toolkit-keyring.gpg] https://#g' | \
        sudo tee /etc/apt/sources.list.d/nvidia-container-toolkit.list
    
    sudo apt update
    sudo apt install -y nvidia-container-toolkit
    
    # Configure Podman for GPU support
    sudo nvidia-ctk runtime configure --runtime=crun --config=$HOME/.config/containers/containers.conf
    print_status "NVIDIA GPU support configured"
else
    print_warning "No NVIDIA GPU detected, skipping GPU setup"
fi

# 6. Install and configure Ollama
print_status "Installing Ollama..."
curl -fsSL https://ollama.ai/install.sh | sh

# Start Ollama service
sudo systemctl enable ollama
sudo systemctl start ollama

print_status "Ollama service started"

# 7. Configure firewall
print_status "Configuring firewall..."
sudo ufw allow ssh
sudo ufw allow 11434  # Ollama API
sudo ufw allow 8080   # Web services
sudo ufw allow 3000   # Development servers
sudo ufw allow 5000   # Additional web services
sudo ufw allow 5032   # ExxerAI API port

# Enable firewall if not already enabled
sudo ufw --force enable

print_status "Firewall configured"

# 8. Create workspace directory for ExxerAI
print_status "Setting up ExxerAI workspace..."
sudo mkdir -p /opt/exxerai
sudo chown claude:claude /opt/exxerai
sudo -u claude mkdir -p /home/claude/exxerai-workspace

# 9. Start some useful containers with Podman
print_status "Starting agent infrastructure containers..."

# Ollama WebUI (Open WebUI)
sudo -u claude podman pull ghcr.io/open-webui/open-webui:main
sudo -u claude podman run -d --name open-webui \
    -p 8080:8080 \
    --add-host=host.containers.internal:host-gateway \
    -v open-webui:/app/backend/data \
    --restart unless-stopped \
    ghcr.io/open-webui/open-webui:main || print_warning "Open WebUI container already running or failed to start"

print_status "Open WebUI started on port 8080"

# 10. Display system information
print_status "System setup complete! Here's your configuration:"

echo -e "\n${BLUE}=== SYSTEM INFO ===${NC}"
neofetch --stdout 2>/dev/null || echo "System: $(uname -a)"

echo -e "\n${BLUE}=== GPU INFO ===${NC}"
if command -v nvidia-smi &> /dev/null; then
    nvidia-smi --query-gpu=index,name,memory.total,memory.used --format=csv,noheader,nounits
else
    echo "No NVIDIA GPUs detected"
fi

echo -e "\n${BLUE}=== NETWORK INFO ===${NC}"
echo "SSH Access: ssh claude@$(hostname -I | awk '{print $1}')"
echo "Ollama API: http://$(hostname -I | awk '{print $1}'):11434"
echo "Open WebUI: http://$(hostname -I | awk '{print $1}'):8080"

echo -e "\n${BLUE}=== SERVICES STATUS ===${NC}"
sudo systemctl is-active --quiet ollama && echo "✅ Ollama: Running" || echo "❌ Ollama: Not running"
sudo -u claude podman ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>/dev/null || echo "No containers running"

echo -e "\n${GREEN}🎉 Ubuntu Agent Network setup complete!${NC}"
echo -e "${GREEN}Ready for remote agent management and multi-machine orchestration!${NC}"

print_status "You can now SSH to this machine as: ssh claude@$(hostname -I | awk '{print $1}')" 