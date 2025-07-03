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
    public class TaskServiceBehavioralTests5
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<TaskService> _logger;
        private readonly TaskService _service;

        public TaskServiceBehavioralTests5()
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

    public class WorkflowServiceBehavioralTests
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly ILogger<WorkflowService> _logger;
        private readonly WorkflowService _service;

        public WorkflowServiceBehavioralTests()
        {
            _workflowRepository = Substitute.For<IWorkflowRepository>();
            _logger = Substitute.For<ILogger<WorkflowService>>();
            _service = new WorkflowService(_workflowRepository, _logger);
        }

        [Fact]
        public async Task CancelWorkflowExecutionAsync_Should_Return_Success_When_CancellationCompletes()
        {
            // Arrange
            var workflowId = Guid.NewGuid();
            _workflowRepository.CancelAsync(workflowId, Arg.Any<CancellationToken>())
                .Returns(Result.Success());

            // Act
            var result = await _service.CancelWorkflowExecutionAsync(workflowId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task CancelWorkflowExecutionAsync_Should_Return_Failure_When_RepositoryFails()
        {
            // Arrange
            var workflowId = Guid.NewGuid();
            _workflowRepository.CancelAsync(workflowId, Arg.Any<CancellationToken>())
                .Returns(Result.Failure("Cancellation failed"));

            // Act
            var result = await _service.CancelWorkflowExecutionAsync(workflowId);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBe("Cancellation failed");
        }
    }
}