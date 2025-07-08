---

description: Handling Cancellation in Functional .NET Applications appliesTo:

- Hexagonal architecture (ports & adapters)
- Functional .NET systems
- Applications returning Result rather than raw Task
- Projects avoiding exceptions for flow control



## Task: Audit and Refactor for Functional Cancellation Handling

### Objective

Refactor all asynchronous methods to ensure they:

- Accept and propagate `CancellationToken`
- Avoid exception-based cancellation control
- Return `Result<T>` with functional cancellation semantics via `ResultExtensions.Cancelled<T>()`

### Scope

- All application layers: Domain, Application, Infrastructure, Adapters
- Methods returning `Task`, `Task<T>`, `ValueTask`, or `ValueTask<T>`
- Excludes explicitly fire-and-forget or truly synchronous methods

### Acceptance Criteria

1. **Token Acceptance**

   - Every `async` method accepts a `CancellationToken` parameter.

2. **Token Propagation**

   - Internal async calls receive and forward the `CancellationToken`.

3. **Proactive Checks**

   - Add `if (ct.IsCancellationRequested)` checks before initiating work.

4. **Try/Catch Conversion**

   - Convert `OperationCanceledException` into `ResultExtensions.Cancelled<T>()`.

5. **Test Updates**

   - Add tests asserting cancellation behavior using `IsCancelled()` helpers.

6. **Reusable Utility**

   - Use `WrapCancellationAware<T>()` for chaining or repetitive logic.

7. **Audit Compliance**

   - Perform full codebase audit to identify violations or missing patterns.

8. **Build & Test Verification**

   - All projects **must compile**
   - **All tests must pass**
   - **No compiler warnings** allowed after changes

### Deliverables

- Refactored and audited code
- Passing CI builds with no warnings
- Optional: Analyzer script for automated pattern detection
- Task report listing impacted files and modules

---

---

# Rule: All Asynchronous Methods Must Support and Propagate `CancellationToken`, Handling Cancellation Functionally

## Scope

This rule applies to all asynchronous code within .NET applications that:

- Use hexagonal architecture
- Follow functional programming principles
- Return `Result<T>` instead of throwing
- Avoid using exceptions like `OperationCanceledException` for control flow

---

## Requirements

### 1. Accept a `CancellationToken`

All asynchronous or potentially cancellable methods **must expose** a `CancellationToken` parameter.

### 2. Propagate the Token

Pass the `CancellationToken` to all downstream or nested asynchronous methods to enable full cancellation propagation.

### 3. Avoid Exception-Based Control Flow

- Do **not** throw `OperationCanceledException` for normal control logic.
- Instead, **proactively check** `ct.IsCancellationRequested` and return a cancellation `Result<T>`.
- If cancellation might throw (e.g., `Task.Delay`), catch and convert it.

### 4. Return a Functional Cancellation Result

Always use a functional-style failure when cancellation is triggered:

```csharp
return ResultExtensions.Cancelled<T>();
```

---

## Implementation Pattern

### Result Error Definitions

```csharp
public static class ResultErrors
{
    public const string OperationCancelled = "Operation was cancelled by the user.";
}
```

### Result Extensions for Cancellation

```csharp
public static class ResultExtensions
{
    public static Result Cancelled() =>
        Result.WithFailure(ResultErrors.OperationCancelled);

    public static Result<T> Cancelled<T>() =>
        Result<T>.WithFailure(ResultErrors.OperationCancelled);

    public static bool IsCancelled(this Result result) =>
        result.Errors.Contains(ResultErrors.OperationCancelled);

    public static bool IsCancelled<T>(this Result<T> result) =>
        result.Errors.Contains(ResultErrors.OperationCancelled);
}
```

### Method Pattern

```csharp
public async Task<Result<MyDto>> HandleAsync(CancellationToken ct)
{
    if (ct.IsCancellationRequested)
        return ResultExtensions.Cancelled<MyDto>();

    try
    {
        await Task.Delay(1000, ct);
        return Result<MyDto>.WithSuccess(...);
    }
    catch (OperationCanceledException)
    {
        return ResultExtensions.Cancelled<MyDto>();
    }
}
```

### Optional Utility Wrapper

```csharp
public static async Task<Result<T>> WrapCancellationAware<T>(
    Func<CancellationToken, Task<T>> operation,
    CancellationToken ct)
{
    try
    {
        var result = await operation(ct);
        return Result<T>.WithSuccess(result);
    }
    catch (OperationCanceledException)
    {
        return ResultExtensions.Cancelled<T>();
    }
}
```

---

## Summary Table

| Aspect                       | Rule                                                     |
| ---------------------------- | -------------------------------------------------------- |
| Accept Token                 | All async methods must take `CancellationToken`          |
| Propagate Token              | Pass token to all internal async calls                   |
| Pre-check Cancellation       | Call `token.IsCancellationRequested` early               |
| Handle CancellationException | Catch and convert to `Result.Cancelled<T>()`             |
| Functional Return            | Return `Result<T>` for cancellation instead of exception |
| Avoid Exceptions             | Do not throw exceptions for control flow                 |

---