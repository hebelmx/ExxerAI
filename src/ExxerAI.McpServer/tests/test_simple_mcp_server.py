#!/usr/bin/env python3
"""
Unit tests for SimpleMCPServer
Tests the core MCP protocol implementation and tool functionality.
"""

import unittest
import asyncio
import json
import sys
import os
from unittest.mock import patch, mock_open
from datetime import datetime

# Add parent directory to path to import the MCP server
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
from simple_mcp_server import SimpleMCPServer


class TestSimpleMCPServer(unittest.TestCase):
    """Test cases for SimpleMCPServer class"""
    
    def setUp(self):
        """Set up test fixtures before each test method."""
        self.server = SimpleMCPServer()
    
    def test_server_initialization(self):
        """Test that server initializes with correct tools"""
        expected_tools = {
            "get_system_info",
            "get_current_time", 
            "list_files",
            "calculate"
        }
        self.assertEqual(set(self.server.tools.keys()), expected_tools)
    
    def test_tool_schemas(self):
        """Test that all tools have proper schema definitions"""
        for tool_name, tool_def in self.server.tools.items():
            self.assertIn("description", tool_def)
            self.assertIn("parameters", tool_def)
            self.assertIsInstance(tool_def["description"], str)
            self.assertIsInstance(tool_def["parameters"], dict)
    
    async def test_initialize_request(self):
        """Test MCP initialize method"""
        request = {"method": "initialize", "params": {}}
        response = await self.server.handle_request(request)
        
        self.assertIn("capabilities", response)
        self.assertIn("serverInfo", response)
        self.assertEqual(response["serverInfo"]["name"], "simple-mcp-server")
        self.assertEqual(response["serverInfo"]["version"], "1.0.0")
    
    async def test_tools_list_request(self):
        """Test MCP tools/list method"""
        request = {"method": "tools/list", "params": {}}
        response = await self.server.handle_request(request)
        
        self.assertIn("tools", response)
        self.assertEqual(len(response["tools"]), 4)
        
        # Check each tool has required fields
        for tool in response["tools"]:
            self.assertIn("name", tool)
            self.assertIn("description", tool)
            self.assertIn("inputSchema", tool)
    
    async def test_get_system_info_tool(self):
        """Test get_system_info tool"""
        request = {
            "method": "tools/call",
            "params": {
                "name": "get_system_info",
                "arguments": {}
            }
        }
        response = await self.server.handle_request(request)
        
        self.assertIn("content", response)
        self.assertEqual(len(response["content"]), 1)
        self.assertEqual(response["content"][0]["type"], "text")
        
        # Parse the JSON content and verify it has expected keys
        content_json = json.loads(response["content"][0]["text"])
        expected_keys = {"platform", "system", "machine", "python_version", "current_directory"}
        self.assertTrue(expected_keys.issubset(content_json.keys()))
    
    async def test_get_current_time_tool_readable(self):
        """Test get_current_time tool with readable format"""
        request = {
            "method": "tools/call",
            "params": {
                "name": "get_current_time",
                "arguments": {"format": "readable"}
            }
        }
        response = await self.server.handle_request(request)
        
        self.assertIn("content", response)
        time_text = response["content"][0]["text"]
        
        # Check format matches YYYY-MM-DD HH:MM:SS
        import re
        pattern = r'\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}'
        self.assertRegex(time_text, pattern)
    
    async def test_get_current_time_tool_iso(self):
        """Test get_current_time tool with ISO format"""
        request = {
            "method": "tools/call",
            "params": {
                "name": "get_current_time",
                "arguments": {"format": "iso"}
            }
        }
        response = await self.server.handle_request(request)
        
        time_text = response["content"][0]["text"]
        # Should be able to parse as ISO format
        try:
            datetime.fromisoformat(time_text.replace('Z', '+00:00'))
        except ValueError:
            self.fail("Time string is not in valid ISO format")
    
    async def test_get_current_time_tool_timestamp(self):
        """Test get_current_time tool with timestamp format"""
        request = {
            "method": "tools/call",
            "params": {
                "name": "get_current_time",
                "arguments": {"format": "timestamp"}
            }
        }
        response = await self.server.handle_request(request)
        
        time_text = response["content"][0]["text"]
        # Should be a valid float timestamp
        try:
            float(time_text)
        except ValueError:
            self.fail("Timestamp is not a valid number")
    
    @patch('os.listdir')
    @patch('os.path.isfile')
    @patch('os.path.isdir')
    async def test_list_files_tool(self, mock_isdir, mock_isfile, mock_listdir):
        """Test list_files tool"""
        # Mock file system
        mock_listdir.return_value = ['file1.txt', 'file2.py', 'directory1']
        mock_isfile.side_effect = lambda x: x.endswith('.txt') or x.endswith('.py')
        mock_isdir.side_effect = lambda x: x.endswith('directory1')
        
        request = {
            "method": "tools/call",
            "params": {
                "name": "list_files",
                "arguments": {"path": "/test/path"}
            }
        }
        response = await self.server.handle_request(request)
        
        self.assertIn("content", response)
        content_json = json.loads(response["content"][0]["text"])
        
        self.assertIn("files", content_json)
        self.assertIn("directories", content_json)
        self.assertEqual(len(content_json["files"]), 2)
        self.assertEqual(len(content_json["directories"]), 1)
    
    async def test_calculate_tool_valid_expression(self):
        """Test calculate tool with valid mathematical expression"""
        request = {
            "method": "tools/call",
            "params": {
                "name": "calculate",
                "arguments": {"expression": "2 + 3 * 4"}
            }
        }
        response = await self.server.handle_request(request)
        
        result_text = response["content"][0]["text"]
        self.assertEqual(result_text, "2 + 3 * 4 = 14")
    
    async def test_calculate_tool_invalid_characters(self):
        """Test calculate tool with invalid characters"""
        request = {
            "method": "tools/call",
            "params": {
                "name": "calculate",
                "arguments": {"expression": "2 + 3; import os"}
            }
        }
        response = await self.server.handle_request(request)
        
        result_text = response["content"][0]["text"]
        self.assertIn("invalid characters", result_text)
    
    async def test_calculate_tool_division_by_zero(self):
        """Test calculate tool with division by zero"""
        request = {
            "method": "tools/call",
            "params": {
                "name": "calculate",
                "arguments": {"expression": "5 / 0"}
            }
        }
        response = await self.server.handle_request(request)
        
        result_text = response["content"][0]["text"]
        self.assertIn("Error calculating", result_text)
    
    async def test_unknown_tool(self):
        """Test calling an unknown tool"""
        request = {
            "method": "tools/call",
            "params": {
                "name": "unknown_tool",
                "arguments": {}
            }
        }
        response = await self.server.handle_request(request)
        
        self.assertIn("error", response)
        self.assertIn("Unknown tool", response["error"])
    
    async def test_unknown_method(self):
        """Test calling an unknown method"""
        request = {"method": "unknown/method", "params": {}}
        response = await self.server.handle_request(request)
        
        self.assertIn("error", response)
        self.assertIn("Unknown method", response["error"])


