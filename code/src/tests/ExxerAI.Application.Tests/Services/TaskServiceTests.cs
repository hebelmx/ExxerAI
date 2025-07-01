using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;

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
        var result = await _taskService.CreateTaskAsync(title, description, taskType, priority, deadline);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Title.ShouldBe(title);
        result.Data.Description.ShouldBe(description);
        result.Data.TaskType.ShouldBe(taskType);
        result.Data.Priority.ShouldBe(priority);
        result.Data.Deadline.ShouldBe(deadline);
    }

    [Theory]
    [InlineData(null, "Valid Description", "ValidType")]
    [InlineData("", "Valid Description", "ValidType")]
    [InlineData("   ", "Valid Description", "ValidType")]
    [InlineData("Valid Title", null, "ValidType")]
    [InlineData("Valid Title", "", "ValidType")]
    [InlineData("Valid Title", "Valid Description", null)]
    [InlineData("Valid Title", "Valid Description", "")]
    public async Task CreateTaskAsync_WithInvalidInput_ShouldReturnFailureResult(string title, string description, string taskType)
    {
        // Arrange
        var priority = TaskPriority.Normal;
        var expectedResult = Result<AgentTask>.WithFailure("Invalid input provided");

        _taskService.CreateTaskAsync(title, description, taskType, priority, null, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.CreateTaskAsync(title, description, taskType, priority);

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
        var result = await _taskService.CreateTaskAsync(title, description, taskType, priority);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.Priority.ShouldBe(priority);
    }

    #endregion

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
        var result = await _taskService.GetTaskAsync(taskId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Id.ShouldBe(taskId);
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
        var result = await _taskService.GetTaskAsync(taskId);

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
        var result = await _taskService.GetTaskAsync(taskId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region GetPendingTasksAsync Tests

    [Fact]
    public async Task GetPendingTasksAsync_WhenPendingTasksExist_ShouldReturnPendingTasks()
    {
        // Arrange
        var maxCount = 50;
        var pendingTasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task1", Status = ExxerAI.Domain.TaskStatus.Pending },
            new() { Id = Guid.NewGuid(), Title = "Task2", Status = ExxerAI.Domain.TaskStatus.Pending },
            new() { Id = Guid.NewGuid(), Title = "Task3", Status = ExxerAI.Domain.TaskStatus.Pending }
        };
        var expectedResult = Result<IEnumerable<AgentTask>>.WithSuccess(pendingTasks);

        _taskService.GetPendingTasksAsync(maxCount, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetPendingTasksAsync(maxCount);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count().ShouldBe(3);
        result.Data.All(t => t.Status == ExxerAI.Domain.TaskStatus.Pending).ShouldBeTrue();
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
        var result = await _taskService.GetPendingTasksAsync(maxCount);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldBeEmpty();
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
            tasks.Add(new AgentTask { Id = Guid.NewGuid(), Title = $"Task{i}", Status = ExxerAI.Domain.TaskStatus.Pending });
        }
        var expectedResult = Result<IEnumerable<AgentTask>>.WithSuccess(tasks);

        _taskService.GetPendingTasksAsync(maxCount, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetPendingTasksAsync(maxCount);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.Count().ShouldBeLessThanOrEqualTo(maxCount);
    }

    #endregion

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
        var result = await _taskService.GetAgentTasksAsync(agentId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count().ShouldBe(2);
        result.Data.All(t => t.AssignedAgentId == agentId).ShouldBeTrue();
    }

    [Theory]
    [InlineData(ExxerAI.Domain.TaskStatus.Pending)]
    [InlineData(ExxerAI.Domain.TaskStatus.InProgress)]
    [InlineData(ExxerAI.Domain.TaskStatus.Completed)]
    [InlineData(ExxerAI.Domain.TaskStatus.Failed)]
    public async Task GetAgentTasksAsync_WithStatusFilter_ShouldReturnFilteredTasks(ExxerAI.Domain.TaskStatus status)
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var filteredTasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task1", AssignedAgentId = agentId, Status = status }
        };
        var expectedResult = Result<IEnumerable<AgentTask>>.WithSuccess(filteredTasks);

        _taskService.GetAgentTasksAsync(agentId, status, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.GetAgentTasksAsync(agentId, status);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.All(t => t.Status == status).ShouldBeTrue();
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
        var result = await _taskService.GetAgentTasksAsync(agentId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region UpdateTaskStatusAsync Tests

    [Theory]
    [InlineData(ExxerAI.Domain.TaskStatus.Pending)]
    [InlineData(ExxerAI.Domain.TaskStatus.InProgress)]
    [InlineData(ExxerAI.Domain.TaskStatus.Completed)]
    [InlineData(ExxerAI.Domain.TaskStatus.Failed)]
    [InlineData(ExxerAI.Domain.TaskStatus.Cancelled)]
    public async Task UpdateTaskStatusAsync_WithValidInput_ShouldReturnSuccess(ExxerAI.Domain.TaskStatus status)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _taskService.UpdateTaskStatusAsync(taskId, status, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.UpdateTaskStatusAsync(taskId, status);

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
        var status = ExxerAI.Domain.TaskStatus.Completed;
        var expectedResult = Result<bool>.WithFailure("Task ID cannot be empty");

        _taskService.UpdateTaskStatusAsync(taskId, status, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.UpdateTaskStatusAsync(taskId, status);

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
        var status = ExxerAI.Domain.TaskStatus.Completed;
        var expectedResult = Result<bool>.WithFailure("Task not found");

        _taskService.UpdateTaskStatusAsync(taskId, status, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _taskService.UpdateTaskStatusAsync(taskId, status);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

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
        var result = await _taskService.AssignTaskToAgentAsync(taskId, agentId);

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
        var result = await _taskService.AssignTaskToAgentAsync(taskId, agentId);

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
        var result = await _taskService.AssignTaskToAgentAsync(taskId, agentId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

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
        var result = await _taskService.CompleteTaskAsync(taskId, outputData);

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
        var result = await _taskService.CompleteTaskAsync(taskId);

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
        var result = await _taskService.CompleteTaskAsync(taskId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

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
        var result = await _taskService.FailTaskAsync(taskId, errorMessage);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
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
        var result = await _taskService.FailTaskAsync(taskId, errorMessage);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

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
        var result = await _taskService.GetOverdueTasksAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count().ShouldBe(2);
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
        var result = await _taskService.GetOverdueTasksAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldBeEmpty();
    }

    #endregion

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
        var result = await _taskService.DeleteTaskAsync(taskId);

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
        var result = await _taskService.DeleteTaskAsync(taskId);

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
        var result = await _taskService.DeleteTaskAsync(taskId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

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
            await _taskService.CreateTaskAsync("test", "test", "test", TaskPriority.Normal, null, cts.Token);
            await _taskService.GetTaskAsync(Guid.NewGuid(), cts.Token);
            await _taskService.GetPendingTasksAsync(100, cts.Token);
            await _taskService.GetAgentTasksAsync(Guid.NewGuid(), null, cts.Token);
            await _taskService.UpdateTaskStatusAsync(Guid.NewGuid(), ExxerAI.Domain.TaskStatus.Completed, cts.Token);
            await _taskService.AssignTaskToAgentAsync(Guid.NewGuid(), Guid.NewGuid(), cts.Token);
            await _taskService.CompleteTaskAsync(Guid.NewGuid(), null, cts.Token);
            await _taskService.FailTaskAsync(Guid.NewGuid(), "error", cts.Token);
            await _taskService.CancelTaskAsync(Guid.NewGuid(), cts.Token);
            await _taskService.GetOverdueTasksAsync(cts.Token);
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

    #endregion
} 