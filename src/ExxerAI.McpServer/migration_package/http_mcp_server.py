#!/usr/bin/env python3
"""
HTTP MCP Server Wrapper
Wraps the simple MCP server with an HTTP interface for easy testing.
"""

from flask import Flask, request, jsonify
import asyncio
import json
import sys
import os

# Add the current directory to path to import our MCP server
sys.path.append(os.path.dirname(os.path.abspath(__file__)))
from simple_mcp_server import SimpleMCPServer

app = Flask(__name__)
mcp_server = SimpleMCPServer()

@app.route('/')
def home():
    """Home page with API documentation"""
    return """
    <h1>🚀 Simple MCP Server HTTP Interface</h1>
    <h2>Available Endpoints:</h2>
    <ul>
        <li><strong>GET /mcp/servers</strong> - List available MCP servers</li>
        <li><strong>GET /mcp/tools</strong> - List available tools</li>
        <li><strong>POST /mcp/call</strong> - Call a tool</li>
        <li><strong>GET /mcp/system-info</strong> - Get system information</li>
        <li><strong>GET /mcp/time</strong> - Get current time</li>
        <li><strong>GET /mcp/files?path=.</strong> - List files in directory</li>
        <li><strong>GET /mcp/calculate?expr=2+2</strong> - Calculate expression</li>
    </ul>
    
    <h2>Example Usage:</h2>
    <pre>
    curl http://localhost:3000/mcp/servers
    curl http://localhost:3000/mcp/tools
    curl http://localhost:3000/mcp/system-info
    curl "http://localhost:3000/mcp/calculate?expr=10*5+2"
    </pre>
    """

@app.route('/mcp/servers')
def list_servers():
    """List available MCP servers"""
    return jsonify({
        "simple-mcp-server": {
            "name": "Simple MCP Server",
            "version": "1.0.0",
            "description": "A basic MCP server for testing and learning",
            "tools": list(mcp_server.tools.keys()),
            "status": "running",
            "port": 3000,
            "endpoints": [
                "/mcp/servers",
                "/mcp/tools", 
                "/mcp/call",
                "/mcp/system-info",
                "/mcp/time",
                "/mcp/files",
                "/mcp/calculate"
            ]
        }
    })

@app.route('/mcp/tools')
def list_tools():
    """List available tools"""
    tools = []
    for name, tool in mcp_server.tools.items():
        tools.append({
            "name": name,
            "description": tool["description"],
            "parameters": tool["parameters"]
        })
    return jsonify({"tools": tools})

@app.route('/mcp/call', methods=['POST'])
def call_tool():
    """Call a tool with parameters"""
    data = request.get_json()
    
    if not data or 'tool' not in data:
        return jsonify({"error": "Missing 'tool' in request"}), 400
    
    tool_name = data['tool']
    arguments = data.get('arguments', {})
    
    # Create MCP request format
    mcp_request = {
        "method": "tools/call",
        "params": {
            "name": tool_name,
            "arguments": arguments
        }
    }
    
    # Handle the request using our MCP server
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    try:
        result = loop.run_until_complete(mcp_server.handle_request(mcp_request))
        return jsonify(result)
    finally:
        loop.close()

@app.route('/mcp/system-info')
def get_system_info():
    """Get system information (convenience endpoint)"""
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    try:
        result = loop.run_until_complete(mcp_server.get_system_info())
        return jsonify(result)
    finally:
        loop.close()

@app.route('/mcp/time')
def get_time():
    """Get current time (convenience endpoint)"""
    format_type = request.args.get('format', 'readable')
    
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    try:
        result = loop.run_until_complete(mcp_server.get_current_time(format_type))
        return jsonify(result)
    finally:
        loop.close()

@app.route('/mcp/files')
def list_files():
    """List files (convenience endpoint)"""
    path = request.args.get('path', '.')
    
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    try:
        result = loop.run_until_complete(mcp_server.list_files(path))
        return jsonify(result)
    finally:
        loop.close()

@app.route('/mcp/calculate')
def calculate():
    """Calculate expression (convenience endpoint)"""
    expression = request.args.get('expr')
    
    if not expression:
        return jsonify({"error": "Missing 'expr' parameter"}), 400
    
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    try:
        result = loop.run_until_complete(mcp_server.calculate(expression))
        return jsonify(result)
    finally:
        loop.close()

@app.errorhandler(404)
def not_found(error):
    """Handle 404 errors"""
    return jsonify({
        "error": "Endpoint not found",
        "available_endpoints": [
            "/",
            "/mcp/servers",
            "/mcp/tools",
            "/mcp/call",
            "/mcp/system-info", 
            "/mcp/time",
            "/mcp/files",
            "/mcp/calculate"
        ]
    }), 404

@app.errorhandler(500)
def internal_error(error):
    """Handle 500 errors"""
    return jsonify({
        "error": "Internal server error",
        "message": str(error)
    }), 500

if __name__ == '__main__':
    # Try different ports if 3000 is not available
    ports_to_try = [3000, 3001, 3002, 8000, 8080]
    
    for port in ports_to_try:
        try:
            print(f"🚀 Trying to start HTTP MCP Server on http://localhost:{port}")
            app.run(host='localhost', port=port, debug=False)
            break
        except OSError as e:
            if "Address already in use" in str(e) or "access" in str(e).lower():
                print(f"   ❌ Port {port} is not available, trying next...")
                continue
            else:
                raise e
        except KeyboardInterrupt:
            print(f"\n👋 HTTP MCP Server stopped on port {port}")
            break
    else:
        print("❌ Could not find an available port to start the server")
