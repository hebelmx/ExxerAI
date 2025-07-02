namespace ExxerAI.Domain;

/// <summary>
/// Represents a null argument validation error
/// </summary>
public class NullArgumentError
{
	/// <summary>
	/// Name of the parameter that was null
	/// </summary>
	public string ParameterName { get; }

	/// <summary>
	/// Optional message describing the error
	/// </summary>
	public string? Message { get; }

	/// <summary>
	/// Initializes a new instance of NullArgumentError
	/// </summary>
	/// <param name="parameterName">Name of the null parameter</param>
	/// <param name="message">Optional error message</param>
	public NullArgumentError(string parameterName, string? message = null)
	{
		ParameterName = parameterName;
		Message = message ?? $"Parameter '{parameterName}' cannot be null";
	}

	/// <summary>
	/// Returns string representation of the error
	/// </summary>
	public override string ToString() => Message ?? $"Parameter '{ParameterName}' cannot be null";
}

/// <summary>
/// Represents multiple null argument validation errors
/// </summary>
public class MultipleNullArgumentsError
{
	/// <summary>
	/// Collection of null argument errors
	/// </summary>
	public IReadOnlyList<NullArgumentError> Errors { get; }

	/// <summary>
	/// Initializes a new instance with multiple null argument errors
	/// </summary>
	/// <param name="errors">Collection of null argument errors</param>
	public MultipleNullArgumentsError(IEnumerable<NullArgumentError> errors)
	{
		Errors = errors.ToList().AsReadOnly();
	}

	/// <summary>
	/// Initializes a new instance with parameter names
	/// </summary>
	/// <param name="parameterNames">Names of null parameters</param>
	public MultipleNullArgumentsError(params string[] parameterNames)
	{
		Errors = parameterNames.Select(name => new NullArgumentError(name)).ToList().AsReadOnly();
	}

	/// <summary>
	/// Returns string representation of all errors
	/// </summary>
	public override string ToString() => 
		$"Multiple null parameters: {string.Join(", ", Errors.Select(e => e.ParameterName))}";
}

/// <summary>
/// Static factory for creating null argument validation results
/// </summary>
public static class NullArgumentValidation
{
	/// <summary>
	/// Creates a failure result for a single null argument
	/// </summary>
	/// <typeparam name="T">Result type</typeparam>
	/// <param name="parameterName">Name of the null parameter</param>
	/// <param name="message">Optional error message</param>
	/// <returns>Failed result with null argument error</returns>
	public static Result<T> Failure<T>(string parameterName, string? message = null)
	{
		var error = new NullArgumentError(parameterName, message);
		return Result<T>.WithFailure(error.ToString());
	}

	/// <summary>
	/// Creates a failure result for multiple null arguments
	/// </summary>
	/// <typeparam name="T">Result type</typeparam>
	/// <param name="parameterNames">Names of null parameters</param>
	/// <returns>Failed result with multiple null argument errors</returns>
	public static Result<T> Failure<T>(params string[] parameterNames)
	{
		var error = new MultipleNullArgumentsError(parameterNames);
		return Result<T>.WithFailure(error.ToString());
	}

	/// <summary>
	/// Creates a non-generic failure result for a single null argument
	/// </summary>
	/// <param name="parameterName">Name of the null parameter</param>
	/// <param name="message">Optional error message</param>
	/// <returns>Failed result with null argument error</returns>
	public static Result Failure(string parameterName, string? message = null)
	{
		var error = new NullArgumentError(parameterName, message);
		return Result.WithFailure(error.ToString());
	}

	/// <summary>
	/// Creates a non-generic failure result for multiple null arguments
	/// </summary>
	/// <param name="parameterNames">Names of null parameters</param>
	/// <returns>Failed result with multiple null argument errors</returns>
	public static Result Failure(params string[] parameterNames)
	{
		var error = new MultipleNullArgumentsError(parameterNames);
		return Result.WithFailure(error.ToString());
	}
} 