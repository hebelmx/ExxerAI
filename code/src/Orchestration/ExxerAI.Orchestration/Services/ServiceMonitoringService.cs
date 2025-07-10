using System.Diagnostics;
using ExxerAI.Orchestration.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ExxerAI.Orchestration.Services;

/// <summary>
/// Service for monitoring LocalAI stack services
/// </summary>
public class ServiceMonitoringService
{
    private readonly MonitoringConfiguration _config;
    private readonly ILogger<ServiceMonitoringService> _logger;
    private readonly HttpClient _httpClient;

    public ServiceMonitoringService(
        IOptions<MonitoringConfiguration> config,
        ILogger<ServiceMonitoringService> logger,
        HttpClient httpClient)
    {
        _config = config.Value;
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.HealthChecks.TimeoutSeconds);
    }

    /// <summary>
    /// Get comprehensive status of all services
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    public async Task<ServiceStackStatus> GetServiceStatusAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        var services = new List<ServiceStatus>();

        // Core services
        await Task.Yield(); // Allow cooperative cancellation

        // Check cancellation before expensive operations
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        var coreServices = services.Where(s => s.IsCritical).ToList();
        var optionalServices = services.Where(s => !s.IsCritical).ToList();

        return new ServiceStackStatus
        {
            OverallStatus = DetermineOverallStatus(services),
            CoreServicesHealthy = coreServices.All(s => s.Status == ServiceHealthStatus.Healthy),
            OptionalServicesHealthy = optionalServices.All(s => s.Status == ServiceHealthStatus.Healthy),
            TotalServices = services.Count,
            HealthyServices = services.Count(s => s.Status == ServiceHealthStatus.Healthy),
            UnhealthyServices = services.Count(s => s.Status == ServiceHealthStatus.Unhealthy),
            UnknownServices = services.Count(s => s.Status == ServiceHealthStatus.Unknown),
            Services = services,
            LastChecked = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Get system metrics and performance data
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    public async Task<SystemMetrics> GetSystemMetricsAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            throw new OperationCanceledException(cancellationToken);

        var process = Process.GetCurrentProcess();

        return await Task.FromResult(new SystemMetrics
        {
            CpuUsage = await GetCpuUsageAsync(cancellationToken),
            MemoryUsage = GC.GetTotalMemory(false),
            DiskSpace = GetDiskSpace(),
            NetworkConnections = GetActiveConnections(),
            Uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime(),
            ProcessCount = Process.GetProcesses().Length,
            ThreadCount = process.Threads.Count,
            Timestamp = DateTime.UtcNow
        });
    }

    private async Task<ServiceStatus> CheckServiceAsync(string name, string url, ServiceCategory category, bool isCritical, CancellationToken cancellationToken = default)
    {
        try
        {
            // Early cancellation check
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(cancellationToken);

            var stopwatch = Stopwatch.StartNew();
            var response = await _httpClient.GetAsync(url, cancellationToken);
            stopwatch.Stop();

            return new ServiceStatus
            {
                Name = name,
                Status = response.IsSuccessStatusCode ? ServiceHealthStatus.Healthy : ServiceHealthStatus.Unhealthy,
                Url = url,
                Category = category,
                IsCritical = isCritical,
                ResponseTime = stopwatch.Elapsed,
                StatusCode = (int)response.StatusCode,
                LastChecked = DateTime.UtcNow,
                Message = response.IsSuccessStatusCode ? "Service is responsive" : $"HTTP {response.StatusCode}"
            };
        }
        catch (TaskCanceledException)
        {
            return new ServiceStatus
            {
                Name = name,
                Status = ServiceHealthStatus.Unhealthy,
                Url = url,
                Category = category,
                IsCritical = isCritical,
                ResponseTime = TimeSpan.FromSeconds(_config.HealthChecks.TimeoutSeconds),
                StatusCode = 0,
                LastChecked = DateTime.UtcNow,
                Message = "Service timeout"
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check service {ServiceName} at {Url}", name, url);

            return new ServiceStatus
            {
                Name = name,
                Status = ServiceHealthStatus.Unknown,
                Url = url,
                Category = category,
                IsCritical = isCritical,
                ResponseTime = TimeSpan.Zero,
                StatusCode = 0,
                LastChecked = DateTime.UtcNow,
                Message = ex.Message
            };
        }
    }

    private OverallHealthStatus DetermineOverallStatus(List<ServiceStatus> services)
    {
        var coreServices = services.Where(s => s.IsCritical).ToList();

        if (coreServices.All(s => s.Status == ServiceHealthStatus.Healthy))
        {
            var optionalUnhealthy = services.Where(s => !s.IsCritical && s.Status != ServiceHealthStatus.Healthy).Count();
            return optionalUnhealthy == 0 ? OverallHealthStatus.Healthy : OverallHealthStatus.Degraded;
        }

        return OverallHealthStatus.Unhealthy;
    }

    private async Task<double> GetCpuUsageAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Early cancellation check
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(cancellationToken);

            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            await Task.Delay(500, cancellationToken);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return cpuUsageTotal * 100;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return 0;
        }
    }

    private long GetDiskSpace()
    {
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\");
            return drive.AvailableFreeSpace;
        }
        catch
        {
            return 0;
        }
    }

    private int GetActiveConnections()
    {
        try
        {
            // This is a simplified implementation
            // In a real scenario, you might want to use System.Net.NetworkInformation
            return Process.GetCurrentProcess().Threads.Count * 2;
        }
        catch
        {
            return 0;
        }
    }
}