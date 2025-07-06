namespace ExxerAI.Application.DTOs;

/// <summary>
/// Result of direct text extraction from digital documents
/// </summary>
public class DirectTextResult
{
    /// <summary>
    /// Gets or sets whether the extraction was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Gets or sets the extracted text content
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the confidence score of the extraction
    /// </summary>
    public float Confidence { get; set; }
    
    /// <summary>
    /// Gets whether the extracted text has meaningful content
    /// </summary>
    public bool HasMeaningfulContent => !string.IsNullOrWhiteSpace(Text) && Text.Length > 50;
} 