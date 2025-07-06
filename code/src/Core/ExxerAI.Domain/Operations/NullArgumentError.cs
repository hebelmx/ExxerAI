namespace ExxerAI.Domain.Operations;

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