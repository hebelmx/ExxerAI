#!/usr/bin/env python3
"""
Unit tests for MCP Dashboard
Tests the dashboard functionality and API endpoints.
"""

import unittest
import json
import sys
import os
from unittest.mock import patch, MagicMock

# Add parent directory to path
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

try:
    from mcp_dashboard import app, dashboard
    DASHBOARD_AVAILABLE = True
except ImportError:
    DASHBOARD_AVAILABLE = False


@unittest.skipUnless(DASHBOARD_AVAILABLE, "Dashboard not available")
class TestMCPDashboard(unittest.TestCase):
    """Test cases for MCP Dashboard"""
    
    def setUp(self):
        """Set up test fixtures"""
        self.app = app.test_client()
        self.app.testing = True
    
    def test_dashboard_home(self):
        """Test dashboard home page"""
        response = self.app.get('/')
        self.assertEqual(response.status_code, 200)
        self.assertIn(b'text/html', response.content_type.encode())
    
    @patch('requests.get')
    def test_api_discover_no_servers(self, mock_get):
        """Test API discover endpoint with no servers"""
        mock_get.side_effect = Exception("Connection failed")
        
        response = self.app.get('/api/discover')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIn('servers', data)
        self.assertIn('total_found', data)
        self.assertEqual(data['total_found'], 0)
    
    @patch('requests.get')
    def test_api_discover_with_servers(self, mock_get):
        """Test API discover endpoint with servers found"""
        mock_response = MagicMock()
        mock_response.status_code = 200
        mock_response.json.return_value = {"status": "active"}
        mock_response.elapsed.total_seconds.return_value = 0.05
        mock_get.return_value = mock_response
        
        response = self.app.get('/api/discover')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIn('servers', data)
        self.assertIn('total_found', data)
        # At least one server should be found due to our mock
        self.assertGreaterEqual(data['total_found'], 1)
    
    @patch('psutil.cpu_percent')
    @patch('psutil.virtual_memory')
    @patch('psutil.disk_usage')
    @patch('psutil.boot_time')
    @patch('platform.python_version')
    @patch('platform.platform')
    def test_api_health(self, mock_platform, mock_python_version, 
                       mock_boot_time, mock_disk_usage, 
                       mock_virtual_memory, mock_cpu_percent):
        """Test API health endpoint"""
        # Mock system info
        mock_cpu_percent.return_value = 25.5
        mock_memory = MagicMock()
        mock_memory.percent = 60.0
        mock_virtual_memory.return_value = mock_memory
        mock_disk = MagicMock()
        mock_disk.percent = 45.0
        mock_disk_usage.return_value = mock_disk
        mock_boot_time.return_value = 1640995200  # Some timestamp
        mock_python_version.return_value = "3.9.0"
        mock_platform.return_value = "Test Platform"
        
        response = self.app.get('/api/health')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIn('cpu_percent', data)
        self.assertIn('memory_percent', data)
        self.assertIn('disk_percent', data)
        self.assertIn('python_version', data)
        self.assertEqual(data['cpu_percent'], 25.5)
        self.assertEqual(data['memory_percent'], 60.0)
    
    @patch('psutil.process_iter')
    def test_api_processes(self, mock_process_iter):
        """Test API processes endpoint"""
        # Mock processes
        mock_proc = MagicMock()
        mock_proc.info = {
            'pid': 1234,
            'name': 'python',
            'cmdline': ['python', 'simple_mcp_server.py'],
            'cpu_percent': 5.0,
            'memory_percent': 2.5
        }
        mock_process_iter.return_value = [mock_proc]
        
        response = self.app.get('/api/processes')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIsInstance(data, list)
        self.assertEqual(len(data), 1)
        self.assertEqual(data[0]['pid'], 1234)
        self.assertIn('mcp', data[0]['cmdline'].lower())
    
    def test_api_servers(self):
        """Test API servers endpoint"""
        response = self.app.get('/api/servers')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIn('servers', data)
        self.assertIn('last_discovery', data)
    
    @patch('requests.get')
    def test_api_server_tools_not_found(self, mock_get):
        """Test API server tools for non-existent server"""
        response = self.app.get('/api/server/nonexistent/tools')
        self.assertEqual(response.status_code, 404)
        
        data = json.loads(response.data)
        self.assertIn('error', data)
        self.assertIn('not found', data['error'])
    
    @patch('requests.post')
    def test_api_server_call_not_found(self, mock_post):
        """Test API server call for non-existent server"""
        payload = {"name": "test_tool", "arguments": {}}
        response = self.app.post('/api/server/nonexistent/call',
                               json=payload,
                               content_type='application/json')
        self.assertEqual(response.status_code, 404)
    
    @patch('subprocess.Popen')
    @patch('platform.system')
    def test_api_start_server_windows(self, mock_system, mock_popen):
        """Test API start server on Windows"""
        mock_system.return_value = "Windows"
        mock_popen.return_value = MagicMock()
        
        response = self.app.get('/api/start-server/simple')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIn('message', data)
        self.assertIn('Started simple server', data['message'])
    
    @patch('subprocess.Popen')
    @patch('platform.system')
    def test_api_start_server_linux(self, mock_system, mock_popen):
        """Test API start server on Linux"""
        mock_system.return_value = "Linux"
        mock_popen.return_value = MagicMock()
        
        response = self.app.get('/api/start-server/http')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIn('message', data)
        self.assertIn('Started http server', data['message'])
    
    def test_api_start_server_unknown_type(self):
        """Test API start server with unknown server type"""
        response = self.app.get('/api/start-server/unknown')
        self.assertEqual(response.status_code, 400)
        
        data = json.loads(response.data)
        self.assertIn('error', data)
        self.assertIn('Unknown server type', data['error'])


