using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.DTOs;

/// <summary>
/// Result of OCR processing on document data
/// </summary>
public class OCRResult
{
    /// <summary>
    /// Gets or sets whether the OCR processing was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Gets or sets the extracted text content
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the confidence score of the OCR result
    /// </summary>
    public float Confidence { get; set; }
    
    /// <summary>
    /// Gets or sets the processed OCR regions
    /// </summary>
    public List<OCRRegion> ProcessedRegions { get; set; } = new();
} 