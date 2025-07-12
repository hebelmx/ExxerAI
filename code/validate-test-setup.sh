#!/bin/bash

# ExxerAI Test Setup Validation Script

echo "🔍 Validating Google Drive test setup..."
echo ""

# Check if we're in the right directory
if [ ! -f "setup-credentials.sh" ]; then
    echo "❌ Please run this script from the /code/ directory"
    exit 1
fi

# Function to check file exists
check_file() {
    local file_path="$1"
    local description="$2"
    
    if [ -f "$file_path" ]; then
        echo "✅ $description: Found"
        return 0
    else
        echo "❌ $description: Missing"
        return 1
    fi
}

# Function to check JSON configuration
check_json_config() {
    local config_file="$1"
    local description="$2"
    
    if [ ! -f "$config_file" ]; then
        echo "❌ $description: File missing"
        return 1
    fi
    
    # Check if placeholders are still present
    if grep -q "YOUR_TEST_" "$config_file"; then
        echo "⚠️ $description: Contains placeholder values - needs configuration"
        echo "   Please edit $config_file and replace YOUR_TEST_* values"
        return 1
    else
        echo "✅ $description: Configured"
        return 0
    fi
}

echo "📁 Checking credential files..."
check_file "src/tests/ExxerAI.IntegrationTests/google-credentials.json" "Integration Tests Credentials"
check_file "src/tests/ExxerAI.Infrastructure.Tests/google-credentials.json" "Infrastructure Tests Credentials"

echo ""
echo "⚙️ Checking configuration files..."
check_json_config "src/tests/ExxerAI.IntegrationTests/appsettings.test.json" "Integration Tests Config"
check_json_config "src/tests/ExxerAI.Infrastructure.Tests/appsettings.test.json" "Infrastructure Tests Config"

echo ""
echo "🔧 Checking template files..."
check_file "src/tests/ExxerAI.IntegrationTests/appsettings.test.json.template" "Integration Template"
check_file "src/tests/ExxerAI.Infrastructure.Tests/appsettings.test.json.template" "Infrastructure Template"

echo ""
echo "🧪 Checking test files..."
test_files=(
    "src/tests/ExxerAI.Infrastructure.Tests/MCP/GoogleDriveServiceTests.cs"
    "src/tests/ExxerAI.Infrastructure.Tests/MCP/GoogleDriveToolsTests.cs"
    "src/tests/ExxerAI.Infrastructure.Tests/MCP/DocumentProcessingToolsTests.cs"
    "src/tests/ExxerAI.IntegrationTests/MCP/GoogleDriveIntegrationTests.cs"
    "src/tests/ExxerAI.IntegrationTests/MCP/DocumentIngestionChainTests.cs"
    "src/tests/ExxerAI.IntegrationTests/MCP/MCPEdgeCasesAndErrorTests.cs"
)

all_tests_present=true
for test_file in "${test_files[@]}"; do
    if ! check_file "$test_file" "$(basename "$test_file")"; then
        all_tests_present=false
    fi
done

echo ""
echo "🎯 Test Suite Summary:"
if [ "$all_tests_present" = true ]; then
    echo "✅ All MCP test files are present"
    echo "✅ 6 comprehensive test suites ready"
    echo "✅ Infrastructure + Integration + E2E coverage"
else
    echo "❌ Some test files are missing"
fi

echo ""
echo "📋 Next Steps:"
echo "1. Place your google-credentials.json file in the /code/ directory"
echo "2. Run: ./setup-credentials.sh"
echo "3. Edit appsettings.test.json files with your Google Drive IDs"
echo "4. Run tests: dotnet test src/tests/ExxerAI.IntegrationTests/"

echo ""
echo "🚀 Quick Test Commands:"
echo "# Infrastructure tests (mocked):"
echo "dotnet test src/tests/ExxerAI.Infrastructure.Tests/"
echo ""
echo "# Integration tests (real Google Drive API):"
echo "dotnet test src/tests/ExxerAI.IntegrationTests/"
echo ""
echo "# Specific test class:"
echo "dotnet test src/tests/ExxerAI.IntegrationTests/ --filter GoogleDriveIntegrationTests"