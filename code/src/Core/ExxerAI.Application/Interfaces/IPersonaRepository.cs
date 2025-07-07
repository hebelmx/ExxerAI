using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for persona entities providing specialized query operations
/// for persona management and prompt template association
/// </summary>
public interface IPersonaRepository : IRepository<Persona>
{
    /// <summary>
    /// Gets active personas that are available for use
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active personas</returns>
    Task<Result<IEnumerable<Persona>>> GetActivePersonasAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets personas by role classification
    /// </summary>
    /// <param name="role">The persona role to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of personas with the specified role</returns>
    Task<Result<IEnumerable<Persona>>> GetByRoleAsync(
        string role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds personas that have expertise in specific knowledge domains
    /// </summary>
    /// <param name="knowledgeDomains">The knowledge domains to search for</param>
    /// <param name="requireAll">If true, persona must have all domains; if false, persona needs at least one</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of personas matching the knowledge domain criteria</returns>
    Task<Result<IEnumerable<Persona>>> FindByKnowledgeDomainsAsync(
        IEnumerable<string> knowledgeDomains,
        bool requireAll = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches personas by traits using key-value matching
    /// </summary>
    /// <param name="traits">Dictionary of trait keys and values to match</param>
    /// <param name="exactMatch">If true, requires exact trait value match; if false, performs partial matching</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of personas with matching traits</returns>
    Task<Result<IEnumerable<Persona>>> SearchByTraitsAsync(
        Dictionary<string, string> traits,
        bool exactMatch = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets personas along with their associated prompt templates
    /// </summary>
    /// <param name="includeInactivePersonas">Whether to include inactive personas in results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of personas with their loaded template collections</returns>
    Task<Result<IEnumerable<Persona>>> GetPersonasWithTemplatesAsync(
        bool includeInactivePersonas = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds personas suitable for a specific context based on role, knowledge domains, and traits
    /// </summary>
    /// <param name="contextTag">The context tag to match against templates</param>
    /// <param name="preferredRole">Optional preferred role for the persona</param>
    /// <param name="requiredDomains">Optional required knowledge domains</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of personas ranked by suitability for the context</returns>
    Task<Result<IEnumerable<Persona>>> FindSuitableForContextAsync(
        string contextTag,
        string? preferredRole = null,
        IEnumerable<string>? requiredDomains = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets personas created or updated within a specific date range
    /// </summary>
    /// <param name="fromDate">Start date for the range</param>
    /// <param name="toDate">End date for the range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of personas within the date range</returns>
    Task<Result<IEnumerable<Persona>>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches personas by name with partial matching support
    /// </summary>
    /// <param name="namePattern">The name pattern to search for (supports partial matching)</param>
    /// <param name="caseSensitive">Whether the search should be case sensitive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of personas with names matching the pattern</returns>
    Task<Result<IEnumerable<Persona>>> SearchByNameAsync(
        string namePattern,
        bool caseSensitive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets usage statistics for personas including template count and last used date
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of persona usage statistics</returns>
    Task<Result<IEnumerable<PersonaUsageStatistics>>> GetUsageStatisticsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents usage statistics for a persona
/// </summary>
public class PersonaUsageStatistics
{
    /// <summary>
    /// Gets or sets the persona identifier
    /// </summary>
    public Guid PersonaId { get; set; }

    /// <summary>
    /// Gets or sets the persona name
    /// </summary>
    public string PersonaName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of associated prompt templates
    /// </summary>
    public int TemplateCount { get; set; }

    /// <summary>
    /// Gets or sets the date when the persona was last used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Gets or sets whether the persona is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the total number of knowledge domains
    /// </summary>
    public int KnowledgeDomainCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of traits defined
    /// </summary>
    public int TraitCount { get; set; }
}