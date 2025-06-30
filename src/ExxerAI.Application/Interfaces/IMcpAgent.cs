using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for MCP-aware agents that can discover and use MCP tools
/// </summary>
public interface IMcpAgent : IAgent
{
    /// <summary>
    /// Automatically discovers relevant MCP tools for the given context
    /// </summary>
    /// <param name="context">Agent execution context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Relevant MCP tools for the task</returns>
    Task<IEnumerable<McpTool>> DiscoverRelevantToolsAsync(
        AgentContext context, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes agent task with MCP tool integration
    /// </summary>
    /// <param name="context">Agent execution context</param>
    /// <param name="availableTools">Available MCP tools</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Agent result with MCP tool usage</returns>
    Task<AgentResult> ExecuteWithMcpToolsAsync(
        AgentContext context, 
        IEnumerable<McpTool> availableTools, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if MCP tools are needed for the given context
    /// </summary>
    /// <param name="context">Agent execution context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if MCP tools would enhance the task</returns>
    Task<bool> RequiresMcpToolsAsync(
        AgentContext context, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets preferred MCP servers for this agent type
    /// </summary>
    /// <returns>List of preferred MCP server URIs</returns>
    IEnumerable<string> GetPreferredMcpServers();

    /// <summary>
    /// Evaluates MCP tool call necessity during execution
    /// </summary>
    /// <param name="currentContext">Current execution context</param>
    /// <param name="partialResult">Partial execution result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>MCP tool call recommendation</returns>
    Task<McpToolCallRecommendation> EvaluateToolCallNeedAsync(
        AgentContext currentContext, 
        string partialResult, 
        CancellationToken cancellationToken = default);
} 