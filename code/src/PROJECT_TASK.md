---
description: Handling Cancellation in Functional .NET Applications - Real Implementation Standard
appliesTo:
- Production Code
- Hexagonal architecture (ports & adapters)
- Functional .NET systems
- Applications returning Result rather than raw Task
- Projects avoiding exceptions for flow control
---

# Task: Audit, implement and enforce This rule and the CANCELATION_RULE.MD across all the Projects

## One the Present Folder, Without Introducing any bugs o warnings, this project must compile wiht warning as errors

# RULE: All Asynchronous Methods Must Support and Propagate `CancellationToken`, Handling Cancellation Functionally

## Scope

This rule applies to all asynchronous code within .NET applications that:

- Use hexagonal architecture
- Follow functional programming principles
- Return `Result<T>` instead of throwing
- Avoid using exceptions like `OperationCanceledException` for control flow
- On CancelationTokenException Always use a functional-style failure when cancellation is triggered:
- return ResultExtensions.Cancelled<T>();
- return ResultExtensions.Cancelled<T>("Error message");

	## Unit test Methods
	- On unit test, the propagation Token must be suplied by the testing framework XUnit.V3 we must use TestContext.Current.CancellationToken 
---

## Requirements

### 1. Accept a `CancellationToken`

All asynchronous or potentially cancellable methods **must expose** a `CancellationToken` parameter.

### 2. Propagate the Token

Pass the `CancellationToken` to all downstream or nested asynchronous methods to enable full cancellation propagation.
On Unit testing Supply a TestContext.Current.CancellationToken as cancelationToken

### 3. Avoid Exception-Based Control Flow

- Do **not** throw `OperationCanceledException` for normal control logic.
- Instead, **proactively check** `ct.IsCancellationRequested` and return a cancellation `Result<T>`.
- If cancellation might throw (e.g., `Task.Delay`), catch and convert it.

### 4. Return a Functional Cancellation Result

Always use a functional-style failure when cancellation is triggered:

```csharp
return Result<T>.WithFailure("Operation was cancelled");
```

---

## Real Implementation Patterns (Based on Actual Codebase)

### Pattern 1: Simple Cancellation Handling

**From TaskService.cs - CreateAsync method:**
```csharp
public async Task<Result<AgentTask>> CreateAsync(
    string title, 
    TaskType taskType, 
    TaskPriority priority = TaskPriority.Medium,
    CancellationToken cancellationToken = default)
{
    try
    {
        await _taskLock.WaitAsync(cancellationToken);
        try
        {
            var task = new AgentTask
            {
                Id = Guid.NewGuid(),
                Title = title,
                Type = taskType,
                Priority = priority,
                Status = TaskStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _taskRepository.AddAsync(task, cancellationToken);
            
            _logger.LogInformation("Created task {TaskId} with title '{Title}' of type {TaskType} and priority {Priority}",
                task.Id, title, taskType, priority);

            return Result<AgentTask>.Success(task);
        }
        finally
        {
            _taskLock.Release();
        }
    }
    catch (OperationCanceledException)
    {
        _logger.LogInformation("Create task operation was cancelled");
        return Result<AgentTask>.WithFailure("Operation was cancelled");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating task {TaskTitle}", title);
        return Result<AgentTask>.WithFailure($"Failed to create task: {ex.Message}");
    }
}
```

**Simplified Example Pattern:**
```csharp
public async Task<Result<MyEntity>> CreateMyEntityAsync(
    string name, 
    CancellationToken cancellationToken = default)
{
    try
    {
        var entity = new MyEntity { Name = name };
        await _repository.AddAsync(entity, cancellationToken);
        return Result<MyEntity>.Success(entity);
    }
    catch (OperationCanceledException)
    {
        _logger.LogInformation("Create entity operation was cancelled");
        return Result<MyEntity>.WithFailure("Operation was cancelled");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating entity");
        return Result<MyEntity>.WithFailure($"Failed to create entity: {ex.Message}");
    }
}
```

### Pattern 2: Distinguish Between Cancellation and Timeout

**From HealthCheckService.cs - CheckComponentHealthAsync method:**
```csharp
private async Task<Result<ComponentHealthReport>> CheckComponentHealthAsync(
    string componentName, 
    IHealthCheckProvider provider, 
    CancellationToken cancellationToken)
{
    try
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(provider.Timeout);

        var result = await provider.CheckHealthAsync(timeoutCts.Token);
        _logger.LogDebug("Health check completed for component {ComponentName} with status {Status}",
            componentName, result.IsSuccess ? result.Value?.Status : "Failed");

        return result;
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        return Result<ComponentHealthReport>.WithFailure($"Health check cancelled for component '{componentName}'");
    }
    catch (OperationCanceledException)
    {
        return Result<ComponentHealthReport>.WithFailure($"Health check timeout for component '{componentName}' after {provider.Timeout}");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during health check for component {ComponentName}", componentName);
        return Result<ComponentHealthReport>.WithFailure($"Health check failed for component '{componentName}': {ex.Message}");
    }
}
```

