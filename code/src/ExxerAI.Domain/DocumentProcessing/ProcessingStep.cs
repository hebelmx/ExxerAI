using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a single processing step in the data lineage
/// </summary>
public class ProcessingStep
{
    /// <summary>
    /// Gets or sets the step identifier
    /// </summary>
    public string StepId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the name of the processing step
    /// </summary>
    [StringLength(255)]
    public string StepName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this step was executed
    /// </summary>
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the processor that executed this step
    /// </summary>
    [StringLength(255)]
    public string ProcessedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the input to this processing step
    /// </summary>
    public Dictionary<string, object> Input { get; init; } = new();

    /// <summary>
    /// Gets or sets the output from this processing step
    /// </summary>
    public Dictionary<string, object> Output { get; init; } = new();

    /// <summary>
    /// Gets or sets the duration of this processing step
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets whether this step was successful
    /// </summary>
    public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// Gets or sets any error message if the step failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}