#!/usr/bin/env python3
"""
MCP Server Setup Script
Automates the installation and configuration of popular MCP servers.
"""

import subprocess
import sys
import os
import json
import platform
from pathlib import Path

class MCPSetupManager:
    def __init__(self):
        self.system = platform.system()
        self.mcp_config_dir = Path.home() / ".mcp"
        self.servers_config = self.mcp_config_dir / "servers.json"
        
    def run_command(self, command: str, description: str) -> bool:
        """Run a command and return success status"""
        print(f"🔧 {description}...")
        try:
            if self.system == "Windows":
                # Use PowerShell for Windows
                result = subprocess.run(
                    ["powershell", "-Command", command],
                    capture_output=True,
                    text=True,
                    check=True
                )
            else:
                result = subprocess.run(
                    command,
                    shell=True,
                    capture_output=True,
                    text=True,
                    check=True
                )
            
            print(f"   ✅ Success: {description}")
            if result.stdout:
                print(f"   Output: {result.stdout.strip()}")
            return True
            
        except subprocess.CalledProcessError as e:
            print(f"   ❌ Failed: {description}")
            print(f"   Error: {e.stderr.strip() if e.stderr else str(e)}")
            return False
    
    def check_nodejs(self) -> bool:
        """Check if Node.js is installed"""
        print("🔍 Checking for Node.js...")
        try:
            result = subprocess.run(
                ["node", "--version"],
                capture_output=True,
                text=True,
                check=True
            )
            version = result.stdout.strip()
            print(f"   ✅ Node.js found: {version}")
            return True
        except (subprocess.CalledProcessError, FileNotFoundError):
            print("   ❌ Node.js not found")
            return False
    
    def install_nodejs_servers(self) -> bool:
        """Install popular Node.js MCP servers"""
        if not self.check_nodejs():
            print("\n📥 Installing Node.js...")
            if self.system == "Windows":
                print("   Please install Node.js from: https://nodejs.org/")
                print("   Or use winget: winget install OpenJS.NodeJS")
                return False
            else:
                # Try to install Node.js on Unix systems
                commands = [
                    "curl -fsSL https://deb.nodesource.com/setup_lts.x | sudo -E bash -",
                    "sudo apt-get install -y nodejs"
                ]
                for cmd in commands:
                    if not self.run_command(cmd, f"Installing Node.js"):
                        return False
        
        print("\n📦 Installing MCP servers...")
        
        # List of popular MCP servers
        servers = [
            ("@modelcontextprotocol/server-filesystem", "File system operations"),
            ("@modelcontextprotocol/server-github", "GitHub API access"),
            ("@modelcontextprotocol/server-sqlite", "SQLite database operations"),
            ("@modelcontextprotocol/server-brave-search", "Web search capabilities"),
            ("@modelcontextprotocol/server-postgres", "PostgreSQL database operations"),
        ]
        
        installed_servers = []
        for package, description in servers:
            cmd = f"npm install -g {package}"
            if self.run_command(cmd, f"Installing {package} ({description})"):
                installed_servers.append(package)
        
        print(f"\n🎉 Successfully installed {len(installed_servers)} MCP servers")
        return len(installed_servers) > 0
    
    def create_config_directory(self):
        """Create MCP configuration directory"""
        self.mcp_config_dir.mkdir(exist_ok=True)
        print(f"📁 Created config directory: {self.mcp_config_dir}")
    
    def create_sample_config(self):
        """Create a sample MCP configuration"""
        config = {
            "mcpServers": {
                "filesystem": {
                    "command": "npx",
                    "args": ["@modelcontextprotocol/server-filesystem", "/path/to/allowed/directory"],
                    "env": {}
                },
                "github": {
                    "command": "npx",
                    "args": ["@modelcontextprotocol/server-github"],
                    "env": {
                        "GITHUB_PERSONAL_ACCESS_TOKEN": "your_github_token_here"
                    }
                },
                "sqlite": {
                    "command": "npx",
                    "args": ["@modelcontextprotocol/server-sqlite", "--db-path", "/path/to/database.db"],
                    "env": {}
                },
                "simple-python-server": {
                    "command": "python",
                    "args": ["simple_mcp_server.py"],
                    "env": {}
                }
            }
        }
        
        with open(self.servers_config, 'w') as f:
            json.dump(config, f, indent=2)
        
        print(f"📝 Created sample config: {self.servers_config}")
        print("   💡 Edit this file to configure your MCP servers")
    
    def create_startup_scripts(self):
        """Create scripts to start MCP servers"""
        
        # PowerShell script for Windows
        ps_script = '''# MCP Server Startup Script
Write-Host "🚀 Starting MCP Servers..." -ForegroundColor Green

# Start filesystem server on port 3000
Write-Host "📁 Starting Filesystem MCP Server on port 3000..." -ForegroundColor Yellow
Start-Process -NoNewWindow -FilePath "npx" -ArgumentList "@modelcontextprotocol/server-filesystem", "."

# Start simple Python server
Write-Host "🐍 Starting Simple Python MCP Server..." -ForegroundColor Yellow
Start-Process -NoNewWindow -FilePath "python" -ArgumentList "simple_mcp_server.py"

Write-Host "✅ MCP Servers started! Check the processes in Task Manager." -ForegroundColor Green
Write-Host "🔍 Run 'python mcpservers.py' to discover active servers." -ForegroundColor Cyan
'''
        
        with open("start_mcp_servers.ps1", 'w') as f:
            f.write(ps_script)
        
        # Bash script for Unix systems
        bash_script = '''#!/bin/bash
# MCP Server Startup Script

echo "🚀 Starting MCP Servers..."

# Start filesystem server in background
echo "📁 Starting Filesystem MCP Server..."
npx @modelcontextprotocol/server-filesystem . &

# Start simple Python server in background  
echo "🐍 Starting Simple Python MCP Server..."
python simple_mcp_server.py &

echo "✅ MCP Servers started in background!"
echo "🔍 Run 'python mcpservers.py' to discover active servers."
echo "🛑 Use 'pkill -f mcp' to stop all MCP servers."
'''
        
        with open("start_mcp_servers.sh", 'w') as f:
            f.write(bash_script)
        
        # Make executable on Unix systems
        if self.system != "Windows":
            os.chmod("start_mcp_servers.sh", 0o755)
        
        print("📜 Created startup scripts:")
        print("   - start_mcp_servers.ps1 (Windows PowerShell)")
        print("   - start_mcp_servers.sh (Unix/Linux/Mac)")
    
    def show_next_steps(self):
        """Show next steps for the user"""
        print("\n" + "="*60)
        print("🎯 NEXT STEPS")
        print("="*60)
        
        print("\n1. 📋 Test the discovery script:")
        print("   python mcpservers.py")
        
        print("\n2. 🚀 Start MCP servers:")
        if self.system == "Windows":
            print("   .\\start_mcp_servers.ps1")
        else:
            print("   ./start_mcp_servers.sh")
        
        print("\n3. 🧪 Test the simple Python server:")
        print("   python simple_mcp_server.py")
        
        print("\n4. ⚙️ Configure your servers:")
        print(f"   Edit: {self.servers_config}")
        
        print("\n5. 🔑 For GitHub server, set your token:")
        print("   Set GITHUB_PERSONAL_ACCESS_TOKEN environment variable")
        
        print("\n6. 📚 Learn more about MCP:")
        print("   https://github.com/modelcontextprotocol/python-sdk")
        print("   https://modelcontextprotocol.io/")

def main():
    """Main setup function"""
    print("🔧 MCP Server Setup Manager")
    print("="*40)
    
    manager = MCPSetupManager()
    
    # Create configuration
    manager.create_config_directory()
    manager.create_sample_config()
    
    # Install Node.js servers
    if input("\n❓ Install Node.js MCP servers? (y/n): ").lower().startswith('y'):
        manager.install_nodejs_servers()
    
    # Create startup scripts
    manager.create_startup_scripts()
    
    # Show next steps
    manager.show_next_steps()

if __name__ == "__main__":
    main()
