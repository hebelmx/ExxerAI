using ExxerAI.Api.Controllers;
using ExxerAI.Api.Models;
using ExxerAI.Application;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;

namespace ExxerAI.Api.Tests;

/// <summary>
/// Unit tests for AgentsController
/// </summary>
public class AgentsControllerTests
{
    private readonly IAgentService _mockAgentService;
    private readonly AgentsController _controller;

    public AgentsControllerTests()
    {
        _mockAgentService = Substitute.For<IAgentService>();
        _controller = new AgentsController(_mockAgentService);
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

            // Act
            var controller = new AgentsController(service);

            // Assert
            controller.ShouldNotBeNull();
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_ServiceIsNull()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new AgentsController(null!))
                .ParamName.ShouldBe("agentService");
        }
    }

    /// <summary>
    /// Test class for GET /api/agents endpoint
    /// </summary>
    public class GetAllAgentsTests : AgentsControllerTests
    {
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
            var result = await _controller.GetAllAgents();

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.ShouldBeOfType<ApiResponse<IEnumerable<Agent>>>();

            var response = (ApiResponse<IEnumerable<Agent>>)okResult.Value!;
            response.Success.ShouldBeTrue();
            response.Data.ShouldNotBeNull();
            response.Data.Count().ShouldBe(2);
        }

        [Fact]
        public async Task Should_ReturnOkWithEmptyList_When_NoAgentsExist()
        {
            // Arrange
            _mockAgentService.GetActiveAgentsAsync(Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Agent>>.Success(Array.Empty<Agent>()));

            // Act
            var result = await _controller.GetAllAgents();

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = (ApiResponse<IEnumerable<Agent>>)okResult.Value!;
            response.Success.ShouldBeTrue();
            response.Data.ShouldNotBeNull();
            response.Data.Count().ShouldBe(0);
        }

        [Fact]
        public async Task Should_ReturnBadRequest_When_ServiceFails()
        {
            // Arrange
            _mockAgentService.GetActiveAgentsAsync(Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<Agent>>.WithFailure("Service error"));

            // Act
            var result = await _controller.GetAllAgents();

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            var badResult = (BadRequestObjectResult)result;
            var response = (ApiResponse<IEnumerable<Agent>>)badResult.Value!;
            response.Success.ShouldBeFalse();
            response.Errors.ShouldContain("Service error");
        }
    }

    /// <summary>
    /// Test class for GET /api/agents/{id} endpoint
    /// </summary>
    public class GetAgentByIdTests : AgentsControllerTests
    {
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
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = (ApiResponse<Agent>)okResult.Value!;
            response.Success.ShouldBeTrue();
            response.Data.ShouldBe(agent);
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
            result.ShouldBeOfType<NotFoundObjectResult>();
            var notFoundResult = (NotFoundObjectResult)result;
            var response = (ApiResponse<Agent>)notFoundResult.Value!;
            response.Success.ShouldBeFalse();
            response.Errors.ShouldContain("Agent not found");
        }

        [Fact]
        public async Task Should_ReturnBadRequest_When_EmptyGuidProvided()
        {
            // Act
            var result = await _controller.GetAgent(Guid.Empty);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
            var badResult = (BadRequestObjectResult)result;
            var response = (ApiResponse<Agent>)badResult.Value!;
            response.Success.ShouldBeFalse();
            response.Errors.ShouldContain("Invalid agent ID");
        }
    }

    /// <summary>
    /// Test class for POST /api/agents endpoint
    /// </summary>
    public class CreateAgentTests : AgentsControllerTests
    {
        [Fact]
        public async Task Should_ReturnCreatedWithAgent_When_ValidRequestProvided()
        {
            // Arrange
            var request = new CreateAgentRequest
            {
                Name = "Test Agent",
                Description = "Test Description",
                SupportedTaskTypes = ["web", "api"]
            };

            var createdAgent = new Agent
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Capabilities = new AgentCapabilities { SupportedTaskTypes = request.SupportedTaskTypes },
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
            result.ShouldBeOfType<CreatedAtActionResult>();
            var createdResult = (CreatedAtActionResult)result;
            var response = (ApiResponse<Agent>)createdResult.Value!;
            response.Success.ShouldBeTrue();
            response.Data.ShouldBe(createdAgent);

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
            result.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Should_ReturnBadRequest_When_ServiceFails()
        {
            // Arrange
            var request = new CreateAgentRequest
            {
                Name = "Test Agent",
                Description = "Test Description",
                SupportedTaskTypes = ["web", "api"]
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
            result.ShouldBeOfType<BadRequestObjectResult>();
            var badResult = (BadRequestObjectResult)result;
            var response = (ApiResponse<Agent>)badResult.Value!;
            response.Success.ShouldBeFalse();
            response.Errors.ShouldContain("Service error");
        }
    }

    /// <summary>
    /// Test class for PUT /api/agents/{id} endpoint
    /// </summary>
    public class UpdateAgentTests : AgentsControllerTests
    {
        [Fact]
        public async Task Should_ReturnOk_When_ValidUpdateProvided()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentRequest
            {
                Name = "Updated Agent",
                Description = "Updated Description",
                Status = AgentStatus.Active
            };

            var existingAgent = new Agent { Id = agentId, Name = "Old Name" };
            var updatedAgent = new Agent { Id = agentId, Name = request.Name, Description = request.Description };

            _mockAgentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
                .Returns(Result<Agent>.Success(existingAgent));

            _mockAgentService.UpdateAgentStatusAsync(agentId, request.Status!.Value, Arg.Any<CancellationToken>())
                .Returns(Result.Success());

            // Act
            var result = await _controller.UpdateAgent(agentId, request);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = (ApiResponse<string>)okResult.Value!;
            response.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnNotFound_When_AgentDoesNotExist()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentRequest { Name = "Updated Agent" };

            _mockAgentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
                .Returns(Result<Agent>.WithFailure("Agent not found"));

            // Act
            var result = await _controller.UpdateAgent(agentId, request);

            // Assert
            result.ShouldBeOfType<NotFoundObjectResult>();
        }
    }

    /// <summary>
    /// Test class for DELETE /api/agents/{id} endpoint
    /// </summary>
    public class DeleteAgentTests : AgentsControllerTests
    {
        [Fact]
        public async Task Should_ReturnOk_When_AgentDeletedSuccessfully()
        {
            // Arrange
            var agentId = Guid.NewGuid();

            _mockAgentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
                .Returns(Result.Success());

            // Act
            var result = await _controller.DeleteAgent(agentId);

            // Assert
            result.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            var response = (ApiResponse<string>)okResult.Value!;
            response.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnNotFound_When_AgentDoesNotExist()
        {
            // Arrange
            var agentId = Guid.NewGuid();

            _mockAgentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
                .Returns(Result.WithFailure("Agent not found"));

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
            result.ShouldBeOfType<BadRequestObjectResult>();
        }
    }
}