namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Document notification information
/// </summary>
public record DocumentNotification(
    string Type,
    string DocumentId,
    string DocumentName,
    DateTime Timestamp,
    Dictionary<string, object> Properties);