using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Services;

/// <summary>
/// Enhanced LLM service that supports multiple providers with rate limiting, cost tracking,
/// and intelligent provider selection based on model requirements and availability
/// </summary>
public class EnhancedLLMService : ILLMService
{
    private readonly IEnumerable<ILLMProvider> _providers;
    private readonly ILanguageModelRepository _modelRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly LLMServiceConfiguration _configuration;
    private readonly Dictionary<string, ILLMProvider> _providerCache;
    private readonly Dictionary<string, DateTime> _lastRateLimitCheck;
    private readonly Dictionary<string, decimal> _dailyCostTracker;

    /// <summary>
    /// Initializes a new instance of the EnhancedLLMService class
    /// </summary>
    /// <param name="providers">Available LLM providers</param>
    /// <param name="modelRepository">Language model repository</param>
    /// <param name="conversationRepository">Conversation repository</param>
    /// <param name="configuration">Service configuration</param>
    public EnhancedLLMService(
        IEnumerable<ILLMProvider> providers,
        ILanguageModelRepository modelRepository,
        IConversationRepository conversationRepository,
        LLMServiceConfiguration configuration)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _providerCache = new Dictionary<string, ILLMProvider>();
        _lastRateLimitCheck = new Dictionary<string, DateTime>();
        _dailyCostTracker = new Dictionary<string, decimal>();

