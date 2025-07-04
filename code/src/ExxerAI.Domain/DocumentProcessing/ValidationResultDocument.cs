namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents validation results for extracted data
/// </summary>
public class ValidationResultDocument
{
    /// <summary>
    /// Gets or sets whether the data passed validation
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of validation errors
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Gets or sets the list of validation warnings
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Gets or sets field-specific validation results
    /// </summary>
    public Dictionary<string, FieldValidationResult> FieldResults { get; init; } = new();

    /// <summary>
    /// Gets or sets the validation confidence score
    /// </summary>
    public float Confidence { get; set; } = 1.0f;
}