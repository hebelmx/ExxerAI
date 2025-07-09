using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents a document change event detected during folder watching.
/// </summary>
public class DocumentChangeEvent
{
    /// <summary>
    /// Gets or sets the unique event identifier.
    /// </summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the document ID that changed.
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of change that occurred.
    /// </summary>
    public DocumentChangeType ChangeType { get; set; }

    /// <summary>
    /// Gets or sets when the change was detected.
    /// </summary>
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the change actually occurred.
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// Gets or sets the document metadata at time of change.
    /// </summary>
    public DocumentMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the previous document hash for change detection.
    /// </summary>
    public string? PreviousHash { get; set; }

    /// <summary>
    /// Gets or sets the new document hash.
    /// </summary>
    public string? NewHash { get; set; }

    /// <summary>
    /// Gets or sets the watch session that detected this change.
    /// </summary>
    public string WatchSessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional change metadata.
    /// </summary>
    public Dictionary<string, object> Properties { get; set; } = new();

    /// <summary>
    /// Gets whether this change represents a content modification.
    /// </summary>
    public bool IsContentChange => ChangeType == DocumentChangeType.Modified &&
                                   !string.IsNullOrEmpty(PreviousHash) &&
                                   !string.IsNullOrEmpty(NewHash) &&
                                   !PreviousHash.Equals(NewHash);

    /// <summary>
    /// Gets whether this change requires processing.
    /// </summary>
    public bool RequiresProcessing => ChangeType == DocumentChangeType.Created ||
                                      ChangeType == DocumentChangeType.Modified ||
                                      ChangeType == DocumentChangeType.Restored;
}