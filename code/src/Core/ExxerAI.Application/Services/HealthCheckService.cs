using ExxerAI.Domain;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.Health;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ExxerAI.Application.Services;

/// <summary>
/// Service for monitoring and reporting system health status across all components
/// </summary>
public class HealthCheckService : IHealthCheckService
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly ConcurrentDictionary<string, IHealthCheckProvider> _healthCheckProviders;
    private readonly UptimeInfo _uptimeInfo;
    private readonly object _lockObject = new();

    public HealthCheckService(ILogger<HealthCheckService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _healthCheckProviders = new ConcurrentDictionary<string, IHealthCheckProvider>();
        _uptimeInfo = new UptimeInfo
        {
            StartedAt = DateTime.UtcNow,
            Version = GetApplicationVersion(),
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            HostName = Environment.MachineName,
            ProcessId = Environment.ProcessId,
            RestartCount = 0
        };

        _logger.LogInformation("HealthCheckService initialized at {StartTime}", _uptimeInfo.StartedAt);
    }

    /// <inheritdoc />
    public async Task<Result<SystemHealthReport>> CheckSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTime.UtcNow;

            _logger.LogInformation("Starting comprehensive system health check");

            var systemReport = new SystemHealthReport
            {
                CheckedAt = startTime,
                Uptime = _uptimeInfo,
                Performance = await GatherPerformanceMetricsAsync(cancellationToken)
            };

            // Get all health check providers ordered by priority
            var providers = _healthCheckProviders.Values
                .OrderByDescending(p => p.Priority)
                .ToList();

            // Execute health checks with timeout and error handling
            var componentTasks = providers.Select(async provider =>
            {
                try
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(provider.Timeout);

                    var result = await provider.CheckHealthAsync(timeoutCts.Token);
                    return (provider.ComponentName, Result: result);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return (provider.ComponentName, Result: Result<ComponentHealthReport>.WithFailure($"Health check cancelled for {provider.ComponentName}"));
                }
                catch (OperationCanceledException)
                {
                    return (provider.ComponentName, Result: Result<ComponentHealthReport>.WithFailure($"Health check timeout for {provider.ComponentName} after {provider.Timeout}"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during health check for component {ComponentName}", provider.ComponentName);
                    return (provider.ComponentName, Result: Result<ComponentHealthReport>.WithFailure($"Health check failed for {provider.ComponentName}: {ex.Message}"));
                }
            });

            var componentResults = await Task.WhenAll(componentTasks);

            // Process component results
            foreach (var (componentName, result) in componentResults)
            {
                if (result.IsSuccess && result.Value != null)
                {
                    systemReport.ComponentReports[componentName] = result.Value;
                }
                else
                {
                    // Create a failed component report
                    var failedReport = new ComponentHealthReport
                    {
                        ComponentName = componentName,
                        Status = HealthStatus.Critical,
                        CheckedAt = DateTime.UtcNow,
                        StatusMessage = result.Error ?? "Health check failed",
                        CheckDuration = TimeSpan.Zero
                    };
                    failedReport.AddIssue(HealthIssueSeverity.Critical, result.Error ?? "Health check failed");
                    systemReport.ComponentReports[componentName] = failedReport;
                }
            }

            stopwatch.Stop();
            systemReport.TotalCheckDuration = stopwatch.Elapsed;

            // Determine overall system status
            systemReport.OverallStatus = DetermineOverallStatus(systemReport.ComponentReports.Values);

            // Collect issues and warnings
            CollectSystemIssues(systemReport);

            _logger.LogInformation("System health check completed in {Duration}ms. Overall status: {Status}",
                stopwatch.ElapsedMilliseconds, systemReport.OverallStatus);

            return Result<SystemHealthReport>.Success(systemReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform system health check");
            return Result<SystemHealthReport>.WithFailure($"System health check failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ComponentHealthReport>> CheckComponentHealthAsync(string componentName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(componentName))
        {
            return Result<ComponentHealthReport>.WithFailure("Component name cannot be null or empty");
        }

        if (!_healthCheckProviders.TryGetValue(componentName, out var provider))
        {
            return Result<ComponentHealthReport>.WithFailure($"No health check provider registered for component '{componentName}'");
        }

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(provider.Timeout);

            var result = await provider.CheckHealthAsync(timeoutCts.Token);
            _logger.LogDebug("Health check completed for component {ComponentName} with status {Status}",
                componentName, result.IsSuccess ? result.Value?.Status : "Failed");

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Result<ComponentHealthReport>.WithFailure($"Health check cancelled for component '{componentName}'");
        }
        catch (OperationCanceledException)
        {
            return Result<ComponentHealthReport>.WithFailure($"Health check timeout for component '{componentName}' after {provider.Timeout}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check for component {ComponentName}", componentName);
            return Result<ComponentHealthReport>.WithFailure($"Health check failed for component '{componentName}': {ex.Message}");
        }
    }

    /// <inheritdoc />
    public void RegisterHealthCheckProvider(string componentName, IHealthCheckProvider healthCheckProvider)
    {
        if (string.IsNullOrEmpty(componentName))
            throw new ArgumentException("Component name cannot be null or empty", nameof(componentName));

        if (healthCheckProvider == null)
            throw new ArgumentNullException(nameof(healthCheckProvider));

        lock (_lockObject)
        {
            _healthCheckProviders.AddOrUpdate(componentName, healthCheckProvider, (key, existing) =>
            {
                _logger.LogWarning("Replacing existing health check provider for component {ComponentName}", componentName);
                return healthCheckProvider;
            });
        }

        _logger.LogInformation("Registered health check provider for component {ComponentName} with priority {Priority}",
            componentName, healthCheckProvider.Priority);
    }

    /// <inheritdoc />
    public async Task<Result<Dictionary<string, HealthStatus>>> GetHealthStatusSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var statusTasks = _healthCheckProviders.Values.Select(async provider =>
            {
                try
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(5)); // Quick status check timeout

                    var result = await provider.CheckHealthStatusAsync(timeoutCts.Token);
                    return (provider.ComponentName, Status: result.IsSuccess ? result.Value : HealthStatus.Critical);
                }
                catch
                {
                    return (provider.ComponentName, Status: HealthStatus.Critical);
                }
            });

            var results = await Task.WhenAll(statusTasks);
            var statusSummary = results.ToDictionary(r => r.ComponentName, r => r.Status);

            return Result<Dictionary<string, HealthStatus>>.Success(statusSummary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get health status summary");
            return Result<Dictionary<string, HealthStatus>>.WithFailure($"Failed to get health status summary: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task MonitorHealthAsync(Action<SystemHealthReport> reportCallback, TimeSpan checkInterval, CancellationToken cancellationToken = default)
    {
        if (reportCallback == null)
            throw new ArgumentNullException(nameof(reportCallback));

        _logger.LogInformation("Starting health monitoring with interval {Interval}", checkInterval);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var healthResult = await CheckSystemHealthAsync(cancellationToken);

                if (healthResult.IsSuccess && healthResult.Value is not null)
                {
                    reportCallback(healthResult.Value);
                }
                else
                {
                    _logger.LogWarning("Health check failed during monitoring: {Error}", healthResult.Error);
                }

                await Task.Delay(checkInterval, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Health monitoring stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health monitoring");
            throw;
        }
    }

    private static HealthStatus DetermineOverallStatus(IEnumerable<ComponentHealthReport> componentReports)
    {
        if (!componentReports.Any())
            return HealthStatus.Warning;

        var statuses = componentReports.Select(r => r.Status).ToList();

        if (statuses.Contains(HealthStatus.Critical))
            return HealthStatus.Critical;

        if (statuses.Contains(HealthStatus.Degraded))
            return HealthStatus.Degraded;

        if (statuses.Contains(HealthStatus.Warning))
            return HealthStatus.Warning;

        return HealthStatus.Healthy;
    }

    private static void CollectSystemIssues(SystemHealthReport systemReport)
    {
        foreach (var componentReport in systemReport.ComponentReports.Values)
        {
            var criticalIssues = componentReport.GetCriticalIssues()
                .Select(i => $"{componentReport.ComponentName}: {i.Message}");
            systemReport.CriticalIssues.AddRange(criticalIssues);

            var warningIssues = componentReport.GetWarningIssues()
                .Select(i => $"{componentReport.ComponentName}: {i.Message}");
            systemReport.Warnings.AddRange(warningIssues);
        }
    }

    private static async Task<PerformanceMetrics> GatherPerformanceMetricsAsync(CancellationToken cancellationToken)
    {
        var metrics = new PerformanceMetrics();

        try
        {
            // Gather system performance metrics
            var process = Process.GetCurrentProcess();

            metrics.MemoryUsageBytes = process.WorkingSet64;
            metrics.ThreadCount = process.Threads.Count;
            metrics.HandleCount = process.HandleCount;

            // GC metrics
            metrics.GCMetrics = new GarbageCollectionMetrics
            {
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                TotalAllocatedBytes = GC.GetTotalAllocatedBytes()
            };

            // Total system memory (approximate)
            var gcMemoryInfo = GC.GetGCMemoryInfo();
            metrics.TotalMemoryBytes = gcMemoryInfo.TotalAvailableMemoryBytes;

            // Disk usage for current drive
            var currentDirectory = Directory.GetCurrentDirectory();
            var drive = new DriveInfo(Path.GetPathRoot(currentDirectory) ?? "C:");
            if (drive.IsReady)
            {
                metrics.DiskUsage[drive.Name] = new DiskUsage
                {
                    Drive = drive.Name,
                    TotalBytes = drive.TotalSize,
                    FreeBytes = drive.TotalFreeSpace
                };
            }
        }
        catch (Exception)
        {
            // Performance metrics gathering is best-effort
        }

        await Task.CompletedTask; // For async consistency
        return metrics;
    }

    private static string GetApplicationVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            return assembly?.GetName().Version?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}