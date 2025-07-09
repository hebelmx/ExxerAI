namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a field definition within a document schema
/// </summary>
public class FieldDefinition
{
    /// <summary>
    /// Gets or sets the field name
    /// </summary>
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field type
    /// </summary>
    public FieldType Type { get; set; } = FieldType.Text;

    /// <summary>
    /// Gets or sets whether this field is required
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets the primary extraction pattern for this field
    /// </summary>
    public string PrimaryPattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets alternative extraction patterns
    /// </summary>
    public List<ExtractionPattern> AlternativePatterns { get; init; } = [];

    /// <summary>
    /// Gets or sets the validation rules for this field
    /// </summary>
    public List<ValidationRule> ValidationRules { get; init; } = [];

    /// <summary>
    /// Gets or sets the field description
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets example values for this field
    /// </summary>
    public List<string> ExampleValues { get; init; } = [];

    /// <summary>
    /// Gets or sets the confidence threshold for this field
    /// </summary>
    public float ConfidenceThreshold { get; set; } = 0.8f;

    /// <summary>
    /// Initializes a new instance of the FieldDefinition class
    /// </summary>
    public FieldDefinition() { }

    /// <summary>
    /// Initializes a new instance of the FieldDefinition class with basic properties
    /// </summary>
    /// <param name="name">The field name</param>
    /// <param name="type">The field type</param>
    /// <param name="isRequired">Whether the field is required</param>
    /// <param name="primaryPattern">The primary extraction pattern</param>
    public FieldDefinition(string name, FieldType type, bool isRequired, string primaryPattern)
    {
        Name = name;
        Type = type;
        IsRequired = isRequired;
        PrimaryPattern = primaryPattern;
    }
}