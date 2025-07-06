namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents the result of field extraction operations
/// </summary>
public class ExtractionResult
{
    /// <summary>
    /// Gets or sets the extracted fields with their values and confidence scores
    /// </summary>
    public Dictionary<string, FieldValue> ExtractedFields { get; set; } = new();

    /// <summary>
    /// Gets or sets the overall extraction confidence score (0.0 to 1.0)
    /// </summary>
    public float Confidence { get; set; } = 0.0f;

    /// <summary>
    /// Gets or sets the extraction method used
    /// </summary>
    public ExtractionMethod Method { get; set; } = ExtractionMethod.DirectText;

    /// <summary>
    /// Gets or sets any warnings or issues encountered during extraction
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets or sets the schema used for extraction
    /// </summary>
    public string? SchemaId { get; set; }

    /// <summary>
    /// Gets or sets the processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when extraction was performed
    /// </summary>
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets whether the extraction was successful
    /// </summary>
    public bool IsSuccessful => ExtractedFields.Any() && Confidence > 0.0f;

    /// <summary>
    /// Gets the number of successfully extracted fields
    /// </summary>
    public int FieldCount => ExtractedFields.Count;

    /// <summary>
    /// Gets the average confidence score across all fields
    /// </summary>
    public float AverageFieldConfidence => 
        ExtractedFields.Values.DefaultIfEmpty(new FieldValue { Confidence = 0.0f })
                               .Average(f => f.Confidence);

    /// <summary>
    /// Adds an extracted field to the result
    /// </summary>
    /// <param name="fieldName">The name of the field</param>
    /// <param name="value">The extracted value</param>
    /// <param name="confidence">The confidence score for this field</param>
    /// <param name="dataType">The data type of the extracted value</param>
    public void AddField(string fieldName, object value, float confidence, string? dataType = null)
    {
        ExtractedFields[fieldName] = new FieldValue
        {
            Value = value,
            Confidence = confidence,
            DataType = dataType ?? value?.GetType().Name ?? "Unknown"
        };
    }

    /// <summary>
    /// Adds a warning to the extraction result
    /// </summary>
    /// <param name="warning">The warning message</param>
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    /// <summary>
    /// Gets a field value by name
    /// </summary>
    /// <param name="fieldName">The field name</param>
    /// <returns>The field value or null if not found</returns>
    public FieldValue? GetField(string fieldName)
    {
        return ExtractedFields.TryGetValue(fieldName, out var value) ? value : null;
    }

    /// <summary>
    /// Checks if a specific field was extracted
    /// </summary>
    /// <param name="fieldName">The field name to check</param>
    /// <returns>True if the field was extracted</returns>
    public bool HasField(string fieldName)
    {
        return ExtractedFields.ContainsKey(fieldName);
    }
}