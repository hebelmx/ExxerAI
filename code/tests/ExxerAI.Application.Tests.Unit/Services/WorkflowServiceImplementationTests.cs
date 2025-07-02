using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.Unit.Services;

/// <summary>
/// Implementation tests for WorkflowService - targeting 286 lines of business logic
/// Tests real implementation with mocked dependencies to achieve mutation coverage
/// </summary>
public class WorkflowServiceImplementationTests
{
    private readonly IWorkflowRepository _mockRepository;
    private readonly WorkflowService _service;

    public WorkflowServiceImplementationTests()
    {
        _mockRepository = Substitute.For<IWorkflowRepository>();
        _service = new WorkflowService(_mockRepository);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_WorkflowRepositoryIsNull()
    {
        // Arrange & Act & Assert
        Should.Throw<ArgumentNullException>(() => new WorkflowService(null!))
            .ParamName.ShouldBe("workflowRepository");
    }

    [Fact]
    public void Constructor_Should_CreateInstance_When_ValidRepositoryProvided()
    {
        // Arrange & Act
        var service = new WorkflowService(_mockRepository);

        // Assert
        service.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateWorkflowAsync_Should_ReturnFailure_When_NameIsNullOrWhiteSpace(string invalidName)
    {
        // Arrange
        var description = "Test workflow";
        var steps = new List<WorkflowStep>();

        // Act
        var result = await _service.CreateWorkflowAsync(invalidName, description, steps);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Workflow name cannot be null or empty");
    }

    [Fact]
    public async Task CreateWorkflowAsync_Should_CreateWorkflow_When_ValidNameProvided()
    {
        // Arrange
        var name = "Test Workflow";
        var description = "Test Description";
        var steps = new List<WorkflowStep>
        {
            new() { Name = "Step 1", Order = 1 }
        };

        _mockRepository.AddAsync(Arg.Any<Workflow>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.WithSuccess(true));

        // Act
        var result = await _service.CreateWorkflowAsync(name, description, steps);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Name.ShouldBe(name);
        result.Value.Description.ShouldBe(description);
        result.Value.Definition.Steps.Count.ShouldBe(1);
        result.Value.Status.ShouldBe(WorkflowStatus.Draft);
        result.Value.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow);

        await _mockRepository.Received(1).AddAsync(Arg.Any<Workflow>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateWorkflowAsync_Should_HandleNullDescription_When_CreatingWorkflow()
    {
        // Arrange
        var name = "Test Workflow";
        string? description = null;
        var steps = new List<WorkflowStep>();

        _mockRepository.AddAsync(Arg.Any<Workflow>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.WithSuccess(true));

        // Act
        var result = await _service.CreateWorkflowAsync(name, description, steps);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Description.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task CreateWorkflowAsync_Should_ReturnFailure_When_RepositoryAddFails()
    {
        // Arrange
        var name = "Test Workflow";
        var description = "Test Description";
        var steps = new List<WorkflowStep>();

        _mockRepository.AddAsync(Arg.Any<Workflow>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.WithFailure("Database error"));

        // Act
        var result = await _service.CreateWorkflowAsync(name, description, steps);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Database error");
    }

    [Fact]
    public async Task GetWorkflowAsync_Should_ReturnWorkflow_When_WorkflowExists()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var expectedWorkflow = new Workflow
        {
            Id = workflowId,
            Name = "Test Workflow",
            Status = WorkflowStatus.Active
        };

        _mockRepository.GetByIdAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result<Workflow>.WithSuccess(expectedWorkflow));

        // Act
        var result = await _service.GetWorkflowAsync(workflowId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Id.ShouldBe(workflowId);
        result.Value.Name.ShouldBe("Test Workflow");

        await _mockRepository.Received(1).GetByIdAsync(workflowId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetActiveWorkflowsAsync_Should_ReturnActiveWorkflows_When_WorkflowsExist()
    {
        // Arrange
        var activeWorkflows = new List<Workflow>
        {
            new() { Id = Guid.NewGuid(), Name = "Workflow 1", Status = WorkflowStatus.Active },
            new() { Id = Guid.NewGuid(), Name = "Workflow 2", Status = WorkflowStatus.Active }
        };

        _mockRepository.GetByStatusAsync(WorkflowStatus.Active, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Workflow>>.WithSuccess(activeWorkflows));

        // Act
        var result = await _service.GetActiveWorkflowsAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Count().ShouldBe(2);
        result.Value.All(w => w.Status == WorkflowStatus.Active).ShouldBeTrue();

        await _mockRepository.Received(1).GetByStatusAsync(WorkflowStatus.Active, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_Should_CreateExecution_When_WorkflowExists()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var input = new Dictionary<string, object> { { "key1", "value1" } };
        var existingWorkflow = new Workflow { Id = workflowId, Name = "Test Workflow" };

        _mockRepository.GetByIdAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result<Workflow>.WithSuccess(existingWorkflow));

        // Act
        var result = await _service.ExecuteWorkflowAsync(workflowId, input);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.WorkflowId.ShouldBe(workflowId);
        result.Value.Input.ShouldBe(input);
        result.Value.Status.ShouldBe(WorkflowExecutionStatus.Running);
        result.Value.StartedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow);
    }

    [Fact]
    public async Task DeleteWorkflowAsync_Should_DeleteWorkflow_When_WorkflowExists()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var existingWorkflow = new Workflow { Id = workflowId, Name = "Test Workflow" };

        _mockRepository.GetByIdAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result<Workflow>.WithSuccess(existingWorkflow));
        _mockRepository.DeleteAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result<bool>.WithSuccess(true));

        // Act
        var result = await _service.DeleteWorkflowAsync(workflowId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();

        await _mockRepository.Received(1).DeleteAsync(workflowId, Arg.Any<CancellationToken>());
    }
}
