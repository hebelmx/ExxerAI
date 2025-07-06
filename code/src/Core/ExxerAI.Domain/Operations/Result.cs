namespace ExxerAI.Domain.Operations;

/// <summary>
/// Represents the result of an operation, including success status and error messages.
/// Use this class for operations that do not return a value but need to indicate success or failure.
/// </summary>
public sealed class Result
{
    // Note: Error messages are now centralized in ResultConstants class

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class with the specified success state and errors.
    /// </summary>
    /// <param name="succeeded">Indicates whether the operation succeeded.</param>
    /// <param name="errors">A collection of error messages.</param>
    private Result(bool succeeded, IEnumerable<string> errors)
    {
        var errorArray = errors?.ToArray() ?? Array.Empty<string>();
        var hasAnyErrors = errorArray.Length > 0;

        IsSuccess = succeeded && !hasAnyErrors;
        Errors = errorArray;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class as a failure with no errors.
    /// </summary>
    public Result()
    {
        IsSuccess = false;
        Errors = Array.Empty<string>();
    }

    /// <summary>
    /// Returns a string representation of the result, showing either "Success" or the list of errors.
    /// </summary>
    public override string ToString()
    {
        return IsSuccess ? ResultConstants.SuccessPrefix : FormatErrorsString(Errors, ResultConstants.FailurePrefix);
    }

    /// <summary>
    /// Formats error messages into a readable string. Shared logic for consistent formatting.
    /// Uses Span&lt;char&gt; optimizations for small error collections to reduce allocations.
    /// </summary>
    /// <param name="errors">The collection of error messages.</param>
    /// <param name="prefix">The prefix to use (e.g., "Success", "Failure").</param>
    /// <returns>A formatted string representation.</returns>
    public static string FormatErrorsString(IEnumerable<string> errors, string prefix)
    {
        if (errors is null || !errors.Any())
            return prefix;

        // Fast path for arrays/collections with known count
        if (errors is string[] errorArray)
        {
            return FormatErrorsStringSpan(errorArray.AsSpan(), prefix);
        }

        if (errors is ICollection<string> collection && collection.Count <= 16)
        {
            // Use array for small collections (avoid repeated enumeration)
            var collectionArray = new string[collection.Count];
            var index = 0;
            foreach (var error in collection)
            {
                collectionArray[index++] = error;
            }
            return FormatErrorsStringSpan(collectionArray.AsSpan(), prefix);
        }

        // Fallback to StringBuilder for large collections
        return FormatErrorsStringFallback(errors, prefix);
    }

    /// <summary>
    /// High-performance formatting using Span&lt;string&gt; for small collections.
    /// Uses stackalloc char buffer to minimize allocations.
    /// </summary>
    /// <param name="errorSpan">The span of error messages.</param>
    /// <param name="prefix">The prefix to use.</param>
    /// <returns>A formatted string.</returns>
    private static string FormatErrorsStringSpan(ReadOnlySpan<string> errorSpan, string prefix)
    {
        if (errorSpan.IsEmpty)
            return prefix;

        // Estimate capacity: prefix + ": " + errors + separators
        var estimatedLength = prefix.Length + 2; // ": "
        foreach (var error in errorSpan)
        {
            estimatedLength += (error?.Length ?? 0) + 2; // ", "
        }

        // Use stackalloc for small strings, StringBuilder for large ones
        if (estimatedLength <= 512)
        {
            Span<char> buffer = stackalloc char[estimatedLength];
            return BuildStringInSpan(buffer, errorSpan, prefix);
        }
        else
        {
            return FormatErrorsStringFallback(errorSpan.ToArray(), prefix);
        }
    }

    /// <summary>
    /// Builds the formatted string directly in a Span&lt;char&gt; buffer for maximum efficiency.
    /// </summary>
    /// <param name="buffer">The character buffer to write to.</param>
    /// <param name="errorSpan">The span of error messages.</param>
    /// <param name="prefix">The prefix to use.</param>
    /// <returns>The formatted string.</returns>
    private static string BuildStringInSpan(Span<char> buffer, ReadOnlySpan<string> errorSpan, string prefix)
    {
        var position = 0;

        // Write prefix
        prefix.AsSpan().CopyTo(buffer[position..]);
        position += prefix.Length;

        // Write ": "
        ": ".AsSpan().CopyTo(buffer[position..]);
        position += 2;

        // Write errors with separators
        for (var i = 0; i < errorSpan.Length; i++)
        {
            if (i > 0)
            {
                ", ".AsSpan().CopyTo(buffer[position..]);
                position += 2;
            }

            var error = errorSpan[i] ?? string.Empty;
            error.AsSpan().CopyTo(buffer[position..]);
            position += error.Length;
        }

        return new string(buffer[..position]);
    }

    /// <summary>
    /// StringBuilder fallback for large collections or when Span optimization isn't beneficial.
    /// </summary>
    /// <param name="errors">The error messages.</param>
    /// <param name="prefix">The prefix to use.</param>
    /// <returns>A formatted string.</returns>
    private static string FormatErrorsStringFallback(IEnumerable<string> errors, string prefix)
    {
        var stringBuilder = new StringBuilder($"{prefix}: ");
        var isFirst = true;

        foreach (var error in errors)
        {
            if (!isFirst)
                stringBuilder.Append(", ");
            stringBuilder.Append(error);
            isFirst = false;
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets a value indicating whether the result is a success.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets the collection of error messages associated with the result.
    /// </summary>
    public IEnumerable<string> Errors { get; private set; }

    /// <summary>
    /// Gets the first non-empty error message, or null if none exist.
    /// </summary>
    public string? Error => Errors.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e));

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="Result"/> instance.</returns>
    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The collection of error messages.</param>
    /// <returns>A failed <see cref="Result"/> instance.</returns>
    public static Result WithFailure(IEnumerable<string> errors)
    {
        // Check for null or empty collections and provide default error message (consistent with generic version)
        var errorArray = errors?.ToArray();
        if (errorArray is null || errorArray.Length == 0)
        {
            errorArray = [ResultConstants.DefaultErrorMessage];
        }
        return new Result(false, errorArray);
    }

    /// <summary>
    /// Creates a failed result with the specified errors (overload for string array).
    /// </summary>
    /// <param name="errors">The array of error messages.</param>
    /// <returns>A failed <see cref="Result"/> instance.</returns>
    public static Result WithFailure(string[] errors)
    {
        // Check for empty array and provide default error message (consistent with IEnumerable overload)
        if (errors is null || errors.Length == 0)
        {
            errors = [ResultConstants.DefaultErrorMessage];
        }
        return new Result(false, errors);
    }

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed <see cref="Result"/> instance.</returns>
    public static Result WithFailure(string error)
    {
        return new Result(false, new List<string>() { error });
    }

    /// <summary>
    /// Executes the specified action if the result is successful.
    /// </summary>
    /// <param name="action">The action to execute on success.</param>
    /// <returns>The current <see cref="Result"/> instance.</returns>
    public Result OnSuccess(Action action)
    {
        if (IsSuccess)
        {
            action();
        }
        return this;
    }

    /// <summary>
    /// Executes the specified action if the result is a failure.
    /// </summary>
    /// <param name="action">The action to execute on failure, receiving the error messages.</param>
    /// <returns>The current <see cref="Result"/> instance.</returns>
    /// <remarks>
    /// BUG FIX (2025-01-03): Previously this method called action(Errors) without null checking,
    /// causing NullReferenceException when Errors collection was null. Fixed by adding proper
    /// null checking and fallback error message, making it consistent with Result&lt;T&gt;.OnFailure.
    /// Test: Result_OnFailure_ShouldInvokeActionWithErrors_WhenResultIsFailure now passes.
    ///
    /// CONSISTENCY FIX (2025-01-03): Standardized default error message to use DefaultErrorMessage
    /// constant to eliminate inconsistencies throughout the codebase.
    /// </remarks>
    public Result OnFailure(Action<IEnumerable<string>> action)
    {
        if (IsFailure)
        {
            if (Errors is not null)
            {
                action(Errors);
            }
            else
            {
                action([ResultConstants.DefaultErrorMessage]);
            }
        }
        return this;
    }

    /// <summary>
    /// Maps a successful result to a <see cref="Result{T}"/> using the provided function, or propagates errors.
    /// </summary>
    /// <typeparam name="T">The type of the value to return on success.</typeparam>
    /// <param name="func">The function to execute on success.</param>
    /// <returns>A <see cref="Result{T}"/> representing the outcome.</returns>
    public Result<T> Map<T>(Func<T> func)
    {
        return IsSuccess ? Result<T>.Success(func()) : Result<T>.WithFailure(Errors);
    }

    /// <summary>
    /// Binds a successful result to another <see cref="Result{T}"/> using the provided function, or propagates errors.
    /// </summary>
    /// <typeparam name="T">The type of the value to return on success.</typeparam>
    /// <param name="func">The function to execute on success.</param>
    /// <returns>A <see cref="Result{T}"/> representing the outcome.</returns>
    public Result<T> Bind<T>(Func<Result<T>> func)
    {
        return IsSuccess ? func() : Result<T>.WithFailure(Errors);
    }

    /// <summary>
    /// Ensures a condition is met for a successful result, otherwise returns a failure with the specified error message.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="errorMessage">The error message if the condition fails.</param>
    /// <returns>A <see cref="Result"/> representing the outcome.</returns>
    public Result Ensure(Func<bool> condition, string errorMessage)
    {
        if (IsSuccess && !condition())
        {
            return WithFailure(errorMessage);
        }
        return this;
    }

    /// <summary>
    /// Executes the specified action if the result is successful, returning the current result.
    /// </summary>
    /// <param name="action">The action to execute on success.</param>
    /// <returns>The current <see cref="Result"/> instance.</returns>
    public Result Tap(Action action)
    {
        if (IsSuccess)
        {
            action();
        }
        return this;
    }

    /// <summary>
    /// Combines multiple results, aggregating all errors. Returns success if all are successful.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A <see cref="Result"/> representing the combined outcome.</returns>
    public Result Combine(params Result[] results)
    {
        if (results is null || results.Length == 0)
            return this;

        var errorList = new List<string>();

        // Add current result's errors if it's a failure
        if (IsFailure && Errors is not null)
        {
            errorList.AddRange(Errors);
        }

        // Add errors from all failed results
        foreach (var result in results)
        {
            if (result.IsFailure && result.Errors is not null)
            {
                errorList.AddRange(result.Errors);
            }
        }

        return errorList.Count > 0 ? WithFailure(errorList) : Success();
    }

    /// <summary>
    /// Matches the result to either a success or failure function.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure, receiving the errors.</param>
    /// <returns>The result of the executed function.</returns>
    public T Match<T>(Func<T> onSuccess, Func<IEnumerable<string>, T> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Errors);
    }

