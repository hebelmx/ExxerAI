using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Services;

/// <summary>
/// Specialized agent for content creation, documentation, and writing tasks
/// </summary>
public class WritingAgent : IAgent
{
    private readonly ILLMProvider _llmProvider;

    public string AgentId { get; }
    public string AgentType { get; }

    public WritingAgent(
        ILLMProvider llmProvider,
        string? agentId = null)
    {
        _llmProvider = llmProvider;
        AgentId = agentId ?? Guid.NewGuid().ToString();
        AgentType = "Writing";
    }

    /// <summary>
    /// Determines if this agent can handle writing and content creation tasks
    /// </summary>
    public async Task<bool> CanHandleAsync(AgentContext context)
    {
        try
        {
            var isHealthy = await _llmProvider.IsHealthyAsync();
            if (!isHealthy || string.IsNullOrEmpty(context.Input))
                return false;

            // Check if task contains writing-related keywords
            var writingKeywords = new[] { 
                "write", "create", "draft", "compose", "document", "content", 
                "article", "blog", "report", "summary", "description", "proposal",
                "plan", "guide", "manual", "story", "copy", "email", "letter",
                "presentation", "slides", "outline", "script", "narrative" 
            };

            return writingKeywords.Any(keyword => 
                context.Input.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking if WritingAgent can handle context: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Executes content creation and writing tasks
    /// </summary>
    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"WritingAgent {AgentId} executing writing task: {context.Input}");

        try
        {
            // Create specialized writing prompt
            var writingContext = new AgentContext
            {
                Input = CreateWritingPrompt(context.Input),
                ContextId = context.ContextId,
                UserId = context.UserId,
                SessionId = context.SessionId,
                Metadata = context.Metadata
            };

            var result = await _llmProvider.GenerateAgentResponseAsync(writingContext, cancellationToken);
            
            // Add writing-specific metadata
            if (result.IsSuccessful)
            {
                result.Metadata["agentType"] = AgentType;
                result.Metadata["writingTimestamp"] = DateTime.UtcNow;
                result.Metadata["specialization"] = "ContentCreation";
                result.Metadata["estimatedWordCount"] = EstimateWordCount(result.Output);
            }

            Console.WriteLine($"WritingAgent {AgentId} completed writing task successfully");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WritingAgent {AgentId} failed to execute task: {context.Input}");
            return AgentResult.CreateFailure($"Writing execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a specialized prompt for content creation
    /// </summary>
    private string CreateWritingPrompt(string originalInput)
    {
        return $"""
            You are an expert Content Creator and Technical Writer with exceptional writing skills across multiple formats and styles.
            
            WRITING TASK: {originalInput}
            
            WRITING APPROACH:
            1. Understand the target audience and purpose
            2. Create clear, engaging, and well-structured content
            3. Use appropriate tone and style for the context
            4. Ensure proper organization and flow
            5. Include compelling headlines and clear sections
            
            CONTENT STANDARDS:
            ✓ Clear and concise language
            ✓ Logical structure and flow
            ✓ Engaging and professional tone
            ✓ Proper formatting and organization
            ✓ Error-free grammar and spelling
            ✓ Actionable and valuable content
            
            FORMATTING GUIDELINES:
            - Use headers and subheaders for organization
            - Include bullet points for lists
            - Bold important concepts
            - Create scannable, well-structured content
            - Add clear call-to-actions where appropriate
            
            Focus on creating content that is:
            ★ Professionally written and polished
            ★ Appropriate for the intended audience
            ★ Clear and easy to understand
            ★ Engaging and valuable to readers
            ★ Well-organized and structured
            """;
    }

    /// <summary>
    /// Estimates word count of generated content
    /// </summary>
    private int EstimateWordCount(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return 0;
            
        return content.Split(new[] { ' ', '\t', '\n', '\r' }, 
                           StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Legacy execution method
    /// </summary>
    public async Task ExecuteAsync(string prompt, string agentType)
    {
        var context = new AgentContext { Input = prompt };
        var result = await ExecuteAsync(context);
        Console.WriteLine($"WritingAgent legacy execution completed with result: {result.IsSuccessful}");
    }

    /// <summary>
    /// Executes with default cancellation token
    /// </summary>
    public async Task<AgentResult> ExecuteAsync(AgentContext context)
    {
        return await ExecuteAsync(context, CancellationToken.None);
    }

    /// <summary>
    /// Gets the current status of the writing agent
    /// </summary>
    public async Task<string> GetAgentStatusAsync()
    {
        try
        {
            var isHealthy = await _llmProvider.IsHealthyAsync();
            var status = isHealthy ? "Ready for Writing" : "LLM Provider Unavailable";
            return $"WritingAgent {AgentId}: {status}";
        }
        catch (Exception ex)
        {
            return $"WritingAgent {AgentId}: Error - {ex.Message}";
        }
    }
} 