using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;
using TaskStatus = ExxerAI.Domain.TaskStatus;

namespace ExxerAI.Application.Tests;

public class TaskServiceTests
{
private readonly ITaskRepository _mockTaskRepository;
private readonly IAgentRepository _mockAgentRepository;
private readonly TaskService _taskService;

public TaskServiceTests()
{
_mockTaskRepository = Substitute.For<ITaskRepository>();
_mockAgentRepository = Substitute.For<IAgentRepository>();
_taskService = new TaskService(_mockTaskRepository, _mockAgentRepository);
}

[Fact]
public async Task Should_CreateTask_When_ValidDataProvided()
{
// Arrange
var title = "Test Task";
var taskType = "Analysis";
var input = new TaskData { Content = "Test input" };
_mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(new AgentTask()));

// Act
var result = await _taskService.CreateTaskAsync(title, "Description", taskType, input);

// Assert
result.IsSuccess.ShouldBeTrue();
await _mockTaskRepository.Received(1).AddAsync(Arg.Is<AgentTask>(t =>
t.Title == title && t.TaskType == taskType && t.Status == TaskStatus.Pending), 
Arg.Any<CancellationToken>());
}

[Fact]
public async Task Should_ReturnFailure_When_TitleIsNull()
{
// Act
var result = await _taskService.CreateTaskAsync(null!, "Description", "Analysis", new TaskData());

// Assert
result.IsFailure.ShouldBeTrue();
result.Errors.ShouldContain("Task title cannot be null or empty");
}

[Fact]
public async Task Should_ReturnFailure_When_TaskTypeIsNull()
{
// Act
var result = await _taskService.CreateTaskAsync("Title", "Description", null!, new TaskData());

// Assert
result.IsFailure.ShouldBeTrue();
result.Errors.ShouldContain("Task type cannot be null or empty");
}

[Fact]
public async Task Should_ReturnTask_When_TaskExists()
{
// Arrange
var taskId = Guid.NewGuid();
var task = new AgentTask { Id = taskId, Title = "Test Task" };
_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));

// Act
var result = await _taskService.GetTaskAsync(taskId);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBe(task);
}

[Fact]
public async Task Should_ReturnPendingTasks_When_NoTypeFilter()
{
// Arrange
var tasks = new[]
{
new AgentTask { Title = "Task 1", TaskType = "Analysis", Status = TaskStatus.Pending },
new AgentTask { Title = "Task 2", TaskType = "Processing", Status = TaskStatus.Pending }
};
_mockTaskRepository.GetByStatusAsync(TaskStatus.Pending, Arg.Any<CancellationToken>())
.Returns(Result<IEnumerable<AgentTask>>.Success(tasks));

// Act
var result = await _taskService.GetPendingTasksAsync();

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.Count().ShouldBe(2);
}

[Fact]
public async Task Should_FilterByTaskType_When_TypeFilterProvided()
{
// Arrange
var tasks = new[]
{
new AgentTask { Title = "Task 1", TaskType = "Analysis", Status = TaskStatus.Pending },
new AgentTask { Title = "Task 2", TaskType = "Processing", Status = TaskStatus.Pending }
};
_mockTaskRepository.GetByStatusAsync(TaskStatus.Pending, Arg.Any<CancellationToken>())
.Returns(Result<IEnumerable<AgentTask>>.Success(tasks));

// Act
var result = await _taskService.GetPendingTasksAsync("Analysis");

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.Count().ShouldBe(1);
result.Value.First().TaskType.ShouldBe("Analysis");
}

[Fact]
public async Task Should_StartTask_When_TaskIsPending()
{
// Arrange
var taskId = Guid.NewGuid();
var task = new AgentTask { Id = taskId, Status = TaskStatus.Pending };
_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));
_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));

// Act
var result = await _taskService.StartTaskAsync(taskId);

// Assert
result.IsSuccess.ShouldBeTrue();
task.Status.ShouldBe(TaskStatus.InProgress);
task.StartedAt.ShouldNotBeNull();
}

[Fact]
public async Task Should_ReturnFailure_When_TaskNotPending()
{
// Arrange
var taskId = Guid.NewGuid();
var task = new AgentTask { Id = taskId, Status = TaskStatus.InProgress };
_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));

// Act
var result = await _taskService.StartTaskAsync(taskId);

// Assert
result.IsFailure.ShouldBeTrue();
result.Errors.First().ShouldContain("is not in Pending status");
}

