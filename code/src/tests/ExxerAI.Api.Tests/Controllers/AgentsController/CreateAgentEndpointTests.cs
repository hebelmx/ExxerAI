using ExxerAI.Api.Models;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit.Abstractions;

namespace ExxerAI.Api.Tests.Controllers.AgentsController;

/// <summary>
/// Unit tests for POST /api/agents endpoint
/// </summary>
public class CreateAgentEndpointTests
{
    private readonly IAgentService _mockAgentService;
    private readonly ILogger<Api.Controllers.AgentsController> _logger;
    private readonly Api.Controllers.AgentsController _controller;

    public CreateAgentEndpointTests(ITestOutputHelper testOutputHelper)
    {
        _mockAgentService = Substitute.For<IAgentService>();
        _logger = XUnitLogger.CreateLogger<Api.Controllers.AgentsController>(testOutputHelper);
        _controller = new Api.Controllers.AgentsController(_mockAgentService, _logger);
    }

    [Fact]
    public async Task Should_ReturnCreatedWithAgent_When_ValidRequestProvided()
    {
        // Arrange
        var request = new CreateAgentRequest
        {
            Name = "Test Agent",
            Description = "Test Description",
            Capabilities = new AgentCapabilitiesDto
            {
                SupportedTaskTypes = ["web", "api"]
            }
        };

        var createdAgent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Capabilities = new AgentCapabilities { SupportedTaskTypes = request.Capabilities.SupportedTaskTypes },
            Status = AgentStatus.Active
        };

        _mockAgentService.CreateAgentAsync(
            request.Name,
            request.Description,
            Arg.Any<AgentCapabilities>(),
            Arg.Any<CancellationToken>())
            .Returns(Result<Agent>.Success(createdAgent));

        // Act
        var result = await _controller.CreateAgent(request);

        // Assert
        var actionResult = result.Result;
        actionResult.ShouldBeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)actionResult;
        var response = (ApiResponse<AgentResponse>)createdResult.Value!;
        response.Success.ShouldBeTrue();
        response.Data.ShouldNotBeNull();
        response.Data.Name.ShouldBe(createdAgent.Name);

        // Verify route values
        createdResult.ActionName.ShouldBe(nameof(Api.Controllers.AgentsController.GetAgent));
        createdResult.RouteValues!["id"].ShouldBe(createdAgent.Id);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_InvalidRequestProvided()
    {
        // Arrange
        var request = new CreateAgentRequest(); // Invalid - missing required fields

        // Add model state error to simulate validation failure
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateAgent(request);

        // Assert
        var actionResult = result.Result;
        actionResult.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_ServiceFails()
    {
        // Arrange
        var request = new CreateAgentRequest
        {
            Name = "Test Agent",
            Description = "Test Description",
            Capabilities = new AgentCapabilitiesDto
            {
                SupportedTaskTypes = ["web", "api"]
            }
        };

        _mockAgentService.CreateAgentAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AgentCapabilities>(),
            Arg.Any<CancellationToken>())
            .Returns(Result<Agent>.WithFailure("Service error"));

        // Act
        var result = await _controller.CreateAgent(request);

        // Assert
        var actionResult = result.Result;
        actionResult.ShouldBeOfType<BadRequestObjectResult>();
        var badResult = (BadRequestObjectResult)actionResult;
        var response = (ApiResponse<object>)badResult.Value!;
        response.Success.ShouldBeFalse();
        response.Errors.ShouldContain("Service error");
    }



    [Fact]
    public async Task Should_HandleMultipleModelStateErrors_When_ValidationFails()
    {
        // Arrange
        var request = new CreateAgentRequest();
        _controller.ModelState.AddModelError("Name", "Name is required");
        _controller.ModelState.AddModelError("Description", "Description is required");
        _controller.ModelState.AddModelError("Capabilities", "Capabilities cannot be null");

        // Act
        var result = await _controller.CreateAgent(request);

        // Assert
        var actionResult = result.Result;
        actionResult.ShouldBeOfType<BadRequestObjectResult>();
    }
} 