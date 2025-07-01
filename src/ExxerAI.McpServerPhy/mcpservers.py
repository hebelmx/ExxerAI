import requests
import json
import time
from typing import Dict, List, Optional

class MCPDiscovery:
    def __init__(self):
        self.common_ports = [3000, 3001, 3002, 8080, 8000, 9000]
        self.common_endpoints = [
            "/mcp/servers",
            "/api/mcp/servers", 
            "/mcp/status",
            "/servers",
            "/mcp"
        ]
    
    def check_endpoint(self, url: str, timeout: int = 5) -> Optional[Dict]:
        """Check if an MCP endpoint is available"""
        try:
            response = requests.get(url, timeout=timeout)
            response.raise_for_status()
            return response.json()
        except requests.RequestException:
            return None
        except json.JSONDecodeError:
            return {"raw_response": response.text[:200]}
    
    def discover_mcp_servers(self) -> Dict[str, Dict]:
        """Discover available MCP servers on common ports and endpoints"""
        print("🔍 Discovering MCP servers...")
        found_servers = {}
        
        for port in self.common_ports:
            for endpoint in self.common_endpoints:
                url = f"http://localhost:{port}{endpoint}"
                print(f"   Checking: {url}")
                
                result = self.check_endpoint(url)
                if result:
                    found_servers[url] = result
                    print(f"   ✅ Found MCP endpoint at {url}")
                
                time.sleep(0.1)  # Be nice to the system
        
        return found_servers
    
    def display_servers(self, servers: Dict[str, Dict]):
        """Display discovered MCP servers in a nice format"""
        if not servers:
            print("\n❌ No MCP servers found on common ports")
            self.show_setup_help()
            return
        
        print(f"\n🎉 Found {len(servers)} MCP endpoint(s):")
        
        for url, data in servers.items():
            print(f"\n🔹 Endpoint: {url}")
            
            if isinstance(data, dict) and "raw_response" in data:
                print(f"   Raw response: {data['raw_response']}")
                continue
            
            # Handle different response formats
            if isinstance(data, dict):
                # Check for servers in response
                if "servers" in data or any(key for key in data.keys() if isinstance(data[key], dict)):
                    servers_data = data.get("servers", data)
                    for server_id, details in servers_data.items():
                        if isinstance(details, dict):
                            tools = details.get("tools", details.get("capabilities", []))
                            print(f"   � Server: {server_id}")
                            print(f"      Tools: {len(tools) if isinstance(tools, list) else 'N/A'}")
                            if isinstance(tools, list) and tools:
                                for tool in tools[:5]:  # Show first 5 tools
                                    print(f"         - {tool}")
                                if len(tools) > 5:
                                    print(f"         ... and {len(tools) - 5} more")
                else:
                    # Generic dict response
                    print(f"   Data keys: {list(data.keys())}")
            else:
                print(f"   Response type: {type(data)}")
    
    def show_setup_help(self):
        """Show help for setting up MCP servers"""
        print("\n📚 MCP Server Setup Guide:")
        print("=" * 50)
        print("To get started with MCP servers, you can:")
        print("\n1. Install Node.js MCP servers:")
        print("   npm install -g @modelcontextprotocol/server-filesystem")
        print("   npm install -g @modelcontextprotocol/server-github")
        print("   npm install -g @modelcontextprotocol/server-sqlite")
        
        print("\n2. Run a simple MCP server:")
        print("   See 'simple_mcp_server.py' (I'll create this for you)")
        
        print("\n3. Popular MCP servers to try:")
        print("   - GitHub MCP Server (for GitHub API access)")
        print("   - Filesystem MCP Server (for file operations)")
        print("   - SQLite MCP Server (for database operations)")
        print("   - Brave Search MCP Server (for web search)")
        
        print(f"\n4. Check the setup scripts I'll create in this directory")

def main():
    """Main function to discover and display MCP servers"""
    discovery = MCPDiscovery()
    servers = discovery.discover_mcp_servers()
    discovery.display_servers(servers)

if __name__ == "__main__":
    main()
