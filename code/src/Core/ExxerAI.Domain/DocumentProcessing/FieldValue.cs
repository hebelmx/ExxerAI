namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a field value with its confidence score and metadata
/// </summary>
public class FieldValue
{
    /// <summary>
    /// Gets or sets the extracted value
    /// </summary>
    public object Value { get; set; } = new();

    /// <summary>
    /// Gets or sets the confidence score for this field (0.0 to 1.0)
    /// </summary>
    public float Confidence { get; set; } = 0.0f;

    /// <summary>
    /// Gets or sets the data type of the extracted value
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extraction pattern that matched this field
    /// </summary>
    public string? ExtractedBy { get; set; }

    /// <summary>
    /// Gets or sets the source location where this field was found
    /// </summary>
    public string? SourceLocation { get; set; }

    /// <summary>
    /// Gets or sets the original text that was extracted
    /// </summary>
    public string? OriginalText { get; set; }

    /// <summary>
    /// Gets or sets whether this field value was validated
    /// </summary>
    public bool IsValidated { get; set; } = false;

    /// <summary>
    /// Gets the string representation of the value
    /// </summary>
    public string StringValue => Value?.ToString() ?? string.Empty;

    /// <summary>
    /// Gets whether this field has a valid value
    /// </summary>
    public bool HasValue => Value != null && !string.IsNullOrWhiteSpace(StringValue);

    /// <summary>
    /// Gets whether this field has high confidence (>= 0.8)
    /// </summary>
    public bool IsHighConfidence => Confidence >= 0.8f;
}