using ExxerAI.Application.Interfaces;
using ExxerAI.CLI.Commands;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;

namespace ExxerAI.CLI.Tests;

/// <summary>
/// Comprehensive unit tests for TaskCommands CLI functionality
/// Tests all task management operations, validation, error handling, and edge cases
/// </summary>
public class TaskCommandsTests
{
    private readonly ITaskRepository _taskRepository;
    private readonly IRepository<Agent> _agentRepository;
    private readonly TaskCommands _taskCommands;

    public TaskCommandsTests()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _agentRepository = Substitute.For<IRepository<Agent>>();
        _taskCommands = new TaskCommands(_taskRepository, _agentRepository);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldInitializeSuccessfully()
    {
        // Act & Assert
        _taskCommands.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithNullTaskRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new TaskCommands(null!, _agentRepository));
    }

    [Fact]
    public void Constructor_WithNullAgentRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new TaskCommands(_taskRepository, null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyArgs_ShouldReturnZero()
    {
        // Act
        var result = await _taskCommands.ExecuteAsync([]);

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
        var result = await _taskCommands.ExecuteAsync([helpCommand]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCommand_ShouldReturnOne()
    {
        // Act
        var result = await _taskCommands.ExecuteAsync(["invalidcommand"]);

        // Assert
        result.ShouldBe(1);
    }

    [Theory]
    [InlineData("list")]
    [InlineData("ls")]
    public async Task ExecuteAsync_WithListCommands_ShouldCallRepository(string command)
    {
        // Arrange
        _taskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.WithSuccess([]));

        // Act
        var result = await _taskCommands.ExecuteAsync([command]);

        // Assert
        result.ShouldBe(0);
        await _taskRepository.Received(1).GetAllAsync();
    }

    [Theory]
    [InlineData("create")]
    [InlineData("new")]
    public async Task ExecuteAsync_WithCreateCommands_ShouldCreateTask(string command)
    {
        // Arrange
        var testTask = CreateTestTask();
        _taskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.WithSuccess(testTask));

        // Act
        var result = await _taskCommands.ExecuteAsync([command, "TestTask"]);

        // Assert
        result.ShouldBe(0);
        await _taskRepository.Received(1).AddAsync(Arg.Any<AgentTask>());
    }

    [Fact]
    public async Task ExecuteAsync_CreateWithoutName_ShouldReturnOne()
    {
        // Act
        var result = await _taskCommands.ExecuteAsync(["create"]);

        // Assert
        result.ShouldBe(1);
    }

    [Theory]
    [InlineData("delete")]
    [InlineData("remove")]
    [InlineData("rm")]
    public async Task ExecuteAsync_WithDeleteCommands_ShouldCallRepository(string command)
    {
        // Arrange
        var testId = Guid.NewGuid();
        _taskRepository.DeleteAsync(testId).Returns(Result<bool>.WithSuccess(true));

        // Act
        var result = await _taskCommands.ExecuteAsync([command, testId.ToString()]);

        // Assert
        result.ShouldBe(0);
        await _taskRepository.Received(1).DeleteAsync(testId);
    }

    [Fact]
    public async Task ExecuteAsync_DeleteWithInvalidId_ShouldReturnOne()
    {
        // Act
        var result = await _taskCommands.ExecuteAsync(["delete", "invalid-id"]);

        // Assert
        result.ShouldBe(1);
    }

    [Theory]
    [InlineData("status")]
    [InlineData("info")]
    public async Task ExecuteAsync_WithStatusCommands_ShouldCallRepository(string command)
    {
        // Arrange
        var testTask = CreateTestTask();
        _taskRepository.GetByIdAsync(testTask.Id).Returns(Result<AgentTask>.WithSuccess(testTask));

        // Act
        var result = await _taskCommands.ExecuteAsync([command, testTask.Id.ToString()]);

        // Assert
        result.ShouldBe(0);
        await _taskRepository.Received(1).GetByIdAsync(testTask.Id);
    }

    [Fact]
    public async Task ExecuteAsync_AssignWithValidIds_ShouldCallRepository()
    {
        // Arrange
        var testTask = CreateTestTask();
        var testAgent = CreateTestAgent();
        _taskRepository.GetByIdAsync(testTask.Id).Returns(Result<AgentTask>.WithSuccess(testTask));
        _agentRepository.GetByIdAsync(testAgent.Id).Returns(Result<Agent>.WithSuccess(testAgent));
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.WithSuccess(testTask));

        // Act
        var result = await _taskCommands.ExecuteAsync(["assign", testTask.Id.ToString(), testAgent.Id.ToString()]);

        // Assert
        result.ShouldBe(0);
        await _taskRepository.Received(1).GetByIdAsync(testTask.Id);
        await _agentRepository.Received(1).GetByIdAsync(testAgent.Id);
        await _taskRepository.Received(1).UpdateAsync(Arg.Any<AgentTask>());
    }

    [Fact]
    public async Task ExecuteAsync_AssignWithInvalidTaskId_ShouldReturnOne()
    {
        // Act
        var result = await _taskCommands.ExecuteAsync(["assign", "invalid-task-id", Guid.NewGuid().ToString()]);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_AssignWithInvalidAgentId_ShouldReturnOne()
    {
        // Act
        var result = await _taskCommands.ExecuteAsync(["assign", Guid.NewGuid().ToString(), "invalid-agent-id"]);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateWithValidId_ShouldCallRepository()
    {
        // Arrange
        var testTask = CreateTestTask();
        _taskRepository.GetByIdAsync(testTask.Id).Returns(Result<AgentTask>.WithSuccess(testTask));
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.WithSuccess(testTask));

        // Act
        var result = await _taskCommands.ExecuteAsync(["update", testTask.Id.ToString(), "--name", "NewName"]);

        // Assert
        result.ShouldBe(0);
        await _taskRepository.Received(1).GetByIdAsync(testTask.Id);
        await _taskRepository.Received(1).UpdateAsync(Arg.Any<AgentTask>());
    }

    [Theory]
    [InlineData("start")]
    [InlineData("complete")]
    [InlineData("cancel")]
    public async Task ExecuteAsync_StatusChangeCommands_ShouldCallRepository(string command)
    {
        // Arrange
        var testTask = CreateTestTask();
        _taskRepository.GetByIdAsync(testTask.Id).Returns(Result<AgentTask>.WithSuccess(testTask));
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.WithSuccess(testTask));

        // Act
        var result = await _taskCommands.ExecuteAsync([command, testTask.Id.ToString()]);

        // Assert
        result.ShouldBe(0);
        await _taskRepository.Received(1).GetByIdAsync(testTask.Id);
        await _taskRepository.Received(1).UpdateAsync(Arg.Any<AgentTask>());
    }

    [Fact]
    public async Task ExecuteAsync_ListWithStatusFilter_ShouldWork()
    {
        // Arrange
        var tasks = new[] { CreateTestTask("Task1", ExxerAI.Domain.TaskStatus.InProgress) };
        _taskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.WithSuccess(tasks));

        // Act
        var result = await _taskCommands.ExecuteAsync(["list", "--status", "InProgress"]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_ListWithAgentFilter_ShouldWork()
    {
        // Arrange
        var testAgent = CreateTestAgent();
        var tasks = new[] { CreateTestTask("Task1") };
        tasks[0].AssignedAgentId = testAgent.Id;
        _taskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.WithSuccess(tasks));

        // Act
        var result = await _taskCommands.ExecuteAsync(["list", "--agent", testAgent.Id.ToString()]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_CreateWithDescription_ShouldWork()
    {
        // Arrange
        var testTask = CreateTestTask();
        _taskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.WithSuccess(testTask));

        // Act
        var result = await _taskCommands.ExecuteAsync(["create", "TestTask", "--description", "Test Description"]);

        // Assert
        result.ShouldBe(0);
        await _taskRepository.Received(1).AddAsync(Arg.Any<AgentTask>());
    }

    [Fact]
    public async Task ExecuteAsync_CreateWithPriority_ShouldWork()
    {
        // Arrange
        var testTask = CreateTestTask();
        _taskRepository.AddAsync(Arg.Any<AgentTask>()).Returns(Result<AgentTask>.WithSuccess(testTask));

        // Act
        var result = await _taskCommands.ExecuteAsync(["create", "TestTask", "--priority", "High"]);

        // Assert
        result.ShouldBe(0);
        await _taskRepository.Received(1).AddAsync(Arg.Any<AgentTask>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRepositoryFailure_ShouldReturnOne()
    {
        // Arrange
        _taskRepository.GetAllAsync().Returns(Result<IEnumerable<AgentTask>>.WithFailure("Repository error"));

        // Act
        var result = await _taskCommands.ExecuteAsync(["list"]);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithException_ShouldReturnOne()
    {
        // Arrange
        _taskRepository.GetAllAsync().Returns(Task.FromException<Result<IEnumerable<AgentTask>>>(new InvalidOperationException("Test exception")));

        // Act
        var result = await _taskCommands.ExecuteAsync(["list"]);

        // Assert
        result.ShouldBe(1);
    }

    private static AgentTask CreateTestTask(string title = "TestTask", ExxerAI.Domain.TaskStatus status = ExxerAI.Domain.TaskStatus.Pending)
    {
        return new AgentTask
        {
            Id = Guid.NewGuid(),
            Title = title,
            Status = status,
            Description = $"Test task {title}",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            Priority = TaskPriority.Normal,
            TaskType = "TestType",
            AssignedAgentId = null
        };
    }

    private static Agent CreateTestAgent(string name = "TestAgent")
    {
        return new Agent
        {
            Id = Guid.NewGuid(),
            Name = name,
            Status = AgentStatus.Active,
            Description = $"Test agent {name}",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            Capabilities = new AgentCapabilities
            {
                CanProcessNaturalLanguage = true,
                MaxConcurrentTasks = 1
            }
        };
    }
} 