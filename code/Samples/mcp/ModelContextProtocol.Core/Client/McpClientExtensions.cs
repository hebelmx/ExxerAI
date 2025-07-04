namespace ModelContextProtocol.Client;

/// <summary>
/// Legacy compatibility class that imports all focused MCP client extension methods.
/// </summary>
/// <remarks>
/// <para>
/// This class exists for backward compatibility. All extension methods have been moved to focused classes:
/// </para>
/// <list type="bullet">
/// <item><see cref="McpClientConnectionExtensions"/> - Connection and server management (ping, logging)</item>
/// <item><see cref="McpClientToolExtensions"/> - Tool operations (list, enumerate, call)</item>
/// <item><see cref="McpClientPromptExtensions"/> - Prompt operations (list, enumerate, get)</item>
/// <item><see cref="McpClientResourceExtensions"/> - Resource operations (list, read, subscribe)</item>
/// <item><see cref="McpClientResourceTemplateExtensions"/> - Resource template operations</item>
/// <item><see cref="McpClientCompletionExtensions"/> - Completion functionality</item>
/// <item><see cref="McpClientSamplingExtensions"/> - AI sampling and chat integration</item>
/// </list>
/// <para>
/// Use the specific extension classes directly for better organization and performance.
/// </para>
/// </remarks>
public static class McpClientExtensions
{
    // This class now serves as a compatibility layer.
    // All extension methods have been moved to focused classes for better organization:
    //
    // - McpClientConnectionExtensions: Connection management, ping, logging
    // - McpClientToolExtensions: Tool listing, enumeration, and execution
    // - McpClientPromptExtensions: Prompt listing, enumeration, and retrieval
    // - McpClientResourceExtensions: Resource listing, reading, subscriptions
    // - McpClientResourceTemplateExtensions: Resource template operations
    // - McpClientCompletionExtensions: Completion functionality
    // - McpClientSamplingExtensions: AI sampling and chat integration
    //
    // This preserves backward compatibility while providing better organization.
}