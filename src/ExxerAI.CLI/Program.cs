using ExxerAI.CLI;

// ===============================================================================
// ExxerAI Command Line Interface - Multi-Agent Orchestration Testing
// ===============================================================================
// Purpose: Test and demonstrate multi-agent task orchestration and coordination
// Dependencies: Local Ollama instance, SemanticKernel, ExxerAI services
// ===============================================================================

Console.WriteLine("🚀 ExxerAI - Autonomous Agent System");
Console.WriteLine("====================================\n");

try 
{
    // Run the multi-agent orchestration test
    await OrchestrationTest.RunOrchestrationTestAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Application failed: {ex.Message}");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}