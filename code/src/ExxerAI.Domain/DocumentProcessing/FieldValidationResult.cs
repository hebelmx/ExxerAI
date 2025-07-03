namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents validation results for a specific field
/// </summary>
public class FieldValidationResult
{
    /// <summary>
    /// Gets or sets whether the field passed validation
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Gets or sets the field-specific error message
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the confidence score for this field validation
    /// </summary>
    public float Confidence { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the suggested correction for invalid fields
    /// </summary>
    public string? SuggestedCorrection { get; set; }
}