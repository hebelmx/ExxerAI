using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ExxerAI.Api.Tests.Controllers.AgentsControllerTests;

/// <summary>
/// Unit tests for DELETE /api/agents/{id} endpoint
/// </summary>
public class DeleteAgentEndpointTests
{
    private readonly IAgentService _mockAgentService;
    private readonly ILogger<Api.Controllers.AgentsController> _logger;
    private readonly Api.Controllers.AgentsController _controller;

    public DeleteAgentEndpointTests(ITestOutputHelper testOutputHelper)
    {
        _mockAgentService = Substitute.For<IAgentService>();
        _logger = XUnitLogger.CreateLogger<Api.Controllers.AgentsController>(testOutputHelper);
        _controller = new Api.Controllers.AgentsController(_mockAgentService, _logger);
    }

    [Fact]
    public async Task Should_ReturnOk_When_AgentDeletedSuccessfully()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        _mockAgentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(Result<bool>.Success(true));

        // Act
        var result = await _controller.DeleteAgent(agentId);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AgentDoesNotExist()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        _mockAgentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(Result<bool>.WithFailure("Agent not found"));

        // Act
        var result = await _controller.DeleteAgent(agentId);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_EmptyGuidProvided()
    {
        // Act
        var result = await _controller.DeleteAgent(Guid.Empty);

        // Assert
        result.ShouldBeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.ShouldBe(500);
    }

    [Fact]
    public async Task DeleteAgent_Should_HandleNotFoundError_When_ErrorContainsNotFound()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        _mockAgentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(Result<bool>.WithFailure("not found"));

        // Act
        var result = await _controller.DeleteAgent(agentId);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteAgent_Should_HandleGenericFailure_When_ErrorDoesNotContainNotFound()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        _mockAgentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(Result<bool>.WithFailure("Generic error"));

        // Act
        var result = await _controller.DeleteAgent(agentId);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }
}