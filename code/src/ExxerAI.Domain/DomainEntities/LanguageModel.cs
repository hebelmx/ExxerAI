namespace ExxerAI.Domain.DomainEntities;

/// <summary>
/// Represents a Large Language Model in the ExxerAI system
/// </summary>
public class LanguageModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the model
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the model name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model provider (e.g., OpenAI, Anthropic, etc.)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model version
    /// </summary>
    [StringLength(50)]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model capabilities
    /// </summary>
    public ModelCapabilities Capabilities { get; set; } = new();

    /// <summary>
    /// Gets or sets the model configuration
    /// </summary>
    public ModelConfiguration Configuration { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the model is currently available
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Gets or sets the model's context window size
    /// </summary>
    public int ContextWindowSize { get; set; } = 4096;

    /// <summary>
    /// Gets or sets when the model was added to the system
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the model was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}