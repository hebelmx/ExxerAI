namespace ExxerAI.Domain.Helpers;

/// <summary>
/// Represents the result of an operation, including success status and error messages.
/// Use this class for operations that do not return a value but need to indicate success or failure.
/// </summary>
public class Result
{
    // Note: Error messages are now centralized in ResultConstants class

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class with the specified success state and errors.
    /// </summary>
    /// <param name="succeeded">Indicates whether the operation succeeded.</param>
    /// <param name="errors">A collection of error messages.</param>
    private Result(bool succeeded, IEnumerable<string> errors)
    {
        IsSuccess = succeeded && (errors?.Any() != true);
        Errors = errors?.ToArray() ?? Array.Empty<string>();
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
        return IsSuccess ? ResultConstants.SuccessPrefix : ListOfErrorsString();
    }

    /// <summary>
    /// Returns a formatted string containing all error messages.
    /// </summary>
    private string ListOfErrorsString()
    {
        var stringBuilder = new StringBuilder($"{ResultConstants.FailurePrefix}: ");
        foreach (var error in Errors)
        {
            stringBuilder.Append(error);
            stringBuilder.Append(", ");
        }
        // Remove the trailing comma and space, if any
        var prefixLength = ResultConstants.FailurePrefix.Length + 2; // +2 for ": "
        if (stringBuilder.Length > prefixLength)
        {
            stringBuilder.Length -= 2; // Remove the last ", "
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
        return new Result(false, errors);
    }

    /// <summary>
    /// Creates a failed result with the specified errors (overload for string array).
    /// </summary>
    /// <param name="errors">The array of error messages.</param>
    /// <returns>A failed <see cref="Result"/> instance.</returns>
    public static Result WithFailure(string[] errors)
    {
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
        var allErrors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList();
        return allErrors.Any() ? WithFailure(allErrors) : Success();
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
    /// </summary>
    /// <param name="primaryErrors">The primary error messages.</param>
    /// <param name="secondaryErrors">The secondary error messages.</param>
    /// <returns>A failed <see cref="Result"/> with all errors.</returns>
    public static Result CombineErrors(IEnumerable<string>? primaryErrors, IEnumerable<string>? secondaryErrors)
    {
        // Combine non-null lists, or return a failure message if both are null.
        var combinedErrors = (primaryErrors ?? [])
            .Concat(secondaryErrors ?? [])
            .ToList();
        return combinedErrors.Any()
            ? WithFailure(combinedErrors)
            : WithFailure(ResultConstants.NoErrorsFoundMessage);
    }
}

/// <summary>
/// Represents the result of an operation that returns a value, including success status, value, and error messages.
/// Use this class for operations that return a value and need to indicate success, failure, or warnings.
/// </summary>
public class Result<T>
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
        _hasErrors = errors?.Any() == true;
        Errors = errors?.ToArray() ?? Array.Empty<string>();
        _value = value;
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
        _hasErrors = errors?.Any() == true;
        Errors = errors?.ToArray() ?? Array.Empty<string>();
        _value = value;
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
    private readonly bool _hasErrors;

    /// <summary>
    /// Gets a value indicating whether the result is a success.
    /// A result is considered successful if it was explicitly marked as successful 
    /// and does not contain any error messages (warnings are allowed for successful results).
    /// </summary>
    public bool IsSuccess => _isSuccess && !_hasErrors;

    /// <summary>
    /// Gets a value indicating whether the result has warnings (i.e., is successful but contains error messages).
    /// </summary>
    public bool HasWarnings => _isSuccess && _hasErrors;

    /// <summary>
    /// Gets a value indicating whether the result is recoverable (either successful or has warnings).
    /// </summary>
    public bool IsRecoverable => _isSuccess; // Recoverable if operation succeeded, regardless of warnings

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
    /// </summary>
    /// <param name="data">The value associated with the result.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance.</returns>
    public static Result<T> Success(T data)
    {
        return new Result<T>(true, Array.Empty<string>(), data);
    }

    /// <summary>
    /// Creates a successful result with the specified value (alias for Success).
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
        return _isSuccess ? $"{ResultConstants.SuccessPrefix}: {Value?.ToString()}" : ListOfErrorsString() ?? ResultConstants.DefaultFailureString;
    }

