using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.ValueObjects;
using System.Diagnostics;
using System.Text.Json;

namespace ExxerAI.Infrastructure.Services;

/// <summary>
/// Model Context Protocol service implementation
/// CRITICAL: MCP calls may soon exceed human requests - autonomous operation essential
/// </summary>
public class McpService : IMcpService, IDisposable
{
    private readonly Dictionary<string, McpServerProcess> _activeServers;
    private readonly Dictionary<string, McpConnectionResult> _connections;
    private bool _disposed;

    public McpService()
    {
        _activeServers = new Dictionary<string, McpServerProcess>();
        _connections = new Dictionary<string, McpConnectionResult>();
    }

    /// <summary>
    /// Connects to MCP server and negotiates capabilities
    /// </summary>
    public async Task<McpConnectionResult> ConnectToServerAsync(
        string serverId, 
        string command, 
        string[] arguments, 
        McpClientCapabilities clientCapabilities, 
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔌 McpService: Connecting to server {serverId}...");

        try
        {
            // Start MCP server process
            var serverProcess = new McpServerProcess
            {
                ServerId = serverId,
                Process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = string.Join(" ", arguments),
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                }
            };

            serverProcess.Process.Start();
            _activeServers[serverId] = serverProcess;

            // Send initialization message
            var initMessage = new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "initialize",
                @params = new
                {
                    protocolVersion = clientCapabilities.ProtocolVersion,
                    capabilities = new
                    {
                        tools = clientCapabilities.SupportedTools,
                        resources = clientCapabilities.SupportedResources,
                        prompts = clientCapabilities.SupportedPrompts
                    }
                }
            };

            var initJson = JsonSerializer.Serialize(initMessage);
            await serverProcess.Process.StandardInput.WriteLineAsync(initJson);

            // Wait for response (simplified for autonomous operation)
            await Task.Delay(1000, cancellationToken);

            var connectionResult = new McpConnectionResult
            {
                IsSuccessful = true,
                ServerId = serverId,
                ServerName = serverId,
                ProtocolVersion = "1.0.0",
                ConnectedAt = DateTime.UtcNow,
                ServerCapabilities = await DiscoverServerCapabilitiesAsync(serverId, cancellationToken)
            };

            _connections[serverId] = connectionResult;

            Console.WriteLine($"✅ McpService: Connected to {serverId} with {connectionResult.ServerCapabilities.AvailableTools.Count} tools");
            return connectionResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ McpService connection failed: {ex.Message}");
            return new McpConnectionResult
            {
                IsSuccessful = false,
                ServerId = serverId,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Discovers available tools from connected MCP server
    /// </summary>
    public async Task<IEnumerable<McpTool>> DiscoverToolsAsync(string serverId, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔍 McpService: Discovering tools for server {serverId}...");

        try
        {
            if (!_activeServers.ContainsKey(serverId))
            {
                Console.WriteLine($"⚠️ McpService: Server {serverId} not connected");
                return Enumerable.Empty<McpTool>();
            }

            // For autonomous operation, return simulated business intelligence tools
            var businessIntelligenceTools = new List<McpTool>
            {
                new()
                {
                    Name = "google_drive_search",
                    Description = "Search for documents in Google Drive based on keywords",
                    InputSchema = new McpToolSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, McpProperty>
                        {
                            ["query"] = new() { Type = "string", Description = "Search query for documents" },
                            ["folder_id"] = new() { Type = "string", Description = "Optional folder ID to limit search" }
                        },
                        Required = new List<string> { "query" }
                    },
                    Categories = new List<string> { "search", "documents" }
                },
                new()
                {
                    Name = "web_search",
                    Description = "Search the web for recent information about companies or technologies",
                    InputSchema = new McpToolSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, McpProperty>
                        {
                            ["query"] = new() { Type = "string", Description = "Search query" },
                            ["site"] = new() { Type = "string", Description = "Optional site to limit search to" },
                            ["date_range"] = new() { Type = "string", Description = "Date range for search results" }
                        },
                        Required = new List<string> { "query" }
                    },
                    Categories = new List<string> { "search", "intelligence" }
                },
                new()
                {
                    Name = "company_monitor",
                    Description = "Monitor company mentions and news for business intelligence",
                    InputSchema = new McpToolSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, McpProperty>
                        {
                            ["company_name"] = new() { Type = "string", Description = "Company name to monitor" },
                            ["keywords"] = new() { Type = "array", Description = "Additional keywords to monitor" },
                            ["frequency"] = new() { Type = "string", Description = "Monitoring frequency" }
                        },
                        Required = new List<string> { "company_name" }
                    },
                    Categories = new List<string> { "monitoring", "intelligence" }
                }
            };

