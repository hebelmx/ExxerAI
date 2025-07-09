namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents data lineage tracking for audit purposes
/// </summary>
public class DataLineage
{
    /// <summary>
    /// Gets or sets the unique identifier for the lineage
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the record ID this lineage tracks
    /// </summary>
    public string RecordId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source documents involved
    /// </summary>
    public IEnumerable<string> SourceDocuments { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the processing history steps
    /// </summary>
    public IEnumerable<string> ProcessingHistory { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets when the lineage was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the source document or data
    /// </summary>
    public DataSource OriginalSource { get; set; } = new();

    /// <summary>
    /// Gets or sets the processing steps that transformed the data
    /// </summary>
    public List<ProcessingStep> ProcessingSteps { get; init; } = new();

    /// <summary>
    /// Gets or sets the final truth record ID
    /// </summary>
    [StringLength(255)]
    public string FinalTruthRecordId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional lineage metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}