namespace ExxerAI.CLI.Tests;

/// <summary>
/// Tests for CLI validation patterns and Result<T> behavior
/// </summary>
public class CLIValidationTests
{
    private readonly IAgentService _agentService;
    private readonly IRepository<Agent> _agentRepository;
    private readonly ITaskRepository _taskRepository;

    public CLIValidationTests()
    {
        _agentService = Substitute.For<IAgentService>();
        _agentRepository = Substitute.For<IRepository<Agent>>();
        _taskRepository = Substitute.For<ITaskRepository>();
    }

    [Fact]
    public void AgentCommands_Constructor_WithNullArguments_ShouldCreateInstance()
    {
        // Arrange & Act - Following Result<T> pattern, constructors don't validate
        var withNullService = new AgentCommands(null!, _agentRepository);
        var withNullRepo = new AgentCommands(_agentService, null!);
        var withBothNull = new AgentCommands(null!, null!);

        // Assert - Constructors succeed, validation happens at execution
        withNullService.ShouldNotBeNull();
        withNullRepo.ShouldNotBeNull();
        withBothNull.ShouldNotBeNull();
    }

    [Fact]
    public void TaskCommands_Constructor_WithNullArguments_ShouldCreateInstance()
    {
        // Arrange & Act - Following Result<T> pattern
        var withNullTaskRepo = new TaskCommands(null!, _agentRepository);
        var withNullAgentRepo = new TaskCommands(_taskRepository, null!);

        // Assert
        withNullTaskRepo.ShouldNotBeNull();
        withNullAgentRepo.ShouldNotBeNull();
    }

    [Fact]
    public void WorkflowCommands_Constructor_ShouldCreateInstance()
    {
        // Arrange & Act - Following Result<T> pattern (parameterless constructor)
        var workflowCommands = new WorkflowCommands();

        // Assert
        workflowCommands.ShouldNotBeNull();
    }

    [Fact]
    public void CommandRouter_Constructor_WithNullArguments_ShouldCreateInstance()
    {
        // Arrange
        var agentCommands = new AgentCommands(_agentService, _agentRepository);
        var taskCommands = new TaskCommands(_taskRepository, _agentRepository);
        var workflowCommands = new WorkflowCommands();

        // Act - Following Result<T> pattern
        var withNullAgent = new CommandRouter(null!, taskCommands, workflowCommands);
        var withNullTask = new CommandRouter(agentCommands, null!, workflowCommands);
        var withNullWorkflow = new CommandRouter(agentCommands, taskCommands, null!);

        // Assert
        withNullAgent.ShouldNotBeNull();
        withNullTask.ShouldNotBeNull();
        withNullWorkflow.ShouldNotBeNull();
    }

