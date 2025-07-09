namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Batch processing configuration options
/// </summary>
public class BatchProcessingOptions
{
    /// <summary>
    /// Gets or sets the maximum number of concurrent processing operations
    /// </summary>
    public int? MaxConcurrency { get; set; }

    /// <summary>
    /// Gets or sets whether to continue processing if individual documents fail
    /// </summary>
    public bool ContinueOnError { get; set; } = true;

    /// <summary>
    /// Gets or sets the batch size for database operations
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets timeout for individual document processing
    /// </summary>
    public TimeSpan ProcessingTimeout { get; set; } = TimeSpan.FromMinutes(5);
}