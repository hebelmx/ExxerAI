using ExxerAI.Domain.Operations;

namespace ExxerAI.Domain.Health;

/// <summary>
/// Provider interface for component-specific health checks
/// </summary>
public interface IHealthCheckProvider
{
    /// <summary>
    /// Name of the component this provider monitors
    /// </summary>
    string ComponentName { get; }

    /// <summary>
    /// Performs a health check for this component
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Health report for the component</returns>
    Task<Result<ComponentHealthReport>> CheckHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a quick health status check (for frequent monitoring)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Simple health status</returns>
    Task<Result<HealthStatus>> CheckHealthStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Priority level for this health check (higher priority checked first)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Timeout for this health check operation
    /// </summary>
    TimeSpan Timeout { get; }
}