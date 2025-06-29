using ExxerAI.CLI;

// ===============================================================================
// ExxerAI Command Line Interface - Autonomous Agent Testing
// ===============================================================================
// Purpose: Test and demonstrate autonomous AI agent capabilities
// Dependencies: Local Ollama instance, SemanticKernel, ExxerAI services
// ===============================================================================

Console.WriteLine("🚀 ExxerAI - Autonomous Agent System");
Console.WriteLine("====================================\n");

try 
{
    // Run the basic agent test
    await TestAgent.RunBasicTestAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Application failed: {ex.Message}");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}