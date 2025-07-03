namespace ExxerAI.Domain.DocumentProcessing;

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