namespace ExxerAI.Domain.Tests;

/// <summary>
/// Unit tests for WorkflowExecution domain entity
/// </summary>
public class WorkflowExecutionTests
{
    [Fact]
    public void Should_CreateWorkflowExecution_When_ValidDataProvided()
    {
        // Arrange & Act
        var execution = new WorkflowExecution
        {
            WorkflowId = Guid.NewGuid(),
            Status = WorkflowExecutionStatus.Running
        };

        // Assert
        execution.Id.ShouldNotBe(Guid.Empty);
        execution.WorkflowId.ShouldNotBe(Guid.Empty);
        execution.Status.ShouldBe(WorkflowExecutionStatus.Running);
        execution.StartedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        execution.Input.ShouldNotBeNull();
        execution.Output.ShouldNotBeNull();
        execution.StepExecutions.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(nameof(WorkflowExecutionStatus.Starting))]
    [InlineData(nameof(WorkflowExecutionStatus.Running))]
    [InlineData(nameof(WorkflowExecutionStatus.Completed))]
    [InlineData(nameof(WorkflowExecutionStatus.Failed))]
    [InlineData(nameof(WorkflowExecutionStatus.Cancelled))]
    [InlineData(nameof(WorkflowExecutionStatus.Paused))]
    public void Should_HandleExecutionStatuses_When_DifferentStatesProvided(string statusName)
    {
        // Arrange
        var execution = new WorkflowExecution();
        var expectedStatus = EnumModelHelper.FromName<WorkflowExecutionStatus>(statusName);

        // Act
        execution.Status = expectedStatus;

        // Assert
        execution.Status.ShouldBe(expectedStatus);
    }
}