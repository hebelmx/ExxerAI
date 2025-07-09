namespace ExxerAI.Orchestration.Services;

/// <summary>
/// SignalR hub for real-time dashboard updates
/// </summary>
public class DashboardHub : Hub
{
    /// <summary>
    /// Join a monitoring group for real-time updates
    /// </summary>
    public async Task JoinMonitoringGroupAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "monitoring");
    }

    /// <summary>
    /// Leave the monitoring group
    /// </summary>
    public async Task LeaveMonitoringGroupAsync()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "monitoring");
    }
}