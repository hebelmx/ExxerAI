namespace ExxerAI.CLI.Tests;

/// <summary>
/// Proposed Rule: Null Argument Validation Pattern
///
/// For constructors and methods that receive null arguments:
/// 1. Constructors: Accept nulls, validate at execution time
/// 2. Methods: Return Result<T> with NullArgumentError containing:
///    - Parameter name(s) that were null
///    - Descriptive error message
///    - List of all offending parameters if multiple
///
/// Example:
/// public Result<Agent> CreateAgent(string name, AgentCapabilities capabilities)
/// {
///     var nullParams = new List<string>();
///     if (name == null) nullParams.Add(nameof(name));
///     if (capabilities == null) nullParams.Add(nameof(capabilities));
///
///     if (nullParams.Any())
///         return Result<Agent>.WithFailure($"Null arguments: {string.Join(", ", nullParams)}");
///
///     // ... rest of method
/// }
/// </summary>
public static class ValidationRuleDocumentation
{
    public const string NullArgumentValidationRule = @"
		RULE: Null Argument Validation Pattern

		1. Never throw ArgumentNullException
		2. Return Result<T> with validation errors
		3. Include parameter name(s) in error message
		4. Support multiple null parameter reporting
		5. Validate at method entry, not constructor
	";
}