using System.ComponentModel.DataAnnotations;

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
}

/// <summary>
/// Represents the types of validation that can be performed
/// </summary>
public enum ValidationType
{
    /// <summary>
    /// Field is required
    /// </summary>
    Required,
    
    /// <summary>
    /// Field must match a regex pattern
    /// </summary>
    RegexPattern,
    
    /// <summary>
    /// Field must be within a numeric range
    /// </summary>
    NumericRange,
    
    /// <summary>
    /// Field must be a valid date
    /// </summary>
    DateFormat,
    
    /// <summary>
    /// Field must be within a specific length
    /// </summary>
    LengthLimit,
    
    /// <summary>
    /// Custom validation logic
    /// </summary>
    Custom
}

 