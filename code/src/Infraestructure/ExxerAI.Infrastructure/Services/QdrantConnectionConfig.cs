namespace ExxerAI.Infrastructure.Services;

/// <summary>
/// Configuration for connecting to Qdrant vector database
/// </summary>
public class QdrantConnectionConfig
{
    /// <summary>
    /// Gets or sets the host address for Qdrant connection
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP port for Qdrant connection
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the gRPC port for Qdrant connection
    /// </summary>
    public int GrpcPort { get; set; }

    /// <summary>
    /// Gets or sets the HTTP URL for Qdrant connection
    /// </summary>
    public string HttpUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the connection uses HTTPS/TLS
    /// </summary>
    public bool IsSecure { get; set; }
} 