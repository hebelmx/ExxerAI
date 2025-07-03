using ModelContextProtocol.Protocol;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ModelContextProtocol.Client;

/// <summary>
/// Provides extension methods for tool operations with an <see cref="IMcpClient"/>.
/// </summary>
public static class McpClientToolExtensions
{
    /// <summary>
    /// Retrieves a list of available tools from the server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="serializerOptions">The serializer options governing tool parameter serialization. If null, the default options will be used.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A list of all available tools as <see cref="McpClientTool"/> instances.</returns>
    /// <remarks>
    /// <para>
    /// This method fetches all available tools from the MCP server and returns them as a complete list.
    /// It automatically handles pagination with cursors if the server responds with only a portion per request.
    /// </para>
    /// <para>
    /// For servers with a large number of tools and that responds with paginated responses, consider using 
    /// <see cref="EnumerateToolsAsync"/> instead, as it streams tools as they arrive rather than loading them all at once.
    /// </para>
    /// <para>
    /// The serializer options provided are flowed to each <see cref="McpClientTool"/> and will be used
    /// when invoking tools in order to serialize any parameters.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get all tools available on the server
    /// var tools = await mcpClient.ListToolsAsync();
    /// 
    /// // Use tools with an AI client
    /// ChatOptions chatOptions = new()
    /// {
    ///     Tools = [.. tools]
    /// };
    /// 
    /// await foreach (var update in chatClient.GetStreamingResponseAsync(userMessage, chatOptions))
    /// {
    ///     Console.Write(update);
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static async ValueTask<IList<McpClientTool>> ListToolsAsync(
        this IMcpClient client,
        JsonSerializerOptions? serializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);

        serializerOptions ??= McpJsonUtilities.DefaultOptions;
        serializerOptions.MakeReadOnly();

        List<McpClientTool>? tools = null;
        string? cursor = null;
        do
        {
            var toolResults = await client.SendRequestAsync(
                RequestMethods.ToolsList,
                new() { Cursor = cursor },
                McpJsonUtilities.JsonContext.Default.ListToolsRequestParams,
                McpJsonUtilities.JsonContext.Default.ListToolsResult,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            tools ??= new List<McpClientTool>(toolResults.Tools.Count);
            foreach (var tool in toolResults.Tools)
            {
                tools.Add(new McpClientTool(client, tool, serializerOptions));
            }

            cursor = toolResults.NextCursor;
        }
        while (cursor is not null);

        return tools;
    }

    /// <summary>
    /// Creates an enumerable for asynchronously enumerating all available tools from the server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="serializerOptions">The serializer options governing tool parameter serialization. If null, the default options will be used.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>An asynchronous sequence of all available tools as <see cref="McpClientTool"/> instances.</returns>
    /// <remarks>
    /// <para>
    /// This method uses asynchronous enumeration to retrieve tools from the server, which allows processing tools
    /// as they arrive rather than waiting for all tools to be retrieved. The method automatically handles pagination
    /// with cursors if the server responds with tools split across multiple responses.
    /// </para>
    /// <para>
    /// The serializer options provided are flowed to each <see cref="McpClientTool"/> and will be used
    /// when invoking tools in order to serialize any parameters.
    /// </para>
    /// <para>
    /// Every iteration through the returned <see cref="IAsyncEnumerable{McpClientTool}"/>
    /// will result in re-querying the server and yielding the sequence of available tools.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Enumerate all tools available on the server
    /// await foreach (var tool in client.EnumerateToolsAsync())
    /// {
    ///     Console.WriteLine($"Tool: {tool.Name}");
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static async IAsyncEnumerable<McpClientTool> EnumerateToolsAsync(
        this IMcpClient client,
        JsonSerializerOptions? serializerOptions = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);

        serializerOptions ??= McpJsonUtilities.DefaultOptions;
        serializerOptions.MakeReadOnly();

        string? cursor = null;
        do
        {
            var toolResults = await client.SendRequestAsync(
                RequestMethods.ToolsList,
                new() { Cursor = cursor },
                McpJsonUtilities.JsonContext.Default.ListToolsRequestParams,
                McpJsonUtilities.JsonContext.Default.ListToolsResult,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            foreach (var tool in toolResults.Tools)
            {
                yield return new McpClientTool(client, tool, serializerOptions);
            }

            cursor = toolResults.NextCursor;
        }
        while (cursor is not null);
    }

