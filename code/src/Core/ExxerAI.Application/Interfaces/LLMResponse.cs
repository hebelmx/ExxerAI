namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents a response from a language model
/// </summary>
public class LLMResponse
{
    /// <summary>
    /// Gets or sets the generated content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of tokens in the input
    /// </summary>
    public int InputTokens { get; set; }

    /// <summary>
    /// Gets or sets the number of tokens in the output
    /// </summary>
    public int OutputTokens { get; set; }

    /// <summary>
    /// Gets or sets the total tokens used
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;

    /// <summary>
    /// Gets or sets the estimated cost of the request
    /// </summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>
    /// Gets or sets the response time in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the reason the generation finished
    /// </summary>
    public string FinishReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets response metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = [];
}