    /// <summary>
    /// Recovers from a failure by executing the provided recovery function.
    /// </summary>
    /// <param name="recoverFunc">The function to execute on failure.</param>
    /// <returns>The recovered or original <see cref="Result"/>.</returns>
    public Result Recover(Func<Result> recoverFunc)
    {
        return IsFailure ? recoverFunc() : this;
    }

    /// <summary>
    /// Combines two sets of errors into a single failed result, or a default failure if both are empty.
    /// Uses Span&lt;T&gt; optimizations for small collections to reduce allocations.
    /// </summary>
    /// <param name="primaryErrors">The primary error messages.</param>
    /// <param name="secondaryErrors">The secondary error messages.</param>
    /// <returns>A failed <see cref="Result"/> with all errors.</returns>
    public static Result CombineErrors(IEnumerable<string>? primaryErrors, IEnumerable<string>? secondaryErrors)
    {
        // Fast path: both null
        if (primaryErrors is null && secondaryErrors is null)
        {
            return WithFailure(ResultConstants.NoErrorsFoundMessage);
        }

        // Fast path: one is null
        if (primaryErrors is null)
        {
            return WithFailure(secondaryErrors!);
        }
        if (secondaryErrors is null)
        {
            return WithFailure(primaryErrors);
        }

        // Span optimization for small collections
        if (TryGetSmallCollectionCounts(primaryErrors, secondaryErrors, out var primaryCount, out var secondaryCount))
        {
            var totalCount = primaryCount + secondaryCount;
            if (totalCount <= 32) // Reasonable stackalloc limit
            {
                return CombineErrorsSpan(primaryErrors, secondaryErrors, primaryCount, secondaryCount, totalCount);
            }
        }

        // Fallback to List<string> for large collections
        return CombineErrorsFallback(primaryErrors, secondaryErrors);
    }

