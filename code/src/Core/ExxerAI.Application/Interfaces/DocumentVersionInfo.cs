namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Document version information
/// </summary>
public record DocumentVersionInfo(
    string Version,
    DateTime CreatedDate,
    DateTime ModifiedDate,
    string Hash,
    long Size,
    Dictionary<string, object> Properties);