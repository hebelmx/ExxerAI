# 🚀 MCP Server Setup and Discovery Kit

This directory contains everything you need to set up, run, and discover Model Context Protocol (MCP) servers on your machine.

## 📁 Files Overview

| File | Description |
|------|-------------|
| `mcpservers.py` | 🔍 Enhanced MCP server discovery tool |
| `simple_mcp_server.py` | 🐍 Basic Python MCP server implementation |
| `http_mcp_server.py` | 🌐 HTTP wrapper for the MCP server |
| `setup_mcp_servers.py` | ⚙️ Automated setup for popular MCP servers |
| `requirements.txt` | 📦 Python dependencies |
| `start_mcp_servers.ps1` | 🚀 Windows PowerShell startup script |
| `start_mcp_servers.sh` | 🚀 Unix/Linux startup script |

## 🎯 Quick Start

### 1. Test MCP Discovery (Works Now!)
```bash
python mcpservers.py
```
This will scan common ports for MCP servers and show setup help if none are found.

### 2. Start a Simple MCP Server
```bash
python http_mcp_server.py
```
This starts an HTTP MCP server on `http://localhost:3000` with these tools:
- 📊 System information
- 🕐 Current time
- 📁 File listing
- 🧮 Basic calculator

### 3. Test the Discovery Again
```bash
python mcpservers.py
```
Now you should see your running MCP server!

### 4. Test the Server Directly
Visit `http://localhost:3000` in your browser or use curl:
```bash
curl http://localhost:3000/mcp/servers
curl http://localhost:3000/mcp/tools
curl "http://localhost:3000/mcp/calculate?expr=10*5+2"
```

## 🔧 Advanced Setup

### Install Popular MCP Servers
```bash
python setup_mcp_servers.py
```
This will:
- ✅ Check for Node.js
- 📦 Install popular MCP servers (GitHub, filesystem, SQLite, etc.)
- 📝 Create configuration files
- 🚀 Generate startup scripts

### Available MCP Servers to Install
- **GitHub MCP Server** - GitHub API access
- **Filesystem MCP Server** - File operations
- **SQLite MCP Server** - Database operations  
- **Brave Search MCP Server** - Web search
- **PostgreSQL MCP Server** - PostgreSQL database

## 🛠️ MCP Server Tools

### Simple MCP Server Tools
| Tool | Description | Example |
|------|-------------|---------|
| `get_system_info` | Get system information | System details, Python version |
| `get_current_time` | Get current time | ISO, readable, or timestamp format |
| `list_files` | List directory contents | Files and folders with sizes |
| `calculate` | Basic math calculations | `2+2`, `10*5+3`, etc. |

### HTTP Endpoints
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/` | GET | API documentation |
| `/mcp/servers` | GET | List MCP servers |
| `/mcp/tools` | GET | List available tools |
| `/mcp/call` | POST | Call a tool |
| `/mcp/system-info` | GET | Get system info |
| `/mcp/time?format=iso` | GET | Get current time |
| `/mcp/files?path=.` | GET | List files |
| `/mcp/calculate?expr=2+2` | GET | Calculate expression |

## 📖 Usage Examples

### Discover MCP Servers
```python
from mcpservers import MCPDiscovery

discovery = MCPDiscovery()
servers = discovery.discover_mcp_servers()
discovery.display_servers(servers)
```

### Call MCP Tools via HTTP
```python
import requests

# Get system info
response = requests.get('http://localhost:3000/mcp/system-info')
print(response.json())

# Calculate expression
response = requests.get('http://localhost:3000/mcp/calculate?expr=sqrt(16)')
print(response.json())

# List files
response = requests.get('http://localhost:3000/mcp/files?path=.')
print(response.json())
```

### Call MCP Tools via POST
```python
import requests

data = {
    "tool": "get_current_time",
    "arguments": {"format": "iso"}
}

response = requests.post('http://localhost:3000/mcp/call', json=data)
print(response.json())
```

## 🚀 Starting Multiple Servers

### Windows (PowerShell)
```powershell
.\start_mcp_servers.ps1
```

### Unix/Linux/Mac
```bash
./start_mcp_servers.sh
```

## 🔍 Troubleshooting

### No MCP Servers Found
1. Make sure you have Node.js installed: `node --version`
2. Run the setup script: `python setup_mcp_servers.py`
3. Start the simple server: `python http_mcp_server.py`
4. Check the discovery again: `python mcpservers.py`

### Port 3000 Already in Use
Edit `http_mcp_server.py` and change the port:
```python
app.run(host='localhost', port=3001, debug=True)
```

### Missing Dependencies
```bash
pip install -r requirements.txt
```

## 🔐 Security Notes

- The simple calculator only allows basic math operations
- File listing is restricted to readable directories
- The HTTP server runs on localhost only
- Consider authentication for production use

## 📚 Learn More

- [Model Context Protocol Documentation](https://modelcontextprotocol.io/)
- [MCP Python SDK](https://github.com/modelcontextprotocol/python-sdk)
- [Official MCP Servers](https://github.com/modelcontextprotocol)

## 🤝 Contributing

Feel free to extend the simple MCP server with more tools:
1. Add new tool definitions to `simple_mcp_server.py`
2. Implement the tool logic
3. Add HTTP endpoints in `http_mcp_server.py`
4. Update this README

## 📝 Configuration

### MCP Configuration File
Location: `~/.mcp/servers.json`

```json
{
  "mcpServers": {
    "filesystem": {
      "command": "npx",
      "args": ["@modelcontextprotocol/server-filesystem", "."],
      "env": {}
    },
    "github": {
      "command": "npx", 
      "args": ["@modelcontextprotocol/server-github"],
      "env": {
        "GITHUB_PERSONAL_ACCESS_TOKEN": "your_token_here"
      }
    }
  }
}
```

## 🎉 What's Next?

1. **Test the current setup** - Start with the simple server
2. **Install Node.js MCP servers** - Run the setup script
3. **Configure GitHub access** - Add your GitHub token
4. **Build custom tools** - Extend the simple server
5. **Integrate with your projects** - Use MCP in your applications

---

🚀 **Ready to explore MCP? Start with `python mcpservers.py`!**
