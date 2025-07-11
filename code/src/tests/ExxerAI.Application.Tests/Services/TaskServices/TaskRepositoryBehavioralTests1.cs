using System;
using System.Threading;
using ExxerAI.Domain;
using ExxerAI.Domain.Operations;
using ExxerAI.Application.Services;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.Services
{
    public class TaskRepositoryBehavioralTests1
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly TaskService _service;

        public TaskRepositoryBehavioralTests1()
        {
            _taskRepository = Substitute.For<ITaskRepository>();
            _agentRepository = Substitute.For<IAgentRepository>();

            var logger = Substitute.For<ILogger<TaskService>>();
            _service = new TaskService(logger);
        }

        [Fact]
        public async Task GetPendingTasksAsync_Should_Return_CorrectCount_When_TasksExistAsync()
        {
            // Arrange - Create tasks in the service first
            await _service.CreateTaskAsync("Task 1", "Description 1", "Type1", TaskPriority.Normal, cancellationToken: TestContext.Current.CancellationToken);
            await _service.CreateTaskAsync("Task 2", "Description 2", "Type2", TaskPriority.High, cancellationToken: TestContext.Current.CancellationToken);

            // Act
            var result = await _service.GetPendingTasksAsync(10, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Value!.Count().ShouldBe(2);
            result.Value!.All(t => t.AgentStatus == TaskAgentStatus.Pending).ShouldBeTrue();
        }

        [Fact]
        public async Task GetPendingTasksAsync_Should_Return_Empty_When_NoPendingTasksAsync()
        {
            // Arrange - No tasks created, so should be empty

            // Act
            var result = await _service.GetPendingTasksAsync(5, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeEmpty();
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_Should_Return_Success_When_UpdateSucceedsAsync()
        {
            // Arrange - Create a task first
            var createResult = await _service.CreateTaskAsync("Test Task", "Test Description", "TestType", TaskPriority.Normal, cancellationToken: TestContext.Current.CancellationToken);
            createResult.IsSuccess.ShouldBeTrue();
            var taskId = createResult.Value!.Id;
            var newStatus = TaskAgentStatus.Completed;

            // Act
            var result = await _service.UpdateTaskStatusAsync(taskId, newStatus, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_Should_Return_Failure_When_TaskNotFoundAsync()
        {
            // Arrange - Use a non-existent task ID
            var taskId = Guid.NewGuid();

            // Act
            var result = await _service.UpdateTaskStatusAsync(taskId, TaskAgentStatus.Completed, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBe("Task not found");
        }
    }
}