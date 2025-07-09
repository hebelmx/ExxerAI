namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents processing options for document processing
/// </summary>
public class ProcessingOptions
{
    /// <summary>
    /// Gets or sets whether to use OCR if direct text extraction fails
    /// </summary>
    public bool UseOCRFallback { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use LLM for field extraction
    /// </summary>
    public bool UseLLMExtraction { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable schema learning
    /// </summary>
    public bool EnableSchemaLearning { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum confidence threshold for auto-processing
    /// </summary>
    public float MinimumConfidenceThreshold { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets whether to store in primary source of truth
    /// </summary>
    public bool StoreTruthRecord { get; set; } = true;

    /// <summary>
    /// Gets or sets the OCR language codes to use
    /// </summary>
    public List<string> OCRLanguages { get; init; } = new() { "spa", "eng" };

    /// <summary>
    /// Gets or sets custom processing parameters
    /// </summary>
    public Dictionary<string, object> CustomParameters { get; init; } = new();
}