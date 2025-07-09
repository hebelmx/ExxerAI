namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents quality requirements for a specific field.
/// </summary>
public class FieldQualityRequirement
{
    /// <summary>
    /// Gets or sets the minimum confidence for this field.
    /// </summary>
    public float MinimumConfidence { get; set; } = 0.8f;

    /// <summary>
    /// Gets or sets whether this field is required.
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum allowed length.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the minimum allowed length.
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    /// Gets or sets custom validation rules.
    /// </summary>
    public List<string> CustomValidationRules { get; set; } = new();
}