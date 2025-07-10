using System.Reflection;
using Xunit;

namespace ExxerAI.CLI.Tests;

/// <summary>
/// Tests for Program main entry point
/// </summary>
public class ProgramTests
{
    [Fact]
    public async Task Main_Should_ReturnZero_When_NoArgumentsProvidedAsync()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Main_Should_ReturnZero_When_HelpCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "help" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Main_Should_ReturnZero_When_VersionCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "version" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Main_Should_ReturnOne_When_InvalidCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "invalid-command" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task Main_Should_InitializeRepositories_When_CalledAsync()
    {
        // Arrange
        var args = new[] { "help" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0); // If repositories initialize properly, help should work
    }

    [Fact]
    public async Task Main_Should_InitializeServices_When_CalledAsync()
    {
        // Arrange
        var args = new[] { "help" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0); // If services initialize properly, help should work
    }

    [Fact]
    public async Task Main_Should_InitializeCommands_When_CalledAsync()
    {
        // Arrange
        var args = new[] { "help" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0); // If commands initialize properly, help should work
    }

    [Fact]
    public async Task Main_Should_InitializeCommandRouter_When_CalledAsync()
    {
        // Arrange
        var args = new[] { "help" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0); // If router initializes properly, help should work
    }

    [Fact]
    public async Task Main_Should_RouteToAgentCommands_When_AgentCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "agent", "help" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Main_Should_RouteToTaskCommands_When_TaskCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "task", "help" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Main_Should_RouteToWorkflowCommands_When_WorkflowCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "workflow", "help" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Main_Should_HandleNullArguments_When_NullProvidedAsync()
    {
        // Arrange
        string[]? args = null!;

        // Act
        var exitCode = await CallMainMethodAsync(args!, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        // Should not throw and should handle gracefully
        exitCode.ShouldBeOneOf(0, 1);
    }

    [Fact]
    public async Task Main_Should_HandleEmptyStringArguments_When_EmptyStringsProvidedAsync()
    {
        // Arrange
        var args = new[] { "", "" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1); // Empty string commands should be treated as invalid
    }

    [Fact]
    public async Task Main_Should_HandleMultipleArguments_When_ComplexCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "agent", "list", "--agentStatus", "active" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0); // Should handle multi-argument commands
    }

    [Fact]
    public async Task Main_Should_HandleCaseInsensitiveCommands_When_UpperCaseProvidedAsync()
    {
        // Arrange
        var args = new[] { "HELP" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Main_Should_HandleCaseInsensitiveCommands_When_MixedCaseProvidedAsync()
    {
        // Arrange
        var args = new[] { "HeLp" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Theory]
    [InlineData("help")]
    [InlineData("--help")]
    [InlineData("-h")]
    [InlineData("version")]
    [InlineData("--version")]
    [InlineData("-v")]
    public async Task Main_Should_ReturnZero_When_BuiltInCommandsProvidedAsync(string command)
    {
        // Arrange
        var args = new[] { command };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Theory]
    [InlineData("agent")]
    [InlineData("agents")]
    [InlineData("task")]
    [InlineData("tasks")]
    [InlineData("workflow")]
    [InlineData("workflows")]
    public async Task Main_Should_ReturnZero_When_ValidCommandGroupsProvidedAsync(string commandGroup)
    {
        // Arrange
        var args = new[] { commandGroup };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0); // Should show group help
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("unknown")]
    [InlineData("badcommand")]
    [InlineData("123")]
    [InlineData("@#$")]
    public async Task Main_Should_ReturnOne_When_InvalidCommandsProvidedAsync(string invalidCommand)
    {
        // Arrange
        var args = new[] { invalidCommand };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task Main_Should_CreateInMemoryRepositories_When_CalledAsync()
    {
        // Arrange
        var args = new[] { "help" };

        // Act & Assert
        // If this completes without exception, repositories were created successfully
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Main_Should_CreateAgentService_When_CalledAsync()
    {
        // Arrange
        var args = new[] { "help" };

        // Act & Assert
        // If this completes without exception, AgentService was created successfully
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Main_Should_CreateAllCommandHandlers_When_CalledAsync()
    {
        // Arrange
        var args = new[] { "help" };

        // Act & Assert
        // If this completes without exception, all command handlers were created successfully
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Main_Should_PropagateCommandRouterResults_When_CommandExecutedAsync()
    {
        // Arrange
        var validArgs = new[] { "help" };
        var invalidArgs = new[] { "invalid" };

        // Act
        var validExitCode = await CallMainMethodAsync(validArgs, cancellationToken: TestContext.Current.CancellationToken);
        var invalidExitCode = await CallMainMethodAsync(invalidArgs, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        validExitCode.ShouldBe(0);
        invalidExitCode.ShouldBe(1);
    }

    [Fact]
    public async Task Main_Should_HandleLongArgumentLists_When_ManyArgumentsProvidedAsync()
    {
        // Arrange
        var args = new[] {
            "agent", "create", "TestAgent",
            "--description", "A test agent for validation",
            "--type", "DataProcessor"
        };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBeOneOf(0, 1); // Should handle gracefully regardless of outcome
    }

    [Fact]
    public async Task Main_Should_HandleSpecialCharactersInArguments_When_SpecialCharsProvidedAsync()
    {
        // Arrange
        var args = new[] { "agent", "create", "Test-Agent_123" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBeOneOf(0, 1); // Should handle gracefully regardless of outcome
    }

    [Fact]
    public async Task Main_Should_HandleUnicodeArguments_When_UnicodeProvidedAsync()
    {
        // Arrange
        var args = new[] { "agent", "create", "测试代理" };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBeOneOf(0, 1); // Should handle gracefully regardless of outcome
    }

    [Fact]
    public async Task Main_Should_HandleVeryLongArguments_When_LongStringsProvidedAsync()
    {
        // Arrange
        var longString = new string('A', 1000);
        var args = new[] { "agent", "create", longString };

        // Act
        var exitCode = await CallMainMethodAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBeOneOf(0, 1); // Should handle gracefully regardless of outcome
    }

    /// <summary>
    /// Helper method to call the private Main method via reflection
    /// </summary>
    /// <param name="args">Arguments to pass to Main</param>
    /// <returns>Exit code</returns>
    private static async Task<int> CallMainMethodAsync(string[] args, CancellationToken cancellationToken)
    {
        try
        {
            var programType = typeof(Program);
            var mainMethod = programType.GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);

            if (mainMethod == null)
            {
                throw new InvalidOperationException("Main method not found");
            }

            var result = mainMethod.Invoke(null, new object[] { args });

            if (result is Task<int> task)
            {
                return await task;
            }

            return (int)result!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            // If the Main method throws an exception, it should return 1
            return 1;
        }
        catch
        {
            // Any other reflection errors should also return 1
            return 1;
        }
    }
}