namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Enumeration of system health statuses.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// System is operating normally.
    /// </summary>
    Healthy,

    /// <summary>
    /// System has minor issues but is functional.
    /// </summary>
    Warning,

    /// <summary>
    /// System has significant issues affecting functionality.
    /// </summary>
    Degraded,

    /// <summary>
    /// System is not functional.
    /// </summary>
    Critical
}