class TestMCPDashboardClass(unittest.TestCase):
    """Test the MCPDashboard class directly"""
    
    def setUp(self):
        """Set up test fixtures"""
        from mcp_dashboard import MCPDashboard
        self.dashboard = MCPDashboard()
    
    def test_dashboard_initialization(self):
        """Test dashboard initialization"""
        self.assertEqual(len(self.dashboard.discovery_ports), 6)
        self.assertIn(3000, self.dashboard.discovery_ports)
        self.assertEqual(self.dashboard.servers, {})
        self.assertIsNone(self.dashboard.last_discovery)
    
    @patch('requests.get')
    def test_discover_servers_method(self, mock_get):
        """Test discover_servers method"""
        mock_response = MagicMock()
        mock_response.status_code = 200
        mock_response.json.return_value = {"status": "active"}
        mock_response.elapsed.total_seconds.return_value = 0.1
        mock_get.return_value = mock_response
        
        result = self.dashboard.discover_servers()
        
        self.assertIsInstance(result, dict)
        self.assertIsNotNone(self.dashboard.last_discovery)
    
    @patch('psutil.cpu_percent')
    @patch('psutil.virtual_memory')
    @patch('psutil.disk_usage')
    def test_get_system_health_method(self, mock_disk_usage, 
                                    mock_virtual_memory, mock_cpu_percent):
        """Test get_system_health method"""
        mock_cpu_percent.return_value = 30.0
        mock_memory = MagicMock()
        mock_memory.percent = 50.0
        mock_virtual_memory.return_value = mock_memory
        mock_disk = MagicMock()
        mock_disk.percent = 70.0
        mock_disk_usage.return_value = mock_disk
        
        result = self.dashboard.get_system_health()
        
        self.assertIn('cpu_percent', result)
        self.assertIn('memory_percent', result)
        self.assertEqual(result['cpu_percent'], 30.0)
        self.assertEqual(result['memory_percent'], 50.0)
    
    @patch('psutil.process_iter')
    def test_get_mcp_processes_method(self, mock_process_iter):
        """Test get_mcp_processes method"""
        mock_proc1 = MagicMock()
        mock_proc1.info = {
            'pid': 1234,
            'name': 'python',
            'cmdline': ['python', 'simple_mcp_server.py'],
            'cpu_percent': 5.0,
            'memory_percent': 2.5
        }
        
        mock_proc2 = MagicMock()
        mock_proc2.info = {
            'pid': 5678,
            'name': 'node',
            'cmdline': ['node', 'app.js'],
            'cpu_percent': 3.0,
            'memory_percent': 4.0
        }
        
        mock_process_iter.return_value = [mock_proc1, mock_proc2]
        
        result = self.dashboard.get_mcp_processes()
        
        self.assertEqual(len(result), 1)  # Only MCP-related process
        self.assertEqual(result[0]['pid'], 1234)
        self.assertIn('mcp', result[0]['cmdline'].lower())


if __name__ == '__main__':
    if not DASHBOARD_AVAILABLE:
        print("Dashboard not available, skipping dashboard tests")
        print("Install dependencies with: pip install -r dashboard_requirements.txt")
    else:
        unittest.main(verbosity=2)
