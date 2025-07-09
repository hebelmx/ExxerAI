using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service interface for managing personas and their associated prompt templates
/// in the ExxerAI system
/// </summary>
public interface IPersonaService
{
    /// <summary>
    /// Creates a new persona with the specified characteristics
    /// </summary>
    /// <param name="name">The persona name</param>
    /// <param name="role">The persona role or job function</param>
    /// <param name="description">Detailed description of the persona's purpose</param>
    /// <param name="systemPrompt">The base system prompt that defines persona behavior</param>
    /// <param name="traits">Optional behavioral traits dictionary</param>
    /// <param name="knowledgeDomains">Optional knowledge domains list</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the created persona</returns>
    Task<Result<Persona>> CreatePersonaAsync(
        string name,
        string role,
        string description,
        string systemPrompt,
        Dictionary<string, string>? traits = null,
        List<string>? knowledgeDomains = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing persona's properties
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="name">Optional new name</param>
    /// <param name="role">Optional new role</param>
    /// <param name="description">Optional new description</param>
    /// <param name="systemPrompt">Optional new system prompt</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the updated persona</returns>
    Task<Result<Persona>> UpdatePersonaAsync(
        Guid personaId,
        string? name = null,
        string? role = null,
        string? description = null,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a persona by its identifier
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the persona if found</returns>
    Task<Result<Persona>> GetPersonaByIdAsync(
        Guid personaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active personas
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing all active personas</returns>
    Task<Result<IEnumerable<Persona>>> GetActivePersonasAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for personas matching specific criteria
    /// </summary>
    /// <param name="searchCriteria">The search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing matching personas</returns>
    Task<Result<IEnumerable<Persona>>> SearchPersonasAsync(
        PersonaSearchCriteria searchCriteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a trait to a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="traitKey">The trait key</param>
    /// <param name="traitValue">The trait value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> AddPersonaTraitAsync(
        Guid personaId,
        string traitKey,
        string traitValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a trait from a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="traitKey">The trait key to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> RemovePersonaTraitAsync(
        Guid personaId,
        string traitKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a knowledge domain to a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="knowledgeDomain">The knowledge domain to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> AddKnowledgeDomainAsync(
        Guid personaId,
        string knowledgeDomain,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a knowledge domain from a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="knowledgeDomain">The knowledge domain to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> RemoveKnowledgeDomainAsync(
        Guid personaId,
        string knowledgeDomain,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Associates a prompt template with a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="template">The prompt template to associate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> AssociateTemplateAsync(
        Guid personaId,
        PromptTemplate template,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates and associates a new prompt template with a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="templateName">The template name</param>
    /// <param name="templateText">The template text with placeholders</param>
    /// <param name="contextTag">The context tag for the template</param>
    /// <param name="description">Optional template description</param>
    /// <param name="expectedParameters">Optional expected parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the created template</returns>
    Task<Result<PromptTemplate>> CreateTemplateForPersonaAsync(
        Guid personaId,
        string templateName,
        string templateText,
        string contextTag,
        string? description = null,
        Dictionary<string, string>? expectedParameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all prompt templates associated with a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="includeInactive">Whether to include inactive templates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the associated templates</returns>
    Task<Result<IEnumerable<PromptTemplate>>> GetPersonaTemplatesAsync(
        Guid personaId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the best persona for a given context and requirements
    /// </summary>
    /// <param name="contextTag">The context tag to match</param>
    /// <param name="preferredRole">Optional preferred role</param>
    /// <param name="requiredKnowledgeDomains">Optional required knowledge domains</param>
    /// <param name="requiredTraits">Optional required traits</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the best matching persona</returns>
    Task<Result<Persona>> FindBestPersonaForContextAsync(
        string contextTag,
        string? preferredRole = null,
        IEnumerable<string>? requiredKnowledgeDomains = null,
        Dictionary<string, string>? requiredTraits = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a persona making it available for use
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> ActivatePersonaAsync(
        Guid personaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a persona making it unavailable for use
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> DeactivatePersonaAsync(
        Guid personaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a persona and all its associated templates
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    Task<Result<bool>> DeletePersonaAsync(
        Guid personaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets usage statistics for all personas
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing persona usage statistics</returns>
    Task<Result<IEnumerable<PersonaUsageStatistics>>> GetUsageStatisticsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a persona configuration for completeness and consistency
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing validation results</returns>
    Task<Result<PersonaValidationResult>> ValidatePersonaAsync(
        Guid personaId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents search criteria for finding personas
/// </summary>
public class PersonaSearchCriteria
{
    /// <summary>
    /// Gets or sets the name pattern to search for
    /// </summary>
    public string? NamePattern { get; set; }

    /// <summary>
    /// Gets or sets the role to filter by
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets required knowledge domains
    /// </summary>
    public List<string> RequiredKnowledgeDomains { get; set; } = new();

    /// <summary>
    /// Gets or sets required traits
    /// </summary>
    public Dictionary<string, string> RequiredTraits { get; set; } = new();

    /// <summary>
    /// Gets or sets whether all knowledge domains must match (true) or at least one (false)
    /// </summary>
    public bool RequireAllKnowledgeDomains { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include inactive personas
    /// </summary>
    public bool IncludeInactive { get; set; } = false;

    /// <summary>
    /// Gets or sets the date range for created personas
    /// </summary>
    public DateRange? CreatedDateRange { get; set; }

    /// <summary>
    /// Gets or sets the date range for updated personas
    /// </summary>
    public DateRange? UpdatedDateRange { get; set; }
}

/// <summary>
/// Represents validation results for a persona
/// </summary>
public class PersonaValidationResult
{
    /// <summary>
    /// Gets or sets whether the persona is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets or sets suggestions for improvement
    /// </summary>
    public List<string> Suggestions { get; set; } = new();
}