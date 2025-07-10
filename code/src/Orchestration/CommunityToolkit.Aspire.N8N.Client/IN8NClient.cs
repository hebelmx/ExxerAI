using System.Threading.Tasks;

namespace CommunityToolkit.Aspire.N8N.Client;

/// <summary>
/// Contract for communicating with the N8N instance.
/// </summary>
public interface IN8NClient
{
    Task<string> GetHealthAsync();

    Task<string> GetMetricsAsync();

    Task<string> TriggerWorkflowAsync(string workflowId, object? input = null);
}