using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests;

/// <summary>
/// Comprehensive ITDD test bed for ITaskService
/// Tests the interface contract for task management operations
/// </summary>
public class TaskServiceTests
{
    private readonly ITaskService _taskService;
    private readonly CancellationToken _cancellationToken;

    public TaskServiceTests()
    {
        _taskService = Substitute.For<ITaskService>();
        _cancellationToken = TestContext.Current.CancellationToken;
    }

    public class CreateTaskAsyncTests : TaskServiceTests
    {
        [Fact]
        public async Task Should_ReturnCreatedTask_When_ValidParametersProvided()
        {
            // Arrange
            var title = "Test Task";
            var description = "Test task description";
            var taskType = "DocumentProcessing";
            var priority = TaskPriority.High;
            var deadline = DateTime.UtcNow.AddDays(7);
            var expectedTask = CreateValidAgentTask();

            _taskService.CreateTaskAsync(title, description, taskType, priority, deadline, _cancellationToken)
                .Returns(Result<AgentTask>.Success(expectedTask));

            // Act
            var result = await _taskService.CreateTaskAsync(title, description, taskType, priority, deadline, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.Title.ShouldBe(title);
            result.Value!.Description.ShouldBe(description);
            result.Value!.TaskType.ShouldBe(taskType);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public async Task Should_ReturnFailure_When_InvalidTitleProvided(string? invalidTitle)
        {
            // Arrange
            var description = "Test task description";
            var taskType = "DocumentProcessing";

            _taskService.CreateTaskAsync(invalidTitle!, description, taskType, TaskPriority.Normal, null, _cancellationToken)
                .Returns(Result<AgentTask>.WithFailure("Task title cannot be empty"));

            // Act
            var result = await _taskService.CreateTaskAsync(invalidTitle!, description, taskType, TaskPriority.Normal, null, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Task title cannot be empty");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public async Task Should_ReturnFailure_When_InvalidDescriptionProvided(string? invalidDescription)
        {
            // Arrange
            var title = "Test Task";
            var taskType = "DocumentProcessing";

            _taskService.CreateTaskAsync(title, invalidDescription!, taskType, TaskPriority.Normal, null, _cancellationToken)
                .Returns(Result<AgentTask>.WithFailure("Task description cannot be empty"));

            // Act
            var result = await _taskService.CreateTaskAsync(title, invalidDescription!, taskType, TaskPriority.Normal, null, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Task description cannot be empty");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public async Task Should_ReturnFailure_When_InvalidTaskTypeProvided(string? invalidTaskType)
        {
            // Arrange
            var title = "Test Task";
            var description = "Test task description";

            _taskService.CreateTaskAsync(title, description, invalidTaskType!, TaskPriority.Normal, null, _cancellationToken)
                .Returns(Result<AgentTask>.WithFailure("Task type cannot be empty"));

            // Act
            var result = await _taskService.CreateTaskAsync(title, description, invalidTaskType!, TaskPriority.Normal, null, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Task type cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_DeadlineInPast()
        {
            // Arrange
            var title = "Test Task";
            var description = "Test task description";
            var taskType = "DocumentProcessing";
            var pastDeadline = DateTime.UtcNow.AddDays(-1);

            _taskService.CreateTaskAsync(title, description, taskType, TaskPriority.Normal, pastDeadline, _cancellationToken)
                .Returns(Result<AgentTask>.WithFailure("Deadline cannot be in the past"));

            // Act
            var result = await _taskService.CreateTaskAsync(title, description, taskType, TaskPriority.Normal, pastDeadline, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Deadline cannot be in the past");
        }
    }

    public class GetTaskAsyncTests : TaskServiceTests
    {
        [Fact]
        public async Task Should_ReturnTask_When_ValidTaskIdProvided()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var expectedTask = CreateValidAgentTask();

            _taskService.GetTaskAsync(taskId, _cancellationToken)
                .Returns(Result<AgentTask>.Success(expectedTask));

            // Act
            var result = await _taskService.GetTaskAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.Id.ShouldBe(expectedTask.Id);
        }

        [Fact]
        public async Task Should_ReturnNotFound_When_TaskDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            _taskService.GetTaskAsync(nonExistentId, _cancellationToken)
                .Returns(Result<AgentTask>.WithFailure("Task not found"));

            // Act
            var result = await _taskService.GetTaskAsync(nonExistentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Task not found");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_EmptyGuidProvided()
        {
            // Arrange
            var emptyId = Guid.Empty;

            _taskService.GetTaskAsync(emptyId, _cancellationToken)
                .Returns(Result<AgentTask>.WithFailure("Task ID cannot be empty"));

            // Act
            var result = await _taskService.GetTaskAsync(emptyId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Task ID cannot be empty");
        }
    }

    public class GetPendingTasksAsyncTests : TaskServiceTests
    {
        [Fact]
        public async Task Should_ReturnPendingTasks_When_Called()
        {
            // Arrange
            var expectedTasks = CreatePendingTasks();

            _taskService.GetPendingTasksAsync(100, _cancellationToken)
                .Returns(Result<IEnumerable<AgentTask>>.Success(expectedTasks));

            // Act
            var result = await _taskService.GetPendingTasksAsync(100, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value?.Count().ShouldBeGreaterThan(0);
            result.Value!.All(t => t.AgentStatus == TaskAgentStatus.Pending).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnLimitedTasks_When_MaxCountSpecified()
        {
            // Arrange
            var maxCount = 5;
            var expectedTasks = CreatePendingTasks().Take(maxCount);

            _taskService.GetPendingTasksAsync(maxCount, _cancellationToken)
                .Returns(Result<IEnumerable<AgentTask>>.Success(expectedTasks));

            // Act
            var result = await _taskService.GetPendingTasksAsync(maxCount, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value?.Count().ShouldBeLessThanOrEqualTo(maxCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task Should_ReturnFailure_When_InvalidMaxCountProvided(int invalidCount)
        {
            // Arrange
            _taskService.GetPendingTasksAsync(invalidCount, _cancellationToken)
                .Returns(Result<IEnumerable<AgentTask>>.WithFailure("Max count must be greater than zero"));

            // Act
            var result = await _taskService.GetPendingTasksAsync(invalidCount, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Max count must be greater than zero");
        }
    }

    public class GetAgentTasksAsyncTests : TaskServiceTests
    {
        [Fact]
        public async Task Should_ReturnAgentTasks_When_ValidAgentIdProvided()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var expectedTasks = CreateAgentTasks(agentId);

            _taskService.GetAgentTasksAsync(agentId, null, _cancellationToken)
                .Returns(Result<IEnumerable<AgentTask>>.Success(expectedTasks));

            // Act
            var result = await _taskService.GetAgentTasksAsync(agentId, null, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.All(t => t.AssignedAgentId == agentId).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnFilteredTasks_When_StatusFilterProvided()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var status = TaskAgentStatus.InProgress;
            var expectedTasks = CreateAgentTasks(agentId).Where(t => t.AgentStatus == status);

            _taskService.GetAgentTasksAsync(agentId, status, _cancellationToken)
                .Returns(Result<IEnumerable<AgentTask>>.Success(expectedTasks));

            // Act
            var result = await _taskService.GetAgentTasksAsync(agentId, status, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.All(t => t.AgentStatus == status).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnFailure_When_EmptyAgentIdProvided()
        {
            // Arrange
            var emptyId = Guid.Empty;

            _taskService.GetAgentTasksAsync(emptyId, null, _cancellationToken)
                .Returns(Result<IEnumerable<AgentTask>>.WithFailure("Agent ID cannot be empty"));

            // Act
            var result = await _taskService.GetAgentTasksAsync(emptyId, null, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Agent ID cannot be empty");
        }
    }

    public class UpdateTaskStatusAsyncTests : TaskServiceTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidParametersProvided()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var newStatus = TaskAgentStatus.InProgress;

            _taskService.UpdateTaskStatusAsync(taskId, newStatus, _cancellationToken)
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _taskService.UpdateTaskStatusAsync(taskId, newStatus, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnFailure_When_EmptyTaskIdProvided()
        {
            // Arrange
            var emptyId = Guid.Empty;
            var newStatus = TaskAgentStatus.InProgress;

            _taskService.UpdateTaskStatusAsync(emptyId, newStatus, _cancellationToken)
                .Returns(Result<bool>.WithFailure("Task ID cannot be empty"));

            // Act
            var result = await _taskService.UpdateTaskStatusAsync(emptyId, newStatus, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Task ID cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_TaskNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var newStatus = TaskAgentStatus.InProgress;

            _taskService.UpdateTaskStatusAsync(nonExistentId, newStatus, _cancellationToken)
                .Returns(Result<bool>.WithFailure("Task not found"));

            // Act
            var result = await _taskService.UpdateTaskStatusAsync(nonExistentId, newStatus, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Task not found");
        }
    }

    public class AssignTaskToAgentAsyncTests : TaskServiceTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidParametersProvided()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var agentId = Guid.NewGuid();

            _taskService.AssignTaskToAgentAsync(taskId, agentId, _cancellationToken)
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _taskService.AssignTaskToAgentAsync(taskId, agentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnFailure_When_EmptyTaskIdProvided()
        {
            // Arrange
            var emptyTaskId = Guid.Empty;
            var agentId = Guid.NewGuid();

            _taskService.AssignTaskToAgentAsync(emptyTaskId, agentId, _cancellationToken)
                .Returns(Result<bool>.WithFailure("Task ID cannot be empty"));

            // Act
            var result = await _taskService.AssignTaskToAgentAsync(emptyTaskId, agentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Task ID cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_EmptyAgentIdProvided()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var emptyAgentId = Guid.Empty;

            _taskService.AssignTaskToAgentAsync(taskId, emptyAgentId, _cancellationToken)
                .Returns(Result<bool>.WithFailure("Agent ID cannot be empty"));

            // Act
            var result = await _taskService.AssignTaskToAgentAsync(taskId, emptyAgentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Agent ID cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_TaskAlreadyAssigned()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var agentId = Guid.NewGuid();

            _taskService.AssignTaskToAgentAsync(taskId, agentId, _cancellationToken)
                .Returns(Result<bool>.WithFailure("Task is already assigned to another agent"));

            // Act
            var result = await _taskService.AssignTaskToAgentAsync(taskId, agentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Task is already assigned to another agent");
        }
    }

    public class CompleteTaskAsyncTests : TaskServiceTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidTaskIdProvided()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var outputData = CreateValidTaskData();

            _taskService.CompleteTaskAsync(taskId, outputData, _cancellationToken)
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _taskService.CompleteTaskAsync(taskId, outputData, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnSuccess_When_NoOutputDataProvided()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _taskService.CompleteTaskAsync(taskId, null, _cancellationToken)
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _taskService.CompleteTaskAsync(taskId, null, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnFailure_When_TaskNotInProgress()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var outputData = CreateValidTaskData();

            _taskService.CompleteTaskAsync(taskId, outputData, _cancellationToken)
                .Returns(Result<bool>.WithFailure("Task is not in progress"));

            // Act
            var result = await _taskService.CompleteTaskAsync(taskId, outputData, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Task is not in progress");
        }
    }

    public class FailTaskAsyncTests : TaskServiceTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidParametersProvided()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var errorMessage = "Processing failed due to invalid input";

            _taskService.FailTaskAsync(taskId, errorMessage, _cancellationToken)
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _taskService.FailTaskAsync(taskId, errorMessage, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public async Task Should_ReturnFailure_When_InvalidErrorMessageProvided(string? invalidMessage)
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _taskService.FailTaskAsync(taskId, invalidMessage!, _cancellationToken)
                .Returns(Result<bool>.WithFailure("Error message cannot be empty"));

            // Act
            var result = await _taskService.FailTaskAsync(taskId, invalidMessage!, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Error message cannot be empty");
        }
    }

    public class CancelTaskAsyncTests : TaskServiceTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidTaskIdProvided()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _taskService.CancelTaskAsync(taskId, _cancellationToken)
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _taskService.CancelTaskAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnFailure_When_TaskCannotBeCancelled()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _taskService.CancelTaskAsync(taskId, _cancellationToken)
                .Returns(Result<bool>.WithFailure("Task cannot be cancelled in current state"));

            // Act
            var result = await _taskService.CancelTaskAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Task cannot be cancelled in current state");
        }
    }

    public class GetOverdueTasksAsyncTests : TaskServiceTests
    {
        [Fact]
        public async Task Should_ReturnOverdueTasks_When_Called()
        {
            // Arrange
            var expectedTasks = CreateOverdueTasks();

            _taskService.GetOverdueTasksAsync(_cancellationToken)
                .Returns(Result<IEnumerable<AgentTask>>.Success(expectedTasks));

            // Act
            var result = await _taskService.GetOverdueTasksAsync(cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.All(t => t.Deadline < DateTime.UtcNow).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnEmptyList_When_NoOverdueTasks()
        {
            // Arrange
            var emptyTasks = Array.Empty<AgentTask>();

            _taskService.GetOverdueTasksAsync(_cancellationToken)
                .Returns(Result<IEnumerable<AgentTask>>.Success(emptyTasks));

            // Act
            var result = await _taskService.GetOverdueTasksAsync(cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value?.Count().ShouldBe(0);
        }
    }

    // Test Value Factory Methods
    private static AgentTask CreateValidAgentTask()
    {
        return new AgentTask
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Test task description",
            TaskType = "DocumentProcessing",
            Priority = TaskPriority.Normal,
            AgentStatus = TaskAgentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Deadline = DateTime.UtcNow.AddDays(7),
            AssignedAgentId = null,
            Input = CreateValidTaskData(),
            Output = null!
        };
    }

    private static TaskData CreateValidTaskData()
    {
        return new TaskData
        {
            Properties = new Dictionary<string, object>
            {
                { "DocumentId", "doc-123" },
                { "ProcessingType", "OCR" },
                { "Priority", "High" }
            }
        };
    }

    private static IEnumerable<AgentTask> CreatePendingTasks()
    {
        return new[]
        {
            new AgentTask
            {
                Id = Guid.NewGuid(),
                Title = "Pending Task 1",
                Description = "First pending task",
                TaskType = "DocumentProcessing",
                Priority = TaskPriority.Normal,
                AgentStatus = TaskAgentStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                Deadline = DateTime.UtcNow.AddDays(1)
            },
            new AgentTask
            {
                Id = Guid.NewGuid(),
                Title = "Pending Task 2",
                Description = "Second pending task",
                TaskType = "DataValidation",
                Priority = TaskPriority.High,
                AgentStatus = TaskAgentStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-15),
                Deadline = DateTime.UtcNow.AddHours(12)
            }
        };
    }

    private static IEnumerable<AgentTask> CreateAgentTasks(Guid? agentId = null)
    {
        var actualAgentId = agentId ?? Guid.NewGuid();
        return new[]
        {
            new AgentTask
            {
                Id = Guid.NewGuid(),
                Title = "Agent Task 1",
                Description = "First agent task",
                TaskType = "DocumentProcessing",
                Priority = TaskPriority.Normal,
                AgentStatus = TaskAgentStatus.InProgress,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                AssignedAgentId = actualAgentId,
                Deadline = DateTime.UtcNow.AddDays(2)
            },
            new AgentTask
            {
                Id = Guid.NewGuid(),
                Title = "Agent Task 2",
                Description = "Second agent task",
                TaskType = "DataValidation",
                Priority = TaskPriority.Low,
                AgentStatus = TaskAgentStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddHours(-4),
                AssignedAgentId = actualAgentId,
                Deadline = DateTime.UtcNow.AddDays(1)
            }
        };
    }

    private static IEnumerable<AgentTask> CreateOverdueTasks()
    {
        return new[]
        {
            new AgentTask
            {
                Id = Guid.NewGuid(),
                Title = "Overdue Task 1",
                Description = "First overdue task",
                TaskType = "DocumentProcessing",
                Priority = TaskPriority.High,
                AgentStatus = TaskAgentStatus.InProgress,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Deadline = DateTime.UtcNow.AddDays(-1),
                AssignedAgentId = Guid.NewGuid()
            },
            new AgentTask
            {
                Id = Guid.NewGuid(),
                Title = "Overdue Task 2",
                Description = "Second overdue task",
                TaskType = "DataValidation",
                Priority = TaskPriority.Normal,
                AgentStatus = TaskAgentStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Deadline = DateTime.UtcNow.AddHours(-6)
            }
        };
    }
}