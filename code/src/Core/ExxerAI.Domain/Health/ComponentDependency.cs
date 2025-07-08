using ExxerAI.Application.Interfaces;

namespace ExxerAI.Domain.Health;

/// <summary>
/// Represents a dependency that a component relies on
/// </summary>
public class ComponentDependency
{
    /// <summary>
    /// Name of the dependency
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of dependency (Database, API, Service, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the dependency
    /// </summary>
    public HealthStatus Status { get; set; }

    /// <summary>
    /// Connection string or endpoint
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Version of the dependency
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Response time to the dependency
    /// </summary>
    public TimeSpan? ResponseTime { get; set; }

    /// <summary>
    /// Last successful connection timestamp
    /// </summary>
    public DateTime? LastSuccessfulConnection { get; set; }

    /// <summary>
    /// Error message if the dependency is unhealthy
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether this dependency is critical for component operation
    /// </summary>
    public bool IsCritical { get; set; } = true;

    /// <summary>
    /// Connection timeout configuration
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Additional metadata about the dependency
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}