using ModelContextProtocol.Protocol;
using System.Runtime.CompilerServices;

namespace ModelContextProtocol.Client;

/// <summary>
/// Provides extension methods for resource template operations with an <see cref="IMcpClient"/>.
/// </summary>
public static class McpClientResourceTemplateExtensions
{
    /// <summary>
    /// Retrieves a list of available resource templates from the server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A list of all available resource templates as <see cref="ResourceTemplate"/> instances.</returns>
    /// <remarks>
    /// <para>
    /// This method fetches all available resource templates from the MCP server and returns them as a complete list.
    /// It automatically handles pagination with cursors if the server responds with only a portion per request.
    /// </para>
    /// <para>
    /// For servers with a large number of resource templates and that responds with paginated responses, consider using 
    /// <see cref="EnumerateResourceTemplatesAsync"/> instead, as it streams templates as they arrive rather than loading them all at once.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static async ValueTask<IList<McpClientResourceTemplate>> ListResourceTemplatesAsync(
        this IMcpClient client, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);

        List<McpClientResourceTemplate>? resourceTemplates = null;

        string? cursor = null;
        do
        {
            var templateResults = await client.SendRequestAsync(
                RequestMethods.ResourcesTemplatesList,
                new() { Cursor = cursor },
                McpJsonUtilities.JsonContext.Default.ListResourceTemplatesRequestParams,
                McpJsonUtilities.JsonContext.Default.ListResourceTemplatesResult,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            resourceTemplates ??= new List<McpClientResourceTemplate>(templateResults.ResourceTemplates.Count);
            foreach (var template in templateResults.ResourceTemplates)
            {
                resourceTemplates.Add(new McpClientResourceTemplate(client, template));
            }

            cursor = templateResults.NextCursor;
        }
        while (cursor is not null);

        return resourceTemplates;
    }

    /// <summary>
    /// Creates an enumerable for asynchronously enumerating all available resource templates from the server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>An asynchronous sequence of all available resource templates as <see cref="ResourceTemplate"/> instances.</returns>
    /// <remarks>
    /// <para>
    /// This method uses asynchronous enumeration to retrieve resource templates from the server, which allows processing templates
    /// as they arrive rather than waiting for all templates to be retrieved. The method automatically handles pagination
    /// with cursors if the server responds with templates split across multiple responses.
    /// </para>
    /// <para>
    /// Every iteration through the returned <see cref="IAsyncEnumerable{McpClientResourceTemplate}"/>
    /// will result in re-querying the server and yielding the sequence of available resource templates.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Enumerate all resource templates available on the server
    /// await foreach (var template in client.EnumerateResourceTemplatesAsync())
    /// {
    ///     Console.WriteLine($"Template: {template.Name}");
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static async IAsyncEnumerable<McpClientResourceTemplate> EnumerateResourceTemplatesAsync(
        this IMcpClient client, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);

        string? cursor = null;
        do
        {
            var templateResults = await client.SendRequestAsync(
                RequestMethods.ResourcesTemplatesList,
                new() { Cursor = cursor },
                McpJsonUtilities.JsonContext.Default.ListResourceTemplatesRequestParams,
                McpJsonUtilities.JsonContext.Default.ListResourceTemplatesResult,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            foreach (var templateResult in templateResults.ResourceTemplates)
            {
                yield return new McpClientResourceTemplate(client, templateResult);
            }

            cursor = templateResults.NextCursor;
        }
        while (cursor is not null);
    }
} 