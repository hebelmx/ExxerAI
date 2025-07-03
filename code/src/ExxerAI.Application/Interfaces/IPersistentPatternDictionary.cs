using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Database-persisted pattern dictionary system based on KpiExxerpro field extraction patterns
/// Supports continuous learning and pattern evolution based on successful extractions
/// Maintains audit trail and confidence scoring for all pattern variations
/// </summary>
public interface IPersistentPatternDictionary
{
    /// <summary>
    /// Retrieves extraction patterns for a specific field and document type
    /// Returns patterns ordered by confidence score (highest first)
    /// </summary>
    /// <param name="fieldName">Name of the field to extract (e.g., "registro_patronal")</param>
    /// <param name="documentType">Type of document (e.g., "IMSSPayment")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of extraction patterns ordered by confidence</returns>
    Task<List<ExtractionPattern>> GetPatternsForFieldAsync(string fieldName, string documentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all extraction patterns for a specific document type
    /// Groups patterns by field name for efficient batch processing
    /// </summary>
    /// <param name="documentType">Type of document to get patterns for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary mapping field names to their extraction patterns</returns>
    Task<Dictionary<string, List<ExtractionPattern>>> GetPatternsForDocumentTypeAsync(string documentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates pattern success statistics based on successful field extraction
    /// Implements continuous learning to improve pattern confidence scores
    /// </summary>
    /// <param name="learningResult">Result of pattern learning analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the update operation</returns>
    Task UpdatePatternFromSuccessfulExtractionAsync(PatternLearningResult learningResult, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the evolution history of patterns for a specific field
    /// Useful for analyzing pattern effectiveness over time
    /// </summary>
    /// <param name="fieldName">Field name to get history for</param>
    /// <param name="fromDate">Start date for history retrieval</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Pattern evolution history with confidence changes</returns>
    Task<PatternEvolutionHistory> GetPatternEvolutionHistoryAsync(string fieldName, DateTime fromDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current confidence score for a specific pattern
    /// Based on success rate and usage statistics
    /// </summary>
    /// <param name="patternId">Unique identifier of the pattern</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current confidence score (0.0 to 1.0)</returns>
    Task<float> GetPatternConfidenceScoreAsync(string patternId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new extraction pattern discovered through learning
    /// Validates pattern before adding to prevent duplicates and conflicts
    /// </summary>
    /// <param name="pattern">New extraction pattern to persist</param>
    /// <param name="initialConfidence">Initial confidence score for the pattern</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Unique identifier of the persisted pattern</returns>
    Task<string> PersistNewPatternAsync(ExtractionPattern pattern, float initialConfidence, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates patterns that consistently fail to extract correct data
    /// Implements pattern quality management to maintain system accuracy
    /// </summary>
    /// <param name="fieldName">Field name to analyze</param>
    /// <param name="documentType">Document type to analyze</param>
    /// <param name="minimumSuccessRate">Minimum success rate to keep pattern active</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of patterns deactivated</returns>
    Task<int> DeactivateUnderperformingPatternsAsync(string fieldName, string documentType, float minimumSuccessRate = 0.5f, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds the pattern dictionary with proven KpiExxerpro patterns
    /// Used for initial system setup with battle-tested extraction patterns
    /// </summary>
    /// <param name="seedPatterns">Collection of proven patterns to seed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of patterns successfully seeded</returns>
    Task<int> SeedKpiExxerproPatternsAsync(IEnumerable<PatternDictionaryEntity> seedPatterns, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports pattern dictionary for backup or transfer to other systems
    /// Includes full pattern metadata and usage statistics
    /// </summary>
    /// <param name="documentType">Optional document type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Serializable pattern dictionary export</returns>
    Task<PatternDictionaryExport> ExportPatternDictionaryAsync(string? documentType = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Base class for extraction patterns supporting different pattern types
/// Provides polymorphic behavior for various extraction strategies
/// </summary>
public abstract class ExtractionPattern
{
    /// <summary>
    /// Gets or sets the unique identifier for this pattern
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field name this pattern extracts
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type this pattern applies to
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets the pattern type identifier
    /// </summary>
    public abstract string PatternType { get; }

    /// <summary>
    /// Gets or sets the confidence score for this pattern (0.0 to 1.0)
    /// </summary>
    public float Confidence { get; set; }

    /// <summary>
    /// Gets or sets usage statistics for this pattern
    /// </summary>
    public PatternUsageStatistics Usage { get; set; } = new();

    /// <summary>
    /// Gets or sets pattern-specific metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Attempts to extract the field value from the provided text
    /// Returns null if pattern does not match
    /// </summary>
    /// <param name="text">Text to extract from</param>
    /// <param name="context">Optional processing context</param>
    /// <returns>Extracted value or null if no match</returns>
    public abstract Task<string?> ExtractAsync(string text, ExtractionContext? context = null);

    /// <summary>
    /// Validates whether this pattern is applicable to the given context
    /// Allows for context-sensitive pattern selection
    /// </summary>
    /// <param name="context">Extraction context to validate against</param>
    /// <returns>True if pattern is applicable</returns>
    public virtual bool IsApplicable(ExtractionContext context) => true;
}

/// <summary>
/// Regular expression-based extraction pattern
/// Based on proven KpiExxerpro regex patterns with 95%+ accuracy
/// </summary>
public class RegexPattern : ExtractionPattern
{
    /// <summary>
    /// Gets the pattern type identifier
    /// </summary>
    public override string PatternType => "Regex";

    /// <summary>
    /// Gets or sets the regular expression pattern
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets regex options for the pattern
    /// </summary>
    public System.Text.RegularExpressions.RegexOptions Options { get; set; } = System.Text.RegularExpressions.RegexOptions.IgnoreCase;

    /// <summary>
    /// Gets or sets the capture group index to extract (default: 1)
    /// </summary>
    public int CaptureGroup { get; set; } = 1;

    /// <summary>
    /// Extracts field value using regular expression matching
    /// </summary>
    /// <param name="text">Text to extract from</param>
    /// <param name="context">Optional processing context</param>
    /// <returns>Extracted value or null if no match</returns>
    public override async Task<string?> ExtractAsync(string text, ExtractionContext? context = null)
    {
        await Task.CompletedTask; // For async consistency

        try
        {
            var regex = new System.Text.RegularExpressions.Regex(Expression, Options);
            var match = regex.Match(text);

            if (match.Success && match.Groups.Count > CaptureGroup)
            {
                return match.Groups[CaptureGroup].Value.Trim();
            }
        }
        catch (Exception ex)
        {
            // Log regex error but don't throw - allow other patterns to try
            Metadata["LastError"] = ex.Message;
            Metadata["LastErrorTime"] = DateTime.UtcNow;
        }

        return null;
    }
}

/// <summary>
/// OCR region-specific extraction pattern
/// Based on KpiExxerpro region extraction with OpenCV contour detection
/// </summary>
public class OCRRegionPattern : ExtractionPattern
{
    /// <summary>
    /// Gets the pattern type identifier
    /// </summary>
    public override string PatternType => "OCRRegion";

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
    public override async Task<string?> ExtractAsync(string text, ExtractionContext? context = null)
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
        var match = System.Text.RegularExpressions.Regex.Match(line, @"[\d,]+\.?\d*");
        return match.Success ? match.Value : null;
    }
}

/// <summary>
/// Keyword-based extraction pattern with positional search
/// </summary>
public class KeywordPattern : ExtractionPattern
{
    /// <summary>
    /// Gets the pattern type identifier
    /// </summary>
    public override string PatternType => "Keyword";

    /// <summary>
    /// Gets or sets the keyword to search for
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the position strategy relative to keyword
    /// </summary>
    public PositionStrategy PositionStrategy { get; set; }

    /// <summary>
    /// Gets or sets the number of tokens to offset from keyword
    /// </summary>
    public int TokenOffset { get; set; } = 1;

    /// <summary>
    /// Extracts field value using keyword-based positional search
    /// </summary>
    /// <param name="text">Text to extract from</param>
    /// <param name="context">Optional processing context</param>
    /// <returns>Extracted value or null if not found</returns>
    public override async Task<string?> ExtractAsync(string text, ExtractionContext? context = null)
    {
        await Task.CompletedTask;

        var tokens = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var keywordIndex = Array.FindIndex(tokens, t => t.Contains(Keyword, StringComparison.OrdinalIgnoreCase));

        if (keywordIndex < 0) return null;

        var targetIndex = PositionStrategy switch
        {
            PositionStrategy.NextToken => keywordIndex + TokenOffset,
            PositionStrategy.PreviousToken => keywordIndex - TokenOffset,
            PositionStrategy.SameToken => keywordIndex,
            _ => -1
        };

        return targetIndex >= 0 && targetIndex < tokens.Length ? tokens[targetIndex] : null;
    }
}

/// <summary>
/// LLM-powered extraction pattern for complex or unstructured data
/// </summary>
public class LLMPattern : ExtractionPattern
{
    /// <summary>
    /// Gets the pattern type identifier
    /// </summary>
    public override string PatternType => "LLM";

    /// <summary>
    /// Gets or sets the LLM prompt for field extraction
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected response format
    /// </summary>
    public string ExpectedFormat { get; set; } = string.Empty;

    /// <summary>
    /// Extracts field value using LLM processing
    /// This would integrate with the LLM service in a real implementation
    /// </summary>
    /// <param name="text">Text to extract from</param>
    /// <param name="context">Processing context with LLM access</param>
    /// <returns>Extracted value or null if not found</returns>
    public override async Task<string?> ExtractAsync(string text, ExtractionContext? context = null)
    {
        // Placeholder for LLM integration
        // In real implementation, this would call the LLM service
        await Task.CompletedTask;
        return null; // LLM integration would be implemented in the concrete service
    }
}

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

/// <summary>
/// Position strategies for keyword patterns
/// </summary>
public enum PositionStrategy
{
    /// <summary>
    /// Extract the next token after the keyword
    /// </summary>
    NextToken,

    /// <summary>
    /// Extract the previous token before the keyword
    /// </summary>
    PreviousToken,

    /// <summary>
    /// Extract part of the same token as the keyword
    /// </summary>
    SameToken
}

/// <summary>
/// Region bounds for OCR region patterns
/// </summary>
public class RegionBounds
{
    /// <summary>
    /// Gets or sets the X offset from reference point
    /// </summary>
    public int XOffset { get; set; }

    /// <summary>
    /// Gets or sets the Y offset from reference point
    /// </summary>
    public int YOffset { get; set; }

    /// <summary>
    /// Gets or sets the width of the region
    /// </summary>
    public int Width { get; set; } = 200;

    /// <summary>
    /// Gets or sets the height of the region
    /// </summary>
    public int Height { get; set; } = 50;
}

/// <summary>
/// Pattern usage statistics for continuous learning
/// </summary>
public class PatternUsageStatistics
{
    /// <summary>
    /// Gets or sets the number of successful extractions
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of extraction attempts
    /// </summary>
    public int TotalAttempts { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of last successful use
    /// </summary>
    public DateTime? LastSuccessAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of last use (successful or failed)
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Gets the calculated success rate (0.0 to 1.0)
    /// </summary>
    public float SuccessRate => TotalAttempts > 0 ? (float)SuccessCount / TotalAttempts : 0f;
}

/// <summary>
/// Context information for extraction operations
/// </summary>
public class ExtractionContext
{
    /// <summary>
    /// Gets or sets the document type being processed
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OCR regions if available
    /// </summary>
    public List<OCRRegion>? OCRRegions { get; set; }

    /// <summary>
    /// Gets or sets additional context metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of pattern learning analysis
/// </summary>
public class PatternLearningResult
{
    /// <summary>
    /// Gets or sets the pattern that was used
    /// </summary>
    public ExtractionPattern Pattern { get; set; } = null!;

    /// <summary>
    /// Gets or sets whether the extraction was successful
    /// </summary>
    public bool WasSuccessful { get; set; }

    /// <summary>
    /// Gets or sets the extracted value
    /// </summary>
    public string? ExtractedValue { get; set; }

    /// <summary>
    /// Gets or sets the expected value for validation
    /// </summary>
    public string? ExpectedValue { get; set; }

    /// <summary>
    /// Gets or sets the confidence score for this extraction
    /// </summary>
    public float Confidence { get; set; }

    /// <summary>
    /// Gets or sets the processing time for this extraction
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets feedback for pattern improvement
    /// </summary>
    public string? Feedback { get; set; }
}

/// <summary>
/// Pattern evolution history for analytics
/// </summary>
public class PatternEvolutionHistory
{
    /// <summary>
    /// Gets or sets the field name
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the history entries
    /// </summary>
    public List<PatternHistoryEntry> History { get; set; } = new();
}

/// <summary>
/// Individual pattern history entry
/// </summary>
public class PatternHistoryEntry
{
    /// <summary>
    /// Gets or sets the pattern ID
    /// </summary>
    public string PatternId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of this entry
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the confidence score at this time
    /// </summary>
    public float ConfidenceScore { get; set; }

    /// <summary>
    /// Gets or sets the success count at this time
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the total attempts at this time
    /// </summary>
    public int TotalAttempts { get; set; }

    /// <summary>
    /// Gets or sets the event type (Created, Updated, Deactivated, etc.)
    /// </summary>
    public string EventType { get; set; } = string.Empty;
}

/// <summary>
/// Database entity for persistent pattern storage with full audit capabilities
/// Supports pattern versioning, confidence tracking, and usage analytics
/// </summary>
public class PatternDictionaryEntity
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the field name this pattern extracts
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type this pattern applies to
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pattern type (Regex, OCRRegion, Keyword, LLM)
    /// </summary>
    public string PatternType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pattern expression or configuration
    /// </summary>
    public string PatternExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current confidence score (0.0 to 1.0)
    /// </summary>
    public float ConfidenceScore { get; set; }

    /// <summary>
    /// Gets or sets the number of successful extractions
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of extraction attempts
    /// </summary>
    public int TotalAttempts { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last usage timestamp
    /// </summary>
    public DateTime LastUsedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the creator/source of this pattern
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this pattern is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets additional notes about this pattern
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets pattern-specific metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets the calculated success rate based on actual usage statistics
    /// </summary>
    public float SuccessRate => TotalAttempts > 0 ? (float)SuccessCount / TotalAttempts : 0f;
}

/// <summary>
/// Export format for pattern dictionary backup and transfer
/// </summary>
public class PatternDictionaryExport
{
    /// <summary>
    /// Gets or sets the export timestamp
    /// </summary>
    public DateTime ExportedAt { get; set; }

    /// <summary>
    /// Gets or sets the exported patterns
    /// </summary>
    public List<PatternDictionaryEntity> Patterns { get; set; } = new();

    /// <summary>
    /// Gets or sets export metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the export version
    /// </summary>
    public string Version { get; set; } = "1.0";
}