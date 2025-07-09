namespace ExxerAI.Application.Data;

/// <summary>
/// Statistics for pattern seed data
/// </summary>
public class PatternSeedStatistics
{
    /// <summary>
    /// Gets or sets the total number of patterns
    /// </summary>
    public int TotalPatterns { get; set; }

    /// <summary>
    /// Gets or sets the number of unique fields covered
    /// </summary>
    public int UniqueFields { get; set; }

    /// <summary>
    /// Gets or sets the number of unique document types
    /// </summary>
    public int UniqueDocumentTypes { get; set; }

    /// <summary>
    /// Gets or sets the average confidence score across all patterns
    /// </summary>
    public float AverageConfidence { get; set; }

    /// <summary>
    /// Gets or sets the total number of successful extractions
    /// </summary>
    public int TotalSuccessfulExtractions { get; set; }

    /// <summary>
    /// Gets or sets the total number of extraction attempts
    /// </summary>
    public int TotalAttempts { get; set; }

    /// <summary>
    /// Gets or sets the overall success rate across all patterns
    /// </summary>
    public float OverallSuccessRate { get; set; }
}