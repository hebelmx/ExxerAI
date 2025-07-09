using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Batch processing result summary
/// </summary>
public class BatchProcessingResult
{
    /// <summary>
    /// Gets or sets the total number of documents in the batch
    /// </summary>
    public int TotalDocuments { get; set; }

    /// <summary>
    /// Gets or sets the number of successfully processed documents
    /// </summary>
    public int SuccessfullyProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of failed documents
    /// </summary>
    public int FailedDocuments { get; set; }

    /// <summary>
    /// Gets or sets the average confidence score for successful documents
    /// </summary>
    public float AverageConfidence { get; set; }

    /// <summary>
    /// Gets or sets the total processing time for the batch
    /// </summary>
    public TimeSpan TotalProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets individual document processing results
    /// </summary>
    public List<DocumentProcessingResult> Results { get; set; } = [];

    /// <summary>
    /// Gets the success rate for the batch (0.0 to 1.0)
    /// </summary>
    public float SuccessRate => TotalDocuments > 0 ? (float)SuccessfullyProcessed / TotalDocuments : 0f;
}