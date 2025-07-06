namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents parameters for language model generation
/// </summary>
public class LLMParameters
{
    /// <summary>
    /// Gets or sets the temperature for generation (0.0-2.0)
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets the maximum number of tokens to generate
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the top-p value for nucleus sampling
    /// </summary>
    public double TopP { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the frequency penalty
    /// </summary>
    public double FrequencyPenalty { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the presence penalty
    /// </summary>
    public double PresencePenalty { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets stop sequences
    /// </summary>
    public ICollection<string> StopSequences { get; init; } = new List<string>();

    /// <summary>
    /// Gets or sets custom parameters
    /// </summary>
    public Dictionary<string, object> CustomParameters { get; init; } = new();
}