    /// <summary>
    /// Attempts to get collection counts for small collections that benefit from Span optimization.
    /// </summary>
    /// <param name="primary">Primary error collection.</param>
    /// <param name="secondary">Secondary error collection.</param>
    /// <param name="primaryCount">Count of primary errors.</param>
    /// <param name="secondaryCount">Count of secondary errors.</param>
    /// <returns>True if both collections are small enough for Span optimization.</returns>
    private static bool TryGetSmallCollectionCounts(
        IEnumerable<string> primary,
        IEnumerable<string> secondary,
        out int primaryCount,
        out int secondaryCount)
    {
        primaryCount = 0;
        secondaryCount = 0;

        // Only optimize for collections with known counts
        if (primary is ICollection<string> primaryCollection && primaryCollection.Count <= 16)
        {
            primaryCount = primaryCollection.Count;
        }
        else
        {
            return false;
        }

        if (secondary is ICollection<string> secondaryCollection && secondaryCollection.Count <= 16)
        {
            secondaryCount = secondaryCollection.Count;
        }
        else
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// High-performance error combining using arrays for small collections.
    /// </summary>
    /// <param name="primaryErrors">Primary error collection.</param>
    /// <param name="secondaryErrors">Secondary error collection.</param>
    /// <param name="primaryCount">Count of primary errors.</param>
    /// <param name="secondaryCount">Count of secondary errors.</param>
    /// <param name="totalCount">Total error count.</param>
    /// <returns>A failed Result with combined errors.</returns>
    private static Result CombineErrorsSpan(
        IEnumerable<string> primaryErrors,
        IEnumerable<string> secondaryErrors,
        int primaryCount,
        int secondaryCount,
        int totalCount)
    {
        var errorArray = new string[totalCount];
        var position = 0;

        // Copy primary errors
        foreach (var error in primaryErrors)
        {
            errorArray[position++] = error;
        }

        // Copy secondary errors
        foreach (var error in secondaryErrors)
        {
            errorArray[position++] = error;
        }

        return WithFailure(errorArray);
    }

    /// <summary>
    /// Fallback implementation for large collections using List&lt;string&gt;.
    /// </summary>
    /// <param name="primaryErrors">Primary error collection.</param>
    /// <param name="secondaryErrors">Secondary error collection.</param>
    /// <returns>A failed Result with combined errors.</returns>
    private static Result CombineErrorsFallback(IEnumerable<string> primaryErrors, IEnumerable<string> secondaryErrors)
    {
        var errorList = new List<string>();

        errorList.AddRange(primaryErrors);
        errorList.AddRange(secondaryErrors);

        return errorList.Count > 0
            ? WithFailure(errorList)
            : WithFailure(ResultConstants.NoErrorsFoundMessage);
    }
}

/// <summary>
/// Represents the result of an operation that returns a value, including success status, value, and error messages.
/// Use this class for operations that return a value and need to indicate success, failure, or warnings.
/// </summary>
public sealed class Result<T>
{
    // Note: Error messages are now centralized in ResultConstants class

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class for deserialization.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="errors">A collection of error messages.</param>
    /// <param name="value">The value returned by the operation.</param>
    [JsonConstructor]
    public Result(bool isSuccess, IEnumerable<string>? errors, T? value = default)
    {
        _isSuccess = isSuccess;
        var errorArray = errors?.ToArray() ?? Array.Empty<string>();
        _hasErrors = errorArray.Length > 0;
        Errors = errorArray;
        _value = value;

        // Validate state consistency after deserialization
        ValidateInternalState();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class with a list of errors.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="errors">A list of error messages.</param>
    /// <param name="value">The value returned by the operation.</param>
    public Result(bool isSuccess, List<string>? errors, T? value = default)
    {
        _isSuccess = isSuccess;
        var errorArray = errors?.ToArray() ?? Array.Empty<string>();
        _hasErrors = errorArray.Length > 0;
        Errors = errorArray;
        _value = value;
    }

    /// <summary>
    /// Validates the internal state consistency of the Result object.
    /// </summary>
    private void ValidateInternalState()
    {
        // Check for inconsistent states that could indicate deserialization issues
        var actualHasErrors = Errors?.Any() == true;

        if (_hasErrors != actualHasErrors)
        {
            // Log warning but don't throw - fix the inconsistency
            _hasErrors = actualHasErrors;
        }
    }

    /// <summary>
    /// Gets the value associated with the result, or null if the operation failed.
    /// </summary>
    public T? Value => _value;

    /// <summary>
    /// Gets the data associated with the result (alias for Value).
    /// </summary>
    public T? Data => _value;

    private readonly T? _value;
    private readonly bool _isSuccess;
    private bool _hasErrors; // Made non-readonly to allow validation fixes

    /// <summary>
    /// Gets a value indicating whether the result is a success.
    /// A result is considered successful if it was explicitly marked as successful
    /// (warnings do not affect success status - they are just diagnostic information).
    /// </summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>
    /// Gets a value indicating whether the result has warnings or error messages.
    /// This includes both diagnostic warnings (for successful operations) and error messages (for failures).
    /// </summary>
    public bool HasErrors => _hasErrors;

    /// <summary>
    /// Gets a value indicating whether the result has warnings (i.e., is successful but contains diagnostic messages).
    /// </summary>
    public bool HasWarnings => _isSuccess && _hasErrors;

    /// <summary>
    /// Gets a value indicating whether the result is recoverable (successful operations, even with warnings).
    /// </summary>
    public bool IsRecoverable => _isSuccess;

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => !_isSuccess;

    /// <summary>
    /// Gets the collection of error messages associated with the result.
    /// </summary>
    public IEnumerable<string> Errors { get; init; }

    /// <summary>
    /// Gets the first non-empty error message, or null if none exist.
    /// </summary>
    public string? Error => Errors?.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e));

