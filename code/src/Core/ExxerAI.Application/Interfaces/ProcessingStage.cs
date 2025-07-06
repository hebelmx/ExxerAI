namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Document processing stages for detailed pipeline tracking
/// Based on KpiExxerpro multi-stage methodology
/// </summary>
public class ProcessingStage
{
    /// <summary>
    /// Initializes a new processing stage with completion agentStatus and confidence
    /// </summary>
    /// <param name="stageName">Name of the processing stage</param>
    /// <param name="isSuccessful">Whether the stage completed successfully</param>
    /// <param name="confidence">Confidence score for this stage (0.0 to 1.0)</param>
    public ProcessingStage(string stageName, bool isSuccessful, float confidence)
    {
        StageName = stageName;
        IsSuccessful = isSuccessful;
        Confidence = confidence;
        ProcessedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the name of the processing stage
    /// </summary>
    public string StageName { get; }

    /// <summary>
    /// Gets whether the stage completed successfully
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets the confidence score for this stage (0.0 to 1.0)
    /// </summary>
    public float Confidence { get; }

    /// <summary>
    /// Gets the timestamp when this stage was processed
    /// </summary>
    public DateTime ProcessedAt { get; }

    /// <summary>
    /// Gets or sets additional stage-specific metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Gets or sets error information if the stage failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}