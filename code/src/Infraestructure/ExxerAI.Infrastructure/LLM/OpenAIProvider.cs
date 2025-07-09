using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Operations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExxerAI.Infrastructure.LLM;

/// <summary>
/// OpenAI provider implementation for Large Language Model interactions
/// Supports both GPT-3.5 and GPT-4 models with streaming and function calling capabilities
/// </summary>
public class OpenAIProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIConfiguration _configuration;
    private Dictionary<string, LLMModelInfo> _modelInfo = [];

    /// <summary>
    /// Initializes a new instance of the OpenAIProvider class
    /// </summary>
    /// <param name="httpClient">HTTP client for API requests</param>
    /// <param name="configuration">OpenAI configuration settings</param>
    public OpenAIProvider(HttpClient httpClient, OpenAIConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        ConfigureHttpClient();
        InitializeModelInfo();
    }

    /// <summary>
    /// Gets the provider name
    /// </summary>
    public string ProviderName => "OpenAI";

    /// <summary>
    /// Gets the supported model identifiers for this provider
    /// </summary>
    public IEnumerable<string> SupportedModels => _modelInfo.Keys;

    /// <summary>
    /// Gets whether this provider supports streaming responses
    /// </summary>
    public bool SupportsStreaming => true;

    /// <summary>
    /// Gets whether this provider supports function calling
    /// </summary>
    public bool SupportsFunctionCalling => true;

    /// <summary>
    /// Generates a text completion using the specified model and parameters
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="prompt">The input prompt or system message</param>
    /// <param name="parameters">Generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The generated response</returns>
    public async Task<Result<LLMResponse>> GenerateCompletionAsync(
        string modelName,
        string prompt,
        LLMParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return Result<LLMResponse>.WithFailure("Model name cannot be empty");

            if (string.IsNullOrWhiteSpace(prompt))
                return Result<LLMResponse>.WithFailure("Prompt cannot be empty");

            if (!_modelInfo.ContainsKey(modelName))
                return Result<LLMResponse>.WithFailure($"Unsupported model: {modelName}");

            var messages = new List<ChatMessage>
            {
                ChatMessage.User(prompt)
            };

            return await GenerateChatCompletionAsync(modelName, messages, parameters, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result<LLMResponse>.WithFailure($"Error generating completion: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a chat completion using the specified model and messages
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="messages">The conversation messages</param>
    /// <param name="parameters">Generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The generated response</returns>
    public async Task<Result<LLMResponse>> GenerateChatCompletionAsync(
        string modelName,
        IEnumerable<ChatMessage> messages,
        LLMParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return Result<LLMResponse>.WithFailure("Model name cannot be empty");

            if (messages == null || !messages.Any())
                return Result<LLMResponse>.WithFailure("Messages cannot be empty");

            if (!_modelInfo.ContainsKey(modelName))
                return Result<LLMResponse>.WithFailure($"Unsupported model: {modelName}");

            var request = CreateChatCompletionRequest(modelName, messages, parameters);
            var requestJson = JsonSerializer.Serialize(request, JsonSerializerOptions);

            var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var startTime = DateTime.UtcNow;

            var response = await _httpClient.PostAsync("chat/completions", httpContent, cancellationToken)
                .ConfigureAwait(false);

            var responseTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return Result<LLMResponse>.WithFailure($"OpenAI API error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var openAIResponse = JsonSerializer.Deserialize<OpenAIChatCompletionResponse>(responseContent, JsonSerializerOptions);

            if (openAIResponse?.Choices == null || !openAIResponse.Choices.Any())
                return Result<LLMResponse>.WithFailure("No response choices returned from OpenAI");

            var choice = openAIResponse.Choices[0];
            var usage = openAIResponse.Usage ?? new OpenAIUsage();

            var llmResponse = new LLMResponse
            {
                Content = choice.Message?.Content ?? string.Empty,
                InputTokens = usage.PromptTokens,
                OutputTokens = usage.CompletionTokens,
                EstimatedCost = CalculateCost(modelName, usage.PromptTokens, usage.CompletionTokens),
                ResponseTimeMs = responseTime,
                FinishReason = choice.FinishReason ?? "unknown",
                Metadata = new Dictionary<string, object>
                {
                    { "model", modelName },
                    { "provider", ProviderName },
                    { "usage", usage },
                    { "response_id", openAIResponse.Id ?? string.Empty }
                }
            };

            return Result<LLMResponse>.WithSuccess(llmResponse);
        }
        catch (HttpRequestException ex)
        {
            return Result<LLMResponse>.WithFailure($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            return Result<LLMResponse>.WithFailure($"Request timeout: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return Result<LLMResponse>.WithFailure($"JSON parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<LLMResponse>.WithFailure($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Streams a text completion using the specified model and parameters
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="prompt">The input prompt</param>
    /// <param name="parameters">Generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of response chunks</returns>
    public async IAsyncEnumerable<LLMResponseChunk> StreamCompletionAsync(
        string modelName,
        string prompt,
        LLMParameters? parameters = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.User(prompt)
        };

        await foreach (var chunk in StreamChatCompletionAsync(modelName, messages, parameters, cancellationToken)
                          .WithCancellation(cancellationToken))
        {
            yield return chunk;
        }
    }

    /// <summary>
    /// Streams a chat completion using the specified model and messages
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="messages">The conversation messages</param>
    /// <param name="parameters">Generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of response chunks</returns>
    public async IAsyncEnumerable<LLMResponseChunk> StreamChatCompletionAsync(
        string modelName,
        IEnumerable<ChatMessage> messages,
        LLMParameters? parameters = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelName) || messages == null || !messages.Any())
            yield break;

        var request = CreateChatCompletionRequest(modelName, messages, parameters, stream: true);
        var requestJson = JsonSerializer.Serialize(request, JsonSerializerOptions);

        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("chat/completions", httpContent, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            yield break;

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream);

        var chunkIndex = 0;
        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var data = line[6..]; // Remove "data: " prefix
            if (data == "[DONE]")
                break;

            LLMResponseChunk? chunk = null;
            try
            {
                var chunkResponse = JsonSerializer.Deserialize<OpenAIStreamResponse>(data, JsonSerializerOptions);
                if (chunkResponse?.Choices != null && chunkResponse.Choices.Any())
                {
                    var choice = chunkResponse.Choices[0];
                    var content = choice.Delta?.Content ?? string.Empty;

                    if (!string.IsNullOrEmpty(content))
                    {
                        chunk = new LLMResponseChunk
                        {
                            Content = content,
                            ChunkIndex = chunkIndex++,
                            FinishReason = choice.FinishReason,
                            Metadata = new Dictionary<string, object>
                            {
                                { "model", modelName },
                                { "provider", ProviderName },
                                { "response_id", chunkResponse.Id ?? string.Empty }
                            }
                        };
                    }
                }
            }
            catch (JsonException)
            {
                // Skip malformed chunks
            }

            if (chunk != null)
                yield return chunk;
        }
    }

    /// <summary>
    /// Counts the number of tokens in the given text for the specified model
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="text">The text to count tokens for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The token count</returns>
    public async Task<Result<int>> CountTokensAsync(
        string modelName,
        string text,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
                return Result<int>.WithSuccess(0);

            // Simple approximation: 1 token ≈ 4 characters for English text
            // This is a fallback implementation - ideally use tiktoken or similar
            await Task.Yield(); // Allow cooperative cancellation

            var approximateTokens = (int)Math.Ceiling(text.Length / 4.0);

            // Apply model-specific adjustments
            var adjustment = modelName.ToLowerInvariant() switch
            {
                var name when name.Contains("gpt-4") => 1.1, // GPT-4 tends to use slightly more tokens
                var name when name.Contains("gpt-3.5") => 1.0,
                _ => 1.0
            };

            var estimatedTokens = (int)(approximateTokens * adjustment);

            return Result<int>.WithSuccess(estimatedTokens);
        }
        catch (Exception ex)
        {
            return Result<int>.WithFailure($"Error counting tokens: {ex.Message}");
        }
    }

    /// <summary>
    /// Estimates the cost for the given token counts using the specified model
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="inputTokens">Number of input tokens</param>
    /// <param name="outputTokens">Number of output tokens</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The estimated cost in USD</returns>
    public async Task<Result<decimal>> EstimateCostAsync(
        string modelName,
        int inputTokens,
        int outputTokens,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_modelInfo.TryGetValue(modelName, out var modelInfo))
                return Result<decimal>.WithFailure($"Unknown model: {modelName}");

            await Task.Yield(); // Allow cooperative cancellation

            var inputCost = (inputTokens / 1000m) * modelInfo.InputTokenCostPer1K;
            var outputCost = (outputTokens / 1000m) * modelInfo.OutputTokenCostPer1K;
            var totalCost = inputCost + outputCost;

            return Result<decimal>.WithSuccess(totalCost);
        }
        catch (Exception ex)
        {
            return Result<decimal>.WithFailure($"Error calculating cost: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates that the provider is properly configured and the model is accessible
    /// </summary>
    /// <param name="modelName">The model name to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with any error messages</returns>
    public async Task<Result<ProviderValidationResult>> ValidateAsync(
        string modelName,
        CancellationToken cancellationToken = default)
    {
        var result = new ProviderValidationResult
        {
            TestedModel = modelName
        };

        try
        {
            var startTime = DateTime.UtcNow;

            // Test with a simple prompt
            var testMessages = new List<ChatMessage>
            {
                ChatMessage.User("Hello")
            };

            var testParams = new LLMParameters
            {
                MaxTokens = 10,
                Temperature = 0.1
            };

            var response = await GenerateChatCompletionAsync(modelName, testMessages, testParams, cancellationToken)
                .ConfigureAwait(false);

            result.ResponseTime = DateTime.UtcNow - startTime;

            if (response.IsSuccess)
            {
                result.IsValid = true;
            }
            else
            {
                result.IsValid = false;
                result.Errors.Add($"API test failed: {response.Error}");
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }

        return Result<ProviderValidationResult>.WithSuccess(result);
    }

    /// <summary>
    /// Gets the rate limiting information for this provider
    /// </summary>
    /// <param name="modelName">The model name/identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rate limiting information</returns>
    public async Task<Result<RateLimitInfo>> GetRateLimitInfoAsync(
        string modelName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // OpenAI rate limits vary by tier and model
            // These are conservative defaults - should be updated based on actual account limits
            await Task.Yield(); // Allow cooperative cancellation

            var rateLimitInfo = modelName.ToLowerInvariant() switch
            {
                var name when name.Contains("gpt-4") => new RateLimitInfo
                {
                    RequestsPerMinute = 500,
                    TokensPerMinute = 30_000,
                    RemainingRequests = 500,
                    RemainingTokens = 30_000,
                    ResetTime = DateTime.UtcNow.AddMinutes(1),
                    IsLimitExceeded = false
                },
                var name when name.Contains("gpt-3.5") => new RateLimitInfo
                {
                    RequestsPerMinute = 3500,
                    TokensPerMinute = 90_000,
                    RemainingRequests = 3500,
                    RemainingTokens = 90_000,
                    ResetTime = DateTime.UtcNow.AddMinutes(1),
                    IsLimitExceeded = false
                },
                _ => new RateLimitInfo
                {
                    RequestsPerMinute = 1000,
                    TokensPerMinute = 40_000,
                    RemainingRequests = 1000,
                    RemainingTokens = 40_000,
                    ResetTime = DateTime.UtcNow.AddMinutes(1),
                    IsLimitExceeded = false
                }
            };

            return Result<RateLimitInfo>.WithSuccess(rateLimitInfo);
        }
        catch (Exception ex)
        {
            return Result<RateLimitInfo>.WithFailure($"Error getting rate limit info: {ex.Message}");
        }
    }

    /// <summary>
    /// Lists available models for this provider
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of available models</returns>
    public async Task<Result<IEnumerable<LLMModelInfo>>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Yield(); // Allow cooperative cancellation
            return Result<IEnumerable<LLMModelInfo>>.WithSuccess(_modelInfo.Values);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<LLMModelInfo>>.WithFailure($"Error listing models: {ex.Message}");
        }
    }

    #region Private Methods

    /// <summary>
    /// Configures the HTTP client with OpenAI-specific settings
    /// </summary>
    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _configuration.ApiKey);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("ExxerAI", "1.0"));
        _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.TimeoutSeconds);
    }

    /// <summary>
    /// Initializes model information and pricing
    /// </summary>
    private void InitializeModelInfo()
    {
        _modelInfo = new Dictionary<string, LLMModelInfo>
        {
            ["gpt-4o"] = new()
            {
                Id = "gpt-4o",
                Name = "GPT-4o",
                Description = "Most advanced GPT-4 model with improved capabilities",
                MaxContextLength = 128000,
                MaxOutputTokens = 4096,
                InputTokenCostPer1K = 0.005m,
                OutputTokenCostPer1K = 0.015m,
                SupportsFunctionCalling = true,
                SupportsStreaming = true,
                Capabilities = ["text", "function_calling", "json_mode"]
            },
            ["gpt-4-turbo"] = new()
            {
                Id = "gpt-4-turbo",
                Name = "GPT-4 Turbo",
                Description = "Efficient GPT-4 model with good performance and cost balance",
                MaxContextLength = 128000,
                MaxOutputTokens = 4096,
                InputTokenCostPer1K = 0.01m,
                OutputTokenCostPer1K = 0.03m,
                SupportsFunctionCalling = true,
                SupportsStreaming = true,
                Capabilities = ["text", "function_calling", "json_mode"]
            },
            ["gpt-3.5-turbo"] = new()
            {
                Id = "gpt-3.5-turbo",
                Name = "GPT-3.5 Turbo",
                Description = "Fast and cost-effective model for most tasks",
                MaxContextLength = 16384,
                MaxOutputTokens = 4096,
                InputTokenCostPer1K = 0.0005m,
                OutputTokenCostPer1K = 0.0015m,
                SupportsFunctionCalling = true,
                SupportsStreaming = true,
                Capabilities = ["text", "function_calling"]
            }
        };
    }

    /// <summary>
    /// Creates a chat completion request object
    /// </summary>
    private OpenAIChatCompletionRequest CreateChatCompletionRequest(
        string modelName,
        IEnumerable<ChatMessage> messages,
        LLMParameters? parameters = null,
        bool stream = false)
    {
        var openAIMessages = messages.Select(m => new OpenAIMessage
        {
            Role = m.Role,
            Content = m.Content,
            Name = m.Name
        }).ToList();

        return new OpenAIChatCompletionRequest
        {
            Model = modelName,
            Messages = openAIMessages,
            MaxTokens = parameters?.MaxTokens ?? 1000,
            Temperature = parameters?.Temperature ?? 0.7,
            TopP = parameters?.TopP ?? 1.0,
            FrequencyPenalty = parameters?.FrequencyPenalty ?? 0.0,
            PresencePenalty = parameters?.PresencePenalty ?? 0.0,
            Stop = parameters?.StopSequences?.ToList(),
            Stream = stream
        };
    }

    /// <summary>
    /// Calculates the cost for a request based on token usage
    /// </summary>
    private decimal CalculateCost(string modelName, int inputTokens, int outputTokens)
    {
        if (!_modelInfo.TryGetValue(modelName, out var modelInfo))
            return 0;

        var inputCost = (inputTokens / 1000m) * modelInfo.InputTokenCostPer1K;
        var outputCost = (outputTokens / 1000m) * modelInfo.OutputTokenCostPer1K;
        return inputCost + outputCost;
    }

    /// <summary>
    /// JSON serializer options for OpenAI API
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    #endregion Private Methods
}

