namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents the methods available for text extraction from documents
/// </summary>
public enum ExtractionMethod
{
    /// <summary>
    /// Direct text extraction from digital documents
    /// </summary>
    DirectText,
    
    /// <summary>
    /// Optical Character Recognition for scanned documents
    /// </summary>
    OCR,
    
    /// <summary>
    /// LLM-assisted extraction
    /// </summary>
    LLMAssisted,
    
    /// <summary>
    /// Hybrid approach using multiple methods
    /// </summary>
    Hybrid
}