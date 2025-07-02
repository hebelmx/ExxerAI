using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using Microsoft.Extensions.Logging;
using Meziantou.Extensions.Logging.Xunit;
using NSubstitute;
using Shouldly;
using Xunit.Abstractions;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Real implementation tests for TaskService - testing actual business logic, not interface contracts
/// Targeting 311 lines of complex business logic to kill NoCoverage mutants
/// Tests all 11 public methods with comprehensive mutation hunting coverage
/// </summary>
public class TaskServiceImplementationTests
{
	private readonly ITaskRepository _mockTaskRepository;
	private readonly IAgentRepository _mockAgentRepository;
	private readonly TaskService _taskService;

	public TaskServiceImplementationTests(ITestOutputHelper testOutputHelper)
	{
		_mockTaskRepository = Substitute.For<ITaskRepository>();
		_mockAgentRepository = Substitute.For<IAgentRepository>();

		// Create REAL implementation, not interface mock
		_taskService = new TaskService(_mockTaskRepository, _mockAgentRepository);
	}

	[Fact]
	public void Constructor_Should_InitializeWithValidDependencies_When_AllParametersProvided()
	{
		// Arrange & Act
		var service = new TaskService(_mockTaskRepository, _mockAgentRepository);

		// Assert
		service.ShouldNotBeNull();
	}

	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_TaskRepositoryIsNull()
	{
		// Arrange & Act & Assert
		Should.Throw<ArgumentNullException>(() => new TaskService(null!, _mockAgentRepository))
			.ParamName.ShouldBe("taskRepository");
	}

	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_AgentRepositoryIsNull()
	{
		// Arrange & Act & Assert
		Should.Throw<ArgumentNullException>(() => new TaskService(_mockTaskRepository, null!))
			.ParamName.ShouldBe("agentRepository");
	}

