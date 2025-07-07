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
    /// </summary>
    /// <param name="line">The line containing the reference text</param>
    /// <returns>The first token after the reference text, or null if not found</returns>
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
        return tokens.Length > 0 ? tokens[0] : null;
    }

    /// <summary>
    /// Extracts numeric values from the same line as the reference text, after the reference
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
        
        // Extract numeric value from the remaining text
        // Remove any leading non-digit characters (currency symbols, negative signs, etc.) and find the first complete number
        var match = Regex.Match(afterRef.Trim(), @"[^\d]*(\d{1,3}(?:,\d{3})*(?:\.\d+)?)");
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Extracts numeric values from a line using prioritized regex patterns
    /// Handles various currency formats including comma-separated thousands
    /// </summary>
    /// <param name="line">The line to extract numeric values from</param>
    /// <returns>The extracted numeric value, or null if not found</returns>
    /// <remarks>
    /// Uses a six-tier pattern matching system:
    /// 1. Full currency amounts at line start (e.g., "1,100.00 USD")
    /// 2. Simple decimals at line start (e.g., "1250.75")
    /// 3. Integers at line start (e.g., "500")
    /// 4. Currency amounts anywhere in line (fallback)
    /// 5. Any decimal numbers (fallback)
    /// 6. Any integers (final fallback)
    /// </remarks>
    private string? ExtractFromLine(string line)
    {
        // Extract numeric value from the entire line
        // For the specific failing case: "1,100.00 USD" should return "1,100.00"
        var trimmedLine = line.Trim();
        
        // Improved patterns to handle currency amounts properly
        // Priority order: longest and most complete matches first
        var patterns = new[]
        {
            // Pattern 1: Full currency amount with commas at start of line (e.g., "1,100.00 USD", "$1,500.00", "€2,750.50")
            @"^[^\d]*(\d{1,3}(?:,\d{3})+(?:\.\d{2})?)",
            
            // Pattern 2: Simple number with decimals at start (e.g., "1250.75", "$999.99", "5.25%")  
            @"^[^\d]*(\d+\.\d{2})",
            
            // Pattern 3: Integer at start (e.g., "500", "$100", "10%")
            @"^[^\d]*(\d+)",
            
            // Pattern 4: Currency amount anywhere in line (fallback)
            @"[^\d]*(\d{1,3}(?:,\d{3})+(?:\.\d{2})?)",
            
            // Pattern 5: Any decimal number (fallback)
            @"[^\d]*(\d+\.\d{2})",
            
            // Pattern 6: Any integer (final fallback)
            @"[^\d]*(\d+)"
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