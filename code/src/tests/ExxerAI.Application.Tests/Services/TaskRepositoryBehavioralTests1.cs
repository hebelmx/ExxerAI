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
    public class TaskRepositoryBehavioralTests1
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly TaskService _service;

        public TaskRepositoryBehavioralTests1()
        {
            _taskRepository = Substitute.For<ITaskRepository>();
            _agentRepository = Substitute.For<IAgentRepository>();
            
            _service = new TaskService(_taskRepository, _agentRepository);
        }

        [Fact]
        public async Task GetPendingTasksAsync_Should_Return_CorrectCount_When_TasksExist()
        {
            // Arrange
            var pendingTasks = new List<AgentTask>
            {
                new() { Id = Guid.NewGuid(), AgentStatus = TaskAgentStatus.Pending },
                new() { Id = Guid.NewGuid(), AgentStatus = TaskAgentStatus.Pending }
            };
            _taskRepository.GetByStatusAsync(TaskAgentStatus.Pending, Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<AgentTask>>.Success(pendingTasks));

            // Act
            var result = await _service.GetPendingTasksAsync(10);

            // Assert
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Value!.Count().ShouldBe(2);
            result.Value.All(t => t.AgentStatus == TaskAgentStatus.Pending).ShouldBeTrue();
        }

        [Fact]
        public async Task GetPendingTasksAsync_Should_Return_Empty_When_NoPendingTasks()
        {
            // Arrange
            _taskRepository.GetByStatusAsync(TaskAgentStatus.Pending, Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<AgentTask>>.Success(new List<AgentTask>()));

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
            var taskId = Guid.NewGuid();

            var task = new AgentTask
            {
                Id = taskId,
                AgentStatus = TaskAgentStatus.Pending
            };
            // Arrange
            _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
                .Returns(Result<AgentTask>.Success(task));
            var newStatus = TaskAgentStatus.Completed;
            task.AgentStatus = newStatus;

            _taskRepository.UpdateAsync(task, Arg.Any<CancellationToken>())
                .Returns(Result<AgentTask>.Success(task));

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
            var task = new AgentTask
            {
                Id = taskId,
                AgentStatus = TaskAgentStatus.Pending
            };
            
            _taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
                .Returns(Result<AgentTask>.Success(task));
            _taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
                .Returns(Result<AgentTask>.WithFailure("Update failed"));

            // Act
            var result = await _service.UpdateTaskStatusAsync(taskId, TaskAgentStatus.Completed);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Error.ShouldBe("Update failed");
        }
    }
}