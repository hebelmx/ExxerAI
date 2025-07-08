using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Application.Services;

/// <summary>
/// Service implementation for managing personas and their associated prompt templates
/// in the ExxerAI system
/// </summary>
public class PersonaService : IPersonaService
{
    private readonly IPersonaRepository _personaRepository;
    private readonly IPromptTemplateRepository _promptTemplateRepository;

    /// <summary>
    /// Initializes a new instance of the PersonaService class
    /// </summary>
    /// <param name="personaRepository">The persona repository</param>
    /// <param name="promptTemplateRepository">The prompt template repository</param>
    public PersonaService(
        IPersonaRepository personaRepository,
        IPromptTemplateRepository promptTemplateRepository)
    {
        _personaRepository = personaRepository ?? throw new ArgumentNullException(nameof(personaRepository));
        _promptTemplateRepository = promptTemplateRepository ?? throw new ArgumentNullException(nameof(promptTemplateRepository));
    }

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
    public async Task<Result<Persona>> CreatePersonaAsync(
        string name,
        string role,
        string description,
        string systemPrompt,
        Dictionary<string, string>? traits = null,
        List<string>? knowledgeDomains = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(name))
                return Result<Persona>.WithFailure("Persona name cannot be empty");

            if (string.IsNullOrWhiteSpace(role))
                return Result<Persona>.WithFailure("Persona role cannot be empty");

            if (string.IsNullOrWhiteSpace(description))
                return Result<Persona>.WithFailure("Persona description cannot be empty");

            if (string.IsNullOrWhiteSpace(systemPrompt))
                return Result<Persona>.WithFailure("System prompt cannot be empty");

            // Create the persona
            var persona = new Persona
            {
                Name = name.Trim(),
                Role = role.Trim(),
                Description = description.Trim(),
                SystemPrompt = systemPrompt.Trim(),
                Traits = traits ?? new Dictionary<string, string>(),
                KnowledgeDomains = knowledgeDomains ?? new List<string>()
            };

            // Save to repository
            var result = await _personaRepository.AddAsync(persona, cancellationToken).ConfigureAwait(false);

            if (!result.IsSuccess)
                return Result<Persona>.WithFailure($"Failed to create persona: {result.Error}");

