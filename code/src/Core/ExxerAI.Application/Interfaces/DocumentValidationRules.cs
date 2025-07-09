namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Enhanced document validation rules for KpiExxerpro pattern processing
/// </summary>
public class DocumentValidationRules
{
    /// <summary>
    /// Gets or sets field-specific validation rules
    /// </summary>
    public Dictionary<string, List<DocumentValidationRule>> FieldRules { get; set; } = new();

    /// <summary>
    /// Gets or sets minimum confidence threshold
    /// </summary>
    public float MinimumConfidence { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets required fields that must be present
    /// </summary>
    public List<string> RequiredFields { get; set; } = new();
}