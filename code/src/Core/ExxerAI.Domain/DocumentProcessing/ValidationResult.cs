namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets whether the validation passed
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Gets or sets the validation errors
    /// </summary>
    public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the validation warnings
    /// </summary>
    public IEnumerable<string> Warnings { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets when the validation was performed
    /// </summary>
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the validation confidence score (0.0 - 1.0)
    /// </summary>
    public float ConfidenceScore { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets additional validation metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    /// <returns>A valid ValidationResult</returns>
    public static ValidationResult Success()
    {
        return new ValidationResult
        {
            IsValid = true,
            Errors = Array.Empty<string>(),
            Warnings = Array.Empty<string>(),
            ValidatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed validation result with errors
    /// </summary>
    /// <param name="errors">The validation errors</param>
    /// <returns>An invalid ValidationResult</returns>
    public static ValidationResult WithErrors(IEnumerable<string> errors)
    {
        return new ValidationResult
        {
            IsValid = false,
            Errors = errors,
            Warnings = Array.Empty<string>(),
            ValidatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a validation result with warnings
    /// </summary>
    /// <param name="warnings">The validation warnings</param>
    /// <returns>A valid ValidationResult with warnings</returns>
    public static ValidationResult WithWarnings(IEnumerable<string> warnings)
    {
        return new ValidationResult
        {
            IsValid = true,
            Errors = Array.Empty<string>(),
            Warnings = warnings,
            ValidatedAt = DateTime.UtcNow
        };
    }
} 