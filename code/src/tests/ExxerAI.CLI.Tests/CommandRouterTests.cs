using ExxerAI.Domain.Helpers.Operations;

namespace ExxerAI.CLI.Tests;

/// <summary>
/// Tests for CommandRouter CLI functionality
/// </summary>
public class CommandRouterTests
{
    private readonly IAgentService _mockAgentService;
    private readonly IRepository<Agent> _mockAgentRepository;
    private readonly ITaskRepository _mockTaskRepository;
    private readonly AgentCommands _agentCommands;
    private readonly TaskCommands _taskCommands;
    private readonly WorkflowCommands _workflowCommands;
    private readonly CommandRouter _commandRouter;

    public CommandRouterTests()
    {
        // Create mocked dependencies
        _mockAgentService = Substitute.For<IAgentService>();
        _mockAgentRepository = Substitute.For<IRepository<Agent>>();
        _mockTaskRepository = Substitute.For<ITaskRepository>();

        // Create real command instances with mocked dependencies
        _agentCommands = new AgentCommands(_mockAgentService, _mockAgentRepository);
        _taskCommands = new TaskCommands(_mockTaskRepository, _mockAgentRepository);
        _workflowCommands = new WorkflowCommands();

        _commandRouter = new CommandRouter(_agentCommands, _taskCommands, _workflowCommands);
    }

    [Fact]
    public void Constructor_Should_InitializeCorrectly_When_ValidParametersProvided()
    {
        // Arrange & Act
        var router = new CommandRouter(_agentCommands, _taskCommands, _workflowCommands);

        // Assert
        router.ShouldNotBeNull();
    }

