namespace ExxerAI.Application.Services;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Configurations;
using ExxerAI.Domain.Helpers;

/// <summary>
/// Represents an active watch session for a Google Drive folder.
/// </summary>
internal class WatchSession
{
    /// <summary>
    /// Gets or sets the unique session identifier.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Google Drive folder ID being watched.
    /// </summary>
    public string FolderId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the watch session was started.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the watch session was stopped.
    /// </summary>
    public DateTime? StoppedAt { get; set; }

    /// <summary>
    /// Gets or sets whether the session is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the number of documents being watched in this session.
    /// </summary>
    public int DocumentsWatched { get; set; }

    /// <summary>
    /// Gets or sets the number of documents processed through this session.
    /// </summary>
    public int DocumentsProcessed { get; set; }

    /// <summary>
    /// Gets or sets when a document was last processed in this session.
    /// </summary>
    public DateTime? LastProcessedAt { get; set; }
}