namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents an OCR region that was processed
/// </summary>
public class OCRRegion
{
    /// <summary>
    /// Gets or sets the region identifier
    /// </summary>
    public string RegionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extracted text from this region
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence score for this region
    /// </summary>
    public float Confidence { get; set; } = 0.0f;

    /// <summary>
    /// Gets or sets the bounding box coordinates
    /// </summary>
    public BoundingBox BoundingBox { get; set; } = new();

    /// <summary>
    /// Gets or sets the fields found in this region
    /// </summary>
    public List<string> FieldsFound { get; init; } = new();
}