using ModelContextProtocol.Protocol;
using System.Runtime.CompilerServices;

namespace ModelContextProtocol.Client;

/// <summary>
/// Provides extension methods for resource operations with an <see cref="IMcpClient"/>.
/// </summary>
public static class McpClientResourceExtensions
{
    /// <summary>
    /// Retrieves a list of available resources from the server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A list of all available resources as <see cref="Resource"/> instances.</returns>
    /// <remarks>
    /// <para>
    /// This method fetches all available resources from the MCP server and returns them as a complete list.
    /// It automatically handles pagination with cursors if the server responds with only a portion per request.
    /// </para>
    /// <para>
    /// For servers with a large number of resources and that responds with paginated responses, consider using 
    /// <see cref="EnumerateResourcesAsync"/> instead, as it streams resources as they arrive rather than loading them all at once.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Get all resources available on the server
    /// var resources = await client.ListResourcesAsync();
    /// 
    /// // Display information about each resource
    /// foreach (var resource in resources)
    /// {
    ///     Console.WriteLine($"Resource URI: {resource.Uri}");
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static async ValueTask<IList<McpClientResource>> ListResourcesAsync(
        this IMcpClient client, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);

        List<McpClientResource>? resources = null;

        string? cursor = null;
        do
        {
            var resourceResults = await client.SendRequestAsync(
                RequestMethods.ResourcesList,
                new() { Cursor = cursor },
                McpJsonUtilities.JsonContext.Default.ListResourcesRequestParams,
                McpJsonUtilities.JsonContext.Default.ListResourcesResult,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            resources ??= new List<McpClientResource>(resourceResults.Resources.Count);
            foreach (var resource in resourceResults.Resources)
            {
                resources.Add(new McpClientResource(client, resource));
            }

            cursor = resourceResults.NextCursor;
        }
        while (cursor is not null);

        return resources;
    }

    /// <summary>
    /// Creates an enumerable for asynchronously enumerating all available resources from the server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>An asynchronous sequence of all available resources as <see cref="Resource"/> instances.</returns>
    /// <remarks>
    /// <para>
    /// This method uses asynchronous enumeration to retrieve resources from the server, which allows processing resources
    /// as they arrive rather than waiting for all resources to be retrieved. The method automatically handles pagination
    /// with cursors if the server responds with resources split across multiple responses.
    /// </para>
    /// <para>
    /// Every iteration through the returned <see cref="IAsyncEnumerable{McpClientResource}"/>
    /// will result in re-querying the server and yielding the sequence of available resources.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Enumerate all resources available on the server
    /// await foreach (var resource in client.EnumerateResourcesAsync())
    /// {
    ///     Console.WriteLine($"Resource URI: {resource.Uri}");
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static async IAsyncEnumerable<McpClientResource> EnumerateResourcesAsync(
        this IMcpClient client, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);

        string? cursor = null;
        do
        {
            var resourceResults = await client.SendRequestAsync(
                RequestMethods.ResourcesList,
                new() { Cursor = cursor },
                McpJsonUtilities.JsonContext.Default.ListResourcesRequestParams,
                McpJsonUtilities.JsonContext.Default.ListResourcesResult,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            foreach (var resource in resourceResults.Resources)
            {
                yield return new McpClientResource(client, resource);
            }

            cursor = resourceResults.NextCursor;
        }
        while (cursor is not null);
    }

    /// <summary>
    /// Reads a resource from the server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="uri">The uri of the resource.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is <see langword="null"/>.</exception>
    public static ValueTask<ReadResourceResult> ReadResourceAsync(
        this IMcpClient client, string uri, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);
        Throw.IfNullOrWhiteSpace(uri);

        return client.SendRequestAsync(
            RequestMethods.ResourcesRead,
            new() { Uri = uri },
            McpJsonUtilities.JsonContext.Default.ReadResourceRequestParams,
            McpJsonUtilities.JsonContext.Default.ReadResourceResult,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Reads a resource from the server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="uri">The uri of the resource.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is <see langword="null"/>.</exception>
    public static ValueTask<ReadResourceResult> ReadResourceAsync(
        this IMcpClient client, Uri uri, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);
        Throw.IfNull(uri);

        return ReadResourceAsync(client, uri.ToString(), cancellationToken);
    }

    /// <summary>
    /// Reads a resource from the server using a URI template with arguments.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="uriTemplate">The URI template to expand with the provided arguments.</param>
    /// <param name="arguments">The arguments to substitute into the URI template.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="uriTemplate"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="arguments"/> is <see langword="null"/>.</exception>
    public static ValueTask<ReadResourceResult> ReadResourceAsync(
        this IMcpClient client, string uriTemplate, IReadOnlyDictionary<string, object?> arguments, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);
        Throw.IfNullOrWhiteSpace(uriTemplate);
        Throw.IfNull(arguments);

