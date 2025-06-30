#!/usr/bin/env python3
"""
Unit tests for MCP Discovery Tool
Tests the server discovery functionality.
"""

import unittest
import sys
import os
from unittest.mock import patch, MagicMock
import requests

# Add parent directory to path
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

try:
    from mcpservers import MCPDiscovery
    MCP_DISCOVERY_AVAILABLE = True
except ImportError:
    MCP_DISCOVERY_AVAILABLE = False


@unittest.skipUnless(MCP_DISCOVERY_AVAILABLE, "MCPDiscovery not available")
class TestMCPDiscovery(unittest.TestCase):
    """Test cases for MCP Discovery functionality"""
    
    def setUp(self):
        """Set up test fixtures"""
        self.discovery = MCPDiscovery()
    
    def test_initialization(self):
        """Test MCPDiscovery initialization"""
        self.assertEqual(len(self.discovery.common_ports), 6)
        self.assertIn(3000, self.discovery.common_ports)
        self.assertIn(8080, self.discovery.common_ports)
        
        self.assertEqual(len(self.discovery.common_endpoints), 5)
        self.assertIn("/mcp/servers", self.discovery.common_endpoints)
        self.assertIn("/mcp/tools", self.discovery.common_endpoints)
    
    @patch('requests.get')
    def test_check_endpoint_success(self, mock_get):
        """Test successful endpoint check"""
        # Mock successful response
        mock_response = MagicMock()
        mock_response.status_code = 200
        mock_response.json.return_value = {"status": "active"}
        mock_response.elapsed.total_seconds.return_value = 0.05
        mock_get.return_value = mock_response
        
        result = self.discovery.check_endpoint("http://localhost:3000/mcp/servers")
        
        self.assertIsNotNone(result)
        self.assertEqual(result, {"status": "active"})
        mock_get.assert_called_once_with("http://localhost:3000/mcp/servers", timeout=5)
    
    @patch('requests.get')
    def test_check_endpoint_connection_error(self, mock_get):
        """Test endpoint check with connection error"""
        mock_get.side_effect = requests.ConnectionError("Connection failed")
        
        result = self.discovery.check_endpoint("http://localhost:3000/mcp/servers")
        
        self.assertIsNone(result)
    
    @patch('requests.get')
    def test_check_endpoint_timeout(self, mock_get):
        """Test endpoint check with timeout"""
        mock_get.side_effect = requests.Timeout("Request timed out")
        
        result = self.discovery.check_endpoint("http://localhost:3000/mcp/servers")
        
        self.assertIsNone(result)
    
    @patch('requests.get')
    def test_check_endpoint_json_decode_error(self, mock_get):
        """Test endpoint check with JSON decode error"""
        mock_response = MagicMock()
        mock_response.status_code = 200
        mock_response.json.side_effect = ValueError("Invalid JSON")
        mock_response.text = "Plain text response"
        mock_get.return_value = mock_response
        
        result = self.discovery.check_endpoint("http://localhost:3000/mcp/servers")
        
        self.assertIsNotNone(result)
        self.assertIn("raw_response", result)
        self.assertEqual(result["raw_response"], "Plain text response")
    
    @patch('time.sleep')
    @patch('requests.get')
    def test_discover_mcp_servers_found(self, mock_get, mock_sleep):
        """Test server discovery with servers found"""
        # Mock responses - one successful, others fail
        responses = []
        
        def mock_get_side_effect(url, timeout):
            if "localhost:3000/mcp/servers" in url:
                response = MagicMock()
                response.status_code = 200
                response.json.return_value = {"server": "mcp-test"}
                response.elapsed.total_seconds.return_value = 0.1
                return response
            else:
                raise requests.ConnectionError("No server")
        
        mock_get.side_effect = mock_get_side_effect
        
        result = self.discovery.discover_mcp_servers()
        
        self.assertEqual(len(result), 1)
        self.assertIn("localhost:3000", result)
        self.assertEqual(result["localhost:3000"]["status"], "active")
        self.assertIsNotNone(self.discovery.last_discovery)
    
    @patch('time.sleep')
    @patch('requests.get')
    def test_discover_mcp_servers_none_found(self, mock_get, mock_sleep):
        """Test server discovery with no servers found"""
        mock_get.side_effect = requests.ConnectionError("No servers")
        
        result = self.discovery.discover_mcp_servers()
        
        self.assertEqual(len(result), 0)
        self.assertIsNotNone(self.discovery.last_discovery)
    
    def test_display_servers_empty(self):
        """Test display servers with empty server list"""
        # Capture stdout to test print output
        from io import StringIO
        import sys
        
        captured_output = StringIO()
        sys.stdout = captured_output
        
        self.discovery.display_servers({})
        
        sys.stdout = sys.__stdout__
        output = captured_output.getvalue()
        
        self.assertIn("No MCP servers found", output)
    
    def test_display_servers_with_data(self):
        """Test display servers with server data"""
        from datetime import datetime
        from io import StringIO
        import sys
        
        servers = {
            "localhost:3000": {
                "url": "http://localhost:3000",
                "status": "active",
                "last_seen": datetime.now(),
                "response_time": 0.05,
                "data": {"tools": ["tool1", "tool2"]}
            }
        }
        
        captured_output = StringIO()
        sys.stdout = captured_output
        
        self.discovery.display_servers(servers)
        
        sys.stdout = sys.__stdout__
        output = captured_output.getvalue()
        
        self.assertIn("Found 1 MCP server", output)
        self.assertIn("localhost:3000", output)
        self.assertIn("Response time: 50ms", output)


class TestMCPDiscoveryIntegration(unittest.TestCase):
    """Integration tests for MCP Discovery"""
    
    def setUp(self):
        self.discovery = MCPDiscovery()
    
    @patch('builtins.print')
    def test_full_discovery_cycle(self, mock_print):
        """Test a full discovery cycle"""
        # This test runs the actual discovery but with mocked printing
        # to avoid cluttering test output
        
        with patch('requests.get') as mock_get:
            mock_get.side_effect = requests.ConnectionError("No servers")
            
            servers = self.discovery.discover_mcp_servers()
            self.discovery.display_servers(servers)
            
            # Verify the cycle completed without errors
            self.assertIsInstance(servers, dict)
            self.assertIsNotNone(self.discovery.last_discovery)


if __name__ == '__main__':
    if not MCP_DISCOVERY_AVAILABLE:
        print("MCPDiscovery not available, skipping discovery tests")
    else:
        unittest.main(verbosity=2)