    /// <summary>
    /// Creates a successful result with the specified value.
    /// Follows industry standard Result&lt;T&gt; pattern: null values are valid success results when T is nullable.
    /// </summary>
    /// <param name="data">The value associated with the result.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance.</returns>
    public static Result<T> Success(T data)
    {
        return new Result<T>(true, Array.Empty<string>(), data);
    }

    /// <summary>
    /// Creates a successful result with the specified value (alias for Success).
    /// Follows industry standard Result&lt;T&gt; pattern: null values are valid success results when T is nullable.
    /// </summary>
    /// <param name="data">The value associated with the result.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance.</returns>
    public static Result<T> WithSuccess(T data)
    {
        return new Result<T>(true, Array.Empty<string>(), data);
    }

    /// <summary>
    /// Returns a string representation of the result, showing either the value or the list of errors.
    /// </summary>
    public override string ToString()
    {
        return _isSuccess
            ? $"{ResultConstants.SuccessPrefix}: {Value?.ToString()}"
            : Result.FormatErrorsString(Errors, ResultConstants.FailurePrefix);
    }

    /// <summary>
    /// Creates a failed result with the specified errors and optional value.
    /// </summary>
    /// <param name="errors">The collection of error messages.</param>
    /// <param name="value">The value to associate with the result (optional).</param>
    /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
    public static Result<T> WithFailure(IEnumerable<string>? errors, T? value = default)
    {
        // Use the provided errors or fall back to the default error message
        var errorArray = errors?.ToArray();
        if (errorArray is null || errorArray.Length == 0)
        {
            errorArray = [ResultConstants.DefaultErrorMessage];
        }
        return new Result<T>(false, errorArray, value);
    }

