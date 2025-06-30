using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Services;

/// <summary>
/// Specialized agent for data analysis, insights, and analytical thinking
/// </summary>
public class AnalysisAgent : IAgent
{
    private readonly ILLMProvider _llmProvider;

    public string AgentId { get; }
    public string AgentType { get; }

    public AnalysisAgent(
        ILLMProvider llmProvider,
        string? agentId = null)
    {
        _llmProvider = llmProvider;
        AgentId = agentId ?? Guid.NewGuid().ToString();
        AgentType = "Analysis";
    }

    /// <summary>
    /// Determines if this agent can handle analytical tasks
    /// </summary>
    public async Task<bool> CanHandleAsync(AgentContext context)
    {
        try
        {
            var isHealthy = await _llmProvider.IsHealthyAsync();
            if (!isHealthy || string.IsNullOrEmpty(context.Input))
                return false;

            // Check if task contains analytical keywords
            var analyticalKeywords = new[] { 
                "analyze", "analysis", "insights", "data", "trends", "patterns", 
                "metrics", "performance", "evaluate", "assess", "compare", "statistics",
                "breakdown", "examine", "investigate", "review", "study" 
            };

            return analyticalKeywords.Any(keyword => 
                context.Input.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking if AnalysisAgent can handle context: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Executes analytical thinking and data analysis
    /// </summary>
    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"AnalysisAgent {AgentId} executing analytical task: {context.Input}");

        try
        {
            // Create specialized analytical prompt
            var analyticalContext = new AgentContext
            {
                Input = CreateAnalyticalPrompt(context.Input),
                ContextId = context.ContextId,
                UserId = context.UserId,
                SessionId = context.SessionId,
                Metadata = context.Metadata
            };

            var result = await _llmProvider.GenerateAgentResponseAsync(analyticalContext, cancellationToken);
            
            // Add analysis-specific metadata
            if (result.IsSuccessful)
            {
                result.Metadata["agentType"] = AgentType;
                result.Metadata["analysisTimestamp"] = DateTime.UtcNow;
                result.Metadata["specialization"] = "DataAnalysis";
            }

            Console.WriteLine($"AnalysisAgent {AgentId} completed analytical task successfully");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AnalysisAgent {AgentId} failed to execute task: {context.Input} - {ex.Message}");
            return AgentResult.CreateFailure($"Analysis execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a specialized prompt for analytical thinking
    /// </summary>
    private string CreateAnalyticalPrompt(string originalInput)
    {
        return $"""
            You are an expert Data Analyst and Business Intelligence specialist with deep analytical thinking capabilities.
            
            TASK: {originalInput}
            
            ANALYTICAL APPROACH:
            1. Break down the problem systematically
            2. Identify key metrics, patterns, and relationships
            3. Provide data-driven insights and conclusions
            4. Use structured thinking and logical reasoning
            5. Include specific examples and actionable recommendations
            
            RESPONSE FORMAT:
            - Executive Summary
            - Key Findings (use bullet points)
            - Data Analysis (include relevant metrics/comparisons)
            - Insights & Implications
            - Recommendations
            
            Focus on:
            ✓ Quantitative analysis where possible
            ✓ Trend identification and pattern recognition
            ✓ Risk assessment and opportunity identification
            ✓ Clear, actionable insights
            ✓ Evidence-based conclusions
            """;
    }

    /// <summary>
    /// Legacy execution method
    /// </summary>
    public async Task ExecuteAsync(string prompt, string agentType)
    {
        var context = new AgentContext { Input = prompt };
        var result = await ExecuteAsync(context);
        Console.WriteLine($"AnalysisAgent legacy execution completed with result: {result.IsSuccessful}");
    }

    /// <summary>
    /// Executes with default cancellation token
    /// </summary>
    public async Task<AgentResult> ExecuteAsync(AgentContext context)
    {
        return await ExecuteAsync(context, CancellationToken.None);
    }

    /// <summary>
    /// Gets the current status of the analysis agent
    /// </summary>
    public async Task<string> GetAgentStatusAsync()
    {
        try
        {
            var isHealthy = await _llmProvider.IsHealthyAsync();
            var status = isHealthy ? "Ready for Analysis" : "LLM Provider Unavailable";
            return $"AnalysisAgent {AgentId}: {status}";
        }
        catch (Exception ex)
        {
            return $"AnalysisAgent {AgentId}: Error - {ex.Message}";
        }
    }
} 