namespace ExxerAI.Orchestration.Services;

/// <summary>
/// Background service for real-time health monitoring
/// </summary>
public class HealthCheckService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(
        IServiceProvider serviceProvider,
        IHubContext<DashboardHub> hubContext,
        ILogger<HealthCheckService> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var monitoring = scope.ServiceProvider.GetRequiredService<ServiceMonitoringService>();

                var status = await monitoring.GetServiceStatusAsync(stoppingToken);
                var metrics = await monitoring.GetSystemMetricsAsync(stoppingToken);

                // Send real-time updates to connected clients
                await _hubContext.Clients.Group("monitoring").SendAsync("ServiceStatusUpdate", status, stoppingToken);
                await _hubContext.Clients.Group("monitoring").SendAsync("SystemMetricsUpdate", metrics, stoppingToken);

                _logger.LogDebug("Health check completed. Overall status: {Status}", status.OverallStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during health check execution");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}