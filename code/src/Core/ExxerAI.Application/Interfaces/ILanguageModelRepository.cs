using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Helpers.Operations;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for language model entities
/// </summary>
public interface ILanguageModelRepository : IRepository<LanguageModel>
{
    /// <summary>
    /// Gets available language models
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of available language models</returns>
    Task<Result<IEnumerable<LanguageModel>>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets language models by provider
    /// </summary>
    /// <param name="provider">The provider name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of language models from the specified provider</returns>
    Task<Result<IEnumerable<LanguageModel>>> GetByProviderAsync(
        string provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the best model for a specific capability
    /// </summary>
    /// <param name="capability">The required capability</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The best model for the capability if found</returns>
    Task<Result<LanguageModel>> FindBestModelForCapabilityAsync(
        string capability,
        CancellationToken cancellationToken = default);
}