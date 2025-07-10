using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CommunityToolkit.Aspire.N8N.Client;

/// <summary>
/// Concrete implementation of the IN8NClient using HttpClient.
/// </summary>
public class N8NHttpClient : IN8NClient
{
    private readonly HttpClient _httpClient;

    public N8NHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetHealthAsync()
    {
        var response = await _httpClient.GetAsync("/healthz");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> GetMetricsAsync()
    {
        var response = await _httpClient.GetAsync("/metrics");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> TriggerWorkflowAsync(string workflowId, object? input = null)
    {
        var url = $"/webhook/{workflowId}";
        var response = await _httpClient.PostAsJsonAsync(url, input ?? new { });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}