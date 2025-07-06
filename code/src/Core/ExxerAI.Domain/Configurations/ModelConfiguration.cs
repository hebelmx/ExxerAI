namespace ExxerAI.Domain.Configurations;

/// <summary>
/// Represents configuration settings for a language model
/// </summary>
public class ModelConfiguration
{
    /// <summary>
    /// Gets or sets the model endpoint URL
    /// </summary>
    public string EndpointUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key for the model
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default temperature for generation
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets the maximum tokens per request
    /// </summary>
    public int MaxTokensPerRequest { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the timeout for requests in seconds
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the rate limit per minute
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 60;

    /// <summary>
    /// Gets or sets custom configuration properties
    /// </summary>
    public Dictionary<string, object> CustomProperties { get; init; } = new();
}