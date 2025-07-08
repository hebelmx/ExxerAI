using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ExxerAI.Api.Tests.Controllers.AgentsControllerTests;

/// <summary>
/// Unit tests for GET /api/agents/{id} endpoint
/// </summary>
public class GetAgentByIdEndpointTests
{
    private readonly IAgentService _mockAgentService;
    private readonly ILogger<Api.Controllers.AgentsController> _logger;
    private readonly Api.Controllers.AgentsController _controller;

    public GetAgentByIdEndpointTests(ITestOutputHelper testOutputHelper)
    {
        _mockAgentService = Substitute.For<IAgentService>();
        _logger = XUnitLogger.CreateLogger<Api.Controllers.AgentsController>(testOutputHelper);
        _controller = new Api.Controllers.AgentsController(_mockAgentService, _logger);
    }

    [Fact]
    public async Task Should_ReturnOkWithAgent_When_AgentExists()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var agent = new Agent { Id = agentId, Name = "Test Agent", Description = "Test Description" };

        _mockAgentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(Result<Agent>.Success(agent));

        // Act
        var result = await _controller.GetAgent(agentId);

        // Assert
        var actionResult = result.Result;
        actionResult.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)actionResult;
        var response = (ApiResponse<AgentResponse>)okResult.Value!;
        response.Success.ShouldBeTrue();
        response.Data.ShouldNotBeNull();
        response.Data.Name.ShouldBe(agent.Name);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AgentDoesNotExist()
    {
        // Arrange
        var agentId = Guid.NewGuid();

        _mockAgentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(Result<Agent>.WithFailure("Agent not found"));

        // Act
        var result = await _controller.GetAgent(agentId);

        // Assert
        var actionResult = result.Result;
        actionResult.ShouldBeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)actionResult;
        var response = (ApiResponse<object>)notFoundResult.Value!;
        response.Success.ShouldBeFalse();
        response.Errors.ShouldContain("Agent not found");
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_EmptyGuidProvided()
    {
        // Act
        var result = await _controller.GetAgent(Guid.Empty);

        // Assert
        var actionResult = result.Result;
        actionResult.ShouldBeOfType<ObjectResult>();
        var objectResult = (ObjectResult)actionResult;
        objectResult.StatusCode.ShouldBe(500);
        var response = (ApiResponse<object>)objectResult.Value!;
        response.Success.ShouldBeFalse();
    }
}