[Fact]
public async Task Should_CompleteTask_When_TaskIsInProgress()
{
// Arrange
var taskId = Guid.NewGuid();
var task = new AgentTask { Id = taskId, Status = TaskStatus.InProgress };
var output = new TaskData { Content = "Test output" };
_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));
_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));

// Act
var result = await _taskService.CompleteTaskAsync(taskId, output);

// Assert
result.IsSuccess.ShouldBeTrue();
task.Status.ShouldBe(TaskStatus.Completed);
task.CompletedAt.ShouldNotBeNull();
task.Output.ShouldBe(output);
}

[Fact]
public async Task Should_FailTask_When_TaskIsInProgress()
{
// Arrange
var taskId = Guid.NewGuid();
var task = new AgentTask { Id = taskId, Status = TaskStatus.InProgress };
var errorMessage = "Test error";
_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));
_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));

// Act
var result = await _taskService.FailTaskAsync(taskId, errorMessage);

// Assert
result.IsSuccess.ShouldBeTrue();
task.Status.ShouldBe(TaskStatus.Failed);
task.ErrorMessage.ShouldBe(errorMessage);
}

[Fact]
public async Task Should_RetryTask_When_TaskIsFailed()
{
// Arrange
var taskId = Guid.NewGuid();
var task = new AgentTask { Id = taskId, Status = TaskStatus.Failed, RetryCount = 1 };
_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));
_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));

// Act
var result = await _taskService.RetryTaskAsync(taskId);

// Assert
result.IsSuccess.ShouldBeTrue();
task.Status.ShouldBe(TaskStatus.Pending);
task.RetryCount.ShouldBe(2);
task.ErrorMessage.ShouldBeNull();
}

[Fact]
public async Task Should_CancelTask_When_TaskCanBeCancelled()
{
// Arrange
var taskId = Guid.NewGuid();
var task = new AgentTask { Id = taskId, Status = TaskStatus.Pending };
_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));
_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));

// Act
var result = await _taskService.CancelTaskAsync(taskId);

// Assert
result.IsSuccess.ShouldBeTrue();
task.Status.ShouldBe(TaskStatus.Cancelled);
task.CompletedAt.ShouldNotBeNull();
}

[Fact]
public async Task Should_ReturnFailure_When_TaskCannotBeCancelled()
{
// Arrange
var taskId = Guid.NewGuid();
var task = new AgentTask { Id = taskId, Status = TaskStatus.Completed };
_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));

// Act
var result = await _taskService.CancelTaskAsync(taskId);

// Assert
result.IsFailure.ShouldBeTrue();
result.Errors.First().ShouldContain("cannot be cancelled");
}

[Fact]
public async Task Should_UpdateMetadata_When_ValidTaskAndMetadata()
{
// Arrange
var taskId = Guid.NewGuid();
var task = new AgentTask { Id = taskId };
var metadata = new TaskMetadata();
metadata.Properties["key"] = "value";
_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));
_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
.Returns(Result<AgentTask>.Success(task));

// Act
var result = await _taskService.UpdateTaskMetadataAsync(taskId, metadata);

// Assert
result.IsSuccess.ShouldBeTrue();
task.Metadata.ShouldBe(metadata);
}

[Fact]
public async Task Should_ReturnOverdueTasks_When_ValidRequest()
{
// Arrange
var overdueTasks = new[]
{
new AgentTask { Title = "Overdue Task 1", Deadline = DateTime.UtcNow.AddDays(-1) },
new AgentTask { Title = "Overdue Task 2", Deadline = DateTime.UtcNow.AddDays(-2) }
};
_mockTaskRepository.GetOverdueTasksAsync(Arg.Any<CancellationToken>())
.Returns(Result<IEnumerable<AgentTask>>.Success(overdueTasks));

// Act
var result = await _taskService.GetOverdueTasksAsync();

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value.ShouldBe(overdueTasks);
}

[Fact]
public void Should_ThrowException_When_TaskRepositoryIsNull()
{
// Arrange & Act & Assert
Should.Throw<ArgumentNullException>(() => new TaskService(null!, Substitute.For<IAgentRepository>()));
}

[Fact]
public void Should_ThrowException_When_AgentRepositoryIsNull()
{
// Arrange & Act & Assert
Should.Throw<ArgumentNullException>(() => new TaskService(Substitute.For<ITaskRepository>(), null!));
}
}
