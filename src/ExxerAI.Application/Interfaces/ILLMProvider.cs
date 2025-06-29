using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for Large Language Model providers
/// </summary>
public interface ILLMProvider
{
    /// <summary>
    /// Gets the name of the LLM provider
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Generates a response based on the input prompt
    /// </summary>
    /// <param name="prompt">The input prompt</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The LLM response</returns>
    Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a structured response for agent execution
    /// </summary>
    /// <param name="context">The agent context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The agent result</returns>
    Task<AgentResult> GenerateAgentResponseAsync(AgentContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the provider is available and healthy
    /// </summary>
    /// <returns>True if the provider is healthy</returns>
    Task<bool> IsHealthyAsync();
} 