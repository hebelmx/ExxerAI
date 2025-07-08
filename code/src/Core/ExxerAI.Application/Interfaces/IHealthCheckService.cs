using ExxerAI.Domain;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Service for monitoring and reporting system health status across all components
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Performs a comprehensive health check of all system components
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Overall system health report</returns>
    Task<Result<SystemHealthReport>> CheckSystemHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a health check of a specific component
    /// </summary>
    /// <param name="componentName">Name of the component to check</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Component-specific health report</returns>
    Task<Result<ComponentHealthReport>> CheckComponentHealthAsync(string componentName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a health check provider for a specific component
    /// </summary>
    /// <param name="componentName">Name of the component</param>
    /// <param name="healthCheckProvider">Health check provider implementation</param>
    void RegisterHealthCheckProvider(string componentName, IHealthCheckProvider healthCheckProvider);

    /// <summary>
    /// Gets the current health status for all registered components
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Dictionary of component names and their health status</returns>
    Task<Result<Dictionary<string, HealthStatus>>> GetHealthStatusSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Monitors system health continuously and reports degradations
    /// </summary>
    /// <param name="reportCallback">Callback for health status changes</param>
    /// <param name="checkInterval">Interval between health checks</param>
    /// <param name="cancellationToken">Cancellation token for the monitoring</param>
    /// <returns>Task representing the monitoring operation</returns>
    Task MonitorHealthAsync(Action<SystemHealthReport> reportCallback, TimeSpan checkInterval, CancellationToken cancellationToken = default);
}