using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Orchestrates multiple specialized agents to handle complex multi-step tasks
/// </summary>
public class AgentOrchestrator : IAgentOrchestrator
{
    private readonly ILLMProvider _llmProvider;
    private readonly List<IAgent> _availableAgents;

    public AgentOrchestrator(
        ILLMProvider llmProvider,
        IEnumerable<IAgent> agents)
    {
        _llmProvider = llmProvider;
        _availableAgents = agents.ToList();
    }

    /// <summary>
    /// Executes a complex task by orchestrating multiple agents
    /// </summary>
    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"AgentOrchestrator starting task orchestration: {context.Input}");

        try
        {
            // Step 1: Analyze task complexity and determine if orchestration is needed
            var taskAnalysis = await AnalyzeTaskComplexityAsync(context, cancellationToken);
            
            if (taskAnalysis.RequiresOrchestration)
            {
                Console.WriteLine($"Complex task detected, breaking into subtasks: {taskAnalysis.Subtasks.Count}");
                return await ExecuteOrchestrationAsync(context, taskAnalysis, cancellationToken);
            }
            else
            {
                Console.WriteLine("Simple task detected, delegating to single agent");
                return await DelegateToSingleAgentAsync(context, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AgentOrchestrator failed to execute task: {context.Input} - {ex.Message}");
            return AgentResult.CreateFailure($"Orchestration failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Analyzes task complexity to determine orchestration strategy
    /// </summary>
    private async Task<TaskAnalysis> AnalyzeTaskComplexityAsync(AgentContext context, CancellationToken cancellationToken)
    {
        var analysisPrompt = $@"Analyze this task and determine if it requires multiple specialized agents:
            
TASK: {context.Input}

Respond with JSON in this format:
{{
    ""requiresOrchestration"": true/false,
    ""complexity"": ""simple/moderate/complex"", 
    ""subtasks"": [
        {{""step"": 1, ""description"": ""Research market data"", ""agentType"": ""Research""}},
        {{""step"": 2, ""description"": ""Analyze trends"", ""agentType"": ""Analysis""}},
        {{""step"": 3, ""description"": ""Create report"", ""agentType"": ""Writing""}}
    ],
    ""reasoning"": ""Explanation of why orchestration is/isn't needed""
}}

Consider orchestration for tasks that:
- Need research + analysis + writing
- Require multiple data sources
- Have distinct sequential steps
- Benefit from specialized expertise";

        var analysisContext = new AgentContext 
        { 
            Input = analysisPrompt,
            ContextId = context.ContextId,
            UserId = context.UserId,
            SessionId = context.SessionId
        };

        var result = await _llmProvider.GenerateAgentResponseAsync(analysisContext, cancellationToken);
        
        if (!result.IsSuccessful)
        {
            return new TaskAnalysis { RequiresOrchestration = false, Complexity = "simple" };
        }

        try
        {
            return ParseTaskAnalysis(result.Output);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse task analysis, defaulting to single agent: {ex.Message}");
            return new TaskAnalysis { RequiresOrchestration = false, Complexity = "simple" };
        }
    }

    /// <summary>
    /// Executes orchestrated task with multiple agents
    /// </summary>
    private async Task<AgentResult> ExecuteOrchestrationAsync(AgentContext context, TaskAnalysis analysis, CancellationToken cancellationToken)
    {
        var orchestrationResults = new List<AgentResult>();
        var accumulatedOutput = new List<string>();

        Console.WriteLine($"Executing {analysis.Subtasks.Count} orchestrated subtasks");

        foreach (var subtask in analysis.Subtasks)
        {
            Console.WriteLine($"Executing subtask {subtask.Step}: {subtask.Description} using {subtask.AgentType}");

            // Find appropriate agent for this subtask
            var agent = FindAgentByType(subtask.AgentType);
            if (agent == null)
            {
                Console.WriteLine($"No agent found for type {subtask.AgentType}, using fallback");
                agent = _availableAgents.FirstOrDefault();
            }

            if (agent == null)
            {
                return AgentResult.CreateFailure($"No agents available for subtask: {subtask.Description}");
            }

            // Create context for subtask with previous results
            var subtaskContext = new AgentContext
            {
                Input = CreateSubtaskPrompt(subtask, context.Input, accumulatedOutput),
                ContextId = context.ContextId,
                UserId = context.UserId,
                SessionId = context.SessionId,
                Metadata = context.Metadata
            };

            var subtaskResult = await agent.ExecuteAsync(subtaskContext, cancellationToken);
            orchestrationResults.Add(subtaskResult);

            if (subtaskResult.IsSuccessful)
            {
                accumulatedOutput.Add($"**{subtask.AgentType} Result ({subtask.Description}):**\n{subtaskResult.Output}\n");
                Console.WriteLine($"Subtask {subtask.Step} completed successfully");
            }
            else
            {
                Console.WriteLine($"Subtask {subtask.Step} failed: {subtaskResult.ErrorMessage}");
                return AgentResult.CreateFailure($"Orchestration failed at step {subtask.Step}: {subtaskResult.ErrorMessage}");
            }
        }

        // Combine all results
        var finalOutput = string.Join("\n" + "=".PadRight(80, '=') + "\n", accumulatedOutput);
        
        var result = AgentResult.CreateSuccess(finalOutput);
        
        // Add orchestration metadata
        result.Metadata["orchestrationType"] = "multi-agent";
        result.Metadata["totalSubtasks"] = analysis.Subtasks.Count;
        result.Metadata["complexity"] = analysis.Complexity;
        result.Metadata["completedAt"] = DateTime.UtcNow;
        result.Metadata["agentsUsed"] = string.Join(", ", orchestrationResults.Select(r => r.Metadata.GetValueOrDefault("agentType", "Unknown")));

        Console.WriteLine($"Orchestration completed successfully with {analysis.Subtasks.Count} subtasks");
        return result;
    }

    /// <summary>
    /// Delegates simple tasks to a single appropriate agent
    /// </summary>
    private async Task<AgentResult> DelegateToSingleAgentAsync(AgentContext context, CancellationToken cancellationToken)
    {
        foreach (var agent in _availableAgents)
        {
            var canHandle = await agent.CanHandleAsync(context);
            if (canHandle)
            {
                Console.WriteLine($"Delegating to {agent.AgentType}Agent");
                var result = await agent.ExecuteAsync(context, cancellationToken);
                result.Metadata["orchestrationType"] = "single-agent";
                return result;
            }
        }

        return AgentResult.CreateFailure("No agent available to handle this task");
    }

    /// <summary>
    /// Creates prompts for subtasks with context from previous results
    /// </summary>
    private string CreateSubtaskPrompt(Subtask subtask, string originalTask, List<string> previousResults)
    {
        var contextInfo = previousResults.Any() 
            ? $"\n\nPREVIOUS WORK:\n{string.Join("\n", previousResults)}"
            : "";

        return $@"ORIGINAL TASK: {originalTask}
            
YOUR SPECIFIC SUBTASK: {subtask.Description}
STEP: {subtask.Step}

{contextInfo}

Focus specifically on your assigned subtask while being aware of the overall objective.
Build upon previous work when relevant.";
    }

    /// <summary>
    /// Finds agent by type name
    /// </summary>
    private IAgent? FindAgentByType(string agentType)
    {
        return _availableAgents.FirstOrDefault(a => 
            a.AgentType.Equals(agentType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Parses task analysis JSON response
    /// </summary>
    private TaskAnalysis ParseTaskAnalysis(string jsonResponse)
    {
        // Simple JSON parsing for task analysis
        // In production, you'd use System.Text.Json or Newtonsoft.Json
        var requiresOrch = jsonResponse.Contains("\"requiresOrchestration\": true", StringComparison.OrdinalIgnoreCase);
        
        var subtasks = new List<Subtask>();
        if (requiresOrch)
        {
            // Extract subtasks from JSON response
            // This is a simplified implementation
            if (jsonResponse.Contains("Research"))
                subtasks.Add(new Subtask { Step = 1, Description = "Research information", AgentType = "Research" });
            if (jsonResponse.Contains("Analysis"))
                subtasks.Add(new Subtask { Step = 2, Description = "Analyze data", AgentType = "Analysis" });
            if (jsonResponse.Contains("Writing"))
                subtasks.Add(new Subtask { Step = 3, Description = "Create content", AgentType = "Writing" });
        }

        return new TaskAnalysis
        {
            RequiresOrchestration = requiresOrch,
            Complexity = jsonResponse.Contains("complex") ? "complex" : "moderate",
            Subtasks = subtasks
        };
    }

    /// <summary>
    /// Legacy implementation for backward compatibility
    /// </summary>
    public async Task ExecuteAgentAsync(string prompt, string agentType)
    {
        var context = new AgentContext { Input = prompt };
        var result = await ExecuteAsync(context);
        Console.WriteLine($"Legacy orchestration completed with result: {result.IsSuccessful}");
    }

    /// <summary>
    /// Gets status of all available agents
    /// </summary>
    public async Task<string> GetAgentStatusAsync()
    {
        var statusTasks = _availableAgents.Select(async agent =>
        {
            try
            {
                return await agent.GetAgentStatusAsync();
            }
            catch (Exception ex)
            {
                return $"{agent.AgentType}Agent: Error - {ex.Message}";
            }
        });

        var statuses = await Task.WhenAll(statusTasks);
        return $"AgentOrchestrator with {_availableAgents.Count} agents:\n" + string.Join("\n", statuses);
    }
}

/// <summary>
/// Analysis result for task complexity
/// </summary>
public class TaskAnalysis
{
    public bool RequiresOrchestration { get; set; }
    public string Complexity { get; set; } = "simple";
    public List<Subtask> Subtasks { get; set; } = new();
}

/// <summary>
/// Represents a subtask in an orchestrated workflow
/// </summary>
public class Subtask
{
    public int Step { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AgentType { get; set; } = string.Empty;
}