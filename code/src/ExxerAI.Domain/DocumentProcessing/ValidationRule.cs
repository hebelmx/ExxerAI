namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a validation rule for field extraction
/// </summary>
public class ValidationRule
{
    /// <summary>
    /// Gets or sets the validation rule identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the rule name
    /// </summary>
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field name this rule applies to
    /// </summary>
    [StringLength(255)]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the validation type
    /// </summary>
    public ValidationType Type { get; set; } = ValidationType.Required;

    /// <summary>
    /// Gets or sets the validation pattern or constraint
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message for validation failures
    /// </summary>
    [StringLength(500)]
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this rule is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets additional validation parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the ValidationRule class.
    /// </summary>
    public ValidationRule() { }

    /// <summary>
    /// Initializes a new instance of the ValidationRule class with basic properties.
    /// </summary>
    /// <param name="fieldName">The field name this rule applies to.</param>
    /// <param name="type">The validation type.</param>
    /// <param name="pattern">The validation pattern or constraint.</param>
    /// <param name="errorMessage">The error message for validation failures.</param>
    public ValidationRule(string fieldName, ValidationType type, string pattern, string errorMessage)
    {
        FieldName = fieldName;
        Type = type;
        Pattern = pattern;
        ErrorMessage = errorMessage;
        Name = $"{fieldName}_{type}";
    }

    /// <summary>
    /// Validates a field value against this rule.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if the value passes validation, false otherwise.</returns>
    public bool Validate(object? value)
    {
        if (!IsActive) return true;

        var stringValue = value?.ToString() ?? string.Empty;

        return Type switch
        {
            ValidationType.Required => ValidateRequired(stringValue),
            ValidationType.RegexPattern => ValidateRegexPattern(stringValue),
            ValidationType.NumericRange => ValidateNumericRange(stringValue),
            ValidationType.DateFormat => ValidateDateFormat(stringValue),
            ValidationType.LengthLimit => ValidateLengthLimit(stringValue),
            ValidationType.Custom => ValidateCustom(value),
            _ => true
        };
    }

    /// <summary>
    /// Validates that a required field has a value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if the value is not null or empty.</returns>
    private bool ValidateRequired(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Validates that a value matches a regex pattern.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if the value matches the pattern.</returns>
    private bool ValidateRegexPattern(string value)
    {
        if (string.IsNullOrEmpty(Pattern)) return true;

        try
        {
            var regex = new Regex(Pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(value);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that a numeric value is within a specified range.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if the value is within the range.</returns>
    private bool ValidateNumericRange(string value)
    {
        if (!Parameters.TryGetValue("MinValue", out var minObj) ||
            !Parameters.TryGetValue("MaxValue", out var maxObj))
        {
            return true; // No range specified
        }

        if (!decimal.TryParse(value, out var numericValue) ||
            !decimal.TryParse(minObj.ToString(), out var minValue) ||
            !decimal.TryParse(maxObj.ToString(), out var maxValue))
        {
            return false;
        }

        return numericValue >= minValue && numericValue <= maxValue;
    }

    /// <summary>
    /// Validates that a value is a valid date format.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if the value is a valid date.</returns>
    private bool ValidateDateFormat(string value)
    {
        if (string.IsNullOrEmpty(Pattern))
        {
            return DateTime.TryParse(value, out _);
        }

        // Try to parse with specific format
        return DateTime.TryParseExact(value, Pattern, null, System.Globalization.DateTimeStyles.None, out _);
    }

    /// <summary>
    /// Validates that a value meets length requirements.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if the value meets length requirements.</returns>
    private bool ValidateLengthLimit(string value)
    {
        var length = value?.Length ?? 0;

        if (Parameters.TryGetValue("MinLength", out var minObj) &&
            int.TryParse(minObj.ToString(), out var minLength) &&
            length < minLength)
        {
            return false;
        }

        if (Parameters.TryGetValue("MaxLength", out var maxObj) &&
            int.TryParse(maxObj.ToString(), out var maxLength) &&
            length > maxLength)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Validates using custom logic defined in the Pattern.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if the value passes custom validation.</returns>
    private bool ValidateCustom(object? value)
    {
        // For custom validation, we could implement a scripting engine
        // or use reflection to call custom validation methods
        // For now, always return true for custom validations
        return true;
    }

    /// <summary>
    /// Creates a required field validation rule.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A new required validation rule.</returns>
    public static ValidationRule Required(string fieldName, string? errorMessage = null)
    {
        return new ValidationRule(
            fieldName,
            ValidationType.Required,
            string.Empty,
            errorMessage ?? $"Field '{fieldName}' is required");
    }

    /// <summary>
    /// Creates a regex pattern validation rule.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="pattern">The regex pattern.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A new regex validation rule.</returns>
    public static ValidationRule RegexPattern(string fieldName, string pattern, string? errorMessage = null)
    {
        return new ValidationRule(
            fieldName,
            ValidationType.RegexPattern,
            pattern,
            errorMessage ?? $"Field '{fieldName}' does not match the required pattern");
    }

    /// <summary>
    /// Creates a numeric range validation rule.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A new numeric range validation rule.</returns>
    public static ValidationRule NumericRange(string fieldName, decimal minValue, decimal maxValue, string? errorMessage = null)
    {
        var rule = new ValidationRule(
            fieldName,
            ValidationType.NumericRange,
            string.Empty,
            errorMessage ?? $"Field '{fieldName}' must be between {minValue} and {maxValue}");

        rule.Parameters["MinValue"] = minValue;
        rule.Parameters["MaxValue"] = maxValue;

        return rule;
    }

    /// <summary>
    /// Creates a length limit validation rule.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="minLength">The minimum length.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A new length validation rule.</returns>
    public static ValidationRule LengthLimit(string fieldName, int? minLength = null, int? maxLength = null, string? errorMessage = null)
    {
        var rule = new ValidationRule(
            fieldName,
            ValidationType.LengthLimit,
            string.Empty,
            errorMessage ?? $"Field '{fieldName}' length is invalid");

        if (minLength.HasValue) rule.Parameters["MinLength"] = minLength.Value;
        if (maxLength.HasValue) rule.Parameters["MaxLength"] = maxLength.Value;

        return rule;
    }

    /// <summary>
    /// Returns a string representation of this validation rule.
    /// </summary>
    /// <returns>A formatted string with rule information.</returns>
    public override string ToString()
    {
        return $"ValidationRule[{FieldName}, {Type}, Active: {IsActive}]";
    }
}