namespace ExxerAI.Domain.Tests;

/// <summary>
/// Unit tests for Workflow domain entity
/// </summary>
public class WorkflowTests
{
    [Fact]
    public void Should_CreateWorkflow_When_ValidDataProvided()
    {
        // Arrange & Act
        var workflow = new Workflow
        {
            Name = "Test Workflow",
            Description = "A test workflow for development",
            Status = WorkflowStatus.Active
        };

        // Assert
        workflow.Id.ShouldNotBe(Guid.Empty);
        workflow.Name.ShouldBe("Test Workflow");
        workflow.Description.ShouldBe("A test workflow for development");
        workflow.Status.ShouldBe(WorkflowStatus.Active);
        workflow.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        workflow.Definition.ShouldNotBeNull();
        workflow.Executions.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(nameof(WorkflowStatus.Draft))]
    [InlineData(nameof(WorkflowStatus.Active))]
    [InlineData(nameof(WorkflowStatus.Running))]
    [InlineData(nameof(WorkflowStatus.Completed))]
    [InlineData(nameof(WorkflowStatus.Failed))]
    [InlineData(nameof(WorkflowStatus.Paused))]
    [InlineData(nameof(WorkflowStatus.Archived))]
    public void Should_HandleWorkflowStatuses_When_DifferentStatesProvided(string statusName)
    {
        // Arrange
        var workflow = new Workflow { Name = "AgentStatus Test" };
        var expectedStatus = EnumModelHelper.FromName<WorkflowStatus>(statusName);

        // Act
        workflow.Status = expectedStatus;

        // Assert
        workflow.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Should_InitializeEmptyExecutions_When_WorkflowCreated()
    {
        // Arrange & Act
        var workflow = new Workflow();

        // Assert
        workflow.Executions.ShouldNotBeNull();
        workflow.Executions.ShouldBeEmpty();
    }
}