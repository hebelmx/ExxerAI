#!/bin/bash

# ExxerAI Google Drive Testing Setup Script

echo "🔧 Setting up Google Drive testing credentials..."

# Check if credentials file exists in current directory
if [ ! -f "google-credentials.json" ]; then
    echo "❌ Please place your google-credentials.json file in the current directory first!"
    echo "📁 Expected path: $(pwd)/google-credentials.json"
    exit 1
fi

# Create test directories if they don't exist
mkdir -p src/tests/ExxerAI.IntegrationTests
mkdir -p src/tests/ExxerAI.Infrastructure.Tests

# Copy credentials to test projects
echo "📋 Copying credentials to test projects..."
cp google-credentials.json src/tests/ExxerAI.IntegrationTests/
cp google-credentials.json src/tests/ExxerAI.Infrastructure.Tests/

# Create appsettings.test.json files from templates
echo "⚙️ Creating test configuration files..."

cd src/tests/ExxerAI.IntegrationTests/
if [ ! -f "appsettings.test.json" ]; then
    cp appsettings.test.json.template appsettings.test.json
    echo "✅ Created IntegrationTests/appsettings.test.json"
    echo "📝 Please edit this file and add your Google Drive document/folder IDs"
else
    echo "ℹ️ IntegrationTests/appsettings.test.json already exists"
fi

cd ../ExxerAI.Infrastructure.Tests/
if [ ! -f "appsettings.test.json" ]; then
    cp appsettings.test.json.template appsettings.test.json
    echo "✅ Created Infrastructure.Tests/appsettings.test.json"
else
    echo "ℹ️ Infrastructure.Tests/appsettings.test.json already exists"
fi

cd ../../../

echo ""
echo "🎉 Setup complete!"
echo ""
echo "📋 Next steps:"
echo "1. Edit src/tests/ExxerAI.IntegrationTests/appsettings.test.json"
echo "2. Replace YOUR_TEST_DOCUMENT_ID_HERE with actual Google Drive document IDs"
echo "3. Replace YOUR_TEST_FOLDER_ID_HERE with actual Google Drive folder IDs"
echo "4. Run tests: dotnet test src/tests/ExxerAI.IntegrationTests/"
echo ""
echo "🔒 Your credentials are secure - they're in .gitignore and won't be committed!"