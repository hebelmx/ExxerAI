using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.DTOs;

/// <summary>
/// Result of pattern learning analysis for continuous improvement
/// </summary>
public class PatternLearningResult
{
    /// <summary>
    /// Gets or sets the pattern that was used for extraction
    /// </summary>
    public ExtractionPattern Pattern { get; set; } = null!;

    /// <summary>
    /// Gets or sets whether the extraction was successful
    /// </summary>
    public bool WasSuccessful { get; set; }

    /// <summary>
    /// Gets or sets the value that was extracted
    /// </summary>
    public string? ExtractedValue { get; set; }

    /// <summary>
    /// Gets or sets the expected value for validation
    /// </summary>
    public string? ExpectedValue { get; set; }

    /// <summary>
    /// Gets or sets the confidence score of the extraction
    /// </summary>
    public float Confidence { get; set; }

    /// <summary>
    /// Gets or sets the time taken to process the extraction
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets optional feedback about the extraction quality
    /// </summary>
    public string? Feedback { get; set; }
} 