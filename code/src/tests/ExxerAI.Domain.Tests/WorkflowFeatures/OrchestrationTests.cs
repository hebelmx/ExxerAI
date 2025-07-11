using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.WorkflowFeatures;

/// <summary>
/// Comprehensive tests for Workflow orchestration domain classes
/// </summary>
public class OrchestrationTests
{
    public class WorkflowTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var workflow = new Workflow();

            // Assert
            workflow.Id.ShouldNotBe(Guid.Empty);
            workflow.Name.ShouldBe(string.Empty);
            workflow.Description.ShouldBe(string.Empty);
            workflow.Status.ShouldBe(WorkflowStatus.Draft);
            workflow.Definition.ShouldNotBeNull();
            workflow.Definition.ShouldBeOfType<WorkflowDefinition>();
            workflow.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            workflow.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            workflow.StartedAt.ShouldBeNull();
            workflow.CompletedAt.ShouldBeNull();
            workflow.Executions.ShouldNotBeNull();
            workflow.Executions.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(WorkflowStatus.Draft)]
        [InlineData(WorkflowStatus.Active)]
        [InlineData(WorkflowStatus.Running)]
        [InlineData(WorkflowStatus.Completed)]
        [InlineData(WorkflowStatus.Failed)]
        [InlineData(WorkflowStatus.Paused)]
        [InlineData(WorkflowStatus.Archived)]
        public void Status_Should_AcceptAllValidValues(WorkflowStatus status)
        {
            // Arrange
            var workflow = new Workflow();

            // Act
            workflow.Status = status;

            // Assert
            workflow.Status.ShouldBe(status);
        }

        [Fact]
        public void Executions_Should_BeInitializedCollection()
        {
            // Arrange & Act
            var workflow = new Workflow();

            // Assert
            workflow.Executions.ShouldNotBeNull();
            workflow.Executions.ShouldBeAssignableTo<ICollection<WorkflowExecution>>();
            workflow.Executions.Count.ShouldBe(0);
        }
    }

    public class WorkflowDefinitionTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var definition = new WorkflowDefinition();

            // Assert
            definition.Steps.ShouldNotBeNull();
            definition.Steps.ShouldBeEmpty();
            definition.Variables.ShouldNotBeNull();
            definition.Variables.ShouldBeEmpty();
            definition.Configuration.ShouldNotBeNull();
            definition.Configuration.ShouldBeOfType<WorkflowConfiguration>();
        }

        [Fact]
        public void Steps_Should_BeModifiableCollection()
        {
            // Arrange
            var definition = new WorkflowDefinition();
            var step = new WorkflowStep { Name = "Test Step" };

            // Act
            definition.Steps.Add(step);

            // Assert
            definition.Steps.Count.ShouldBe(1);
            definition.Steps.ShouldContain(step);
        }

        [Fact]
        public void Variables_Should_BeModifiableDictionary()
        {
            // Arrange
            var definition = new WorkflowDefinition();

            // Act
            definition.Variables["testKey"] = "testValue";
            definition.Variables["numberKey"] = 42;

            // Assert
            definition.Variables.Count.ShouldBe(2);
            definition.Variables["testKey"].ShouldBe("testValue");
            definition.Variables["numberKey"].ShouldBe(42);
        }
    }

    public class WorkflowStepTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var step = new WorkflowStep();

            // Assert
            step.Id.ShouldNotBe(Guid.Empty);
            step.Name.ShouldBe(string.Empty);
            step.StepType.ShouldBe(string.Empty);
            step.Order.ShouldBe(0);
            step.Configuration.ShouldNotBeNull();
            step.Configuration.ShouldBeEmpty();
            step.Conditions.ShouldNotBeNull();
            step.Conditions.ShouldBeOfType<StepConditions>();
            step.NextSteps.ShouldNotBeNull();
            step.NextSteps.ShouldBeEmpty();
        }

        [Fact]
        public void Configuration_Should_BeModifiableDictionary()
        {
            // Arrange
            var step = new WorkflowStep();

            // Act
            step.Configuration["timeout"] = 30;
            step.Configuration["retries"] = 3;

            // Assert
            step.Configuration.Count.ShouldBe(2);
            step.Configuration["timeout"].ShouldBe(30);
            step.Configuration["retries"].ShouldBe(3);
        }

        [Fact]
        public void NextSteps_Should_BeModifiableCollection()
        {
            // Arrange
            var step = new WorkflowStep();
            var nextStep1 = Guid.NewGuid();
            var nextStep2 = Guid.NewGuid();

            // Act
            step.NextSteps.Add(nextStep1);
            step.NextSteps.Add(nextStep2);

            // Assert
            step.NextSteps.Count.ShouldBe(2);
            step.NextSteps.ShouldContain(nextStep1);
            step.NextSteps.ShouldContain(nextStep2);
        }
    }

    public class WorkflowExecutionTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var execution = new WorkflowExecution();

            // Assert
            execution.Id.ShouldNotBe(Guid.Empty);
            execution.WorkflowId.ShouldBe(Guid.Empty);
            execution.Workflow.ShouldBeNull();
            execution.Status.ShouldBe(WorkflowExecutionStatus.Starting);
            execution.StartedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            execution.CompletedAt.ShouldBeNull();
            execution.Input.ShouldNotBeNull();
            execution.Input.ShouldBeEmpty();
            execution.Output.ShouldNotBeNull();
            execution.Output.ShouldBeEmpty();
            execution.CurrentStepId.ShouldBeNull();
            execution.ErrorMessage.ShouldBeNull();
            execution.StepExecutions.ShouldNotBeNull();
            execution.StepExecutions.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(WorkflowExecutionStatus.Starting)]
        [InlineData(WorkflowExecutionStatus.Running)]
        [InlineData(WorkflowExecutionStatus.Completed)]
        [InlineData(WorkflowExecutionStatus.Failed)]
        [InlineData(WorkflowExecutionStatus.Cancelled)]
        [InlineData(WorkflowExecutionStatus.Paused)]
        public void Status_Should_AcceptAllValidValues(WorkflowExecutionStatus status)
        {
            // Arrange
            var execution = new WorkflowExecution();

            // Act
            execution.Status = status;

            // Assert
            execution.Status.ShouldBe(status);
        }

        [Fact]
        public void Input_Should_BeModifiableDictionary()
        {
            // Arrange
            var execution = new WorkflowExecution();

            // Act
            execution.Input["param1"] = "value1";
            execution.Input["param2"] = 42;

            // Assert
            execution.Input.Count.ShouldBe(2);
            execution.Input["param1"].ShouldBe("value1");
            execution.Input["param2"].ShouldBe(42);
        }

        [Fact]
        public void Output_Should_BeModifiableDictionary()
        {
            // Arrange
            var execution = new WorkflowExecution();

            // Act
            execution.Output["result"] = "success";
            execution.Output["data"] = new { Value = 123 };

            // Assert
            execution.Output.Count.ShouldBe(2);
            execution.Output["result"].ShouldBe("success");
        }
    }
} 