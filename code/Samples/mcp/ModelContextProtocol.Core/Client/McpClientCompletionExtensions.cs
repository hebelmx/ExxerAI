using ModelContextProtocol.Protocol;

namespace ModelContextProtocol.Client;

/// <summary>
/// Provides extension methods for completion operations with an <see cref="IMcpClient"/>.
/// </summary>
public static class McpClientCompletionExtensions
{
    /// <summary>
    /// Requests completion suggestions for a specific argument of a resource, prompt, or tool.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="reference">A reference to the resource, prompt, or tool for which to request completion suggestions.</param>
    /// <param name="argumentName">The name of the argument for which to request completion suggestions.</param>
    /// <param name="argumentValue">The current (potentially partial) value of the argument.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task containing completion suggestions for the specified argument.</returns>
    /// <remarks>
    /// <para>
    /// This method is useful for providing autocomplete or suggestion functionality in user interfaces.
    /// The server can analyze the current argument value and context to provide relevant completion options.
    /// </para>
    /// <para>
    /// The reference parameter specifies what the completion is being requested for (e.g., a specific tool or prompt),
    /// while the argument name and value specify which specific parameter needs completion assistance.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="reference"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="argumentName"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="McpException">Thrown when the server encounters an error processing the completion request.</exception>
    public static ValueTask<CompleteResult> CompleteAsync(this IMcpClient client, Reference reference, string argumentName, string argumentValue, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);
        Throw.IfNull(reference);
        Throw.IfNullOrWhiteSpace(argumentName);

        return client.SendRequestAsync(
            RequestMethods.CompletionComplete,
            new()
            {
                Ref = reference,
                Argument = new()
                {
                    Name = argumentName,
                    Value = argumentValue
                }
            },
            McpJsonUtilities.JsonContext.Default.CompleteRequestParams,
            McpJsonUtilities.JsonContext.Default.CompleteResult,
            cancellationToken: cancellationToken);
    }
} 