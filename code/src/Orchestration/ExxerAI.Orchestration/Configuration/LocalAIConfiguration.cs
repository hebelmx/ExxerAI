namespace ExxerAI.Orchestration.Configuration;

/// <summary>
/// LocalAI and Open WebUI configuration
/// Note: API keys are managed by KeyStore
/// </summary>
public class LocalAIConfiguration
{
    public int ApiPort { get; set; } = 8081;
    public int WebUIPort { get; set; } = 3001;
    public string ModelsPath { get; set; } = "./models";
    public string ApiKey { get; set; } = ""; // Managed by KeyStore
    public bool EnableCors { get; set; } = true;
    public int Workers { get; set; } = 1;
    public string DefaultModel { get; set; } = "llama-3.2-3b-instruct";
    public ExternalAPIConfiguration ExternalAPIs { get; set; } = new();
}