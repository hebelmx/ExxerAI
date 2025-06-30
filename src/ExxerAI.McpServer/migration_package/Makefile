# Makefile for MCP Server Collection
# Cross-platform commands for development and testing

.PHONY: help install test test-coverage lint clean format security

# Default target
help:
	@echo "🚀 MCP Server Collection - Available Commands:"
	@echo ""
	@echo "📦 Setup:"
	@echo "  make install        Install all dependencies"
	@echo "  make install-dev    Install development dependencies"
	@echo ""
	@echo "🧪 Testing:"
	@echo "  make test          Run all unit tests"
	@echo "  make test-coverage Run tests with coverage report"
	@echo "  make test-specific TEST=test_simple_mcp_server  Run specific test"
	@echo ""
	@echo "🔍 Code Quality:"
	@echo "  make lint          Run code linting"
	@echo "  make format        Format code with black"
	@echo "  make security      Run security scans"
	@echo ""
	@echo "🧹 Cleanup:"
	@echo "  make clean         Remove temporary files"
	@echo ""
	@echo "🚀 Servers:"
	@echo "  make start-simple  Start simple MCP server"
	@echo "  make start-http    Start HTTP MCP server"
	@echo "  make start-dashboard Start web dashboard"

# Installation
install:
	pip install -r dashboard_requirements.txt

install-dev:
	pip install -r test_requirements.txt
	pip install black flake8 pylint mypy bandit safety

# Testing
test:
	python run_tests.py

test-coverage:
	python run_tests.py --coverage

test-specific:
	python run_tests.py --test $(TEST)

check-deps:
	python run_tests.py --check-deps

# Code Quality
lint:
	flake8 . --count --select=E9,F63,F7,F82 --show-source --statistics
	flake8 . --count --exit-zero --max-complexity=10 --max-line-length=127 --statistics

format:
	black .

format-check:
	black --check --diff .

security:
	bandit -r . -f json -o bandit-report.json
	safety check

# Server Management
start-simple:
	python simple_mcp_server.py

start-http:
	python http_mcp_server.py

start-standalone:
	python simple_http_mcp_server.py

start-dashboard:
	python mcp_dashboard.py

discover:
	python mcpservers.py

# Cleanup
clean:
	find . -type f -name "*.pyc" -delete
	find . -type d -name "__pycache__" -delete
	find . -type f -name "*.coverage" -delete
	find . -type f -name ".coverage" -delete
	find . -type f -name "coverage.xml" -delete
	find . -type f -name "bandit-report.json" -delete
	find . -type f -name "safety-report.json" -delete

# Development
dev-setup: install-dev
	@echo "🎉 Development environment ready!"
	@echo "Run 'make test' to verify everything works"

# Quick verification
verify: test lint security
	@echo "🎉✨ ALL CHECKS PASSED! ✨🎉"
	@echo "🚀 Code is production ready!"

# Release preparation  
pre-commit: format lint test security
	@echo "🎊🎉 READY FOR COMMIT! 🎉🎊"
	@echo "✨ All quality checks passed!"
