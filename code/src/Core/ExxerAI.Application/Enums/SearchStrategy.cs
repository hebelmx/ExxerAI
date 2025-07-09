namespace ExxerAI.Application.Enums;

/// <summary>
/// Search strategies for OCR region patterns
/// </summary>
public enum SearchStrategy
{
    /// <summary>
    /// Extract the next token after reference text
    /// </summary>
    NextToken,

    /// <summary>
    /// Extract from the next line within the same region
    /// </summary>
    NextLineInRegion,

    /// <summary>
    /// Extract from the same line, or next line if not found
    /// </summary>
    SameLineOrNext,

    /// <summary>
    /// Extract using number pattern in same line
    /// </summary>
    NumberInSameLine
} 