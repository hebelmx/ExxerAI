#!/usr/bin/env python3
"""
Simple MCP Server Example
A basic Model Context Protocol server for testing and learning.
"""

import json
import sys
import asyncio
from typing import Any, Dict, List
import datetime
import os
import platform

class SimpleMCPServer:
    def __init__(self):
        self.tools = {
            "get_system_info": {
                "description": "Get system information",
                "parameters": {
                    "type": "object",
                    "properties": {},
                    "required": []
                }
            },
            "get_current_time": {
                "description": "Get current date and time",
                "parameters": {
                    "type": "object",
                    "properties": {
                        "format": {
                            "type": "string",
                            "description": "Time format (iso, readable, timestamp)",
                            "default": "readable"
                        }
                    },
                    "required": []
                }
            },
            "list_files": {
                "description": "List files in a directory",
                "parameters": {
                    "type": "object",
                    "properties": {
                        "path": {
                            "type": "string",
                            "description": "Directory path to list",
                            "default": "."
                        }
                    },
                    "required": []
                }
            },
            "calculate": {
                "description": "Perform basic mathematical calculations",
                "parameters": {
                    "type": "object",
                    "properties": {
                        "expression": {
                            "type": "string",
                            "description": "Mathematical expression to evaluate"
                        }
                    },
                    "required": ["expression"]
                }
            }
        }
    
    async def handle_request(self, request: Dict[str, Any]) -> Dict[str, Any]:
        """Handle incoming MCP requests"""
        method = request.get("method")
        params = request.get("params", {})
        
        if method == "initialize":
            return {
                "capabilities": {
                    "tools": {}
                },
                "serverInfo": {
                    "name": "simple-mcp-server",
                    "version": "1.0.0"
                }
            }
        
        elif method == "tools/list":
            return {
                "tools": [
                    {
                        "name": name,
                        "description": tool["description"],
                        "inputSchema": tool["parameters"]
                    }
                    for name, tool in self.tools.items()
                ]
            }
        
        elif method == "tools/call":
            tool_name = params.get("name")
            arguments = params.get("arguments", {})
            
            if tool_name == "get_system_info":
                return await self.get_system_info()
            
            elif tool_name == "get_current_time":
                return await self.get_current_time(arguments.get("format", "readable"))
            
            elif tool_name == "list_files":
                return await self.list_files(arguments.get("path", "."))
            
            elif tool_name == "calculate":
                return await self.calculate(arguments.get("expression"))
            
            else:
                return {
                    "error": f"Unknown tool: {tool_name}"
                }
        
        else:
            return {
                "error": f"Unknown method: {method}"
            }
    
    async def get_system_info(self) -> Dict[str, Any]:
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
                        "current_directory": os.getcwd()
                    }, indent=2)
                }
            ]
        }
    
    async def get_current_time(self, format_type: str = "readable") -> Dict[str, Any]:
        """Get current time in specified format"""
        now = datetime.datetime.now()
        
        if format_type == "iso":
            time_str = now.isoformat()
        elif format_type == "timestamp":
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
    
    async def list_files(self, path: str = ".") -> Dict[str, Any]:
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
                        "text": f"Files in '{path}':\n" + "\n".join(files[:20]) + 
                               (f"\n... and {len(files) - 20} more" if len(files) > 20 else "")
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
    
    async def calculate(self, expression: str) -> Dict[str, Any]:
        """Safely evaluate mathematical expressions"""
        try:
            # Simple whitelist approach for safety
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

async def main():
    """Main server loop"""
    server = SimpleMCPServer()
    
    print("🚀 Simple MCP Server starting...", file=sys.stderr)
    print("📡 Listening for MCP requests on stdin/stdout", file=sys.stderr)
    
    while True:
        try:
            # Read JSON-RPC request from stdin
            line = sys.stdin.readline()
            if not line:
                break
            
            request = json.loads(line.strip())
            response = await server.handle_request(request)
            
            # Send JSON-RPC response to stdout
            print(json.dumps(response))
            sys.stdout.flush()
        
        except (json.JSONDecodeError, KeyboardInterrupt):
            break
        except Exception as e:
            error_response = {
                "error": f"Server error: {str(e)}"
            }
            print(json.dumps(error_response))
            sys.stdout.flush()

if __name__ == "__main__":
    asyncio.run(main())