            return Result<Persona>.WithSuccess(result.Value);
        }
        catch (Exception ex)
        {
            return Result<Persona>.WithFailure($"Error creating persona: {ex.Message}");
        }
    }

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
    public async Task<Result<Persona>> UpdatePersonaAsync(
        Guid personaId,
        string? name = null,
        string? role = null,
        string? description = null,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<Persona>.WithFailure("Invalid persona identifier");

            // Get existing persona
            var getResult = await _personaRepository.GetByIdAsync(personaId, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess)
                return Result<Persona>.WithFailure($"Persona not found: {getResult.Error}");

            var persona = getResult.Value;

            // Update provided fields
            if (!string.IsNullOrWhiteSpace(name))
                persona.Name = name.Trim();

            if (!string.IsNullOrWhiteSpace(role))
                persona.Role = role.Trim();

            if (!string.IsNullOrWhiteSpace(description))
                persona.Description = description.Trim();

            if (!string.IsNullOrWhiteSpace(systemPrompt))
                persona.SystemPrompt = systemPrompt.Trim();

            persona.UpdatedAt = DateTime.UtcNow;

            // Save updates
            var updateResult = await _personaRepository.UpdateAsync(persona, cancellationToken).ConfigureAwait(false);

            if (!updateResult.IsSuccess)
                return Result<Persona>.WithFailure($"Failed to update persona: {updateResult.Error}");

            return Result<Persona>.WithSuccess(updateResult.Value);
        }
        catch (Exception ex)
        {
            return Result<Persona>.WithFailure($"Error updating persona: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a persona by its identifier
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the persona if found</returns>
    public async Task<Result<Persona>> GetPersonaByIdAsync(
        Guid personaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<Persona>.WithFailure("Invalid persona identifier");

            return await _personaRepository.GetByIdAsync(personaId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result<Persona>.WithFailure($"Error retrieving persona: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all active personas
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing all active personas</returns>
    public async Task<Result<IEnumerable<Persona>>> GetActivePersonasAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _personaRepository.GetActivePersonasAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Persona>>.WithFailure($"Error retrieving active personas: {ex.Message}");
        }
    }

    /// <summary>
    /// Searches for personas matching specific criteria
    /// </summary>
    /// <param name="searchCriteria">The search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing matching personas</returns>
    public async Task<Result<IEnumerable<Persona>>> SearchPersonasAsync(
        PersonaSearchCriteria searchCriteria,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (searchCriteria == null)
                return Result<IEnumerable<Persona>>.WithFailure("Search criteria cannot be null");

            // Start with all personas or active only
            var personasResult = searchCriteria.IncludeInactive
                ? await _personaRepository.GetAllAsync(cancellationToken).ConfigureAwait(false)
                : await _personaRepository.GetActivePersonasAsync(cancellationToken).ConfigureAwait(false);

            if (!personasResult.IsSuccess)
                return personasResult;

            var personas = personasResult.Value.AsQueryable();

            // Apply name pattern filter
            if (!string.IsNullOrWhiteSpace(searchCriteria.NamePattern))
            {
                var nameResult = await _personaRepository.SearchByNameAsync(
                    searchCriteria.NamePattern,
                    false,
                    cancellationToken).ConfigureAwait(false);

                if (nameResult.IsSuccess)
                {
                    var nameMatchIds = nameResult.Value.Select(p => p.Id).ToHashSet();
                    personas = personas.Where(p => nameMatchIds.Contains(p.Id));
                }
            }

            // Apply role filter
            if (!string.IsNullOrWhiteSpace(searchCriteria.Role))
            {
                var roleResult = await _personaRepository.GetByRoleAsync(
                    searchCriteria.Role,
                    cancellationToken).ConfigureAwait(false);

                if (roleResult.IsSuccess)
                {
                    var roleMatchIds = roleResult.Value.Select(p => p.Id).ToHashSet();
                    personas = personas.Where(p => roleMatchIds.Contains(p.Id));
                }
            }

            // Apply knowledge domain filter
            if (searchCriteria.RequiredKnowledgeDomains.Any())
            {
                var domainResult = await _personaRepository.FindByKnowledgeDomainsAsync(
                    searchCriteria.RequiredKnowledgeDomains,
                    searchCriteria.RequireAllKnowledgeDomains,
                    cancellationToken).ConfigureAwait(false);

                if (domainResult.IsSuccess && domainResult.Value is not null)
                {
                    var domainMatchIds = domainResult.Value.Select(p => p.Id).ToHashSet();
                    personas = personas.Where(p => domainMatchIds.Contains(p.Id));
                }
            }

            // Apply trait filter
            if (searchCriteria != null && searchCriteria.RequiredTraits.Any())
            {
                var traitResult = await _personaRepository.SearchByTraitsAsync(
                    searchCriteria.RequiredTraits,
                    true,
                    cancellationToken).ConfigureAwait(false);

                if (traitResult.IsSuccess && traitResult.Value is not null)
                {
                    var traitMatchIds = traitResult.Value.Select(p => p.Id).ToHashSet();
                    personas = personas.Where(p => traitMatchIds.Contains(p.Id));
                }
            }

            return Result<IEnumerable<Persona>>.WithSuccess([.. personas]);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Persona>>.WithFailure($"Error searching personas: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds a trait to a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="traitKey">The trait key</param>
    /// <param name="traitValue">The trait value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> AddPersonaTraitAsync(
        Guid personaId,
        string traitKey,
        string traitValue,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<bool>.WithFailure("Invalid persona identifier");

            if (string.IsNullOrWhiteSpace(traitKey))
                return Result<bool>.WithFailure("Trait key cannot be empty");

            var getResult = await _personaRepository.GetByIdAsync(personaId, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess)
                return Result<bool>.WithFailure($"Persona not found: {getResult.Error}");

            var persona = getResult.Value;
            if (persona is null)
                return Result<bool>.WithFailure("Persona not found: Retrieved persona is null");

            persona.AddTrait(traitKey.Trim(), traitValue?.Trim() ?? string.Empty);

            var updateResult = await _personaRepository.UpdateAsync(persona, cancellationToken).ConfigureAwait(false);
            return updateResult.IsSuccess
                ? Result<bool>.WithSuccess(true)
                : Result<bool>.WithFailure($"Failed to add trait: {updateResult.Error}");
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"Error adding trait: {ex.Message}");
        }
    }

    /// <summary>
    /// Removes a trait from a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="traitKey">The trait key to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> RemovePersonaTraitAsync(
        Guid personaId,
        string traitKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<bool>.WithFailure("Invalid persona identifier");

            if (string.IsNullOrWhiteSpace(traitKey))
                return Result<bool>.WithFailure("Trait key cannot be empty");

            var getResult = await _personaRepository.GetByIdAsync(personaId, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess)
                return Result<bool>.WithFailure($"Persona not found: {getResult.Error}");

            var persona = getResult.Value;
            var removed = persona.Traits.Remove(traitKey.Trim());

            if (removed)
            {
                persona.UpdatedAt = DateTime.UtcNow;
                var updateResult = await _personaRepository.UpdateAsync(persona, cancellationToken).ConfigureAwait(false);
                return updateResult.IsSuccess
                    ? Result<bool>.WithSuccess(true)
                    : Result<bool>.WithFailure($"Failed to remove trait: {updateResult.Error}");
            }

            return Result<bool>.WithSuccess(false); // Trait was not found
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"Error removing trait: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds a knowledge domain to a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="knowledgeDomain">The knowledge domain to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> AddKnowledgeDomainAsync(
        Guid personaId,
        string knowledgeDomain,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<bool>.WithFailure("Invalid persona identifier");

            if (string.IsNullOrWhiteSpace(knowledgeDomain))
                return Result<bool>.WithFailure("Knowledge domain cannot be empty");

            var getResult = await _personaRepository.GetByIdAsync(personaId, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess)
                return Result<bool>.WithFailure($"Persona not found: {getResult.Error}");

            var persona = getResult.Value;
            persona.AddKnowledgeDomain(knowledgeDomain.Trim());

            var updateResult = await _personaRepository.UpdateAsync(persona, cancellationToken).ConfigureAwait(false);
            return updateResult.IsSuccess
                ? Result<bool>.WithSuccess(true)
                : Result<bool>.WithFailure($"Failed to add knowledge domain: {updateResult.Error}");
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"Error adding knowledge domain: {ex.Message}");
        }
    }

    /// <summary>
    /// Removes a knowledge domain from a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="knowledgeDomain">The knowledge domain to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> RemoveKnowledgeDomainAsync(
        Guid personaId,
        string knowledgeDomain,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<bool>.WithFailure("Invalid persona identifier");

            if (string.IsNullOrWhiteSpace(knowledgeDomain))
                return Result<bool>.WithFailure("Knowledge domain cannot be empty");

            var getResult = await _personaRepository.GetByIdAsync(personaId, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess)
                return Result<bool>.WithFailure($"Persona not found: {getResult.Error}");

            var persona = getResult.Value;
            var removed = persona.RemoveKnowledgeDomain(knowledgeDomain.Trim());

            if (removed)
            {
                var updateResult = await _personaRepository.UpdateAsync(persona, cancellationToken).ConfigureAwait(false);
                return updateResult.IsSuccess
                    ? Result<bool>.WithSuccess(true)
                    : Result<bool>.WithFailure($"Failed to remove knowledge domain: {updateResult.Error}");
            }

            return Result<bool>.WithSuccess(false); // Domain was not found
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"Error removing knowledge domain: {ex.Message}");
        }
    }

    /// <summary>
    /// Associates a prompt template with a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="template">The prompt template to associate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> AssociateTemplateAsync(
        Guid personaId,
        PromptTemplate template,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<bool>.WithFailure("Invalid persona identifier");

            if (template == null)
                return Result<bool>.WithFailure("Template cannot be null");

            var getResult = await _personaRepository.GetByIdAsync(personaId, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess)
                return Result<bool>.WithFailure($"Persona not found: {getResult.Error}");

            var persona = getResult.Value;
            template.PersonaId = personaId;
            template.Persona = persona;

            // Save the template
            var templateResult = await _promptTemplateRepository.AddAsync(template, cancellationToken).ConfigureAwait(false);
            if (!templateResult.IsSuccess)
                return Result<bool>.WithFailure($"Failed to save template: {templateResult.Error}");

            // Add to persona's template collection
            persona.AddTemplate(templateResult.Value);

            // Update persona
            var updateResult = await _personaRepository.UpdateAsync(persona, cancellationToken).ConfigureAwait(false);
            return updateResult.IsSuccess
                ? Result<bool>.WithSuccess(true)
                : Result<bool>.WithFailure($"Failed to associate template: {updateResult.Error}");
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"Error associating template: {ex.Message}");
        }
    }

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
    public async Task<Result<PromptTemplate>> CreateTemplateForPersonaAsync(
        Guid personaId,
        string templateName,
        string templateText,
        string contextTag,
        string? description = null,
        Dictionary<string, string>? expectedParameters = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<PromptTemplate>.WithFailure("Invalid persona identifier");

            if (string.IsNullOrWhiteSpace(templateName))
                return Result<PromptTemplate>.WithFailure("Template name cannot be empty");

            if (string.IsNullOrWhiteSpace(templateText))
                return Result<PromptTemplate>.WithFailure("Template text cannot be empty");

            if (string.IsNullOrWhiteSpace(contextTag))
                return Result<PromptTemplate>.WithFailure("Context tag cannot be empty");

            // Verify persona exists
            var personaResult = await _personaRepository.GetByIdAsync(personaId, cancellationToken).ConfigureAwait(false);
            if (!personaResult.IsSuccess)
                return Result<PromptTemplate>.WithFailure($"Persona not found: {personaResult.Error}");

            // Create template
            var template = new PromptTemplate
            {
                Name = templateName.Trim(),
                TemplateText = templateText.Trim(),
                ContextTag = contextTag.Trim(),
                Description = description?.Trim() ?? string.Empty,
                ExpectedParameters = expectedParameters ?? new Dictionary<string, string>(),
                PersonaId = personaId,
                Persona = personaResult.Value
            };

            // Validate template
            var validation = template.Validate();
            if (!validation.IsValid)
                return Result<PromptTemplate>.WithFailure($"Template validation failed: {string.Join(", ", validation.Errors)}");

            // Associate template with persona
            var associateResult = await AssociateTemplateAsync(personaId, template, cancellationToken).ConfigureAwait(false);
            if (!associateResult.IsSuccess)
                return Result<PromptTemplate>.WithFailure($"Failed to associate template: {associateResult.Error}");

            return Result<PromptTemplate>.WithSuccess(template);
        }
        catch (Exception ex)
        {
            return Result<PromptTemplate>.WithFailure($"Error creating template: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all prompt templates associated with a persona
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="includeInactive">Whether to include inactive templates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the associated templates</returns>
    public async Task<Result<IEnumerable<PromptTemplate>>> GetPersonaTemplatesAsync(
        Guid personaId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<IEnumerable<PromptTemplate>>.WithFailure("Invalid persona identifier");

            return await _promptTemplateRepository.GetByPersonaIdAsync(personaId, includeInactive, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PromptTemplate>>.WithFailure($"Error retrieving persona templates: {ex.Message}");
        }
    }

    /// <summary>
    /// Finds the best persona for a given context and requirements
    /// </summary>
    /// <param name="contextTag">The context tag to match</param>
    /// <param name="preferredRole">Optional preferred role</param>
    /// <param name="requiredKnowledgeDomains">Optional required knowledge domains</param>
    /// <param name="requiredTraits">Optional required traits</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing the best matching persona</returns>
    public async Task<Result<Persona>> FindBestPersonaForContextAsync(
        string contextTag,
        string? preferredRole = null,
        IEnumerable<string>? requiredKnowledgeDomains = null,
        Dictionary<string, string>? requiredTraits = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contextTag))
                return Result<Persona>.WithFailure("Context tag cannot be empty");

            var result = await _personaRepository.FindSuitableForContextAsync(
                contextTag,
                preferredRole,
                requiredKnowledgeDomains,
                cancellationToken).ConfigureAwait(false);

            if (result.IsSuccess && result.Value is not null)
            {
                var bestMatch = result.Value.FirstOrDefault();

                return bestMatch != null
                        ? Result<Persona>.WithSuccess(bestMatch)
                        : Result<Persona>.WithFailure("No suitable persona found for the given context");
            }
        }
        catch (Exception ex)
        {
            return Result<Persona>.WithFailure($"Error finding best persona: {ex.Message}");
        }
        // If we reach here, it means no suitable persona was found
        return Result<Persona>.WithFailure("No suitable persona found for the given context");
    }

    /// <summary>
    /// Activates a persona making it available for use
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> ActivatePersonaAsync(
        Guid personaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<bool>.WithFailure("Invalid persona identifier");

            var getResult = await _personaRepository.GetByIdAsync(personaId, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess)
                return Result<bool>.WithFailure($"Persona not found: {getResult.Error}");

            var persona = getResult.Value;
            persona.Activate();

            var updateResult = await _personaRepository.UpdateAsync(persona, cancellationToken).ConfigureAwait(false);
            return updateResult.IsSuccess
                ? Result<bool>.WithSuccess(true)
                : Result<bool>.WithFailure($"Failed to activate persona: {updateResult.Error}");
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"Error activating persona: {ex.Message}");
        }
    }

    /// <summary>
    /// Deactivates a persona making it unavailable for use
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> DeactivatePersonaAsync(
        Guid personaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<bool>.WithFailure("Invalid persona identifier");

            var getResult = await _personaRepository.GetByIdAsync(personaId, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess)
                return Result<bool>.WithFailure($"Persona not found: {getResult.Error}");

            var persona = getResult.Value;
            persona.Deactivate();

            var updateResult = await _personaRepository.UpdateAsync(persona, cancellationToken).ConfigureAwait(false);
            return updateResult.IsSuccess
                ? Result<bool>.WithSuccess(true)
                : Result<bool>.WithFailure($"Failed to deactivate persona: {updateResult.Error}");
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"Error deactivating persona: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a persona and all its associated templates
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the operation</returns>
    public async Task<Result<bool>> DeletePersonaAsync(
        Guid personaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<bool>.WithFailure("Invalid persona identifier");

            // First, delete associated templates
            var templatesResult = await _promptTemplateRepository.GetByPersonaIdAsync(personaId, true, cancellationToken).ConfigureAwait(false);
            if (templatesResult.IsSuccess)
            {
                foreach (var template in templatesResult.Value)
                {
                    await _promptTemplateRepository.DeleteAsync(template.Id, cancellationToken).ConfigureAwait(false);
                }
            }

            // Then delete the persona
            return await _personaRepository.DeleteAsync(personaId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result<bool>.WithFailure($"Error deleting persona: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets usage statistics for all personas
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing persona usage statistics</returns>
    public async Task<Result<IEnumerable<PersonaUsageStatistics>>> GetUsageStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _personaRepository.GetUsageStatisticsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<PersonaUsageStatistics>>.WithFailure($"Error retrieving usage statistics: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates a persona configuration for completeness and consistency
    /// </summary>
    /// <param name="personaId">The persona identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result containing validation results</returns>
    public async Task<Result<PersonaValidationResult>> ValidatePersonaAsync(
        Guid personaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (personaId == Guid.Empty)
                return Result<PersonaValidationResult>.WithFailure("Invalid persona identifier");

            var getResult = await _personaRepository.GetByIdAsync(personaId, cancellationToken).ConfigureAwait(false);
            if (!getResult.IsSuccess)
                return Result<PersonaValidationResult>.WithFailure($"Persona not found: {getResult.Error}");

            var persona = getResult.Value;
            var validation = new PersonaValidationResult();

            // Basic validation
            if (string.IsNullOrWhiteSpace(persona.Name))
                validation.Errors.Add("Persona name is required");

            if (string.IsNullOrWhiteSpace(persona.Role))
                validation.Errors.Add("Persona role is required");

            if (string.IsNullOrWhiteSpace(persona.SystemPrompt))
                validation.Errors.Add("System prompt is required");

            // Content validation
            if (persona.SystemPrompt.Length < 50)
                validation.Warnings.Add("System prompt may be too short for effective persona definition");

            if (!persona.Traits.Any())
                validation.Warnings.Add("No behavioral traits defined - consider adding traits for better persona characterization");

            if (!persona.KnowledgeDomains.Any())
                validation.Warnings.Add("No knowledge domains defined - consider adding domains for better context matching");

            // Template validation
            var templatesResult = await GetPersonaTemplatesAsync(personaId, true, cancellationToken).ConfigureAwait(false);
            if (templatesResult.IsSuccess)
            {
                var templates = templatesResult.Value.ToList();
                if (!templates.Any())
                {
                    validation.Warnings.Add("No prompt templates associated with this persona");
                }
                else
                {
                    foreach (var template in templates)
                    {
                        var templateValidation = template.Validate();
                        if (!templateValidation.IsValid)
                        {
                            validation.Errors.AddRange(templateValidation.Errors.Select(e => $"Template '{template.Name}': {e}"));
                        }
                    }
                }
            }

            // Suggestions
            if (persona.Traits.Count < 3)
                validation.Suggestions.Add("Consider adding more behavioral traits (tone, expertise_level, communication_style, etc.)");

            if (persona.KnowledgeDomains.Count < 2)
                validation.Suggestions.Add("Consider adding more knowledge domains to improve context matching");

            validation.IsValid = !validation.Errors.Any();

            return Result<PersonaValidationResult>.WithSuccess(validation);
        }
        catch (Exception ex)
        {
            return Result<PersonaValidationResult>.WithFailure($"Error validating persona: {ex.Message}");
        }
    }
}