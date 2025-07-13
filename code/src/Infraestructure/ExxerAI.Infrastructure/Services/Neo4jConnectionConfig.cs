namespace ExxerAI.Infrastructure.Services;

/// <summary>
/// Configuration for connecting to Neo4j graph database
/// </summary>
public class Neo4jConnectionConfig
{
    /// <summary>
    /// Gets or sets the Bolt URI for Neo4j connection
    /// </summary>
    public string BoltUri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP URI for Neo4j connection
    /// </summary>
    public string HttpUri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username for Neo4j authentication
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for Neo4j authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database name (default: neo4j)
    /// </summary>
    public string Database { get; set; } = "neo4j";
} 