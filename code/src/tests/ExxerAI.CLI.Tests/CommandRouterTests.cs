using ExxerAI.CLI.Commands;
using NSubstitute;
using Shouldly;

namespace ExxerAI.CLI.Tests;

/// <summary>
/// Comprehensive unit tests for CommandRouter CLI functionality
/// Tests command routing, help system, version display, error handling, and edge cases
/// </summary>
public class CommandRouterTests
{
    private readonly AgentCommands _agentCommands;
    private readonly TaskCommands _taskCommands;
    private readonly WorkflowCommands _workflowCommands;
    private readonly CommandRouter _commandRouter;
    private readonly StringWriter _consoleOutput;
    private readonly TextWriter _originalOutput;

    public CommandRouterTests()
    {
        _agentCommands = Substitute.For<AgentCommands>(null!, null!);
        _taskCommands = Substitute.For<TaskCommands>(null!, null!);
        _workflowCommands = Substitute.For<WorkflowCommands>();
        _commandRouter = new CommandRouter(_agentCommands, _taskCommands, _workflowCommands);
        
        // Capture console output for verification
        _consoleOutput = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _consoleOutput.Dispose();
    }

    public class ConstructorTests : CommandRouterTests
    {
        [Fact]
        public void Constructor_WithValidDependencies_ShouldInitializeSuccessfully()
        {
            // Act & Assert
            _commandRouter.ShouldNotBeNull();
        }