    /// <summary>
    /// Creates a successful result with warnings (non-fatal diagnostics).
    /// </summary>
    /// <param name="warnings">The collection of warning messages.</param>
    /// <param name="value">The value to associate with the result.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance with warnings.</returns>
    public static Result<T> WithWarnings(IEnumerable<string> warnings, T value)
    {
        var warningArray = warnings?.ToArray();
        if (warningArray is null || warningArray.Length == 0)
        {
            warningArray = [ResultConstants.DefaultWarningMessage];
        }
        return new Result<T>(true, warningArray, value);
    }

    /// <summary>
    /// Creates a failed result with the specified errors and optional value (overload for string array).
    /// </summary>
    /// <param name="errors">The array of error messages.</param>
    /// <param name="value">The value to associate with the result (optional).</param>
    /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
    public static Result<T> WithFailure(string[] errors, T? value = default)
    {
        // Check for empty array and provide default error message (consistent with IEnumerable overload)
        if (errors is null || errors.Length == 0)
        {
            errors = [ResultConstants.DefaultErrorMessage];
        }
        return new Result<T>(false, errors, value);
    }

    /// <summary>
    /// Creates a failed result with a single error message and optional value.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="value">The value to associate with the result (optional).</param>
    /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
    public static Result<T> WithFailure(string error, T? value = default)
    {
        return new Result<T>(false, [error], value);
    }

