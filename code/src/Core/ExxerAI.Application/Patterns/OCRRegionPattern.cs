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
                    SearchStrategy.SameLineOrNext => ExtractFromLine(lines[i]) ?? (i + 1 < lines.Length ? ExtractFromLine(lines[i + 1]) : null),
                    _ => null
                };
            }
        }

        return null;
    }

    private string? ExtractNextToken(string line)
    {
        var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var refIndex = Array.FindIndex(tokens, t => t.Contains(ReferenceText, StringComparison.OrdinalIgnoreCase));
        return refIndex >= 0 && refIndex + 1 < tokens.Length ? tokens[refIndex + 1] : null;
    }

    private string? ExtractFromLine(string line)
    {
        // Simple extraction - would be enhanced with more sophisticated parsing
        var match = Regex.Match(line, @"[\d,]+\.?\d*");
        return match.Success ? match.Value : null;
    }
} 