        InitializeProviderCache();
    }

    /// <summary>
    /// Generates a text response using the specified language model
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="prompt">The input prompt</param>
    /// <param name="parameters">Optional generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The generated response</returns>
    public async Task<Result<LLMResponse>> GenerateTextAsync(
        Guid modelId,
        string prompt,
        LLMParameters? parameters = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (modelId == Guid.Empty)
                return Result<LLMResponse>.WithFailure("Invalid model identifier");

            if (string.IsNullOrWhiteSpace(prompt))
                return Result<LLMResponse>.WithFailure("Prompt cannot be empty");

            // Get model information
            var modelResult = await _modelRepository.GetByIdAsync(modelId, cancellationToken).ConfigureAwait(false);
            if (!modelResult.IsSuccess)
                return Result<LLMResponse>.WithFailure($"Model not found: {modelResult.Error}");

            var model = modelResult.Value;

            // Get provider for the model
            var providerResult = await GetProviderForModelAsync(model, cancellationToken).ConfigureAwait(false);
            if (!providerResult.IsSuccess)
                return Result<LLMResponse>.WithFailure($"Provider not available: {providerResult.Error}");

            var provider = providerResult.Value;

            // Check rate limits
            var rateLimitResult = await CheckRateLimitsAsync(provider, model.Name, cancellationToken).ConfigureAwait(false);
            if (!rateLimitResult.IsSuccess)
                return Result<LLMResponse>.WithFailure($"Rate limit exceeded: {rateLimitResult.Error}");

            // Validate cost constraints
            var costValidationResult = await ValidateCostConstraintsAsync(provider, model.Name, parameters, cancellationToken).ConfigureAwait(false);
            if (!costValidationResult.IsSuccess)
                return Result<LLMResponse>.WithFailure($"Cost constraint violation: {costValidationResult.Error}");

            // Generate response
            var response = await provider.GenerateCompletionAsync(model.Name, prompt, parameters, cancellationToken).ConfigureAwait(false);
            
            if (!response.IsSuccess)
                return response;

            // Track costs
            await TrackCostAsync(model.Name, response.Value.EstimatedCost).ConfigureAwait(false);

            // Add model metadata
            response.Value.Metadata["model_id"] = modelId;
            response.Value.Metadata["model_name"] = model.Name;
            response.Value.Metadata["provider_name"] = provider.ProviderName;

            return response;
        }
        catch (Exception ex)
        {
            return Result<LLMResponse>.WithFailure($"Error generating text: {ex.Message}");
        }
    }

    /// <summary>
    /// Continues a conversation with a language model
    /// </summary>
    /// <param name="conversationId">The conversation identifier</param>
    /// <param name="message">The new message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The model's response</returns>
    public async Task<Result<ConversationMessage>> ContinueConversationAsync(
        Guid conversationId,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (conversationId == Guid.Empty)
                return Result<ConversationMessage>.WithFailure("Invalid conversation identifier");

            if (string.IsNullOrWhiteSpace(message))
                return Result<ConversationMessage>.WithFailure("Message cannot be empty");

            // Get conversation
            var conversationResult = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken).ConfigureAwait(false);
            if (!conversationResult.IsSuccess)
                return Result<ConversationMessage>.WithFailure($"Conversation not found: {conversationResult.Error}");

            var conversation = conversationResult.Value;

            // Get model information
            var modelResult = await _modelRepository.GetByIdAsync(conversation.LanguageModelId, cancellationToken).ConfigureAwait(false);
            if (!modelResult.IsSuccess)
                return Result<ConversationMessage>.WithFailure($"Model not found: {modelResult.Error}");

            var model = modelResult.Value;

            // Get provider
            var providerResult = await GetProviderForModelAsync(model, cancellationToken).ConfigureAwait(false);
            if (!providerResult.IsSuccess)
                return Result<ConversationMessage>.WithFailure($"Provider not available: {providerResult.Error}");

            var provider = providerResult.Value;

            // Build conversation history
            var messages = new List<ChatMessage>();

            // Add system prompt if present
            if (!string.IsNullOrWhiteSpace(conversation.SystemPrompt))
            {
                messages.Add(ChatMessage.System(conversation.SystemPrompt));
            }

            // Add conversation history
            foreach (var historyMessage in conversation.Messages.OrderBy(m => m.CreatedAt))
            {
                var role = historyMessage.Role.ToString().ToLowerInvariant();
                messages.Add(new ChatMessage { Role = role, Content = historyMessage.Content });
            }

            // Add new user message
            messages.Add(ChatMessage.User(message));

            // Generate response
            var parameters = new LLMParameters
            {
                MaxTokens = _configuration.DefaultMaxTokens,
                Temperature = _configuration.DefaultTemperature
            };

            var response = await provider.GenerateChatCompletionAsync(model.Name, messages, parameters, cancellationToken).ConfigureAwait(false);
            
            if (!response.IsSuccess)
                return Result<ConversationMessage>.WithFailure($"Failed to generate response: {response.Error}");

            // Create user message
            var userMessage = new ConversationMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = MessageRole.User,
                Content = message,
                CreatedAt = DateTime.UtcNow
            };

            // Create assistant message
            var assistantMessage = new ConversationMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = MessageRole.Assistant,
                Content = response.Value.Content,
                CreatedAt = DateTime.UtcNow,
                Metadata = new ConversationMetadata
                {
                    TokensUsed = response.Value.TotalTokens,
                    Cost = response.Value.EstimatedCost,
                    ResponseTime = response.Value.ResponseTimeMs,
                    ModelName = model.Name,
                    ProviderName = provider.ProviderName
                }
            };

            // Add messages to conversation
            conversation.Messages.Add(userMessage);
            conversation.Messages.Add(assistantMessage);
            conversation.UpdatedAt = DateTime.UtcNow;

            // Update conversation in repository
            await _conversationRepository.UpdateAsync(conversation, cancellationToken).ConfigureAwait(false);

            // Track costs
            await TrackCostAsync(model.Name, response.Value.EstimatedCost).ConfigureAwait(false);

            return Result<ConversationMessage>.WithSuccess(assistantMessage);
        }
        catch (Exception ex)
        {
            return Result<ConversationMessage>.WithFailure($"Error continuing conversation: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new conversation with a language model
    /// </summary>
    /// <param name="agentId">The agent identifier</param>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="title">Optional conversation title</param>
    /// <param name="systemPrompt">Optional system prompt</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created conversation</returns>
    public async Task<Result<Conversation>> CreateConversationAsync(
        Guid agentId,
        Guid modelId,
        string? title = null,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (agentId == Guid.Empty)
                return Result<Conversation>.WithFailure("Invalid agent identifier");

            if (modelId == Guid.Empty)
                return Result<Conversation>.WithFailure("Invalid model identifier");

            // Verify model exists
            var modelResult = await _modelRepository.GetByIdAsync(modelId, cancellationToken).ConfigureAwait(false);
            if (!modelResult.IsSuccess)
                return Result<Conversation>.WithFailure($"Model not found: {modelResult.Error}");

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                AgentId = agentId,
                LanguageModelId = modelId,
                Title = title ?? "New Conversation",
                SystemPrompt = systemPrompt ?? string.Empty,
                Status = ConversationStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Messages = new List<ConversationMessage>(),
                Metadata = new ConversationMetadata()
            };

            var result = await _conversationRepository.AddAsync(conversation, cancellationToken).ConfigureAwait(false);
            
            if (!result.IsSuccess)
                return Result<Conversation>.WithFailure($"Failed to create conversation: {result.Error}");

            return Result<Conversation>.WithSuccess(result.Value);
        }
        catch (Exception ex)
        {
            return Result<Conversation>.WithFailure($"Error creating conversation: {ex.Message}");
        }
    }

    /// <summary>
    /// Estimates the cost of a language model request
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="inputTokens">The number of input tokens</param>
    /// <param name="outputTokens">The estimated number of output tokens</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The estimated cost</returns>
    public async Task<Result<decimal>> EstimateCostAsync(
        Guid modelId,
        int inputTokens,
        int outputTokens,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (modelId == Guid.Empty)
                return Result<decimal>.WithFailure("Invalid model identifier");

            // Get model information
            var modelResult = await _modelRepository.GetByIdAsync(modelId, cancellationToken).ConfigureAwait(false);
            if (!modelResult.IsSuccess)
                return Result<decimal>.WithFailure($"Model not found: {modelResult.Error}");

            var model = modelResult.Value;

            // Get provider
            var providerResult = await GetProviderForModelAsync(model, cancellationToken).ConfigureAwait(false);
            if (!providerResult.IsSuccess)
                return Result<decimal>.WithFailure($"Provider not available: {providerResult.Error}");

            var provider = providerResult.Value;

            return await provider.EstimateCostAsync(model.Name, inputTokens, outputTokens, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result<decimal>.WithFailure($"Error estimating cost: {ex.Message}");
        }
    }

    /// <summary>
    /// Counts the number of tokens in a text string for a specific model
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="text">The text to count tokens for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The token count</returns>
    public async Task<Result<int>> CountTokensAsync(
        Guid modelId,
        string text,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (modelId == Guid.Empty)
                return Result<int>.WithFailure("Invalid model identifier");

            if (string.IsNullOrWhiteSpace(text))
                return Result<int>.WithSuccess(0);

            // Get model information
            var modelResult = await _modelRepository.GetByIdAsync(modelId, cancellationToken).ConfigureAwait(false);
            if (!modelResult.IsSuccess)
                return Result<int>.WithFailure($"Model not found: {modelResult.Error}");

            var model = modelResult.Value;

            // Get provider
            var providerResult = await GetProviderForModelAsync(model, cancellationToken).ConfigureAwait(false);
            if (!providerResult.IsSuccess)
                return Result<int>.WithFailure($"Provider not available: {providerResult.Error}");

            var provider = providerResult.Value;

            return await provider.CountTokensAsync(model.Name, text, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result<int>.WithFailure($"Error counting tokens: {ex.Message}");
        }
    }

    /// <summary>
    /// Streams a text response using the specified language model
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="prompt">The input prompt</param>
    /// <param name="parameters">Optional generation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of response chunks</returns>
    public async IAsyncEnumerable<LLMResponseChunk> StreamTextAsync(
        Guid modelId,
        string prompt,
        LLMParameters? parameters = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (modelId == Guid.Empty || string.IsNullOrWhiteSpace(prompt))
            yield break;

        // Get model information
        var modelResult = await _modelRepository.GetByIdAsync(modelId, cancellationToken).ConfigureAwait(false);
        if (!modelResult.IsSuccess)
            yield break;

        var model = modelResult.Value;

        // Get provider
        var providerResult = await GetProviderForModelAsync(model, cancellationToken).ConfigureAwait(false);
        if (!providerResult.IsSuccess)
            yield break;

        var provider = providerResult.Value;

        if (!provider.SupportsStreaming)
            yield break;

        await foreach (var chunk in provider.StreamCompletionAsync(model.Name, prompt, parameters, cancellationToken)
                          .WithCancellation(cancellationToken))
        {
            // Add model metadata to chunks
            chunk.Metadata["model_id"] = modelId;
            chunk.Metadata["model_name"] = model.Name;
            chunk.Metadata["provider_name"] = provider.ProviderName;
            
            yield return chunk;
        }
    }

    /// <summary>
    /// Validates whether a language model is available and accessible
    /// </summary>
    /// <param name="modelId">The language model identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The validation result</returns>
    public async Task<Result<bool>> ValidateModelAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (modelId == Guid.Empty)
                return Result<bool>.WithFailure("Invalid model identifier");

            // Get model information
            var modelResult = await _modelRepository.GetByIdAsync(modelId, cancellationToken).ConfigureAwait(false);
            if (!modelResult.IsSuccess)
                return Result<bool>.WithFailure($"Model not found: {modelResult.Error}");

            var model = modelResult.Value;

            // Get provider
            var providerResult = await GetProviderForModelAsync(model, cancellationToken).ConfigureAwait(false);
            if (!providerResult.IsSuccess)
                return Result<bool>.WithFailure($"Provider not available: {providerResult.Error}");

            var provider = providerResult.Value;

            // Validate provider and model
            var validationResult = await provider.ValidateAsync(model.Name, cancellationToken).ConfigureAwait(false);
            
            if (!validationResult.IsSuccess)
                return Result<bool>.WithFailure($"Validation failed: {validationResult.Error}");

            return Result<bool>.WithSuccess(validationResult.Value.IsValid);
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"Error validating model: {ex.Message}");
        }
    }

    #region Private Methods

    /// <summary>
    /// Initializes the provider cache mapping models to providers
    /// </summary>
    private void InitializeProviderCache()
    {
        foreach (var provider in _providers)
        {
            foreach (var modelName in provider.SupportedModels)
            {
                _providerCache[modelName] = provider;
            }
        }
    }

    /// <summary>
    /// Gets the appropriate provider for a language model
    /// </summary>
    private async Task<Result<ILLMProvider>> GetProviderForModelAsync(
        LanguageModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            if (_providerCache.TryGetValue(model.Name, out var cachedProvider))
                return Result<ILLMProvider>.WithSuccess(cachedProvider);

            // Try to find a provider that supports this model
            foreach (var provider in _providers)
            {
                if (provider.SupportedModels.Contains(model.Name))
                {
                    _providerCache[model.Name] = provider;
                    return Result<ILLMProvider>.WithSuccess(provider);
                }
            }

            return Result<ILLMProvider>.WithFailure($"No provider found for model: {model.Name}");
        }
        catch (Exception ex)
        {
            return Result<ILLMProvider>.WithFailure($"Error getting provider: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks rate limits for a provider and model
    /// </summary>
    private async Task<Result<bool>> CheckRateLimitsAsync(
        ILLMProvider provider,
        string modelName,
        CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = $"{provider.ProviderName}:{modelName}";
            var now = DateTime.UtcNow;

            // Check if we need to refresh rate limit info
            if (!_lastRateLimitCheck.TryGetValue(cacheKey, out var lastCheck) ||
                now - lastCheck > TimeSpan.FromMinutes(1))
            {
                var rateLimitResult = await provider.GetRateLimitInfoAsync(modelName, cancellationToken).ConfigureAwait(false);
                
                if (!rateLimitResult.IsSuccess)
                    return Result<bool>.WithFailure($"Could not check rate limits: {rateLimitResult.Error}");

                _lastRateLimitCheck[cacheKey] = now;

                if (rateLimitResult.Value.IsLimitExceeded)
                    return Result<bool>.WithFailure("Rate limit exceeded");
            }

            return Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"Error checking rate limits: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates cost constraints for a request
    /// </summary>
    private async Task<Result<bool>> ValidateCostConstraintsAsync(
        ILLMProvider provider,
        string modelName,
        LLMParameters? parameters,
        CancellationToken cancellationToken)
    {
        try
        {
            if (_configuration.MaxDailyCost <= 0)
                return Result<bool>.WithSuccess(true);

            var today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            var dailySpent = _dailyCostTracker.GetValueOrDefault(today, 0);

            if (dailySpent >= _configuration.MaxDailyCost)
                return Result<bool>.WithFailure("Daily cost limit exceeded");

            // Estimate cost for this request
            var maxTokens = parameters?.MaxTokens ?? _configuration.DefaultMaxTokens;
            var estimatedCost = await provider.EstimateCostAsync(modelName, 1000, maxTokens, cancellationToken).ConfigureAwait(false);
            
            if (estimatedCost.IsSuccess && dailySpent + estimatedCost.Value > _configuration.MaxDailyCost)
                return Result<bool>.WithFailure("Request would exceed daily cost limit");

            return Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"Error validating cost constraints: {ex.Message}");
        }
    }

    /// <summary>
    /// Tracks cost for analytics and budget management
    /// </summary>
    private async Task TrackCostAsync(string modelName, decimal cost)
    {
        try
        {
            var today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            
            if (!_dailyCostTracker.ContainsKey(today))
                _dailyCostTracker[today] = 0;

            _dailyCostTracker[today] += cost;

            // Clean up old entries (keep last 30 days)
            var cutoffDate = DateTime.UtcNow.AddDays(-30).Date.ToString("yyyy-MM-dd");
            var keysToRemove = _dailyCostTracker.Keys.Where(k => string.Compare(k, cutoffDate) < 0).ToList();
            
            foreach (var key in keysToRemove)
            {
                _dailyCostTracker.Remove(key);
            }
        }
        catch
        {
            // Swallow tracking errors - don't fail the main operation
        }
    }

    #endregion
}

/// <summary>
/// Configuration for the Enhanced LLM Service
/// </summary>
public class LLMServiceConfiguration
{
    /// <summary>
    /// Gets or sets the maximum daily cost in USD (0 = no limit)
    /// </summary>
    public decimal MaxDailyCost { get; set; } = 100m;

    /// <summary>
    /// Gets or sets the default maximum tokens for responses
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the default temperature for responses
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets whether to enable cost tracking
    /// </summary>
    public bool EnableCostTracking { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable rate limit checking
    /// </summary>
    public bool EnableRateLimitChecking { get; set; } = true;

    /// <summary>
    /// Gets or sets the default request timeout in seconds
    /// </summary>
    public int DefaultTimeoutSeconds { get; set; } = 120;
}