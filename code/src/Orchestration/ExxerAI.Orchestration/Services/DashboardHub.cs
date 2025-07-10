namespace ExxerAI.Orchestration.Services;

/// <summary>
/// SignalR hub for real-time dashboard updates during orchestration processes.
/// </summary>
public class DashboardHub : Hub
{
    /// <summary>
    /// Join a monitoring group for real-time updates
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    public async Task JoinMonitoringGroupAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return;

        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "monitoring", cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // SignalR hub methods handle cancellation gracefully
            return;
        }
    }

    /// <summary>
    /// Leave the monitoring group
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    public async Task LeaveMonitoringGroupAsync(CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return;

        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "monitoring", cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // SignalR hub methods handle cancellation gracefully
            return;
        }
    }
}