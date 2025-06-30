#!/bin/bash

# Fix Ollama Permissions Script
# Run this on Ubuntu machine to fix permission denied errors

set -e

echo "🔧 Fixing Ollama permissions for claude user..."

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Stop ollama service if running
print_status "Stopping Ollama service..."
sudo systemctl stop ollama 2>/dev/null || print_warning "Ollama service not running"

# Add claude user to ollama group (if exists)
if getent group ollama >/dev/null 2>&1; then
    sudo usermod -a -G ollama claude
    print_status "Added claude to ollama group"
else
    print_warning "Ollama group doesn't exist, creating it..."
    sudo groupadd ollama
    sudo usermod -a -G ollama claude
    print_status "Created ollama group and added claude"
fi

# Fix Ollama service configuration to allow multi-user access
print_status "Configuring Ollama service for multi-user access..."

# Create or update ollama systemd service override
sudo mkdir -p /etc/systemd/system/ollama.service.d/

sudo tee /etc/systemd/system/ollama.service.d/override.conf > /dev/null << 'EOF'
[Service]
# Allow all users to access Ollama
Environment="OLLAMA_HOST=0.0.0.0:11434"
# Set proper user and group
User=ollama
Group=ollama
# Create ollama user if needed
ExecStartPre=/bin/bash -c 'id -u ollama &>/dev/null || useradd -r -s /bin/false -d /usr/share/ollama ollama'
EOF

# Create ollama user if it doesn't exist
if ! id ollama &>/dev/null; then
    sudo useradd -r -s /bin/false -d /usr/share/ollama ollama
    print_status "Created ollama system user"
fi

# Set up proper directories and permissions
sudo mkdir -p /usr/share/ollama
sudo mkdir -p /home/claude/.ollama
sudo chown -R ollama:ollama /usr/share/ollama
sudo chown claude:claude /home/claude/.ollama

# Allow claude user to run ollama commands
sudo tee /etc/sudoers.d/ollama > /dev/null << 'EOF'
claude ALL=(ollama) NOPASSWD: /usr/local/bin/ollama
EOF

# Create a wrapper script for claude user
sudo tee /usr/local/bin/ollama-user > /dev/null << 'EOF'
#!/bin/bash
# Wrapper script to run ollama as claude user
export OLLAMA_HOST=http://localhost:11434
exec /usr/local/bin/ollama "$@"
EOF

sudo chmod +x /usr/local/bin/ollama-user

# Reload systemd and restart service
print_status "Reloading systemd and starting Ollama..."
sudo systemctl daemon-reload
sudo systemctl enable ollama
sudo systemctl start ollama

# Wait for service to start
sleep 5

# Test the service
print_status "Testing Ollama service..."
if systemctl is-active --quiet ollama; then
    print_status "✅ Ollama service is running"
else
    print_error "❌ Ollama service failed to start"
    print_error "Check logs with: sudo journalctl -u ollama -f"
fi

# Test API access
if curl -s http://localhost:11434/api/version >/dev/null; then
    print_status "✅ Ollama API is accessible"
else
    print_warning "❌ Ollama API not responding yet, may need more time"
fi

print_status "Ollama permissions fix completed!"
print_status "You can now run: ollama-user pull llama3.2:3b"
print_status "Or continue with the original setup script" 