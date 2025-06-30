using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Services;

/// <summary>
/// Specialized agent for research, information gathering, and fact-checking
/// </summary>
public class ResearchAgent : IAgent
{
    private readonly ILLMProvider _llmProvider;

    public string AgentId { get; }
    public string AgentType { get; }

    public ResearchAgent(
        ILLMProvider llmProvider,
        string? agentId = null)
    {
        _llmProvider = llmProvider;
        AgentId = agentId ?? Guid.NewGuid().ToString();
        AgentType = "Research";
    }

    /// <summary>
    /// Determines if this agent can handle research and information gathering tasks
    /// </summary>
    public async Task<bool> CanHandleAsync(AgentContext context)
    {
        try
        {
            var isHealthy = await _llmProvider.IsHealthyAsync();
            if (!isHealthy || string.IsNullOrEmpty(context.Input))
                return false;

            // Check if task contains research-related keywords
            var researchKeywords = new[] { 
                "research", "investigate", "find", "gather", "information", "facts", 
                "study", "explore", "discover", "learn", "search", "lookup",
                "verify", "validate", "check", "confirm", "evidence", "sources",
                "background", "history", "trends", "market", "competitive" 
            };

            return researchKeywords.Any(keyword => 
                context.Input.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking if ResearchAgent can handle context: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Executes research and information gathering tasks
    /// </summary>
    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"ResearchAgent {AgentId} executing research task: {context.Input}");

        try
        {
            // Create specialized research prompt
            var researchContext = new AgentContext
            {
                Input = CreateResearchPrompt(context.Input),
                ContextId = context.ContextId,
                UserId = context.UserId,
                SessionId = context.SessionId,
                Metadata = context.Metadata
            };

            var result = await _llmProvider.GenerateAgentResponseAsync(researchContext, cancellationToken);
            
            // Add research-specific metadata
            if (result.IsSuccessful)
            {
                result.Metadata["agentType"] = AgentType;
                result.Metadata["researchTimestamp"] = DateTime.UtcNow;
                result.Metadata["specialization"] = "InformationGathering";
                result.Metadata["researchDepth"] = "comprehensive";
            }

            Console.WriteLine($"ResearchAgent {AgentId} completed research task successfully");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ResearchAgent {AgentId} failed to execute task: {context.Input}");
            return AgentResult.CreateFailure($"Research execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a specialized prompt for research and information gathering
    /// </summary>
    private string CreateResearchPrompt(string originalInput)
    {
        return $"""
            You are an expert Research Analyst and Information Specialist with deep knowledge across multiple domains and excellent fact-checking capabilities.
            
            RESEARCH REQUEST: {originalInput}
            
            RESEARCH METHODOLOGY:
            1. Approach the topic systematically and comprehensively
            2. Gather information from multiple perspectives
            3. Provide factual, well-sourced insights
            4. Identify key trends, patterns, and developments
            5. Validate information accuracy and reliability
            
            RESEARCH STRUCTURE:
            ▶ Executive Summary
            ▶ Key Findings & Facts
            ▶ Background Information
            ▶ Current State Analysis
            ▶ Trends & Developments
            ▶ Implications & Significance
            ▶ Additional Considerations
            
            RESEARCH STANDARDS:
            ✓ Comprehensive and thorough investigation
            ✓ Multiple perspectives and viewpoints
            ✓ Factual accuracy and reliability
            ✓ Current and up-to-date information
            ✓ Clear organization and presentation
            ✓ Identification of limitations and unknowns
            
            Focus on:
            ★ Providing comprehensive, accurate information
            ★ Identifying reliable facts and credible insights
            ★ Presenting multiple viewpoints objectively
            ★ Highlighting important trends and patterns
            ★ Noting areas requiring further investigation
            ★ Maintaining high standards of factual accuracy
            """;
    }

    /// <summary>
    /// Legacy execution method
    /// </summary>
    public async Task ExecuteAsync(string prompt, string agentType)
    {
        var context = new AgentContext { Input = prompt };
        var result = await ExecuteAsync(context);
        Console.WriteLine($"ResearchAgent legacy execution completed with result: {result.IsSuccessful}");
    }

    /// <summary>
    /// Executes with default cancellation token
    /// </summary>
    public async Task<AgentResult> ExecuteAsync(AgentContext context)
    {
        return await ExecuteAsync(context, CancellationToken.None);
    }

    /// <summary>
    /// Gets the current status of the research agent
    /// </summary>
    public async Task<string> GetAgentStatusAsync()
    {
        try
        {
            var isHealthy = await _llmProvider.IsHealthyAsync();
            var status = isHealthy ? "Ready for Research" : "LLM Provider Unavailable";
            return $"ResearchAgent {AgentId}: {status}";
        }
        catch (Exception ex)
        {
            return $"ResearchAgent {AgentId}: Error - {ex.Message}";
        }
    }
} 