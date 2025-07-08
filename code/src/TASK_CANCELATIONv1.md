---
description: Task for Auditing and Refactoring Cancellation Handling - Real Implementation Standard
appliesTo:
- Hexagonal architecture (ports & adapters)
- Functional .NET systems
- Applications returning Result rather than raw Task
- Projects avoiding exceptions for flow control
---

## Task: Audit and Refactor for Functional Cancellation Handling

### Objective

Refactor all asynchronous methods on production code to ensure they consistently follow this rule and the rule CANCELATION_RULE.md actual cancellation patterns used in the ExxerAI codebase:

- All Async Projects Must:
- Accept and propagate `CancellationToken`
- Handle `OperationCanceledException` functionally
- Return `Result<T>.C()` for cancellation scenarios
- Distinguish between cancellation and timeout when applicable
- Maintain consistent error messages and logging

### Scope

- All application layers: Domain, Application, Infrastructure, Adapters
- Methods returning `Task`, `Task<T>`, `ValueTask`, or `ValueTask<T>`
- Excludes explicitly fire-and-forget or truly synchronous methods

### Current Implementation Analysis

Based on codebase analysis, the following patterns are currently used:

#### Pattern 1: Basic Cancellation (TaskService.cs)
```csharp
catch (OperationCanceledException)
{
    _logger.LogInformation("Create task operation was cancelled");
    return Result<AgentTask>.WithFailure("Operation was cancelled");
}
```

#### Pattern 2: Timeout vs Cancellation (HealthCheckService.cs)
```csharp
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    return Result<ComponentHealthReport>.WithFailure($"Health check cancelled for component '{componentName}'");
}
catch (OperationCanceledException)
{
    return Result<ComponentHealthReport>.WithFailure($"Health check timeout for component '{componentName}' after {provider.Timeout}");
}
```

#### Pattern 3: Batch Processing (HybridDocumentProcessor.cs)
```csharp
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    _logger.LogWarning("Batch processing cancelled for document {FileName}", doc.Metadata.FileName);
    var cancelledResult = DocumentProcessingResult.Failed("Processing cancelled");
    results.Add(cancelledResult);
    return cancelledResult;
}
```

### Acceptance Criteria

1. **Token Acceptance**
   - Every `async` method accepts a `CancellationToken` parameter with `default` value.

2. **Token Propagation**
   - Internal async calls receive and forward the `CancellationToken`.

3. **Consistent Exception Handling**
   - All `OperationCanceledException` are caught and converted to `Result<T>.WithFailure()`.

4. **Timeout Distinction**
   - When timeouts are used, distinguish between external cancellation and timeout.

5. **Logging Standards**
   - Log cancellation events at appropriate levels (Information/Warning).

6. **Error Message Consistency**
   - Use consistent error messages across similar operations:
     - `"Operation was cancelled"` - General operations
     - `"Processing cancelled"` - Document processing
     - `"[Operation] cancelled for [resource]"` - Resource-specific operations
     - `"[Operation] timeout for [resource] after [duration]"` - Timeout scenarios

7. **Proactive Checks**
   - Add `if (cancellationToken.IsCancellationRequested)` checks before expensive operations.

8. **Test Coverage**
   - Add tests for cancellation scenarios using standard assertion patterns.

### Implementation Standards

#### Required Exception Handling Pattern

**Template Based on TaskService.cs:**
```csharp
public async Task<Result<T>> OperationAsync(CancellationToken cancellationToken = default)
{
    try
    {
        // Proactive check (optional but recommended)
        if (cancellationToken.IsCancellationRequested)
            return Result<T>.WithFailure("Operation was cancelled");

        // Actual operation with cancellation token
        var result = await SomeAsyncOperation(cancellationToken);
        return Result<T>.Success(result);
    }
    catch (OperationCanceledException)
    {
        _logger.LogInformation("Operation was cancelled");
        return Result<T>.WithFailure("Operation was cancelled");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during operation");
        return Result<T>.WithFailure($"Operation failed: {ex.Message}");
    }
}
```

**Real Example - Creating an Agent:**
```csharp
public async Task<Result<Agent>> CreateAgentAsync(
    CreateAgentRequest request, 
    CancellationToken cancellationToken = default)
{
    try
    {
        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Configuration = request.Configuration,
            Status = AgentStatus.Inactive,
            CreatedAt = DateTime.UtcNow
        };

        await _agentRepository.AddAsync(agent, cancellationToken);
        _logger.LogInformation("Created agent {AgentId} with name '{Name}'", agent.Id, request.Name);
        
        return Result<Agent>.Success(agent);
    }
    catch (OperationCanceledException)
    {
        _logger.LogInformation("Create agent operation was cancelled");
        return Result<Agent>.WithFailure("Operation was cancelled");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating agent {AgentName}", request.Name);
        return Result<Agent>.WithFailure($"Failed to create agent: {ex.Message}");
    }
}
```

#### Timeout Handling Pattern

