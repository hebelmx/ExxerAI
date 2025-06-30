<<<<<<< HEAD
﻿// See https://aka.ms/new-console-template for more information

/// <summary>
/// Main entry point for the ExxerAI Command Line Interface application
/// </summary>
/// <remarks>
/// This is the primary entry point for the CLI application. Currently displays a simple greeting.
/// Future implementations will include command-line argument parsing and agent interaction capabilities.
/// </remarks>
Console.WriteLine("Hello, World!");
=======
﻿using ExxerAI.Application.Interfaces;
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
>>>>>>> d2192c11873934ee65fe539f76fb68767fa498f1
