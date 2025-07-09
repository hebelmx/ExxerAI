namespace ExxerAI.Orchestration.Configuration;

/// <summary>
/// Search engine (SearXNG) configuration
/// </summary>
public class SearchConfiguration
{
    public int Port { get; set; } = 8080;
    public string BaseUrl { get; set; } = "http://localhost:8080/";
    public bool EnableSafeSearch { get; set; } = true;
    public string[] DefaultEngines { get; set; } = { "google", "bing", "duckduckgo" };
}