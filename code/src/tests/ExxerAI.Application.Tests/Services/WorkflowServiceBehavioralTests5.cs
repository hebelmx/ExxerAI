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
    public async Task CancelWorkflowExecutionAsync_Should_Return_Success_When_CancellationCompletes()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        _workflowRepository.CancelAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _service.CancelWorkflowExecutionAsync(workflowId);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task CancelWorkflowExecutionAsync_Should_Return_Failure_When_RepositoryFails()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        _workflowRepository.CancelAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result.WithFailure("Cancellation failed"));

        // Act
        var result = await _service.CancelWorkflowExecutionAsync(workflowId);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Cancellation failed");
    }
}