using Shouldly;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Comprehensive tests for Workflow orchestration domain classes
/// Targets: Workflow, WorkflowDefinition, WorkflowStep, StepConditions, 
/// WorkflowConfiguration, NotificationSettings, WorkflowExecution, StepExecution
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

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var workflow = new Workflow();
            var testId = Guid.NewGuid();
            var testName = "Test Workflow";
            var testDescription = "Test Description";
            var testStatus = WorkflowStatus.Active;
            var testDefinition = new WorkflowDefinition();
            var testCreatedAt = DateTime.UtcNow.AddDays(-1);
            var testUpdatedAt = DateTime.UtcNow.AddHours(-1);
            var testStartedAt = DateTime.UtcNow.AddMinutes(-30);
            var testCompletedAt = DateTime.UtcNow;

            // Act & Assert
            workflow.Id = testId;
            workflow.Id.ShouldBe(testId);

            workflow.Name = testName;
            workflow.Name.ShouldBe(testName);

            workflow.Description = testDescription;
            workflow.Description.ShouldBe(testDescription);

            workflow.Status = testStatus;
            workflow.Status.ShouldBe(testStatus);

            workflow.Definition = testDefinition;
            workflow.Definition.ShouldBe(testDefinition);

            workflow.CreatedAt = testCreatedAt;
            workflow.CreatedAt.ShouldBe(testCreatedAt);

            workflow.UpdatedAt = testUpdatedAt;
            workflow.UpdatedAt.ShouldBe(testUpdatedAt);

            workflow.StartedAt = testStartedAt;
            workflow.StartedAt.ShouldBe(testStartedAt);

            workflow.CompletedAt = testCompletedAt;
            workflow.CompletedAt.ShouldBe(testCompletedAt);
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

        [Fact]
        public void Configuration_Should_SetAndGetCorrectly()
        {
            // Arrange
            var definition = new WorkflowDefinition();
            var config = new WorkflowConfiguration();

            // Act
            definition.Configuration = config;

            // Assert
            definition.Configuration.ShouldBe(config);
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
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var step = new WorkflowStep();
            var testId = Guid.NewGuid();
            var testName = "Test Step";
            var testStepType = "ProcessingStep";
            var testOrder = 5;
            var testConditions = new StepConditions();
            var nextStepId = Guid.NewGuid();

            // Act & Assert
            step.Id = testId;
            step.Id.ShouldBe(testId);

            step.Name = testName;
            step.Name.ShouldBe(testName);

            step.StepType = testStepType;
            step.StepType.ShouldBe(testStepType);

            step.Order = testOrder;
            step.Order.ShouldBe(testOrder);

            step.Conditions = testConditions;
            step.Conditions.ShouldBe(testConditions);

            step.NextSteps.Add(nextStepId);
            step.NextSteps.ShouldContain(nextStepId);
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

    public class StepConditionsTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var conditions = new StepConditions();

            // Assert
            conditions.ExecutionCondition.ShouldBeNull();
            conditions.TimeoutSeconds.ShouldBeNull();
            conditions.MaxRetries.ShouldBe(0);
            conditions.ContinueOnFailure.ShouldBeFalse();
        }

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var conditions = new StepConditions();
            var testCondition = "input != null";
            var testTimeout = 300;
            var testMaxRetries = 3;

            // Act & Assert
            conditions.ExecutionCondition = testCondition;
            conditions.ExecutionCondition.ShouldBe(testCondition);

            conditions.TimeoutSeconds = testTimeout;
            conditions.TimeoutSeconds.ShouldBe(testTimeout);

            conditions.MaxRetries = testMaxRetries;
            conditions.MaxRetries.ShouldBe(testMaxRetries);

            conditions.ContinueOnFailure = true;
            conditions.ContinueOnFailure.ShouldBeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void MaxRetries_Should_AcceptValidValues(int retries)
        {
            // Arrange
            var conditions = new StepConditions();

            // Act
            conditions.MaxRetries = retries;

            // Assert
            conditions.MaxRetries.ShouldBe(retries);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ContinueOnFailure_Should_AcceptBooleanValues(bool continueOnFailure)
        {
            // Arrange
            var conditions = new StepConditions();

            // Act
            conditions.ContinueOnFailure = continueOnFailure;

            // Assert
            conditions.ContinueOnFailure.ShouldBe(continueOnFailure);
        }
    }

    public class WorkflowConfigurationTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var config = new WorkflowConfiguration();

            // Assert
            config.MaxExecutionTimeSeconds.ShouldBe(3600);
            config.AllowParallelExecution.ShouldBeTrue();
            config.Notifications.ShouldNotBeNull();
            config.Notifications.ShouldBeOfType<NotificationSettings>();
            config.CustomProperties.ShouldNotBeNull();
            config.CustomProperties.ShouldBeEmpty();
        }

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var config = new WorkflowConfiguration();
            var testTimeout = 7200;
            var testNotifications = new NotificationSettings();

            // Act & Assert
            config.MaxExecutionTimeSeconds = testTimeout;
            config.MaxExecutionTimeSeconds.ShouldBe(testTimeout);

            config.AllowParallelExecution = false;
            config.AllowParallelExecution.ShouldBeFalse();

            config.Notifications = testNotifications;
            config.Notifications.ShouldBe(testNotifications);
        }

        [Fact]
        public void CustomProperties_Should_BeModifiableDictionary()
        {
            // Arrange
            var config = new WorkflowConfiguration();

            // Act
            config.CustomProperties["environment"] = "production";
            config.CustomProperties["version"] = "1.0";

            // Assert
            config.CustomProperties.Count.ShouldBe(2);
            config.CustomProperties["environment"].ShouldBe("production");
            config.CustomProperties["version"].ShouldBe("1.0");
        }
    }

    public class NotificationSettingsTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var notifications = new NotificationSettings();

            // Assert
            notifications.NotifyOnCompletion.ShouldBeFalse();
            notifications.NotifyOnFailure.ShouldBeTrue();
            notifications.Recipients.ShouldNotBeNull();
            notifications.Recipients.ShouldBeEmpty();
        }

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var notifications = new NotificationSettings();

            // Act & Assert
            notifications.NotifyOnCompletion = true;
            notifications.NotifyOnCompletion.ShouldBeTrue();

            notifications.NotifyOnFailure = false;
            notifications.NotifyOnFailure.ShouldBeFalse();
        }

        [Fact]
        public void Recipients_Should_BeModifiableCollection()
        {
            // Arrange
            var notifications = new NotificationSettings();
            var recipient1 = "admin@example.com";
            var recipient2 = "user@example.com";

            // Act
            notifications.Recipients.Add(recipient1);
            notifications.Recipients.Add(recipient2);

            // Assert
            notifications.Recipients.Count.ShouldBe(2);
            notifications.Recipients.ShouldContain(recipient1);
            notifications.Recipients.ShouldContain(recipient2);
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

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var execution = new WorkflowExecution();
            var testId = Guid.NewGuid();
            var testWorkflowId = Guid.NewGuid();
            var testWorkflow = new Workflow();
            var testStatus = WorkflowExecutionStatus.Running;
            var testStartedAt = DateTime.UtcNow.AddMinutes(-10);
            var testCompletedAt = DateTime.UtcNow;
            var testCurrentStepId = Guid.NewGuid();
            var testErrorMessage = "Test error";

            // Act & Assert
            execution.Id = testId;
            execution.Id.ShouldBe(testId);

            execution.WorkflowId = testWorkflowId;
            execution.WorkflowId.ShouldBe(testWorkflowId);

            execution.Workflow = testWorkflow;
            execution.Workflow.ShouldBe(testWorkflow);

            execution.Status = testStatus;
            execution.Status.ShouldBe(testStatus);

            execution.StartedAt = testStartedAt;
            execution.StartedAt.ShouldBe(testStartedAt);

            execution.CompletedAt = testCompletedAt;
            execution.CompletedAt.ShouldBe(testCompletedAt);

            execution.CurrentStepId = testCurrentStepId;
            execution.CurrentStepId.ShouldBe(testCurrentStepId);

            execution.ErrorMessage = testErrorMessage;
            execution.ErrorMessage.ShouldBe(testErrorMessage);
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
            execution.Input["inputData"] = "test value";
            execution.Input["configOptions"] = new { timeout = 30 };

            // Assert
            execution.Input.Count.ShouldBe(2);
            execution.Input["inputData"].ShouldBe("test value");
            execution.Input.ShouldContainKey("configOptions");
        }

        [Fact]
        public void Output_Should_BeModifiableDictionary()
        {
            // Arrange
            var execution = new WorkflowExecution();

            // Act
            execution.Output["result"] = "success";
            execution.Output["processedCount"] = 100;

            // Assert
            execution.Output.Count.ShouldBe(2);
            execution.Output["result"].ShouldBe("success");
            execution.Output["processedCount"].ShouldBe(100);
        }

        [Fact]
        public void StepExecutions_Should_BeModifiableCollection()
        {
            // Arrange
            var execution = new WorkflowExecution();
            var stepExecution = new StepExecution();

            // Act
            execution.StepExecutions.Add(stepExecution);

            // Assert
            execution.StepExecutions.Count.ShouldBe(1);
            execution.StepExecutions.ShouldContain(stepExecution);
        }
    }

    public class StepExecutionTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var stepExecution = new StepExecution();

            // Assert
            stepExecution.Id.ShouldNotBe(Guid.Empty);
            stepExecution.WorkflowExecutionId.ShouldBe(Guid.Empty);
            stepExecution.WorkflowExecution.ShouldBeNull();
            stepExecution.StepId.ShouldBe(Guid.Empty);
            stepExecution.Status.ShouldBe(StepExecutionStatus.Pending);
            stepExecution.StartedAt.ShouldBeNull();
            stepExecution.CompletedAt.ShouldBeNull();
            stepExecution.Input.ShouldNotBeNull();
            stepExecution.Input.ShouldBeEmpty();
            stepExecution.Output.ShouldNotBeNull();
            stepExecution.Output.ShouldBeEmpty();
            stepExecution.ErrorMessage.ShouldBeNull();
            stepExecution.RetryCount.ShouldBe(0);
        }

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var stepExecution = new StepExecution();
            var testId = Guid.NewGuid();
            var testWorkflowExecutionId = Guid.NewGuid();
            var testWorkflowExecution = new WorkflowExecution();
            var testStepId = Guid.NewGuid();
            var testStatus = StepExecutionStatus.Running;
            var testStartedAt = DateTime.UtcNow.AddMinutes(-5);
            var testCompletedAt = DateTime.UtcNow;
            var testErrorMessage = "Step failed";
            var testRetryCount = 2;

            // Act & Assert
            stepExecution.Id = testId;
            stepExecution.Id.ShouldBe(testId);

            stepExecution.WorkflowExecutionId = testWorkflowExecutionId;
            stepExecution.WorkflowExecutionId.ShouldBe(testWorkflowExecutionId);

            stepExecution.WorkflowExecution = testWorkflowExecution;
            stepExecution.WorkflowExecution.ShouldBe(testWorkflowExecution);

            stepExecution.StepId = testStepId;
            stepExecution.StepId.ShouldBe(testStepId);

            stepExecution.Status = testStatus;
            stepExecution.Status.ShouldBe(testStatus);

            stepExecution.StartedAt = testStartedAt;
            stepExecution.StartedAt.ShouldBe(testStartedAt);

            stepExecution.CompletedAt = testCompletedAt;
            stepExecution.CompletedAt.ShouldBe(testCompletedAt);

            stepExecution.ErrorMessage = testErrorMessage;
            stepExecution.ErrorMessage.ShouldBe(testErrorMessage);

            stepExecution.RetryCount = testRetryCount;
            stepExecution.RetryCount.ShouldBe(testRetryCount);
        }

        [Theory]
        [InlineData(StepExecutionStatus.Pending)]
        [InlineData(StepExecutionStatus.Running)]
        [InlineData(StepExecutionStatus.Completed)]
        [InlineData(StepExecutionStatus.Failed)]
        [InlineData(StepExecutionStatus.Skipped)]
        [InlineData(StepExecutionStatus.Cancelled)]
        public void Status_Should_AcceptAllValidValues(StepExecutionStatus status)
        {
            // Arrange
            var stepExecution = new StepExecution();

            // Act
            stepExecution.Status = status;

            // Assert
            stepExecution.Status.ShouldBe(status);
        }

        [Fact]
        public void Input_Should_BeModifiableDictionary()
        {
            // Arrange
            var stepExecution = new StepExecution();

            // Act
            stepExecution.Input["parameter1"] = "value1";
            stepExecution.Input["parameter2"] = 42;

            // Assert
            stepExecution.Input.Count.ShouldBe(2);
            stepExecution.Input["parameter1"].ShouldBe("value1");
            stepExecution.Input["parameter2"].ShouldBe(42);
        }

        [Fact]
        public void Output_Should_BeModifiableDictionary()
        {
            // Arrange
            var stepExecution = new StepExecution();

            // Act
            stepExecution.Output["result"] = "completed";
            stepExecution.Output["duration"] = TimeSpan.FromSeconds(30);

            // Assert
            stepExecution.Output.Count.ShouldBe(2);
            stepExecution.Output["result"].ShouldBe("completed");
            stepExecution.Output.ShouldContainKey("duration");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void RetryCount_Should_AcceptValidValues(int retryCount)
        {
            // Arrange
            var stepExecution = new StepExecution();

            // Act
            stepExecution.RetryCount = retryCount;

            // Assert
            stepExecution.RetryCount.ShouldBe(retryCount);
        }
    }

    public class WorkflowStatusTests
    {
        [Theory]
        [InlineData(WorkflowStatus.Draft)]
        [InlineData(WorkflowStatus.Active)]
        [InlineData(WorkflowStatus.Running)]
        [InlineData(WorkflowStatus.Completed)]
        [InlineData(WorkflowStatus.Failed)]
        [InlineData(WorkflowStatus.Paused)]
        [InlineData(WorkflowStatus.Archived)]
        public void WorkflowStatus_Should_HaveAllExpectedValues(WorkflowStatus status)
        {
            // Assert
            Enum.IsDefined(typeof(WorkflowStatus), status).ShouldBeTrue();
        }

        [Fact]
        public void WorkflowStatus_Should_HaveCorrectCount()
        {
            // Act
            var statusCount = Enum.GetValues<WorkflowStatus>().Length;

            // Assert
            statusCount.ShouldBe(7);
        }
    }

    public class WorkflowExecutionStatusTests
    {
        [Theory]
        [InlineData(WorkflowExecutionStatus.Starting)]
        [InlineData(WorkflowExecutionStatus.Running)]
        [InlineData(WorkflowExecutionStatus.Completed)]
        [InlineData(WorkflowExecutionStatus.Failed)]
        [InlineData(WorkflowExecutionStatus.Cancelled)]
        [InlineData(WorkflowExecutionStatus.Paused)]
        public void WorkflowExecutionStatus_Should_HaveAllExpectedValues(WorkflowExecutionStatus status)
        {
            // Assert
            Enum.IsDefined(typeof(WorkflowExecutionStatus), status).ShouldBeTrue();
        }

        [Fact]
        public void WorkflowExecutionStatus_Should_HaveCorrectCount()
        {
            // Act
            var statusCount = Enum.GetValues<WorkflowExecutionStatus>().Length;

            // Assert
            statusCount.ShouldBe(6);
        }
    }

    public class StepExecutionStatusTests
    {
        [Theory]
        [InlineData(StepExecutionStatus.Pending)]
        [InlineData(StepExecutionStatus.Running)]
        [InlineData(StepExecutionStatus.Completed)]
        [InlineData(StepExecutionStatus.Failed)]
        [InlineData(StepExecutionStatus.Skipped)]
        [InlineData(StepExecutionStatus.Cancelled)]
        public void StepExecutionStatus_Should_HaveAllExpectedValues(StepExecutionStatus status)
        {
            // Assert
            Enum.IsDefined(typeof(StepExecutionStatus), status).ShouldBeTrue();
        }

        [Fact]
        public void StepExecutionStatus_Should_HaveCorrectCount()
        {
            // Act
            var statusCount = Enum.GetValues<StepExecutionStatus>().Length;

            // Assert
            statusCount.ShouldBe(6);
        }
    }
} 