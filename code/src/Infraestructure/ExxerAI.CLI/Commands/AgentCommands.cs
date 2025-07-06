using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.CLI.Commands;

/// <summary>
/// Handles agent-related CLI commands
/// </summary>
public class AgentCommands
{
    private readonly IAgentService _agentService;
    private readonly IRepository<Agent> _agentRepository;

    /// <summary>
    /// Initializes a new instance of the AgentCommands class
    /// </summary>
    /// <param name="agentService">Agent service for operations</param>
    /// <param name="agentRepository">Agent repository for direct access</param>
    public AgentCommands(IAgentService agentService, IRepository<Agent> agentRepository)
    {
        _agentService = agentService;
        _agentRepository = agentRepository;
    }

    /// <summary>
    /// Validates constructor parameters and returns validation result
    /// </summary>
    /// <param name="agentService">Agent service for operations</param>
    /// <param name="agentRepository">Agent repository for direct access</param>
    /// <returns>Validation result indicating success or failure with parameter names</returns>
    public static Result ValidateConstructorParameters(IAgentService agentService, IRepository<Agent> agentRepository)
    {
        return ResultExtensions.ValidateNotNull(
            (agentService, nameof(agentService)),
            (agentRepository, nameof(agentRepository))
        );
    }

    /// <summary>
    /// Executes agent commands based on provided arguments
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    public async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            ShowAgentHelp();
            return 0;
        }

        var subCommand = args[0].ToLowerInvariant();
        var commandArgs = args.Skip(1).ToArray();

        return subCommand switch
        {
            "list" or "ls" => await ListAgents(commandArgs),
            "create" or "new" => await CreateAgent(commandArgs),
            "delete" or "remove" or "rm" => await DeleteAgent(commandArgs),
            "agentstatus" or "info" => await ShowAgentStatus(commandArgs),
            "update" => await UpdateAgent(commandArgs),
            "activate" => await ActivateAgent(commandArgs),
            "deactivate" => await DeactivateAgent(commandArgs),
            "help" or "--help" or "-h" => ShowAgentHelp(),
            _ => ShowUnknownAgentCommand(subCommand)
        };
    }

    /// <summary>
    /// Lists all agents with optional filtering
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> ListAgents(string[] args)
    {
        try
        {
            var result = await _agentRepository.GetAllAsync();
            if (result.IsFailure)
            {
                Console.WriteLine($"Error retrieving agents: {result.Error}");
                return 1;
            }

            var agents = result.Data!;

            // Parse filtering options
            string? statusFilter = null;

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--agentStatus" || args[i] == "-s")
                    statusFilter = args[i + 1];
            }

            // Apply filters
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse<AgentStatus>(statusFilter, true, out var status))
                    agents = agents.Where(a => a.Status == status);
                else
                {
                    Console.WriteLine($"Invalid agentStatus filter: {statusFilter}");
                    return 1;
                }
            }

            var agentList = agents.ToList();

            if (!agentList.Any())
            {
                Console.WriteLine("No agents found.");
                return 0;
            }

            Console.WriteLine("Agents:");
            Console.WriteLine($"{"ID",-10} {"Name",-25} {"AgentStatus",-15} {"Created",-20}");
            Console.WriteLine(new string('-', 70));

            foreach (var agent in agentList)
            {
                Console.WriteLine($"{agent.Id.ToString()[..8],-10} {agent.Name,-25} {agent.Status,-15} {agent.CreatedAt:yyyy-MM-dd HH:mm}");
            }

            Console.WriteLine($"\nTotal: {agentList.Count} agent(s)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing agents: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Creates a new agent
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> CreateAgent(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: Agent name is required.");
            Console.WriteLine("Usage: exxerai agent create <name> [--description <description>]");
            return 1;
        }

        try
        {
            var name = args[0];
            string? description = null;

            // Parse options
            for (int i = 1; i < args.Length - 1; i++)
            {
                if (args[i] == "--description" || args[i] == "-d")
                    description = args[i + 1];
            }

            var capabilities = new AgentCapabilities
            {
                CanProcessNaturalLanguage = true,
                MaxConcurrentTasks = 1
            };

            var result = await _agentService.CreateAgentAsync(
                name,
                description ?? $"Auto-generated agent {name}",
                capabilities);

            if (result.IsFailure)
            {
                Console.WriteLine($"Error creating agent: {result.Error}");
                return 1;
            }

            var createdAgent = result.Data!;
            Console.WriteLine($"Agent '{name}' created successfully with ID: {createdAgent.Id}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating agent: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Deletes an agent
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> DeleteAgent(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: Agent ID is required.");
            Console.WriteLine("Usage: exxerai agent delete <id>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var agentId))
        {
            Console.WriteLine($"Error: Invalid agent ID format '{args[0]}'.");
            return 1;
        }

        try
        {
            var result = await _agentRepository.DeleteAsync(agentId);
            if (result.IsFailure)
            {
                Console.WriteLine($"Error deleting agent: {result.Error}");
                return 1;
            }

            Console.WriteLine($"Agent {agentId} deleted successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting agent: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Shows detailed agent agentStatus information
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> ShowAgentStatus(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: Agent ID is required.");
            Console.WriteLine("Usage: exxerai agent agentStatus <id>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var agentId))
        {
            Console.WriteLine($"Error: Invalid agent ID format '{args[0]}'.");
            return 1;
        }

        try
        {
            var result = await _agentRepository.GetByIdAsync(agentId);
            if (result.IsFailure)
            {
                Console.WriteLine($"Error retrieving agent: {result.Error}");
                return 1;
            }

            var agent = result.Data!;
            if (agent == null)
            {
                Console.WriteLine($"Agent {agentId} not found.");
                return 1;
            }

            Console.WriteLine($"Agent Details:");
            Console.WriteLine($"  ID:          {agent.Id}");
            Console.WriteLine($"  Name:        {agent.Name}");
            Console.WriteLine($"  AgentStatus:      {agent.Status}");
            Console.WriteLine($"  Description: {agent.Description ?? "None"}");
            Console.WriteLine($"  Created:     {agent.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"  Updated:     {agent.UpdatedAt:yyyy-MM-dd HH:mm:ss} UTC");

            // Display capabilities
            var caps = agent.Capabilities;
            Console.WriteLine($"  Capabilities:");
            Console.WriteLine($"    Natural Language: {caps.CanProcessNaturalLanguage}");
            Console.WriteLine($"    Code Generation:  {caps.CanGenerateCode}");
            Console.WriteLine($"    Value Analysis:    {caps.CanAnalyzeData}");
            Console.WriteLine($"    External APIs:    {caps.CanCallExternalAPIs}");
            Console.WriteLine($"    Max Concurrent:   {caps.MaxConcurrentTasks}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing agent agentStatus: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Updates agent properties
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> UpdateAgent(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: Agent ID is required.");
            Console.WriteLine("Usage: exxerai agent update <id> --name <name> [--description <description>]");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var agentId))
        {
            Console.WriteLine($"Error: Invalid agent ID format '{args[0]}'.");
            return 1;
        }

        try
        {
            var result = await _agentRepository.GetByIdAsync(agentId);
            if (result.IsFailure || result.Data == null)
            {
                Console.WriteLine($"Error retrieving agent: {result.Error}");
                return 1;
            }

            var agent = result.Data;

            // Parse update options
            for (int i = 1; i < args.Length - 1; i++)
            {
                if (args[i] == "--name" || args[i] == "-n")
                    agent.Name = args[i + 1];
                else if (args[i] == "--description" || args[i] == "-d")
                    agent.Description = args[i + 1];
            }

            agent.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _agentRepository.UpdateAsync(agent);
            if (updateResult.IsFailure)
            {
                Console.WriteLine($"Error updating agent: {updateResult.Error}");
                return 1;
            }

            Console.WriteLine($"Agent {agentId} updated successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating agent: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Activates an agent
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> ActivateAgent(string[] args)
    {
        return await ChangeAgentStatus(args, AgentStatus.Active, "activated");
    }

    /// <summary>
    /// Deactivates an agent
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> DeactivateAgent(string[] args)
    {
        return await ChangeAgentStatus(args, AgentStatus.Inactive, "deactivated");
    }

    /// <summary>
    /// Changes agent agentStatus
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <param name="newStatus">New agentStatus</param>
    /// <param name="action">Action description</param>
    /// <returns>Exit code</returns>
    private async Task<int> ChangeAgentStatus(string[] args, AgentStatus newStatus, string action)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: Agent ID is required.");
            Console.WriteLine($"Usage: exxerai agent {action.ToLower()} <id>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var agentId))
        {
            Console.WriteLine($"Error: Invalid agent ID format '{args[0]}'.");
            return 1;
        }

        try
        {
            var result = await _agentRepository.GetByIdAsync(agentId);
            if (result.IsFailure || result.Data == null)
            {
                Console.WriteLine($"Error retrieving agent: {result.Error}");
                return 1;
            }

            var agent = result.Data;
            agent.Status = newStatus;
            agent.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _agentRepository.UpdateAsync(agent);
            if (updateResult.IsFailure)
            {
                Console.WriteLine($"Error changing agent agentStatus: {updateResult.Error}");
                return 1;
            }

            Console.WriteLine($"Agent {agentId} {action} successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing agent agentStatus: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Shows agent command help
    /// </summary>
    /// <returns>Exit code</returns>
    private static int ShowAgentHelp()
    {
        Console.WriteLine("Agent Commands:");
        Console.WriteLine();
        Console.WriteLine("Usage: exxerai agent <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  list      List all agents");
        Console.WriteLine("  create    Create a new agent");
        Console.WriteLine("  delete    Delete an agent");
        Console.WriteLine("  agentStatus    Show detailed agent information");
        Console.WriteLine("  update    Update agent properties");
        Console.WriteLine("  activate  Activate an agent");
        Console.WriteLine("  deactivate Deactivate an agent");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  exxerai agent list");
        Console.WriteLine("  exxerai agent list --agentStatus active");
        Console.WriteLine("  exxerai agent create \"Value Processor\" --description \"Processes data files\"");
        Console.WriteLine("  exxerai agent agentStatus 12345678-1234-1234-1234-123456789012");
        Console.WriteLine("  exxerai agent update 12345678-1234-1234-1234-123456789012 --name \"New Name\"");
        Console.WriteLine();
        return 0;
    }

    /// <summary>
    /// Shows unknown agent command error
    /// </summary>
    /// <param name="command">Unknown command</param>
    /// <returns>Exit code</returns>
    private static int ShowUnknownAgentCommand(string command)
    {
        Console.WriteLine($"Unknown agent command: {command}");
        Console.WriteLine("Use 'exxerai agent help' to see available commands.");
        return 1;
    }
}