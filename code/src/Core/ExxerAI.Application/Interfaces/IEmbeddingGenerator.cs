using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for generating embeddings from text content
/// Abstracts different embedding providers (OpenAI, Sentence Transformers, etc.)
/// </summary>
public interface IEmbeddingGenerator
{
    /// <summary>
    /// Generate embeddings for a single text input
    /// </summary>
    /// <param name="text">Text content to embed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result<float[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate embeddings for multiple text inputs in batch
    /// </summary>
    /// <param name="texts">Collection of text content to embed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result<IEnumerable<EmbeddingResult>>> GenerateBatchEmbeddingsAsync(
        IEnumerable<string> texts, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the embedding model's vector dimensions
    /// </summary>
    int EmbeddingDimensions { get; }

    /// <summary>
    /// Get the embedding model name/identifier
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Get maximum input tokens supported by the model
    /// </summary>
    int MaxTokens { get; }
}

/// <summary>
/// Result from embedding generation
/// </summary>
public class EmbeddingResult
{
    public string Text { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public int TokenCount { get; set; }
}