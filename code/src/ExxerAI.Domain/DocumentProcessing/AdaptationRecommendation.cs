namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents an adaptation recommendation based on processing history analysis.
/// </summary>
public class AdaptationRecommendation
{
    /// <summary>
    /// Gets or sets the unique identifier for this recommendation.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the type of recommendation.
    /// </summary>
    public RecommendationType Type { get; set; } = RecommendationType.SchemaImprovement;

    /// <summary>
    /// Gets or sets the priority of this recommendation.
    /// </summary>
    public RecommendationPriority Priority { get; set; } = RecommendationPriority.Medium;

    /// <summary>
    /// Gets or sets the description of the recommendation.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence in this recommendation.
    /// </summary>
    public float Confidence { get; set; } = 0f;

    /// <summary>
    /// Gets or sets the expected impact of implementing this recommendation.
    /// </summary>
    public string ExpectedImpact { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this recommendation was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether this recommendation has been implemented.
    /// </summary>
    public bool IsImplemented { get; set; } = false;

    /// <summary>
    /// Gets or sets when this recommendation was implemented.
    /// </summary>
    public DateTime? ImplementedAt { get; set; }
}