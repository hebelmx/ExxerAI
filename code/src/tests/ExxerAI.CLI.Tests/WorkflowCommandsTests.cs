using ExxerAI.CLI.Commands;
using Shouldly;

namespace ExxerAI.CLI.Tests;

/// <summary>
/// Comprehensive unit tests for WorkflowCommands CLI functionality
/// Tests all workflow management operations, validation, error handling, and edge cases
/// </summary>
public class WorkflowCommandsTests
{
    private readonly WorkflowCommands _workflowCommands;

    public WorkflowCommandsTests()
    {
        _workflowCommands = new WorkflowCommands();
    }

    [Fact]
    public void Constructor_ShouldInitializeSuccessfully()
    {
        // Act & Assert
        _workflowCommands.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyArgs_ShouldReturnZero()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync([]);

        // Assert
        result.ShouldBe(0);
    }

    [Theory]
    [InlineData("help")]
    [InlineData("--help")]
    [InlineData("-h")]
    public async Task ExecuteAsync_WithHelpCommands_ShouldReturnZero(string helpCommand)
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync([helpCommand]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCommand_ShouldReturnOne()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["invalidcommand"]);

        // Assert
        result.ShouldBe(1);
    }

    [Theory]
    [InlineData("list")]
    [InlineData("ls")]
    public async Task ExecuteAsync_WithListCommands_ShouldReturnZero(string command)
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync([command]);

        // Assert
        result.ShouldBe(0);
    }

    [Theory]
    [InlineData("create")]
    [InlineData("new")]
    public async Task ExecuteAsync_WithCreateCommands_ShouldReturnZero(string command)
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync([command, "TestWorkflow"]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_CreateWithoutName_ShouldReturnOne()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["create"]);

        // Assert
        result.ShouldBe(1);
    }

    [Theory]
    [InlineData("delete")]
    [InlineData("remove")]
    [InlineData("rm")]
    public async Task ExecuteAsync_WithDeleteCommands_ShouldReturnZero(string command)
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync([command, Guid.NewGuid().ToString()]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_DeleteWithInvalidId_ShouldReturnOne()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["delete", "invalid-id"]);

        // Assert
        result.ShouldBe(1);
    }

    [Theory]
    [InlineData("execute")]
    [InlineData("run")]
    public async Task ExecuteAsync_WithExecuteCommands_ShouldReturnZero(string command)
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync([command, Guid.NewGuid().ToString()]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_ExecuteWithInvalidId_ShouldReturnOne()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["execute", "invalid-id"]);

        // Assert
        result.ShouldBe(1);
    }

    [Theory]
    [InlineData("status")]
    [InlineData("info")]
    public async Task ExecuteAsync_WithStatusCommands_ShouldReturnZero(string command)
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync([command, Guid.NewGuid().ToString()]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_StatusWithInvalidId_ShouldReturnOne()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["status", "invalid-id"]);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_CreateWithDescription_ShouldReturnZero()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["create", "TestWorkflow", "--description", "Test Description"]);

        // Assert
        result.ShouldBe(0);
    }

    [Theory]
    [InlineData("-d")]
    [InlineData("--description")]
    public async Task ExecuteAsync_CreateWithDescriptionVariants_ShouldReturnZero(string descriptionFlag)
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["create", "TestWorkflow", descriptionFlag, "Test Description"]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_StopWithValidId_ShouldReturnZero()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["stop", Guid.NewGuid().ToString()]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_StopWithInvalidId_ShouldReturnOne()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["stop", "invalid-id"]);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_PauseWithValidId_ShouldReturnZero()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["pause", Guid.NewGuid().ToString()]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_ResumeWithValidId_ShouldReturnZero()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["resume", Guid.NewGuid().ToString()]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateWithValidId_ShouldReturnZero()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["update", Guid.NewGuid().ToString(), "--name", "NewName"]);

        // Assert
        result.ShouldBe(0);
    }

    [Theory]
    [InlineData("-n")]
    [InlineData("--name")]
    public async Task ExecuteAsync_UpdateWithNameVariants_ShouldReturnZero(string nameFlag)
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["update", Guid.NewGuid().ToString(), nameFlag, "NewName"]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_ListWithStatusFilter_ShouldReturnZero()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["list", "--status", "Running"]);

        // Assert
        result.ShouldBe(0);
    }

    [Theory]
    [InlineData("-s")]
    [InlineData("--status")]
    public async Task ExecuteAsync_ListWithStatusFilterVariants_ShouldReturnZero(string statusFlag)
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["list", statusFlag, "Completed"]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutRequiredArgs_ShouldReturnOne()
    {
        // Act
        var result1 = await _workflowCommands.ExecuteAsync(["delete"]);
        var result2 = await _workflowCommands.ExecuteAsync(["execute"]);
        var result3 = await _workflowCommands.ExecuteAsync(["status"]);
        var result4 = await _workflowCommands.ExecuteAsync(["stop"]);
        var result5 = await _workflowCommands.ExecuteAsync(["pause"]);
        var result6 = await _workflowCommands.ExecuteAsync(["resume"]);
        var result7 = await _workflowCommands.ExecuteAsync(["update"]);

        // Assert
        result1.ShouldBe(1);
        result2.ShouldBe(1);
        result3.ShouldBe(1);
        result4.ShouldBe(1);
        result5.ShouldBe(1);
        result6.ShouldBe(1);
        result7.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleCommands_ShouldHandleIndependently()
    {
        // Act
        var result1 = await _workflowCommands.ExecuteAsync(["create", "Workflow1"]);
        var result2 = await _workflowCommands.ExecuteAsync(["list"]);
        var result3 = await _workflowCommands.ExecuteAsync(["help"]);

        // Assert
        result1.ShouldBe(0);
        result2.ShouldBe(0);
        result3.ShouldBe(0);
    }

    [Theory]
    [InlineData("CREATE")]
    [InlineData("Create")]
    [InlineData("cReAtE")]
    public async Task ExecuteAsync_WithMixedCaseCommands_ShouldWork(string command)
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync([command, "TestWorkflow"]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithSpecialCharactersInName_ShouldWork()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["create", "Workflow!@#$%", "--description", "Test with spaces & symbols"]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyStringArgs_ShouldHandleGracefully()
    {
        // Act
        var result = await _workflowCommands.ExecuteAsync(["create", "", "--description", ""]);

        // Assert - Should return error for empty name
        result.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithLongArgumentList_ShouldHandleEfficiently()
    {
        // Arrange
        var longArgList = new[] { "create", "TestWorkflow" }
            .Concat(Enumerable.Range(0, 100).SelectMany(i => new[] { $"--option{i}", $"value{i}" }))
            .ToArray();

        // Act
        var result = await _workflowCommands.ExecuteAsync(longArgList);

        // Assert
        result.ShouldBe(0);
    }
} 