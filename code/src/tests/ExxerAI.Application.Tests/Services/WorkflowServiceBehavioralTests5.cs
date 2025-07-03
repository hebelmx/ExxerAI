using Microsoft.Extensions.Logging;

namespace ExxerAI.Application.Tests.Services;

public class WorkflowServiceBehavioralTests5
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly WorkflowService _service;

    public WorkflowServiceBehavioralTests5()
    {
        _workflowRepository = Substitute.For<IWorkflowRepository>();
        _service = new WorkflowService(_workflowRepository);
    }

    [Fact]
    public async Task GetWorkflowAsync_Should_Return_Success_When_WorkflowExists()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var expectedWorkflow = new Domain.Workflow { Id = workflowId, Name = "Test Workflow" };
        _workflowRepository.GetByIdAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(ExxerAI.Domain.Result<Domain.Workflow>.WithSuccess(expectedWorkflow));

        // Act
        var result = await _service.GetWorkflowAsync(workflowId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Test Workflow");
    }

    [Fact]
    public async Task GetWorkflowAsync_Should_Return_Failure_When_WorkflowNotFound()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        _workflowRepository.GetByIdAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(ExxerAI.Domain.Result<Domain.Workflow>.WithFailure("Workflow not found"));

        // Act
        var result = await _service.GetWorkflowAsync(workflowId);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Workflow not found");
    }
}