using ExxerAI.Domain.Operations;
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
        result.Error!.ShouldContain("taskRepository");
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
        result.Error!.ShouldContain("agentRepository");
    }

    [Fact]
    public async Task ExecuteAsync_Should_ShowHelp_When_NoArgumentsProvidedAsync()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallListTasks_When_ListCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "list" };
        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallListTasks_When_LsCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "ls" };
        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallCreateTask_When_CreateCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "create", "TestTask", "--type", "DataProcessing" };
        var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
        _mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).AddAsync(Arg.Is<AgentTask>(t => t.Title == "TestTask"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallAssignTask_When_AssignCommandProvidedAsync()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var args = new[] { "assign", taskId.ToString(), agentId.ToString() };

        var testAgent = new Agent { Id = agentId, Name = "TestAgent" };
        var testTask = new AgentTask { Id = taskId, Title = "TestTask" };

        _mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>()).Returns(Result<Agent>.Success(testAgent));
        _mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));
        _mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => t.AssignedAgentId == agentId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallUpdateTaskStatus_When_UpdateCommandProvidedAsync()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var args = new[] { "update", taskId.ToString(), "--agentStatus", "Completed" };
        var testTask = new AgentTask { Id = taskId, Title = "TestTask", AgentStatus = TaskAgentStatus.InProgress };

        _mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));
        _mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => t.AgentStatus == TaskAgentStatus.Completed), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallDeleteTask_When_DeleteCommandProvidedAsync()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var args = new[] { "delete", taskId.ToString() };
        _mockTaskRepository.DeleteAsync(taskId, Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result<bool>.Success(true)));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).DeleteAsync(taskId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallShowTaskStatus_When_StatusCommandProvidedAsync()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var args = new[] { "agentStatus", taskId.ToString() };
        var testTask = new AgentTask { Id = taskId, Title = "TestTask" };
        _mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result<AgentTask>.Success(testTask)));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).GetByIdAsync(taskId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallListOverdueTasks_When_OverdueCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "overdue" };
        var agentId = Guid.NewGuid();
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "OverdueTask", Deadline = DateTime.UtcNow.AddDays(-1), TaskType = "Test", AgentStatus =TaskAgentStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow, AssignedAgentId = agentId },
            new() { Id = Guid.NewGuid(), Title = "FutureTask", Deadline = DateTime.UtcNow.AddDays(1), TaskType = "Test", AgentStatus =TaskAgentStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow }
        };
        _mockTaskRepository.GetOverdueTasksAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(tasks));
        // Mock agent lookup for assigned tasks
        _mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>()).Returns(Result<Agent>.Success(new Agent { Id = agentId, Name = "TestAgent" }));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ShowHelp_When_HelpCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "help" };

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ShowUnknownCommand_When_InvalidCommandProvidedAsync()
    {
        // Arrange
        var args = new[] { "invalid" };

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ListTasks_Should_ReturnSuccess_When_NoTasksExistAsync()
    {
        // Arrange
        var args = new[] { "list" };
        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>())));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ListTasks_Should_DisplayTasks_When_TasksExistAsync()
    {
        // Arrange
        var args = new[] { "list" };
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task1", TaskType = "DataProcessing", AgentStatus =TaskAgentStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Task2", TaskType = "Analysis", AgentStatus =TaskAgentStatus.InProgress, Priority = TaskPriority.High, CreatedAt = DateTime.UtcNow }
        };
        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Success(tasks)));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ListTasks_Should_FilterByStatus_When_StatusFilterProvidedAsync()
    {
        // Arrange
        var args = new[] { "list", "--agentStatus", "Pending" };
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "PendingTask", AgentStatus =TaskAgentStatus.Pending, TaskType = "Test", Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "CompletedTask", AgentStatus =TaskAgentStatus.Completed, TaskType = "Test", Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow }
        };
        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(tasks));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ListTasks_Should_FilterByPriority_When_PriorityFilterProvidedAsync()
    {
        // Arrange
        var args = new[] { "list", "--priority", "High" };
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "HighPriorityTask", Priority = TaskPriority.High, TaskType = "Test", AgentStatus =TaskAgentStatus.Pending, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "NormalPriorityTask", Priority = TaskPriority.Normal, TaskType = "Test", AgentStatus =TaskAgentStatus.Pending, CreatedAt = DateTime.UtcNow }
        };
        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(tasks));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ListTasks_Should_FilterByType_When_TypeFilterProvidedAsync()
    {
        // Arrange
        var args = new[] { "list", "--type", "DataProcessing" };
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "DataTask", TaskType = "DataProcessing", AgentStatus =TaskAgentStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "AnalysisTask", TaskType = "Analysis", AgentStatus =TaskAgentStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow }
        };
        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(tasks));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ListTasks_Should_FilterByAgent_When_AgentFilterProvidedAsync()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "list", "--agent", agentId.ToString() };
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "AssignedTask", AssignedAgentId = agentId, TaskType = "Test", AgentStatus =TaskAgentStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "UnassignedTask", AssignedAgentId = null, TaskType = "Test", AgentStatus =TaskAgentStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow }
        };
        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(tasks));
        // Mock agent lookup for agent name display
        _mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>()).Returns(Result<Agent>.Success(new Agent { Id = agentId, Name = "TestAgent" }));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ListTasks_Should_ReturnError_When_InvalidStatusFilterAsync()
    {
        // Arrange
        var args = new[] { "list", "--agentStatus", "InvalidStatus" };
        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ListTasks_Should_ReturnError_When_InvalidPriorityFilterAsync()
    {
        // Arrange
        var args = new[] { "list", "--priority", "InvalidPriority" };
        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ListTasks_Should_ReturnError_When_InvalidAgentIdFormatAsync()
    {
        // Arrange
        var args = new[] { "list", "--agent", "invalid-agent-id" };
        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ListTasks_Should_ReturnError_When_RepositoryFailsAsync()
    {
        // Arrange
        var args = new[] { "list" };
        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.WithFailure("Database error"));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task CreateTask_Should_ReturnError_When_NoTitleProvidedAsync()
    {
        // Arrange
        var args = new[] { "create" };

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task CreateTask_Should_ReturnError_When_NoTypeProvidedAsync()
    {
        // Arrange
        var args = new[] { "create", "TestTask" };

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task CreateTask_Should_CreateWithDefaultPriority_When_NoPriorityProvidedAsync()
    {
        // Arrange
        var args = new[] { "create", "TestTask", "--type", "DataProcessing" };
        var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
        _mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).AddAsync(Arg.Is<AgentTask>(t => t.Priority == TaskPriority.Normal), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTask_Should_CreateWithCustomPriority_When_PriorityProvidedAsync()
    {
        // Arrange
        var args = new[] { "create", "TestTask", "--type", "DataProcessing", "--priority", "High" };
        var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
        _mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).AddAsync(Arg.Is<AgentTask>(t => t.Priority == TaskPriority.High), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTask_Should_CreateWithDescription_When_DescriptionProvidedAsync()
    {
        // Arrange
        var args = new[] { "create", "TestTask", "--type", "DataProcessing", "--description", "Custom description" };
        var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
        _mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).AddAsync(Arg.Is<AgentTask>(t => t.Description == "Custom description"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTask_Should_CreateWithDeadline_When_DeadlineProvidedAsync()
    {
        // Arrange
        var args = new[] { "create", "TestTask", "--type", "DataProcessing", "--deadline", "2025-12-31" };
        var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
        _mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).AddAsync(Arg.Is<AgentTask>(t => t.Deadline.HasValue), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTask_Should_ReturnError_When_InvalidPriorityAsync()
    {
        // Arrange
        var args = new[] { "create", "TestTask", "--type", "DataProcessing", "--priority", "InvalidPriority" };

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task CreateTask_Should_ReturnError_When_InvalidDeadlineFormatAsync()
    {
        // Arrange
        var args = new[] { "create", "TestTask", "--type", "DataProcessing", "--deadline", "invalid-date" };

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task CreateTask_Should_ReturnError_When_RepositoryFailsAsync()
    {
        // Arrange
        var args = new[] { "create", "TestTask", "--type", "DataProcessing" };
        _mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.WithFailure("Repository error"));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task AssignTask_Should_ReturnError_When_InsufficientArgumentsAsync()
    {
        // Arrange
        var args = new[] { "assign", Guid.NewGuid().ToString() };

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task AssignTask_Should_ReturnError_When_InvalidTaskIdFormatAsync()
    {
        // Arrange
        var args = new[] { "assign", "invalid-task-id", Guid.NewGuid().ToString() };

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task AssignTask_Should_ReturnError_When_InvalidAgentIdFormatAsync()
    {
        // Arrange
        var args = new[] { "assign", Guid.NewGuid().ToString(), "invalid-agent-id" };

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task AssignTask_Should_ReturnError_When_AgentNotFoundAsync()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var args = new[] { "assign", taskId.ToString(), agentId.ToString() };
        _mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>()).Returns(Result<Agent>.WithFailure("Agent not found"));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task AssignTask_Should_ReturnError_When_TaskNotFoundAsync()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var args = new[] { "assign", taskId.ToString(), agentId.ToString() };
        var testAgent = new Agent { Id = agentId, Name = "TestAgent" };
        _mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>()).Returns(Result<Agent>.Success(testAgent));
        _mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.WithFailure("Task not found"));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task AssignTask_Should_ReturnError_When_UpdateFailsAsync()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var args = new[] { "assign", taskId.ToString(), agentId.ToString() };
        var testAgent = new Agent { Id = agentId, Name = "TestAgent" };
        var testTask = new AgentTask { Id = taskId, Title = "TestTask" };

        _mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>()).Returns(Result<Agent>.Success(testAgent));
        _mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));
        _mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.WithFailure("Update failed"));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("-h")]
    public async Task ExecuteAsync_Should_ShowHelp_When_HelpFlagsProvidedAsync(string helpFlag)
    {
        // Arrange
        var args = new[] { helpFlag };

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Theory]
    [InlineData("new")]
    [InlineData("remove")]
    [InlineData("rm")]
    [InlineData("info")]
    public async Task ExecuteAsync_Should_HandleCommandAliases_When_AliasesProvidedAsync(string alias)
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
            _mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result<AgentTask>.Success(testTask)));
        }
        else if (alias == "remove" || alias == "rm")
        {
            _mockTaskRepository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result<bool>.Success(true)));
        }
        else if (alias == "info")
        {
            var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
            _mockTaskRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));
        }

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_HandleExceptions_When_ExceptionThrownAsync()
    {
        // Arrange
        var args = new[] { "list" };
        _mockTaskRepository.When(x => x.GetAllAsync()).Do(x => throw new InvalidOperationException("Test exception"));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Theory]
    [InlineData("-s")]
    [InlineData("-p")]
    [InlineData("-t")]
    [InlineData("-a")]
    [InlineData("-d")]
    public async Task ExecuteAsync_Should_HandleShortFlags_When_ShortFlagsProvidedAsync(string shortFlag)
    {
        // Arrange
        var args = shortFlag switch
        {
            "-s" => new[] { "list", shortFlag, "Pending" },
            "-p" => new[] { "list", shortFlag, "High" },
            "-t" => new[] { "list", shortFlag, "DataProcessing" },
            "-a" => new[] { "list", shortFlag, "invalid-agent-id" },
            "-d" => new[] { "create", "TestTask", "--type", "DataProcessing", shortFlag, "Test description" },
            _ => Array.Empty<string>()
        };

        if (shortFlag == "-d")
        {
            var testTask = new AgentTask { Id = Guid.NewGuid(), Title = "TestTask" };
            _mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));
        }
        else
        {
            _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));
        }

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        // For invalid agent GUID, it should return error (1), for others success (0)
        if (shortFlag == "-a")
        {
            exitCode.ShouldBe(1); // Invalid GUID format should fail
        }
        else
        {
            exitCode.ShouldBe(0);
        }
    }

    [Fact]
    public async Task ListTasks_Should_ShowAgentNames_When_TasksAreAssignedAsync()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "list" };
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "AssignedTask", AssignedAgentId = agentId, TaskType = "Test", AgentStatus =TaskAgentStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow }
        };
        var testAgent = new Agent { Id = agentId, Name = "TestAgent" };

        _mockTaskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(tasks));
        _mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>()).Returns(Result<Agent>.Success(testAgent));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentRepository.Received(1).GetByIdAsync(agentId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTaskStatus_Should_SetStartedAt_When_ChangingToPendingAsync()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var args = new[] { "update", taskId.ToString(), "--agentStatus", "InProgress" };
        var testTask = new AgentTask { Id = taskId, Title = "TestTask", AgentStatus = TaskAgentStatus.Pending, StartedAt = null };

        _mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));
        _mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t =>
            t.AgentStatus == TaskAgentStatus.InProgress && t.StartedAt.HasValue), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTaskStatus_Should_SetCompletedAt_When_ChangingToCompletedAsync()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var args = new[] { "update", taskId.ToString(), "--agentStatus", "Completed" };
        var testTask = new AgentTask { Id = taskId, Title = "TestTask", AgentStatus = TaskAgentStatus.InProgress, CompletedAt = null };

        _mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));
        _mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()).Returns(Result<AgentTask>.Success(testTask));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t =>
            t.AgentStatus == TaskAgentStatus.Completed && t.CompletedAt.HasValue), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListOverdueTasks_Should_FilterOverdueTasks_When_OverdueTasksExistAsync()
    {
        // Arrange
        var args = new[] { "overdue" };
        var agentId = Guid.NewGuid();
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "OverdueTask", Deadline = DateTime.UtcNow.AddDays(-1), TaskType = "Test", AgentStatus =TaskAgentStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow, AssignedAgentId = agentId },
            new() { Id = Guid.NewGuid(), Title = "FutureTask", Deadline = DateTime.UtcNow.AddDays(1), TaskType = "Test", AgentStatus =TaskAgentStatus.Pending, Priority = TaskPriority.Normal, CreatedAt = DateTime.UtcNow }
        };
        _mockTaskRepository.GetOverdueTasksAsync(Arg.Any<CancellationToken>()).Returns(Result<IEnumerable<AgentTask>>.Success(tasks));
        // Mock agent lookup for assigned tasks
        _mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>()).Returns(Result<Agent>.Success(new Agent { Id = agentId, Name = "TestAgent" }));

        // Act
        var exitCode = await _taskCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }
}