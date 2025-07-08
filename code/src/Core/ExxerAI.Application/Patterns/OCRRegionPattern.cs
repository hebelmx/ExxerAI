using System.Text.RegularExpressions;
using ExxerAI.Application.DTOs;
using ExxerAI.Application.Enums;
using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Patterns;

/// <summary>
/// OCR region-specific extraction pattern
/// Based on KpiExxerpro region extraction with OpenCV contour detection
/// </summary>
public class OCRRegionPattern : ExtractionPattern
{


    /// <summary>
    /// Gets or sets the reference text to locate the region
    /// </summary>
    public string ReferenceText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the search strategy for finding the target text
    /// </summary>
    public SearchStrategy SearchStrategy { get; set; }

    /// <summary>
    /// Gets or sets the region bounds relative to reference text
    /// </summary>
    public RegionBounds RegionBounds { get; set; } = new();

    /// <summary>
    /// Extracts field value using OCR region-specific processing
    /// This is a simplified implementation - full OCR integration would be in infrastructure layer
    /// </summary>
    /// <param name="text">Text to extract from (or OCR input)</param>
    /// <param name="context">Processing context with OCR capabilities</param>
    /// <returns>Extracted value or null if not found</returns>
    public override string? ExtractValue(string text, ExtractionContext context)
    {
        // Handle null or empty input
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // This would typically integrate with OCR services in the infrastructure layer
        // For now, implement as text-based search as fallback
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            // Use more precise matching to avoid "Subtotal:" matching "Total:"
            // Check if the line contains the reference text as a complete word or phrase
            var line = lines[i];
            var referenceIndex = line.IndexOf(ReferenceText, StringComparison.OrdinalIgnoreCase);
            
            if (referenceIndex >= 0)
            {
                // Ensure it's a word boundary match - reference text should be at start or after non-letter
                bool isWordBoundary = referenceIndex == 0 || 
                                    !char.IsLetter(line[referenceIndex - 1]);
                
                // Also check the character after the reference text (if any)
                int endIndex = referenceIndex + ReferenceText.Length;
                if (endIndex < line.Length)
                {
                    isWordBoundary = isWordBoundary && !char.IsLetter(line[endIndex]);
                }
                
                if (isWordBoundary)
                {
                    return SearchStrategy switch
                    {
                        SearchStrategy.NextToken => ExtractNextToken(lines[i]),
                        SearchStrategy.NextLineInRegion => i + 1 < lines.Length ? ExtractFromLine(lines[i + 1]) : null,
                        SearchStrategy.SameLineOrNext => ExtractFromSameLine(lines[i]) ?? (i + 1 < lines.Length ? ExtractFromLine(lines[i + 1]) : null),
                        _ => null
                    };
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts the next token immediately following the reference text on the same line
    /// Strips currency symbols, negative signs, and percentage signs
    /// </summary>
    /// <param name="line">The line containing the reference text</param>
    /// <returns>The first numeric token after the reference text, cleaned of symbols, or null if not found</returns>
    private string? ExtractNextToken(string line)
    {
        // Find the reference text position
        var refIndex = line.IndexOf(ReferenceText, StringComparison.OrdinalIgnoreCase);
        if (refIndex == -1) return null;
        
        // Get the text after the reference text
        var afterRef = line.Substring(refIndex + ReferenceText.Length).Trim();
        if (string.IsNullOrEmpty(afterRef)) return null;
        
        // Extract the first token after the reference text
        var tokens = afterRef.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) return null;
        
        var firstToken = tokens[0];
        
        // Apply regex patterns to extract clean numeric value from the token
        // This handles currency symbols, negative signs, and percentage signs
        var patterns = new[]
        {
            // Pattern 1: Currency amounts with commas - extract numeric part only
            @"[^\d]*(\d{1,3}(?:,\d{3})+(?:\.\d{2})?)[^\d]*",
            
            // Pattern 2: Simple decimal numbers - extract numeric part only
            @"[^\d]*(\d+\.\d+)[^\d]*",
            
            // Pattern 3: Integer numbers - extract numeric part only
            @"[^\d]*(\d+)[^\d]*"
        };
        
        foreach (var pattern in patterns)
        {
            var match = Regex.Match(firstToken, pattern);
            if (match.Success && match.Groups.Count > 1)
            {
                var value = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(value) && value.Any(char.IsDigit))
                {
                    return value;
                }
            }
        }
        
        return null;
    }

    /// <summary>
    /// Extracts numeric values from the same line as the reference text, after the reference
    /// Strips currency symbols, negative signs, and percentage signs
    /// </summary>
    /// <param name="line">The line containing the reference text</param>
    /// <returns>The extracted numeric value, or null if not found</returns>
    private string? ExtractFromSameLine(string line)
    {
        // Find the reference text position
        var refIndex = line.IndexOf(ReferenceText, StringComparison.OrdinalIgnoreCase);
        if (refIndex == -1) return null;
        
        // Get the text after the reference text on the same line
        var afterRef = line.Substring(refIndex + ReferenceText.Length).Trim();
        if (string.IsNullOrEmpty(afterRef)) return null;
        
        // Extract numeric value using the same patterns as ExtractFromLine
        var patterns = new[]
        {
            @"[^\d]*(\d{1,3}(?:,\d{3})+(?:\.\d{2})?)[^\d]*",  // Currency amounts with commas
            @"[^\d]*(\d+\.\d+)[^\d]*",                         // Decimal numbers
            @"[^\d]*(\d+)[^\d]*"                               // Integer numbers
        };
        
        foreach (var pattern in patterns)
        {
            var match = Regex.Match(afterRef, pattern);
            if (match.Success && match.Groups.Count > 1)
            {
                var value = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(value) && value.Any(char.IsDigit))
                {
                    return value;
                }
            }
        }
        
        return null;
    }

