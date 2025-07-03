namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents strategies for extracting values relative to keywords
/// </summary>
public enum PositionStrategy
{
    /// <summary>
    /// Extract the next token after the keyword
    /// </summary>
    NextToken,
    
    /// <summary>
    /// Extract from the same line as the keyword
    /// </summary>
    SameLine,
    
    /// <summary>
    /// Extract from the next line after the keyword
    /// </summary>
    NextLine,
    
    /// <summary>
    /// Extract from the same line or next if not found
    /// </summary>
    SameLineOrNext
}