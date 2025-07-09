namespace ExxerAI.Application.DTOs;

/// <summary>
/// Evolution history of patterns for a specific field
/// </summary>
public class PatternEvolutionHistory
{
    /// <summary>
    /// Gets or sets the field name
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the history entries
    /// </summary>
    public List<PatternHistoryEntry> History { get; set; } = [];
}