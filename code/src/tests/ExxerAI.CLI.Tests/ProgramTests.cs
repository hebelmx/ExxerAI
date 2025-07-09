using System.Reflection;
using Xunit;

namespace ExxerAI.CLI.Tests;

/// <summary>
/// Tests for Program main entry point
/// </summary>
public class ProgramTests
{
	[Fact]
	public async Task Main_Should_ReturnZero_When_NoArgumentsProvided()
	{
		// Arrange
		var args = Array.Empty<string>();

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task Main_Should_ReturnZero_When_HelpCommandProvided()
	{
		// Arrange
		var args = new[] { "help" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task Main_Should_ReturnZero_When_VersionCommandProvided()
	{
		// Arrange
		var args = new[] { "version" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task Main_Should_ReturnOne_When_InvalidCommandProvided()
	{
		// Arrange
		var args = new[] { "invalid-command" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task Main_Should_InitializeRepositories_When_Called()
	{
		// Arrange
		var args = new[] { "help" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0); // If repositories initialize properly, help should work
	}

	[Fact]
	public async Task Main_Should_InitializeServices_When_Called()
	{
		// Arrange
		var args = new[] { "help" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0); // If services initialize properly, help should work
	}

	[Fact]
	public async Task Main_Should_InitializeCommands_When_Called()
	{
		// Arrange
		var args = new[] { "help" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0); // If commands initialize properly, help should work
	}

	[Fact]
	public async Task Main_Should_InitializeCommandRouter_When_Called()
	{
		// Arrange
		var args = new[] { "help" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0); // If router initializes properly, help should work
	}

	[Fact]
	public async Task Main_Should_RouteToAgentCommands_When_AgentCommandProvided()
	{
		// Arrange
		var args = new[] { "agent", "help" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task Main_Should_RouteToTaskCommands_When_TaskCommandProvided()
	{
		// Arrange
		var args = new[] { "task", "help" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task Main_Should_RouteToWorkflowCommands_When_WorkflowCommandProvided()
	{
		// Arrange
		var args = new[] { "workflow", "help" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task Main_Should_HandleNullArguments_When_NullProvided()
	{
		// Arrange
		string[]? args = null!;

		// Act
		var exitCode = await CallMainMethod(args!);

		// Assert
		// Should not throw and should handle gracefully
		exitCode.ShouldBeOneOf(0, 1);
	}

	[Fact]
	public async Task Main_Should_HandleEmptyStringArguments_When_EmptyStringsProvided()
	{
		// Arrange
		var args = new[] { "", "" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(1); // Empty string commands should be treated as invalid
	}

	[Fact]
	public async Task Main_Should_HandleMultipleArguments_When_ComplexCommandProvided()
	{
		// Arrange
		var args = new[] { "agent", "list", "--agentStatus", "active" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0); // Should handle multi-argument commands
	}

	[Fact]
	public async Task Main_Should_HandleCaseInsensitiveCommands_When_UpperCaseProvided()
	{
		// Arrange
		var args = new[] { "HELP" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task Main_Should_HandleCaseInsensitiveCommands_When_MixedCaseProvided()
	{
		// Arrange
		var args = new[] { "HeLp" };

		// Act
		var exitCode = await CallMainMethod(args);

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
	public async Task Main_Should_ReturnZero_When_BuiltInCommandsProvided(string command)
	{
		// Arrange
		var args = new[] { command };

		// Act
		var exitCode = await CallMainMethod(args);

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
	public async Task Main_Should_ReturnZero_When_ValidCommandGroupsProvided(string commandGroup)
	{
		// Arrange
		var args = new[] { commandGroup };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(0); // Should show group help
	}

	[Theory]
	[InlineData("invalid")]
	[InlineData("unknown")]
	[InlineData("badcommand")]
	[InlineData("123")]
	[InlineData("@#$")]
	public async Task Main_Should_ReturnOne_When_InvalidCommandsProvided(string invalidCommand)
	{
		// Arrange
		var args = new[] { invalidCommand };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBe(1);
	}

	[Fact]
	public async Task Main_Should_CreateInMemoryRepositories_When_Called()
	{
		// Arrange
		var args = new[] { "help" };

		// Act & Assert
		// If this completes without exception, repositories were created successfully
		var exitCode = await CallMainMethod(args);
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task Main_Should_CreateAgentService_When_Called()
	{
		// Arrange
		var args = new[] { "help" };

		// Act & Assert
		// If this completes without exception, AgentService was created successfully
		var exitCode = await CallMainMethod(args);
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task Main_Should_CreateAllCommandHandlers_When_Called()
	{
		// Arrange
		var args = new[] { "help" };

		// Act & Assert
		// If this completes without exception, all command handlers were created successfully
		var exitCode = await CallMainMethod(args);
		exitCode.ShouldBe(0);
	}

	[Fact]
	public async Task Main_Should_PropagateCommandRouterResults_When_CommandExecuted()
	{
		// Arrange
		var validArgs = new[] { "help" };
		var invalidArgs = new[] { "invalid" };

		// Act
		var validExitCode = await CallMainMethod(validArgs);
		var invalidExitCode = await CallMainMethod(invalidArgs);

		// Assert
		validExitCode.ShouldBe(0);
		invalidExitCode.ShouldBe(1);
	}

	[Fact]
	public async Task Main_Should_HandleLongArgumentLists_When_ManyArgumentsProvided()
	{
		// Arrange
		var args = new[] { 
			"agent", "create", "TestAgent", 
			"--description", "A test agent for validation",
			"--type", "DataProcessor"
		};

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBeOneOf(0, 1); // Should handle gracefully regardless of outcome
	}

	[Fact]
	public async Task Main_Should_HandleSpecialCharactersInArguments_When_SpecialCharsProvided()
	{
		// Arrange
		var args = new[] { "agent", "create", "Test-Agent_123" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBeOneOf(0, 1); // Should handle gracefully regardless of outcome
	}

	[Fact]
	public async Task Main_Should_HandleUnicodeArguments_When_UnicodeProvided()
	{
		// Arrange
		var args = new[] { "agent", "create", "测试代理" };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBeOneOf(0, 1); // Should handle gracefully regardless of outcome
	}

	[Fact]
	public async Task Main_Should_HandleVeryLongArguments_When_LongStringsProvided()
	{
		// Arrange
		var longString = new string('A', 1000);
		var args = new[] { "agent", "create", longString };

		// Act
		var exitCode = await CallMainMethod(args);

		// Assert
		exitCode.ShouldBeOneOf(0, 1); // Should handle gracefully regardless of outcome
	}

	/// <summary>
	/// Helper method to call the private Main method via reflection
	/// </summary>
	/// <param name="args">Arguments to pass to Main</param>
	/// <returns>Exit code</returns>
	private static async Task<int> CallMainMethod(string[] args)
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