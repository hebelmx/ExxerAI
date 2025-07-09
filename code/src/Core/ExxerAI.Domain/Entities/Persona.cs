namespace ExxerAI.Domain.Entities;

/// <summary>
/// Represents a persona profile that defines behavioral traits, knowledge domains, and prompt templates
/// for LLM interactions within the ExxerAI system. Personas enable context-aware, role-based AI responses
/// tailored to specific use cases and user expectations.
/// </summary>
public class Persona
{
    /// <summary>
    /// Gets or sets the unique identifier for the persona
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the persona's name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the persona's role or job function
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a detailed description of the persona's purpose and behavior
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the persona's behavioral traits as key-value pairs
    /// Examples: "tone": "professional", "expertise_level": "expert", "communication_style": "concise"
    /// </summary>
    public Dictionary<string, string> Traits { get; set; } = new();

    /// <summary>
    /// Gets or sets the knowledge domains this persona specializes in
    /// Examples: "software_development", "financial_analysis", "medical_research"
    /// </summary>
    public List<string> KnowledgeDomains { get; set; } = new();

    /// <summary>
    /// Gets or sets the system prompt that defines the persona's base behavior
    /// </summary>
    [StringLength(2000)]
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the persona was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the persona was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether this persona is currently active and available for use
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets the collection of prompt templates associated with this persona
    /// </summary>
    public ICollection<PromptTemplate> Templates { get; init; } = new List<PromptTemplate>();

    /// <summary>
    /// Gets or sets metadata for the persona including version, author, and configuration
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Adds a behavioral trait to the persona
    /// </summary>
    /// <param name="key">The trait name (e.g., "tone", "expertise_level")</param>
    /// <param name="value">The trait value (e.g., "professional", "expert")</param>
    public void AddTrait(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Trait key cannot be null or empty", nameof(key));
        
        Traits[key] = value ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Retrieves a behavioral trait value by key
    /// </summary>
    /// <param name="key">The trait key to lookup</param>
    /// <returns>The trait value if found, empty string otherwise</returns>
    public string GetTrait(string key)
    {
        return Traits.GetValueOrDefault(key, string.Empty);
    }

    /// <summary>
    /// Adds a knowledge domain to the persona's expertise areas
    /// </summary>
    /// <param name="domain">The knowledge domain to add</param>
    public void AddKnowledgeDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Knowledge domain cannot be null or empty", nameof(domain));
        
        if (!KnowledgeDomains.Contains(domain, StringComparer.OrdinalIgnoreCase))
        {
            KnowledgeDomains.Add(domain);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Removes a knowledge domain from the persona's expertise areas
    /// </summary>
    /// <param name="domain">The knowledge domain to remove</param>
    /// <returns>True if the domain was removed, false if it wasn't found</returns>
    public bool RemoveKnowledgeDomain(string domain)
    {
        var removed = KnowledgeDomains.Remove(domain);
        if (removed)
        {
            UpdatedAt = DateTime.UtcNow;
        }
        return removed;
    }

    /// <summary>
    /// Checks if the persona has expertise in a specific knowledge domain
    /// </summary>
    /// <param name="domain">The knowledge domain to check</param>
    /// <returns>True if the persona has expertise in the domain, false otherwise</returns>
    public bool HasKnowledgeDomain(string domain)
    {
        return KnowledgeDomains.Contains(domain, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Associates a prompt template with this persona
    /// </summary>
    /// <param name="template">The prompt template to add</param>
    public void AddTemplate(PromptTemplate template)
    {
        if (template == null)
            throw new ArgumentNullException(nameof(template));
        
        Templates.Add(template);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Retrieves a prompt template by context tag
    /// </summary>
    /// <param name="contextTag">The context tag to search for</param>
    /// <returns>The matching template if found, null otherwise</returns>
    public PromptTemplate? GetTemplate(string contextTag)
    {
        return Templates.FirstOrDefault(t => t.ContextTag.Equals(contextTag, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Updates the persona's metadata
    /// </summary>
    /// <param name="key">The metadata key</param>
    /// <param name="value">The metadata value</param>
    public void SetMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty", nameof(key));
        
        Metadata[key] = value;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Retrieves metadata value by key
    /// </summary>
    /// <typeparam name="T">The expected type of the metadata value</typeparam>
    /// <param name="key">The metadata key</param>
    /// <returns>The metadata value cast to the specified type, or default if not found</returns>
    public T? GetMetadata<T>(string key)
    {
        if (Metadata.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Deactivates the persona, making it unavailable for use
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the persona, making it available for use
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}