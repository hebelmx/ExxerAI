using ExxerAI.Application.Interfaces;
using ExxerAI.CLI.Commands;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.CLI.Tests;

/// <summary>
/// Tests for CommandRouter CLI functionality
/// </summary>
public class CommandRouterTests
{
	private readonly AgentCommands _mockAgentCommands;
	private readonly TaskCommands _mockTaskCommands;
	private readonly WorkflowCommands _mockWorkflowCommands;
	private readonly CommandRouter _commandRouter;

	public CommandRouterTests()
	{
		_mockAgentCommands = Substitute.For<AgentCommands>(
			Substitute.For<IAgentService>(),
			Substitute.For<IRepository<Agent>>());
		_mockTaskCommands = Substitute.For<TaskCommands>(
			Substitute.For<ITaskRepository>(),
			Substitute.For<IRepository<Agent>>());
		_mockWorkflowCommands = Substitute.For<WorkflowCommands>();
		
		_commandRouter = new CommandRouter(_mockAgentCommands, _mockTaskCommands, _mockWorkflowCommands);
	}

	[Fact]
	public void Constructor_Should_InitializeCorrectly_When_ValidParametersProvided()
	{
		// Arrange & Act
		var router = new CommandRouter(_mockAgentCommands, _mockTaskCommands, _mockWorkflowCommands);

		// Assert
		router.ShouldNotBeNull();
	}

	[Fact]
	public void ValidateConstructorParameters_Should_ReturnSuccess_When_AllParametersValid()
	{
		// Arrange
		var agentCommands = Substitute.For<AgentCommands>(
			Substitute.For<IAgentService>(),
			Substitute.For<IRepository<Agent>>());
		var taskCommands = Substitute.For<TaskCommands>(
			Substitute.For<ITaskRepository>(),
			Substitute.For<IRepository<Agent>>());
		var workflowCommands = Substitute.For<WorkflowCommands>();

		// Act
		var result = CommandRouter.ValidateConstructorParameters(agentCommands, taskCommands, workflowCommands);

		// Assert
		result.IsSuccess.ShouldBeTrue();
	}

	[Fact]
	public void ValidateConstructorParameters_Should_ReturnFailure_When_AgentCommandsIsNull()
	{
		// Arrange
		var taskCommands = Substitute.For<TaskCommands>(
			Substitute.For<ITaskRepository>(),
			Substitute.For<IRepository<Agent>>());
		var workflowCommands = Substitute.For<WorkflowCommands>();

		// Act
		var result = CommandRouter.ValidateConstructorParameters(null!, taskCommands, workflowCommands);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldContain("agentCommands");
	}

	[Fact]
	public void ValidateConstructorParameters_Should_ReturnFailure_When_TaskCommandsIsNull()
	{
		// Arrange
		var agentCommands = Substitute.For<AgentCommands>(
			Substitute.For<IAgentService>(),
			Substitute.For<IRepository<Agent>>());
		var workflowCommands = Substitute.For<WorkflowCommands>();

		// Act
		var result = CommandRouter.ValidateConstructorParameters(agentCommands, null!, workflowCommands);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldContain("taskCommands");
	}

	[Fact]
	public void ValidateConstructorParameters_Should_ReturnFailure_When_WorkflowCommandsIsNull()
	{
		// Arrange
		var agentCommands = Substitute.For<AgentCommands>(
			Substitute.For<IAgentService>(),
			Substitute.For<IRepository<Agent>>());
		var taskCommands = Substitute.For<TaskCommands>(
			Substitute.For<ITaskRepository>(),
			Substitute.For<IRepository<Agent>>());

		// Act
		var result = CommandRouter.ValidateConstructorParameters(agentCommands, taskCommands, null!);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldContain("workflowCommands");
	}

	[Fact]
	public async Task ExecuteAsync_Should_ShowHelp_When_NoArgumentsProvided()
	{
		// Arrange
		var args = Array.Empty<string>();

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ExecuteAsync_Should_RouteToAgentCommands_When_AgentCommandProvided()
	{
		// Arrange
		var args = new[] { "agent", "list" };
		_mockAgentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockAgentCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(x => x.SequenceEqual(new[] { "list" })));
	}

