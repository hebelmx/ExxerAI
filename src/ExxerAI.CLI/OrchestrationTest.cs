using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Infrastructure.Services;

namespace ExxerAI.CLI;

/// <summary>
/// Test class to demonstrate multi-agent task orchestration capabilities
/// </summary>
public static class OrchestrationTest
{
    public static async Task RunOrchestrationTestAsync()
    {
        Console.WriteLine("🎪 ExxerAI Task Orchestration Test - Multi-Agent Coordination\n");

        var ollamaProvider = new OllamaProvider();

        // Create specialized agents
        var agents = new List<IAgent>
        {
            new AnalysisAgent(ollamaProvider),
            new WritingAgent(ollamaProvider),
            new ResearchAgent(ollamaProvider),
            new GeneralPurposeAgent(ollamaProvider)
        };

        // Create orchestrator
        var orchestrator = new AgentOrchestrator(
            ollamaProvider,
            agents);

        Console.WriteLine($"🎯 Created orchestrator with {agents.Count} specialized agents");
        Console.WriteLine("🔗 Testing multi-agent task coordination...\n");

        // Test complex tasks that should trigger orchestration
        var orchestrationTests = new[]
        {
            new
            {
                Name = "Market Research Report",
                Task = "Create a comprehensive market research report on electric vehicle adoption trends, including analysis of consumer behavior data and recommendations for automotive manufacturers",
                Expected = "Research → Analysis → Writing"
            },
            new
            {
                Name = "Competitive Analysis",
                Task = "Research our competitors in the AI agent space, analyze their strengths and weaknesses, and write a strategic positioning document",
                Expected = "Research → Analysis → Writing"
            },
            new
            {
                Name = "Simple Query",
                Task = "What is machine learning?",
                Expected = "Single Agent"
            }
        };

        foreach (var test in orchestrationTests)
        {
            Console.WriteLine($"🎪 TEST: {test.Name}");
            Console.WriteLine($"📋 Task: {test.Task}");
            Console.WriteLine($"🎯 Expected: {test.Expected}");
            Console.WriteLine(new string('=', 100));

            var context = new AgentContext 
            { 
                Input = test.Task,
                ContextId = Guid.NewGuid().ToString(),
                UserId = "test-user",
                SessionId = Guid.NewGuid().ToString()
            };

            var startTime = DateTime.UtcNow;

            try
            {
                var result = await orchestrator.ExecuteAsync(context);

                var duration = DateTime.UtcNow - startTime;
                
                if (result.IsSuccessful)
                {
                    Console.WriteLine($"✅ SUCCESS! Completed in {duration.TotalSeconds:F1} seconds");
                    Console.WriteLine($"📊 Orchestration Type: {result.Metadata.GetValueOrDefault("orchestrationType", "unknown")}");
                    
                    if (result.Metadata.ContainsKey("totalSubtasks"))
                    {
                        Console.WriteLine($"🔢 Total Subtasks: {result.Metadata["totalSubtasks"]}");
                        Console.WriteLine($"🤖 Agents Used: {result.Metadata.GetValueOrDefault("agentsUsed", "unknown")}");
                        Console.WriteLine($"⚡ Complexity: {result.Metadata.GetValueOrDefault("complexity", "unknown")}");
                    }

                    Console.WriteLine($"📄 Output Length: {result.Output.Length} characters");
                    
                    // Show structured preview of orchestrated results
                    if (result.Output.Contains("=".PadRight(80, '=')))
                    {
                        Console.WriteLine("\n📝 ORCHESTRATED RESULTS PREVIEW:");
                        var sections = result.Output.Split("=".PadRight(80, '='), StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < Math.Min(3, sections.Length); i++)
                        {
                            var preview = sections[i].Trim();
                            if (preview.Length > 200)
                                preview = preview[..200] + "...";
                            Console.WriteLine($"   Section {i + 1}: {preview}");
                        }
                    }
                    else
                    {
                        // Single agent result
                        var preview = result.Output.Length > 300 ? result.Output[..300] + "..." : result.Output;
                        Console.WriteLine($"📋 Preview: {preview}");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ FAILED: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXCEPTION: {ex.Message}");
            }

            Console.WriteLine(new string('-', 100));
            Console.WriteLine("Continuing to next test...");
            Console.WriteLine();
        }

        // Test orchestrator status
        Console.WriteLine("🔍 ORCHESTRATOR STATUS:");
        var status = await orchestrator.GetAgentStatusAsync();
        Console.WriteLine(status);

        Console.WriteLine("\n🏁 Multi-Agent Orchestration Test completed successfully!");
    }
} 