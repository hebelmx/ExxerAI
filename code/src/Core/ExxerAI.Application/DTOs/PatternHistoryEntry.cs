namespace ExxerAI.Application.DTOs;

/// <summary>
/// Single entry in pattern evolution history
/// </summary>
public class PatternHistoryEntry
{
    /// <summary>
    /// Gets or sets the pattern identifier
    /// </summary>
    public string PatternId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of this history entry
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the confidence score at this point in time
    /// </summary>
    public float ConfidenceScore { get; set; }

    /// <summary>
    /// Gets or sets the success count at this point in time
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the total attempts at this point in time
    /// </summary>
    public int TotalAttempts { get; set; }

    /// <summary>
    /// Gets or sets the type of event that triggered this history entry
    /// </summary>
    public string EventType { get; set; } = string.Empty;
}