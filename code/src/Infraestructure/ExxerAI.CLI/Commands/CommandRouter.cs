using ExxerAI.Domain.Operations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExxerAI.CLI.Commands;

/// <summary>
/// Routes CLI commands to appropriate handlers
/// </summary>
public class CommandRouter
{
    private readonly AgentCommands _agentCommands;
    private readonly TaskCommands _taskCommands;
    private readonly WorkflowCommands _workflowCommands;

    /// <summary>
    /// Initializes a new instance of the CommandRouter class
    /// </summary>
    /// <param name="agentCommands">Agent commands handler</param>
    /// <param name="taskCommands">Task commands handler</param>
    /// <param name="workflowCommands">Workflow commands handler</param>
    public CommandRouter(AgentCommands agentCommands, TaskCommands taskCommands, WorkflowCommands workflowCommands)
    {
        _agentCommands = agentCommands;
        _taskCommands = taskCommands;
        _workflowCommands = workflowCommands;
    }

    /// <summary>
    /// Validates constructor parameters and returns validation result
    /// </summary>
    /// <param name="agentCommands">Agent commands handler</param>
    /// <param name="taskCommands">Task commands handler</param>
    /// <param name="workflowCommands">Workflow commands handler</param>
    /// <returns>Validation result indicating success or failure with parameter names</returns>
    public static Result ValidateConstructorParameters(AgentCommands agentCommands, TaskCommands taskCommands, WorkflowCommands workflowCommands)
    {
        return ResultExtensions.ValidateNotNull(
            (agentCommands, nameof(agentCommands)),
            (taskCommands, nameof(taskCommands)),
            (workflowCommands, nameof(workflowCommands))
        );
    }

    /// <summary>
    /// Executes the appropriate command based on command line arguments
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Exit code</returns>
    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return 0;
        }

        var command = args[0].ToLowerInvariant();
        var commandArgs = args.Skip(1).ToArray();

        try
        {
            return command switch
            {
                "agent" or "agents" => await _agentCommands.ExecuteAsync(commandArgs, cancellationToken).ConfigureAwait(false),
                "task" or "tasks" => await _taskCommands.ExecuteAsync(commandArgs, cancellationToken).ConfigureAwait(false),
                "workflow" or "workflows" => await _workflowCommands.ExecuteAsync(commandArgs, cancellationToken).ConfigureAwait(false),
                "help" or "--help" or "-h" => ShowHelp(),
                "version" or "--version" or "-v" => ShowVersion(),
                _ => ShowUnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing command '{command}': {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Shows help information
    /// </summary>
    /// <returns>Exit code</returns>
    private static int ShowHelp()
    {
        Console.WriteLine("ExxerAI CLI - Autonomous AI Agent Management System");
        Console.WriteLine();
        Console.WriteLine("Usage: exxerai <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  agent    Manage AI agents (list, create, delete, agentStatus)");
        Console.WriteLine("  task     Manage tasks (list, create, assign, update)");
        Console.WriteLine("  workflow Manage workflows (list, create, execute)");
        Console.WriteLine("  help     Show this help information");
        Console.WriteLine("  version  Show version information");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  exxerai agent list");
        Console.WriteLine("  exxerai agent create \"Value Processor\" --description \"Processes data\"");
        Console.WriteLine("  exxerai task list --agentStatus pending");
        Console.WriteLine("  exxerai task assign 123 456");
        Console.WriteLine();
        Console.WriteLine("Use 'exxerai <command> --help' for more information about a command.");
        return 0;
    }

    /// <summary>
    /// Shows version information
    /// </summary>
    /// <returns>Exit code</returns>
    private static int ShowVersion()
    {
        Console.WriteLine("ExxerAI CLI v1.0.0");
        Console.WriteLine("Autonomous AI Agent Management System");
        return 0;
    }

    /// <summary>
    /// Shows unknown command error
    /// </summary>
    /// <param name="command">Unknown command</param>
    /// <returns>Exit code</returns>
    private static int ShowUnknownCommand(string command)
    {
        Console.WriteLine($"Unknown command: {command}");
        Console.WriteLine("Use 'exxerai help' to see available commands.");
        return 1;
    }
}