        return client.SendRequestAsync(
            RequestMethods.ResourcesRead,
            new() { Uri = UriTemplate.FormatUri(uriTemplate, arguments) },
            McpJsonUtilities.JsonContext.Default.ReadResourceRequestParams,
            McpJsonUtilities.JsonContext.Default.ReadResourceResult,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Subscribes to a resource on the server to receive notifications when it changes.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="uri">The URI of the resource to which to subscribe.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This method allows the client to register interest in a specific resource identified by its URI.
    /// When the resource changes, the server will send notifications to the client, enabling real-time
    /// updates without polling.
    /// </para>
    /// <para>
    /// The subscription remains active until explicitly unsubscribed using <see cref="M:UnsubscribeFromResourceAsync"/>
    /// or until the client disconnects from the server.
    /// </para>
    /// <para>
    /// To handle resource change notifications, register an event handler for the appropriate notification events,
    /// such as with <see cref="IMcpEndpoint.RegisterNotificationHandler"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is empty or composed entirely of whitespace.</exception>
    public static Task SubscribeToResourceAsync(this IMcpClient client, string uri, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);
        Throw.IfNullOrWhiteSpace(uri);

        return client.SendRequestAsync(
            RequestMethods.ResourcesSubscribe,
            new() { Uri = uri },
            McpJsonUtilities.JsonContext.Default.SubscribeRequestParams,
            McpJsonUtilities.JsonContext.Default.EmptyResult,
            cancellationToken: cancellationToken).AsTask();
    }

    /// <summary>
    /// Subscribes to a resource on the server to receive notifications when it changes.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="uri">The URI of the resource to which to subscribe.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This method allows the client to register interest in a specific resource identified by its URI.
    /// When the resource changes, the server will send notifications to the client, enabling real-time
    /// updates without polling.
    /// </para>
    /// <para>
    /// The subscription remains active until explicitly unsubscribed using <see cref="M:UnsubscribeFromResourceAsync"/>
    /// or until the client disconnects from the server.
    /// </para>
    /// <para>
    /// To handle resource change notifications, register an event handler for the appropriate notification events,
    /// such as with <see cref="IMcpEndpoint.RegisterNotificationHandler"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is <see langword="null"/>.</exception>
    public static Task SubscribeToResourceAsync(this IMcpClient client, Uri uri, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);
        Throw.IfNull(uri);

        return SubscribeToResourceAsync(client, uri.ToString(), cancellationToken);
    }

    /// <summary>
    /// Unsubscribes from a resource on the server to stop receiving notifications about its changes.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="uri">The URI of the resource to unsubscribe from.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This method cancels a previous subscription to a resource, stopping the client from receiving
    /// notifications when that resource changes.
    /// </para>
    /// <para>
    /// The unsubscribe operation is idempotent, meaning it can be called multiple times for the same
    /// resource without causing errors, even if there is no active subscription.
    /// </para>
    /// <para>
    /// Due to the nature of the MCP protocol, it is possible the client may receive notifications after
    /// unsubscribing if those notifications were issued by the server prior to the unsubscribe request being received.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="uri"/> is empty or composed entirely of whitespace.</exception>
    public static Task UnsubscribeFromResourceAsync(this IMcpClient client, string uri, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);
        Throw.IfNullOrWhiteSpace(uri);

        return client.SendRequestAsync(
            RequestMethods.ResourcesUnsubscribe,
            new() { Uri = uri },
            McpJsonUtilities.JsonContext.Default.UnsubscribeRequestParams,
            McpJsonUtilities.JsonContext.Default.EmptyResult,
            cancellationToken: cancellationToken).AsTask();
    }

    /// <summary>
    /// Unsubscribes from a resource on the server to stop receiving notifications about its changes.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="uri">The URI of the resource to unsubscribe from.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This method cancels a previous subscription to a resource, stopping the client from receiving
    /// notifications when that resource changes.
    /// </para>
    /// <para>
    /// The unsubscribe operation is idempotent, meaning it can be called multiple times for the same
    /// resource without causing errors, even if there is no active subscription.
    /// </para>
    /// <para>
    /// Due to the nature of the MCP protocol, it is possible the client may receive notifications after
    /// unsubscribing if those notifications were issued by the server prior to the unsubscribe request being received.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="uri"/> is <see langword="null"/>.</exception>
    public static Task UnsubscribeFromResourceAsync(this IMcpClient client, Uri uri, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);
        Throw.IfNull(uri);

        return UnsubscribeFromResourceAsync(client, uri.ToString(), cancellationToken);
    }
} 