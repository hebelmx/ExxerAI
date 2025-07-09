using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.CLI.Commands;

/// <summary>
/// Handles task-related CLI commands
/// </summary>
public class TaskCommands
{
    private readonly ITaskRepository _taskRepository;
    private readonly IRepository<Agent> _agentRepository;

    /// <summary>
    /// Initializes a new instance of the TaskCommands class
    /// </summary>
    /// <param name="taskRepository">Task repository for operations</param>
    /// <param name="agentRepository">Agent repository for lookups</param>
    public TaskCommands(ITaskRepository taskRepository, IRepository<Agent> agentRepository)
    {
        _taskRepository = taskRepository;
        _agentRepository = agentRepository;
    }

    /// <summary>
    /// Validates constructor parameters and returns validation result
    /// </summary>
    /// <param name="taskRepository">Task repository for operations</param>
    /// <param name="agentRepository">Agent repository for lookups</param>
    /// <returns>Validation result indicating success or failure with parameter names</returns>
    public static Result ValidateConstructorParameters(ITaskRepository taskRepository, IRepository<Agent> agentRepository)
    {
        return ResultExtensions.ValidateNotNull(
            (taskRepository, nameof(taskRepository)),
            (agentRepository, nameof(agentRepository))
        );
    }

    /// <summary>
    /// Executes task commands based on provided arguments
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            ShowTaskHelp();
            return 0;
        }

        var subCommand = args[0].ToLowerInvariant();
        var commandArgs = args.Skip(1).ToArray();

        return subCommand switch
        {
            "list" or "ls" => await ListTasksAsync(commandArgs),
            "create" or "new" => await CreateTaskAsync(commandArgs),
            "assign" => await AssignTaskAsync(commandArgs),
            "update" => await UpdateTaskStatusAsync(commandArgs),
            "delete" or "remove" or "rm" => await DeleteTaskAsync(commandArgs),
            "agentstatus" or "info" => await ShowTaskStatusAsync(commandArgs),
            "overdue" => await ListOverdueTasksAsync(),
            "help" or "--help" or "-h" => ShowTaskHelp(),
            _ => ShowUnknownTaskCommand(subCommand)
        };
    }

    /// <summary>
    /// Lists tasks with optional filtering
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> ListTasksAsync(string[] args)
    {
        try
        {
            var result = await _taskRepository.GetAllAsync();
            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error retrieving tasks: {string.Join(", ", result.Errors)}");
                return 1;
            }

            var tasks = result.Value ?? Enumerable.Empty<AgentTask>();

            // Parse filtering options
            string? statusFilter = null;
            string? priorityFilter = null;
            string? typeFilter = null;
            string? agentFilter = null;

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--agentStatus" || args[i] == "-s")
                    statusFilter = args[i + 1];
                else if (args[i] == "--priority" || args[i] == "-p")
                    priorityFilter = args[i + 1];
                else if (args[i] == "--type" || args[i] == "-t")
                    typeFilter = args[i + 1];
                else if (args[i] == "--agent" || args[i] == "-a")
                    agentFilter = args[i + 1];
            }

            // Apply filters
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse<TaskAgentStatus>(statusFilter, true, out var status))
                    tasks = tasks.Where(t => t.AgentStatus == status);
                else
                {
                    Console.WriteLine($"Invalid agentStatus filter: {statusFilter}");
                    return 1;
                }
            }

            if (!string.IsNullOrEmpty(priorityFilter))
            {
                if (Enum.TryParse<TaskPriority>(priorityFilter, true, out var priority))
                    tasks = tasks.Where(t => t.Priority == priority);
                else
                {
                    Console.WriteLine($"Invalid priority filter: {priorityFilter}");
                    return 1;
                }
            }

            if (!string.IsNullOrEmpty(typeFilter))
            {
                tasks = tasks.Where(t => t.TaskType.Equals(typeFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(agentFilter))
            {
                if (Guid.TryParse(agentFilter, out var agentId))
                    tasks = tasks.Where(t => t.AssignedAgentId == agentId);
                else
                {
                    Console.WriteLine($"Invalid agent ID format: {agentFilter}");
                    return 1;
                }
            }

            var taskList = tasks.ToList();

            if (!taskList.Any())
            {
                Console.WriteLine("No tasks found.");
                return 0;
            }

            Console.WriteLine("Tasks:");
            Console.WriteLine($"{"ID",-10} {"Title",-30} {"Type",-15} {"AgentStatus",-12} {"Priority",-8} {"Agent",-25} {"Deadline",-12}");
            Console.WriteLine(new string('-', 125));

            foreach (var task in taskList)
            {
                var agentName = "Unassigned";
                if (task.AssignedAgentId.HasValue)
                {
                    var agentResult = await _agentRepository.GetByIdAsync(task.AssignedAgentId.Value);
                    if (agentResult.IsSuccess && agentResult.Value != null)
                        agentName = agentResult.Value.Name;
                }

                var deadlineStr = task.Deadline?.ToString("yyyy-MM-dd") ?? "None";

                Console.WriteLine($"{task.Id.ToString()[..8],-10} {task.Title,-30} {task.TaskType,-15} {task.AgentStatus,-12} {task.Priority,-8} {agentName,-25} {deadlineStr,-12}");
            }

            Console.WriteLine($"\nTotal: {taskList.Count} task(s)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing tasks: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> CreateTaskAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: Task title is required.");
            Console.WriteLine("Usage: exxerai task create <title> --type <type> [--priority <priority>] [--description <description>] [--deadline <date>]");
            return 1;
        }

        try
        {
            var title = args[0];
            string? typeStr = null;
            string? priorityStr = null;
            string? description = null;
            string? deadlineStr = null;

            // Parse options
            for (int i = 1; i < args.Length - 1; i++)
            {
                if (args[i] == "--type" || args[i] == "-t")
                    typeStr = args[i + 1];
                else if (args[i] == "--priority" || args[i] == "-p")
                    priorityStr = args[i + 1];
                else if (args[i] == "--description" || args[i] == "-d")
                    description = args[i + 1];
                else if (args[i] == "--deadline")
                    deadlineStr = args[i + 1];
            }

            if (string.IsNullOrEmpty(typeStr))
            {
                Console.WriteLine("Error: Task type is required.");
                Console.WriteLine("Example types: DataProcessing, Analysis, Coordination, Communication");
                return 1;
            }

            var priority = TaskPriority.Normal;
            if (!string.IsNullOrEmpty(priorityStr))
            {
                if (!Enum.TryParse<TaskPriority>(priorityStr, true, out priority))
                {
                    Console.WriteLine($"Error: Invalid priority '{priorityStr}'.");
                    Console.WriteLine("Valid priorities: Low, Normal, High, Critical");
                    return 1;
                }
            }

            DateTime? deadline = null;
            if (!string.IsNullOrEmpty(deadlineStr))
            {
                if (!DateTime.TryParse(deadlineStr, out var deadlineValue))
                {
                    Console.WriteLine($"Error: Invalid deadline format '{deadlineStr}'. Use format: YYYY-MM-DD");
                    return 1;
                }
                deadline = deadlineValue;
            }

            var task = new AgentTask
            {
                Title = title,
                TaskType = typeStr,
                Priority = priority,
                Description = description ?? $"Auto-generated {typeStr} task",
                AgentStatus = TaskAgentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Deadline = deadline
            };

            var result = await _taskRepository.AddAsync(task);
            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error creating task: {string.Join(", ", result.Errors)}");
                return 1;
            }

            Console.WriteLine($"Task '{title}' created successfully with ID: {result.Value?.Id}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating task: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Assigns a task to an agent
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> AssignTaskAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Error: Task ID and agent ID are required.");
            Console.WriteLine("Usage: exxerai task assign <task-id> <agent-id>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var taskId))
        {
            Console.WriteLine($"Error: Invalid task ID format '{args[0]}'.");
            return 1;
        }

        if (!Guid.TryParse(args[1], out var agentId))
        {
            Console.WriteLine($"Error: Invalid agent ID format '{args[1]}'.");
            return 1;
        }

        try
        {
            // Verify agent exists
            var agentResult = await _agentRepository.GetByIdAsync(agentId);
            if (!agentResult.IsSuccess || agentResult.Value == null)
            {
                Console.WriteLine($"Error: Agent {agentId} not found.");
                return 1;
            }

            // Get task
            var taskResult = await _taskRepository.GetByIdAsync(taskId);
            if (!taskResult.IsSuccess || taskResult.Value == null)
            {
                Console.WriteLine($"Error: Task {taskId} not found.");
                return 1;
            }

            var task = taskResult.Value;
            task.AssignedAgentId = agentId;
            task.AgentStatus = TaskAgentStatus.InProgress;
            task.StartedAt = DateTime.UtcNow;

            var updateResult = await _taskRepository.UpdateAsync(task);
            if (!updateResult.IsSuccess)
            {
                Console.WriteLine($"Error assigning task: {string.Join(", ", updateResult.Errors)}");
                return 1;
            }

            Console.WriteLine($"Task {taskId} assigned to agent {agentResult.Value.Name} ({agentId}) successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error assigning task: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Updates task agentStatus
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> UpdateTaskStatusAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Error: Task ID and agentStatus are required.");
            Console.WriteLine("Usage: exxerai task update <task-id> --agentStatus <agentStatus>");
            Console.WriteLine("Valid statuses: Pending, InProgress, Completed, Failed, Cancelled, Paused");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var taskId))
        {
            Console.WriteLine($"Error: Invalid task ID format '{args[0]}'.");
            return 1;
        }

        string? statusStr = null;
        for (int i = 1; i < args.Length - 1; i++)
        {
            if (args[i] == "--agentStatus" || args[i] == "-s")
                statusStr = args[i + 1];
        }

        if (string.IsNullOrEmpty(statusStr))
        {
            Console.WriteLine("Error: AgentStatus is required.");
            Console.WriteLine("Valid statuses: Pending, InProgress, Completed, Failed, Cancelled, Paused");
            return 1;
        }

        if (!Enum.TryParse<TaskAgentStatus>(statusStr, true, out var newStatus))
        {
            Console.WriteLine($"Error: Invalid agentStatus '{statusStr}'.");
            Console.WriteLine("Valid statuses: Pending, InProgress, Completed, Failed, Cancelled, Paused");
            return 1;
        }

        try
        {
            var taskResult = await _taskRepository.GetByIdAsync(taskId);
            if (!taskResult.IsSuccess || taskResult.Value == null)
            {
                Console.WriteLine($"Error: Task {taskId} not found.");
                return 1;
            }

            var task = taskResult.Value;
            task.AgentStatus = newStatus;

            // Update timestamps based on agentStatus
            if (newStatus == TaskAgentStatus.InProgress && !task.StartedAt.HasValue)
                task.StartedAt = DateTime.UtcNow;
            else if (newStatus == TaskAgentStatus.Completed)
                task.CompletedAt = DateTime.UtcNow;

            var updateResult = await _taskRepository.UpdateAsync(task);
            if (!updateResult.IsSuccess)
            {
                Console.WriteLine($"Error updating task agentStatus: {string.Join(", ", updateResult.Errors)}");
                return 1;
            }

            Console.WriteLine($"Task {taskId} agentStatus updated to {newStatus} successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating task agentStatus: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Deletes a task
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> DeleteTaskAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: Task ID is required.");
            Console.WriteLine("Usage: exxerai task delete <task-id>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var taskId))
        {
            Console.WriteLine($"Error: Invalid task ID format '{args[0]}'.");
            return 1;
        }

        try
        {
            var result = await _taskRepository.DeleteAsync(taskId);
            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error deleting task: {string.Join(", ", result.Errors)}");
                return 1;
            }

            Console.WriteLine($"Task {taskId} deleted successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting task: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Shows detailed task agentStatus information
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <returns>Exit code</returns>
    private async Task<int> ShowTaskStatusAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: Task ID is required.");
            Console.WriteLine("Usage: exxerai task agentStatus <task-id>");
            return 1;
        }

        if (!Guid.TryParse(args[0], out var taskId))
        {
            Console.WriteLine($"Error: Invalid task ID format '{args[0]}'.");
            return 1;
        }

        try
        {
            var result = await _taskRepository.GetByIdAsync(taskId);
            if (!result.IsSuccess || result.Value == null)
            {
                Console.WriteLine($"Error retrieving task: {string.Join(", ", result.Errors)}");
                return 1;
            }

            var task = result.Value;

            Console.WriteLine($"Task Details:");
            Console.WriteLine($"  ID:          {task.Id}");
            Console.WriteLine($"  Title:       {task.Title}");
            Console.WriteLine($"  Type:        {task.TaskType}");
            Console.WriteLine($"  AgentStatus:      {task.AgentStatus}");
            Console.WriteLine($"  Priority:    {task.Priority}");
            Console.WriteLine($"  Description: {task.Description ?? "None"}");
            Console.WriteLine($"  Created:     {task.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"  Started:     {(task.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Not started")}");
            Console.WriteLine($"  Completed:   {(task.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Not completed")}");
            Console.WriteLine($"  Deadline:    {(task.Deadline?.ToString("yyyy-MM-dd HH:mm:ss") ?? "None")}");
            Console.WriteLine($"  Retry Count: {task.RetryCount}");

            if (task.ExecutionDuration.HasValue)
                Console.WriteLine($"  Duration:    {task.ExecutionDuration.Value.TotalMinutes:F2} minutes");

            if (task.IsOverdue)
                Console.WriteLine($"  ⚠️  OVERDUE");

            if (task.AssignedAgentId.HasValue)
            {
                var agentResult = await _agentRepository.GetByIdAsync(task.AssignedAgentId.Value);
                if (agentResult.IsSuccess && agentResult.Value != null)
                {
                    Console.WriteLine($"  Assigned to: {agentResult.Value.Name} ({task.AssignedAgentId})");
                }
                else
                {
                    Console.WriteLine($"  Assigned to: Unknown Agent ({task.AssignedAgentId})");
                }
            }
            else
            {
                Console.WriteLine($"  Assigned to: Unassigned");
            }

            if (!string.IsNullOrEmpty(task.ErrorMessage))
                Console.WriteLine($"  Error:       {task.ErrorMessage}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing task agentStatus: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Lists overdue tasks
    /// </summary>
    /// <returns>Exit code</returns>
    private async Task<int> ListOverdueTasksAsync()
    {
        try
        {
            var result = await _taskRepository.GetOverdueTasksAsync();
            if (!result.IsSuccess)
            {
                Console.WriteLine($"Error retrieving overdue tasks: {string.Join(", ", result.Errors)}");
                return 1;
            }

            var tasks = (result.Value ?? Enumerable.Empty<AgentTask>()).ToList();

            if (!tasks.Any())
            {
                Console.WriteLine("No overdue tasks found.");
                return 0;
            }

            Console.WriteLine("Overdue Tasks:");
            Console.WriteLine($"{"ID",-10} {"Title",-30} {"Priority",-8} {"Days Overdue",-12} {"Agent",-25}");
            Console.WriteLine(new string('-', 95));

            foreach (var task in tasks)
            {
                var daysOverdue = task.Deadline.HasValue ? (DateTime.UtcNow - task.Deadline.Value).Days : 0;

                var agentName = "Unassigned";
                if (task.AssignedAgentId.HasValue)
                {
                    var agentResult = await _agentRepository.GetByIdAsync(task.AssignedAgentId.Value);
                    if (agentResult.IsSuccess && agentResult.Value != null)
                        agentName = agentResult.Value.Name;
                }

                Console.WriteLine($"{task.Id.ToString()[..8],-10} {task.Title,-30} {task.Priority,-8} {daysOverdue,-12} {agentName,-25}");
            }

            Console.WriteLine($"\nTotal: {tasks.Count} overdue task(s)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing overdue tasks: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Shows task command help
    /// </summary>
    /// <returns>Exit code</returns>
    private static int ShowTaskHelp()
    {
        Console.WriteLine("Task Commands:");
        Console.WriteLine();
        Console.WriteLine("Usage: exxerai task <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  list      List all tasks");
        Console.WriteLine("  create    Create a new task");
        Console.WriteLine("  assign    Assign a task to an agent");
        Console.WriteLine("  update    Update task agentStatus");
        Console.WriteLine("  delete    Delete a task");
        Console.WriteLine("  agentStatus    Show detailed task information");
        Console.WriteLine("  overdue   List overdue tasks");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  exxerai task list");
        Console.WriteLine("  exxerai task list --agentStatus pending");
        Console.WriteLine("  exxerai task create \"Process Value\" --type DataProcessing --priority High");
        Console.WriteLine("  exxerai task assign 12345678-1234-1234-1234-123456789012 87654321-4321-4321-4321-210987654321");
        Console.WriteLine("  exxerai task update 12345678-1234-1234-1234-123456789012 --agentStatus Completed");
        Console.WriteLine();
        return 0;
    }

    /// <summary>
    /// Shows unknown task command error
    /// </summary>
    /// <param name="command">Unknown command</param>
    /// <returns>Exit code</returns>
    private static int ShowUnknownTaskCommand(string command)
    {
        Console.WriteLine($"Unknown task command: {command}");
        Console.WriteLine("Use 'exxerai task help' to see available commands.");
        return 1;
    }
}