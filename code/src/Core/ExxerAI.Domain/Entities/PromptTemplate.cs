namespace ExxerAI.Domain.Entities;

/// <summary>
/// Represents a reusable prompt template with versioning, token substitution, and contextual interpolation
/// capabilities. Templates enable consistent, maintainable prompt management across the ExxerAI system.
/// </summary>
public class PromptTemplate
{
    /// <summary>
    /// Gets or sets the unique identifier for the prompt template
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the template name for identification and organization
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template text with placeholders for token substitution
    /// Supports {{variable}} syntax for dynamic content replacement
    /// </summary>
    [Required]
    [StringLength(5000)]
    public string TemplateText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the context tag that identifies when this template should be used
    /// Examples: "greeting", "error_handling", "data_analysis", "code_review"
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ContextTag { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template version number for change management and rollback
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets a description of what this template is used for
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the template was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the template was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether this template is currently active and available for use
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the expected parameters for this template with their descriptions
    /// Key: parameter name, Value: parameter description/type information
    /// </summary>
    public Dictionary<string, string> ExpectedParameters { get; set; } = new();

    /// <summary>
    /// Gets or sets default values for template parameters
    /// Key: parameter name, Value: default value
    /// </summary>
    public Dictionary<string, string> DefaultParameters { get; set; } = new();

    /// <summary>
    /// Gets or sets metadata for the template including author, tags, and configuration
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the foreign key to the associated persona
    /// </summary>
    public Guid? PersonaId { get; set; }

    /// <summary>
    /// Gets or sets the associated persona (navigation property)
    /// </summary>
    public Persona? Persona { get; set; }

    /// <summary>
    /// Renders the template by replacing tokens with provided context values
    /// </summary>
    /// <param name="context">Dictionary of parameter names and their values for substitution</param>
    /// <returns>The rendered template with all tokens replaced</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when required parameters are missing</exception>
    public string RenderTemplate(Dictionary<string, object> context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // Validate required parameters
        var missingParams = ExpectedParameters.Keys
            .Where(param => !context.ContainsKey(param) && !DefaultParameters.ContainsKey(param))
            .ToList();

        if (missingParams.Any())
        {
            throw new InvalidOperationException(
                $"Missing required parameters: {string.Join(", ", missingParams)}");
        }

        var result = TemplateText;

        // Replace tokens with context values or defaults
        foreach (var param in ExpectedParameters.Keys)
        {
            var value = GetParameterValue(param, context);
            var token = $"{{{{{param}}}}}";
            result = result.Replace(token, value, StringComparison.OrdinalIgnoreCase);
        }

        // Replace any additional context values not in expected parameters
        foreach (var kvp in context.Where(c => !ExpectedParameters.ContainsKey(c.Key)))
        {
            var token = $"{{{{{kvp.Key}}}}}";
            var value = kvp.Value?.ToString() ?? string.Empty;
            result = result.Replace(token, value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    /// <summary>
    /// Creates a new version of this template with updated content
    /// </summary>
    /// <param name="updatedText">The new template text</param>
    /// <param name="description">Optional description of changes made</param>
    /// <returns>A new PromptTemplate instance with incremented version</returns>
    public PromptTemplate CreateNewVersion(string updatedText, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(updatedText))
            throw new ArgumentException("Updated template text cannot be null or empty", nameof(updatedText));

        return new PromptTemplate
        {
            Name = Name,
            TemplateText = updatedText,
            ContextTag = ContextTag,
            Version = Version + 1,
            Description = description ?? Description,
            ExpectedParameters = new Dictionary<string, string>(ExpectedParameters),
            DefaultParameters = new Dictionary<string, string>(DefaultParameters),
            Metadata = new Dictionary<string, object>(Metadata),
            PersonaId = PersonaId
        };
    }

    /// <summary>
    /// Validates the template syntax and parameter consistency
    /// </summary>
    /// <returns>A validation result with any errors or warnings found</returns>
    public TemplateValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Check for required fields
        if (string.IsNullOrWhiteSpace(TemplateText))
            errors.Add("Template text is required");

        if (string.IsNullOrWhiteSpace(ContextTag))
            errors.Add("Context tag is required");

        // Find all tokens in template
        var tokenPattern = @"\{\{(\w+)\}\}";
        var matches = Regex.Matches(TemplateText, tokenPattern);
        var tokensInTemplate = matches.Cast<Match>()
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToList();

        // Check for undefined parameters
        var undefinedParams = tokensInTemplate
            .Where(token => !ExpectedParameters.ContainsKey(token))
            .ToList();

        if (undefinedParams.Any())
        {
            warnings.Add($"Template contains undefined parameters: {string.Join(", ", undefinedParams)}");
        }

        // Check for unused expected parameters
        var unusedParams = ExpectedParameters.Keys
            .Where(param => !tokensInTemplate.Contains(param))
            .ToList();

        if (unusedParams.Any())
        {
            warnings.Add($"Expected parameters not used in template: {string.Join(", ", unusedParams)}");
        }

        return new TemplateValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors,
            Warnings = warnings,
            TokensFound = tokensInTemplate
        };
    }

    /// <summary>
    /// Adds an expected parameter with its description
    /// </summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="description">Description of the parameter's purpose and format</param>
    /// <param name="defaultValue">Optional default value for the parameter</param>
    public void AddExpectedParameter(string parameterName, string description, string? defaultValue = null)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
            throw new ArgumentException("Parameter name cannot be null or empty", nameof(parameterName));

        ExpectedParameters[parameterName] = description ?? string.Empty;
        
        if (defaultValue != null)
        {
            DefaultParameters[parameterName] = defaultValue;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes an expected parameter and its default value
    /// </summary>
    /// <param name="parameterName">The parameter name to remove</param>
    /// <returns>True if the parameter was removed, false if it wasn't found</returns>
    public bool RemoveExpectedParameter(string parameterName)
    {
        var removed = ExpectedParameters.Remove(parameterName);
        DefaultParameters.Remove(parameterName); // Remove default too if exists
        
        if (removed)
        {
            UpdatedAt = DateTime.UtcNow;
        }
        
        return removed;
    }

    /// <summary>
    /// Deactivates the template, making it unavailable for use
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the template, making it available for use
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the parameter value from context or defaults
    /// </summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="context">The context dictionary</param>
    /// <returns>The parameter value as string</returns>
    private string GetParameterValue(string parameterName, Dictionary<string, object> context)
    {
        if (context.TryGetValue(parameterName, out var contextValue))
        {
            return contextValue?.ToString() ?? string.Empty;
        }

        if (DefaultParameters.TryGetValue(parameterName, out var defaultValue))
        {
            return defaultValue;
        }

        return string.Empty;
    }
}

/// <summary>
/// Represents the result of template validation
/// </summary>
public class TemplateValidationResult
{
    /// <summary>
    /// Gets or sets whether the template is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets validation errors that prevent template usage
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets validation warnings that should be addressed
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets or sets the tokens found in the template
    /// </summary>
    public List<string> TokensFound { get; set; } = new();
}