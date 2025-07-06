namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents conflict resolution information when multiple data sources conflict
/// </summary>
public class ConflictResolution
{
    /// <summary>
    /// Gets or sets the unique identifier for the conflict resolution
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the conflicting data sources
    /// </summary>
    public List<DataSource> ConflictingSources { get; init; } = new();

    /// <summary>
    /// Gets or sets the resolution strategy used
    /// </summary>
    public ConflictResolutionStrategy Strategy { get; set; } = ConflictResolutionStrategy.MostConfident;

    /// <summary>
    /// Gets or sets the resolved data
    /// </summary>
    public ExtractedData ResolvedData { get; set; } = new();

    /// <summary>
    /// Gets or sets whether human intervention was required
    /// </summary>
    public bool RequiresHumanIntervention { get; set; } = false;

    /// <summary>
    /// Gets or sets the confidence score for the resolution
    /// </summary>
    public float ResolutionConfidence { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets when the conflict was resolved
    /// </summary>
    public DateTime ResolvedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets who or what resolved the conflict
    /// </summary>
    [StringLength(255)]
    public string ResolvedBy { get; set; } = "System";

    /// <summary>
    /// Gets or sets additional resolution notes
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}