	[Fact]
	public async Task ExecuteAsync_Should_RouteToAgentCommands_When_AgentsCommandProvided()
	{
		// Arrange
		var args = new[] { "agents", "create", "TestAgent" };
		_mockAgentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockAgentCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(x => x.SequenceEqual(new[] { "create", "TestAgent" })));
	}

	[Fact]
	public async Task ExecuteAsync_Should_RouteToTaskCommands_When_TaskCommandProvided()
	{
		// Arrange
		var args = new[] { "task", "list" };
		_mockTaskCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(x => x.SequenceEqual(new[] { "list" })));
	}

	[Fact]
	public async Task ExecuteAsync_Should_RouteToTaskCommands_When_TasksCommandProvided()
	{
		// Arrange
		var args = new[] { "tasks", "create", "TestTask" };
		_mockTaskCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(x => x.SequenceEqual(new[] { "create", "TestTask" })));
	}

	[Fact]
	public async Task ExecuteAsync_Should_RouteToWorkflowCommands_When_WorkflowCommandProvided()
	{
		// Arrange
		var args = new[] { "workflow", "list" };
		_mockWorkflowCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockWorkflowCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(x => x.SequenceEqual(new[] { "list" })));
	}

	[Fact]
	public async Task ExecuteAsync_Should_RouteToWorkflowCommands_When_WorkflowsCommandProvided()
	{
		// Arrange
		var args = new[] { "workflows", "create", "TestWorkflow" };
		_mockWorkflowCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockWorkflowCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(x => x.SequenceEqual(new[] { "create", "TestWorkflow" })));
	}

	[Fact]
	public async Task ExecuteAsync_Should_ShowHelp_When_HelpCommandProvided()
	{
		// Arrange
		var args = new[] { "help" };

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ExecuteAsync_Should_ShowHelp_When_HelpFlagProvided()
	{
		// Arrange
		var args = new[] { "--help" };

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ExecuteAsync_Should_ShowHelp_When_ShortHelpFlagProvided()
	{
		// Arrange
		var args = new[] { "-h" };

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ExecuteAsync_Should_ShowVersion_When_VersionCommandProvided()
	{
		// Arrange
		var args = new[] { "version" };

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ExecuteAsync_Should_ShowVersion_When_VersionFlagProvided()
	{
		// Arrange
		var args = new[] { "--version" };

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ExecuteAsync_Should_ShowVersion_When_ShortVersionFlagProvided()
	{
		// Arrange
		var args = new[] { "-v" };

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ExecuteAsync_Should_ShowUnknownCommand_When_InvalidCommandProvided()
	{
		// Arrange
		var args = new[] { "invalid" };

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task ExecuteAsync_Should_HandleCaseInsensitiveCommands_When_UpperCaseProvided()
	{
		// Arrange
		var args = new[] { "AGENT", "list" };
		_mockAgentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockAgentCommands.Received(1).ExecuteAsync(Arg.Any<string[]>());
	}

	[Fact]
	public async Task ExecuteAsync_Should_HandleCaseInsensitiveCommands_When_MixedCaseProvided()
	{
		// Arrange
		var args = new[] { "TaSk", "status" };
		_mockTaskCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockTaskCommands.Received(1).ExecuteAsync(Arg.Any<string[]>());
	}

	[Fact]
	public async Task ExecuteAsync_Should_PropagateExitCode_When_CommandFails()
	{
		// Arrange
		var args = new[] { "agent", "list" };
		_mockAgentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(1);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task ExecuteAsync_Should_PropagateExitCode_When_CommandSucceeds()
	{
		// Arrange
		var args = new[] { "task", "create", "TestTask" };
		_mockTaskCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task ExecuteAsync_Should_HandleExceptions_When_CommandThrows()
	{
		// Arrange
		var args = new[] { "agent", "list" };
		_mockAgentCommands.ExecuteAsync(Arg.Any<string[]>()).Throws(new InvalidOperationException("Test exception"));

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task ExecuteAsync_Should_PassCorrectArguments_When_MultipleArgumentsProvided()
	{
		// Arrange
		var args = new[] { "agent", "create", "TestAgent", "--description", "Test description" };
		_mockAgentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockAgentCommands.Received(1).ExecuteAsync(
			Arg.Is<string[]>(x => x.SequenceEqual(new[] { "create", "TestAgent", "--description", "Test description" })));
	}

	[Fact]
	public async Task ExecuteAsync_Should_PassEmptyArguments_When_OnlyCommandProvided()
	{
		// Arrange
		var args = new[] { "workflow" };
		_mockWorkflowCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		await _mockWorkflowCommands.Received(1).ExecuteAsync(
			Arg.Is<string[]>(x => x.Length == 0));
	}

	[Theory]
	[InlineData("agent", "list")]
	[InlineData("agents", "create")]
	[InlineData("task", "status")]
	[InlineData("tasks", "update")]
	[InlineData("workflow", "execute")]
	[InlineData("workflows", "delete")]
	public async Task ExecuteAsync_Should_RouteCorrectly_When_CommandAliasesProvided(string command, string subCommand)
	{
		// Arrange
		var args = new[] { command, subCommand };
		_mockAgentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);
		_mockTaskCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);
		_mockWorkflowCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
		
		if (command.StartsWith("agent"))
		{
			await _mockAgentCommands.Received(1).ExecuteAsync(Arg.Any<string[]>());
		}
		else if (command.StartsWith("task"))
		{
			await _mockTaskCommands.Received(1).ExecuteAsync(Arg.Any<string[]>());
		}
		else if (command.StartsWith("workflow"))
		{
			await _mockWorkflowCommands.Received(1).ExecuteAsync(Arg.Any<string[]>());
		}
	}

	[Theory]
	[InlineData("help")]
	[InlineData("--help")]
	[InlineData("-h")]
	public async Task ExecuteAsync_Should_ShowHelp_When_HelpVariantsProvided(string helpCommand)
	{
		// Arrange
		var args = new[] { helpCommand };

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Theory]
	[InlineData("version")]
	[InlineData("--version")]
	[InlineData("-v")]
	public async Task ExecuteAsync_Should_ShowVersion_When_VersionVariantsProvided(string versionCommand)
	{
		// Arrange
		var args = new[] { versionCommand };

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Theory]
	[InlineData("unknown")]
	[InlineData("invalid")]
	[InlineData("badcommand")]
	[InlineData("")]
	public async Task ExecuteAsync_Should_ShowUnknownCommand_When_InvalidCommandsProvided(string invalidCommand)
	{
		// Arrange
		var args = string.IsNullOrEmpty(invalidCommand) ? new[] { "" } : new[] { invalidCommand };

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task ExecuteAsync_Should_HandleAsyncExceptions_When_AgentCommandThrows()
	{
		// Arrange
		var args = new[] { "agent", "status", "invalid-id" };
		_mockAgentCommands.ExecuteAsync(Arg.Any<string[]>())
			.Returns(Task.FromException<int>(new ArgumentException("Invalid ID")));

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task ExecuteAsync_Should_HandleAsyncExceptions_When_TaskCommandThrows()
	{
		// Arrange
		var args = new[] { "task", "assign", "invalid-task-id", "invalid-agent-id" };
		_mockTaskCommands.ExecuteAsync(Arg.Any<string[]>())
			.Returns(Task.FromException<int>(new FormatException("Invalid format")));

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task ExecuteAsync_Should_HandleAsyncExceptions_When_WorkflowCommandThrows()
	{
		// Arrange
		var args = new[] { "workflow", "execute", "invalid-workflow" };
		_mockWorkflowCommands.ExecuteAsync(Arg.Any<string[]>())
			.Returns(Task.FromException<int>(new InvalidOperationException("Workflow error")));

		// Act
		var exitCode = await _commandRouter.ExecuteAsync(args);

		// Assert
		exitCode.ShouldBe(1);
	}
} 