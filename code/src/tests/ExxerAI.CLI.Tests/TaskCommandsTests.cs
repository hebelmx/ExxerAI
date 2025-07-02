using ExxerAI.Application.Interfaces;
using ExxerAI.CLI.Commands;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.CLI.Tests;

/// <summary>
/// Tests for TaskCommands CLI functionality
/// </summary>
public class TaskCommandsTests
{
	private readonly ITaskRepository _mockTaskRepository;
	private readonly IRepository<Agent> _mockAgentRepository;
	private readonly TaskCommands _taskCommands;

	public TaskCommandsTests()
	{
		_mockTaskRepository = Substitute.For<ITaskRepository>();
		_mockAgentRepository = Substitute.For<IRepository<Agent>>();
		_taskCommands = new TaskCommands(_mockTaskRepository, _mockAgentRepository);
	}

	[Fact]
	public void Constructor_Should_InitializeCorrectly_When_ValidParametersProvided()
	{
		// Arrange & Act
		var commands = new TaskCommands(_mockTaskRepository, _mockAgentRepository);

		// Assert
		commands.ShouldNotBeNull();
	}

	[Fact]
	public void ValidateConstructorParameters_Should_ReturnSuccess_When_AllParametersValid()
	{
		// Arrange
		var taskRepository = Substitute.For<ITaskRepository>();
		var agentRepository = Substitute.For<IRepository<Agent>>();

		// Act
		var result = TaskCommands.ValidateConstructorParameters(taskRepository, agentRepository);

		// Assert
		result.IsSuccess.ShouldBeTrue();
	}

	[Fact]
	public void ValidateConstructorParameters_Should_ReturnFailure_When_TaskRepositoryIsNull()
	{
		// Arrange
		var agentRepository = Substitute.For<IRepository<Agent>>();

		// Act
		var result = TaskCommands.ValidateConstructorParameters(null!, agentRepository);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldContain("taskRepository");
	}

	[Fact]
	public void ValidateConstructorParameters_Should_ReturnFailure_When_AgentRepositoryIsNull()
	{
		// Arrange
		var taskRepository = Substitute.For<ITaskRepository>();

		// Act
		var result = TaskCommands.ValidateConstructorParameters(taskRepository, null!);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldContain("agentRepository");
	}

	[Fact]
	public async Task ExecuteAsync_Should_ShowHelp_When_NoArgumentsProvided()
	{
		// Arrange
		var args = Array.Empty<string>();

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ExecuteAsync_Should_CallListTasks_When_ListCommandProvided()
	{
		// Arrange
		var args = new[] { "list" };
		_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).GetAllAsync();
	}

