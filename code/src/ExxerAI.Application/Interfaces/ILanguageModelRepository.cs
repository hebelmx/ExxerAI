namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Repository interface for language model entities
/// </summary>
public interface ILanguageModelRepository : IRepository<Domain.LanguageModel>
{
    /// <summary>
    /// Gets available language models
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of available language models</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<Domain.LanguageModel>>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets language models by provider
    /// </summary>
    /// <param name="provider">The provider name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of language models from the specified provider</returns>
    Task<ExxerAI.Domain.Result<IEnumerable<Domain.LanguageModel>>> GetByProviderAsync(
        string provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the best model for a specific capability
    /// </summary>
    /// <param name="capability">The required capability</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The best model for the capability if found</returns>
    Task<ExxerAI.Domain.Result<Domain.LanguageModel>> FindBestModelForCapabilityAsync(
        string capability,
        CancellationToken cancellationToken = default);
}