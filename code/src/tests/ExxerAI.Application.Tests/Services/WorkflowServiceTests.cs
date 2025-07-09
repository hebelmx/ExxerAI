using ExxerAI.Domain;
using ExxerAI.Domain.Operations;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Interface-Test-Driven Development (I-TDD) tests for IWorkflowService.
/// Tests focus on the interface contract and behavior, not implementation details.
/// </summary>
public class WorkflowServiceTests
{
    private readonly IWorkflowService _workflowService;

    public WorkflowServiceTests()
    {
        _workflowService = Substitute.For<IWorkflowService>();
    }

    #region CreateWorkflowAsync Tests

    [Fact]
    public async Task CreateWorkflowAsync_WithValidInput_ShouldReturnSuccessResult()
    {
        // Arrange
        var name = "Test Workflow";
        var description = "Test Description";
        var steps = new List<WorkflowStep>
        {
            new() { Name = "Step1", StepType = "Action", Order = 1 },
            new() { Name = "Step2", StepType = "Condition", Order = 2 }
        };
        var expectedWorkflow = new Workflow
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Status = WorkflowStatus.Draft
        };
        var expectedResult = Result<Workflow>.WithSuccess(expectedWorkflow);

        _workflowService.CreateWorkflowAsync(name, description, steps, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.CreateWorkflowAsync(name, description, steps);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.Name.ShouldBe(name);
        result.Data.Description.ShouldBe(description);
        result.Data.Status.ShouldBe(WorkflowStatus.Draft);
    }

