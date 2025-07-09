using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents grounding statistics for a specific document type
/// </summary>
public class GroundingStatistics
{
    /// <summary>
    /// Gets or sets the document type
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Unknown;

    /// <summary>
    /// Gets or sets the total number of documents processed
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of successful extractions
    /// </summary>
    public int SuccessfulExtractions { get; set; }

    /// <summary>
    /// Gets or sets the average processing time in milliseconds
    /// </summary>
    public long AverageProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the average confidence score
    /// </summary>
    public float AverageConfidence { get; set; }

    /// <summary>
    /// Gets or sets field-specific extraction statistics
    /// </summary>
    public Dictionary<string, FieldStatistics> FieldStats { get; init; } = new();
}