namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents data quality requirements for validation.
/// </summary>
public class DataQualityRequirements
{
    /// <summary>
    /// Gets or sets the minimum overall confidence required.
    /// </summary>
    public float MinimumConfidence { get; set; } = 0.8f;

    /// <summary>
    /// Gets or sets the completeness threshold (percentage of required fields).
    /// </summary>
    public float CompletenessThreshold { get; set; } = 0.9f;

    /// <summary>
    /// Gets or sets whether empty values are allowed.
    /// </summary>
    public bool AllowEmptyValues { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to require manual review for low confidence.
    /// </summary>
    public bool RequireManualReviewForLowConfidence { get; set; } = true;

    /// <summary>
    /// Gets or sets field-specific quality requirements.
    /// </summary>
    public Dictionary<string, FieldQualityRequirement> FieldRequirements { get; set; } = new();
}