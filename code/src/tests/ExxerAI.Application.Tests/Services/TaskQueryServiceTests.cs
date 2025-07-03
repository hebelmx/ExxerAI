using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Focused tests for TaskService query functionality
/// </summary>
public class TaskQueryServiceTests
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly ITaskService _taskService;
    private readonly ILogger<TaskService> _logger;

    public TaskQueryServiceTests()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _agentRepository = Substitute.For<IAgentRepository>();
        _logger = Substitute.For<ILogger<TaskService>>();
        _taskService = new TaskService(_taskRepository, _agentRepository, _logger);
    }

    [Fact]
    public async Task Should_GetAllTasks_When_NoFiltersProvided()
    {
        // Arrange
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", AgentStatus = TaskAgentStatus.Pending },
            new() { Id = Guid.NewGuid(), Title = "Task 2", AgentStatus = TaskAgentStatus.InProgress },
            new() { Id = Guid.NewGuid(), Title = "Task 3", AgentStatus = TaskAgentStatus.Completed }
        };

        _taskRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<AgentTask>>.WithSuccess(tasks));

        // Act
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data!.Count().ShouldBe(3);
    }

    [Fact]
    public async Task Should_GetTaskById_When_ValidIdProvided()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new AgentTask
        {
            Id = taskId,
            Title = "Test Task",
            AgentStatus = TaskAgentStatus.Pending
        };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask?>.WithSuccess(task));

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data!.Id.ShouldBe(taskId);
        result.Data.Title.ShouldBe("Test Task");
    }

    [Fact]
    public async Task Should_ReturnNull_When_TaskNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask?>.WithSuccess((AgentTask?)null));

        // Act
        var result = await _taskService.GetTaskByIdAsync(taskId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeNull();
    }

    [Fact]
    public async Task Should_FilterTasksByStatus_When_StatusFilterProvided()
    {
        // Arrange
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", AgentStatus = TaskAgentStatus.Pending },
            new() { Id = Guid.NewGuid(), Title = "Task 2", AgentStatus = TaskAgentStatus.Pending },
            new() { Id = Guid.NewGuid(), Title = "Task 3", AgentStatus = TaskAgentStatus.Completed }
        };

        _taskRepository.GetByStatusAsync(TaskAgentStatus.Pending, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<AgentTask>>.WithSuccess(tasks.Where(t => t.AgentStatus == TaskAgentStatus.Pending)));

        // Act
        var result = await _taskService.GetTasksByStatusAsync(TaskAgentStatus.Pending);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data!.Count().ShouldBe(2);
        result.Data.All(t => t.AgentStatus == TaskAgentStatus.Pending).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_FilterTasksByPriority_When_PriorityFilterProvided()
    {
        // Arrange
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", Priority = TaskPriority.High },
            new() { Id = Guid.NewGuid(), Title = "Task 2", Priority = TaskPriority.Normal },
            new() { Id = Guid.NewGuid(), Title = "Task 3", Priority = TaskPriority.High }
        };

        _taskRepository.GetByPriorityAsync(TaskPriority.High, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<AgentTask>>.WithSuccess(tasks.Where(t => t.Priority == TaskPriority.High)));

        // Act
        var result = await _taskService.GetTasksByPriorityAsync(TaskPriority.High);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data!.Count().ShouldBe(2);
        result.Data.All(t => t.Priority == TaskPriority.High).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_GetTasksByAgent_When_ValidAgentIdProvided()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1", AssignedAgentId = agentId },
            new() { Id = Guid.NewGuid(), Title = "Task 2", AssignedAgentId = agentId }
        };

        _taskRepository.GetByAgentIdAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<AgentTask>>.WithSuccess(tasks));

        // Act
        var result = await _taskService.GetTasksByAgentAsync(agentId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data!.Count().ShouldBe(2);
        result.Data.All(t => t.AssignedAgentId == agentId).ShouldBeTrue();
    }

    [Theory]
    [InlineData(TaskStatus.Pending)]
    [InlineData(TaskStatus.InProgress)]
    [InlineData(TaskStatus.Completed)]
    [InlineData(TaskStatus.Failed)]
    public async Task Should_HandleAllTaskStatuses_When_StatusFilterProvided(TaskStatus status)
    {
        // Arrange
        var tasks = new List<AgentTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Test Task", Status = status }
        };

        _taskRepository.GetByStatusAsync(status, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<AgentTask>>.WithSuccess(tasks));

        // Act
        var result = await _taskService.GetTasksByStatusAsync(status);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data!.First().Status.ShouldBe(status);
    }
} 