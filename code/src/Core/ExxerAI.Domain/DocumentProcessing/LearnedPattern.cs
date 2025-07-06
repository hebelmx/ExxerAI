namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a learned pattern from processing history.
/// </summary>
public class LearnedPattern
{
    /// <summary>
    /// Gets or sets the unique identifier for this pattern.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the field name this pattern applies to.
    /// </summary>
    [StringLength(255)]
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the extraction pattern.
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pattern type (Regex, Keyword, etc.).
    /// </summary>
    public string PatternType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how many times this pattern was successful.
    /// </summary>
    public int SuccessCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets how many times this pattern failed.
    /// </summary>
    public int FailureCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets the confidence score for this pattern.
    /// </summary>
    public float Confidence { get; set; } = 0f;

    /// <summary>
    /// Gets or sets when this pattern was first learned.
    /// </summary>
    public DateTime LearnedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this pattern was last used successfully.
    /// </summary>
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the success rate for this pattern.
    /// </summary>
    public float SuccessRate => (SuccessCount + FailureCount) > 0 ? 
        (float)SuccessCount / (SuccessCount + FailureCount) : 0f;
}