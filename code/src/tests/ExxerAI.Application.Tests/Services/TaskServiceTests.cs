using ExxerAI.Domain;
using ExxerAI.Domain.Operations;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Interface-Test-Driven Development (I-TDD) tests for ITaskService.
/// Tests focus on the interface contract and behavior, not implementation details.
/// </summary>
public class TaskServiceTests
{
    private readonly ITaskService _taskService;

    public TaskServiceTests()
    {
        _taskService = Substitute.For<ITaskService>();
    }

    #region CreateTaskAsync Tests

    [Fact]
    public async Task CreateTaskAsync_WithValidInput_ShouldReturnSuccessResult()
    {
        // Arrange
        var title = "Test Task";
        var description = "Test Description";
        var taskType = "DocumentProcessing";
        var priority = TaskPriority.Normal;
        var deadline = DateTime.UtcNow.AddDays(7);

        var expectedTask = new AgentTask
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            TaskType = taskType,
            Priority = priority,
            Deadline = deadline
        };
        var expectedResult = Result<AgentTask>.WithSuccess(expectedTask);

        _taskService.CreateTaskAsync(title, description, taskType, priority, deadline, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.CreateTaskAsync(title, description, taskType, priority, deadline, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.Title.ShouldBe(title);
        result.Data.Description.ShouldBe(description);
        result.Data.TaskType.ShouldBe(taskType);
        result.Data.Priority.ShouldBe(priority);
        result.Data.Deadline.ShouldBe(deadline);
    }

    [Theory]
    [InlineData("", "Valid Description", "ValidType")]
    [InlineData("   ", "Valid Description", "ValidType")]
    [InlineData("Valid Title", "", "ValidType")]
    [InlineData("Valid Title", "Valid Description", "")]
    public async Task CreateTaskAsync_WithInvalidInput_ShouldReturnFailureResult(string title, string description, string taskType)
    {
        // Arrange
        var priority = TaskPriority.Normal;
        var expectedResult = Result<AgentTask>.WithFailure("Invalid input provided");

        _taskService.CreateTaskAsync(title, description, taskType, priority, null, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.CreateTaskAsync(title, description, taskType, priority, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(TaskPriority.Low)]
    [InlineData(TaskPriority.Normal)]
    [InlineData(TaskPriority.High)]
    [InlineData(TaskPriority.Critical)]
    public async Task CreateTaskAsync_WithDifferentPriorities_ShouldHandleAllPriorityLevels(TaskPriority priority)
    {
        // Arrange
        var title = "Test Task";
        var description = "Test Description";
        var taskType = "TestType";

        var expectedTask = new AgentTask
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            TaskType = taskType,
            Priority = priority
        };
        var expectedResult = Result<AgentTask>.WithSuccess(expectedTask);

        _taskService.CreateTaskAsync(title, description, taskType, priority, null, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.CreateTaskAsync(title, description, taskType, priority, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.Priority.ShouldBe(priority);
    }

    #endregion CreateTaskAsync Tests

    #region GetTaskAsync Tests

    [Fact]
    public async Task GetTaskAsync_WithValidId_ShouldReturnTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var expectedTask = new AgentTask { Id = taskId, Title = "Test Task" };
        var expectedResult = Result<AgentTask>.WithSuccess(expectedTask);

        _taskService.GetTaskAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetTaskAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.Id.ShouldBe(taskId);
    }

    [Fact]
    public async Task GetTaskAsync_WithEmptyGuid_ShouldReturnFailureResult()
    {
        // Arrange
        var taskId = Guid.Empty;
        var expectedResult = Result<AgentTask>.WithFailure("Task ID cannot be empty");

        _taskService.GetTaskAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetTaskAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetTaskAsync_WithNonExistentId_ShouldReturnFailureResult()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var expectedResult = Result<AgentTask>.WithFailure("Task not found");

        _taskService.GetTaskAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetTaskAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion GetTaskAsync Tests

    #region GetPendingTasksAsync Tests

    [Fact]
    public async Task GetPendingTasksAsync_WhenPendingTasksExist_ShouldReturnPendingTasks()
    {
        // Arrange
        var maxCount = 50;
        var pendingTasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task1", AgentStatus =TaskAgentStatus.Pending },
            new() { Id = Guid.NewGuid(), Title = "Task2", AgentStatus =TaskAgentStatus.Pending },
            new() { Id = Guid.NewGuid(), Title = "Task3", AgentStatus =TaskAgentStatus.Pending }
        };
        var expectedResult = Result<IEnumerable<AgentTask>>.WithSuccess(pendingTasks);

        _taskService.GetPendingTasksAsync(maxCount, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetPendingTasksAsync(maxCount, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.Count().ShouldBe(3);
        result.Data.All(t => t.AgentStatus == TaskAgentStatus.Pending).ShouldBeTrue();
    }

    [Fact]
    public async Task GetPendingTasksAsync_WhenNoPendingTasks_ShouldReturnEmptyList()
    {
        // Arrange
        var maxCount = 100;
        var emptyTasks = new List<AgentTask>();
        var expectedResult = Result<IEnumerable<AgentTask>>.WithSuccess(emptyTasks);

        _taskService.GetPendingTasksAsync(maxCount, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetPendingTasksAsync(maxCount, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task GetPendingTasksAsync_WithDifferentMaxCounts_ShouldRespectLimit(int maxCount)
    {
        // Arrange
        var tasks = new List<AgentTask>();
        for (int i = 0; i < Math.Min(maxCount, 5); i++)
        {
            tasks.Add(new AgentTask { Id = Guid.NewGuid(), Title = $"Task{i}", AgentStatus = TaskAgentStatus.Pending });
        }
        var expectedResult = Result<IEnumerable<AgentTask>>.WithSuccess(tasks);

        _taskService.GetPendingTasksAsync(maxCount, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetPendingTasksAsync(maxCount, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.Count().ShouldBeLessThanOrEqualTo(maxCount);
    }

    #endregion GetPendingTasksAsync Tests

    #region GetAgentTasksAsync Tests

    [Fact]
    public async Task GetAgentTasksAsync_WithValidAgentId_ShouldReturnAgentTasks()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var agentTasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task1", AssignedAgentId = agentId },
            new() { Id = Guid.NewGuid(), Title = "Task2", AssignedAgentId = agentId }
        };
        var expectedResult = Result<IEnumerable<AgentTask>>.WithSuccess(agentTasks);

        _taskService.GetAgentTasksAsync(agentId, null, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetAgentTasksAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.Count().ShouldBe(2);
        result.Data.All(t => t.AssignedAgentId == agentId).ShouldBeTrue();
    }

    [Theory]
    [InlineData(TaskAgentStatus.Pending)]
    [InlineData(TaskAgentStatus.InProgress)]
    [InlineData(TaskAgentStatus.Completed)]
    [InlineData(TaskAgentStatus.Failed)]
    public async Task GetAgentTasksAsync_WithStatusFilter_ShouldReturnFilteredTasks(TaskAgentStatus agentStatus)
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var filteredTasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task1", AssignedAgentId = agentId, AgentStatus = agentStatus }
        };
        var expectedResult = Result<IEnumerable<AgentTask>>.WithSuccess(filteredTasks);

        _taskService.GetAgentTasksAsync(agentId, agentStatus, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetAgentTasksAsync(agentId, agentStatus, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.All(t => t.AgentStatus == agentStatus).ShouldBeTrue();
    }

    [Fact]
    public async Task GetAgentTasksAsync_WithEmptyAgentId_ShouldReturnFailure()
    {
        // Arrange
        var agentId = Guid.Empty;
        var expectedResult = Result<IEnumerable<AgentTask>>.WithFailure("Agent ID cannot be empty");

        _taskService.GetAgentTasksAsync(agentId, null, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetAgentTasksAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion GetAgentTasksAsync Tests

    #region UpdateTaskStatusAsync Tests

    [Theory]
    [InlineData(TaskAgentStatus.Pending)]
    [InlineData(TaskAgentStatus.InProgress)]
    [InlineData(TaskAgentStatus.Completed)]
    [InlineData(TaskAgentStatus.Failed)]
    [InlineData(TaskAgentStatus.Cancelled)]
    public async Task UpdateTaskStatusAsync_WithValidInput_ShouldReturnSuccess(TaskAgentStatus agentStatus)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _taskService.UpdateTaskStatusAsync(taskId, agentStatus, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.UpdateTaskStatusAsync(taskId, agentStatus, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_WithEmptyTaskId_ShouldReturnFailure()
    {
        // Arrange
        var taskId = Guid.Empty;
        var status = TaskAgentStatus.Completed;
        var expectedResult = Result<bool>.WithFailure("Task ID cannot be empty");

        _taskService.UpdateTaskStatusAsync(taskId, status, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.UpdateTaskStatusAsync(taskId, status, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_WithNonExistentTask_ShouldReturnFailure()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var status = TaskAgentStatus.Completed;
        var expectedResult = Result<bool>.WithFailure("Task not found");

        _taskService.UpdateTaskStatusAsync(taskId, status, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.UpdateTaskStatusAsync(taskId, status, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion UpdateTaskStatusAsync Tests

    #region AssignTaskToAgentAsync Tests

    [Fact]
    public async Task AssignTaskToAgentAsync_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _taskService.AssignTaskToAgentAsync(taskId, agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.AssignTaskToAgentAsync(taskId, agentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task AssignTaskToAgentAsync_WithEmptyTaskId_ShouldReturnFailure()
    {
        // Arrange
        var taskId = Guid.Empty;
        var agentId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithFailure("Task ID cannot be empty");

        _taskService.AssignTaskToAgentAsync(taskId, agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.AssignTaskToAgentAsync(taskId, agentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task AssignTaskToAgentAsync_WithEmptyAgentId_ShouldReturnFailure()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var agentId = Guid.Empty;
        var expectedResult = Result<bool>.WithFailure("Agent ID cannot be empty");

        _taskService.AssignTaskToAgentAsync(taskId, agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.AssignTaskToAgentAsync(taskId, agentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion AssignTaskToAgentAsync Tests

    #region CompleteTaskAsync Tests

    [Fact]
    public async Task CompleteTaskAsync_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var outputData = new TaskData { Properties = { ["result"] = "success" } };
        var expectedResult = Result<bool>.WithSuccess(true);

        _taskService.CompleteTaskAsync(taskId, outputData, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.CompleteTaskAsync(taskId, outputData, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task CompleteTaskAsync_WithoutOutputData_ShouldReturnSuccess()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _taskService.CompleteTaskAsync(taskId, null, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.CompleteTaskAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task CompleteTaskAsync_WithEmptyTaskId_ShouldReturnFailure()
    {
        // Arrange
        var taskId = Guid.Empty;
        var expectedResult = Result<bool>.WithFailure("Task ID cannot be empty");

        _taskService.CompleteTaskAsync(taskId, null, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.CompleteTaskAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion CompleteTaskAsync Tests

    #region FailTaskAsync Tests

    [Fact]
    public async Task FailTaskAsync_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var errorMessage = "Task failed due to timeout";
        var expectedResult = Result<bool>.WithSuccess(true);

        _taskService.FailTaskAsync(taskId, errorMessage, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.FailTaskAsync(taskId, errorMessage, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task FailTaskAsync_WithInvalidErrorMessage_ShouldReturnFailure(string errorMessage)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithFailure("Error message cannot be null or empty");

        _taskService.FailTaskAsync(taskId, errorMessage, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.FailTaskAsync(taskId, errorMessage, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion FailTaskAsync Tests

    #region GetOverdueTasksAsync Tests

    [Fact]
    public async Task GetOverdueTasksAsync_WhenOverdueTasksExist_ShouldReturnOverdueTasks()
    {
        // Arrange
        var overdueTasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Overdue1", Deadline = DateTime.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), Title = "Overdue2", Deadline = DateTime.UtcNow.AddDays(-2) }
        };
        var expectedResult = Result<IEnumerable<AgentTask>>.WithSuccess(overdueTasks);

        _taskService.GetOverdueTasksAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetOverdueTasksAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.Count().ShouldBe(2);
        result.Data.All(t => t.Deadline < DateTime.UtcNow).ShouldBeTrue();
    }

    [Fact]
    public async Task GetOverdueTasksAsync_WhenNoOverdueTasks_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyTasks = new List<AgentTask>();
        var expectedResult = Result<IEnumerable<AgentTask>>.WithSuccess(emptyTasks);

        _taskService.GetOverdueTasksAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetOverdueTasksAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.ShouldBeEmpty();
    }

    #endregion GetOverdueTasksAsync Tests

    #region DeleteTaskAsync Tests

    [Fact]
    public async Task DeleteTaskAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _taskService.DeleteTaskAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.DeleteTaskAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteTaskAsync_WithEmptyGuid_ShouldReturnFailure()
    {
        // Arrange
        var taskId = Guid.Empty;
        var expectedResult = Result<bool>.WithFailure("Task ID cannot be empty");

        _taskService.DeleteTaskAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.DeleteTaskAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteTaskAsync_WithNonExistentTask_ShouldReturnFailure()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithFailure("Task not found");

        _taskService.DeleteTaskAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.DeleteTaskAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion DeleteTaskAsync Tests

    #region Interface Contract Tests

    [Fact]
    public async Task ITaskService_AllMethods_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - All methods should accept and handle cancellation tokens
        await Should.NotThrowAsync(async () =>
        {
            #pragma warning disable xUnit1051

            await _taskService.CreateTaskAsync("test", "test", "test", TaskPriority.Normal, null, cts.Token);

            #pragma warning restore xUnit1051
            await _taskService.GetTaskAsync(Guid.NewGuid(), cts.Token);
            #pragma warning disable xUnit1051

            await _taskService.GetPendingTasksAsync(100, cts.Token);

            #pragma warning restore xUnit1051
            await _taskService.GetAgentTasksAsync(Guid.NewGuid(), null, cts.Token);
            await _taskService.UpdateTaskStatusAsync(Guid.NewGuid(), TaskAgentStatus.Completed, cts.Token);
            await _taskService.AssignTaskToAgentAsync(Guid.NewGuid(), Guid.NewGuid(), cts.Token);
            await _taskService.CompleteTaskAsync(Guid.NewGuid(), null, cts.Token);
            await _taskService.FailTaskAsync(Guid.NewGuid(), "error", cts.Token);
            await _taskService.CancelTaskAsync(Guid.NewGuid(), cts.Token);
            #pragma warning disable xUnit1051

            await _taskService.GetOverdueTasksAsync(cts.Token);

            #pragma warning restore xUnit1051
            await _taskService.DeleteTaskAsync(Guid.NewGuid(), cts.Token);
        });
    }

    [Fact]
    public void ITaskService_AllMethods_ShouldReturnResult()
    {
        // Arrange & Act & Assert - All methods should return Result<T> for consistent error handling
        var serviceType = typeof(ITaskService);
        var methods = serviceType.GetMethods();

        foreach (var method in methods.Where(m => !m.IsSpecialName))
        {
            var returnType = method.ReturnType;

            // Should be Task<Result<T>>
            returnType.IsGenericType.ShouldBeTrue($"Method {method.Name} should return a generic type");
            returnType.GetGenericTypeDefinition().ShouldBe(typeof(Task<>), $"Method {method.Name} should return Task");

            var taskInnerType = returnType.GetGenericArguments()[0];
            taskInnerType.IsGenericType.ShouldBeTrue($"Method {method.Name} should return Task<Result<T>>");
            taskInnerType.GetGenericTypeDefinition().ShouldBe(typeof(Result<>), $"Method {method.Name} should return Task<Result<T>>");
        }
    }

    #endregion Interface Contract Tests
}