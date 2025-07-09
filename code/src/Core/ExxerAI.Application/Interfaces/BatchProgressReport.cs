namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Batch processing progress report
/// </summary>
public class BatchProgressReport
{
    /// <summary>
    /// Gets or sets the number of documents processed
    /// </summary>
    public int ProcessedCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of documents to process
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the number of successfully processed documents
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the currently processing document name
    /// </summary>
    public string CurrentDocument { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the estimated time remaining for batch completion
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// Gets the completion percentage (0.0 to 1.0)
    /// </summary>
    public float CompletionPercentage => TotalCount > 0 ? (float)ProcessedCount / TotalCount : 0f;
}