    /// <summary>
    /// Implicitly converts a value of type T to a successful <see cref="Result{T}"/>.
    /// Follows industry standard Result&lt;T&gt; pattern: null values are valid success results when T is nullable.
    /// </summary>
    /// <param name="data">The value to convert.</param>
    public static implicit operator Result<T>(T data)
    {
        return Success(data);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Result{T}"/> to a non-generic <see cref="Result"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result(Result<T> result)
    {
        return result._isSuccess ? Result.Success() : Result.WithFailure(result.Errors ?? [ResultConstants.DefaultErrorMessage]);
    }

    /// <summary>
    /// Deconstructs the result into its success state, value, and errors.
    /// </summary>
    /// <param name="succeeded">Indicates whether the operation succeeded.</param>
    /// <param name="data">The value returned by the operation.</param>
    /// <param name="errors">The collection of error messages.</param>
    public void Deconstruct(out bool succeeded, out T? data, out IEnumerable<string> errors)
    {
        succeeded = _isSuccess;
        data = Value;
        errors = Errors ?? [];
    }

    /// <summary>
    /// Executes the specified action if the result is successful.
    /// Follows industry standard Result&lt;T&gt; pattern: executes for all successful results regardless of null values.
    /// </summary>
    /// <param name="action">The action to execute on success, receiving the value.</param>
    /// <returns>The current <see cref="Result{T}"/> instance.</returns>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (_isSuccess)
        {
            action(Value);
        }
        return this;
    }

    /// <summary>
    /// Executes the specified action if the result is a failure.
    /// </summary>
    /// <param name="action">The action to execute on failure, receiving the error messages.</param>
    /// <returns>The current <see cref="Result{T}"/> instance.</returns>
    public Result<T> OnFailure(Action<IEnumerable<string>> action)
    {
        if (IsFailure)
        {
            if (Errors is not null && _hasErrors)
            {
                action(Errors);
            }
            else
            {
                action([ResultConstants.DefaultErrorMessage]);
            }
        }
        return this;
    }

    /// <summary>
    /// Maps a successful result to a <see cref="Result{TOut}"/> using the provided function, or propagates errors.
    /// Follows industry standard Result&lt;T&gt; pattern: maps successful results regardless of null values.
    /// </summary>
    /// <typeparam name="TOut">The type of the value to return on success.</typeparam>
    /// <param name="func">The function to execute on success.</param>
    /// <returns>A <see cref="Result{TOut}"/> representing the outcome.</returns>
    public Result<TOut> Map<TOut>(Func<T, TOut> func)
    {
        return _isSuccess ? Result<TOut>.Success(func(Value)) : Result<TOut>.WithFailure(Errors);
    }

    /// <summary>
    /// Binds a successful result to another <see cref="Result{TOut}"/> using the provided function, or propagates errors.
    /// Follows industry standard Result&lt;T&gt; pattern: binds successful results regardless of null values.
    /// </summary>
    /// <typeparam name="TOut">The type of the value to return on success.</typeparam>
    /// <param name="func">The function to execute on success.</param>
    /// <returns>A <see cref="Result{TOut}"/> representing the outcome.</returns>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func)
    {
        return _isSuccess ? func(Value) : Result<TOut>.WithFailure(Errors);
    }

    /// <summary>
    /// Ensures a condition is met for a successful result, otherwise returns a failure with the specified error message.
    /// </summary>
    /// <param name="condition">The condition to check, receiving the value.</param>
    /// <param name="errorMessage">The error message if the condition fails.</param>
    /// <returns>A <see cref="Result{T}"/> representing the outcome.</returns>
    public Result<T> Ensure(Func<T, bool> condition, string errorMessage)
    {
        if (!_isSuccess)
            return this;

        if (Value is null)
        {
            return WithFailure(ResultConstants.ConditionEvaluationWithNullValue);
        }

        if (!condition(Value))
        {
            return WithFailure(errorMessage);
        }

        return this;
    }

    /// <summary>
    /// Executes the specified action if the result is successful, returning the current result.
    /// Follows industry standard Result&lt;T&gt; pattern: executes for all successful results regardless of null values.
    /// </summary>
    /// <param name="action">The action to execute on success, receiving the value.</param>
    /// <returns>The current <see cref="Result{T}"/> instance.</returns>
    public Result<T> Tap(Action<T> action)
    {
        if (_isSuccess)
        {
            action(Value);
        }
        return this;
    }

