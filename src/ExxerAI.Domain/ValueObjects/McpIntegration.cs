namespace ExxerAI.Domain.ValueObjects;

/// <summary>
/// MCP client capabilities for server negotiation
/// </summary>
public record McpClientCapabilities
{
    public string ProtocolVersion { get; init; } = "1.0.0";
    public List<string> SupportedTools { get; init; } = new();
    public List<string> SupportedResources { get; init; } = new();
    public List<string> SupportedPrompts { get; init; } = new();
    public bool SupportsStreaming { get; init; } = true;
    public bool SupportsNotifications { get; init; } = true;
    public Dictionary<string, object> Extensions { get; init; } = new();
}

/// <summary>
/// Result of MCP server connection
/// </summary>
public record McpConnectionResult
{
    public bool IsSuccessful { get; init; }
    public string ServerId { get; init; } = string.Empty;
    public string ServerName { get; init; } = string.Empty;
    public string ProtocolVersion { get; init; } = string.Empty;
    public McpServerCapabilities ServerCapabilities { get; init; } = new();
    public string ErrorMessage { get; init; } = string.Empty;
    public DateTime ConnectedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// MCP server capabilities
/// </summary>
public record McpServerCapabilities
{
    public List<McpTool> AvailableTools { get; init; } = new();
    public List<McpResource> AvailableResources { get; init; } = new();
    public List<McpPrompt> AvailablePrompts { get; init; } = new();
    public bool SupportsSubscriptions { get; init; }
    public Dictionary<string, object> ServerInfo { get; init; } = new();
}

/// <summary>
/// MCP tool definition
/// </summary>
public record McpTool
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public McpToolSchema InputSchema { get; init; } = new();
    public List<string> Categories { get; init; } = new();
    public bool RequiresAuth { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// MCP tool input schema
/// </summary>
public record McpToolSchema
{
    public string Type { get; init; } = "object";
    public Dictionary<string, McpProperty> Properties { get; init; } = new();
    public List<string> Required { get; init; } = new();
}

/// <summary>
/// MCP property definition
/// </summary>
public record McpProperty
{
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public object? Default { get; init; }
    public List<object> Enum { get; init; } = new();
}

/// <summary>
/// MCP tool call request
/// </summary>
public record McpToolCall
{
    public string ToolName { get; init; } = string.Empty;
    public Dictionary<string, object> Arguments { get; init; } = new();
    public string CallId { get; init; } = Guid.NewGuid().ToString();
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// MCP tool execution result
/// </summary>
public record McpToolResult
{
    public string CallId { get; init; } = string.Empty;
    public bool IsSuccessful { get; init; }
    public object? Result { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public TimeSpan ExecutionTime { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// MCP resource definition
/// </summary>
public record McpResource
{
    public string Uri { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public long? Size { get; init; }
    public DateTime? LastModified { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// MCP resource content
/// </summary>
public record McpResourceContent
{
    public string Uri { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public byte[]? BinaryContent { get; init; }
    public DateTime RetrievedAt { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// MCP subscription information
/// </summary>
public record McpSubscription
{
    public string SubscriptionId { get; init; } = Guid.NewGuid().ToString();
    public string ResourceUri { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool IsActive { get; init; } = true;
    public string CallbackUrl { get; init; } = string.Empty;
}

/// <summary>
/// MCP prompt definition
/// </summary>
public record McpPrompt
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<McpPromptArgument> Arguments { get; init; } = new();
    public string Template { get; init; } = string.Empty;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// MCP prompt argument
/// </summary>
public record McpPromptArgument
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Type { get; init; } = "string";
    public bool Required { get; init; }
    public object? Default { get; init; }
}

/// <summary>
/// MCP prompt execution result
/// </summary>
public record McpPromptResult
{
    public string PromptName { get; init; } = string.Empty;
    public string GeneratedPrompt { get; init; } = string.Empty;
    public Dictionary<string, object> ResolvedArguments { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// MCP session status
/// </summary>
public record McpSessionStatus
{
    public string ServerId { get; init; } = string.Empty;
    public bool IsConnected { get; init; }
    public DateTime LastActivity { get; init; }
    public int ToolCallsCount { get; init; }
    public int ResourceAccessCount { get; init; }
    public List<string> ActiveSubscriptions { get; init; } = new();
}

/// <summary>
/// MCP tool call recommendation
/// </summary>
public record McpToolCallRecommendation
{
    public bool ShouldCallTool { get; init; }
    public string RecommendedTool { get; init; } = string.Empty;
    public Dictionary<string, object> SuggestedArguments { get; init; } = new();
    public string Reasoning { get; init; } = string.Empty;
    public float ConfidenceLevel { get; init; } = 0.0f;
} 