namespace ExxerAI.Application.DTOs;

/// <summary>
/// Export format for pattern dictionary data
/// </summary>
public class PatternDictionaryExport
{
    /// <summary>
    /// Gets or sets the export timestamp
    /// </summary>
    public DateTime ExportedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of exported patterns
    /// </summary>
    public List<PatternDictionaryEntity> Patterns { get; set; } = new();

    /// <summary>
    /// Gets or sets export metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the export format version
    /// </summary>
    public string Version { get; set; } = "1.0";
} 