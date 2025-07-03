namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents context information for pattern extraction
/// </summary>
public class ExtractionContext
{
    /// <summary>
    /// Gets or sets the document type
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the OCR regions if available
    /// </summary>
    public List<OCRRegion> OCRRegions { get; init; } = new();

    /// <summary>
    /// Gets or sets additional context properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();

    /// <summary>
    /// Gets or sets the confidence threshold for extraction
    /// </summary>
    public float ConfidenceThreshold { get; set; } = 0.8f;
}