    /// <summary>
    /// Combines multiple non-generic results, aggregating all errors. Returns success if all are successful.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A <see cref="Result{T}"/> representing the combined outcome, with the current value if successful.</returns>
    public Result<T> Combine(params Result[] results)
    {
        if (results is null || results.Length == 0)
            return this;

        var errorList = new List<string>();

        // Add current result's errors if it's a failure
        if (IsFailure && Errors is not null)
        {
            errorList.AddRange(Errors);
        }

        // Add errors from all failed results
        foreach (var result in results)
        {
            if (result.IsFailure && result.Errors is not null)
            {
                errorList.AddRange(result.Errors);
            }
        }

        if (errorList.Count > 0)
        {
            return Result<T>.WithFailure(errorList);
        }

        // All operations succeeded - return the current successful result (null values are valid)
        return _isSuccess
            ? Result<T>.Success(Value)
            : this;
    }

    /// <summary>
    /// Matches the result to either a success or failure function.
    /// Follows industry standard Result&lt;T&gt; pattern: null values are treated as valid success values.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute on success, receiving the value.</param>
    /// <param name="onFailure">Function to execute on failure, receiving the errors.</param>
    /// <returns>The result of the executed function.</returns>
    public Result<TOut> Match<TOut>(Func<T, TOut> onSuccess, Func<IEnumerable<string>, TOut> onFailure)
    {
        if (!_isSuccess)
        {
            return onFailure(Errors ?? [ResultConstants.DefaultErrorMessage]);
        }

        // Industry standard: null values are valid success values when T is nullable
        return onSuccess(Value);
    }

    /// <summary>
    /// Recovers from a failure by executing the provided recovery function.
    /// </summary>
    /// <param name="recoverFunc">The function to execute on failure.</param>
    /// <returns>The recovered or original <see cref="Result{T}"/>.</returns>
    public Result<T> Recover(Func<Result<T>> recoverFunc)
    {
        return IsFailure ? recoverFunc() : this;
    }

    /// <summary>
    /// Recovers from a failure by executing the provided recovery function, returning a result of a different type.
    /// </summary>
    /// <typeparam name="TOut">The type of the value to return on recovery.</typeparam>
    /// <param name="recoverFunc">The function to execute on failure.</param>
    /// <returns>The recovered <see cref="Result{TOut}"/> or a successful result with the current value.</returns>
    /// <remarks>
    /// BUG FIX (2025-01-03): Previously used dangerous cast (TOut)(object)Value! which could throw
    /// InvalidCastException at runtime. Now properly handles type conversion with validation.
    /// This method should only be used when T and TOut are compatible types.
    /// </remarks>
    public Result<TOut> RecoverWith<TOut>(Func<Result<TOut>> recoverFunc)
    {
        if (IsFailure)
        {
            return recoverFunc();
        }

        // Safe type conversion: only proceed if Value can be safely converted to TOut
        if (Value is TOut convertedValue)
        {
            return Result<TOut>.Success(convertedValue);
        }

        // If types are incompatible, return a failure instead of throwing
        return Result<TOut>.WithFailure(string.Format(ResultConstants.RecoverWithTypeConversionError, typeof(T).Name, typeof(TOut).Name));
    }

    /// <summary>
    /// Combines two sets of errors into a single failed result of type Result&lt;TOut&gt;, or a default failure if both are empty.
    /// </summary>
    /// <typeparam name="TOut">The type of the value to associate with the result.</typeparam>
    /// <param name="primaryErrors">The primary error messages.</param>
    /// <param name="secondaryErrors">The secondary error messages.</param>
    /// <param name="value">The value to associate with the result (optional).</param>
    /// <returns>A failed result with all errors.</returns>
    public static Result<TOut> CombineErrors<TOut>(IEnumerable<string>? primaryErrors, IEnumerable<string>? secondaryErrors, TOut? value = default)
    {
        var errorList = new List<string>();

        if (primaryErrors is not null)
        {
            errorList.AddRange(primaryErrors);
        }

        if (secondaryErrors is not null)
        {
            errorList.AddRange(secondaryErrors);
        }

        return errorList.Count > 0
            ? Result<TOut>.WithFailure(errorList, value)
            : Result<TOut>.WithFailure(ResultConstants.NoErrorsFoundMessage, value);
    }
}