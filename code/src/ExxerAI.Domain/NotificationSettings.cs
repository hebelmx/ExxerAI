namespace ExxerAI.Domain;

/// <summary>
/// Represents notification settings for a workflow
/// </summary>
public class NotificationSettings
{
    /// <summary>
    /// Gets or sets whether to notify on completion
    /// </summary>
    public bool NotifyOnCompletion { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to notify on failure
    /// </summary>
    public bool NotifyOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the notification recipients
    /// </summary>
    public ICollection<string> Recipients { get; init; } = new List<string>();
}