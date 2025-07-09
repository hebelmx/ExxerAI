namespace ExxerAI.Application.DTOs;

/// <summary>
/// Entity representing a pattern in the pattern dictionary database
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
    /// Gets or sets the type of pattern (Regex, OCRRegion, Keyword, LLM)
    /// </summary>
    public string PatternType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pattern expression or configuration
    /// </summary>
    public string PatternExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current confidence score
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
    /// Gets or sets the last used timestamp
    /// </summary>
    public DateTime LastUsedAt { get; set; }

    /// <summary>
    /// Gets or sets the last updated timestamp
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the creator identifier
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the pattern is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets additional notes about the pattern
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional metadata for the pattern
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets the calculated success rate
    /// </summary>
    public float SuccessRate => TotalAttempts > 0 ? (float)SuccessCount / TotalAttempts : 0f;
} 