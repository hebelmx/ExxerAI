using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.ValueObjects;
using System.Text;
using System.Text.Json;

namespace ExxerAI.Infrastructure.Services;

/// <summary>
/// LLM Provider implementation for local Ollama instance using direct HTTP calls
/// </summary>
public class OllamaProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;
    private readonly string _baseUrl;

    public string ProviderName => "Ollama";

    public OllamaProvider(string modelName = "llama3.1", string baseUrl = "http://localhost:11434")
    {
        _modelName = modelName;
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Generates a response based on the input prompt
    /// </summary>
    public async Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            var requestBody = new
            {
                model = _modelName,
                prompt = prompt,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

            return responseObj.GetProperty("response").GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate response from Ollama: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generates a structured response for agent execution
    /// </summary>
    public async Task<AgentResult> GenerateAgentResponseAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create a structured prompt for the agent
            var systemPrompt = $"""
                You are an autonomous AI agent.
                Task: {context.Input}
                Context ID: {context.ContextId}
                
                Please provide a thoughtful response that addresses the task.
                Be concise but thorough in your analysis and recommendations.
                """;

            var response = await GenerateResponseAsync(systemPrompt, cancellationToken);

            return new AgentResult
            {
                IsSuccessful = true,
                Output = response,
                ExecutionTimeMs = 0,
                Metadata = new Dictionary<string, object>
                {
                    ["model"] = _modelName,
                    ["provider"] = ProviderName,
                    ["timestamp"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            return AgentResult.CreateFailure($"Agent execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the provider is available and healthy
    /// </summary>
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var testResponse = await GenerateResponseAsync("Hello", CancellationToken.None);
            return !string.IsNullOrEmpty(testResponse);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Dispose the HTTP client
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
} 