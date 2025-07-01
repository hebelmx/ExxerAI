using System.ComponentModel.DataAnnotations;

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
    public ValidationResult ValidationResults { get; set; } = new();

    /// <summary>
    /// Gets or sets when this truth record was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

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
    /// Gets or sets the status of this truth record
    /// </summary>
    public TruthRecordStatus Status { get; set; } = TruthRecordStatus.Active;

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
    public bool IsActive => Status == TruthRecordStatus.Active;

    /// <summary>
    /// Marks this truth record as superseded by a new version
    /// </summary>
    /// <param name="newVersionId">The ID of the new version</param>
    /// <param name="modifiedBy">Who performed the superseding</param>
    public void MarkAsSuperseded(string newVersionId, string modifiedBy)
    {
        Status = TruthRecordStatus.Superseded;
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
        Status = TruthRecordStatus.RequiresHumanReview;
        LastModified = DateTime.UtcNow;
        Metadata["ReviewReason"] = reason;
        Metadata["ReviewRequested"] = DateTime.UtcNow;
    }
}

/// <summary>
/// Represents the possible states of a truth record
/// </summary>
public enum TruthRecordStatus
{
    /// <summary>
    /// Record is active and serves as the authoritative source
    /// </summary>
    Active,
    
    /// <summary>
    /// Record has been superseded by a newer version
    /// </summary>
    Superseded,
    
    /// <summary>
    /// Record is under review and may not be reliable
    /// </summary>
    UnderReview,
    
    /// <summary>
    /// Record requires human review before being activated
    /// </summary>
    RequiresHumanReview,
    
    /// <summary>
    /// Record has been archived and is no longer active
    /// </summary>
    Archived,
    
    /// <summary>
    /// Record has been marked as invalid
    /// </summary>
    Invalid
}

/// <summary>
/// Represents information about the source of data
/// </summary>
public class DataSource
{
    /// <summary>
    /// Gets or sets the type of the data source
    /// </summary>
    [StringLength(100)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the source
    /// </summary>
    [StringLength(255)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path or location of the source
    /// </summary>
    [StringLength(1000)]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets who or what processed this data
    /// </summary>
    [StringLength(255)]
    public string ProcessedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MCP session ID if applicable
    /// </summary>
    [StringLength(255)]
    public string? MCPSessionId { get; set; }

    /// <summary>
    /// Gets or sets additional source metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Gets or sets when the source was accessed
    /// </summary>
    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;
}

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

/// <summary>
/// Represents the strategies available for resolving data conflicts
/// </summary>
public enum ConflictResolutionStrategy
{
    /// <summary>
    /// Use the data with the highest confidence score
    /// </summary>
    MostConfident,
    
    /// <summary>
    /// Use the most recent data
    /// </summary>
    MostRecent,
    
    /// <summary>
    /// Use the data from the most trusted source
    /// </summary>
    MostTrusted,
    
    /// <summary>
    /// Merge data from multiple sources
    /// </summary>
    Merge,
    
    /// <summary>
    /// Require human intervention to resolve
    /// </summary>
    HumanReview,
    
    /// <summary>
    /// Use a custom algorithm to resolve
    /// </summary>
    Custom
}

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
    /// Gets or sets when the lineage was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets additional lineage metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

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