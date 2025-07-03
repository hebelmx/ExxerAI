namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a keyword-based extraction pattern with positional strategy
/// </summary>
public class KeywordPattern : ExtractionPattern
{
    /// <summary>
    /// Gets or sets the keyword to search for
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the position strategy for value extraction
    /// </summary>
    public PositionStrategy Strategy { get; set; } = PositionStrategy.NextToken;

    /// <summary>
    /// Initializes a new instance of the KeywordPattern class
    /// </summary>
    public KeywordPattern() { }

    /// <summary>
    /// Initializes a new instance of the KeywordPattern class with keyword and strategy
    /// </summary>
    /// <param name="keyword">The keyword to search for</param>
    /// <param name="strategy">The position strategy</param>
    /// <param name="confidence">The confidence score</param>
    public KeywordPattern(string keyword, PositionStrategy strategy, float confidence)
    {
        Keyword = keyword;
        Strategy = strategy;
        Confidence = confidence;
    }

    /// <summary>
    /// Attempts to extract a value using the keyword pattern
    /// </summary>
    /// <param name="text">The text to search in</param>
    /// <param name="context">Additional context for extraction</param>
    /// <returns>The extracted value or null if not found</returns>
    public override string? ExtractValue(string text, ExtractionContext context)
    {
        var index = text.IndexOf(Keyword, StringComparison.OrdinalIgnoreCase);
        if (index == -1) return null;

        var lines = text.Split('\n');
        var keywordLine = lines.FirstOrDefault(line => line.Contains(Keyword, StringComparison.OrdinalIgnoreCase));
        if (keywordLine == null) return null;

        return Strategy switch
        {
            PositionStrategy.NextToken => ExtractNextToken(keywordLine, Keyword),
            PositionStrategy.SameLine => ExtractFromSameLine(keywordLine, Keyword),
            PositionStrategy.NextLine => ExtractFromNextLine(lines, keywordLine),
            _ => null
        };
    }

    private static string? ExtractNextToken(string line, string keyword)
    {
        var index = line.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (index == -1) return null;

        var afterKeyword = line.Substring(index + keyword.Length).Trim();
        var tokens = afterKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length > 0 ? tokens[0] : null;
    }

    private static string? ExtractFromSameLine(string line, string keyword)
    {
        var index = line.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (index == -1) return null;

        return line.Substring(index + keyword.Length).Trim();
    }

    private static string? ExtractFromNextLine(string[] lines, string keywordLine)
    {
        var keywordIndex = Array.IndexOf(lines, keywordLine);
        if (keywordIndex == -1 || keywordIndex + 1 >= lines.Length) return null;

        return lines[keywordIndex + 1].Trim();
    }
}