---

description: Handling Cancellation in Functional .NET Applications appliesTo:

- Hexagonal architecture (ports & adapters)
- Functional .NET systems
- Applications returning Result rather than raw Task
- Projects avoiding exceptions for flow control

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
### 5. Source of CancelationToken
- On Production Code the Cancelation Token must be supplied by the caller method if there is no one the calling method must be refactor to accept and propagate a cancelation Token
- On unit test, the propagation Token must be suplied by the testing framework XUnit.V3 we must use TestContext.Current.CancellationToken 
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

