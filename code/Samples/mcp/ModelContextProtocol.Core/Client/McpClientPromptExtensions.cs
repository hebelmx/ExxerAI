using ModelContextProtocol.Protocol;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ModelContextProtocol.Client;

/// <summary>
/// Provides extension methods for prompt operations with an <see cref="IMcpClient"/>.
/// </summary>
public static class McpClientPromptExtensions
{
    /// <summary>
    /// Retrieves a list of available prompts from the server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A list of all available prompts as <see cref="McpClientPrompt"/> instances.</returns>
    /// <remarks>
    /// <para>
    /// This method fetches all available prompts from the MCP server and returns them as a complete list.
    /// It automatically handles pagination with cursors if the server responds with only a portion per request.
    /// </para>
    /// <para>
    /// For servers with a large number of prompts and that responds with paginated responses, consider using 
    /// <see cref="EnumeratePromptsAsync"/> instead, as it streams prompts as they arrive rather than loading them all at once.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static async ValueTask<IList<McpClientPrompt>> ListPromptsAsync(
        this IMcpClient client, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);

        List<McpClientPrompt>? prompts = null;
        string? cursor = null;
        do
        {
            var promptResults = await client.SendRequestAsync(
                RequestMethods.PromptsList,
                new() { Cursor = cursor },
                McpJsonUtilities.JsonContext.Default.ListPromptsRequestParams,
                McpJsonUtilities.JsonContext.Default.ListPromptsResult,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            prompts ??= new List<McpClientPrompt>(promptResults.Prompts.Count);
            foreach (var prompt in promptResults.Prompts)
            {
                prompts.Add(new McpClientPrompt(client, prompt));
            }

            cursor = promptResults.NextCursor;
        }
        while (cursor is not null);

        return prompts;
    }

    /// <summary>
    /// Creates an enumerable for asynchronously enumerating all available prompts from the server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>An asynchronous sequence of all available prompts as <see cref="McpClientPrompt"/> instances.</returns>
    /// <remarks>
    /// <para>
    /// This method uses asynchronous enumeration to retrieve prompts from the server, which allows processing prompts
    /// as they arrive rather than waiting for all prompts to be retrieved. The method automatically handles pagination
    /// with cursors if the server responds with prompts split across multiple responses.
    /// </para>
    /// <para>
    /// Every iteration through the returned <see cref="IAsyncEnumerable{McpClientPrompt}"/>
    /// will result in re-querying the server and yielding the sequence of available prompts.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Enumerate all prompts available on the server
    /// await foreach (var prompt in client.EnumeratePromptsAsync())
    /// {
    ///     Console.WriteLine($"Prompt: {prompt.Name}");
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static async IAsyncEnumerable<McpClientPrompt> EnumeratePromptsAsync(
        this IMcpClient client, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);

        string? cursor = null;
        do
        {
            var promptResults = await client.SendRequestAsync(
                RequestMethods.PromptsList,
                new() { Cursor = cursor },
                McpJsonUtilities.JsonContext.Default.ListPromptsRequestParams,
                McpJsonUtilities.JsonContext.Default.ListPromptsResult,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            foreach (var prompt in promptResults.Prompts)
            {
                yield return new(client, prompt);
            }

            cursor = promptResults.NextCursor;
        }
        while (cursor is not null);
    }

    /// <summary>
    /// Retrieves a specific prompt from the MCP server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="name">The name of the prompt to retrieve.</param>
    /// <param name="arguments">Optional arguments for the prompt. Keys are parameter names, and values are the argument values.</param>
    /// <param name="serializerOptions">The serialization options governing argument serialization.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task containing the prompt's result with content and messages.</returns>
    /// <remarks>
    /// <para>
    /// This method sends a request to the MCP server to create the specified prompt with the provided arguments.
    /// The server will process the arguments and return a prompt containing messages or other content.
    /// </para>
    /// <para>
    /// Arguments are serialized into JSON and passed to the server, where they may be used to customize the 
    /// prompt's behavior or content. Each prompt may have different argument requirements.
    /// </para>
    /// <para>
    /// The returned <see cref="GetPromptResult"/> contains a collection of <see cref="PromptMessage"/> objects,
    /// which can be converted to <see cref="ChatMessage"/> objects using the <see cref="AIContentExtensions.ToChatMessages"/> method.
    /// </para>
    /// </remarks>
    /// <exception cref="McpException">Thrown when the prompt does not exist, when required arguments are missing, or when the server encounters an error processing the prompt.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static ValueTask<GetPromptResult> GetPromptAsync(
        this IMcpClient client,
        string name,
        IReadOnlyDictionary<string, object?>? arguments = null,
        JsonSerializerOptions? serializerOptions = null,
        CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);
        Throw.IfNullOrWhiteSpace(name);

        serializerOptions ??= McpJsonUtilities.DefaultOptions;
        serializerOptions.MakeReadOnly();

        return client.SendRequestAsync(
            RequestMethods.PromptsGet,
            new() { Name = name, Arguments = ToArgumentsDictionary(arguments, serializerOptions) },
            McpJsonUtilities.JsonContext.Default.GetPromptRequestParams,
            McpJsonUtilities.JsonContext.Default.GetPromptResult,
            cancellationToken: cancellationToken);
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