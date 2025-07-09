using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.DTOs;

/// <summary>
/// Feedback data for machine learning improvements in document processing
/// </summary>
public class LearningFeedback
{
    /// <summary>
    /// Gets or sets the document type that was processed
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dictionary of extracted fields and their values
    /// </summary>
    public Dictionary<string, object> ExtractedFields { get; set; } = [];

    /// <summary>
    /// Gets or sets the overall confidence score of the processing
    /// </summary>
    public float OverallConfidence { get; set; }

    /// <summary>|
    /// Gets or sets the validation results from the processing
    /// </summary>
    public ValidationResultDocument? ValidationResultDocument { get; set; }
}