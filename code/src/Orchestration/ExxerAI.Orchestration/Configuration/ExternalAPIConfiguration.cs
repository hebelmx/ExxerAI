namespace ExxerAI.Orchestration.Configuration;

/// <summary>
/// External API integrations configuration
/// Note: All API keys are managed by KeyStore
/// </summary>
public class ExternalAPIConfiguration
{
    public bool EnableOpenAI { get; set; } = false;
    public bool EnableAnthropic { get; set; } = false;
    public bool EnableGoogleCloud { get; set; } = false;
    public bool EnableAzureOpenAI { get; set; } = false;
    public bool EnableHuggingFace { get; set; } = false;
    
    // Configuration endpoints (keys stored in KeyStore)
    public string OpenAIEndpoint { get; set; } = "https://api.openai.com/v1";
    public string AnthropicEndpoint { get; set; } = "https://api.anthropic.com";
    public string AzureOpenAIEndpoint { get; set; } = "";
    public string HuggingFaceEndpoint { get; set; } = "https://api-inference.huggingface.co";
}