**Simplified Example Pattern:**
```csharp
public async Task<Result<MyData>> GetDataWithTimeoutAsync(
    string dataId, 
    TimeSpan timeout,
    CancellationToken cancellationToken = default)
{
    try
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        
        var data = await _dataService.GetAsync(dataId, timeoutCts.Token);
        return Result<MyData>.Success(data);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        _logger.LogInformation("Get data operation was cancelled");
        return Result<MyData>.WithFailure("Operation was cancelled");
    }
    catch (OperationCanceledException)
    {
        _logger.LogWarning("Get data operation timed out after {Timeout}", timeout);
        return Result<MyData>.WithFailure($"Operation timed out after {timeout}");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting data");
        return Result<MyData>.WithFailure($"Failed to get data: {ex.Message}");
    }
}
```

### Pattern 3: Batch Processing with Cancellation

**From HybridDocumentProcessor.cs - ProcessDocumentBatchAsync method:**
```csharp
private async Task<DocumentProcessingResult> ProcessSingleDocumentAsync(
    DocumentBatchItem doc, 
    CancellationToken cancellationToken)
{
    try
    {
        // Process individual document
        var result = await ProcessDocumentAsync(doc.Content, doc.Metadata, cancellationToken);
        
        if (result.IsSuccess)
        {
            results.Add(result.Value!);
        }
        else
        {
            var failedResult = DocumentProcessingResult.Failed(result.Error ?? "Unknown processing error");
            results.Add(failedResult);
        }

        return result.IsSuccess ? result.Value! : DocumentProcessingResult.Failed(result.Error ?? "Unknown processing error");
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        _logger.LogWarning("Batch processing cancelled for document {FileName}", doc.Metadata.FileName);
        var cancelledResult = DocumentProcessingResult.Failed("Processing cancelled");
        results.Add(cancelledResult);
        return cancelledResult;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing document {FileName} in batch: {ErrorMessage}",
            doc.Metadata.FileName, ex.Message);

        var errorResult = DocumentProcessingResult.Failed($"Batch processing error: {ex.Message}");
        results.Add(errorResult);
        return errorResult;
    }
}
```

**Simplified Example Pattern:**
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
            // Check for cancellation before processing each item
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

---

## Actual Error Messages Used

Based on the codebase analysis, these are the actual cancellation messages used:

- `"Operation was cancelled"` - Standard cancellation message
- `"Processing cancelled"` - For document processing operations
- `"Health check cancelled for component '{componentName}'"` - For health check operations
- `"Health check timeout for component '{componentName}' after {timeout}"` - For timeout scenarios

---

## Implementation Guidelines

### 1. Exception Handling Pattern

```csharp
try
{
    // Async operation with cancellation token
    var result = await operation(cancellationToken);
    return Result<T>.Success(result);
}
catch (OperationCanceledException)
{
    // Log the cancellation
    _logger.LogInformation("Operation was cancelled");
    return Result<T>.WithFailure("Operation was cancelled");
}
```

### 2. Timeout vs Cancellation Distinction

```csharp
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    // External cancellation
    return Result<T>.WithFailure("Operation was cancelled");
}
catch (OperationCanceledException)
{
    // Timeout occurred
    return Result<T>.WithFailure("Operation timed out");
}
```

### 3. Proactive Cancellation Checks

```csharp
if (cancellationToken.IsCancellationRequested)
{
    return Result<T>.WithFailure("Operation was cancelled");
}
```

---

## Summary Table

| Aspect                       | Rule                                                     |
| ---------------------------- | -------------------------------------------------------- |
| Accept Token                 | All async methods must take `CancellationToken`          |
| Propagate Token              | Pass token to all internal async calls                   |
| Pre-check Cancellation       | Call `token.IsCancellationRequested` early               |
| Handle CancellationException | Catch and convert to `Result<T>.WithFailure()`           |
| Functional Return            | Return `Result<T>` for cancellation instead of exception |
| Avoid Exceptions             | Do not throw exceptions for control flow                 |
| Logging                      | Log cancellation events appropriately                    |
| Message Consistency          | Use consistent error messages across the application     |
| Unit testing 	               | Use TestContext.Current.CancellationToken as Canceltoken |
---

## Key Differences from Idealized Documentation

1. **No `ResultExtensions.Cancelled()`** - The actual codebase uses `Result<T>.WithFailure()` directly
2. **No centralized error constants** - Messages are defined inline
3. **Sophisticated timeout handling** - Distinguishes between cancellation and timeout
4. **Contextual error messages** - Different messages for different types of operations
5. **Logging integration** - Cancellation events are logged consistently

This document reflects the actual patterns used in the ExxerAI codebase rather than theoretical ideals.