using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.DTOs;

/// <summary>
/// Result of extracting data from a specific document region
/// </summary>
public class RegionExtractionResult
{
    /// <summary>
    /// Gets or sets the name of the field that was extracted
    /// </summary>
    public string FieldName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the extracted value from the region
    /// </summary>
    public string ExtractedValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the confidence score of the extraction
    /// </summary>
    public float Confidence { get; set; }
    
    /// <summary>
    /// Gets or sets the bounding box of the extracted region
    /// </summary>
    public BoundingBox Region { get; set; } = new();
} 