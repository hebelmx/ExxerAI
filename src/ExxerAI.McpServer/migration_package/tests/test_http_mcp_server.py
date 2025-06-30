#!/usr/bin/env python3
"""
Unit tests for HTTP MCP Server
Tests the Flask-based HTTP wrapper functionality.
"""

import unittest
import json
import sys
import os
from unittest.mock import patch, MagicMock

# Add parent directory to path
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

try:
    from http_mcp_server import app
    FLASK_AVAILABLE = True
except ImportError:
    FLASK_AVAILABLE = False


@unittest.skipUnless(FLASK_AVAILABLE, "Flask not available")
class TestHTTPMCPServer(unittest.TestCase):
    """Test cases for HTTP MCP Server"""
    
    def setUp(self):
        """Set up test fixtures"""
        self.app = app.test_client()
        self.app.testing = True
    
    def test_home_endpoint(self):
        """Test the home page endpoint"""
        response = self.app.get('/')
        self.assertEqual(response.status_code, 200)
        self.assertIn(b'Simple MCP Server HTTP Interface', response.data)
        self.assertIn(b'Available Endpoints', response.data)
    
    def test_list_servers_endpoint(self):
        """Test the /mcp/servers endpoint"""
        response = self.app.get('/mcp/servers')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIn('simple-mcp-server', data)
        self.assertIn('name', data['simple-mcp-server'])
    
    def test_list_tools_endpoint(self):
        """Test the /mcp/tools endpoint"""
        response = self.app.get('/mcp/tools')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIn('tools', data)
        self.assertEqual(len(data['tools']), 4)
        
        # Verify tool names
        tool_names = {tool['name'] for tool in data['tools']}
        expected_tools = {'get_system_info', 'get_current_time', 'list_files', 'calculate'}
        self.assertEqual(tool_names, expected_tools)
    
    def test_system_info_endpoint(self):
        """Test the /mcp/system-info endpoint"""
        response = self.app.get('/mcp/system-info')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIn('content', data)
        
        # Parse the nested JSON content
        content_json = json.loads(data['content'][0]['text'])
        expected_keys = {'platform', 'system', 'machine', 'python_version'}
        self.assertTrue(expected_keys.issubset(content_json.keys()))
    
    def test_time_endpoint(self):
        """Test the /mcp/time endpoint"""
        response = self.app.get('/mcp/time')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIn('content', data)
        
        # Should return readable time format by default
        time_text = data['content'][0]['text']
        import re
        pattern = r'\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}'
        self.assertRegex(time_text, pattern)
    
    def test_time_endpoint_with_format(self):
        """Test the /mcp/time endpoint with format parameter"""
        response = self.app.get('/mcp/time?format=iso')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        time_text = data['content'][0]['text']
        
        # Should be ISO format
        from datetime import datetime
        try:
            datetime.fromisoformat(time_text.replace('Z', '+00:00'))
        except ValueError:
            self.fail("Time string is not in valid ISO format")
    
    @patch('os.listdir')
    @patch('os.path.isfile')
    @patch('os.path.isdir')
    def test_files_endpoint(self, mock_isdir, mock_isfile, mock_listdir):
        """Test the /mcp/files endpoint"""
        # Mock file system
        mock_listdir.return_value = ['file1.txt', 'directory1']
        mock_isfile.side_effect = lambda x: x.endswith('.txt')
        mock_isdir.side_effect = lambda x: x.endswith('directory1')
        
        response = self.app.get('/mcp/files?path=.')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        content_json = json.loads(data['content'][0]['text'])
        
        self.assertIn('files', content_json)
        self.assertIn('directories', content_json)
    
    def test_calculate_endpoint(self):
        """Test the /mcp/calculate endpoint"""
        response = self.app.get('/mcp/calculate?expr=5*5+10')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        result_text = data['content'][0]['text']
        self.assertEqual(result_text, '5*5+10 = 35')
    
    def test_calculate_endpoint_missing_expr(self):
        """Test the /mcp/calculate endpoint without expression"""
        response = self.app.get('/mcp/calculate')
        self.assertEqual(response.status_code, 400)
        
        data = json.loads(response.data)
        self.assertIn('error', data)
        self.assertIn('Missing', data['error'])
    
    def test_call_tool_endpoint_get_system_info(self):
        """Test POST /mcp/call endpoint with get_system_info"""
        payload = {
            "name": "get_system_info",
            "arguments": {}
        }
        response = self.app.post('/mcp/call', 
                               json=payload,
                               content_type='application/json')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIn('content', data)
    
    def test_call_tool_endpoint_calculate(self):
        """Test POST /mcp/call endpoint with calculate tool"""
        payload = {
            "name": "calculate", 
            "arguments": {"expression": "3 + 4 * 2"}
        }
        response = self.app.post('/mcp/call',
                               json=payload, 
                               content_type='application/json')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        result_text = data['content'][0]['text']
        self.assertEqual(result_text, '3 + 4 * 2 = 11')
    
    def test_call_tool_endpoint_unknown_tool(self):
        """Test POST /mcp/call endpoint with unknown tool"""
        payload = {
            "name": "unknown_tool",
            "arguments": {}
        }
        response = self.app.post('/mcp/call',
                               json=payload,
                               content_type='application/json')
        self.assertEqual(response.status_code, 200)
        
        data = json.loads(response.data)
        self.assertIn('error', data)
    
    def test_call_tool_endpoint_invalid_json(self):
        """Test POST /mcp/call endpoint with invalid JSON"""
        response = self.app.post('/mcp/call',
                               data='invalid json',
                               content_type='application/json')
        self.assertEqual(response.status_code, 400)
    
    def test_nonexistent_endpoint(self):
        """Test accessing a non-existent endpoint"""
        response = self.app.get('/nonexistent')
        self.assertEqual(response.status_code, 404)


if __name__ == '__main__':
    if not FLASK_AVAILABLE:
        print("Flask not available, skipping HTTP MCP server tests")
        print("Install Flask with: pip install flask")
    else:
        unittest.main(verbosity=2)