    [Fact]
    public async Task AgentCommands_ExecuteAsync_WithNullDependency_ShouldReturnErrorExitCode()
    {
        // Arrange - CLI with null service dependency
        var commands = new AgentCommands(null!, _agentRepository);
        var args = new[] { "create", "TestAgent" };

        // Capture console output - Linux style with proper restoration
        var originalOut = Console.Out;
        try
        {
            using var consoleCapture = new StringWriter();
            Console.SetOut(consoleCapture);

            // Act - Execution should handle null gracefully and return error
            var result = await commands.ExecuteAsync(args);

            // Restore console before assertions
            Console.SetOut(originalOut);

            // Assert - Should return error exit code (1) not success (0)
            result.ShouldBe(1);
        }
        finally
        {
            // Ensure console is always restored
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task CommandRouter_ExecuteAsync_WithEmptyArgs_ShouldShowHelpAndReturnSuccess()
    {
        // Arrange
        var agentCommands = new AgentCommands(_agentService, _agentRepository);
        var taskCommands = new TaskCommands(_taskRepository, _agentRepository);
        var workflowCommands = new WorkflowCommands();
        var router = new CommandRouter(agentCommands, taskCommands, workflowCommands);

        // Act
        var result = await router.ExecuteAsync(Array.Empty<string>());

        // Assert
        result.ShouldBe(0); // Help returns success
    }

    [Fact]
    public async Task CommandRouter_ExecuteAsync_WithVersionCommand_ShouldShowVersionAndReturnSuccess()
    {
        // Arrange
        var agentCommands = new AgentCommands(_agentService, _agentRepository);
        var taskCommands = new TaskCommands(_taskRepository, _agentRepository);
        var workflowCommands = new WorkflowCommands();
        var router = new CommandRouter(agentCommands, taskCommands, workflowCommands);

        // Act
        var result = await router.ExecuteAsync(new[] { "version" });

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task CommandRouter_ExecuteAsync_WithUnknownCommand_ShouldReturnErrorExitCode()
    {
        // Arrange
        var agentCommands = new AgentCommands(_agentService, _agentRepository);
        var taskCommands = new TaskCommands(_taskRepository, _agentRepository);
        var workflowCommands = new WorkflowCommands();
        var router = new CommandRouter(agentCommands, taskCommands, workflowCommands);

        // Capture console output - Linux style with proper restoration
        var originalOut = Console.Out;
        try
        {
            using var consoleCapture = new StringWriter();

            Console.SetOut(consoleCapture);

            // Act - await fully before reading output
            var result = await router.ExecuteAsync(new[] { "invalidcommand" });

            // Restore console before reading capture
            Console.SetOut(originalOut);

            // Assert
            result.ShouldBe(1); // Unknown command returns error
            var output = consoleCapture.ToString();
            output.ShouldContain("Unknown command: invalidcommand", Case.Insensitive);
        }
        finally
        {
            // Ensure console is always restored
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Tests for AgentCommands validation
    /// </summary>
    public class AgentCommandsValidationTests
    {
        [Fact]
        public void ValidateConstructorParameters_WithValidParameters_ShouldReturnSuccess()
        {
            // Arrange
            var agentService = Substitute.For<IAgentService>();
            var agentRepository = Substitute.For<IRepository<Agent>>();

            // Act
            var result = AgentCommands.ValidateConstructorParameters(agentService, agentRepository);

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void ValidateConstructorParameters_WithNullAgentService_ShouldReturnFailure()
        {
            // Arrange
            var agentRepository = Substitute.For<IRepository<Agent>>();

            // Act
            var result = AgentCommands.ValidateConstructorParameters(null!, agentRepository);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Parameter 'agentService' cannot be null");
        }

        [Fact]
        public void ValidateConstructorParameters_WithNullAgentRepository_ShouldReturnFailure()
        {
            // Arrange
            var agentService = Substitute.For<IAgentService>();

            // Act
            var result = AgentCommands.ValidateConstructorParameters(agentService, null!);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Parameter 'agentRepository' cannot be null");
        }

        [Fact]
        public void ValidateConstructorParameters_WithBothNull_ShouldReturnFailureWithBothNames()
        {
            // Act
            var result = AgentCommands.ValidateConstructorParameters(null!, null!);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Multiple null parameters: agentService, agentRepository");
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange
            var agentService = Substitute.For<IAgentService>();
            var agentRepository = Substitute.For<IRepository<Agent>>();

            // Act
            var agentCommands = new AgentCommands(agentService, agentRepository);

            // Assert
            agentCommands.ShouldNotBeNull();
        }

        [Fact]
        public void Constructor_WithNullParameters_ShouldStillCreateInstance()
        {
            // Act & Assert - Following the rule of not throwing exceptions
            var agentCommands = new AgentCommands(null!, null!);
            agentCommands.ShouldNotBeNull();
        }
    }

    /// <summary>
    /// Tests for TaskCommands validation
    /// </summary>
    public class TaskCommandsValidationTests
    {
        [Fact]
        public void ValidateConstructorParameters_WithValidParameters_ShouldReturnSuccess()
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
        public void ValidateConstructorParameters_WithNullTaskRepository_ShouldReturnFailure()
        {
            // Arrange
            var agentRepository = Substitute.For<IRepository<Agent>>();

            // Act
            var result = TaskCommands.ValidateConstructorParameters(null!, agentRepository);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Parameter 'taskRepository' cannot be null");
        }

        [Fact]
        public void ValidateConstructorParameters_WithNullAgentRepository_ShouldReturnFailure()
        {
            // Arrange
            var taskRepository = Substitute.For<ITaskRepository>();

            // Act
            var result = TaskCommands.ValidateConstructorParameters(taskRepository, null!);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Parameter 'agentRepository' cannot be null");
        }

        [Fact]
        public void ValidateConstructorParameters_WithBothNull_ShouldReturnFailureWithBothNames()
        {
            // Act
            var result = TaskCommands.ValidateConstructorParameters(null!, null!);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Multiple null parameters: taskRepository, agentRepository");
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange
            var taskRepository = Substitute.For<ITaskRepository>();
            var agentRepository = Substitute.For<IRepository<Agent>>();

            // Act
            var taskCommands = new TaskCommands(taskRepository, agentRepository);

            // Assert
            taskCommands.ShouldNotBeNull();
        }

        [Fact]
        public void Constructor_WithNullParameters_ShouldStillCreateInstance()
        {
            // Act & Assert - Following the rule of not throwing exceptions
            var taskCommands = new TaskCommands(null!, null!);
            taskCommands.ShouldNotBeNull();
        }
    }

    /// <summary>
    /// Tests for CommandRouter validation
    /// </summary>
    public class CommandRouterValidationTests
    {
        [Fact]
        public void ValidateConstructorParameters_WithValidParameters_ShouldReturnSuccess()
        {
            // Arrange
            var agentCommands = new AgentCommands(null!, null!);
            var taskCommands = new TaskCommands(null!, null!);
            var workflowCommands = new WorkflowCommands();

            // Act
            var result = CommandRouter.ValidateConstructorParameters(agentCommands, taskCommands, workflowCommands);

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void ValidateConstructorParameters_WithNullAgentCommands_ShouldReturnFailure()
        {
            // Arrange
            var taskCommands = new TaskCommands(null!, null!);
            var workflowCommands = new WorkflowCommands();

            // Act
            var result = CommandRouter.ValidateConstructorParameters(null!, taskCommands, workflowCommands);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Parameter 'agentCommands' cannot be null");
        }

        [Fact]
        public void ValidateConstructorParameters_WithNullTaskCommands_ShouldReturnFailure()
        {
            // Arrange
            var agentCommands = new AgentCommands(null!, null!);
            var workflowCommands = new WorkflowCommands();

            // Act
            var result = CommandRouter.ValidateConstructorParameters(agentCommands, null!, workflowCommands);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Parameter 'taskCommands' cannot be null");
        }

        [Fact]
        public void ValidateConstructorParameters_WithNullWorkflowCommands_ShouldReturnFailure()
        {
            // Arrange
            var agentCommands = new AgentCommands(null!, null!);
            var taskCommands = new TaskCommands(null!, null!);

            // Act
            var result = CommandRouter.ValidateConstructorParameters(agentCommands, taskCommands, null!);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Parameter 'workflowCommands' cannot be null");
        }

        [Fact]
        public void ValidateConstructorParameters_WithMultipleNulls_ShouldReturnFailureWithAllNames()
        {
            // Arrange
            var workflowCommands = new WorkflowCommands();

            // Act
            var result = CommandRouter.ValidateConstructorParameters(null!, null!, workflowCommands);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Multiple null parameters: agentCommands, taskCommands");
        }

        [Fact]
        public void ValidateConstructorParameters_WithAllNull_ShouldReturnFailureWithAllNames()
        {
            // Act
            var result = CommandRouter.ValidateConstructorParameters(null!, null!, null!);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Multiple null parameters: agentCommands, taskCommands, workflowCommands");
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange
            var agentCommands = new AgentCommands(null!, null!);
            var taskCommands = new TaskCommands(null!, null!);
            var workflowCommands = new WorkflowCommands();

            // Act
            var commandRouter = new CommandRouter(agentCommands, taskCommands, workflowCommands);

            // Assert
            commandRouter.ShouldNotBeNull();
        }

        [Fact]
        public void Constructor_WithNullParameters_ShouldStillCreateInstance()
        {
            // Act & Assert - Following the rule of not throwing exceptions
            var commandRouter = new CommandRouter(null!, null!, null!);
            commandRouter.ShouldNotBeNull();
        }
    }

    /// <summary>
    /// Tests for WorkflowCommands (parameterless constructor)
    /// </summary>
    public class WorkflowCommandsValidationTests
    {
        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Act
            var workflowCommands = new WorkflowCommands();

            // Assert
            workflowCommands.ShouldNotBeNull();
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyArgs_ShouldReturnZero()
        {
            // Arrange
            var workflowCommands = new WorkflowCommands();

            // Act
            var result = await workflowCommands.ExecuteAsync(Array.Empty<string>());

            // Assert
            result.ShouldBe(0);
        }

        [Fact]
        public async Task ExecuteAsync_WithHelpCommand_ShouldReturnZero()
        {
            // Arrange
            var workflowCommands = new WorkflowCommands();

            // Act
            var result = await workflowCommands.ExecuteAsync(new[] { "help" });

            // Assert
            result.ShouldBe(0);
        }

        [Fact]
        public async Task ExecuteAsync_WithUnknownCommand_ShouldReturnOne()
        {
            // Arrange
            var workflowCommands = new WorkflowCommands();

            // Act
            var result = await workflowCommands.ExecuteAsync(new[] { "unknown" });

            // Assert
            result.ShouldBe(1);
        }
    }
}