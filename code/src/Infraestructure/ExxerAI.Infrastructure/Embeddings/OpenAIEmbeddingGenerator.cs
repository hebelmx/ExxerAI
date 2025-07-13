using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Operations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;

namespace ExxerAI.Infrastructure.Embeddings;

/// <summary>
/// OpenAI implementation for generating text embeddings using Microsoft.Extensions.AI
/// </summary>
public class OpenAIEmbeddingGenerator : ExxerAI.Application.Interfaces.IEmbeddingGenerator
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly ILogger<OpenAIEmbeddingGenerator> _logger;
    private readonly string _modelName;
    private readonly SemaphoreSlim _rateLimitSemaphore;

    public OpenAIEmbeddingGenerator(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        string modelName,
        ILogger<OpenAIEmbeddingGenerator> logger)
    {
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        _modelName = !string.IsNullOrWhiteSpace(modelName) ? modelName : "text-embedding-3-small";
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Rate limiting: OpenAI allows high throughput, but we'll be conservative
        _rateLimitSemaphore = new SemaphoreSlim(10, 10);
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

        await _rateLimitSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
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

            var embedding = await _embeddingGenerator.GenerateAsync([text], options: null, cancellationToken).ConfigureAwait(false);
            var firstEmbedding = embedding.FirstOrDefault();
            
            if (firstEmbedding?.Vector != null)
            {
                _logger.LogDebug("Generated embedding with {Dimensions} dimensions", firstEmbedding.Vector.Length);
                return Result<float[]>.WithSuccess(firstEmbedding.Vector.ToArray());
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
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout during embedding generation");
            return Result<float[]>.WithFailure("Request timed out");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Generate embedding operation was cancelled");
            return ResultExtensions.Cancelled<float[]>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during embedding generation");
            return Result<float[]>.WithFailure($"Unexpected error: {ex.Message}");
        }
        finally
        {
            _rateLimitSemaphore.Release();
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
                return Result<IEnumerable<EmbeddingResult>>.WithSuccess(Enumerable.Empty<EmbeddingResult>());

            // Filter out null/empty texts
            var validTexts = textList.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            
            if (!validTexts.Any())
                return Result<IEnumerable<EmbeddingResult>>.WithFailure("No valid texts provided");

            _logger.LogDebug("Generating embeddings for {Count} texts", validTexts.Count);

            var allResults = new List<EmbeddingResult>();

            // Use Microsoft.Extensions.AI for batch processing
            var embeddings = await _embeddingGenerator.GenerateAsync(validTexts, options: null, cancellationToken).ConfigureAwait(false);
                
            for (int i = 0; i < validTexts.Count && i < embeddings.Count(); i++)
            {
                var embedding = embeddings[i];
                if (embedding?.Vector != null)
                {
                    allResults.Add(new EmbeddingResult
                    {
                        Text = validTexts[i],
                        Embedding = embedding.Vector.ToArray(),
                        TokenCount = EstimateTokenCount(validTexts[i])
                    });
                }
            }

            _logger.LogInformation("Generated {Count} embeddings successfully", allResults.Count);
            return Result<IEnumerable<EmbeddingResult>>.WithSuccess(allResults.AsEnumerable());
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


    private static int EstimateTokenCount(string text)
    {
        // Rough estimation: 4 characters per token for English text
        // This is an approximation - actual tokenization is more complex
        return string.IsNullOrEmpty(text) ? 0 : Math.Max(1, text.Length / 4);
    }

    public void Dispose()
    {
        _rateLimitSemaphore?.Dispose();
        _embeddingGenerator?.Dispose();
    }
}