	[Fact]
	public async Task CreateTaskAsync_Should_CreateTask_When_ValidParametersProvided()
	{
		// Arrange
		var title = "Test Task";
		var description = "Test Description";
		var taskType = "DocumentProcessing";
		var priority = TaskPriority.High;
		var deadline = DateTime.UtcNow.AddDays(7);

		var createdTask = new AgentTask
		{
			Id = Guid.NewGuid(),
			Title = title,
			Description = description,
			TaskType = taskType,
			Priority = priority,
			Status = Domain.TaskStatus.Pending,
			Deadline = deadline,
			CreatedAt = DateTime.UtcNow
		};

		_mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(createdTask)));

		// Act
		var result = await _taskService.CreateTaskAsync(title, description, taskType, priority, deadline);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.Title.ShouldBe(title);
		result.Value.Description.ShouldBe(description);
		result.Value.TaskType.ShouldBe(taskType);
		result.Value.Priority.ShouldBe(priority);
		result.Value.Status.ShouldBe(Domain.TaskStatus.Pending);
		result.Value.Deadline.ShouldBe(deadline);

		await _mockTaskRepository.Received(1).AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData("", "Valid Description", "ValidType")]
	[InlineData("   ", "Valid Description", "ValidType")]
	[InlineData(null, "Valid Description", "ValidType")]
	public async Task CreateTaskAsync_Should_ReturnFailure_When_TitleIsNullOrEmpty(string title, string description, string taskType)
	{
		// Arrange & Act
		var result = await _taskService.CreateTaskAsync(title, description, taskType);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe("Task title cannot be null or empty");

		await _mockTaskRepository.DidNotReceive().AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData("Valid Title", "Valid Description", "")]
	[InlineData("Valid Title", "Valid Description", "   ")]
	[InlineData("Valid Title", "Valid Description", null)]
	public async Task CreateTaskAsync_Should_ReturnFailure_When_TaskTypeIsNullOrEmpty(string title, string description, string taskType)
	{
		// Arrange & Act
		var result = await _taskService.CreateTaskAsync(title, description, taskType);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe("Task type cannot be null or empty");

		await _mockTaskRepository.DidNotReceive().AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateTaskAsync_Should_HandleNullDescription_When_DescriptionIsNull()
	{
		// Arrange
		var title = "Test Task";
		string? description = null;
		var taskType = "TestType";

		var createdTask = new AgentTask { Id = Guid.NewGuid(), Title = title };

		_mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(createdTask)));

		// Act
		var result = await _taskService.CreateTaskAsync(title, description, taskType);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		
		await _mockTaskRepository.Received(1).AddAsync(Arg.Is<AgentTask>(t => t.Description == string.Empty), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(TaskPriority.Low)]
	[InlineData(TaskPriority.Normal)]
	[InlineData(TaskPriority.High)]
	[InlineData(TaskPriority.Critical)]
	public async Task CreateTaskAsync_Should_SetCorrectPriority_When_DifferentPrioritiesProvided(TaskPriority priority)
	{
		// Arrange
		var title = "Test Task";
		var description = "Test Description";
		var taskType = "TestType";

		var createdTask = new AgentTask { Id = Guid.NewGuid(), Priority = priority };

		_mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(createdTask)));

		// Act
		var result = await _taskService.CreateTaskAsync(title, description, taskType, priority);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.Priority.ShouldBe(priority);
	}

	[Fact]
	public async Task CreateTaskAsync_Should_ReturnFailure_When_RepositoryFails()
	{
		// Arrange
		var title = "Test Task";
		var description = "Test Description";
		var taskType = "TestType";

		_mockTaskRepository.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Failure("Database error")));

		// Act
		var result = await _taskService.CreateTaskAsync(title, description, taskType);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe("Database error");
	}

	[Fact]
	public async Task CreateTaskAsync_Should_ReturnFailure_When_RepositoryThrowsException()
	{
		// Arrange
		var title = "Test Task";
		var description = "Test Description";
		var taskType = "TestType";

		_mockTaskRepository.When(x => x.AddAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>()))
			.Do(x => throw new InvalidOperationException("Repository connection failed"));

		// Act
		var result = await _taskService.CreateTaskAsync(title, description, taskType);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldContain("An error occurred while creating the task: Repository connection failed");
	}

	[Fact]
	public async Task GetTaskAsync_Should_ReturnTask_When_TaskExists()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var existingTask = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Existing Task",
			TaskType = "TestType",
			Status = Domain.TaskStatus.Pending
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(existingTask)));

		// Act
		var result = await _taskService.GetTaskAsync(taskId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.Id.ShouldBe(taskId);
		result.Value.Title.ShouldBe("Existing Task");

		await _mockTaskRepository.Received(1).GetByIdAsync(taskId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetTaskAsync_Should_ReturnFailure_When_TaskNotFound()
	{
		// Arrange
		var taskId = Guid.NewGuid();

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Failure("Task not found")));

		// Act
		var result = await _taskService.GetTaskAsync(taskId);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe($"Task with ID {taskId} not found");
	}

	[Fact]
	public async Task GetTaskAsync_Should_ReturnFailure_When_RepositoryThrowsException()
	{
		// Arrange
		var taskId = Guid.NewGuid();

		_mockTaskRepository.When(x => x.GetByIdAsync(taskId, Arg.Any<CancellationToken>()))
			.Do(x => throw new TimeoutException("Database timeout"));

		// Act
		var result = await _taskService.GetTaskAsync(taskId);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldContain("An error occurred while retrieving the task: Database timeout");
	}

	[Fact]
	public async Task GetPendingTasksAsync_Should_ReturnPendingTasks_When_TasksExist()
	{
		// Arrange
		var maxCount = 50;
		var pendingTasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "Task1", Status = Domain.TaskStatus.Pending },
			new() { Id = Guid.NewGuid(), Title = "Task2", Status = Domain.TaskStatus.Pending },
			new() { Id = Guid.NewGuid(), Title = "Task3", Status = Domain.TaskStatus.Pending }
		};

		_mockTaskRepository.GetByStatusAsync(Domain.TaskStatus.Pending, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Success(pendingTasks)));

		// Act
		var result = await _taskService.GetPendingTasksAsync(maxCount);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.Count().ShouldBe(3);
		result.Value.All(t => t.Status == Domain.TaskStatus.Pending).ShouldBeTrue();

		await _mockTaskRepository.Received(1).GetByStatusAsync(Domain.TaskStatus.Pending, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetPendingTasksAsync_Should_LimitResults_When_MoreTasksThanMaxCount()
	{
		// Arrange
		var maxCount = 2;
		var pendingTasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "Task1", Status = Domain.TaskStatus.Pending },
			new() { Id = Guid.NewGuid(), Title = "Task2", Status = Domain.TaskStatus.Pending },
			new() { Id = Guid.NewGuid(), Title = "Task3", Status = Domain.TaskStatus.Pending },
			new() { Id = Guid.NewGuid(), Title = "Task4", Status = Domain.TaskStatus.Pending }
		};

		_mockTaskRepository.GetByStatusAsync(Domain.TaskStatus.Pending, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Success(pendingTasks)));

		// Act
		var result = await _taskService.GetPendingTasksAsync(maxCount);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.Count().ShouldBe(2); // Limited to maxCount
	}

	[Fact]
	public async Task GetPendingTasksAsync_Should_UseDefaultMaxCount_When_NoMaxCountProvided()
	{
		// Arrange
		var pendingTasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "Task1", Status = Domain.TaskStatus.Pending }
		};

		_mockTaskRepository.GetByStatusAsync(Domain.TaskStatus.Pending, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Success(pendingTasks)));

		// Act
		var result = await _taskService.GetPendingTasksAsync(); // No maxCount provided

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.Count().ShouldBe(1);
	}

	[Fact]
	public async Task GetPendingTasksAsync_Should_ReturnEmptyCollection_When_NoTasksFound()
	{
		// Arrange
		var emptyTasks = new List<AgentTask>();

		_mockTaskRepository.GetByStatusAsync(Domain.TaskStatus.Pending, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Success(emptyTasks)));

		// Act
		var result = await _taskService.GetPendingTasksAsync();

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.ShouldBeEmpty();
	}

	[Fact]
	public async Task GetPendingTasksAsync_Should_ReturnFailure_When_RepositoryFails()
	{
		// Arrange
		_mockTaskRepository.GetByStatusAsync(Domain.TaskStatus.Pending, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Failure("Database connection failed")));

		// Act
		var result = await _taskService.GetPendingTasksAsync();

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe("Database connection failed");
	}

	[Fact]
	public async Task GetPendingTasksAsync_Should_ReturnFailure_When_RepositoryThrowsException()
	{
		// Arrange
		_mockTaskRepository.When(x => x.GetByStatusAsync(Domain.TaskStatus.Pending, Arg.Any<CancellationToken>()))
			.Do(x => throw new ArgumentException("Invalid status parameter"));

		// Act
		var result = await _taskService.GetPendingTasksAsync();

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldContain("An error occurred while retrieving pending tasks: Invalid status parameter");
	}

	[Fact]
	public async Task GetAgentTasksAsync_Should_ReturnAgentTasks_When_TasksExist()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var agentTasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "Task1", AssignedAgentId = agentId, Status = Domain.TaskStatus.InProgress },
			new() { Id = Guid.NewGuid(), Title = "Task2", AssignedAgentId = agentId, Status = Domain.TaskStatus.Completed }
		};

		_mockTaskRepository.GetByAgentAsync(agentId, null, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Success(agentTasks)));

		// Act
		var result = await _taskService.GetAgentTasksAsync(agentId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.Count().ShouldBe(2);
		result.Value.All(t => t.AssignedAgentId == agentId).ShouldBeTrue();

		await _mockTaskRepository.Received(1).GetByAgentAsync(agentId, null, Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(Domain.TaskStatus.Pending)]
	[InlineData(Domain.TaskStatus.InProgress)]
	[InlineData(Domain.TaskStatus.Completed)]
	[InlineData(Domain.TaskStatus.Failed)]
	[InlineData(Domain.TaskStatus.Cancelled)]
	public async Task GetAgentTasksAsync_Should_FilterByStatus_When_StatusProvided(Domain.TaskStatus status)
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var filteredTasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "Task1", AssignedAgentId = agentId, Status = status }
		};

		_mockTaskRepository.GetByAgentAsync(agentId, status, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Success(filteredTasks)));

		// Act
		var result = await _taskService.GetAgentTasksAsync(agentId, status);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.All(t => t.Status == status).ShouldBeTrue();

		await _mockTaskRepository.Received(1).GetByAgentAsync(agentId, status, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetAgentTasksAsync_Should_ReturnFailure_When_RepositoryFails()
	{
		// Arrange
		var agentId = Guid.NewGuid();

		_mockTaskRepository.GetByAgentAsync(agentId, null, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Failure("Agent not found")));

		// Act
		var result = await _taskService.GetAgentTasksAsync(agentId);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe("Agent not found");
	}

	[Fact]
	public async Task UpdateTaskStatusAsync_Should_UpdateStatus_When_TaskExists()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var newStatus = Domain.TaskStatus.InProgress;
		var existingTask = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Test Task",
			Status = Domain.TaskStatus.Pending
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(existingTask)));
		_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(existingTask)));

		// Act
		var result = await _taskService.UpdateTaskStatusAsync(taskId, newStatus);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBeTrue();

		await _mockTaskRepository.Received(1).GetByIdAsync(taskId, Arg.Any<CancellationToken>());
		await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => t.Status == newStatus), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UpdateTaskStatusAsync_Should_ReturnFailure_When_TaskNotFound()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var newStatus = Domain.TaskStatus.InProgress;

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Failure("Task not found")));

		// Act
		var result = await _taskService.UpdateTaskStatusAsync(taskId, newStatus);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe($"Task with ID {taskId} not found");

		await _mockTaskRepository.DidNotReceive().UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AssignTaskToAgentAsync_Should_AssignTask_When_TaskIsPending()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var agentId = Guid.NewGuid();
		var pendingTask = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Test Task",
			Status = Domain.TaskStatus.Pending
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(pendingTask)));
		_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(pendingTask)));

		// Act
		var result = await _taskService.AssignTaskToAgentAsync(taskId, agentId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBeTrue();

		await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => t.AssignedAgentId == agentId), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(Domain.TaskStatus.InProgress)]
	[InlineData(Domain.TaskStatus.Completed)]
	[InlineData(Domain.TaskStatus.Failed)]
	[InlineData(Domain.TaskStatus.Cancelled)]
	public async Task AssignTaskToAgentAsync_Should_ReturnFailure_When_TaskNotPending(Domain.TaskStatus status)
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var agentId = Guid.NewGuid();
		var nonPendingTask = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Test Task",
			Status = status
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(nonPendingTask)));

		// Act
		var result = await _taskService.AssignTaskToAgentAsync(taskId, agentId);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe($"Task {taskId} is not available for assignment. Current status: {status}");

		await _mockTaskRepository.DidNotReceive().UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CompleteTaskAsync_Should_CompleteTask_When_TaskIsInProgress()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var outputData = new TaskData { Properties = { ["result"] = "success" } };
		var inProgressTask = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Test Task",
			Status = Domain.TaskStatus.InProgress
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(inProgressTask)));
		_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(inProgressTask)));

		// Act
		var result = await _taskService.CompleteTaskAsync(taskId, outputData);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBeTrue();

		await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => 
			t.Status == Domain.TaskStatus.Completed &&
			t.CompletedAt.HasValue &&
			t.Output == outputData), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CompleteTaskAsync_Should_CompleteTaskWithEmptyOutput_When_NoOutputDataProvided()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var inProgressTask = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Test Task",
			Status = Domain.TaskStatus.InProgress
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(inProgressTask)));
		_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(inProgressTask)));

		// Act
		var result = await _taskService.CompleteTaskAsync(taskId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBeTrue();

		await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => 
			t.Status == Domain.TaskStatus.Completed &&
			t.Output != null), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(Domain.TaskStatus.Pending)]
	[InlineData(Domain.TaskStatus.Completed)]
	[InlineData(Domain.TaskStatus.Failed)]
	[InlineData(Domain.TaskStatus.Cancelled)]
	public async Task CompleteTaskAsync_Should_ReturnFailure_When_TaskNotInProgress(Domain.TaskStatus status)
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var task = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Test Task",
			Status = status
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(task)));

		// Act
		var result = await _taskService.CompleteTaskAsync(taskId);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe($"Task {taskId} is not in InProgress status. Current status: {status}");

		await _mockTaskRepository.DidNotReceive().UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task FailTaskAsync_Should_FailTask_When_TaskIsInProgress()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var errorMessage = "Task failed due to network error";
		var inProgressTask = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Test Task",
			Status = Domain.TaskStatus.InProgress
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(inProgressTask)));
		_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(inProgressTask)));

		// Act
		var result = await _taskService.FailTaskAsync(taskId, errorMessage);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBeTrue();

		await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => 
			t.Status == Domain.TaskStatus.Failed &&
			t.CompletedAt.HasValue &&
			t.ErrorMessage == errorMessage), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task FailTaskAsync_Should_HandleNullErrorMessage_When_ErrorMessageIsNull()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		string? errorMessage = null;
		var inProgressTask = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Test Task",
			Status = Domain.TaskStatus.InProgress
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(inProgressTask)));
		_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(inProgressTask)));

		// Act
		var result = await _taskService.FailTaskAsync(taskId, errorMessage);

		// Assert
		result.IsSuccess.ShouldBeTrue();

		await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => 
			t.ErrorMessage == "Task failed without specific error message"), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CancelTaskAsync_Should_CancelTask_When_TaskCanBeCancelled()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var pendingTask = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Test Task",
			Status = Domain.TaskStatus.Pending
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(pendingTask)));
		_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(pendingTask)));

		// Act
		var result = await _taskService.CancelTaskAsync(taskId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBeTrue();

		await _mockTaskRepository.Received(1).UpdateAsync(Arg.Is<AgentTask>(t => 
			t.Status == Domain.TaskStatus.Cancelled &&
			t.CompletedAt.HasValue), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData(Domain.TaskStatus.Completed)]
	[InlineData(Domain.TaskStatus.Cancelled)]
	public async Task CancelTaskAsync_Should_ReturnFailure_When_TaskCannotBeCancelled(Domain.TaskStatus status)
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var task = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Test Task",
			Status = status
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(task)));

		// Act
		var result = await _taskService.CancelTaskAsync(taskId);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe($"Task {taskId} cannot be cancelled. Current status: {status}");

		await _mockTaskRepository.DidNotReceive().UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetOverdueTasksAsync_Should_ReturnOverdueTasks_When_TasksExist()
	{
		// Arrange
		var overdueTasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Title = "Overdue Task 1", Deadline = DateTime.UtcNow.AddDays(-1) },
			new() { Id = Guid.NewGuid(), Title = "Overdue Task 2", Deadline = DateTime.UtcNow.AddDays(-2) }
		};

		_mockTaskRepository.GetOverdueTasksAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<IEnumerable<AgentTask>>.Success(overdueTasks)));

		// Act
		var result = await _taskService.GetOverdueTasksAsync();

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value!.Count().ShouldBe(2);

		await _mockTaskRepository.Received(1).GetOverdueTasksAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task GetOverdueTasksAsync_Should_ReturnFailure_When_RepositoryThrowsException()
	{
		// Arrange
		_mockTaskRepository.When(x => x.GetOverdueTasksAsync(Arg.Any<CancellationToken>()))
			.Do(x => throw new InvalidOperationException("Database connection failed"));

		// Act
		var result = await _taskService.GetOverdueTasksAsync();

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldContain("An error occurred while retrieving overdue tasks: Database connection failed");
	}

	[Fact]
	public async Task DeleteTaskAsync_Should_DeleteTask_When_TaskExists()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var existingTask = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Task to Delete"
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(existingTask)));
		_mockTaskRepository.DeleteAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<bool>.Success(true)));

		// Act
		var result = await _taskService.DeleteTaskAsync(taskId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBeTrue();

		await _mockTaskRepository.Received(1).GetByIdAsync(taskId, Arg.Any<CancellationToken>());
		await _mockTaskRepository.Received(1).DeleteAsync(taskId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteTaskAsync_Should_ReturnFailure_When_TaskNotFound()
	{
		// Arrange
		var taskId = Guid.NewGuid();

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Failure("Task not found")));

		// Act
		var result = await _taskService.DeleteTaskAsync(taskId);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe($"Task with ID {taskId} not found");

		await _mockTaskRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task DeleteTaskAsync_Should_ReturnFailure_When_DeleteOperationFails()
	{
		// Arrange
		var taskId = Guid.NewGuid();
		var existingTask = new AgentTask 
		{ 
			Id = taskId, 
			Title = "Task to Delete"
		};

		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<AgentTask>.Success(existingTask)));
		_mockTaskRepository.DeleteAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result<bool>.Failure("Cannot delete task with active dependencies")));

		// Act
		var result = await _taskService.DeleteTaskAsync(taskId);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe("Cannot delete task with active dependencies");
	}
} 