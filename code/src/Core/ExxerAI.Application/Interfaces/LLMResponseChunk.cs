namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents a chunk of streamed response from a language model
/// </summary>
public class LLMResponseChunk
{
    /// <summary>
    /// Gets or sets the content chunk
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is the final chunk
    /// </summary>
    public bool IsComplete { get; set; } = false;

    /// <summary>
    /// Gets or sets the chunk index
    /// </summary>
    public int ChunkIndex { get; set; }

    /// <summary>
    /// Gets or sets the reason the generation finished (if this is the last chunk)
    /// </summary>
    public string? FinishReason { get; set; }

    /// <summary>
    /// Gets or sets chunk metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = [];
}