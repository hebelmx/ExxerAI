using Microsoft.Extensions.Logging;
using NSubstitute;
using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;
using ExxerAI.Domain.Operations;

namespace ExxerAI.Infrastructure.Tests.Services;

/// <summary>
/// Unit tests for WorkflowExecutionEngine
/// Tests workflow execution scenarios, error handling, and step processing
/// </summary>
public class WorkflowExecutionEngineTests
{
    private readonly IWorkflowExecutionRepository _executionRepository;
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IAgentService _agentService;
    private readonly ITaskService _taskService;
    private readonly ILogger<WorkflowExecutionEngine> _logger;
    private readonly WorkflowExecutionEngine _engine;

    public WorkflowExecutionEngineTests()
    {
        _executionRepository = Substitute.For<IWorkflowExecutionRepository>();
        _workflowRepository = Substitute.For<IWorkflowRepository>();
        _agentService = Substitute.For<IAgentService>();
        _taskService = Substitute.For<ITaskService>();
        _logger = Substitute.For<ILogger<WorkflowExecutionEngine>>();

        _engine = new WorkflowExecutionEngine(
            _executionRepository,
            _workflowRepository,
            _agentService,
            _taskService,
            _logger);
    }

    #region ExecuteWorkflowAsync Tests

    [Fact]
    public async Task ExecuteWorkflowAsync_WithValidWorkflow_ShouldExecuteSuccessfully()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var workflow = CreateTestWorkflow(workflowId);
        var input = new Dictionary<string, object> { ["testInput"] = "value" };

