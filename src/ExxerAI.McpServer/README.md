# MCP Server Collection

This folder contains all Model Context Protocol (MCP) server implementations and related utilities with comprehensive **Test-Driven Development (TDD)** practices.

> 🧪 **TDD Focused**: This project follows TDD principles with comprehensive unit tests to prevent regressions and ensure code quality across all implementations.

## Core MCP Server Implementations

### 1. `simple_mcp_server.py` ⭐ **Primary MCP Server**
- **Purpose**: Main implementation of the MCP protocol specification
- **Protocol**: JSON-RPC over stdin/stdout (standard MCP communication)
- **Features**: 
  - Handles `initialize`, `tools/list`, `tools/call` methods
  - Provides tools: `get_system_info`, `get_current_time`, `list_files`, `calculate`
  - Async/await pattern for request handling
- **Usage**: This is the official MCP server following the protocol specification

### 2. `http_mcp_server.py` 🌐 **HTTP Wrapper**
- **Purpose**: Flask-based HTTP interface for the MCP server
- **Dependencies**: Requires Flask (`pip install flask`)
- **Features**:
  - REST API endpoints: `/mcp/servers`, `/mcp/tools`, `/mcp/call`
  - Web interface for testing MCP functionality
  - Wraps `simple_mcp_server.py` for HTTP access
- **Usage**: For web-based testing and integration

### 3. `simple_http_mcp_server.py` 🚀 **Standalone HTTP Server**
- **Purpose**: Minimal HTTP server with no external dependencies
- **Protocol**: HTTP using Python's built-in `http.server`
- **Features**:
  - Similar endpoints as Flask version
  - No external dependencies required
  - Good for lightweight deployment
- **Usage**: When you need HTTP access without installing Flask

## Utility Tools

### 4. `mcpservers.py` 🔍 **MCP Discovery Tool**
- **Purpose**: Discovers existing MCP servers on the network
- **Features**:
  - Scans common ports (3000, 3001, 3002, 8080, 8000, 9000)
  - Checks common MCP endpoints
  - Reports found servers with their capabilities
- **Usage**: For finding and testing MCP servers

### 5. `setup_mcp_servers.py` ⚙️ **MCP Setup Utility**
- **Purpose**: Automates installation and configuration of MCP servers
- **Features**:
  - Cross-platform setup (Windows/Linux/macOS)
  - Installs popular MCP servers
  - Configures MCP client settings
- **Usage**: For setting up MCP development environment

### 6. `mcp_dashboard.py` 📊 **Web Dashboard**
- **Purpose**: Web-based monitoring and management interface
- **Dependencies**: Flask, requests, psutil (see `dashboard_requirements.txt`)
- **Features**:
  - 🔍 **Server Discovery**: Automatically finds running MCP servers
  - 📊 **Health Monitoring**: System CPU, memory, disk usage
  - ⚡ **Process Tracking**: Shows running MCP processes
  - 🔧 **Tool Management**: View and test server tools
  - ▶️ **Server Control**: Start servers directly from the interface
- **Usage**: Web interface at `http://localhost:5000`

## Quick Start

### Running the Standard MCP Server
```bash
python simple_mcp_server.py
```

### Running the HTTP Interface
```bash
# Using Flask (requires: pip install flask)
python http_mcp_server.py

# Using built-in HTTP server (no dependencies)
python simple_http_mcp_server.py
```

### Testing MCP Servers
```bash
# Discover existing servers
python mcpservers.py

# Setup MCP environment
python setup_mcp_servers.py
```

### Running the Web Dashboard
```bash
# Install dashboard dependencies
pip install -r dashboard_requirements.txt

# Start the dashboard
python mcp_dashboard.py
# Then open http://localhost:5000 in your browser
```

## File Dependencies

- `http_mcp_server.py` → imports `simple_mcp_server.py`
- `mcp_dashboard.py` → uses `templates/dashboard.html` and requires packages in `dashboard_requirements.txt`
- Other files are standalone

## Testing

This project includes comprehensive unit tests to prevent regressions and ensure code quality.

### Running Tests

```bash
# Install test dependencies
pip install -r test_requirements.txt

# Run all tests
python run_tests.py

# Run with coverage reporting
python run_tests.py --coverage

# Run specific test module
python run_tests.py --test test_simple_mcp_server

# Check dependencies
python run_tests.py --check-deps
```

### Test Structure

```
📁 tests/
├── 📄 test_simple_mcp_server.py    (Core MCP protocol tests)
├── 📄 test_http_mcp_server.py      (HTTP wrapper tests)
├── 📄 test_mcp_discovery.py        (Server discovery tests)
├── 📄 test_mcp_dashboard.py        (Dashboard functionality tests)
└── 📄 __init__.py                  (Test package init)
```

### Test Coverage

The tests cover:
- ✅ **MCP Protocol Compliance**: `initialize`, `tools/list`, `tools/call` methods
- ✅ **Tool Functionality**: All built-in tools with various inputs
- ✅ **HTTP Endpoints**: All REST API endpoints and error handling
- ✅ **Server Discovery**: Network scanning and server detection
- ✅ **Dashboard APIs**: Health monitoring, process tracking, server control
- ✅ **Error Handling**: Invalid inputs, network failures, edge cases
- ✅ **Cross-platform**: Windows, Linux, macOS compatibility

### Continuous Integration

The project uses GitHub Actions for automated testing:
- **Multi-platform testing** (Ubuntu, Windows, macOS)
- **Multi-Python version** (3.9, 3.10, 3.11, 3.12)
- **Code quality checks** (flake8, black, pylint)
- **Security scanning** (bandit, safety)
- **Integration tests** with real server instances

## Notes

- The primary MCP server (`simple_mcp_server.py`) follows the official MCP specification
- HTTP versions are for testing and web integration
- All servers provide the same basic tools but through different interfaces
