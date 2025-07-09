using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for prompt template entities providing specialized operations
/// for template management, versioning, and contextual retrieval
/// </summary>
public interface IPromptTemplateRepository : IRepository<PromptTemplate>
{
    /// <summary>
    /// Gets active prompt templates that are available for use
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active prompt templates</returns>
    Task<Result<IEnumerable<PromptTemplate>>> GetActiveTemplatesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets prompt templates by context tag
    /// </summary>
    /// <param name="contextTag">The context tag to filter by</param>
    /// <param name="includeInactive">Whether to include inactive templates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of templates with the specified context tag</returns>
    Task<Result<IEnumerable<PromptTemplate>>> GetByContextTagAsync(
        string contextTag,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all versions of a template by name, ordered by version descending
    /// </summary>
    /// <param name="templateName">The template name to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of template versions ordered by version number</returns>
    Task<Result<IEnumerable<PromptTemplate>>> GetVersionsByNameAsync(
        string templateName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest version of a template by name
    /// </summary>
    /// <param name="templateName">The template name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The latest version of the template if found</returns>
    Task<Result<PromptTemplate>> GetLatestVersionByNameAsync(
        string templateName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets prompt templates associated with a specific persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="includeInactive">Whether to include inactive templates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of templates associated with the persona</returns>
    Task<Result<IEnumerable<PromptTemplate>>> GetByPersonaIdAsync(
        Guid personaId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches templates by content using text search
    /// </summary>
    /// <param name="searchText">The text to search for in template content</param>
    /// <param name="caseSensitive">Whether the search should be case sensitive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of templates containing the search text</returns>
    Task<Result<IEnumerable<PromptTemplate>>> SearchByContentAsync(
        string searchText,
        bool caseSensitive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets templates that contain specific parameter names
    /// </summary>
    /// <param name="parameterNames">The parameter names to search for</param>
    /// <param name="requireAll">If true, template must contain all parameters; if false, at least one</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of templates containing the specified parameters</returns>
    Task<Result<IEnumerable<PromptTemplate>>> GetByParametersAsync(
        IEnumerable<string> parameterNames,
        bool requireAll = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets templates created or updated within a specific date range
    /// </summary>
    /// <param name="fromDate">Start date for the range</param>
    /// <param name="toDate">End date for the range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of templates within the date range</returns>
    Task<Result<IEnumerable<PromptTemplate>>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets templates with validation issues (missing parameters, syntax errors, etc.)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of templates that have validation issues</returns>
    Task<Result<IEnumerable<TemplateWithValidation>>> GetTemplatesWithValidationIssuesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the best matching template for a given context and parameters
    /// </summary>
    /// <param name="contextTag">The context tag to match</param>
    /// <param name="availableParameters">Parameters available for substitution</param>
    /// <param name="personaId">Optional persona filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The best matching template if found</returns>
    Task<Result<PromptTemplate>> FindBestMatchAsync(
        string contextTag,
        IEnumerable<string> availableParameters,
        Guid? personaId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives old versions of templates, keeping only the latest N versions
    /// </summary>
    /// <param name="templateName">The template name to clean up</param>
    /// <param name="versionsToKeep">Number of versions to keep (default: 5)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of templates archived</returns>
    Task<Result<int>> ArchiveOldVersionsAsync(
        string templateName,
        int versionsToKeep = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets template usage analytics including frequency and parameter usage
    /// </summary>
    /// <param name="fromDate">Start date for analytics period</param>
    /// <param name="toDate">End date for analytics period</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of template analytics data</returns>
    Task<Result<IEnumerable<TemplateAnalytics>>> GetUsageAnalyticsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a template with its validation results
/// </summary>
public class TemplateWithValidation
{
    /// <summary>
    /// Gets or sets the prompt template
    /// </summary>
    public PromptTemplate Template { get; set; } = null!;

    /// <summary>
    /// Gets or sets the validation result
    /// </summary>
    public TemplateValidationResult ValidationResult { get; set; } = null!;
}

/// <summary>
/// Represents analytics data for a prompt template
/// </summary>
public class TemplateAnalytics
{
    /// <summary>
    /// Gets or sets the template identifier
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Gets or sets the template name
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the context tag
    /// </summary>
    public string ContextTag { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of times the template was used
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Gets or sets the date when the template was last used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Gets or sets the average rendering time in milliseconds
    /// </summary>
    public double AverageRenderingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the most frequently used parameters
    /// </summary>
    public Dictionary<string, int> ParameterUsageFrequency { get; set; } = new();

    /// <summary>
    /// Gets or sets error rate as a percentage
    /// </summary>
    public double ErrorRate { get; set; }
}