**Template Based on HealthCheckService.cs:**
```csharp
public async Task<Result<T>> OperationWithTimeoutAsync(
    TimeSpan timeout, 
    CancellationToken cancellationToken = default)
{
    try
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        
        var result = await SomeAsyncOperation(timeoutCts.Token);
        return Result<T>.Success(result);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        _logger.LogInformation("Operation was cancelled");
        return Result<T>.WithFailure("Operation was cancelled");
    }
    catch (OperationCanceledException)
    {
        _logger.LogWarning("Operation timed out after {Timeout}", timeout);
        return Result<T>.WithFailure($"Operation timed out after {timeout}");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during operation");
        return Result<T>.WithFailure($"Operation failed: {ex.Message}");
    }
}
```

**Real Example - Database Connection with Timeout:**
```csharp
public async Task<Result<DatabaseConnection>> ConnectAsync(
    string connectionString,
    TimeSpan timeout,
    CancellationToken cancellationToken = default)
{
    try
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        
        var connection = await _connectionFactory.CreateAsync(connectionString, timeoutCts.Token);
        _logger.LogInformation("Successfully connected to database");
        
        return Result<DatabaseConnection>.Success(connection);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        _logger.LogInformation("Database connection was cancelled");
        return Result<DatabaseConnection>.WithFailure("Connection was cancelled");
    }
    catch (OperationCanceledException)
    {
        _logger.LogWarning("Database connection timed out after {Timeout}", timeout);
        return Result<DatabaseConnection>.WithFailure($"Connection timed out after {timeout}");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error connecting to database");
        return Result<DatabaseConnection>.WithFailure($"Connection failed: {ex.Message}");
    }
}
```

#### Batch Processing Pattern

**Template Based on HybridDocumentProcessor.cs:**
```csharp
public async Task<Result<List<ProcessedItem>>> ProcessBatchAsync(
    IEnumerable<Item> items, 
    CancellationToken cancellationToken = default)
{
    var results = new List<ProcessedItem>();
    
    foreach (var item in items)
    {
        try
        {
            // Check for cancellation before each item
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Batch processing was cancelled");
                return Result<List<ProcessedItem>>.WithFailure("Operation was cancelled");
            }
            
            var processed = await ProcessItemAsync(item, cancellationToken);
            results.Add(processed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Processing cancelled for item {ItemId}", item.Id);
            return Result<List<ProcessedItem>>.WithFailure("Processing cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing item {ItemId}", item.Id);
            return Result<List<ProcessedItem>>.WithFailure($"Processing failed: {ex.Message}");
        }
    }
    
    return Result<List<ProcessedItem>>.Success(results);
}
```
### Unit Testing, on Unit testing the 



### Files Requiring Audit

Based on current analysis, the following files already have partial cancellation handling:

#### ✅ Already Implemented
- `TaskService.cs` - Basic cancellation pattern
- `HealthCheckService.cs` - Timeout vs cancellation distinction
- `HybridDocumentProcessor.cs` - Batch processing with cancellation

#### 🔄 Needs Review/Standardization
- `EnhancedLLMService.cs` - Multiple async operations
- `SemanticSearchService.cs` - Search operations
- `HybridKnowledgeService.cs` - Knowledge operations
- `PersonaService.cs` - Persona management
- `AgentService.cs` - Agent operations
- `WorkflowService.cs` - Workflow management
- `DocumentIngestionService.cs` - Document processing

### Deliverables

1. **Audited and Refactored Code**
   - All async methods follow the established cancellation patterns
   - Consistent error messages and logging

2. **Updated Test Coverage**
   - Tests for cancellation scenarios
   - Tests for timeout vs cancellation distinction

3. **Documentation Updates**
   - This document reflects actual implementation patterns
   - Code comments explain cancellation behavior

4. **Build Verification**
   - All projects compile without warnings
   - All tests pass
   - No regression in functionality

### Verification Steps

1. **Code Review**
   - Verify all async methods accept `CancellationToken`
   - Check exception handling patterns
   - Validate error message consistency

2. **Testing**
   - Run cancellation tests
   - Verify timeout behavior
   - Test edge cases

3. **Performance**
   - Ensure no performance regression
   - Verify proper resource cleanup on cancellation

### Success Metrics

- ✅ 100% of async methods accept `CancellationToken`
- ✅ Consistent error handling across all services
- ✅ Proper timeout vs cancellation distinction
- ✅ Comprehensive logging of cancellation events
- ✅ Zero compiler warnings
- ✅ All tests passing

---

## Key Differences from Theoretical Documentation

1. **No `ResultExtensions.Cancelled()`** - Uses `Result<T>.WithFailure()` directly
2. **No centralized error constants** - Messages defined contextually
3. **Sophisticated timeout handling** - Real-world distinction between cancellation sources
4. **Contextual error messages** - Different messages for different operation types
5. **Integrated logging** - Cancellation events are logged consistently
6. **Practical patterns** - Based on actual codebase usage rather than theoretical ideals

This task document reflects the actual implementation patterns found in the ExxerAI codebase and provides actionable guidance for standardizing cancellation handling across all services.