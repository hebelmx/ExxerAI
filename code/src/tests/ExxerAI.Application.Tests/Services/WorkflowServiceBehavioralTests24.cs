using Microsoft.Extensions.Logging;

namespace ExxerAI.Application.Tests.Services;

public class WorkflowServiceBehavioralTests24
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly ILogger<WorkflowService> _logger;
    private readonly WorkflowService _service;

    public WorkflowServiceBehavioralTests24()
    {
        _workflowRepository = Substitute.For<IWorkflowRepository>();
        _logger = Substitute.For<ILogger<WorkflowService>>();
        _service = new WorkflowService(_workflowRepository, _logger);
    }

    [Fact]
    public async Task CancelWorkflowExecutionAsync_Should_Return_Success_When_CancellationCompletes()
    {
        var workflowId = Guid.NewGuid();
        _workflowRepository.CancelAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _service.CancelWorkflowExecutionAsync(workflowId);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task CancelWorkflowExecutionAsync_Should_Return_Failure_When_RepositoryFails()
    {
        var workflowId = Guid.NewGuid();
        _workflowRepository.CancelAsync(workflowId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Cancellation failed"));

        var result = await _service.CancelWorkflowExecutionAsync(workflowId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Cancellation failed");
    }
}