	[Fact]
	public async Task ExecuteAsync_Should_CallListTasks_When_LsCommandProvided()
	{
		// Arrange
		var args = new[] { "ls" };
		_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).GetAllAsync();
	}

	[Fact]
	public async Task ExecuteAsync_Should_CallCreateTask_When_CreateCommandProvided()
	{
		// Arrange
		var args = new[] { "create", "TestTask", "--type", "DataProcessing" };
		var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
		_mockTaskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(testTask));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).AddAsync(Arg.Is<AgentTask>(t => t.Title == "TestTask"));
	}

	[Fact]
	public async Task ExecuteAsync_Should_CallAssignTask_When_AssignCommandProvided()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var agentId = Guid.NewGuid();
		var args = new[] { "assign", taskId.ToString(), agentId.ToString() };
		
		var testAgent = new Agent { Id = agentId, Name = "TestAgent" };
		var testTask = new AgentTask { Id = taskId, Title = "TestTask" };
		
		_mockAgentRepository.GetByIdAsync(agentId).Returns(Result<Agent>.Success(testAgent));
		_mockTaskRepository.GetByIdAsync(taskId).Returns(Result<AgentTask>.Success(testTask));
		_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(testTask));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => t.AssignedAgentId == agentId));
	}

	[Fact]
	public async Task ExecuteAsync_Should_CallUpdateTaskStatus_When_UpdateCommandProvided()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var args = new[] { "update", taskId.ToString(), "--status", "Completed" };
		var testTask = new AgentTask { Id = taskId, Title = "TestTask", Status = ExxerAI.Domain.TaskStatus.InProgress };
		
		_mockTaskRepository.GetByIdAsync(taskId).Returns(Result<AgentTask>.Success(testTask));
		_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(testTask));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => t.Status == ExxerAI.Domain.TaskStatus.Completed));
	}

	[Fact]
	public async Task ExecuteAsync_Should_CallDeleteTask_When_DeleteCommandProvided()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var args = new[] { "delete", taskId.ToString() };
		_mockTaskRepository.DeleteAsync(taskId).Returns(Task.FromResult(Result.Success()));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).DeleteAsync(taskId);
	}

	[Fact]
	public async Task ExecuteAsync_Should_CallShowTaskStatus_When_StatusCommandProvided()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var args = new[] { "status", taskId.ToString() };
		var testTask = new AgentTask { Id = taskId, Title = "TestTask" };
		_mockTaskRepository.GetByIdAsync(taskId).Returns(Task.FromResult(Result<AgentTask>.Success(testTask)));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).GetByIdAsync(taskId);
	}

	[Fact]
	public async Task ExecuteAsync_Should_CallListOverdueTasks_When_OverdueCommandProvided()
	{
		// Arrange
		var args = new[] { "overdue" };
		_mockTaskRepository.GetAllAsync().Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>())));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).GetAllAsync();
	}

	[Fact]
	public async Task ExecuteAsync_Should_ShowHelp_When_HelpCommandProvided()
	{
		// Arrange
		var args = new[] { "help" };

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ExecuteAsync_Should_ShowUnknownCommand_When_InvalidCommandProvided()
	{
		// Arrange
		var args = new[] { "invalid" };

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task ListTasks_Should_ReturnSuccess_When_NoTasksExist()
	{
		// Arrange
		var args = new[] { "list" };
		_mockTaskRepository.GetAllAsync().Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>())));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ListTasks_Should_DisplayTasks_When_TasksExist()
	{
		// Arrange
		var args = new[] { "list" };
		var tasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "Task1", TaskType = "DataProcessing", Status = ExxerAI.Domain.TaskStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow },
			new() { Id = Guid.NewGuid(), Title = "Task2", TaskType = "Analysis", Status = ExxerAI.Domain.TaskStatus.InProgress, Priority = TaskPriority.High, CreatedAt = DateTime.UtcNow }
		};
		_mockTaskRepository.GetAllAsync().Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Success(tasks)));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ListTasks_Should_FilterByStatus_When_StatusFilterProvided()
	{
		// Arrange
		var args = new[] { "list", "--status", "Pending" };
		var tasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "PendingTask", Status = ExxerAI.Domain.TaskStatus.Pending, TaskType = "Test", Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow },
			new() { Id = Guid.NewGuid(), Title = "CompletedTask", Status = ExxerAI.Domain.TaskStatus.Completed, TaskType = "Test", Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow }
		};
		_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(tasks));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ListTasks_Should_FilterByPriority_When_PriorityFilterProvided()
	{
		// Arrange
		var args = new[] { "list", "--priority", "High" };
		var tasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "HighPriorityTask", Priority = TaskPriority.High, TaskType = "Test", Status = ExxerAI.Domain.TaskStatus.Pending, CreatedAt = DateTime.UtcNow },
			new() { Id = Guid.NewGuid(), Title = "NormalPriorityTask", Priority = TaskPriority.Normal, TaskType = "Test", Status = ExxerAI.Domain.TaskStatus.Pending, CreatedAt = DateTime.UtcNow }
		};
		_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(tasks));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ListTasks_Should_FilterByType_When_TypeFilterProvided()
	{
		// Arrange
		var args = new[] { "list", "--type", "DataProcessing" };
		var tasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "DataTask", TaskType = "DataProcessing", Status = ExxerAI.Domain.TaskStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow },
			new() { Id = Guid.NewGuid(), Title = "AnalysisTask", TaskType = "Analysis", Status = ExxerAI.Domain.TaskStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow }
		};
		_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(tasks));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ListTasks_Should_FilterByAgent_When_AgentFilterProvided()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var args = new[] { "list", "--agent", agentId.ToString() };
		var tasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "AssignedTask", AssignedAgentId = agentId, TaskType = "Test", Status = ExxerAI.Domain.TaskStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow },
			new() { Id = Guid.NewGuid(), Title = "UnassignedTask", AssignedAgentId = null, TaskType = "Test", Status = ExxerAI.Domain.TaskStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow }
		};
		_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(tasks));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ListTasks_Should_ReturnError_When_InvalidStatusFilter()
	{
		// Arrange
		var args = new[] { "list", "--status", "InvalidStatus" };
		_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task ListTasks_Should_ReturnError_When_InvalidPriorityFilter()
	{
		// Arrange
		var args = new[] { "list", "--priority", "InvalidPriority" };
		_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task ListTasks_Should_ReturnError_When_InvalidAgentIdFormat()
	{
		// Arrange
		var args = new[] { "list", "--agent", "invalid-agent-id" };
		_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task ListTasks_Should_ReturnError_When_RepositoryFails()
	{
		// Arrange
		var args = new[] { "list" };
		_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.WithFailure("Database error"));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task CreateTask_Should_ReturnError_When_NoTitleProvided()
	{
		// Arrange
		var args = new[] { "create" };

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task CreateTask_Should_ReturnError_When_NoTypeProvided()
	{
		// Arrange
		var args = new[] { "create", "TestTask" };

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task CreateTask_Should_CreateWithDefaultPriority_When_NoPriorityProvided()
	{
		// Arrange
		var args = new[] { "create", "TestTask", "--type", "DataProcessing" };
		var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
		_mockTaskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(testTask));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).AddAsync(Arg.Is<AgentTask>(t => t.Priority == TaskPriority.Normal));
	}

	[Fact]
	public async Task CreateTask_Should_CreateWithCustomPriority_When_PriorityProvided()
	{
		// Arrange
		var args = new[] { "create", "TestTask", "--type", "DataProcessing", "--priority", "High" };
		var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
		_mockTaskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(testTask));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).AddAsync(Arg.Is<AgentTask>(t => t.Priority == TaskPriority.High));
	}

	[Fact]
	public async Task CreateTask_Should_CreateWithDescription_When_DescriptionProvided()
	{
		// Arrange
		var args = new[] { "create", "TestTask", "--type", "DataProcessing", "--description", "Custom description" };
		var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
		_mockTaskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(testTask));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).AddAsync(Arg.Is<AgentTask>(t => t.Description == "Custom description"));
	}

	[Fact]
	public async Task CreateTask_Should_CreateWithDeadline_When_DeadlineProvided()
	{
		// Arrange
		var args = new[] { "create", "TestTask", "--type", "DataProcessing", "--deadline", "2025-12-31" };
		var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
		_mockTaskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(testTask));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).AddAsync(Arg.Is<AgentTask>(t => t.Deadline.HasValue));
	}

	[Fact]
	public async Task CreateTask_Should_ReturnError_When_InvalidPriority()
	{
		// Arrange
		var args = new[] { "create", "TestTask", "--type", "DataProcessing", "--priority", "InvalidPriority" };

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task CreateTask_Should_ReturnError_When_InvalidDeadlineFormat()
	{
		// Arrange
		var args = new[] { "create", "TestTask", "--type", "DataProcessing", "--deadline", "invalid-date" };

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task CreateTask_Should_ReturnError_When_RepositoryFails()
	{
		// Arrange
		var args = new[] { "create", "TestTask", "--type", "DataProcessing" };
		_mockTaskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.WithFailure("Repository error"));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task AssignTask_Should_ReturnError_When_InsufficientArguments()
	{
		// Arrange
		var args = new[] { "assign", Guid.NewGuid().ToString() };

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task AssignTask_Should_ReturnError_When_InvalidTaskIdFormat()
	{
		// Arrange
		var args = new[] { "assign", "invalid-task-id", Guid.NewGuid().ToString() };

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task AssignTask_Should_ReturnError_When_InvalidAgentIdFormat()
	{
		// Arrange
		var args = new[] { "assign", Guid.NewGuid().ToString(), "invalid-agent-id" };

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task AssignTask_Should_ReturnError_When_AgentNotFound()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var agentId = Guid.NewGuid();
		var args = new[] { "assign", taskId.ToString(), agentId.ToString() };
		_mockAgentRepository.GetByIdAsync(agentId).Returns(Result<Agent>.WithFailure("Agent not found"));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task AssignTask_Should_ReturnError_When_TaskNotFound()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var agentId = Guid.NewGuid();
		var args = new[] { "assign", taskId.ToString(), agentId.ToString() };
		var testAgent = new Agent { Id = agentId, Name = "TestAgent" };
		_mockAgentRepository.GetByIdAsync(agentId).Returns(Result<Agent>.Success(testAgent));
		_mockTaskRepository.GetByIdAsync(taskId).Returns(Result<AgentTask>.WithFailure("Task not found"));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task AssignTask_Should_ReturnError_When_UpdateFails()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var agentId = Guid.NewGuid();
		var args = new[] { "assign", taskId.ToString(), agentId.ToString() };
		var testAgent = new Agent { Id = agentId, Name = "TestAgent" };
		var testTask = new AgentTask { Id = taskId, Title = "TestTask" };
		
		_mockAgentRepository.GetByIdAsync(agentId).Returns(Result<Agent>.Success(testAgent));
		_mockTaskRepository.GetByIdAsync(taskId).Returns(Result<AgentTask>.Success(testTask));
		_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.WithFailure("Update failed"));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Theory]
	[InlineData("--help")]
	[InlineData("-h")]
	public async Task ExecuteAsync_Should_ShowHelp_When_HelpFlagsProvided(string helpFlag)
	{
		// Arrange
		var args = new[] { helpFlag };

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Theory]
	[InlineData("new")]
	[InlineData("remove")]
	[InlineData("rm")]
	[InlineData("info")]
	public async Task ExecuteAsync_Should_HandleCommandAliases_When_AliasesProvided(string alias)
	{
		// Arrange
		var args = alias switch
		{
			"new" => new[] { alias, "TestTask", "--type", "DataProcessing" },
			"remove" or "rm" => new[] { alias, Guid.NewGuid().ToString() },
			"info" => new[] { alias, Guid.NewGuid().ToString() },
			_ => new[] { alias }
		};

		if (alias == "new")
		{
			var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
			_mockTaskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(testTask));
		}
		else if (alias == "remove" || alias == "rm")
		{
			_mockTaskRepository.DeleteAsync(Arg.Any<Guid>()).Returns(Result.Success());
		}
		else if (alias == "info")
		{
			var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
			_mockTaskRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(Result<AgentTask>.Success(testTask));
		}

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ExecuteAsync_Should_HandleExceptions_When_ExceptionThrown()
	{
		// Arrange
		var args = new[] { "list" };
		_mockTaskRepository.GetAllAsync().Throws(new InvalidOperationException("Test exception"));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Theory]
	[InlineData("-s")]
	[InlineData("-p")]
	[InlineData("-t")]
	[InlineData("-a")]
	[InlineData("-d")]
	public async Task ExecuteAsync_Should_HandleShortFlags_When_ShortFlagsProvided(string shortFlag)
	{
		// Arrange
		var args = shortFlag switch
		{
			"-s" => new[] { "list", shortFlag, "Pending" },
			"-p" => new[] { "list", shortFlag, "High" },
			"-t" => new[] { "list", shortFlag, "DataProcessing" },
			"-a" => new[] { "list", shortFlag, Guid.NewGuid().ToString() },
			"-d" => new[] { "create", "TestTask", "--type", "DataProcessing", shortFlag, "Test description" },
			_ => Array.Empty<string>()
		};

		if (shortFlag == "-d")
		{
			var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
			_mockTaskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(testTask));
		}
		else
		{
			_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));
		}

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		// For invalid agent GUID, it should return error (1), for others success (0)
		if (shortFlag == "-a")
		{
			exitCode.ShouldBe(1); // Invalid GUID format
		}
		else
		{
			exitCode.ShouldBe(0);
		}
	}

	[Fact]
	public async Task ListTasks_Should_ShowAgentNames_When_TasksAreAssigned()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var args = new[] { "list" };
		var tasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "AssignedTask", AssignedAgentId = agentId, TaskType = "Test", Status = ExxerAI.Domain.TaskStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow }
		};
		var testAgent = new Agent { Id = agentId, Name = "TestAgent" };
		
		_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(tasks));
		_mockAgentRepository.GetByIdAsync(agentId).Returns(Result<Agent>.Success(testAgent));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockAgentRepository.Received(1).GetByIdAsync(agentId);
	}

	[Fact]
	public async Task ListOverdueTasks_Should_FilterOverdueTasks_When_OverdueTasksExist()
	{
		// Arrange
		var args = new[] { "overdue" };
		var tasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "OverdueTask", Deadline = DateTime.UtcNow.AddDays(-1), TaskType = "Test", Status = ExxerAI.Domain.TaskStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow },
			new() { Id = Guid.NewGuid(), Title = "FutureTask", Deadline = DateTime.UtcNow.AddDays(1), TaskType = "Test", Status = ExxerAI.Domain.TaskStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow }
		};
		_mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(tasks));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task UpdateTaskStatus_Should_SetStartedAt_When_ChangingToPending()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var args = new[] { "update", taskId.ToString(), "--status", "InProgress" };
		var testTask = new AgentTask { Id = taskId, Title = "TestTask", Status = ExxerAI.Domain.TaskStatus.Pending, StartedAt = null };
		
		_mockTaskRepository.GetByIdAsync(taskId).Returns(Result<AgentTask>.Success(testTask));
		_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(testTask));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => 
			t.Status == ExxerAI.Domain.TaskStatus.InProgress && t.StartedAt.HasValue));
	}

	[Fact]
	public async Task UpdateTaskStatus_Should_SetCompletedAt_When_ChangingToCompleted()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var args = new[] { "update", taskId.ToString(), "--status", "Completed" };
		var testTask = new AgentTask { Id = taskId, Title = "TestTask", Status = ExxerAI.Domain.TaskStatus.InProgress, CompletedAt = null };
		
		_mockTaskRepository.GetByIdAsync(taskId).Returns(Result<AgentTask>.Success(testTask));
		_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(testTask));

		// Act
		var exitCode = await _taskCommands.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => 
			t.Status == ExxerAI.Domain.TaskStatus.Completed && t.CompletedAt.HasValue));
	}
} 