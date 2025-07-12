# 🧪 MCP Integration Testing Suite

## 📋 Overview

Comprehensive test suite for Model Context Protocol (MCP) integration with Google Drive. Includes 6 test suites covering the entire pipeline from infrastructure to end-to-end scenarios.

## 🏗️ Test Architecture

### Infrastructure Tests (`ExxerAI.Infrastructure.Tests/MCP/`)
- **GoogleDriveServiceTests.cs** - Unit tests for Google Drive service with mocks
- **GoogleDriveToolsTests.cs** - MCP tools layer validation  
- **DocumentProcessingToolsTests.cs** - Document processing tools testing

### Integration Tests (`ExxerAI.IntegrationTests/MCP/`)
- **GoogleDriveIntegrationTests.cs** - Real Google Drive API integration
- **DocumentIngestionChainTests.cs** - 10-step progressive chain testing
- **MCPEdgeCasesAndErrorTests.cs** - Error scenarios and edge cases

## 🚀 Quick Start

### 1. Validate Setup
```bash
cd /code/
./validate-test-setup.sh
```

### 2. Setup Credentials
```bash
# Place your google-credentials.json in /code/ directory
./setup-credentials.sh
```

### 3. Configure Test Data
Edit the configuration files with your Google Drive document/folder IDs:

**Integration Tests:**
```bash
nano src/tests/ExxerAI.IntegrationTests/appsettings.test.json
```

**Infrastructure Tests:**
```bash
nano src/tests/ExxerAI.Infrastructure.Tests/appsettings.test.json
```

### 4. Run Tests
```bash
# All infrastructure tests (fast, mocked)
dotnet test src/tests/ExxerAI.Infrastructure.Tests/

# All integration tests (real API calls)
dotnet test src/tests/ExxerAI.IntegrationTests/

# Specific test class
dotnet test src/tests/ExxerAI.IntegrationTests/ --filter GoogleDriveIntegrationTests

# Chain tests only
dotnet test src/tests/ExxerAI.IntegrationTests/ --filter DocumentIngestionChainTests
```

## 📊 Test Coverage

### Infrastructure Layer (Unit Tests)
- ✅ Service initialization and configuration
- ✅ Authentication and credential handling
- ✅ Folder and document operations
- ✅ Watch session management
- ✅ Error handling and cancellation
- ✅ Stress testing and performance

### Integration Layer (API Tests)
- ✅ Real Google Drive API connectivity
- ✅ Document metadata and download operations
- ✅ Folder watching and change detection
- ✅ Performance testing with large documents
- ✅ Concurrent operations handling
- ✅ Network resilience and timeout handling

### End-to-End (Chain Tests)
- ✅ **Step 1-3**: Service → Discovery → Retrieval
- ✅ **Step 4-6**: Processing → Analysis → Structure
- ✅ **Step 7-9**: Validation → Integration → Monitoring
- ✅ **Step 10**: Full system orchestration
- ✅ **Performance**: Complete pipeline timing

### Edge Cases & Error Handling
- ✅ Network timeouts and cancellation
- ✅ Authentication failures
- ✅ Corrupted and invalid documents
- ✅ Resource exhaustion scenarios
- ✅ Malicious input validation
- ✅ Concurrent operation safety

## 🔧 Configuration

### Required Google Drive Setup
1. **Test Document** - A PDF or Word document for basic testing
2. **Test Folder** - A folder for watching and change detection
3. **Large Document** - A large file (>1MB) for performance testing

### Configuration Files
- `appsettings.test.json` - Test-specific configuration
- `google-credentials.json` - Google Drive API credentials
- Templates provided for easy setup

### Environment Variables (Optional)
```bash
export EXXERAI_TEST_GoogleDrive__CredentialsPath="./google-credentials.json"
export EXXERAI_TEST_GoogleDrive__TestDocumentId="your-document-id"
export EXXERAI_TEST_GoogleDrive__TestFolderId="your-folder-id"
```

## 🛡️ Security Features

- ✅ All credentials in `.gitignore` - never committed
- ✅ Template files for safe setup
- ✅ Environment variable support
- ✅ Secure credential management
- ✅ Test isolation and cleanup

## 📈 Performance Benchmarks

### Expected Performance
- **Document Download**: < 5 seconds for 10MB files
- **Metadata Retrieval**: < 1 second
- **Folder Watch Setup**: < 2 seconds
- **Complete Chain (Steps 1-10)**: < 60 seconds
- **Concurrent Operations**: 5+ parallel requests supported

### Stress Testing
- **Large Documents**: Up to 50MB files
- **Concurrent Requests**: 10+ simultaneous operations
- **Memory Pressure**: Multiple 5MB documents
- **Extended Operations**: Long-running watch sessions

## 🔍 Troubleshooting

### Common Issues

**Authentication Errors:**
- Check `google-credentials.json` file path and format
- Verify Google Drive API is enabled in Google Cloud Console
- Ensure service account has access to test documents/folders

**Test Configuration:**
- Run `./validate-test-setup.sh` to check setup
- Verify document/folder IDs are correct (no placeholder values)
- Check that test documents are accessible

**Performance Issues:**
- Large documents may timeout - adjust `TestTimeoutMinutes`
- Network issues can cause flaky tests - retry mechanism included
- Rate limiting may occur with rapid API calls

### Debug Mode
Enable detailed logging in `appsettings.test.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "ExxerAI": "Debug",
      "Default": "Information"
    }
  }
}
```

## 🎯 Test Categories

### By Speed
- **Fast** (< 1s): Infrastructure unit tests
- **Medium** (1-10s): Integration API tests
- **Slow** (10s+): Chain tests and performance tests

### By Purpose
- **Smoke Tests**: Basic connectivity and configuration
- **Functional Tests**: Core feature validation
- **Performance Tests**: Speed and resource usage
- **Resilience Tests**: Error handling and recovery
- **Security Tests**: Input validation and boundary conditions

## 📝 Extending Tests

### Adding New Test Cases
1. Follow existing test patterns and naming conventions
2. Use proper setup/teardown with `IAsyncLifetime`
3. Include both positive and negative test scenarios
4. Add performance assertions where relevant
5. Document test purpose and expected behavior

### Test Data Management
- Use `TestDataHelper` for URL parsing and validation
- Create sample documents with `SampleDocuments` helper
- Register resources for automatic cleanup
- Follow test isolation principles

---

🚀 **Ready to test your MCP integration!** The comprehensive test suite provides confidence in your Google Drive integration from unit level through complete end-to-end workflows.