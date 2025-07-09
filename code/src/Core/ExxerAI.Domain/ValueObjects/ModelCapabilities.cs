namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// Represents the capabilities of a language model
/// </summary>
public class ModelCapabilities
{
    /// <summary>
    /// Gets or sets whether the model supports text generation
    /// </summary>
    public bool SupportsTextGeneration { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the model supports code generation
    /// </summary>
    public bool SupportsCodeGeneration { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the model supports image analysis
    /// </summary>
    public bool SupportsImageAnalysis { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the model supports function calling
    /// </summary>
    public bool SupportsFunctionCalling { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the model supports streaming responses
    /// </summary>
    public bool SupportsStreaming { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum output tokens
    /// </summary>
    public int MaxOutputTokens { get; set; } = 2048;

    /// <summary>
    /// Gets or sets the supported response formats
    /// </summary>
    public ICollection<string> SupportedFormats { get; init; } = new List<string> { "text" };
}