using ExxerAI.Domain;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Comprehensive unit tests for AgentTask entity
/// </summary>
public class AgentTaskEntityTests
{
    /// <summary>
    /// Creates a valid AgentTask for testing purposes
    /// </summary>
    /// <returns>A properly initialized AgentTask instance</returns>
    private static AgentTask CreateValidAgentTaskAsync()
    {
        return new AgentTask
        {
            Title = "Test Task",
            Description = "A comprehensive test task",
            TaskType = "TestType",
            Priority = TaskPriority.Normal,
            AgentStatus = TaskAgentStatus.Pending
        };
    }

    public class ConstructorAndDefaultsTests
    {
        [Fact]
        public void Should_InitializeWithDefaults_When_UsingParameterlessConstructor()
        {
            // Act
            var task = new AgentTask();

            // Assert
            task.Id.ShouldNotBe(Guid.Empty);
            task.Title.ShouldBe(string.Empty);
            task.Description.ShouldBe(string.Empty);
            task.TaskType.ShouldBe(string.Empty);
            task.AgentStatus.ShouldBe(TaskAgentStatus.Pending);
            task.Priority.ShouldBe(TaskPriority.Normal);
            task.AssignedAgentId.ShouldBeNull();
            task.AssignedAgent.ShouldBeNull();
            task.Input.ShouldNotBeNull();
            task.Output.ShouldNotBeNull();
            task.StartedAt.ShouldBeNull();
            task.CompletedAt.ShouldBeNull();
            task.Deadline.ShouldBeNull();
            task.ErrorMessage.ShouldBeNull();
            task.RetryCount.ShouldBe(0);
            task.Metadata.ShouldNotBeNull();
            (DateTime.UtcNow - task.CreatedAt).ShouldBeLessThan(TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Should_GenerateUniqueIds_When_MultipleInstancesCreated()
        {
            // Act
            var task1 = new AgentTask();
            var task2 = new AgentTask();
            var task3 = new AgentTask();

            // Assert
            task1.Id.ShouldNotBe(task2.Id);
            task1.Id.ShouldNotBe(task3.Id);
            task2.Id.ShouldNotBe(task3.Id);
        }

        [Fact]
        public void Should_InitializeDataObjects_When_TaskCreated()
        {
            // Act
            var task = new AgentTask();

            // Assert
            task.Input.ShouldNotBeNull();
            task.Input.ContentType.ShouldBe("application/json");
            task.Input.Content.ShouldBe(string.Empty);
            task.Input.Properties.ShouldNotBeNull();
            task.Input.Properties.ShouldBeEmpty();

            task.Output.ShouldNotBeNull();
            task.Output.ContentType.ShouldBe("application/json");
            task.Output.Content.ShouldBe(string.Empty);
            task.Output.Properties.ShouldNotBeNull();
            task.Output.Properties.ShouldBeEmpty();

            task.Metadata.ShouldNotBeNull();
            task.Metadata.Properties.ShouldNotBeNull();
            task.Metadata.Properties.ShouldBeEmpty();
            task.Metadata.Context.ShouldNotBeNull();
            task.Metadata.Context.ShouldBeEmpty();
            task.Metadata.Metrics.ShouldNotBeNull();
            task.Metadata.Metrics.ShouldBeEmpty();
        }
    }
}