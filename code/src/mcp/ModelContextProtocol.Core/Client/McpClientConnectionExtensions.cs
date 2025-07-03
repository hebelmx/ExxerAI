using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace ModelContextProtocol.Client;

/// <summary>
/// Provides extension methods for connection and server management operations with an <see cref="IMcpClient"/>.
/// </summary>
public static class McpClientConnectionExtensions
{
    /// <summary>
    /// Sends a ping request to verify server connectivity.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that completes when the ping is successful.</returns>
    /// <remarks>
    /// <para>
    /// This method is used to check if the MCP server is online and responding to requests.
    /// It can be useful for health checking, ensuring the connection is established, or verifying 
    /// that the client has proper authorization to communicate with the server.
    /// </para>
    /// <para>
    /// The ping operation is lightweight and does not require any parameters. A successful completion
    /// of the task indicates that the server is operational and accessible.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    /// <exception cref="McpException">Thrown when the server cannot be reached or returns an error response.</exception>
    public static Task PingAsync(this IMcpClient client, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);

        return client.SendRequestAsync(
            RequestMethods.Ping,
            parameters: null,
            McpJsonUtilities.JsonContext.Default.Object!,
            McpJsonUtilities.JsonContext.Default.Object,
            cancellationToken: cancellationToken).AsTask();
    }

    /// <summary>
    /// Sets the logging level for the MCP server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="level">The logging level to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that completes when the logging level has been set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static Task SetLoggingLevel(this IMcpClient client, LoggingLevel level, CancellationToken cancellationToken = default)
    {
        Throw.IfNull(client);

        return client.SendNotificationAsync(
            NotificationMethods.LoggingSetLevel,
            new() { Level = level },
            McpJsonUtilities.JsonContext.Default.SetLevelNotificationParams,
            cancellationToken: cancellationToken).AsTask();
    }

    /// <summary>
    /// Sets the logging level for the MCP server.
    /// </summary>
    /// <param name="client">The client instance used to communicate with the MCP server.</param>
    /// <param name="level">The .NET logging level to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that completes when the logging level has been set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static Task SetLoggingLevel(this IMcpClient client, LogLevel level, CancellationToken cancellationToken = default) =>
        SetLoggingLevel(client, level.ToLoggingLevel(), cancellationToken);
} 