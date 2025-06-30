using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Domain;

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

/// <summary>
/// Represents a conversation session with an LLM
/// </summary>
public class Conversation
{
    /// <summary>
    /// Gets or sets the unique identifier for the conversation
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the conversation title
    /// </summary>
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agent participating in this conversation
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// Gets or sets the agent participating in this conversation
    /// </summary>
    public Agent? Agent { get; set; }

    /// <summary>
    /// Gets or sets the language model used for this conversation
    /// </summary>
    public Guid LanguageModelId { get; set; }

    /// <summary>
    /// Gets or sets the language model used for this conversation
    /// </summary>
    public LanguageModel? LanguageModel { get; set; }

    /// <summary>
    /// Gets or sets the conversation status
    /// </summary>
    public ConversationStatus Status { get; set; } = ConversationStatus.Active;

    /// <summary>
    /// Gets or sets when the conversation was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the conversation was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the collection of messages in this conversation
    /// </summary>
    public ICollection<ConversationMessage> Messages { get; init; } = new List<ConversationMessage>();

    /// <summary>
    /// Gets or sets conversation metadata
    /// </summary>
    public ConversationMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Represents the possible states of a conversation
/// </summary>
public enum ConversationStatus
{
    /// <summary>
    /// Conversation is active and accepting messages
    /// </summary>
    Active,
    
    /// <summary>
    /// Conversation is paused
    /// </summary>
    Paused,
    
    /// <summary>
    /// Conversation has completed
    /// </summary>
    Completed,
    
    /// <summary>
    /// Conversation was archived
    /// </summary>
    Archived
}

/// <summary>
/// Represents a message in a conversation
/// </summary>
public class ConversationMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for the message
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the conversation this message belongs to
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// Gets or sets the conversation this message belongs to
    /// </summary>
    public Conversation? Conversation { get; set; }

    /// <summary>
    /// Gets or sets the message role
    /// </summary>
    public MessageRole Role { get; set; }

    /// <summary>
    /// Gets or sets the message content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the message was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the token count for this message
    /// </summary>
    public int TokenCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets message metadata
    /// </summary>
    public MessageMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Represents the role of a message in a conversation
/// </summary>
public enum MessageRole
{
    /// <summary>
    /// Message from the system
    /// </summary>
    System,
    
    /// <summary>
    /// Message from the user
    /// </summary>
    User,
    
    /// <summary>
    /// Message from the assistant/agent
    /// </summary>
    Assistant,
    
    /// <summary>
    /// Message from a function call
    /// </summary>
    Function
}

/// <summary>
/// Represents metadata for a conversation
/// </summary>
public class ConversationMetadata
{
    /// <summary>
    /// Gets or sets the total token count for the conversation
    /// </summary>
    public int TotalTokens { get; set; } = 0;

    /// <summary>
    /// Gets or sets the estimated cost of the conversation
    /// </summary>
    public decimal EstimatedCost { get; set; } = 0;

    /// <summary>
    /// Gets or sets custom metadata properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
}

/// <summary>
/// Represents metadata for a message
/// </summary>
public class MessageMetadata
{
    /// <summary>
    /// Gets or sets the response time in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; set; } = 0;

    /// <summary>
    /// Gets or sets the model parameters used for generation
    /// </summary>
    public Dictionary<string, object> ModelParameters { get; init; } = new();

    /// <summary>
    /// Gets or sets custom metadata properties
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
} 