namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a regex-based extraction pattern
/// </summary>
public class RegexPattern : ExtractionPattern
{
    /// <summary>
    /// Gets or sets the regular expression pattern
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the regex options
    /// </summary>
    public System.Text.RegularExpressions.RegexOptions Options { get; set; } = 
        System.Text.RegularExpressions.RegexOptions.IgnoreCase;

    /// <summary>
    /// Initializes a new instance of the RegexPattern class
    /// </summary>
    public RegexPattern() { }

    /// <summary>
    /// Initializes a new instance of the RegexPattern class with pattern and confidence
    /// </summary>
    /// <param name="pattern">The regex pattern</param>
    /// <param name="confidence">The confidence score</param>
    public RegexPattern(string pattern, float confidence)
    {
        Pattern = pattern;
        Confidence = confidence;
    }

    /// <summary>
    /// Attempts to extract a value using the regex pattern
    /// </summary>
    /// <param name="text">The text to search in</param>
    /// <param name="context">Additional context for extraction</param>
    /// <returns>The extracted value or null if not found</returns>
    public override string? ExtractValue(string text, ExtractionContext context)
    {
        try
        {
            var regex = new System.Text.RegularExpressions.Regex(Pattern, Options);
            var match = regex.Match(text);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }
        catch
        {
            return null;
        }
    }
}