    /// <summary>
    /// Extracts numeric values from a line using prioritized regex patterns
    /// Handles various currency formats including comma-separated thousands
    /// Strips currency symbols, negative signs, and percentage signs
    /// </summary>
    /// <param name="line">The line to extract numeric values from</param>
    /// <returns>The extracted numeric value, or null if not found</returns>
    /// <remarks>
    /// Uses a comprehensive pattern matching system that:
    /// 1. Finds numeric values with various prefixes/suffixes
    /// 2. Strips currency symbols ($, €, £, ¥, etc.)
    /// 3. Strips negative signs (-)
    /// 4. Strips percentage signs (%)
    /// 5. Preserves only digits, commas, and decimal points
    /// </remarks>
    private string? ExtractFromLine(string line)
    {
        // Extract numeric value from the entire line
        // For the specific failing case: "1,100.00 USD" should return "1,100.00"
        var trimmedLine = line.Trim();
        
        // Comprehensive patterns to find numeric values with various prefixes/suffixes
        var patterns = new[]
        {
            // Pattern 1: Currency amounts with commas (e.g., "$1,500.00", "€2,750.50", "£999.99", "¥10,000")
            @"[^\d]*(\d{1,3}(?:,\d{3})+(?:\.\d{2})?)[^\d]*",
            
            // Pattern 2: Simple decimal numbers (e.g., "5.25%", "$999.99", "-500.00")  
            @"[^\d]*(\d+\.\d+)[^\d]*",
            
            // Pattern 3: Integer numbers (e.g., "10%", "$100", "-500")
            @"[^\d]*(\d+)[^\d]*"
        };
        
        foreach (var pattern in patterns)
        {
            var match = Regex.Match(trimmedLine, pattern);
            if (match.Success && match.Groups.Count > 1)
            {
                var value = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(value) && value.Any(char.IsDigit))
                {
                    return value;
                }
            }
        }
        
        return null;
    }
} 