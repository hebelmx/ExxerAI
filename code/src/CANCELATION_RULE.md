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
return ResultExtensions.Cancelled<T>("Customizede Message");
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

Class CancellationAwareResult
Class ResultExtensions
Class ResultErrors
Class ResultConstants

```csharp
public static class ResultExtensions
{
		/// <summary>
        /// Creates a result indicating that an operation was cancelled.
        /// </summary>
        /// <returns>A <see cref="Result"/> object representing a cancelled operation.</returns>
        public static Result Cancelled()
        {
            return Result.WithFailure(ResultErrors.OperationCancelled);
        }

        /// <summary>
        /// /// Creates a generic result indicating that an operation was cancelled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Result<T> Cancelled<T>()
        {
            return Result<T>.WithFailure(ResultErrors.OperationCancelled);
        }
		

    public static bool IsCancelled(this Result result) =>
        ...code emited for brevity

    public static bool IsCancelled<T>(this Result<T> result) =>
        ...code emited for brevity
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
        CancellationToken cancellationToken = default)
    {
        // Validate arguments
        if (operation is null)
            return Result<T>.WithFailure($"Operation was null name of {operation} type Typeof {operation}");

        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<T>();

        try
        {
            var result = await operation(cancellationToken).ConfigureAwait(false);
            return Result<T>.Success(result);
        }
        catch (OperationCanceledException)
        {
            return ResultExtensions.Cancelled<T>();
        }
        catch (Exception ex)
        {
            return Result<T>.WithFailure($"Operation failed: {ex.Message}");
        }
    }
	
	  public static async Task<Result<T>> WrapResultOperation<T>(
	  ...code emited for brevity
	  
	 public static async Task<Result<T>> WrapWithTimeout<T>(
	 ...code emited for brevity
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
| Unit testing 	               | Use TestContext.Current.CancellationToken as Canceltoken |
---

