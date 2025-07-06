namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents quality metrics for a specific field
/// </summary>
public class FieldQualityMetrics
{
    /// <summary>
    /// Gets or sets the field name
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the completeness rate for this field
    /// </summary>
    public float CompletenessRate { get; set; }

    /// <summary>
    /// Gets or sets the accuracy rate for this field
    /// </summary>
    public float AccuracyRate { get; set; }

    /// <summary>
    /// Gets or sets the number of validation errors for this field
    /// </summary>
    public int ValidationErrors { get; set; }
}