using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Interface for Model Context Protocol (MCP) integration
/// CRITICAL: MCP calls may soon exceed human requests - fundamental capability
/// </summary>
public interface IMcpService
{
    /// <summary>
    /// Connects to an MCP server
    /// </summary>
    /// <param name="serverUri">MCP server URI</param>
    /// <param name="capabilities">Client capabilities</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Connection result with server info</returns>
    Task<McpConnectionResult> ConnectToServerAsync(
        string serverUri, 
        McpClientCapabilities capabilities, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Discovers available tools from MCP server
    /// </summary>
    /// <param name="serverId">Server identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Available MCP tools</returns>
    Task<IEnumerable<McpTool>> DiscoverToolsAsync(
        string serverId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes MCP tool call
    /// </summary>
    /// <param name="serverId">Server identifier</param>
    /// <param name="toolCall">Tool call request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tool execution result</returns>
    Task<McpToolResult> ExecuteToolAsync(
        string serverId, 
        McpToolCall toolCall, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available resources from MCP server
    /// </summary>
    /// <param name="serverId">Server identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Available MCP resources</returns>
    Task<IEnumerable<McpResource>> GetResourcesAsync(
        string serverId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads content from MCP resource
    /// </summary>
    /// <param name="serverId">Server identifier</param>
    /// <param name="resourceUri">Resource URI</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Resource content</returns>
    Task<McpResourceContent> ReadResourceAsync(
        string serverId, 
        string resourceUri, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to MCP resource changes
    /// </summary>
    /// <param name="serverId">Server identifier</param>
    /// <param name="resourceUri">Resource URI</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Subscription result</returns>
    Task<McpSubscription> SubscribeToResourceAsync(
        string serverId, 
        string resourceUri, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available MCP prompts from server
    /// </summary>
    /// <param name="serverId">Server identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Available MCP prompts</returns>
    Task<IEnumerable<McpPrompt>> GetPromptsAsync(
        string serverId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes MCP prompt
    /// </summary>
    /// <param name="serverId">Server identifier</param>
    /// <param name="promptName">Prompt name</param>
    /// <param name="arguments">Prompt arguments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Prompt execution result</returns>
    Task<McpPromptResult> ExecutePromptAsync(
        string serverId, 
        string promptName, 
        Dictionary<string, object> arguments, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Manages MCP server sessions
    /// </summary>
    /// <param name="serverId">Server identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session status</returns>
    Task<McpSessionStatus> GetSessionStatusAsync(
        string serverId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from MCP server
    /// </summary>
    /// <param name="serverId">Server identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Disconnection result</returns>
    Task<bool> DisconnectFromServerAsync(string serverId, CancellationToken cancellationToken = default);
} 