namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents extraction statistics for a specific field
/// </summary>
public class FieldStatistics
{
    /// <summary>
    /// Gets or sets the field name
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extraction success rate for this field
    /// </summary>
    public float SuccessRate { get; set; }

    /// <summary>
    /// Gets or sets the average confidence score for this field
    /// </summary>
    public float AverageConfidence { get; set; }

    /// <summary>
    /// Gets or sets the most common extraction patterns used
    /// </summary>
    public List<string> CommonPatterns { get; init; } = new();
}