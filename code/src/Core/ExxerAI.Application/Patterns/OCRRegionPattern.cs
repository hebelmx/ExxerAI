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
            if (lines[i].Contains(ReferenceText, StringComparison.OrdinalIgnoreCase))
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

        return null;
    }

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

    private string? ExtractFromSameLine(string line)
    {
        // Find the reference text position
        var refIndex = line.IndexOf(ReferenceText, StringComparison.OrdinalIgnoreCase);
        if (refIndex == -1) return null;
        
        // Get the text after the reference text on the same line
        var afterRef = line.Substring(refIndex + ReferenceText.Length).Trim();
        if (string.IsNullOrEmpty(afterRef)) return null;
        
        // Extract numeric value from the remaining text
        // Remove any leading non-digit characters and find the first complete number
        var cleanText = Regex.Replace(afterRef.Trim(), @"^[^\d]*", "");
        var match = Regex.Match(cleanText, @"^(\d{1,3}(?:,\d{3})*(?:\.\d+)?)");
        return match.Success ? match.Groups[1].Value : null;
    }

    private string? ExtractFromLine(string line)
    {
        // Extract numeric value from the entire line
        // For the specific failing case: "1,100.00 USD" should return "1,100.00"
        var trimmedLine = line.Trim();
        
        // Try to find numbers that start at the beginning of the string or after whitespace
        // This should prioritize "1,100.00" over "100.00" in "1,100.00 USD"
        var patterns = new[]
        {
            @"^(\d{1,3}(?:,\d{3})*(?:\.\d+)?)",           // Number at start of line
            @"\s+(\d{1,3}(?:,\d{3})*(?:\.\d+)?)",         // Number after whitespace
            @"(\d{1,3}(?:,\d{3})*(?:\.\d+)?)"             // Any number (fallback)
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