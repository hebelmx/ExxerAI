using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.Services
{
    public class TaskServiceBehavioralTests1
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<TaskService> _logger;
        private readonly TaskService _service;

        public TaskServiceBehavioralTests1()
        {
            _taskRepository = Substitute.For<ITaskRepository>();
            _logger = Substitute.For<ILogger<TaskService>>();
            _service = new TaskService(_taskRepository, _logger);
        }

        [Fact]
        public async Task GetPendingTasksAsync_Should_Return_CorrectCount_When_TasksExist()
        {
            // Arrange
            var pendingTasks = new List<AgentTask>
            {
                new() { Id = Guid.NewGuid(), Status = TaskStatus.Pending },
                new() { Id = Guid.NewGuid(), Status = TaskStatus.Pending }
            };
            _taskRepository.GetPendingTasksAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(pendingTasks);

            // Act
            var result = await _service.GetPendingTasksAsync(10);

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count().ShouldBe(2);
            result.Value.All(t => t.Status == TaskStatus.Pending).ShouldBeTrue();
        }

        [Fact]
        public async Task GetPendingTasksAsync_Should_Return_Empty_When_NoPendingTasks()
        {
            // Arrange
            _taskRepository.GetPendingTasksAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(new List<AgentTask>());

            // Act
            var result = await _service.GetPendingTasksAsync(5);

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeEmpty();
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_Should_Return_Success_When_UpdateSucceeds()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var newStatus = TaskStatus.Completed;
            _taskRepository.UpdateStatusAsync(taskId, newStatus, Arg.Any<CancellationToken>())
                .Returns(Result.Success());

            // Act
            var result = await _service.UpdateTaskStatusAsync(taskId, newStatus);

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_Should_Return_Failure_When_RepositoryFails()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _taskRepository.UpdateStatusAsync(taskId, Arg.Any<TaskStatus>(), Arg.Any<CancellationToken>())
                .Returns(Result.Failure("Update failed"));

            // Act
            var result = await _service.UpdateTaskStatusAsync(taskId, TaskStatus.Completed);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBe("Update failed");
        }
    }
}