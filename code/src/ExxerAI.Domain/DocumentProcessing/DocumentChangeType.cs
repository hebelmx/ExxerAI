namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Enumeration of document change types
/// </summary>
public enum DocumentChangeType
{
    /// <summary>
    /// Document was created
    /// </summary>
    Created,

    /// <summary>
    /// Document was modified
    /// </summary>
    Modified,

    /// <summary>
    /// Document was deleted
    /// </summary>
    Deleted,

    /// <summary>
    /// Document was moved
    /// </summary>
    Moved,

    /// <summary>
    /// Document was renamed
    /// </summary>
    Renamed,

    /// <summary>
    /// Document was restored
    /// </summary>
    Restored
}