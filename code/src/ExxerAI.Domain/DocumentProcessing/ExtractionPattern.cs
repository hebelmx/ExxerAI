using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents an extraction pattern for finding field values in documents
/// </summary>
public abstract class ExtractionPattern
{
    /// <summary>
    /// Gets or sets the pattern identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the pattern name
    /// </summary>
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence score for this pattern (0.0 - 1.0)
    /// </summary>
    public float Confidence { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets whether this pattern is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets additional pattern metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Attempts to extract a value using this pattern
    /// </summary>
    /// <param name="text">The text to search in</param>
    /// <param name="context">Additional context for extraction</param>
    /// <returns>The extracted value or null if not found</returns>
    public abstract string? ExtractValue(string text, ExtractionContext context);
}