namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Enumeration of document processing statuses.
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Document is currently being processed.
    /// </summary>
    Processing,

    /// <summary>
    /// Document is successfully processed and active.
    /// </summary>
    Active,

    /// <summary>
    /// Document has been deleted (soft delete).
    /// </summary>
    Deleted,

    /// <summary>
    /// Document is archived (superseded by newer version).
    /// </summary>
    Archived,

    /// <summary>
    /// Document processing failed.
    /// </summary>
    Error
}