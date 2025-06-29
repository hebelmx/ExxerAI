using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace ExxerAI.CLI;

/// <summary>
/// Simple test class to verify Ollama connection and agent functionality
/// </summary>
public static class TestAgent
{
    public static async Task RunBasicTestAsync()
    {
        Console.WriteLine("🤖 ExxerAI Agent Test - Connecting to Local Ollama...\n");

        // Create logger
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<GeneralPurposeAgent>();

        try
        {
            // Create Ollama provider
            var ollamaProvider = new OllamaProvider();
            Console.WriteLine($"✅ Created {ollamaProvider.ProviderName} provider");

            // Test provider health
            Console.WriteLine("🔍 Checking Ollama health...");
            var isHealthy = await ollamaProvider.IsHealthyAsync();
            Console.WriteLine($"📊 Ollama Health: {(isHealthy ? "✅ Healthy" : "❌ Unhealthy")}");

            if (!isHealthy)
            {
                Console.WriteLine("❌ Ollama is not responding. Make sure Docker container is running.");
                return;
            }

            // Create agent
            var agent = new GeneralPurposeAgent(logger, ollamaProvider);
            Console.WriteLine($"🤖 Created agent: {agent.AgentId} ({agent.AgentType})");

            // Test agent status
            var status = await agent.GetAgentStatusAsync();
            Console.WriteLine($"📋 Agent Status: {status}");

            // Create test context
            var context = new AgentContext
            {
                Input = "Analyze the benefits of autonomous AI agent systems and provide 3 key advantages. Focus on practical applications and business value.",
                UserId = "TestUser",
                SessionId = Guid.NewGuid().ToString()
            };

            Console.WriteLine($"\n🎯 Testing agent with task: {context.Input}");

            // Test if agent can handle the task
            var canHandle = await agent.CanHandleAsync(context);
            Console.WriteLine($"🤔 Can handle task: {(canHandle ? "✅ Yes" : "❌ No")}");

            if (canHandle)
            {
                Console.WriteLine("\n🧠 Agent is thinking...");
                
                // Execute the task
                var result = await agent.ExecuteAsync(context);
                
                Console.WriteLine($"\n📋 Execution Result:");
                Console.WriteLine($"   Success: {(result.IsSuccessful ? "✅" : "❌")}");
                Console.WriteLine($"   Response: {result.Output}");
                
                if (result.Metadata.Any())
                {
                    Console.WriteLine("   Metadata:");
                    foreach (var kvp in result.Metadata)
                    {
                        Console.WriteLine($"     {kvp.Key}: {kvp.Value}");
                    }
                }

                if (!result.IsSuccessful)
                {
                    Console.WriteLine($"   Error: {result.ErrorMessage}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test failed: {ex.Message}");
            Console.WriteLine($"🔧 Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine("\n🏁 Test completed. Press any key to exit...");
        Console.ReadKey();
    }
} 