#!/bin/bash
# MCP Server Collection Quick Start
echo "Setting up MCP Server Collection..."

# Install dependencies
echo "Installing dependencies..."
pip install -r test_requirements.txt

# Run tests
echo "Running tests..."
python run_tests.py

# Show status
echo "Setup complete!"
echo "Ready to start dashboard: make start-dashboard"
echo "Available commands: make help"
