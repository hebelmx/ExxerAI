namespace ExxerAI.Application.DTOs;

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