class AsyncTestCase(unittest.TestCase):
    """Base class for running async tests"""
    
    def run_async(self, coro):
        """Helper method to run async functions in tests"""
        return asyncio.get_event_loop().run_until_complete(coro)


class TestSimpleMCPServerAsync(AsyncTestCase):
    """Async test cases for SimpleMCPServer"""
    
    def setUp(self):
        self.server = SimpleMCPServer()
    
    def test_initialize_request_async(self):
        """Test initialize request asynchronously"""
        result = self.run_async(self.server.handle_request({
            "method": "initialize", 
            "params": {}
        }))
        self.assertIn("capabilities", result)
    
    def test_tools_list_request_async(self):
        """Test tools list request asynchronously"""
        result = self.run_async(self.server.handle_request({
            "method": "tools/list", 
            "params": {}
        }))
        self.assertIn("tools", result)
        self.assertEqual(len(result["tools"]), 4)
    
    def test_system_info_tool_async(self):
        """Test system info tool asynchronously"""
        result = self.run_async(self.server.handle_request({
            "method": "tools/call",
            "params": {
                "name": "get_system_info",
                "arguments": {}
            }
        }))
        self.assertIn("content", result)
    
    def test_calculate_tool_async(self):
        """Test calculate tool asynchronously"""
        result = self.run_async(self.server.handle_request({
            "method": "tools/call",
            "params": {
                "name": "calculate",
                "arguments": {"expression": "10 * 5 + 2"}
            }
        }))
        self.assertEqual(result["content"][0]["text"], "10 * 5 + 2 = 52")


if __name__ == '__main__':
    # Run tests with verbose output
    unittest.main(verbosity=2)