    /// <summary>
    /// Returns a formatted string containing all error messages.
    /// </summary>
    private string? ListOfErrorsString()
    {
        if (Errors is null || !Errors.Any()) return default;
        var stringBuilder = new StringBuilder($"{ResultConstants.FailurePrefix}: ");
        foreach (var error in Errors)
        {
            stringBuilder.Append(error);
            stringBuilder.Append(", ");
        }
        // Remove the trailing comma and space, if any
        var prefixLength = ResultConstants.FailurePrefix.Length + 2; // +2 for ": "
        if (stringBuilder.Length > prefixLength)
        {
            stringBuilder.Length -= 2; // Remove the last ", "
        }
        return stringBuilder.ToString();
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
        var errorList = errors?.ToList() ?? [ResultConstants.DefaultErrorMessage];
        return new Result<T>(false, errorList, value);
    }

    /// <summary>
    /// Creates a successful result with warnings (non-fatal diagnostics).
    /// </summary>
    /// <param name="warnings">The collection of warning messages.</param>
    /// <param name="value">The value to associate with the result.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance with warnings.</returns>
    public static Result<T> WithWarnings(IEnumerable<string> warnings, T value)
    {
        var warningList = warnings?.ToList() ?? [ResultConstants.DefaultWarningMessage];
        return new Result<T>(true, warningList, value);
    }

    /// <summary>
    /// Creates a failed result with the specified errors and optional value (overload for string array).
    /// </summary>
    /// <param name="errors">The array of error messages.</param>
    /// <param name="value">The value to associate with the result (optional).</param>
    /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
    public static Result<T> WithFailure(string[] errors, T? value = default)
    {
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
    /// </summary>
    /// <param name="action">The action to execute on success, receiving the value.</param>
    /// <returns>The current <see cref="Result{T}"/> instance.</returns>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (_isSuccess && Value is not null)
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
            if (Errors is not null && Errors.Any())
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
    /// </summary>
    /// <typeparam name="TOut">The type of the value to return on success.</typeparam>
    /// <param name="func">The function to execute on success.</param>
    /// <returns>A <see cref="Result{TOut}"/> representing the outcome.</returns>
    public Result<TOut> Map<TOut>(Func<T, TOut> func)
    {
        return _isSuccess && Value is not null ? Result<TOut>.Success(func(Value)) : Result<TOut>.WithFailure(Errors);
    }

    /// <summary>
    /// Binds a successful result to another <see cref="Result{TOut}"/> using the provided function, or propagates errors.
    /// </summary>
    /// <typeparam name="TOut">The type of the value to return on success.</typeparam>
    /// <param name="func">The function to execute on success.</param>
    /// <returns>A <see cref="Result{TOut}"/> representing the outcome.</returns>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func)
    {
        return _isSuccess && Value is not null ? func(Value) : Result<TOut>.WithFailure(Errors);
    }

    /// <summary>
    /// Ensures a condition is met for a successful result, otherwise returns a failure with the specified error message.
    /// </summary>
    /// <param name="condition">The condition to check, receiving the value.</param>
    /// <param name="errorMessage">The error message if the condition fails.</param>
    /// <returns>A <see cref="Result{T}"/> representing the outcome.</returns>
    public Result<T> Ensure(Func<T, bool> condition, string errorMessage)
    {
        if (_isSuccess && !condition(Value!))
        {
            return WithFailure(errorMessage);
        }
        return this;
    }

    /// <summary>
    /// Executes the specified action if the result is successful, returning the current result.
    /// </summary>
    /// <param name="action">The action to execute on success, receiving the value.</param>
    /// <returns>The current <see cref="Result{T}"/> instance.</returns>
    public Result<T> Tap(Action<T> action)
    {
        if (_isSuccess && Value is not null)
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
        var allErrors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList();
        return allErrors.Any() ? Result<T>.WithFailure(allErrors) : Result<T>.Success(this.Value!);
    }

    /// <summary>
    /// Matches the result to either a success or failure function.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute on success, receiving the value.</param>
    /// <param name="onFailure">Function to execute on failure, receiving the errors.</param>
    /// <returns>The result of the executed function.</returns>
    public Result<TOut> Match<TOut>(Func<T, TOut> onSuccess, Func<IEnumerable<string>, TOut> onFailure)
    {
        return _isSuccess ? onSuccess(Value!) : onFailure(Errors ?? [ResultConstants.DefaultErrorMessage]);
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
        // Combine non-null lists, or return a failure message if both are null.
        var combinedErrors = (primaryErrors ?? [])
            .Concat(secondaryErrors ?? [])
            .ToList();
        return combinedErrors.Any()
            ? Result<TOut>.WithFailure(combinedErrors, value)
            : Result<TOut>.WithFailure(ResultConstants.NoErrorsFoundMessage, value);
    }
}