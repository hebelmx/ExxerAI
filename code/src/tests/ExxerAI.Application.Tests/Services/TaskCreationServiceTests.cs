using ExxerAI.Application.DTOs;
using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Focused tests for TaskService task creation functionality
/// </summary>
public class TaskCreationServiceTests
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly ITaskService _taskService;
    private readonly ILogger<TaskService> _logger;

    public TaskCreationServiceTests()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _agentRepository = Substitute.For<IAgentRepository>();
        _logger = Substitute.For<ILogger<TaskService>>();
        _taskService = new TaskService(_taskRepository, _agentRepository, _logger);
    }

    [Fact]
    public async Task Should_CreateTask_When_ValidTaskRequestProvided()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Type = "DataProcessing",
            Priority = TaskPriority.Normal,
            Description = "Test description"
        };

        var expectedTask = new AgentTask
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Type = request.Type,
            Priority = request.Priority,
            AgentStatus = TaskAgentStatus.Pending,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _taskRepository.CreateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.WithSuccess(expectedTask));

        // Act
        var result = await _taskService.CreateTaskAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data!.Title.ShouldBe(request.Title);
        result.Data.Type.ShouldBe(request.Type);
        result.Data.Priority.ShouldBe(request.Priority);
        result.Data.AgentStatus.ShouldBe(TaskAgentStatus.Pending);
    }

    [Fact]
    public async Task Should_FailCreation_When_TaskTitleIsEmpty()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "",
            Type = "DataProcessing",
            Priority = TaskPriority.Normal
        };

        // Act
        var result = await _taskService.CreateTaskAsync(request);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Title");
    }

    [Fact]
    public async Task Should_FailCreation_When_TaskTypeIsEmpty()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Type = "",
            Priority = TaskPriority.Normal
        };

        // Act
        var result = await _taskService.CreateTaskAsync(request);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Type");
    }

    [Fact]
    public async Task Should_FailCreation_When_RepositoryReturnsError()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Type = "DataProcessing",
            Priority = TaskPriority.Normal
        };

        _taskRepository.CreateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.WithFailure("Repository error"));

        // Act
        var result = await _taskService.CreateTaskAsync(request);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Repository error");
    }

    [Theory]
    [InlineData(TaskPriority.Low)]
    [InlineData(TaskPriority.Normal)]
    [InlineData(TaskPriority.High)]
    [InlineData(TaskPriority.Critical)]
    public async Task Should_CreateTask_When_ValidPriorityProvided(TaskPriority priority)
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Type = "DataProcessing",
            Priority = priority
        };

        var expectedTask = new AgentTask
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Type = request.Type,
            Priority = priority,
            AgentStatus = TaskAgentStatus.Pending
        };

        _taskRepository.CreateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.WithSuccess(expectedTask));

        // Act
        var result = await _taskService.CreateTaskAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Data!.Priority.ShouldBe(priority);
    }
} 