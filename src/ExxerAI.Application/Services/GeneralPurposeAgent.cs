using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ExxerAI.Application.Services;

/// <summary>
/// General-purpose autonomous AI agent capable of handling various tasks
/// </summary>
public class GeneralPurposeAgent : IAgent
{
    private readonly ILogger<GeneralPurposeAgent> _logger;
    private readonly ILLMProvider _llmProvider;

    public string AgentId { get; }
    public string AgentType { get; }

    public GeneralPurposeAgent(
        ILogger<GeneralPurposeAgent> logger, 
        ILLMProvider llmProvider,
        string? agentId = null,
        string agentType = "GeneralPurpose")
    {
        _logger = logger;
        _llmProvider = llmProvider;
        AgentId = agentId ?? Guid.NewGuid().ToString();
        AgentType = agentType;
    }

    /// <summary>
    /// Determines if this agent can handle the given context
    /// </summary>
    public async Task<bool> CanHandleAsync(AgentContext context)
    {
        try
        {
            // General purpose agent can handle most tasks
            // More specialized logic can be added here later
            var isHealthy = await _llmProvider.IsHealthyAsync();
            return isHealthy && !string.IsNullOrEmpty(context.Input);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error checking if agent can handle context: {Error}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Executes the agent with the provided context
    /// </summary>
    public async Task<AgentResult> ExecuteAsync(AgentContext context)
    {
        return await ExecuteAsync(context, CancellationToken.None);
    }

    /// <summary>
    /// Executes the agent with the provided context and cancellation token
    /// </summary>
    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Agent {AgentId} executing task: {Input}", AgentId, context.Input);

        try
        {
            // Use the LLM provider to generate intelligent responses
            var result = await _llmProvider.GenerateAgentResponseAsync(context, cancellationToken);
            
            _logger.LogInformation("Agent {AgentId} completed task successfully", AgentId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent {AgentId} failed to execute task: {Input}", AgentId, context.Input);
            return AgentResult.CreateFailure($"Execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Legacy method for backwards compatibility
    /// </summary>
    public async Task ExecuteAsync(string prompt, string agentType)
    {
        var context = new AgentContext { Input = prompt };
        var result = await ExecuteAsync(context);
        
        _logger.LogInformation("Legacy execution completed with result: {Success}", result.IsSuccessful);
    }

    /// <summary>
    /// Gets the current status of the agent
    /// </summary>
    public async Task<string> GetAgentStatusAsync()
    {
        try
        {
            var isHealthy = await _llmProvider.IsHealthyAsync();
            var status = isHealthy ? "Ready" : "LLM Provider Unavailable";
            
            return $"Agent {AgentId} ({AgentType}): {status}";
        }
        catch (Exception ex)
        {
            return $"Agent {AgentId} ({AgentType}): Error - {ex.Message}";
        }
    }
} 