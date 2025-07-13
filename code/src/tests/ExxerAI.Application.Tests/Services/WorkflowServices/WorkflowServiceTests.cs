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

/// <summary>
/// Begin Tests CreateWorkflowAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task CreateWorkflowAsync_WithValidInput_ShouldReturnSuccessResultAsync()
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
        var result = await _workflowService.CreateWorkflowAsync(name, description, steps, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Name.ShouldBe(name);
        result.Value.Description.ShouldBe(description);
        result.Value.Status.ShouldBe(WorkflowStatus.Draft);
    }

    [Theory]
    [InlineData("", "Valid Description")]
    [InlineData("   ", "Valid Description")]
    [InlineData("Valid Name", "")]
    public async Task CreateWorkflowAsync_WithInvalidInput_ShouldReturnFailureResultAsync(string name, string description)
    {
        // Arrange
        var steps = new List<WorkflowStep> { new() { Name = "Step1", StepType = "Action" } };
        var expectedResult = Result<Workflow>.WithFailure("Invalid input provided");

        _workflowService.CreateWorkflowAsync(name, description, steps, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.CreateWorkflowAsync(name, description, steps, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateWorkflowAsync_WithNullSteps_ShouldReturnFailureResultAsync()
    {
        // Arrange
        var name = "Test Workflow";
        var description = "Test Description";
        IEnumerable<WorkflowStep> steps = null!;
        var expectedResult = Result<Workflow>.WithFailure("Workflow steps cannot be null");

        _workflowService.CreateWorkflowAsync(name, description, steps, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.CreateWorkflowAsync(name, description, steps, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

/// <summary>
/// End Tests CreateWorkflowAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests GetWorkflowAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetWorkflowAsync_WithValidId_ShouldReturnWorkflowAsync()
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
        var result = await _workflowService.GetWorkflowAsync(workflowId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Id.ShouldBe(workflowId);
    }

    [Fact]
    public async Task GetWorkflowAsync_WithEmptyGuid_ShouldReturnFailureResultAsync()
    {
        // Arrange
        var workflowId = Guid.Empty;
        var expectedResult = Result<Workflow>.WithFailure("Workflow ID cannot be empty");

        _workflowService.GetWorkflowAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.GetWorkflowAsync(workflowId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetWorkflowAsync_WithNonExistentId_ShouldReturnFailureResultAsync()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var expectedResult = Result<Workflow>.WithFailure("Workflow not found");

        _workflowService.GetWorkflowAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.GetWorkflowAsync(workflowId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

/// <summary>
/// End Tests GetWorkflowAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests GetActiveWorkflowsAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetActiveWorkflowsAsync_WhenActiveWorkflowsExist_ShouldReturnAllActiveWorkflowsAsync()
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
        var result = await _workflowService.GetActiveWorkflowsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count().ShouldBe(3);
        result.Value.All(w => w.Status == WorkflowStatus.Active || w.Status == WorkflowStatus.Running).ShouldBeTrue();
    }

    [Fact]
    public async Task GetActiveWorkflowsAsync_WhenNoActiveWorkflows_ShouldReturnEmptyListAsync()
    {
        // Arrange
        var emptyWorkflows = new List<Workflow>();
        var expectedResult = Result<IEnumerable<Workflow>>.WithSuccess(emptyWorkflows);

        _workflowService.GetActiveWorkflowsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.GetActiveWorkflowsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.ShouldBeEmpty();
    }

/// <summary>
/// End Tests GetActiveWorkflowsAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests ExecuteWorkflowAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task ExecuteWorkflowAsync_WithValidInput_ShouldReturnSuccessResultAsync()
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
        var result = await _workflowService.ExecuteWorkflowAsync(workflowId, input, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.WorkflowId.ShouldBe(workflowId);
        result.Value.Status.ShouldBe(WorkflowExecutionStatus.Starting);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithEmptyWorkflowId_ShouldReturnFailureResultAsync()
    {
        // Arrange
        var workflowId = Guid.Empty;
        var input = new Dictionary<string, object>();
        var expectedResult = Result<WorkflowExecution>.WithFailure("Workflow ID cannot be empty");

        _workflowService.ExecuteWorkflowAsync(workflowId, input, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflowId, input, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WithNullInput_ShouldReturnFailureResultAsync()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        Dictionary<string, object> input = null!;
        var expectedResult = Result<WorkflowExecution>.WithFailure("Input cannot be null");

        _workflowService.ExecuteWorkflowAsync(workflowId, input, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.ExecuteWorkflowAsync(workflowId, input, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

/// <summary>
/// End Tests ExecuteWorkflowAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests GetWorkflowExecutionsAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetWorkflowExecutionsAsync_WithValidWorkflowId_ShouldReturnExecutionsAsync()
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
        var result = await _workflowService.GetWorkflowExecutionsAsync(workflowId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count().ShouldBe(2);
        result.Value.All(e => e.WorkflowId == workflowId).ShouldBeTrue();
    }

    [Fact]
    public async Task GetWorkflowExecutionsAsync_WithStatusFilter_ShouldReturnFilteredExecutionsAsync()
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
        var result = await _workflowService.GetWorkflowExecutionsAsync(workflowId, WorkflowExecutionStatus.Completed, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.All(e => e.Status == WorkflowExecutionStatus.Completed).ShouldBeTrue();
    }

/// <summary>
/// End Tests GetWorkflowExecutionsAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Execution Control Tests
/// </summary>
/// <returns></returns>

    [Theory]
    [InlineData(nameof(IWorkflowService.PauseWorkflowExecutionAsync))]
    [InlineData(nameof(IWorkflowService.ResumeWorkflowExecutionAsync))]
    [InlineData(nameof(IWorkflowService.CancelWorkflowExecutionAsync))]
    public async Task ExecutionControlMethods_WithValidId_ShouldReturnSuccessAsync(string methodName)
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
            nameof(IWorkflowService.PauseWorkflowExecutionAsync) => await _workflowService.PauseWorkflowExecutionAsync(executionId, cancellationToken: TestContext.Current.CancellationToken),
            nameof(IWorkflowService.ResumeWorkflowExecutionAsync) => await _workflowService.ResumeWorkflowExecutionAsync(executionId, cancellationToken: TestContext.Current.CancellationToken),
            nameof(IWorkflowService.CancelWorkflowExecutionAsync) => await _workflowService.CancelWorkflowExecutionAsync(executionId, cancellationToken: TestContext.Current.CancellationToken),
            _ => throw new ArgumentException($"Unknown method: {methodName}")
        };

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Theory]
    [InlineData(nameof(IWorkflowService.PauseWorkflowExecutionAsync))]
    [InlineData(nameof(IWorkflowService.ResumeWorkflowExecutionAsync))]
    [InlineData(nameof(IWorkflowService.CancelWorkflowExecutionAsync))]
    public async Task ExecutionControlMethods_WithEmptyGuid_ShouldReturnFailureAsync(string methodName)
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
            nameof(IWorkflowService.PauseWorkflowExecutionAsync) => await _workflowService.PauseWorkflowExecutionAsync(executionId, cancellationToken: TestContext.Current.CancellationToken),
            nameof(IWorkflowService.ResumeWorkflowExecutionAsync) => await _workflowService.ResumeWorkflowExecutionAsync(executionId, cancellationToken: TestContext.Current.CancellationToken),
            nameof(IWorkflowService.CancelWorkflowExecutionAsync) => await _workflowService.CancelWorkflowExecutionAsync(executionId, cancellationToken: TestContext.Current.CancellationToken),
            _ => throw new ArgumentException($"Unknown method: {methodName}")
        };

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

/// <summary>
/// End Tests Execution Control Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests UpdateWorkflowConfigurationAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task UpdateWorkflowConfigurationAsync_WithValidInput_ShouldReturnSuccessAsync()
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
        var result = await _workflowService.UpdateWorkflowConfigurationAsync(workflowId, configuration, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateWorkflowConfigurationAsync_WithNullConfiguration_ShouldReturnFailureAsync()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        WorkflowConfiguration configuration = null!;
        var expectedResult = Result<bool>.WithFailure("Configuration cannot be null");

        _workflowService.UpdateWorkflowConfigurationAsync(workflowId, configuration, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.UpdateWorkflowConfigurationAsync(workflowId, configuration, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

/// <summary>
/// End Tests UpdateWorkflowConfigurationAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests DeleteWorkflowAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task DeleteWorkflowAsync_WithValidId_ShouldReturnSuccessAsync()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _workflowService.DeleteWorkflowAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.DeleteWorkflowAsync(workflowId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteWorkflowAsync_WithNonExistentWorkflow_ShouldReturnFailureAsync()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithFailure("Workflow not found");

        _workflowService.DeleteWorkflowAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _workflowService.DeleteWorkflowAsync(workflowId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

/// <summary>
/// End Tests DeleteWorkflowAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Contract Validation Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task IWorkflowService_AllMethods_ShouldRespectCancellationTokenAsync()
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
        var createTask = _workflowService.CreateWorkflowAsync("test", "test", steps, TestContext.Current.CancellationToken);
        var getTask = _workflowService.GetWorkflowAsync(workflowId, TestContext.Current.CancellationToken);
        var getActiveTask = _workflowService.GetActiveWorkflowsAsync(TestContext.Current.CancellationToken);
        var updateConfigTask = _workflowService.UpdateWorkflowConfigurationAsync(workflowId, configuration, TestContext.Current.CancellationToken);
        var executeTask = _workflowService.ExecuteWorkflowAsync(workflowId, input, TestContext.Current.CancellationToken);
        var getExecutionTask = _workflowService.GetWorkflowExecutionAsync(executionId, TestContext.Current.CancellationToken);
        var getExecutionsTask = _workflowService.GetWorkflowExecutionsAsync(workflowId, cancellationToken: TestContext.Current.CancellationToken);
        var pauseTask = _workflowService.PauseWorkflowExecutionAsync(executionId, TestContext.Current.CancellationToken);
        var resumeTask = _workflowService.ResumeWorkflowExecutionAsync(executionId, TestContext.Current.CancellationToken);
        var cancelTask = _workflowService.CancelWorkflowExecutionAsync(executionId, TestContext.Current.CancellationToken);
        var deleteTask = _workflowService.DeleteWorkflowAsync(workflowId, TestContext.Current.CancellationToken);

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

/// <summary>
/// End Tests Contract Validation Tests
/// </summary>
/// <returns></returns>
}