using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Focused tests for TaskService task assignment functionality
/// </summary>
public class TaskAssignmentServiceTests
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly ITaskService _taskService;
    private readonly ILogger<TaskService> _logger;

    public TaskAssignmentServiceTests()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _agentRepository = Substitute.For<IAgentRepository>();
        _logger = Substitute.For<ILogger<TaskService>>();
        _taskService = new TaskService(_taskRepository, _agentRepository, _logger);
    }

    [Fact]
    public async Task Should_AssignTask_When_ValidTaskAndAgentProvided()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var existingTask = new AgentTask
        {
            Id = taskId,
            Title = "Test Task",
            AgentStatus = TaskAgentStatus.Pending,
            AssignedAgentId = null
        };

        var agent = new Agent
        {
            Id = agentId,
            Name = "Test Agent",
            Status = AgentStatus.Active
        };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask?>.WithSuccess(existingTask));
        _agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(Result<Agent?>.WithSuccess(agent));
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.WithSuccess(existingTask));

        // Act
        var result = await _taskService.AssignTaskAsync(taskId, agentId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _taskRepository.Received(1).UpdateAsync(
            Arg.Is<AgentTask>(t => t.AssignedAgentId == agentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_FailAssignment_When_TaskNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask?>.WithSuccess((AgentTask?)null));

        // Act
        var result = await _taskService.AssignTaskAsync(taskId, agentId);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("not found");
    }

    [Fact]
    public async Task Should_FailAssignment_When_AgentNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var existingTask = new AgentTask
        {
            Id = taskId,
            Title = "Test Task",
            AgentStatus = TaskAgentStatus.Pending
        };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask?>.WithSuccess(existingTask));
        _agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(Result<Agent?>.WithSuccess((Agent?)null));

        // Act
        var result = await _taskService.AssignTaskAsync(taskId, agentId);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Agent not found");
    }

    [Fact]
    public async Task Should_FailAssignment_When_TaskAlreadyCompleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var existingTask = new AgentTask
        {
            Id = taskId,
            Title = "Test Task",
            AgentStatus = TaskAgentStatus.Completed
        };

        var agent = new Agent
        {
            Id = agentId,
            Name = "Test Agent",
            Status = AgentStatus.Active
        };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask?>.WithSuccess(existingTask));
        _agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(Result<Agent?>.WithSuccess(agent));

        // Act
        var result = await _taskService.AssignTaskAsync(taskId, agentId);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("completed");
    }

    [Fact]
    public async Task Should_FailAssignment_When_AgentIsInactive()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var existingTask = new AgentTask
        {
            Id = taskId,
            Title = "Test Task",
            AgentStatus = TaskAgentStatus.Pending
        };

        var agent = new Agent
        {
            Id = agentId,
            Name = "Test Agent",
            Status = AgentStatus.Inactive
        };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask?>.WithSuccess(existingTask));
        _agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(Result<Agent?>.WithSuccess(agent));

        // Act
        var result = await _taskService.AssignTaskAsync(taskId, agentId);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("inactive");
    }

    [Fact]
    public async Task Should_ReassignTask_When_TaskAlreadyAssignedToAnotherAgent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var oldAgentId = Guid.NewGuid();
        var newAgentId = Guid.NewGuid();

        var existingTask = new AgentTask
        {
            Id = taskId,
            Title = "Test Task",
            AgentStatus = TaskAgentStatus.Pending,
            AssignedAgentId = oldAgentId
        };

        var newAgent = new Agent
        {
            Id = newAgentId,
            Name = "New Agent",
            Status = AgentStatus.Active
        };

        _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask?>.WithSuccess(existingTask));
        _agentRepository.GetByIdAsync(newAgentId, Arg.Any<CancellationToken>())
            .Returns(Result<Agent?>.WithSuccess(newAgent));
        _taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
            .Returns(Result<AgentTask>.WithSuccess(existingTask));

        // Act
        var result = await _taskService.AssignTaskAsync(taskId, newAgentId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _taskRepository.Received(1).UpdateAsync(
            Arg.Is<AgentTask>(t => t.AssignedAgentId == newAgentId),
            Arg.Any<CancellationToken>());
    }
} 