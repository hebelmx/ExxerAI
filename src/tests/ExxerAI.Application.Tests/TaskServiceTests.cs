using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;
using TaskStatus = ExxerAI.Domain.TaskStatus;

namespace ExxerAI.Application.Tests;

/// <summary>
/// Comprehensive unit tests for TaskService
/// </summary>
public class TaskServiceTests
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _agentRepository = Substitute.For<IAgentRepository>();
        _taskService = new TaskService(_taskRepository, _agentRepository);
    }

    #region Constructor Tests

    [Fact]
    public void Should_ThrowArgumentNullException_When_TaskRepositoryIsNull()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentNullException>(() => new TaskService(null!, _agentRepository))
            .ParamName.ShouldBe("taskRepository");
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_AgentRepositoryIsNull()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentNullException>(() => new TaskService(_taskRepository, null!))
            .ParamName.ShouldBe("agentRepository");
    }

    [Fact]
    public void Should_CreateInstance_When_ValidDependenciesProvided()
    {
        // Arrange, Act & Assert
        _taskService.ShouldNotBeNull();
    }

    #endregion

    #region CreateTaskAsync Tests

    [Fact]
    public async Task Should_CreateTask_When_ValidDataProvided()
    {
        // Arrange
        var title = "Test Task";
        var description = "Test task description";
        var taskType = "DataProcessing";
        var input = new TaskData { Content = "test input" };
        var priority = TaskPriority.High;
        var deadline = DateTime.UtcNow.AddDays(7);

        var expectedTask = new AgentTask
        {
            Title = title,
            Description = description,
            TaskType = taskType,
            Input = input,
            Priority = priority,
            Status = TaskStatus.Pending,
            Deadline = deadline
        };

        _taskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(expectedTask));

        // Act
        var result = await _taskService.CreateTaskAsync(title, description, taskType, input, priority, deadline);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Title.ShouldBe(title);
        result.Value.Description.ShouldBe(description);
        result.Value.TaskType.ShouldBe(taskType);
        result.Value.Priority.ShouldBe(priority);
        result.Value.Status.ShouldBe(TaskStatus.Pending);
        result.Value.Deadline.ShouldBe(deadline);

        await _taskRepository.Received(1).AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Should_ReturnFailure_When_TitleIsInvalid(string? invalidTitle)
    {
        // Arrange
        var taskType = "DataProcessing";
        var input = new TaskData();

        // Act
        var result = await _taskService.CreateTaskAsync(invalidTitle!, "description", taskType, input);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Task title cannot be null or empty");

        await _taskRepository.DidNotReceive().AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Should_ReturnFailure_When_TaskTypeIsInvalid(string? invalidTaskType)
    {
        // Arrange
        var title = "Valid Title";
        var input = new TaskData();

        // Act
        var result = await _taskService.CreateTaskAsync(title, "description", invalidTaskType!, input);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Task type cannot be null or empty");

        await _taskRepository.DidNotReceive().AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_CreateTaskWithDefaults_When_OptionalParametersNotProvided()
    {
        // Arrange
        var title = "Test Task";
        var taskType = "DataProcessing";
        var expectedTask = new AgentTask();

        _taskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(expectedTask));

        // Act
        var result = await _taskService.CreateTaskAsync(title, null, taskType, null);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await _taskRepository.Received(1).AddAsync(
            Arg.Is<AgentTask>(t => 
                t.Title == title &&
                t.Description == string.Empty &&
                t.TaskType == taskType &&
                t.Priority == TaskPriority.Normal &&
                t.Status == TaskStatus.Pending &&
                t.Deadline == null), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnFailure_When_RepositoryAddFails()
    {
        // Arrange
        var title = "Test Task";
        var taskType = "DataProcessing";
        var input = new TaskData();
        var repositoryError = "Repository error";

        _taskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.WithFailure(repositoryError));

        // Act
        var result = await _taskService.CreateTaskAsync(title, "description", taskType, input);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(repositoryError);
    }

    [Fact]
    public async Task Should_HandleException_When_RepositoryThrows()
    {
        // Arrange
        var title = "Test Task";
        var taskType = "DataProcessing";
        var input = new TaskData();

        _taskRepository
            .When(x => x.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()))
            .Do(x => throw new InvalidOperationException("Database error"));

        // Act
        var result = await _taskService.CreateTaskAsync(title, "description", taskType, input);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("An error occurred while creating the task"));
    }

    #endregion

    #region GetTaskAsync Tests

    [Fact]
    public async Task Should_GetTask_When_ValidIdProvided()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var expectedTask = new AgentTask { Id = taskId, Title = "Test Task" };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(expectedTask));

        // Act
        var result = await _taskService.GetTaskAsync(taskId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedTask);

        await _taskRepository.Received(1).GetByIdAsync(taskId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnFailure_When_TaskNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.WithFailure("Not found"));

        // Act
        var result = await _taskService.GetTaskAsync(taskId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Task with ID {taskId} not found");
    }

    #endregion

    #region GetPendingTasksAsync Tests

    [Fact]
    public async Task Should_GetPendingTasks_When_Called()
    {
        // Arrange
        var pendingTasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task1", Status = TaskStatus.Pending },
            new() { Id = Guid.NewGuid(), Title = "Task2", Status = TaskStatus.Pending }
        };

        _taskRepository.GetByStatusAsync(TaskStatus.Pending, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<AgentTask>>.Success(pendingTasks));

        // Act
        var result = await _taskService.GetPendingTasksAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(pendingTasks);

        await _taskRepository.Received(1).GetByStatusAsync(TaskStatus.Pending, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FilterPendingTasksByType_When_TaskTypeProvided()
    {
        // Arrange
        var taskType = "DataProcessing";
        var allPendingTasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task1", TaskType = "DataProcessing", Status = TaskStatus.Pending },
            new() { Id = Guid.NewGuid(), Title = "Task2", TaskType = "TextGeneration", Status = TaskStatus.Pending },
            new() { Id = Guid.NewGuid(), Title = "Task3", TaskType = "dataprocessing", Status = TaskStatus.Pending } // Case insensitive
        };

        _taskRepository.GetByStatusAsync(TaskStatus.Pending, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<AgentTask>>.Success(allPendingTasks));

        // Act
        var result = await _taskService.GetPendingTasksAsync(taskType);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Count().ShouldBe(2); // Should match case-insensitive
        result.Value.All(t => string.Equals(t.TaskType, taskType, StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_ReturnFailure_When_GetPendingTasksFails()
    {
        // Arrange
        var repositoryError = "Repository error";

        _taskRepository.GetByStatusAsync(TaskStatus.Pending, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<AgentTask>>.WithFailure(repositoryError));

        // Act
        var result = await _taskService.GetPendingTasksAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(repositoryError);
    }

    #endregion

    #region GetAgentTasksAsync Tests

    [Fact]
    public async Task Should_GetAgentTasks_When_ValidAgentIdProvided()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var agentTasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), AssignedAgentId = agentId },
            new() { Id = Guid.NewGuid(), AssignedAgentId = agentId }
        };

        _taskRepository.GetByAgentAsync(agentId, null, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<AgentTask>>.Success(agentTasks));

        // Act
        var result = await _taskService.GetAgentTasksAsync(agentId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(agentTasks);

        await _taskRepository.Received(1).GetByAgentAsync(agentId, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_GetAgentTasksByStatus_When_StatusProvided()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var status = TaskStatus.InProgress;
        var agentTasks = new List<AgentTask>();

        _taskRepository.GetByAgentAsync(agentId, status, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<AgentTask>>.Success(agentTasks));

        // Act
        var result = await _taskService.GetAgentTasksAsync(agentId, status);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        await _taskRepository.Received(1).GetByAgentAsync(agentId, status, Arg.Any<CancellationToken>());
    }

    #endregion

    #region StartTaskAsync Tests

    [Fact]
    public async Task Should_StartTask_When_TaskIsPending()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask { Id = taskId, Status = TaskStatus.Pending };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));
        
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _taskService.StartTaskAsync(taskId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        task.Status.ShouldBe(TaskStatus.InProgress);
        task.StartedAt.ShouldNotBeNull();
        task.StartedAt.Value.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));

        await _taskRepository.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnFailure_When_StartTaskNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.WithFailure("Not found"));

        // Act
        var result = await _taskService.StartTaskAsync(taskId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain($"Task with ID {taskId} not found");
    }

    [Theory]
    [InlineData(TaskStatus.InProgress)]
    [InlineData(TaskStatus.Completed)]
    [InlineData(TaskStatus.Failed)]
    [InlineData(TaskStatus.Cancelled)]
    public async Task Should_ReturnFailure_When_TaskNotInPendingStatus(TaskStatus currentStatus)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask { Id = taskId, Status = currentStatus };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));

        // Act
        var result = await _taskService.StartTaskAsync(taskId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("is not in Pending status");
    }

    #endregion

    #region CompleteTaskAsync Tests

    [Fact]
    public async Task Should_CompleteTask_When_TaskIsInProgress()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask { Id = taskId, Status = TaskStatus.InProgress };
        var output = new TaskData { Content = "task output" };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));
        
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _taskService.CompleteTaskAsync(taskId, output);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        task.Status.ShouldBe(TaskStatus.Completed);
        task.CompletedAt.ShouldNotBeNull();
        task.CompletedAt.Value.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        task.Output.ShouldBe(output);

        await _taskRepository.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_CompleteTaskWithEmptyOutput_When_OutputIsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask { Id = taskId, Status = TaskStatus.InProgress };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));
        
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _taskService.CompleteTaskAsync(taskId, null!);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        task.Status.ShouldBe(TaskStatus.Completed);
        task.Output.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(TaskStatus.Pending)]
    [InlineData(TaskStatus.Completed)]
    [InlineData(TaskStatus.Failed)]
    [InlineData(TaskStatus.Cancelled)]
    public async Task Should_ReturnFailure_When_TaskNotInProgressForComplete(TaskStatus currentStatus)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask { Id = taskId, Status = currentStatus };
        var output = new TaskData();

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));

        // Act
        var result = await _taskService.CompleteTaskAsync(taskId, output);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("is not in InProgress status");
    }

    #endregion

    #region FailTaskAsync Tests

    [Fact]
    public async Task Should_FailTask_When_TaskIsInProgress()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask { Id = taskId, Status = TaskStatus.InProgress };
        var errorMessage = "Task failed due to error";

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));
        
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _taskService.FailTaskAsync(taskId, errorMessage);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        task.Status.ShouldBe(TaskStatus.Failed);
        task.CompletedAt.ShouldNotBeNull();
        task.ErrorMessage.ShouldBe(errorMessage);

        await _taskRepository.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FailTaskWithDefaultMessage_When_ErrorMessageIsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask { Id = taskId, Status = TaskStatus.InProgress };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));
        
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _taskService.FailTaskAsync(taskId, null!);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        task.ErrorMessage.ShouldBe("Task failed without specific error message");
    }

    #endregion

    #region RetryTaskAsync Tests

    [Fact]
    public async Task Should_RetryTask_When_TaskIsFailed()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask 
        { 
            Id = taskId, 
            Status = TaskStatus.Failed,
            RetryCount = 2,
            ErrorMessage = "Previous error",
            StartedAt = DateTime.UtcNow.AddHours(-1),
            CompletedAt = DateTime.UtcNow.AddMinutes(-30)
        };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));
        
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _taskService.RetryTaskAsync(taskId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        task.Status.ShouldBe(TaskStatus.Pending);
        task.RetryCount.ShouldBe(3);
        task.ErrorMessage.ShouldBeNull();
        task.StartedAt.ShouldBeNull();
        task.CompletedAt.ShouldBeNull();

        await _taskRepository.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(TaskStatus.Pending)]
    [InlineData(TaskStatus.InProgress)]
    [InlineData(TaskStatus.Completed)]
    [InlineData(TaskStatus.Cancelled)]
    public async Task Should_ReturnFailure_When_TaskNotFailedForRetry(TaskStatus currentStatus)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask { Id = taskId, Status = currentStatus };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));

        // Act
        var result = await _taskService.RetryTaskAsync(taskId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("is not in Failed status");
    }

    #endregion

    #region CancelTaskAsync Tests

    [Theory]
    [InlineData(TaskStatus.Pending)]
    [InlineData(TaskStatus.InProgress)]
    [InlineData(TaskStatus.Failed)]
    public async Task Should_CancelTask_When_TaskIsCancellable(TaskStatus currentStatus)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask { Id = taskId, Status = currentStatus };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));
        
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _taskService.CancelTaskAsync(taskId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        task.Status.ShouldBe(TaskStatus.Cancelled);
        task.CompletedAt.ShouldNotBeNull();

        await _taskRepository.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(TaskStatus.Completed)]
    [InlineData(TaskStatus.Cancelled)]
    public async Task Should_ReturnFailure_When_TaskCannotBeCancelled(TaskStatus currentStatus)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask { Id = taskId, Status = currentStatus };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));

        // Act
        var result = await _taskService.CancelTaskAsync(taskId);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("cannot be cancelled");
    }

    #endregion

    #region UpdateTaskMetadataAsync Tests

    [Fact]
    public async Task Should_UpdateMetadata_When_ValidDataProvided()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask { Id = taskId };
        var metadata = new TaskMetadata();
        metadata.Properties["key"] = "value";

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));
        
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _taskService.UpdateTaskMetadataAsync(taskId, metadata);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        task.Metadata.ShouldBe(metadata);

        await _taskRepository.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_UpdateMetadataWithEmpty_When_MetadataIsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask { Id = taskId };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.Success(task));
        
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _taskService.UpdateTaskMetadataAsync(taskId, null!);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        task.Metadata.ShouldNotBeNull();
    }

    #endregion

    #region GetOverdueTasksAsync Tests

    [Fact]
    public async Task Should_GetOverdueTasks_When_Called()
    {
        // Arrange
        var overdueTasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Deadline = DateTime.UtcNow.AddDays(-1), Status = TaskStatus.Pending },
            new() { Id = Guid.NewGuid(), Deadline = DateTime.UtcNow.AddDays(-2), Status = TaskStatus.InProgress }
        };

        _taskRepository.GetOverdueTasksAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<AgentTask>>.Success(overdueTasks));

        // Act
        var result = await _taskService.GetOverdueTasksAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(overdueTasks);

        await _taskRepository.Received(1).GetOverdueTasksAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_HandleException_When_GetOverdueTasksThrows()
    {
        // Arrange
        _taskRepository
            .When(x => x.GetOverdueTasksAsync(Arg.Any<CancellationToken>()))
            .Do(x => throw new InvalidOperationException("Database error"));

        // Act
        var result = await _taskService.GetOverdueTasksAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Contains("An error occurred while retrieving overdue tasks"));
    }

    #endregion
}
