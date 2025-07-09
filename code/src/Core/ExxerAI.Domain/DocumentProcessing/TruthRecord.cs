namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a record in the primary source of truth system with complete audit trail
/// </summary>
public class TruthRecord
{
    /// <summary>
    /// Gets or sets the unique identifier for the truth record
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the extracted data that serves as the authoritative source
    /// </summary>
    public ExtractedData Data { get; set; } = new();

    /// <summary>
    /// Gets or sets the data source information
    /// </summary>
    public DataSource Source { get; set; } = new();

    /// <summary>
    /// Gets or sets the validation results for this truth record
    /// </summary>
    public ValidationResultDocument ValidationResultsDocument { get; set; } = new();

    /// <summary>
    /// Gets or sets when this truth record was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this truth record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the status of this truth record as string
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Gets or sets the data lineage identifier for audit tracking
    /// </summary>
    public string LineageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SHA-256 hash of the data for integrity verification
    /// </summary>
    [StringLength(64)]
    public string DataHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agentStatus of this truth record
    /// </summary>
    public TruthRecordStatus TruthStatus { get; set; } = TruthRecordStatus.Active;

    /// <summary>
    /// Gets or sets the version number for this record
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets the confidence score for this truth record (0.0 - 1.0)
    /// </summary>
    public float ConfidenceScore { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the conflict resolution information if applicable
    /// </summary>
    public ConflictResolution? ConflictResolution { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for this truth record
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Gets or sets the user or system that created this record
    /// </summary>
    [StringLength(255)]
    public string CreatedBy { get; set; } = "System";

    /// <summary>
    /// Gets or sets when this record was last modified
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Gets or sets who last modified this record
    /// </summary>
    [StringLength(255)]
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Gets whether this truth record is currently active
    /// </summary>
    public bool IsActive => TruthStatus == TruthRecordStatus.Active;

    /// <summary>
    /// Marks this truth record as superseded by a new version
    /// </summary>
    /// <param name="newVersionId">The ID of the new version</param>
    /// <param name="modifiedBy">Who performed the superseding</param>
    public void MarkAsSuperseded(string newVersionId, string modifiedBy)
    {
        TruthStatus = TruthRecordStatus.Superseded;
        Status = "Superseded";
        LastModified = DateTime.UtcNow;
        ModifiedBy = modifiedBy;
        Metadata["SupersededBy"] = newVersionId;
    }

    /// <summary>
    /// Marks this truth record as requiring human review
    /// </summary>
    /// <param name="reason">The reason for requiring review</param>
    public void RequireHumanReview(string reason)
    {
        TruthStatus = TruthRecordStatus.RequiresHumanReview;
        Status = "RequiresReview";
        LastModified = DateTime.UtcNow;
        Metadata["ReviewReason"] = reason;
        Metadata["ReviewRequested"] = DateTime.UtcNow;
    }
}