        _workflowRepository.GetByIdAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result<Workflow>.WithSuccess(workflow));

        _executionRepository.CreateExecutionAsync(Arg.Any<WorkflowExecution>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result<WorkflowExecution>.WithSuccess(callInfo.ArgAt<WorkflowExecution>(0)));

        _executionRepository.UpdateExecutionAsync(Arg.Any<WorkflowExecution>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result<WorkflowExecution>.WithSuccess(callInfo.ArgAt<WorkflowExecution>(0)));

        // Act
        var result = await _engine.ExecuteWorkflowAsync(workflowId, input, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be(WorkflowExecutionStatus.Completed);
        result.Value.Input.Should().ContainKey("testInput");
        result.Value.CompletedAt.Should().NotBeNull();

        await _executionRepository.Received().CreateExecutionAsync(Arg.Any<WorkflowExecution>(), Arg.Any<CancellationToken>());
        await _executionRepository.Received().UpdateExecutionAsync(Arg.Any<WorkflowExecution>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithNonExistentWorkflow_ShouldReturnFailure()
    {
        // Arrange
        var workflowId = Guid.NewGuid();

        _workflowRepository.GetByIdAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result<Workflow>.WithFailure("Workflow not found"));

        // Act
        var result = await _engine.ExecuteWorkflowAsync(workflowId, null, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to load workflow");
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithCancellationToken_ShouldReturnCancelled()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var result = await _engine.ExecuteWorkflowAsync(Guid.NewGuid(), null, cancellationTokenSource.Token);

        // Assert
        result.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithWorkflowHavingNoSteps_ShouldReturnFailure()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var workflow = new Workflow
        {
            Id = workflowId,
            Name = "Empty Workflow",
            Steps = new List<WorkflowStep>()
        };

        _workflowRepository.GetByIdAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result<Workflow>.WithSuccess(workflow));

        _executionRepository.CreateExecutionAsync(Arg.Any<WorkflowExecution>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result<WorkflowExecution>.WithSuccess(callInfo.ArgAt<WorkflowExecution>(0)));

        // Act
        var result = await _engine.ExecuteWorkflowAsync(workflowId, null, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("no steps to execute");
    }

    #endregion

    #region ResumeExecutionAsync Tests

    [Fact]
    public async Task ResumeExecutionAsync_WithPausedExecution_ShouldResumeSuccessfully()
    {
        // Arrange
        var executionId = Guid.NewGuid();
        var workflowId = Guid.NewGuid();
        var workflow = CreateTestWorkflow(workflowId);
        
        var execution = new WorkflowExecution
        {
            Id = executionId,
            WorkflowId = workflowId,
            Workflow = workflow,
            Status = WorkflowExecutionStatus.Paused,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            LastModified = DateTime.UtcNow.AddMinutes(-5)
        };

        _executionRepository.GetExecutionAsync(executionId, Arg.Any<CancellationToken>())
            .Returns(Result<WorkflowExecution>.WithSuccess(execution));

        _executionRepository.UpdateExecutionAsync(Arg.Any<WorkflowExecution>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result<WorkflowExecution>.WithSuccess(callInfo.ArgAt<WorkflowExecution>(0)));

        // Act
        var result = await _engine.ResumeExecutionAsync(executionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(WorkflowExecutionStatus.Completed);

        await _executionRepository.Received().UpdateExecutionAsync(
            Arg.Is<WorkflowExecution>(e => e.Status == WorkflowExecutionStatus.Running), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResumeExecutionAsync_WithNonPausedExecution_ShouldReturnFailure()
    {
        // Arrange
        var executionId = Guid.NewGuid();
        var execution = new WorkflowExecution
        {
            Id = executionId,
            Status = WorkflowExecutionStatus.Running
        };

        _executionRepository.GetExecutionAsync(executionId, Arg.Any<CancellationToken>())
            .Returns(Result<WorkflowExecution>.WithSuccess(execution));

        // Act
        var result = await _engine.ResumeExecutionAsync(executionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Cannot resume execution in Running status");
    }

    [Fact]
    public async Task ResumeExecutionAsync_WithNonExistentExecution_ShouldReturnFailure()
    {
        // Arrange
        var executionId = Guid.NewGuid();

        _executionRepository.GetExecutionAsync(executionId, Arg.Any<CancellationToken>())
            .Returns(Result<WorkflowExecution>.WithFailure("Execution not found"));

        // Act
        var result = await _engine.ResumeExecutionAsync(executionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to load execution");
    }

    #endregion

    #region ExecuteStepAsync Tests

    [Fact]
    public async Task ExecuteStepAsync_WithValidStep_ShouldExecuteSuccessfully()
    {
        // Arrange
        var step = CreateTestWorkflowStep();
        var execution = new WorkflowExecution
        {
            Id = Guid.NewGuid(),
            WorkflowId = Guid.NewGuid(),
            Status = WorkflowExecutionStatus.Running,
            StepExecutions = new List<StepExecution>()
        };

        // Act
        var result = await _engine.ExecuteStepAsync(step, execution, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.StepId.Should().Be(step.Id);
        result.Value.Status.Should().Be(StepExecutionStatus.Completed);
        result.Value.StartedAt.Should().NotBeNull();
        result.Value.CompletedAt.Should().NotBeNull();
        execution.StepExecutions.Should().Contain(result.Value);
    }

    [Fact]
    public async Task ExecuteStepAsync_WithNullStep_ShouldReturnFailure()
    {
        // Arrange
        var execution = new WorkflowExecution { Id = Guid.NewGuid() };

        // Act
        var result = await _engine.ExecuteStepAsync(null!, execution, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Step cannot be null");
    }

    [Fact]
    public async Task ExecuteStepAsync_WithNullExecution_ShouldReturnFailure()
    {
        // Arrange
        var step = CreateTestWorkflowStep();

        // Act
        var result = await _engine.ExecuteStepAsync(step, null!, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Execution cannot be null");
    }

    [Fact]
    public async Task ExecuteStepAsync_WithCancellationToken_ShouldReturnCancelled()
    {
        // Arrange
        var step = CreateTestWorkflowStep();
        var execution = new WorkflowExecution 
        { 
            Id = Guid.NewGuid(),
            StepExecutions = new List<StepExecution>()
        };
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var result = await _engine.ExecuteStepAsync(step, execution, cancellationTokenSource.Token);

        // Assert
        result.IsCancelled.Should().BeTrue();
    }

    #endregion

    #region HandleStepFailureAsync Tests

    [Fact]
    public async Task HandleStepFailureAsync_WithRetriableError_ShouldReturnRetryAction()
    {
        // Arrange
        var stepExecution = new StepExecution
        {
            StepId = Guid.NewGuid(),
            RetryCount = 1
        };
        var error = new InvalidOperationException("Temporary error");

        // Act
        var result = await _engine.HandleStepFailureAsync(stepExecution, error, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(StepExecutionRecoveryAction.Retry);
    }

    [Fact]
    public async Task HandleStepFailureAsync_WithMaxRetriesExceeded_ShouldReturnFailWorkflowAction()
    {
        // Arrange
        var stepExecution = new StepExecution
        {
            StepId = Guid.NewGuid(),
            RetryCount = 5 // Exceeds max retries
        };
        var error = new Exception("Persistent error");

        // Act
        var result = await _engine.HandleStepFailureAsync(stepExecution, error, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(StepExecutionRecoveryAction.FailWorkflow);
    }

    [Fact]
    public async Task HandleStepFailureAsync_WithNullStepExecution_ShouldReturnFailure()
    {
        // Arrange
        var error = new Exception("Test error");

        // Act
        var result = await _engine.HandleStepFailureAsync(null!, error, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Step execution cannot be null");
    }

    #endregion

    #region ValidateExecutionAsync Tests

    [Fact]
    public async Task ValidateExecutionAsync_WithValidExecution_ShouldReturnValid()
    {
        // Arrange
        var execution = new WorkflowExecution
        {
            WorkflowId = Guid.NewGuid(),
            Workflow = CreateTestWorkflow(Guid.NewGuid())
        };

        // Act
        var result = await _engine.ValidateExecutionAsync(execution, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.Issues.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateExecutionAsync_WithNoWorkflowId_ShouldReturnInvalid()
    {
        // Arrange
        var execution = new WorkflowExecution
        {
            WorkflowId = Guid.Empty,
            Workflow = null
        };

        // Act
        var result = await _engine.ValidateExecutionAsync(execution, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Issues.Should().Contain("Workflow ID is required");
    }

    [Fact]
    public async Task ValidateExecutionAsync_WithNoSteps_ShouldReturnInvalid()
    {
        // Arrange
        var execution = new WorkflowExecution
        {
            WorkflowId = Guid.NewGuid(),
            Workflow = new Workflow
            {
                Id = Guid.NewGuid(),
                Name = "Empty Workflow",
                Steps = new List<WorkflowStep>()
            }
        };

        // Act
        var result = await _engine.ValidateExecutionAsync(execution, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
        result.Value.Issues.Should().Contain("Workflow must have at least one step");
    }

    [Fact]
    public async Task ValidateExecutionAsync_WithNullExecution_ShouldReturnFailure()
    {
        // Act
        var result = await _engine.ValidateExecutionAsync(null!, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Execution cannot be null");
    }

    #endregion

    #region UpdateExecutionProgressAsync Tests

    [Fact]
    public async Task UpdateExecutionProgressAsync_WithValidExecution_ShouldUpdateSuccessfully()
    {
        // Arrange
        var execution = new WorkflowExecution { Id = Guid.NewGuid() };

        _executionRepository.UpdateExecutionAsync(execution, Arg.Any<CancellationToken>())
            .Returns(Result<WorkflowExecution>.WithSuccess(execution));

        // Act
        var result = await _engine.UpdateExecutionProgressAsync(execution, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        await _executionRepository.Received().UpdateExecutionAsync(execution, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateExecutionProgressAsync_WithRepositoryFailure_ShouldReturnFailure()
    {
        // Arrange
        var execution = new WorkflowExecution { Id = Guid.NewGuid() };

        _executionRepository.UpdateExecutionAsync(execution, Arg.Any<CancellationToken>())
            .Returns(Result<WorkflowExecution>.WithFailure("Repository error"));

        // Act
        var result = await _engine.UpdateExecutionProgressAsync(execution, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to update execution progress");
    }

    [Fact]
    public async Task UpdateExecutionProgressAsync_WithNullExecution_ShouldReturnFailure()
    {
        // Act
        var result = await _engine.UpdateExecutionProgressAsync(null!, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Execution cannot be null");
    }

    #endregion

    #region Helper Methods

    private static Workflow CreateTestWorkflow(Guid workflowId)
    {
        return new Workflow
        {
            Id = workflowId,
            Name = "Test Workflow",
            Description = "A test workflow for unit testing",
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Id = Guid.NewGuid(),
                    Name = "Step 1",
                    Order = 1,
                    Type = "TestStep",
                    Configuration = new Dictionary<string, object>()
                },
                new WorkflowStep
                {
                    Id = Guid.NewGuid(),
                    Name = "Step 2",
                    Order = 2,
                    Type = "TestStep",
                    Configuration = new Dictionary<string, object>()
                }
            }
        };
    }

    private static WorkflowStep CreateTestWorkflowStep()
    {
        return new WorkflowStep
        {
            Id = Guid.NewGuid(),
            Name = "Test Step",
            Order = 1,
            Type = "TestStep",
            Configuration = new Dictionary<string, object>
            {
                ["testConfig"] = "value"
            }
        };
    }

    #endregion
}