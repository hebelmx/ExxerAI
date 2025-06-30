using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Infrastructure.Services;

namespace ExxerAI.CLI;

/// <summary>
/// Test class to demonstrate specialized agent selection and capabilities
/// </summary>
public static class SpecializedAgentTest
{
    public static async Task RunSpecializationTestAsync()
    {
        Console.WriteLine("🎯 ExxerAI Specialized Agent Test - Agent Selection Demo\n");

        var ollamaProvider = new OllamaProvider();

        // Create specialized agents
        var agents = new List<IAgent>
        {
            new AnalysisAgent(ollamaProvider),
            new WritingAgent(ollamaProvider),
            new ResearchAgent(ollamaProvider),
            new GeneralPurposeAgent(ollamaProvider)
        };

        // Test scenarios for different agent types
        var testScenarios = new[]
        {
            new { Task = "Analyze the market trends for AI startups in 2024", ExpectedAgent = "Analysis" },
            new { Task = "Write a technical documentation for API endpoints", ExpectedAgent = "Writing" },
            new { Task = "Research the latest developments in quantum computing", ExpectedAgent = "Research" },
            new { Task = "Help me understand machine learning basics", ExpectedAgent = "GeneralPurpose" }
        };

        Console.WriteLine($"🤖 Created {agents.Count} specialized agents:");
        foreach (var agent in agents)
        {
            Console.WriteLine($"   • {agent.AgentType}Agent ({agent.AgentId[..8]}...)");
        }

        Console.WriteLine("\n" + "=".PadRight(80, '='));

        foreach (var scenario in testScenarios)
        {
            Console.WriteLine($"\n🎯 Task: {scenario.Task}");
            Console.WriteLine($"🎪 Expected Agent: {scenario.ExpectedAgent}");

            // Find the best agent for the task
            IAgent? selectedAgent = null;
            var context = new AgentContext { Input = scenario.Task };

            foreach (var agent in agents)
            {
                var canHandle = await agent.CanHandleAsync(context);
                if (canHandle)
                {
                    selectedAgent = agent;
                    break;
                }
            }

            if (selectedAgent != null)
            {
                Console.WriteLine($"✅ Selected: {selectedAgent.AgentType}Agent");
                Console.WriteLine($"🧠 Agent is processing...");

                // Execute the task
                var result = await selectedAgent.ExecuteAsync(context);

                if (result.IsSuccessful)
                {
                    Console.WriteLine($"✅ Success! Response length: {result.Output.Length} characters");
                    Console.WriteLine($"📊 Metadata: {string.Join(", ", result.Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
                    
                    // Show first 200 characters of response
                    var preview = result.Output.Length > 200 
                        ? result.Output[..200] + "..." 
                        : result.Output;
                    Console.WriteLine($"📋 Preview: {preview}");
                }
                else
                {
                    Console.WriteLine($"❌ Failed: {result.ErrorMessage}");
                }
            }
            else
            {
                Console.WriteLine("❌ No agent could handle this task");
            }

            Console.WriteLine("\n" + "-".PadRight(80, '-'));
        }

        Console.WriteLine("\n🏁 Specialized Agent Test completed. Press any key to exit...");
        Console.ReadKey();
    }
} 