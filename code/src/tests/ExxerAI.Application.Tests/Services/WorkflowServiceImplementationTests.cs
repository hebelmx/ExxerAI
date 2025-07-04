using ExxerAI.Domain;

namespace ExxerAI.Application.Tests.Services;

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
            .Returns(callInfo => Task.FromResult(Result<Workflow>.WithSuccess(callInfo.Arg<Workflow>())));

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
}