    /// <summary>
    /// Invokes a tool on the server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="toolName">The name of the tool to call on the server..</param>
    /// <param name="arguments">An optional dictionary of arguments to pass to the tool. Each key represents a parameter name,
    /// and its associated value represents the argument value.
    /// </param>
    /// <param name="progress">
    /// An optional <see cref="IProgress{T}"/> to have progress notifications reported to it. Setting this to a non-<see langword="null"/>
    /// value will result in a progress token being included in the call, and any resulting progress notifications during the operation
    /// routed to this instance.
    /// </param>
    /// <param name="serializerOptions">
    /// The JSON serialization options governing argument serialization. If <see langword="null"/>, the default serialization options will be used.
    /// </param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>
    /// A task containing the <see cref="CallToolResult"/> from the tool execution. The response includes
    /// the tool's output content, which may be structured data, text, or an error message.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="toolName"/> is <see langword="null"/>.</exception>
    /// <exception cref="McpException">The server could not find the requested tool, or the server encountered an error while processing the request.</exception>
    /// <example>
    /// <code>
    /// // Call a simple echo tool with a string argument
    /// var result = await client.CallToolAsync(
    ///     "echo",
    ///     new Dictionary&lt;string, object?&gt;
    ///     {
    ///         ["message"] = "Hello MCP!"
    ///     });
    /// </code>
    /// </example>
    public static ValueTask<CallToolResult> CallToolAsync(
        this IMcpClient client,
        string toolName,
        IReadOnlyDictionary<string, object?>? arguments = null,
        IProgress<ProgressNotificationValue>? progress = null,
        JsonSerializerOptions? serializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);
        Throw.IfNull(toolName);
        serializerOptions ??= McpJsonUtilities.DefaultOptions;
        serializerOptions.MakeReadOnly();

        if (progress is not null)
        {
            return SendRequestWithProgressAsync(client, toolName, arguments, progress, serializerOptions, cancellationToken);
        }

        return client.SendRequestAsync(
            RequestMethods.ToolsCall,
            new()
            {
                Name = toolName,
                Arguments = ToArgumentsDictionary(arguments, serializerOptions),
            },
            McpJsonUtilities.JsonContext.Default.CallToolRequestParams,
            McpJsonUtilities.JsonContext.Default.CallToolResult,
            cancellationToken: cancellationToken);

        static async ValueTask<CallToolResult> SendRequestWithProgressAsync(
            IMcpClient client,
            string toolName,
            IReadOnlyDictionary<string, object?>? arguments,
            IProgress<ProgressNotificationValue> progress,
            JsonSerializerOptions serializerOptions,
            CancellationToken cancellationToken)
        {
            ProgressToken progressToken = new(Guid.NewGuid().ToString("N"));

            await using var _ = client.RegisterNotificationHandler(NotificationMethods.ProgressNotification,
                (notification, cancellationToken) =>
                {
                    if (JsonSerializer.Deserialize(notification.Params, McpJsonUtilities.JsonContext.Default.ProgressNotificationParams) is { } pn &&
                        pn.ProgressToken == progressToken)
                    {
                        progress.Report(pn.Progress);
                    }

                    return default;
                }).ConfigureAwait(false);

            return await client.SendRequestAsync(
                RequestMethods.ToolsCall,
                new()
                {
                    Name = toolName,
                    Arguments = ToArgumentsDictionary(arguments, serializerOptions),
                    ProgressToken = progressToken,
                },
                McpJsonUtilities.JsonContext.Default.CallToolRequestParams,
                McpJsonUtilities.JsonContext.Default.CallToolResult,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Convers a dictionary with <see cref="object"/> values to a dictionary with <see cref="JsonElement"/> values.</summary>
    private static Dictionary<string, JsonElement>? ToArgumentsDictionary(
        IReadOnlyDictionary<string, object?>? arguments, JsonSerializerOptions options)
    {
        var typeInfo = options.GetTypeInfo<object?>();

        Dictionary<string, JsonElement>? result = null;
        if (arguments is not null)
        {
            result = new(arguments.Count);
            foreach (var kvp in arguments)
            {
                result.Add(kvp.Key, kvp.Value is JsonElement je ? je : JsonSerializer.SerializeToElement(kvp.Value, typeInfo));
            }
        }

        return result;
    }
} 