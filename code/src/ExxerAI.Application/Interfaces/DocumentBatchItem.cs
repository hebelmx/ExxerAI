using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Individual document item for batch processing
/// </summary>
public class DocumentBatchItem
{
    /// <summary>
    /// Gets or sets the document binary data
    /// </summary>
    public byte[] DocumentData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the document metadata
    /// </summary>
    public DocumentMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the batch item identifier
    /// </summary>
    public string ItemId { get; set; } = string.Empty;
}