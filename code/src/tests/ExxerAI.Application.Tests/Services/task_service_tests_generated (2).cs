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
    public class TaskServiceBehavioralTests4
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<TaskService> _logger;
        private readonly TaskService _service;

        public TaskServiceBehavioralTests4()
        {
            _taskRepository = Substitute.For<ITaskRepository>();
            _logger = Substitute.For<ILogger<TaskService>>();
            _service = new TaskService(_taskRepository, _logger);
        }

        [Fact]
        public async Task GetPendingTasksAsync_Should_Return_CorrectCount_When_TasksExist()
        {
            var pendingTasks = new List<AgentTask>
            {
                new() { Id = Guid.NewGuid(), Status = TaskStatus.Pending },
                new() { Id = Guid.NewGuid(), Status = TaskStatus.Pending }
            };
            _taskRepository.GetPendingTasksAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(pendingTasks);

            var result = await _service.GetPendingTasksAsync(10);

            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count().ShouldBe(2);
            result.Value.All(t => t.Status == TaskStatus.Pending).ShouldBeTrue();
        }

        [Fact]
        public async Task GetPendingTasksAsync_Should_Return_Empty_When_NoPendingTasks()
        {
            _taskRepository.GetPendingTasksAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(new List<AgentTask>());

            var result = await _service.GetPendingTasksAsync(5);

            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeEmpty();
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_Should_Return_Success_When_UpdateSucceeds()
        {
            var taskId = Guid.NewGuid();
            var newStatus = TaskStatus.Completed;
            _taskRepository.UpdateStatusAsync(taskId, newStatus, Arg.Any<CancellationToken>())
                .Returns(Result.Success());

            var result = await _service.UpdateTaskStatusAsync(taskId, newStatus);

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_Should_Return_Failure_When_RepositoryFails()
        {
            var taskId = Guid.NewGuid();
            _taskRepository.UpdateStatusAsync(taskId, Arg.Any<TaskStatus>(), Arg.Any<CancellationToken>())
                .Returns(Result.Failure("Update failed"));

            var result = await _service.UpdateTaskStatusAsync(taskId, TaskStatus.Completed);

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
            var workflowId = Guid.NewGuid();
            _workflowRepository.CancelAsync(workflowId, Arg.Any<CancellationToken>())
                .Returns(Result.Success());

            var result = await _service.CancelWorkflowExecutionAsync(workflowId);

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task CancelWorkflowExecutionAsync_Should_Return_Failure_When_RepositoryFails()
        {
            var workflowId = Guid.NewGuid();
            _workflowRepository.CancelAsync(workflowId, Arg.Any<CancellationToken>())
                .Returns(Result.Failure("Cancellation failed"));

            var result = await _service.CancelWorkflowExecutionAsync(workflowId);

            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBe("Cancellation failed");
        }
    }

    public class GoogleDriveServiceBehavioralTests
    {
        private readonly IGoogleDriveClient _driveClient;
        private readonly ILogger<GoogleDriveService> _logger;
        private readonly GoogleDriveService _service;

        public GoogleDriveServiceBehavioralTests()
        {
            _driveClient = Substitute.For<IGoogleDriveClient>();
            _logger = Substitute.For<ILogger<GoogleDriveService>>();
            _service = new GoogleDriveService(_driveClient, _logger);
        }

        [Fact]
        public async Task DetectDocumentChangesAsync_Should_Return_Changes_When_ChangesExist()
        {
            var folderId = "folder123";
            var changes = new List<string> { "doc1", "doc2" };

            _driveClient.DetectChangesAsync(folderId, Arg.Any<CancellationToken>())
                .Returns(changes);

            var result = await _service.DetectDocumentChangesAsync(folderId);

            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeEquivalentTo(changes);
        }

        [Fact]
        public async Task DetectDocumentChangesAsync_Should_Return_Failure_When_ExceptionOccurs()
        {
            var folderId = "invalid-folder";
            _driveClient.DetectChangesAsync(folderId, Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Drive API error"));

            var result = await _service.DetectDocumentChangesAsync(folderId);

            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldContain("Drive API error");
        }
    }
}