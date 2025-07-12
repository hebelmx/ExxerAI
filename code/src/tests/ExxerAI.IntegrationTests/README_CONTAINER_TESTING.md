# 🐳 Container-Based Integration Testing

This document explains how to use the Docker container-based integration testing framework for ExxerAI knowledge store services.

## 🚀 What We've Built

We've implemented a comprehensive **Testcontainers.NET** framework that automatically spins up real database instances for integration testing:

### ✅ Container Fixtures Available:
- **QdrantContainerFixture** - Qdrant vector database for embedding storage
- **Neo4jContainerFixture** - Neo4j graph database for relationship storage  
- **KnowledgeStoreContainerFixture** - Combined orchestration for hybrid testing

### ✅ Updated Test Classes:
- **QdrantVectorStoreIntegrationTests** - Now uses containerized Qdrant
- **ContainerVerificationTests** - Verifies container setup

## 🛠️ Prerequisites

### Docker Installation Required
```bash
# Install Docker Desktop for Windows/Mac or Docker Engine for Linux
# For WSL2 users, install Docker Desktop and enable WSL2 integration
```

### Verify Docker is Running
```bash
docker --version
docker ps
```

## 📊 Test Status Transformation

### Before Container Implementation:
- ✅ **39 succeeded** 
- ❌ **29 failed** (credential issues)
- ⏭️ **30 skipped** (no external services)

### After Container Implementation (when Docker is available):
- ✅ **Expected: 69+ succeeded** (30 previously skipped now run!)
- ❌ **Expected: <29 failed** (only real issues)
- ⏭️ **Expected: 0 skipped** (all integration tests enabled)

## 🎯 How to Run Container Tests

### Run All Container-Enabled Tests
```bash
cd /path/to/ExxerAI.IntegrationTests
dotnet test --filter "Category!=External"
```

### Run Specific Container Test Categories
```bash
# Qdrant vector store tests
dotnet test --filter "FullyQualifiedName~QdrantVectorStore"

# Neo4j graph store tests  
dotnet test --filter "FullyQualifiedName~Neo4jGraphStore"

# Hybrid knowledge store tests
dotnet test --filter "FullyQualifiedName~HybridKnowledgeService"

# Container verification tests
dotnet test --filter "FullyQualifiedName~ContainerVerification"
```

## 🏗️ Container Architecture

### Automatic Container Lifecycle
1. **Startup**: Containers start automatically before test execution
2. **Health Checks**: Wait for services to be ready (HTTP + custom checks)
3. **Test Execution**: Tests run against real database instances
4. **Cleanup**: Containers are automatically stopped and removed

### Container Configuration
```csharp
// Qdrant Configuration
- Image: qdrant/qdrant:v1.7.4
- Ports: 6333 (HTTP), 6334 (gRPC)
- Health Check: HTTP 200 on /

// Neo4j Configuration  
- Image: neo4j:5-community
- Ports: 7474 (HTTP), 7687 (Bolt)
- Auth: neo4j/test123456
- Health Check: HTTP 200 on /db/neo4j/
```

## 🧪 Test Isolation & Data Management

### Automatic Database Cleaning
Each test gets a fresh database state:
```csharp
// Qdrant: Collections are created/deleted per test
// Neo4j: "MATCH (n) DETACH DELETE n" runs between tests
```

### Test Scenarios
```csharp
await fixture.SetupTestScenarioAsync("empty");         // Clean state
await fixture.SetupTestScenarioAsync("sample_documents"); // Sample data
await fixture.SetupTestScenarioAsync("performance_test");  // Performance data
```

## 📈 Performance Expectations

### Container Startup Times:
- **Qdrant**: ~10-15 seconds
- **Neo4j**: ~20-30 seconds  
- **Combined**: ~30-45 seconds (parallel startup)

### Test Execution:
- **Individual Tests**: 1-5 seconds
- **Full Suite**: 10-20 minutes (with real databases!)

## 🔧 Configuration Examples

### Use Container Fixtures in Your Tests
```csharp
public class MyKnowledgeStoreTests : IClassFixture<KnowledgeStoreContainerFixture>, IAsyncLifetime
{
    private readonly KnowledgeStoreContainerFixture _containerFixture;

    public MyKnowledgeStoreTests(KnowledgeStoreContainerFixture containerFixture)
    {
        _containerFixture = containerFixture;
    }

    public async ValueTask InitializeAsync()
    {
        _containerFixture.EnsureFullyAvailable();
        await _containerFixture.SetupTestScenarioAsync("empty");
    }

    [Fact]
    public async Task MyTest_ShouldUseRealDatabases()
    {
        // Get real service instances
        var qdrantService = _containerFixture.GetRequiredService<IQdrantVectorStore>();
        var neo4jService = _containerFixture.GetRequiredService<INeo4jGraphStore>();
        
        // Test with real databases!
    }
}
```

## 🚨 Troubleshooting

### Docker Not Available
```bash
Error: The command 'docker' could not be found
```
**Solution**: Install Docker Desktop and ensure WSL2 integration is enabled.

### Container Startup Timeout
```bash
Error: Container failed health check after X attempts
```
**Solutions**: 
- Increase memory allocation to Docker
- Check firewall settings
- Verify Docker daemon is running

### Port Conflicts
```bash
Error: Port already in use
```
**Solution**: Testcontainers automatically maps to available ports.

## 🎉 Benefits Achieved

### ✅ Real Integration Testing
- **No Mocks**: Tests against actual Qdrant & Neo4j instances
- **Real Behavior**: Catches integration issues that mocks miss
- **Performance Testing**: Actual database performance characteristics

### ✅ Developer Experience  
- **Zero Setup**: Containers start automatically
- **Isolation**: Each test run is independent
- **Debugging**: Can inspect database state during test execution

### ✅ CI/CD Ready
- **Portable**: Works in any environment with Docker
- **Parallel**: Multiple test suites can run simultaneously
- **Reliable**: Deterministic test outcomes

## 🚀 Next Steps

1. **Install Docker Desktop** on your development machine
2. **Enable WSL2 Integration** in Docker Desktop settings  
3. **Run the tests** and watch 30 previously skipped tests come to life!
4. **Extend the framework** for additional services (Redis, PostgreSQL, etc.)

---

*The future of integration testing is containerized! 🐳*