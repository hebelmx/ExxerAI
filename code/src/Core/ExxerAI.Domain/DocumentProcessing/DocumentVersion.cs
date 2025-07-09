namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents document version information.
/// </summary>
public class DocumentVersion
{
    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    public int Number { get; set; } = 1;

    /// <summary>
    /// Gets or sets the version label (e.g., "final", "draft", "v2").
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the previous version document ID.
    /// </summary>
    public string? PreviousVersionId { get; set; }

    /// <summary>
    /// Gets or sets the next version document ID.
    /// </summary>
    public string? NextVersionId { get; set; }

    /// <summary>
    /// Gets or sets whether this is the latest version.
    /// </summary>
    public bool IsLatest { get; set; } = true;

    /// <summary>
    /// Gets or sets version-specific notes or comments.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this version was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}