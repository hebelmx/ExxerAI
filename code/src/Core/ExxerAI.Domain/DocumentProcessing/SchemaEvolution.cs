namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents the evolution of a schema over time.
/// </summary>
public class SchemaEvolution
{
    /// <summary>
    /// Gets or sets the unique identifier for this evolution record.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the schema ID that evolved.
    /// </summary>
    [StringLength(255)]
    public string SchemaId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the previous version number.
    /// </summary>
    public int PreviousVersion { get; set; } = 1; //why this default values

    /// <summary>
    /// Gets or sets the new version number.
    /// </summary>
    public int NewVersion { get; set; } = 2; //why this default values

    /// <summary>
    /// Gets or sets the changes made in this evolution.
    /// </summary>
    public List<string> Changes { get; set; } = new();

    /// <summary>
    /// Gets or sets the reason for the evolution.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this evolution occurred.
    /// </summary>
    public DateTime EvolvedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the impact of this evolution on processing accuracy.
    /// </summary>
    public float AccuracyImprovement { get; set; } = 0f;
}