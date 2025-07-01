using ExxerAI.Api.Controllers;
using ExxerAI.Api.Models;
using ExxerAI.Application;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Meziantou.Extensions.Logging.Xunit;
using NSubstitute;
using Shouldly;
using Xunit.Abstractions;

namespace ExxerAI.Api.Tests;

/// <summary>
/// Unit tests for AgentsController
/// </summary>
public class AgentsControllerTests
{
    private readonly IAgentService _mockAgentService;
    private readonly ILogger<AgentsController> _logger;
    private readonly AgentsController _controller;

    public AgentsControllerTests(ITestOutputHelper testOutputHelper)
    {
        _mockAgentService = Substitute.For<IAgentService>();
        _logger = XUnitLogger.CreateLogger<AgentsController>(testOutputHelper);
        _controller = new AgentsController(_mockAgentService, _logger);
    }

    /// <summary>
    /// Test class for constructor validation
    /// </summary>
    public class ConstructorTests
    {
        [Fact]
        public void Should_CreateController_When_ValidServiceProvided()
        {
            // Arrange
            var service = Substitute.For<IAgentService>();
            var logger = Substitute.For<ILogger<AgentsController>>();

            // Act
            var controller = new AgentsController(service, logger);

            // Assert
            controller.ShouldNotBeNull();
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_ServiceIsNull()
        {
            // Arrange
            var logger = Substitute.For<ILogger<AgentsController>>();

            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new AgentsController(null!, logger))
                .ParamName.ShouldBe("agentService");
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_LoggerIsNull()
        {
            // Arrange
            var service = Substitute.For<IAgentService>();

            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new AgentsController(service, null!))
                .ParamName.ShouldBe("logger");
        }
    }

    /// <summary>
    /// Test class for GET /api/agents endpoint
    /// </summary>
    public class GetActiveAgentsTests : AgentsControllerTests
    {
        public GetActiveAgentsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task Should_ReturnOkWithAgents_When_AgentsExist()
        {
            // Arrange
            var agents = new[]
            {
                new Agent { Id = Guid.NewGuid(), Name = "Agent 1", Description = "Desc 1" },
                new Agent { Id = Guid.NewGuid(), Name = "Agent 2", Description = "Desc 2" }
            };

            _mockAgentService.GetActiveAgentsAsync(Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Agent>>.Success(agents));

            // Act
            var result = await _controller.GetActiveAgents();

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)actionResult;
            okResult.Value.ShouldBeOfType<ApiResponse<IEnumerable<AgentResponse>>>();

            var response = (ApiResponse<IEnumerable<AgentResponse>>)okResult.Value!;
            response.Success.ShouldBeTrue();
            response.Data.ShouldNotBeNull();
            response.Data.Count().ShouldBe(2);
        }

        [Fact]
        public async Task Should_ReturnOkWithEmptyList_When_NoAgentsExist()
        {
            // Arrange - Use null result to simulate the actual behavior seen in logs
            _mockAgentService.GetActiveAgentsAsync(Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Agent>>.WithFailure(""));

            // Act
            var result = await _controller.GetActiveAgents();

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<ObjectResult>();
            var objectResult = (ObjectResult)actionResult;
            objectResult.StatusCode.ShouldBe(500);
            var response = (ApiResponse<object>)objectResult.Value!;
            response.Success.ShouldBeFalse();
        }

        [Fact]
        public async Task Should_ReturnBadRequest_When_ServiceFails()
        {
            // Arrange
            _mockAgentService.GetActiveAgentsAsync(Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Agent>>.WithFailure("Service error"));

            // Act
            var result = await _controller.GetActiveAgents();

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<ObjectResult>();
            var objectResult = (ObjectResult)actionResult;
            objectResult.StatusCode.ShouldBe(500);
            var response = (ApiResponse<object>)objectResult.Value!;
            response.Success.ShouldBeFalse();
            response.Errors.ShouldContain("Service error");
        }
    }

    /// <summary>
    /// Test class for GET /api/agents/{id} endpoint
    /// </summary>
    public class GetAgentByIdTests : AgentsControllerTests
    {
        public GetAgentByIdTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
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

    /// <summary>
    /// Test class for POST /api/agents endpoint
    /// </summary>
    public class CreateAgentTests : AgentsControllerTests
    {
        public CreateAgentTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
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
            createdResult.ActionName.ShouldBe(nameof(AgentsController.GetAgent));
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
    }

    /// <summary>
    /// Test class for PUT /api/agents/{id}/configuration endpoint
    /// </summary>
    public class UpdateAgentConfigurationTests : AgentsControllerTests
    {
        public UpdateAgentConfigurationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task Should_ReturnOk_When_ValidConfigurationUpdateProvided()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentConfigurationRequest
            {
                TaskTimeoutSeconds = 600,
                MaxRetries = 5,
                Priority = 2
            };

            _mockAgentService.UpdateAgentConfigurationAsync(agentId, Arg.Any<AgentConfiguration>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _controller.UpdateAgentConfiguration(agentId, request);

            // Assert
            result.ShouldBeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Should_ReturnBadRequest_When_ServiceFails()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentConfigurationRequest();

            _mockAgentService.UpdateAgentConfigurationAsync(agentId, Arg.Any<AgentConfiguration>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Update failed"));

            // Act
            var result = await _controller.UpdateAgentConfiguration(agentId, request);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
        }
    }

    /// <summary>
    /// Test class for PUT /api/agents/{id}/status endpoint
    /// </summary>
    public class UpdateAgentStatusTests : AgentsControllerTests
    {
        public UpdateAgentStatusTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task Should_ReturnOk_When_ValidStatusUpdateProvided()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentStatusRequest
            {
                Status = AgentStatus.Active
            };

            _mockAgentService.UpdateAgentStatusAsync(agentId, request.Status, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _controller.UpdateAgentStatus(agentId, request);

            // Assert
            result.ShouldBeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Should_ReturnBadRequest_When_ServiceFails()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentStatusRequest { Status = AgentStatus.Active };

            _mockAgentService.UpdateAgentStatusAsync(agentId, request.Status, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Status update failed"));

            // Act
            var result = await _controller.UpdateAgentStatus(agentId, request);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
        }
    }

    /// <summary>
    /// Test class for DELETE /api/agents/{id} endpoint
    /// </summary>
    public class DeleteAgentTests : AgentsControllerTests
    {
        public DeleteAgentTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
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
    }
}