/// <summary>
/// OpenAI provider configuration
/// </summary>
public class OpenAIConfiguration
{
    /// <summary>
    /// Gets or sets the OpenAI API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for OpenAI API
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";

    /// <summary>
    /// Gets or sets the request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Gets or sets the organization ID (optional)
    /// </summary>
    public string? OrganizationId { get; set; }

    /// <summary>
    /// Gets or sets the project ID (optional)
    /// </summary>
    public string? ProjectId { get; set; }
}

#region OpenAI API Models

/// <summary>
/// OpenAI chat completion request model
/// </summary>
internal class OpenAIChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OpenAIMessage> Messages { get; set; } = [];

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; set; }

    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; set; }

    [JsonPropertyName("stop")]
    public List<string>? Stop { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}

/// <summary>
/// OpenAI message model
/// </summary>
internal class OpenAIMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// OpenAI chat completion response model
/// </summary>
internal class OpenAIChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("choices")]
    public List<OpenAIChoice> Choices { get; set; } = [];

    [JsonPropertyName("usage")]
    public OpenAIUsage? Usage { get; set; }
}

/// <summary>
/// OpenAI choice model
/// </summary>
internal class OpenAIChoice
{
    [JsonPropertyName("message")]
    public OpenAIMessage? Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

/// <summary>
/// OpenAI usage model
/// </summary>
internal class OpenAIUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

/// <summary>
/// OpenAI streaming response model
/// </summary>
internal class OpenAIStreamResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("choices")]
    public List<OpenAIStreamChoice> Choices { get; set; } = [];
}

/// <summary>
/// OpenAI streaming choice model
/// </summary>
internal class OpenAIStreamChoice
{
    [JsonPropertyName("delta")]
    public OpenAIDelta? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

/// <summary>
/// OpenAI delta model for streaming
/// </summary>
internal class OpenAIDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }
}

#endregion OpenAI API Models