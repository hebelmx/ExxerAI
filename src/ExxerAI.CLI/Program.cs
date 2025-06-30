using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Infrastructure.Repositories;
using ExxerAI.CLI.Commands;

namespace ExxerAI.CLI;

/// <summary>
/// Entry point for the ExxerAI CLI application
/// </summary>
internal class Program
{
	/// <summary>
	/// Main entry point for the CLI application
	/// </summary>
	/// <param name="args">Command line arguments</param>
	/// <returns>Exit code</returns>
	private static async Task<int> Main(string[] args)
	{
		try
		{
			// Create repositories
			var agentRepository = new InMemoryAgentRepository();
			var taskRepository = new InMemoryTaskRepository();
			
			// Create services (AgentService needs both repositories)
			var agentService = new AgentService(agentRepository, taskRepository);
			
			// Create commands
			var agentCommands = new AgentCommands(agentService, agentRepository);
			var taskCommands = new TaskCommands(taskRepository, agentRepository);
			var workflowCommands = new WorkflowCommands();
			
			// Create command router
			var commandRouter = new CommandRouter(agentCommands, taskCommands, workflowCommands);
			
			return await commandRouter.ExecuteAsync(args);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
			return 1;
		}
	}
}
