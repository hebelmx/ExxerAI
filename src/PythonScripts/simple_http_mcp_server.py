#!/usr/bin/env python3
"""
Simple HTTP MCP Test Server
A minimal HTTP server for testing MCP functionality without Flask dependencies.
"""

import http.server
import socketserver
import json
import urllib.parse
import os
import sys
import platform
import datetime
from pathlib import Path

class MCPHandler(http.server.BaseHTTPRequestHandler):
    def do_GET(self):
        """Handle GET requests"""
        parsed_path = urllib.parse.urlparse(self.path)
        path = parsed_path.path
        query = urllib.parse.parse_qs(parsed_path.query)
        
        try:
            if path == '/':
                self.send_html_response(self.get_home_page())
            elif path == '/mcp/servers':
                self.send_json_response(self.get_servers())
            elif path == '/mcp/tools':
                self.send_json_response(self.get_tools())
            elif path == '/mcp/system-info':
                self.send_json_response(self.get_system_info())
            elif path == '/mcp/time':
                format_type = query.get('format', ['readable'])[0]
                self.send_json_response(self.get_time(format_type))
            elif path == '/mcp/files':
                path_param = query.get('path', ['.'])[0]
                self.send_json_response(self.list_files(path_param))
            elif path == '/mcp/calculate':
                expr = query.get('expr', [None])[0]
                if expr:
                    self.send_json_response(self.calculate(expr))
                else:
                    self.send_json_response({"error": "Missing 'expr' parameter"}, 400)
            else:
                self.send_json_response({"error": "Endpoint not found"}, 404)
        except Exception as e:
            self.send_json_response({"error": str(e)}, 500)
    
    def do_POST(self):
        """Handle POST requests"""
        if self.path == '/mcp/call':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            
            try:
                data = json.loads(post_data.decode('utf-8'))
                tool_name = data.get('tool')
                arguments = data.get('arguments', {})
                
                if tool_name == 'get_system_info':
                    response = self.get_system_info()
                elif tool_name == 'get_current_time':
                    format_type = arguments.get('format', 'readable')
                    response = self.get_time(format_type)
                elif tool_name == 'list_files':
                    path = arguments.get('path', '.')
                    response = self.list_files(path)
                elif tool_name == 'calculate':
                    expr = arguments.get('expression')
                    if expr:
                        response = self.calculate(expr)
                    else:
                        response = {"error": "Missing 'expression' argument"}
                else:
                    response = {"error": f"Unknown tool: {tool_name}"}
                
                self.send_json_response(response)
            except json.JSONDecodeError:
                self.send_json_response({"error": "Invalid JSON"}, 400)
            except Exception as e:
                self.send_json_response({"error": str(e)}, 500)
        else:
            self.send_json_response({"error": "Endpoint not found"}, 404)
    
    def send_json_response(self, data, status_code=200):
        """Send JSON response"""
        response = json.dumps(data, indent=2)
        self.send_response(status_code)
        self.send_header('Content-type', 'application/json')
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Content-Length', str(len(response)))
        self.end_headers()
        self.wfile.write(response.encode('utf-8'))
    
    def send_html_response(self, html):
        """Send HTML response"""
        self.send_response(200)
        self.send_header('Content-type', 'text/html')
        self.send_header('Content-Length', str(len(html)))
        self.end_headers()
        self.wfile.write(html.encode('utf-8'))
    
    def get_home_page(self):
        """Get home page HTML"""
        return '''
<!DOCTYPE html>
<html>
<head>
    <title>🚀 Simple MCP Server</title>
    <style>
        body { font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px; }
        h1 { color: #333; }
        h2 { color: #666; }
        .endpoint { background: #f5f5f5; padding: 10px; margin: 5px 0; border-radius: 5px; }
        .method { background: #007acc; color: white; padding: 2px 8px; border-radius: 3px; font-size: 12px; }
        code { background: #f0f0f0; padding: 2px 4px; border-radius: 3px; }
    </style>
</head>
<body>
    <h1>🚀 Simple MCP Server</h1>
    <p>A lightweight Model Context Protocol server for testing and development.</p>
    
    <h2>Available Endpoints:</h2>
    <div class="endpoint">
        <span class="method">GET</span> <code>/mcp/servers</code> - List available MCP servers
    </div>
    <div class="endpoint">
        <span class="method">GET</span> <code>/mcp/tools</code> - List available tools
    </div>
    <div class="endpoint">
        <span class="method">POST</span> <code>/mcp/call</code> - Call a tool
    </div>
    <div class="endpoint">
        <span class="method">GET</span> <code>/mcp/system-info</code> - Get system information
    </div>
    <div class="endpoint">
        <span class="method">GET</span> <code>/mcp/time?format=iso</code> - Get current time
    </div>
    <div class="endpoint">
        <span class="method">GET</span> <code>/mcp/files?path=.</code> - List files in directory
    </div>
    <div class="endpoint">
        <span class="method">GET</span> <code>/mcp/calculate?expr=2+2</code> - Calculate expression
    </div>
    
    <h2>Quick Test:</h2>
    <ul>
        <li><a href="/mcp/servers">List servers</a></li>
        <li><a href="/mcp/tools">List tools</a></li>
        <li><a href="/mcp/system-info">System info</a></li>
        <li><a href="/mcp/time">Current time</a></li>
        <li><a href="/mcp/files">List files</a></li>
        <li><a href="/mcp/calculate?expr=10*5+2">Calculate: 10*5+2</a></li>
    </ul>
    
    <h2>Example curl commands:</h2>
    <pre>
curl http://localhost:PORT/mcp/servers
curl http://localhost:PORT/mcp/tools
curl http://localhost:PORT/mcp/system-info
curl "http://localhost:PORT/mcp/calculate?expr=sqrt(16)"
    </pre>
</body>
</html>
        '''
    
    def get_servers(self):
        """Get list of MCP servers"""
        return {
            "simple-mcp-server": {
                "name": "Simple MCP Server", 
                "version": "1.0.0",
                "description": "A lightweight MCP server for testing",
                "tools": ["get_system_info", "get_current_time", "list_files", "calculate"],
                "status": "running"
            }
        }
    
    def get_tools(self):
        """Get list of available tools"""
        return {
            "tools": [
                {
                    "name": "get_system_info",
                    "description": "Get system information"
                },
                {
                    "name": "get_current_time", 
                    "description": "Get current date and time"
                },
                {
                    "name": "list_files",
                    "description": "List files in a directory"
                },
                {
                    "name": "calculate",
                    "description": "Perform basic mathematical calculations"
                }
            ]
        }
    
    def get_system_info(self):
        """Get system information"""
        return {
            "content": [
                {
                    "type": "text",
                    "text": json.dumps({
                        "platform": platform.platform(),
                        "system": platform.system(),
                        "machine": platform.machine(),
                        "processor": platform.processor(),
                        "python_version": platform.python_version(),
                        "current_directory": os.getcwd(),
                        "server_type": "Simple HTTP MCP Server"
                    }, indent=2)
                }
            ]
        }
    
    def get_time(self, format_type='readable'):
        """Get current time"""
        now = datetime.datetime.now()
        
        if format_type == 'iso':
            time_str = now.isoformat()
        elif format_type == 'timestamp':
            time_str = str(now.timestamp())
        else:  # readable
            time_str = now.strftime("%Y-%m-%d %H:%M:%S")
        
        return {
            "content": [
                {
                    "type": "text",
                    "text": f"Current time ({format_type}): {time_str}"
                }
            ]
        }
    
    def list_files(self, path='.'):
        """List files in directory"""
        try:
            if not os.path.exists(path):
                return {
                    "content": [
                        {
                            "type": "text", 
                            "text": f"Error: Path '{path}' does not exist"
                        }
                    ]
                }
            
            files = []
            for item in os.listdir(path):
                item_path = os.path.join(path, item)
                if os.path.isfile(item_path):
                    size = os.path.getsize(item_path)
                    files.append(f"📄 {item} ({size} bytes)")
                else:
                    files.append(f"📁 {item}/")
            
            return {
                "content": [
                    {
                        "type": "text",
                        "text": f"Files in '{path}':\\n" + "\\n".join(files[:20]) + 
                               (f"\\n... and {len(files) - 20} more" if len(files) > 20 else "")
                    }
                ]
            }
        except Exception as e:
            return {
                "content": [
                    {
                        "type": "text",
                        "text": f"Error listing files: {str(e)}"
                    }
                ]
            }
    
    def calculate(self, expression):
        """Calculate mathematical expression"""
        try:
            # Simple whitelist for safety
            allowed_chars = set("0123456789+-*/.() ")
            if not all(c in allowed_chars for c in expression):
                return {
                    "content": [
                        {
                            "type": "text",
                            "text": "Error: Expression contains invalid characters"
                        }
                    ]
                }
            
            result = eval(expression)
            return {
                "content": [
                    {
                        "type": "text",
                        "text": f"{expression} = {result}"
                    }
                ]
            }
        except Exception as e:
            return {
                "content": [
                    {
                        "type": "text",
                        "text": f"Error calculating: {str(e)}"
                    }
                ]
            }

def find_available_port(start_port=3000):
    """Find an available port starting from start_port"""
    import socket
    
    for port in range(start_port, start_port + 20):
        try:
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
                s.bind(('localhost', port))
                return port
        except OSError:
            continue
    return None

def main():
    """Main function to start the server"""
    port = find_available_port(3000)
    
    if port is None:
        print("❌ Could not find an available port")
        return
    
    print(f"🚀 Starting Simple HTTP MCP Server on http://localhost:{port}")
    print(f"📖 Visit http://localhost:{port} for API documentation")
    print("🛑 Press Ctrl+C to stop")
    
    try:
        with socketserver.TCPServer(("localhost", port), MCPHandler) as httpd:
            httpd.serve_forever()
    except KeyboardInterrupt:
        print(f"\\n👋 Simple HTTP MCP Server stopped")

if __name__ == "__main__":
    main()