    [Fact]
    public void ValidateConstructorParameters_Should_ReturnSuccess_When_AllParametersValid()
    {
        // Arrange
        var agentService = Substitute.For<IAgentService>();
        var agentRepository = Substitute.For<IRepository<Agent>>();
        var taskRepository = Substitute.For<ITaskRepository>();
        var agentCommands = new AgentCommands(agentService, agentRepository);
        var taskCommands = new TaskCommands(taskRepository, agentRepository);
        var workflowCommands = new WorkflowCommands();

        // Act
        var result = CommandRouter.ValidateConstructorParameters(agentCommands, taskCommands, workflowCommands);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateConstructorParameters_Should_ReturnFailure_When_AgentCommandsIsNull()
    {
        // Arrange
        var taskRepository = Substitute.For<ITaskRepository>();
        var agentRepository = Substitute.For<IRepository<Agent>>();
        var taskCommands = new TaskCommands(taskRepository, agentRepository);
        var workflowCommands = new WorkflowCommands();

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
        var agentService = Substitute.For<IAgentService>();
        var agentRepository = Substitute.For<IRepository<Agent>>();
        var agentCommands = new AgentCommands(agentService, agentRepository);
        var workflowCommands = new WorkflowCommands();

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
        var agentService = Substitute.For<IAgentService>();
        var agentRepository = Substitute.For<IRepository<Agent>>();
        var taskRepository = Substitute.For<ITaskRepository>();
        var agentCommands = new AgentCommands(agentService, agentRepository);
        var taskCommands = new TaskCommands(taskRepository, agentRepository);

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
        var testAgents = new List<Agent>
        {
            new Agent { Name = "Test Agent", Description = "Test Description", Capabilities = new AgentCapabilities() }
        };
        _mockAgentRepository.GetAllAsync().Returns(Result<IEnumerable<Agent>>.Success(testAgents));

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentRepository.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task ExecuteAsync_Should_RouteToAgentCommands_When_AgentsCommandProvided()
    {
        // Arrange
        var args = new[] { "agents", "create", "TestAgent" };
        var testAgent = new Agent { Name = "TestAgent", Description = "Test Description", Capabilities = new AgentCapabilities() };
        _mockAgentService.CreateAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentCapabilities>())
            .Returns(Result<Agent>.Success(testAgent));

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentService.Received(1).CreateAgentAsync("TestAgent", Arg.Any<string>(), Arg.Any<AgentCapabilities>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_RouteToTaskCommands_When_TaskCommandProvided()
    {
        // Arrange
        var args = new[] { "task", "list" };
        var testTasks = new List<AgentTask>();
        _mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(testTasks));

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task ExecuteAsync_Should_RouteToTaskCommands_When_TasksCommandProvided()
    {
        // Arrange
        var args = new[] { "tasks", "create", "TestTask", "--type", "DataProcessing" };
        _mockTaskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(new AgentTask()));

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).AddAsync(Arg.Any<AgentTask>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_RouteToWorkflowCommands_When_WorkflowCommandProvided()
    {
        // Arrange
        var args = new[] { "workflow", "list" };

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_RouteToWorkflowCommands_When_WorkflowsCommandProvided()
    {
        // Arrange
        var args = new[] { "workflows", "create", "TestWorkflow" };

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(0);
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
        var testAgents = new List<Agent>();
        _mockAgentRepository.GetAllAsync().Returns(Result<IEnumerable<Agent>>.Success(testAgents));

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentRepository.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task ExecuteAsync_Should_HandleCaseInsensitiveCommands_When_MixedCaseProvided()
    {
        // Arrange
        var args = new[] { "TaSk", "list" };
        var testTasks = new List<AgentTask>();
        _mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(testTasks));

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task ExecuteAsync_Should_PropagateExitCode_When_CommandFails()
    {
        // Arrange
        var args = new[] { "agent", "list" };
        _mockAgentRepository.GetAllAsync().Returns(Result<IEnumerable<Agent>>.WithFailure("Test error"));

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_Should_PropagateExitCode_When_CommandSucceeds()
    {
        // Arrange
        var args = new[] { "task", "create", "TestTask", "--type", "DataProcessing" };
        _mockTaskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.Success(new AgentTask()));

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
        _mockAgentRepository.GetAllAsync().Returns(Task.FromException<Result<IEnumerable<Agent>>>(new InvalidOperationException("Test exception")));

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
        var testAgent = new Agent { Name = "TestAgent", Description = "Test description", Capabilities = new AgentCapabilities() };
        _mockAgentService.CreateAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentCapabilities>())
            .Returns(Result<Agent>.Success(testAgent));

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentService.Received(1).CreateAgentAsync("TestAgent", "Test description", Arg.Any<AgentCapabilities>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_PassEmptyArguments_When_OnlyCommandProvided()
    {
        // Arrange
        var args = new[] { "workflow" };

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Theory]
    [InlineData("agent", "list")]
    [InlineData("agents", "list")]
    [InlineData("task", "list")]
    [InlineData("tasks", "list")]
    [InlineData("workflow", "list")]
    [InlineData("workflows", "list")]
    public async Task ExecuteAsync_Should_RouteCorrectly_When_CommandAliasesProvided(string command, string subCommand)
    {
        // Arrange
        var args = new[] { command, subCommand };
        var testAgents = new List<Agent>();
        var testTasks = new List<AgentTask>();
        _mockAgentRepository.GetAllAsync().Returns(Result<IEnumerable<Agent>>.Success(testAgents));
        _mockTaskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.Success(testTasks));

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(0);
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
        var args = new[] { "agent", "agentStatus", "invalid-guid" };

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_Should_HandleAsyncExceptions_When_TaskCommandThrows()
    {
        // Arrange
        var args = new[] { "task", "agentStatus", "invalid-guid" };

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_Should_HandleAsyncExceptions_When_WorkflowCommandThrows()
    {
        // Arrange
        var args = new[] { "workflow", "invalid-subcommand" };

        // Act
        var exitCode = await _commandRouter.ExecuteAsync(args);

        // Assert
        exitCode.ShouldBe(1); // WorkflowCommands returns 1 for unknown commands
    }
}