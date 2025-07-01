
# Handling Cancellation in Functional .NET Applications Using Result<T>

## Context

This guidance is tailored for applications that:
- Follow **hexagonal architecture** (ports & adapters)
- Use **functional programming principles**
- Enforce return of `Result<T>` instead of `void` or raw `Task`
- Avoid exceptions for flow control, including `OperationCanceledException`

## Problem

.NET’s async pattern throws `OperationCanceledException` when `CancellationToken` is triggered. This behavior conflicts with a functional, exception-free model where results are encapsulated in `Result<T>`.

## Solution

### 1. Add Cancellation Factory Method and Constants

Extend the result utility space with a standard cancellation error marker.
Example of code Tailor  to your actual class
```csharp
public static class ResultErrors
{
    public const string OperationCancelled = "Operation was cancelled by the user.";
}
```

Add factory helpers:
Example of code Tailor  to your actual class
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
Example of code Tailor  to your actual class
### 2. Check `CancellationToken` Explicitly Before Async Work
Example of code Tailor  to your actual class
```csharp
public async Task<Result<MyDto>> HandleAsync(CancellationToken ct)
{
    if (ct.IsCancellationRequested)
        return ResultExtensions.Cancelled<MyDto>();

    // Continue with processing
}
```
Example of code Tailor  to your actual class
### 3. Catch Cancellation Exceptions from Awaitable Calls

```csharp
try
{
    await Task.Delay(1000, ct);
    return Result.WithSuccess("Completed");
}
catch (OperationCanceledException)
{
    return ResultExtensions.Cancelled<string>();
}
```
Example of code Tailor  to your actual class
### 4. Wrap Operations via a Generalized Wrapper

```csharp
public static async Task<Result<T>> WrapCancellationAware<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct)
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

## Anti-Pattern

Avoid letting `OperationCanceledException` propagate out of boundaries:

```csharp
// ❌ Avoid
public async Task<Result<T>> UnsafeAsync(CancellationToken ct)
{
    await Task.Delay(1000, ct); // may throw
    return Result<T>.WithSuccess(...);
}
```

## Testing

When asserting, check for the known cancellation marker:

```csharp
result.IsCancelled().ShouldBeTrue();
```

## Summary

| Concern                   | Best Practice                                  |
|---------------------------|-----------------------------------------------|
| Async throws on cancel    | Wrap with `try/catch OperationCanceledException` |
| Void/Task methods         | Always return `Result<T>`                      |
| Contracts disallow exceptions | Use `ResultExtensions.Cancelled<T>()`             |
| Maintain functional purity | Use `ResultErrors` and encapsulated patterns   |

---

End of adapted guide.
