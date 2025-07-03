namespace ExxerAI.Domain.DocumentProcessing;

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