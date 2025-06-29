using ExxerAI.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Represents a general-purpose agent capable of executing tasks based on a given prompt and agent type.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GeneralPurposeAgent"/> class.
/// </remarks>
/// <param name="logger"></param>
/// <param name="agentService"></param>
public class GeneralPurposeAgent(ILogger<GeneralPurposeAgent> logger, IAgentService agentService) : IAgent
{
    // Inject any required services or dependencies here, such as logging, configuration, etc.
    private readonly ILogger<GeneralPurposeAgent> _logger = logger;

    private readonly IAgentService _agentService = agentService;

    public string AgentId { get; } = null!;
    public string AgentType { get; } = null!;

    public Task<bool> CanHandleAsync(AgentContext context)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Executes a task asynchronously based on the provided prompt and agent type.
    /// </summary>
    /// <param name="prompt">The input prompt for the agent.</param>
    /// <param name="agentType">The type of the agent to execute the task.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ExecuteAsync(string prompt, string agentType)
    {
        _logger.LogInformation("Executing task with prompt: {Prompt} and agent type: {AgentType}", prompt, agentType);

        // Perform the task using the provided prompt and agent type.
        return _agentService.ExecuteTaskAsync(prompt, agentType);
    }

    public Task<AgentResult> ExecuteAsync(AgentContext context)
    {
        throw new NotImplementedException();
    }

    public Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetAgentStatusAsync()
    {
        throw new NotImplementedException();
    }
}