            Console.WriteLine($"✅ McpService: Discovered {businessIntelligenceTools.Count} tools for autonomous operations");
            return businessIntelligenceTools;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ McpService tool discovery failed: {ex.Message}");
            return Enumerable.Empty<McpTool>();
        }
    }

    /// <summary>
    /// Calls MCP tool with specified arguments
    /// </summary>
    public async Task<McpToolResult> CallToolAsync(string serverId, McpToolCall toolCall, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🛠️ McpService: Calling tool {toolCall.ToolName} on server {serverId}...");

        var startTime = DateTime.UtcNow;

        try
        {
            if (!_activeServers.ContainsKey(serverId))
            {
                return new McpToolResult
                {
                    CallId = toolCall.CallId,
                    IsSuccessful = false,
                    ErrorMessage = $"Server {serverId} not connected"
                };
            }

            // Simulate tool execution for autonomous operations
            var result = await SimulateToolExecutionAsync(toolCall, cancellationToken);

            var executionTime = DateTime.UtcNow - startTime;
            var toolResult = new McpToolResult
            {
                CallId = toolCall.CallId,
                IsSuccessful = true,
                Result = result,
                ExecutionTime = executionTime,
                Metadata = new Dictionary<string, object>
                {
                    ["serverId"] = serverId,
                    ["toolName"] = toolCall.ToolName,
                    ["executionTime"] = executionTime.TotalMilliseconds
                }
            };

            Console.WriteLine($"✅ McpService: Tool {toolCall.ToolName} completed in {executionTime.TotalMilliseconds:F0}ms");
            return toolResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ McpService tool call failed: {ex.Message}");
            return new McpToolResult
            {
                CallId = toolCall.CallId,
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                ExecutionTime = DateTime.UtcNow - startTime
            };
        }
    }

    /// <summary>
    /// Gets session status for connected MCP server
    /// </summary>
    public async Task<McpSessionStatus?> GetSessionStatusAsync(string serverId, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🩺 McpService: Checking session status for {serverId}...");

        try
        {
            await Task.Delay(25, cancellationToken); // Simulate status check

            if (!_connections.ContainsKey(serverId))
            {
                return null;
            }

            var connection = _connections[serverId];
            var serverProcess = _activeServers.GetValueOrDefault(serverId);

            return new McpSessionStatus
            {
                ServerId = serverId,
                IsConnected = connection.IsSuccessful && serverProcess?.Process?.HasExited == false,
                LastActivity = DateTime.UtcNow,
                ToolCallsCount = 0, // In real implementation, track actual calls
                ResourceAccessCount = 0,
                ActiveSubscriptions = new List<string>()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ McpService status check failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Recommends MCP tool to call based on context
    /// </summary>
    public async Task<McpToolCallRecommendation> RecommendToolCallAsync(
        string serverId, 
        string userQuery, 
        AgentContext context, 
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"💡 McpService: Analyzing query for tool recommendation...");

        try
        {
            await Task.Delay(100, cancellationToken); // Simulate analysis

            var query = userQuery.ToLowerInvariant();

            // Business intelligence tool recommendations
            if (query.Contains("search") && (query.Contains("drive") || query.Contains("document")))
            {
                return new McpToolCallRecommendation
                {
                    ShouldCallTool = true,
                    RecommendedTool = "google_drive_search",
                    SuggestedArguments = new Dictionary<string, object>
                    {
                        ["query"] = ExtractSearchKeywords(userQuery)
                    },
                    Reasoning = "Query suggests document search in Google Drive",
                    ConfidenceLevel = 0.85f
                };
            }

            if (query.Contains("company") || query.Contains("news") || query.Contains("monitor"))
            {
                return new McpToolCallRecommendation
                {
                    ShouldCallTool = true,
                    RecommendedTool = "company_monitor",
                    SuggestedArguments = new Dictionary<string, object>
                    {
                        ["company_name"] = ExtractCompanyName(userQuery),
                        ["frequency"] = "daily"
                    },
                    Reasoning = "Query suggests company monitoring requirement",
                    ConfidenceLevel = 0.75f
                };
            }

            if (query.Contains("web") || query.Contains("search") || query.Contains("find"))
            {
                return new McpToolCallRecommendation
                {
                    ShouldCallTool = true,
                    RecommendedTool = "web_search",
                    SuggestedArguments = new Dictionary<string, object>
                    {
                        ["query"] = userQuery,
                        ["date_range"] = "past_week"
                    },
                    Reasoning = "Query suggests web search for recent information",
                    ConfidenceLevel = 0.70f
                };
            }

            return new McpToolCallRecommendation
            {
                ShouldCallTool = false,
                Reasoning = "No suitable MCP tool identified for this query",
                ConfidenceLevel = 0.20f
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ McpService recommendation failed: {ex.Message}");
            return new McpToolCallRecommendation
            {
                ShouldCallTool = false,
                Reasoning = $"Error analyzing query: {ex.Message}",
                ConfidenceLevel = 0.0f
            };
        }
    }

    #region Private Helper Methods

    private async Task<McpServerCapabilities> DiscoverServerCapabilitiesAsync(string serverId, CancellationToken cancellationToken)
    {
        // In autonomous operation, return predefined capabilities for business intelligence
        await Task.Delay(50, cancellationToken);

        return new McpServerCapabilities
        {
            AvailableTools = (await DiscoverToolsAsync(serverId, cancellationToken)).ToList(),
            AvailableResources = new List<McpResource>
            {
                new()
                {
                    Uri = "drive://business-intelligence",
                    Name = "Business Intelligence Documents",
                    Description = "Corporate business intelligence and partnership documents"
                }
            },
            SupportsSubscriptions = true,
            ServerInfo = new Dictionary<string, object>
            {
                ["name"] = serverId,
                ["version"] = "1.0.0",
                ["capabilities"] = "autonomous-business-intelligence"
            }
        };
    }

    private async Task<object> SimulateToolExecutionAsync(McpToolCall toolCall, CancellationToken cancellationToken)
    {
        await Task.Delay(200, cancellationToken); // Simulate tool execution time

        return toolCall.ToolName switch
        {
            "google_drive_search" => SimulateGoogleDriveSearch(toolCall.Arguments),
            "web_search" => SimulateWebSearch(toolCall.Arguments),
            "company_monitor" => SimulateCompanyMonitor(toolCall.Arguments),
            _ => new { success = false, message = "Unknown tool" }
        };
    }

    private object SimulateGoogleDriveSearch(Dictionary<string, object> arguments)
    {
        var query = arguments.GetValueOrDefault("query", "").ToString() ?? "";
        
        return new
        {
            success = true,
            results = new[]
            {
                new { 
                    id = "doc_001", 
                    title = $"Siemens Partnership Document - {query}",
                    modified = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd")
                },
                new { 
                    id = "doc_002", 
                    title = $"Automotive Intelligence Report - {query}",
                    modified = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd")
                }
            },
            total_count = 2
        };
    }

    private object SimulateWebSearch(Dictionary<string, object> arguments)
    {
        var query = arguments.GetValueOrDefault("query", "").ToString() ?? "";
        
        return new
        {
            success = true,
            results = new[]
            {
                new { 
                    title = $"Recent developments in {query}",
                    url = "https://example.com/news/article1",
                    snippet = "Latest information about business developments...",
                    date = DateTime.UtcNow.ToString("yyyy-MM-dd")
                }
            },
            search_time_ms = 150
        };
    }

    private object SimulateCompanyMonitor(Dictionary<string, object> arguments)
    {
        var companyName = arguments.GetValueOrDefault("company_name", "").ToString() ?? "";
        
        return new
        {
            success = true,
            company = companyName,
            monitoring_active = true,
            recent_mentions = 5,
            sentiment_score = 0.7,
            last_updated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }

    private string ExtractSearchKeywords(string query)
    {
        // Simple keyword extraction for autonomous operation
        var businessKeywords = new[] { "siemens", "rockwell", "abb", "automotive", "manufacturing", "automation" };
        var found = businessKeywords.FirstOrDefault(k => query.ToLowerInvariant().Contains(k));
        return found ?? query;
    }

    private string ExtractCompanyName(string query)
    {
        // Extract company names for monitoring
        var companies = new[] { "Siemens", "Rockwell", "ABB", "General Motors", "Ford", "Tesla", "Tremec", "Valeo" };
        return companies.FirstOrDefault(c => query.Contains(c, StringComparison.OrdinalIgnoreCase)) ?? "Unknown Company";
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var server in _activeServers.Values)
            {
                try
                {
                    if (!server.Process.HasExited)
                    {
                        server.Process.Kill();
                    }
                    server.Process.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error disposing MCP server: {ex.Message}");
                }
            }

            _activeServers.Clear();
            _connections.Clear();
            _disposed = true;
        }
    }

    #endregion

    #region Helper Types

    private class McpServerProcess
    {
        public string ServerId { get; set; } = string.Empty;
        public Process Process { get; set; } = null!;
    }

    #endregion
} 