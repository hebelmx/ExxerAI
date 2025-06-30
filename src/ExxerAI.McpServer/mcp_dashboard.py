#!/usr/bin/env python3
"""
MCP Server Dashboard
A simple web interface for monitoring and managing MCP servers.
"""

from flask import Flask, render_template, jsonify, request
import requests
import json
import subprocess
import platform
import psutil
import threading
import time
from datetime import datetime
from pathlib import Path

app = Flask(__name__)

class MCPDashboard:
    def __init__(self):
        self.servers = {}
        self.discovery_ports = [3000, 3001, 3002, 8080, 8000, 9000]
        self.last_discovery = None
        
    def discover_servers(self):
        """Discover active MCP servers"""
        discovered = {}
        endpoints = ["/mcp/servers", "/mcp/tools", "/mcp/status", "/"]
        
        for port in self.discovery_ports:
            for endpoint in endpoints:
                url = f"http://localhost:{port}{endpoint}"
                try:
                    response = requests.get(url, timeout=2)
                    if response.status_code == 200:
                        server_info = {
                            "url": f"http://localhost:{port}",
                            "endpoint": endpoint,
                            "status": "active",
                            "last_seen": datetime.now(),
                            "response_time": response.elapsed.total_seconds()
                        }
                        
                        # Try to get server info
                        try:
                            if endpoint in ["/mcp/servers", "/mcp/tools"]:
                                server_info["data"] = response.json()
                        except:
                            server_info["data"] = {"raw": response.text[:200]}
                        
                        discovered[f"localhost:{port}"] = server_info
                        break
                        
                except requests.RequestException:
                    continue
        
        self.servers = discovered
        self.last_discovery = datetime.now()
        return discovered
    
    def get_system_health(self):
        """Get system health information"""
        try:
            return {
                "cpu_percent": psutil.cpu_percent(interval=1),
                "memory_percent": psutil.virtual_memory().percent,
                "disk_percent": psutil.disk_usage('/').percent if platform.system() != 'Windows' 
                               else psutil.disk_usage('C:').percent,
                "uptime": datetime.now() - datetime.fromtimestamp(psutil.boot_time()),
                "python_version": platform.python_version(),
                "platform": platform.platform()
            }
        except Exception as e:
            return {"error": str(e)}
    
    def get_mcp_processes(self):
        """Find running MCP-related processes"""
        mcp_processes = []
        for proc in psutil.process_iter(['pid', 'name', 'cmdline', 'cpu_percent', 'memory_percent']):
            try:
                cmdline = ' '.join(proc.info['cmdline'] or [])
                if any(keyword in cmdline.lower() for keyword in ['mcp', 'simple_mcp', 'http_mcp']):
                    mcp_processes.append({
                        "pid": proc.info['pid'],
                        "name": proc.info['name'],
                        "cmdline": cmdline[:100] + "..." if len(cmdline) > 100 else cmdline,
                        "cpu_percent": proc.info['cpu_percent'] or 0,
                        "memory_percent": proc.info['memory_percent'] or 0
                    })
            except (psutil.NoSuchProcess, psutil.AccessDenied):
                continue
        return mcp_processes

dashboard = MCPDashboard()

@app.route('/')
def index():
    """Main dashboard page"""
    return render_template('dashboard.html')

@app.route('/api/discover')
def api_discover():
    """API endpoint to discover servers"""
    servers = dashboard.discover_servers()
    return jsonify({
        "servers": servers,
        "last_discovery": dashboard.last_discovery.isoformat() if dashboard.last_discovery else None,
        "total_found": len(servers)
    })

@app.route('/api/health')
def api_health():
    """API endpoint for system health"""
    return jsonify(dashboard.get_system_health())

@app.route('/api/processes')
def api_processes():
    """API endpoint for MCP processes"""
    return jsonify(dashboard.get_mcp_processes())

@app.route('/api/servers')
def api_servers():
    """API endpoint to get current servers"""
    return jsonify({
        "servers": dashboard.servers,
        "last_discovery": dashboard.last_discovery.isoformat() if dashboard.last_discovery else None
    })

@app.route('/api/server/<server_id>/tools')
def api_server_tools(server_id):
    """Get tools from a specific server"""
    if server_id in dashboard.servers:
        server_url = dashboard.servers[server_id]["url"]
        try:
            response = requests.get(f"{server_url}/mcp/tools", timeout=5)
            return jsonify(response.json())
        except Exception as e:
            return jsonify({"error": str(e)}), 500
    return jsonify({"error": "Server not found"}), 404

@app.route('/api/server/<server_id>/call', methods=['POST'])
def api_server_call(server_id):
    """Call a tool on a specific server"""
    if server_id in dashboard.servers:
        server_url = dashboard.servers[server_id]["url"]
        try:
            tool_data = request.json
            response = requests.post(f"{server_url}/mcp/call", json=tool_data, timeout=10)
            return jsonify(response.json())
        except Exception as e:
            return jsonify({"error": str(e)}), 500
    return jsonify({"error": "Server not found"}), 404

@app.route('/api/start-server/<server_type>')
def api_start_server(server_type):
    """Start a specific MCP server"""
    server_files = {
        "simple": "simple_mcp_server.py",
        "http": "http_mcp_server.py",
        "standalone": "simple_http_mcp_server.py"
    }
    
    if server_type not in server_files:
        return jsonify({"error": "Unknown server type"}), 400
    
    try:
        script_path = Path(__file__).parent / server_files[server_type]
        if platform.system() == "Windows":
            subprocess.Popen(["python", str(script_path)], creationflags=subprocess.CREATE_NEW_CONSOLE)
        else:
            subprocess.Popen(["python", str(script_path)])
        
        return jsonify({"message": f"Started {server_type} server", "script": server_files[server_type]})
    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    print("🚀 MCP Dashboard starting on http://localhost:5000")
    print("📊 Features: Server Discovery, Health Monitoring, Tool Management")
    app.run(debug=True, host='0.0.0.0', port=5000)