        [Fact]
        public void Constructor_WithNullAgentCommands_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new CommandRouter(null!, _taskCommands, _workflowCommands));
        }

        [Fact]
        public void Constructor_WithNullTaskCommands_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new CommandRouter(_agentCommands, null!, _workflowCommands));
        }

        [Fact]
        public void Constructor_WithNullWorkflowCommands_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new CommandRouter(_agentCommands, _taskCommands, null!));
        }
    }

    public class ExecuteAsyncTests : CommandRouterTests
    {
        [Fact]
        public async Task ExecuteAsync_WithEmptyArgs_ShouldShowHelpAndReturnZero()
        {
            // Act
            var result = await _commandRouter.ExecuteAsync([]);

            // Assert
            result.ShouldBe(0);
            var output = _consoleOutput.ToString();
            output.ShouldContain("ExxerAI CLI - Autonomous AI Agent Management System");
            output.ShouldContain("Usage: exxerai <command> [options]");
        }

        [Theory]
        [InlineData("agent")]
        [InlineData("agents")]
        public async Task ExecuteAsync_WithAgentCommands_ShouldRouteToAgentCommands(string command)
        {
            // Arrange
            _agentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

            // Act
            var result = await _commandRouter.ExecuteAsync([command, "list"]);

            // Assert
            result.ShouldBe(0);
            await _agentCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.SequenceEqual(new[] { "list" })));
        }

        [Theory]
        [InlineData("task")]
        [InlineData("tasks")]
        public async Task ExecuteAsync_WithTaskCommands_ShouldRouteToTaskCommands(string command)
        {
            // Arrange
            _taskCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

            // Act
            var result = await _commandRouter.ExecuteAsync([command, "list"]);

            // Assert
            result.ShouldBe(0);
            await _taskCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.SequenceEqual(new[] { "list" })));
        }

        [Theory]
        [InlineData("workflow")]
        [InlineData("workflows")]
        public async Task ExecuteAsync_WithWorkflowCommands_ShouldRouteToWorkflowCommands(string command)
        {
            // Arrange
            _workflowCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

            // Act
            var result = await _commandRouter.ExecuteAsync([command, "list"]);

            // Assert
            result.ShouldBe(0);
            await _workflowCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.SequenceEqual(new[] { "list" })));
        }

        [Theory]
        [InlineData("help")]
        [InlineData("--help")]
        [InlineData("-h")]
        public async Task ExecuteAsync_WithHelpCommands_ShouldShowHelpAndReturnZero(string helpCommand)
        {
            // Act
            var result = await _commandRouter.ExecuteAsync([helpCommand]);

            // Assert
            result.ShouldBe(0);
            var output = _consoleOutput.ToString();
            output.ShouldContain("ExxerAI CLI - Autonomous AI Agent Management System");
            output.ShouldContain("Commands:");
            output.ShouldContain("agent    Manage AI agents");
            output.ShouldContain("task     Manage tasks");
            output.ShouldContain("workflow Manage workflows");
            output.ShouldContain("Examples:");
        }

        [Theory]
        [InlineData("version")]
        [InlineData("--version")]
        [InlineData("-v")]
        public async Task ExecuteAsync_WithVersionCommands_ShouldShowVersionAndReturnZero(string versionCommand)
        {
            // Act
            var result = await _commandRouter.ExecuteAsync([versionCommand]);

            // Assert
            result.ShouldBe(0);
            var output = _consoleOutput.ToString();
            output.ShouldContain("ExxerAI CLI v1.0.0");
            output.ShouldContain("Autonomous AI Agent Management System");
        }

        [Fact]
        public async Task ExecuteAsync_WithUnknownCommand_ShouldShowErrorAndReturnOne()
        {
            // Act
            var result = await _commandRouter.ExecuteAsync(["unknowncommand"]);

            // Assert
            result.ShouldBe(1);
            var output = _consoleOutput.ToString();
            output.ShouldContain("Unknown command: unknowncommand");
            output.ShouldContain("Use 'exxerai help' to see available commands.");
        }

        [Fact]
        public async Task ExecuteAsync_WithCaseInsensitiveCommands_ShouldWork()
        {
            // Arrange
            _agentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

            // Act
            var result1 = await _commandRouter.ExecuteAsync(["AGENT", "list"]);
            var result2 = await _commandRouter.ExecuteAsync(["Agent", "list"]);
            var result3 = await _commandRouter.ExecuteAsync(["aGeNt", "list"]);

            // Assert
            result1.ShouldBe(0);
            result2.ShouldBe(0);
            result3.ShouldBe(0);
            await _agentCommands.Received(3).ExecuteAsync(Arg.Any<string[]>());
        }

        [Fact]
        public async Task ExecuteAsync_WithAgentCommandAndArgs_ShouldPassCorrectArgs()
        {
            // Arrange
            _agentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);
            var expectedArgs = new[] { "create", "TestAgent", "--description", "Test Description" };

            // Act
            var fullArgs = new[] { "agent" }.Concat(expectedArgs).ToArray();
            var result = await _commandRouter.ExecuteAsync(fullArgs);

            // Assert
            result.ShouldBe(0);
            await _agentCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.SequenceEqual(expectedArgs)));
        }

        [Fact]
        public async Task ExecuteAsync_WithTaskCommandAndArgs_ShouldPassCorrectArgs()
        {
            // Arrange
            _taskCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);
            var expectedArgs = new[] { "assign", "task123", "agent456" };

            // Act
            var fullArgs = new[] { "task" }.Concat(expectedArgs).ToArray();
            var result = await _commandRouter.ExecuteAsync(fullArgs);

            // Assert
            result.ShouldBe(0);
            await _taskCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.SequenceEqual(expectedArgs)));
        }

        [Fact]
        public async Task ExecuteAsync_WithWorkflowCommandAndArgs_ShouldPassCorrectArgs()
        {
            // Arrange
            _workflowCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);
            var expectedArgs = new[] { "execute", "workflow123" };

            // Act
            var fullArgs = new[] { "workflow" }.Concat(expectedArgs).ToArray();
            var result = await _commandRouter.ExecuteAsync(fullArgs);

            // Assert
            result.ShouldBe(0);
            await _workflowCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.SequenceEqual(expectedArgs)));
        }

        [Fact]
        public async Task ExecuteAsync_WithAgentCommandOnly_ShouldPassEmptyArgs()
        {
            // Arrange
            _agentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

            // Act
            var result = await _commandRouter.ExecuteAsync(["agent"]);

            // Assert
            result.ShouldBe(0);
            await _agentCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.Length == 0));
        }

        [Fact]
        public async Task ExecuteAsync_WithTaskCommandOnly_ShouldPassEmptyArgs()
        {
            // Arrange
            _taskCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

            // Act
            var result = await _commandRouter.ExecuteAsync(["task"]);

            // Assert
            result.ShouldBe(0);
            await _taskCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.Length == 0));
        }

        [Fact]
        public async Task ExecuteAsync_WithWorkflowCommandOnly_ShouldPassEmptyArgs()
        {
            // Arrange
            _workflowCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

            // Act
            var result = await _commandRouter.ExecuteAsync(["workflow"]);

            // Assert
            result.ShouldBe(0);
            await _workflowCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.Length == 0));
        }
    }

    public class ErrorHandlingTests : CommandRouterTests
    {
        [Fact]
        public async Task ExecuteAsync_WithAgentCommandThrowingException_ShouldReturnOneAndShowError()
        {
            // Arrange
            _agentCommands.ExecuteAsync(Arg.Any<string[]>()).Throws(new InvalidOperationException("Test agent exception"));

            // Act
            var result = await _commandRouter.ExecuteAsync(["agent", "list"]);

            // Assert
            result.ShouldBe(1);
            var output = _consoleOutput.ToString();
            output.ShouldContain("Error executing command 'agent': Test agent exception");
        }

        [Fact]
        public async Task ExecuteAsync_WithTaskCommandThrowingException_ShouldReturnOneAndShowError()
        {
            // Arrange
            _taskCommands.ExecuteAsync(Arg.Any<string[]>()).Throws(new InvalidOperationException("Test task exception"));

            // Act
            var result = await _commandRouter.ExecuteAsync(["task", "list"]);

            // Assert
            result.ShouldBe(1);
            var output = _consoleOutput.ToString();
            output.ShouldContain("Error executing command 'task': Test task exception");
        }

        [Fact]
        public async Task ExecuteAsync_WithWorkflowCommandThrowingException_ShouldReturnOneAndShowError()
        {
            // Arrange
            _workflowCommands.ExecuteAsync(Arg.Any<string[]>()).Throws(new InvalidOperationException("Test workflow exception"));

            // Act
            var result = await _commandRouter.ExecuteAsync(["workflow", "list"]);

            // Assert
            result.ShouldBe(1);
            var output = _consoleOutput.ToString();
            output.ShouldContain("Error executing command 'workflow': Test workflow exception");
        }

        [Fact]
        public async Task ExecuteAsync_WithAgentCommandReturningErrorCode_ShouldReturnSameErrorCode()
        {
            // Arrange
            _agentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(42);

            // Act
            var result = await _commandRouter.ExecuteAsync(["agent", "invalidcommand"]);

            // Assert
            result.ShouldBe(42);
        }

        [Fact]
        public async Task ExecuteAsync_WithTaskCommandReturningErrorCode_ShouldReturnSameErrorCode()
        {
            // Arrange
            _taskCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(99);

            // Act
            var result = await _commandRouter.ExecuteAsync(["task", "invalidcommand"]);

            // Assert
            result.ShouldBe(99);
        }

        [Fact]
        public async Task ExecuteAsync_WithWorkflowCommandReturningErrorCode_ShouldReturnSameErrorCode()
        {
            // Arrange
            _workflowCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(55);

            // Act
            var result = await _commandRouter.ExecuteAsync(["workflow", "invalidcommand"]);

            // Assert
            result.ShouldBe(55);
        }
    }

    public class HelpSystemTests : CommandRouterTests
    {
        [Fact]
        public async Task ExecuteAsync_ShowHelp_ShouldDisplayCompleteHelpText()
        {
            // Act
            var result = await _commandRouter.ExecuteAsync(["help"]);

            // Assert
            result.ShouldBe(0);
            var output = _consoleOutput.ToString();
            
            // Verify header
            output.ShouldContain("ExxerAI CLI - Autonomous AI Agent Management System");
            
            // Verify usage
            output.ShouldContain("Usage: exxerai <command> [options]");
            
            // Verify commands section
            output.ShouldContain("Commands:");
            output.ShouldContain("agent    Manage AI agents (list, create, delete, status)");
            output.ShouldContain("task     Manage tasks (list, create, assign, update)");
            output.ShouldContain("workflow Manage workflows (list, create, execute)");
            output.ShouldContain("help     Show this help information");
            output.ShouldContain("version  Show version information");
            
            // Verify examples section
            output.ShouldContain("Examples:");
            output.ShouldContain("exxerai agent list");
            output.ShouldContain("exxerai agent create \"Value Processor\" --description \"Processes data\"");
            output.ShouldContain("exxerai task list --status pending");
            output.ShouldContain("exxerai task assign 123 456");
            
            // Verify footer
            output.ShouldContain("Use 'exxerai <command> --help' for more information about a command.");
        }

        [Fact]
        public async Task ExecuteAsync_ShowVersion_ShouldDisplayVersionInfo()
        {
            // Act
            var result = await _commandRouter.ExecuteAsync(["version"]);

            // Assert
            result.ShouldBe(0);
            var output = _consoleOutput.ToString();
            output.ShouldContain("ExxerAI CLI v1.0.0");
            output.ShouldContain("Autonomous AI Agent Management System");
        }

        [Fact]
        public async Task ExecuteAsync_ShowUnknownCommand_ShouldDisplayErrorWithHelpHint()
        {
            // Act
            var result = await _commandRouter.ExecuteAsync(["invalidcommand", "arg1", "arg2"]);

            // Assert
            result.ShouldBe(1);
            var output = _consoleOutput.ToString();
            output.ShouldContain("Unknown command: invalidcommand");
            output.ShouldContain("Use 'exxerai help' to see available commands.");
        }
    }

    public class CommandParsingTests : CommandRouterTests
    {
        [Fact]
        public async Task ExecuteAsync_WithWhitespaceInCommands_ShouldHandleCorrectly()
        {
            // Arrange
            _agentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

            // Act
            var result = await _commandRouter.ExecuteAsync([" agent ", "list"]);

            // Assert - Trimming should be handled at caller level, but command should work
            result.ShouldBe(0);
        }

        [Fact]
        public async Task ExecuteAsync_WithSpecialCharactersInArgs_ShouldPassThrough()
        {
            // Arrange
            _agentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);
            var specialArgs = new[] { "create", "Agent!@#$%", "--description", "Test with spaces & symbols" };

            // Act
            var fullArgs = new[] { "agent" }.Concat(specialArgs).ToArray();
            var result = await _commandRouter.ExecuteAsync(fullArgs);

            // Assert
            result.ShouldBe(0);
            await _agentCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.SequenceEqual(specialArgs)));
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyStringArgs_ShouldPassThrough()
        {
            // Arrange
            _agentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);
            var emptyArgs = new[] { "list", "", "--status", "" };

            // Act
            var fullArgs = new[] { "agent" }.Concat(emptyArgs).ToArray();
            var result = await _commandRouter.ExecuteAsync(fullArgs);

            // Assert
            result.ShouldBe(0);
            await _agentCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.SequenceEqual(emptyArgs)));
        }

        [Theory]
        [InlineData("agent", "list")]
        [InlineData("AGENT", "LIST")]
        [InlineData("Agent", "List")]
        [InlineData("aGeNt", "LiSt")]
        public async Task ExecuteAsync_WithMixedCaseCommands_ShouldRouteCorrectly(string command, string subCommand)
        {
            // Arrange
            _agentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

            // Act
            var result = await _commandRouter.ExecuteAsync([command, subCommand]);

            // Assert
            result.ShouldBe(0);
            await _agentCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.Length == 1 && args[0] == subCommand));
        }
    }

    public class PerformanceTests : CommandRouterTests
    {
        [Fact]
        public async Task ExecuteAsync_WithLongArgumentList_ShouldHandleEfficiently()
        {
            // Arrange
            _agentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);
            var longArgList = Enumerable.Range(0, 1000).Select(i => $"arg{i}").ToArray();
            var fullArgs = new[] { "agent" }.Concat(longArgList).ToArray();

            // Act
            var result = await _commandRouter.ExecuteAsync(fullArgs);

            // Assert
            result.ShouldBe(0);
            await _agentCommands.Received(1).ExecuteAsync(Arg.Is<string[]>(args => args.Length == 1000));
        }

        [Fact]
        public async Task ExecuteAsync_MultipleCallsInSequence_ShouldMaintainState()
        {
            // Arrange
            _agentCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);
            _taskCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);
            _workflowCommands.ExecuteAsync(Arg.Any<string[]>()).Returns(0);

            // Act
            var result1 = await _commandRouter.ExecuteAsync(["agent", "list"]);
            var result2 = await _commandRouter.ExecuteAsync(["task", "list"]);
            var result3 = await _commandRouter.ExecuteAsync(["workflow", "list"]);
            var result4 = await _commandRouter.ExecuteAsync(["help"]);

            // Assert
            result1.ShouldBe(0);
            result2.ShouldBe(0);
            result3.ShouldBe(0);
            result4.ShouldBe(0);
            
            await _agentCommands.Received(1).ExecuteAsync(Arg.Any<string[]>());
            await _taskCommands.Received(1).ExecuteAsync(Arg.Any<string[]>());
            await _workflowCommands.Received(1).ExecuteAsync(Arg.Any<string[]>());
        }
    }
} 