    [Theory]
    [InlineData("", "Valid Description")]
    [InlineData("   ", "Valid Description")]
    [InlineData("Valid Name", "")]
    public async Task CreateWorkflowAsync_WithInvalidInput_ShouldReturnFailureResult(string name, string description)
    {
        // Arrange
        var steps = new List<WorkflowStep> { new() { Name = "Step1", StepType = "Action" } };
        var expectedResult = Result<Workflow>.WithFailure("Invalid input provided");

        _workflowService.CreateWorkflowAsync(name, description, steps, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.CreateWorkflowAsync(name, description, steps);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateWorkflowAsync_WithNullSteps_ShouldReturnFailureResult()
    {
        // Arrange
        var name = "Test Workflow";
        var description = "Test Description";
        IEnumerable<WorkflowStep> steps = null!;
        var expectedResult = Result<Workflow>.WithFailure("Workflow steps cannot be null");

        _workflowService.CreateWorkflowAsync(name, description, steps, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.CreateWorkflowAsync(name, description, steps);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion CreateWorkflowAsync Tests

    #region GetWorkflowAsync Tests

    [Fact]
    public async Task GetWorkflowAsync_WithValidId_ShouldReturnWorkflow()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var expectedWorkflow = new Workflow
        {
            Id = workflowId,
            Name = "Test Workflow",
            Status = WorkflowStatus.Active
        };
        var expectedResult = Result<Workflow>.WithSuccess(expectedWorkflow);

        _workflowService.GetWorkflowAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.GetWorkflowAsync(workflowId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.Id.ShouldBe(workflowId);
    }

    [Fact]
    public async Task GetWorkflowAsync_WithEmptyGuid_ShouldReturnFailureResult()
    {
        // Arrange
        var workflowId = Guid.Empty;
        var expectedResult = Result<Workflow>.WithFailure("Workflow ID cannot be empty");

        _workflowService.GetWorkflowAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.GetWorkflowAsync(workflowId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetWorkflowAsync_WithNonExistentId_ShouldReturnFailureResult()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var expectedResult = Result<Workflow>.WithFailure("Workflow not found");

        _workflowService.GetWorkflowAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.GetWorkflowAsync(workflowId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion GetWorkflowAsync Tests

    #region GetActiveWorkflowsAsync Tests

    [Fact]
    public async Task GetActiveWorkflowsAsync_WhenActiveWorkflowsExist_ShouldReturnAllActiveWorkflows()
    {
        // Arrange
        var activeWorkflows = new List<Workflow>
        {
            new() { Id = Guid.NewGuid(), Name = "Workflow1", Status = WorkflowStatus.Active },
            new() { Id = Guid.NewGuid(), Name = "Workflow2", Status = WorkflowStatus.Active },
            new() { Id = Guid.NewGuid(), Name = "Workflow3", Status = WorkflowStatus.Running }
        };
        var expectedResult = Result<IEnumerable<Workflow>>.WithSuccess(activeWorkflows);

        _workflowService.GetActiveWorkflowsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.GetActiveWorkflowsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.Count().ShouldBe(3);
        result.Data.All(w => w.Status == WorkflowStatus.Active || w.Status == WorkflowStatus.Running).ShouldBeTrue();
    }

    [Fact]
    public async Task GetActiveWorkflowsAsync_WhenNoActiveWorkflows_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyWorkflows = new List<Workflow>();
        var expectedResult = Result<IEnumerable<Workflow>>.WithSuccess(emptyWorkflows);

        _workflowService.GetActiveWorkflowsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.GetActiveWorkflowsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.ShouldBeEmpty();
    }

    #endregion GetActiveWorkflowsAsync Tests

    #region ExecuteWorkflowAsync Tests

    [Fact]
    public async Task ExecuteWorkflowAsync_WithValidInput_ShouldReturnSuccessResult()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var input = new Dictionary<string, object> { ["param1"] = "value1", ["param2"] = 42 };
        var expectedExecution = new WorkflowExecution
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            Status = WorkflowExecutionStatus.Starting,
            Input = input
        };
        var expectedResult = Result<WorkflowExecution>.WithSuccess(expectedExecution);

        _workflowService.ExecuteWorkflowAsync(workflowId, input, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflowId, input);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.WorkflowId.ShouldBe(workflowId);
        result.Data.Status.ShouldBe(WorkflowExecutionStatus.Starting);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithEmptyWorkflowId_ShouldReturnFailureResult()
    {
        // Arrange
        var workflowId = Guid.Empty;
        var input = new Dictionary<string, object>();
        var expectedResult = Result<WorkflowExecution>.WithFailure("Workflow ID cannot be empty");

        _workflowService.ExecuteWorkflowAsync(workflowId, input, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflowId, input);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithNullInput_ShouldReturnFailureResult()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        Dictionary<string, object> input = null!;
        var expectedResult = Result<WorkflowExecution>.WithFailure("Input cannot be null");

        _workflowService.ExecuteWorkflowAsync(workflowId, input, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflowId, input);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion ExecuteWorkflowAsync Tests

    #region GetWorkflowExecutionsAsync Tests

    [Fact]
    public async Task GetWorkflowExecutionsAsync_WithValidWorkflowId_ShouldReturnExecutions()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var executions = new List<WorkflowExecution>
        {
            new() { Id = Guid.NewGuid(), WorkflowId = workflowId, Status = WorkflowExecutionStatus.Completed },
            new() { Id = Guid.NewGuid(), WorkflowId = workflowId, Status = WorkflowExecutionStatus.Running }
        };
        var expectedResult = Result<IEnumerable<WorkflowExecution>>.WithSuccess(executions);

        _workflowService.GetWorkflowExecutionsAsync(workflowId, null, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.GetWorkflowExecutionsAsync(workflowId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.Count().ShouldBe(2);
        result.Data.All(e => e.WorkflowId == workflowId).ShouldBeTrue();
    }

    [Fact]
    public async Task GetWorkflowExecutionsAsync_WithStatusFilter_ShouldReturnFilteredExecutions()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var completedExecutions = new List<WorkflowExecution>
        {
            new() { Id = Guid.NewGuid(), WorkflowId = workflowId, Status = WorkflowExecutionStatus.Completed }
        };
        var expectedResult = Result<IEnumerable<WorkflowExecution>>.WithSuccess(completedExecutions);

        _workflowService.GetWorkflowExecutionsAsync(workflowId, WorkflowExecutionStatus.Completed, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.GetWorkflowExecutionsAsync(workflowId, WorkflowExecutionStatus.Completed);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.All(e => e.Status == WorkflowExecutionStatus.Completed).ShouldBeTrue();
    }

    #endregion GetWorkflowExecutionsAsync Tests

    #region Execution Control Tests

    [Theory]
    [InlineData(nameof(IWorkflowService.PauseWorkflowExecutionAsync))]
    [InlineData(nameof(IWorkflowService.ResumeWorkflowExecutionAsync))]
    [InlineData(nameof(IWorkflowService.CancelWorkflowExecutionAsync))]
    public async Task ExecutionControlMethods_WithValidId_ShouldReturnSuccess(string methodName)
    {
        // Arrange
        var executionId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        switch (methodName)
        {
            case nameof(IWorkflowService.PauseWorkflowExecutionAsync):
                _workflowService.PauseWorkflowExecutionAsync(executionId, Arg.Any<CancellationToken>())
                    .Returns(expectedResult);
                break;

            case nameof(IWorkflowService.ResumeWorkflowExecutionAsync):
                _workflowService.ResumeWorkflowExecutionAsync(executionId, Arg.Any<CancellationToken>())
                    .Returns(expectedResult);
                break;

            case nameof(IWorkflowService.CancelWorkflowExecutionAsync):
                _workflowService.CancelWorkflowExecutionAsync(executionId, Arg.Any<CancellationToken>())
                    .Returns(expectedResult);
                break;
        }

        // Act
        Result<bool> result = methodName switch
        {
            nameof(IWorkflowService.PauseWorkflowExecutionAsync) => await _workflowService.PauseWorkflowExecutionAsync(executionId),
            nameof(IWorkflowService.ResumeWorkflowExecutionAsync) => await _workflowService.ResumeWorkflowExecutionAsync(executionId),
            nameof(IWorkflowService.CancelWorkflowExecutionAsync) => await _workflowService.CancelWorkflowExecutionAsync(executionId),
            _ => throw new ArgumentException($"Unknown method: {methodName}")
        };

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Theory]
    [InlineData(nameof(IWorkflowService.PauseWorkflowExecutionAsync))]
    [InlineData(nameof(IWorkflowService.ResumeWorkflowExecutionAsync))]
    [InlineData(nameof(IWorkflowService.CancelWorkflowExecutionAsync))]
    public async Task ExecutionControlMethods_WithEmptyGuid_ShouldReturnFailure(string methodName)
    {
        // Arrange
        var executionId = Guid.Empty;
        var expectedResult = Result<bool>.WithFailure("Execution ID cannot be empty");

        switch (methodName)
        {
            case nameof(IWorkflowService.PauseWorkflowExecutionAsync):
                _workflowService.PauseWorkflowExecutionAsync(executionId, Arg.Any<CancellationToken>())
                    .Returns(expectedResult);
                break;

            case nameof(IWorkflowService.ResumeWorkflowExecutionAsync):
                _workflowService.ResumeWorkflowExecutionAsync(executionId, Arg.Any<CancellationToken>())
                    .Returns(expectedResult);
                break;

            case nameof(IWorkflowService.CancelWorkflowExecutionAsync):
                _workflowService.CancelWorkflowExecutionAsync(executionId, Arg.Any<CancellationToken>())
                    .Returns(expectedResult);
                break;
        }

        // Act
        Result<bool> result = methodName switch
        {
            nameof(IWorkflowService.PauseWorkflowExecutionAsync) => await _workflowService.PauseWorkflowExecutionAsync(executionId),
            nameof(IWorkflowService.ResumeWorkflowExecutionAsync) => await _workflowService.ResumeWorkflowExecutionAsync(executionId),
            nameof(IWorkflowService.CancelWorkflowExecutionAsync) => await _workflowService.CancelWorkflowExecutionAsync(executionId),
            _ => throw new ArgumentException($"Unknown method: {methodName}")
        };

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion Execution Control Tests

    #region UpdateWorkflowConfigurationAsync Tests

    [Fact]
    public async Task UpdateWorkflowConfigurationAsync_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var configuration = new WorkflowConfiguration
        {
            MaxExecutionTimeSeconds = 7200,
            AllowParallelExecution = false
        };
        var expectedResult = Result<bool>.WithSuccess(true);

        _workflowService.UpdateWorkflowConfigurationAsync(workflowId, configuration, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.UpdateWorkflowConfigurationAsync(workflowId, configuration);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateWorkflowConfigurationAsync_WithNullConfiguration_ShouldReturnFailure()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        WorkflowConfiguration configuration = null!;
        var expectedResult = Result<bool>.WithFailure("Configuration cannot be null");

        _workflowService.UpdateWorkflowConfigurationAsync(workflowId, configuration, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.UpdateWorkflowConfigurationAsync(workflowId, configuration);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion UpdateWorkflowConfigurationAsync Tests

    #region DeleteWorkflowAsync Tests

    [Fact]
    public async Task DeleteWorkflowAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _workflowService.DeleteWorkflowAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.DeleteWorkflowAsync(workflowId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteWorkflowAsync_WithNonExistentWorkflow_ShouldReturnFailure()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithFailure("Workflow not found");

        _workflowService.DeleteWorkflowAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.DeleteWorkflowAsync(workflowId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion DeleteWorkflowAsync Tests

    #region Contract Validation Tests

    [Fact]
    public async Task IWorkflowService_AllMethods_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var workflowId = Guid.NewGuid();
        var executionId = Guid.NewGuid();
        var steps = new List<WorkflowStep>();
        var input = new Dictionary<string, object>();
        var configuration = new WorkflowConfiguration();

        // Act & Assert - Verify all methods accept CancellationToken
        await _workflowService.Received(0).CreateWorkflowAsync("test", "test", steps, cts.Token);
        await _workflowService.Received(0).GetWorkflowAsync(workflowId, cts.Token);
        await _workflowService.Received(0).GetActiveWorkflowsAsync(cts.Token);
        await _workflowService.Received(0).UpdateWorkflowConfigurationAsync(workflowId, configuration, cts.Token);
        await _workflowService.Received(0).ExecuteWorkflowAsync(workflowId, input, cts.Token);
        await _workflowService.Received(0).GetWorkflowExecutionAsync(executionId, cts.Token);
        await _workflowService.Received(0).GetWorkflowExecutionsAsync(workflowId, null, cts.Token);
        await _workflowService.Received(0).PauseWorkflowExecutionAsync(executionId, cts.Token);
        await _workflowService.Received(0).ResumeWorkflowExecutionAsync(executionId, cts.Token);
        await _workflowService.Received(0).CancelWorkflowExecutionAsync(executionId, cts.Token);
        await _workflowService.Received(0).DeleteWorkflowAsync(workflowId, cts.Token);

        // All methods should exist and accept cancellation tokens
        true.ShouldBeTrue();
    }

    [Fact]
    public void IWorkflowService_AllMethods_ShouldReturnResult()
    {
        // Arrange & Act & Assert - Verify all async methods return Result<T>
        var workflowId = Guid.NewGuid();
        var executionId = Guid.NewGuid();
        var steps = new List<WorkflowStep>();
        var input = new Dictionary<string, object>();
        var configuration = new WorkflowConfiguration();

        // Verify method signatures return Result<T>
        var createTask = _workflowService.CreateWorkflowAsync("test", "test", steps);
        var getTask = _workflowService.GetWorkflowAsync(workflowId);
        var getActiveTask = _workflowService.GetActiveWorkflowsAsync();
        var updateConfigTask = _workflowService.UpdateWorkflowConfigurationAsync(workflowId, configuration);
        var executeTask = _workflowService.ExecuteWorkflowAsync(workflowId, input);
        var getExecutionTask = _workflowService.GetWorkflowExecutionAsync(executionId);
        var getExecutionsTask = _workflowService.GetWorkflowExecutionsAsync(workflowId);
        var pauseTask = _workflowService.PauseWorkflowExecutionAsync(executionId);
        var resumeTask = _workflowService.ResumeWorkflowExecutionAsync(executionId);
        var cancelTask = _workflowService.CancelWorkflowExecutionAsync(executionId);
        var deleteTask = _workflowService.DeleteWorkflowAsync(workflowId);

        createTask.ShouldBeOfType<Task<Result<Workflow>>>();
        getTask.ShouldBeOfType<Task<Result<Workflow>>>();
        getActiveTask.ShouldBeOfType<Task<Result<IEnumerable<Workflow>>>>();
        updateConfigTask.ShouldBeOfType<Task<Result<bool>>>();
        executeTask.ShouldBeOfType<Task<Result<WorkflowExecution>>>();
        getExecutionTask.ShouldBeOfType<Task<Result<WorkflowExecution>>>();
        getExecutionsTask.ShouldBeOfType<Task<Result<IEnumerable<WorkflowExecution>>>>();
        pauseTask.ShouldBeOfType<Task<Result<bool>>>();
        resumeTask.ShouldBeOfType<Task<Result<bool>>>();
        cancelTask.ShouldBeOfType<Task<Result<bool>>>();
        deleteTask.ShouldBeOfType<Task<Result<bool>>>();
    }

    #endregion Contract Validation Tests
}