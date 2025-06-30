using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for managing user personas and LLM personality configurations
/// Enables personalized AI interactions based on user preferences and context
/// </summary>
public interface IPersonaManager
{
    /// <summary>
    /// Gets available user personas for selection
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Available user personas</returns>
    Task<IEnumerable<UserPersona>> GetUserPersonasAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user persona based on preferences and behavior
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="personaName">Name for the new persona</param>
    /// <param name="preferences">User preferences and settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user persona</returns>
    Task<UserPersona> CreateUserPersonaAsync(
        string userId, 
        string personaName, 
        UserPreferenceProfile preferences, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates existing user persona with new preferences
    /// </summary>
    /// <param name="personaId">Persona identifier</param>
    /// <param name="updatedPreferences">Updated preference profile</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user persona</returns>
    Task<UserPersona> UpdateUserPersonaAsync(
        string personaId, 
        UserPreferenceProfile updatedPreferences, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available LLM personas for different interaction styles
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Available LLM personas</returns>
    Task<IEnumerable<LLMPersona>> GetLLMPersonasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects optimal LLM persona based on user context and task type
    /// </summary>
    /// <param name="userPersona">User persona preferences</param>
    /// <param name="taskContext">Current task context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recommended LLM persona</returns>
    Task<LLMPersona> SelectOptimalLLMPersonaAsync(
        UserPersona userPersona, 
        AgentContext taskContext, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies persona configuration to LLM provider
    /// </summary>
    /// <param name="llmPersona">LLM persona to apply</param>
    /// <param name="providerName">LLM provider name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration result</returns>
    Task<bool> ApplyLLMPersonaAsync(
        LLMPersona llmPersona, 
        string providerName, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Learns and adapts persona based on user feedback and interactions
    /// </summary>
    /// <param name="personaId">Persona identifier</param>
    /// <param name="interactions">Recent user interactions</param>
    /// <param name="feedback">User feedback on persona performance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Persona adaptation result</returns>
    Task<PersonaAdaptationResult> AdaptPersonaAsync(
        string personaId, 
        IEnumerable<UserInteraction> interactions, 
        ReportFeedback feedback, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes user persona and associated data
    /// </summary>
    /// <param name="personaId">Persona identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion result</returns>
    Task<bool> DeletePersonaAsync(string personaId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a user persona with preferences and behavioral patterns
/// </summary>
public record UserPersona
{
    /// <summary>
    /// Gets the unique identifier for this persona
    /// </summary>
    public string PersonaId { get; init; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets the user identifier this persona belongs to
    /// </summary>
    public string UserId { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the display name for this persona
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the user preferences associated with this persona
    /// </summary>
    public UserPreferenceProfile Preferences { get; init; } = new();
    
    /// <summary>
    /// Gets the creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets the last usage timestamp
    /// </summary>
    public DateTime LastUsed { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets whether this persona is currently active
    /// </summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Represents an LLM persona configuration for different interaction styles
/// </summary>
public record LLMPersona
{
    /// <summary>
    /// Gets the unique identifier for this LLM persona
    /// </summary>
    public string PersonaId { get; init; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets the display name for this LLM persona
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the description of this persona's behavior
    /// </summary>
    public string Description { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the system prompt for this persona
    /// </summary>
    public string SystemPrompt { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the temperature setting for response creativity
    /// </summary>
    public float Temperature { get; init; } = 0.7f;
    
    /// <summary>
    /// Gets the maximum tokens for responses
    /// </summary>
    public int MaxTokens { get; init; } = 4096;
    
    /// <summary>
    /// Gets the interaction style categories this persona is suited for
    /// </summary>
    public List<string> SuitedFor { get; init; } = new();
    
    /// <summary>
    /// Gets whether this persona is available for use
    /// </summary>
    public bool IsAvailable { get; init; } = true;
}

/// <summary>
/// Represents the result of persona adaptation
/// </summary>
public record PersonaAdaptationResult
{
    /// <summary>
    /// Gets whether adaptation was successful
    /// </summary>
    public bool IsSuccessful { get; init; }
    
    /// <summary>
    /// Gets the persona identifier that was adapted
    /// </summary>
    public string PersonaId { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the adaptation changes made
    /// </summary>
    public Dictionary<string, object> Changes { get; init; } = new();
    
    /// <summary>
    /// Gets the confidence level of the adaptation
    /// </summary>
    public float ConfidenceLevel { get; init; } = 0.0f;
    
    /// <summary>
    /// Gets any error message if adaptation failed
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;
} 