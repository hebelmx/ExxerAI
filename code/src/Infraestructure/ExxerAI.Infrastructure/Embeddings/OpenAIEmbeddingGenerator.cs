using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Operations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace ExxerAI.Infrastructure.Embeddings;

/// <summary>
/// OpenAI implementation for generating text embeddings
/// Uses Microsoft.Extensions.AI abstractions for consistent interface
/// </summary>
public class OpenAIEmbeddingGenerator : ExxerAI.Application.Interfaces.IEmbeddingGenerator
{
    private readonly Microsoft.Extensions.AI.IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly ILogger<OpenAIEmbeddingGenerator> _logger;
    private readonly string _modelName;

    public OpenAIEmbeddingGenerator(
        Microsoft.Extensions.AI.IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        ILogger<OpenAIEmbeddingGenerator> logger,
        string modelName = "text-embedding-3-small")
    {
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _modelName = modelName;
    }

    public int EmbeddingDimensions => _modelName switch
    {
        "text-embedding-3-small" => 1536,
        "text-embedding-3-large" => 3072,
        "text-embedding-ada-002" => 1536,
        _ => 1536 // Default fallback
    };

    public string ModelName => _modelName;

    public int MaxTokens => _modelName switch
    {
        "text-embedding-3-small" => 8191,
        "text-embedding-3-large" => 8191,
        "text-embedding-ada-002" => 8191,
        _ => 8191 // Default fallback
    };

    /// <summary>
    /// Generate embedding for single text input
    /// </summary>
    public async Task<Result<float[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<float[]>();

        try
        {
            if (string.IsNullOrWhiteSpace(text))
                return Result<float[]>.WithFailure("Text cannot be null or empty");

            // Check token limit (rough estimation: 4 chars per token)
            var estimatedTokens = text.Length / 4;
            if (estimatedTokens > MaxTokens)
            {
                _logger.LogWarning("Text may exceed token limit. Estimated tokens: {EstimatedTokens}, Max: {MaxTokens}", 
                    estimatedTokens, MaxTokens);
            }

            _logger.LogDebug("Generating embedding for text of length: {Length}", text.Length);

            var result = await _embeddingGenerator.GenerateAsync(new[] { text }, cancellationToken: cancellationToken);

            if (result?.Count > 0)
            {
                var embedding = result[0];
                var vector = embedding.Vector.ToArray();

                _logger.LogDebug("Generated embedding with {Dimensions} dimensions", vector.Length);
                return Result<float[]>.Success(vector);
            }

            return Result<float[]>.WithFailure("Failed to generate embedding - empty result");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument for embedding generation");
            return Result<float[]>.WithFailure($"Invalid input: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation during embedding generation");
            return Result<float[]>.WithFailure($"Operation failed: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during embedding generation");
            return Result<float[]>.WithFailure($"Network error: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Generate embedding operation was cancelled");
            return ResultExtensions.Cancelled<float[]>();
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout during embedding generation");
            return Result<float[]>.WithFailure("Request timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during embedding generation");
            return Result<float[]>.WithFailure($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate embeddings for multiple texts in batch for efficiency
    /// </summary>
    public async Task<Result<IEnumerable<EmbeddingResult>>> GenerateBatchEmbeddingsAsync(
        IEnumerable<string> texts,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<IEnumerable<EmbeddingResult>>();

        try
        {
            var textList = texts?.ToList() ?? [];
            
            if (!textList.Any())
                return Result<IEnumerable<EmbeddingResult>>.Success(Enumerable.Empty<EmbeddingResult>());

            // Filter out null/empty texts
            var validTexts = textList.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            
            if (!validTexts.Any())
                return Result<IEnumerable<EmbeddingResult>>.WithFailure("No valid texts provided");

            _logger.LogDebug("Generating embeddings for {Count} texts", validTexts.Count);

            // Process in batches to respect API limits
            const int batchSize = 100; // OpenAI batch limit
            var allResults = new List<EmbeddingResult>();

            for (int i = 0; i < validTexts.Count; i += batchSize)
            {
                var batch = validTexts.Skip(i).Take(batchSize).ToList();
                var batchResults = await GenerateBatchInternalAsync(batch, cancellationToken);
                
                if (batchResults.IsFailure)
                    return Result<IEnumerable<EmbeddingResult>>.WithFailure(batchResults.Error ?? "Batch generation failed");
                
                allResults.AddRange(batchResults.Value!);
            }

            _logger.LogInformation("Generated {Count} embeddings successfully", allResults.Count);
            return Result<IEnumerable<EmbeddingResult>>.Success(allResults.AsEnumerable());
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Generate batch embeddings operation was cancelled");
            return ResultExtensions.Cancelled<IEnumerable<EmbeddingResult>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate batch embeddings");
            return Result<IEnumerable<EmbeddingResult>>.WithFailure($"Batch embedding generation failed: {ex.Message}");
        }
    }

    private async Task<Result<List<EmbeddingResult>>> GenerateBatchInternalAsync(
        List<string> texts,
        CancellationToken cancellationToken)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<List<EmbeddingResult>>();

        try
        {
            var embeddings = await _embeddingGenerator.GenerateAsync(texts, cancellationToken: cancellationToken);

            var results = new List<EmbeddingResult>();
            for (int i = 0; i < texts.Count && i < embeddings.Count; i++)
            {
                var text = texts[i];
                var embedding = embeddings[i];
                
                results.Add(new EmbeddingResult
                {
                    Text = text,
                    Embedding = embedding.Vector.ToArray(),
                    TokenCount = EstimateTokenCount(text)
                });
            }

            return Result<List<EmbeddingResult>>.Success(results);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Generate batch internal operation was cancelled");
            return ResultExtensions.Cancelled<List<EmbeddingResult>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate batch embeddings");
            return Result<List<EmbeddingResult>>.WithFailure($"Batch generation failed: {ex.Message}");
        }
    }

    private static int EstimateTokenCount(string text)
    {
        // Rough estimation: 4 characters per token for English text
        // This is an approximation - actual tokenization is more complex
        return string.IsNullOrEmpty(text) ? 0 : Math.Max(1, text.Length / 4);
    }
}