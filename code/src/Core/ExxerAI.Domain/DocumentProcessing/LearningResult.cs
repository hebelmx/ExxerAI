namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents learning results from document processing
/// </summary>
public class LearningResult
{
    /// <summary>
    /// Gets or sets whether new patterns were learned
    /// </summary>
    public bool PatternsLearned { get; set; } = false;

    /// <summary>
    /// Gets or sets the list of new patterns discovered
    /// </summary>
    public List<string> NewPatterns { get; init; } = new();

    /// <summary>
    /// Gets or sets the confidence improvement from learning
    /// </summary>
    public float ConfidenceImprovement { get; set; } = 0.0f;

    /// <summary>
    /// Gets or sets the schema updates made
    /// </summary>
    public List<string> SchemaUpdates { get; init; } = new();
}