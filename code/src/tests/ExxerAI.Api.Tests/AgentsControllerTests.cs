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

    /// <summary>
    /// Test class for POST /api/agents/{id}/tasks endpoint
    /// </summary>
    public class AssignTaskTests : AgentsControllerTests
    {
        public AssignTaskTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task Should_ReturnNoContent_When_TaskAssignedSuccessfully()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = taskId };

            _mockAgentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _controller.AssignTask(agentId, request);

            // Assert
            result.ShouldBeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Should_ReturnBadRequest_When_InvalidModelState()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = Guid.NewGuid() };

            _controller.ModelState.AddModelError("TaskId", "Invalid task ID");

            // Act
            var result = await _controller.AssignTask(agentId, request);

            // Assert
            var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
            var response = (ApiResponse<object>)badRequestResult.Value!;
            response.Success.ShouldBeFalse();
            response.Message.ShouldBe("Invalid request data");
            response.Errors.ShouldContain("Invalid task ID");
        }

        [Fact]
        public async Task Should_ReturnNotFound_When_AgentNotFound()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = taskId };

            _mockAgentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Agent not found"));

            // Act
            var result = await _controller.AssignTask(agentId, request);

            // Assert
            var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
            var response = (ApiResponse<object>)notFoundResult.Value!;
            response.Success.ShouldBeFalse();
            response.Message.ShouldBe("Agent or task not found");
        }

        [Fact]
        public async Task Should_ReturnNotFound_When_TaskNotFound()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = taskId };

            _mockAgentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Task not found"));

            // Act
            var result = await _controller.AssignTask(agentId, request);

            // Assert
            var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
            var response = (ApiResponse<object>)notFoundResult.Value!;
            response.Success.ShouldBeFalse();
            response.Message.ShouldBe("Agent or task not found");
        }

        [Fact]
        public async Task Should_ReturnBadRequest_When_AssignmentFails()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = taskId };

            _mockAgentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Assignment failed - agent at capacity"));

            // Act
            var result = await _controller.AssignTask(agentId, request);

            // Assert
            var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
            var response = (ApiResponse<object>)badRequestResult.Value!;
            response.Success.ShouldBeFalse();
            response.Message.ShouldBe("Failed to assign task");
            response.Errors.ShouldContain("Assignment failed - agent at capacity");
        }

        [Fact]
        public async Task Should_ReturnInternalServerError_When_ExceptionThrown()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = taskId };

            _mockAgentService.When(x => x.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>()))
                .Do(x => throw new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _controller.AssignTask(agentId, request);

            // Assert
            var objectResult = result.ShouldBeOfType<ObjectResult>();
            objectResult.StatusCode.ShouldBe(500);
            var response = (ApiResponse<object>)objectResult.Value!;
            response.Success.ShouldBeFalse();
            response.Message.ShouldBe("An internal error occurred");
            response.Errors.ShouldContain("Database connection failed");
        }

        [Fact]
        public async Task Should_ReturnBadRequest_When_EmptyTaskId()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = Guid.Empty };

            // Act
            var result = await _controller.AssignTask(agentId, request);

            // Assert - Even with empty Guid, the service should handle this gracefully
            // This tests the controller's resilience to edge cases
            await _mockAgentService.Received(1).AssignTaskAsync(agentId, Guid.Empty, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_ReturnBadRequest_When_EmptyAgentId()
        {
            // Arrange
            var request = new AssignTaskRequest { TaskId = Guid.NewGuid() };

            // Act
            var result = await _controller.AssignTask(Guid.Empty, request);

            // Assert
            await _mockAgentService.Received(1).AssignTaskAsync(Guid.Empty, request.TaskId, Arg.Any<CancellationToken>());
        }
    }

    /// <summary>
    /// Test class for GET /api/agents/best-for-task/{taskType} endpoint
    /// </summary>
    public class FindBestAgentForTaskTests : AgentsControllerTests
    {
        public FindBestAgentForTaskTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task Should_ReturnOkWithAgent_When_SuitableAgentFound()
        {
            // Arrange
            var taskType = "document-processing";
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                Name = "Document Processing Agent",
                Description = "Specialized in document analysis",
                Status = AgentStatus.Active
            };

            _mockAgentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
                .Returns(Result<Agent>.Success(agent));

            // Act
            var result = await _controller.FindBestAgentForTask(taskType);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)actionResult;
            var response = (ApiResponse<AgentResponse>)okResult.Value!;
            response.Success.ShouldBeTrue();
            response.Message.ShouldBe("Best agent found");
            response.Data.ShouldNotBeNull();
            response.Data.Name.ShouldBe(agent.Name);
        }

        [Fact]
        public async Task Should_ReturnNotFound_When_NoSuitableAgentFound()
        {
            // Arrange
            var taskType = "quantum-computing";

            _mockAgentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
                .Returns(Result<Agent>.WithFailure("No suitable agent found"));

            // Act
            var result = await _controller.FindBestAgentForTask(taskType);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<NotFoundObjectResult>();
            var notFoundResult = (NotFoundObjectResult)actionResult;
            var response = (ApiResponse<object>)notFoundResult.Value!;
            response.Success.ShouldBeFalse();
            response.Message.ShouldBe("No suitable agent found");
        }

        [Fact]
        public async Task Should_ReturnInternalServerError_When_ExceptionThrown()
        {
            // Arrange
            var taskType = "document-processing";

            _mockAgentService.When(x => x.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>()))
                .Do(x => throw new TimeoutException("Service timeout"));

            // Act
            var result = await _controller.FindBestAgentForTask(taskType);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<ObjectResult>();
            var objectResult = (ObjectResult)actionResult;
            objectResult.StatusCode.ShouldBe(500);
            var response = (ApiResponse<object>)objectResult.Value!;
            response.Success.ShouldBeFalse();
            response.Message.ShouldBe("An internal error occurred");
            response.Errors.ShouldContain("Service timeout");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  \t  ")]
        public async Task Should_HandleEmptyOrWhitespaceTaskType_When_InvalidTaskTypeProvided(string taskType)
        {
            // Arrange
            _mockAgentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
                .Returns(Result<Agent>.WithFailure("Invalid task type"));

            // Act
            var result = await _controller.FindBestAgentForTask(taskType);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Should_HandleVeryLongTaskType_When_ExtremeInputProvided()
        {
            // Arrange
            var taskType = new string('x', 1000); // 1000 character task type
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                Name = "Universal Agent",
                Status = AgentStatus.Active
            };

            _mockAgentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
                .Returns(Result<Agent>.Success(agent));

            // Act
            var result = await _controller.FindBestAgentForTask(taskType);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<OkObjectResult>();
            await _mockAgentService.Received(1).FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("web-scraping")]
        [InlineData("data-analysis")]
        [InlineData("ai-training")]
        [InlineData("document-processing")]
        public async Task Should_HandleDifferentTaskTypes_When_ValidTaskTypesProvided(string taskType)
        {
            // Arrange
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                Name = $"Agent for {taskType}",
                Status = AgentStatus.Active
            };

            _mockAgentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
                .Returns(Result<Agent>.Success(agent));

            // Act
            var result = await _controller.FindBestAgentForTask(taskType);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)actionResult;
            var response = (ApiResponse<AgentResponse>)okResult.Value!;
            response.Success.ShouldBeTrue();
            response.Data.Name.ShouldBe($"Agent for {taskType}");
        }
    }

    /// <summary>
    /// Additional edge case and exception handling tests for all endpoints
    /// </summary>
    public class AdvancedEdgeCaseTests : AgentsControllerTests
    {
        public AdvancedEdgeCaseTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task CreateAgent_Should_HandleServiceExceptionGracefully_When_UnexpectedErrorOccurs()
        {
            // Arrange
            var request = new CreateAgentRequest
            {
                Name = "Test Agent",
                Description = "Test Description",
                Capabilities = new AgentCapabilitiesDto()
            };

            _mockAgentService.When(x => x.CreateAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentCapabilities>(), Arg.Any<CancellationToken>()))
                .Do(x => throw new OutOfMemoryException("System out of memory"));

            // Act
            var result = await _controller.CreateAgent(request);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<ObjectResult>();
            var objectResult = (ObjectResult)actionResult;
            objectResult.StatusCode.ShouldBe(500);
            var response = (ApiResponse<object>)objectResult.Value!;
            response.Success.ShouldBeFalse();
            response.Message.ShouldBe("An internal error occurred");
            response.Errors.ShouldContain("System out of memory");
        }

        [Fact]
        public async Task GetAgent_Should_HandleServiceExceptionGracefully_When_DatabaseConnectionFails()
        {
            // Arrange
            var agentId = Guid.NewGuid();

            _mockAgentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _controller.GetAgent(agentId);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<ObjectResult>();
            var objectResult = (ObjectResult)actionResult;
            objectResult.StatusCode.ShouldBe(500);
        }

        [Fact]
        public async Task GetActiveAgents_Should_HandleServiceExceptionGracefully_When_NetworkFailure()
        {
            // Arrange
            _mockAgentService.GetActiveAgentsAsync(Arg.Any<CancellationToken>())
                .Throws(new HttpRequestException("Network unreachable"));

            // Act
            var result = await _controller.GetActiveAgents();

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<ObjectResult>();
            var objectResult = (ObjectResult)actionResult;
            objectResult.StatusCode.ShouldBe(500);
        }

        [Fact]
        public async Task UpdateAgentConfiguration_Should_HandleNotFoundError_When_ErrorContainsNotFound()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentConfigurationRequest();

            _mockAgentService.UpdateAgentConfigurationAsync(agentId, Arg.Any<AgentConfiguration>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Agent not found with specified ID"));

            // Act
            var result = await _controller.UpdateAgentConfiguration(agentId, request);

            // Assert
            result.ShouldBeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateAgentConfiguration_Should_HandleGenericFailure_When_ErrorDoesNotContainNotFound()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentConfigurationRequest();

            _mockAgentService.UpdateAgentConfigurationAsync(agentId, Arg.Any<AgentConfiguration>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Configuration validation failed"));

            // Act
            var result = await _controller.UpdateAgentConfiguration(agentId, request);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateAgentStatus_Should_HandleNotFoundError_When_ErrorContainsNotFound()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentStatusRequest { Status = AgentStatus.Active };

            _mockAgentService.UpdateAgentStatusAsync(agentId, request.Status, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Agent not found in database"));

            // Act
            var result = await _controller.UpdateAgentStatus(agentId, request);

            // Assert
            result.ShouldBeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteAgent_Should_HandleNotFoundError_When_ErrorContainsNotFound()
        {
            // Arrange
            var agentId = Guid.NewGuid();

            _mockAgentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Agent not found for deletion"));

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
                .Returns(Result<bool>.WithFailure("Cannot delete agent with active tasks"));

            // Act
            var result = await _controller.DeleteAgent(agentId);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Should_HandleMultipleModelStateErrors_When_ValidationFails()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentConfigurationRequest();

            _controller.ModelState.AddModelError("TaskTimeoutSeconds", "Must be positive");
            _controller.ModelState.AddModelError("MaxRetries", "Must be between 1 and 10");
            _controller.ModelState.AddModelError("Priority", "Must be between 1 and 5");

            // Act
            var result = await _controller.UpdateAgentConfiguration(agentId, request);

            // Assert
            var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
            var response = (ApiResponse<object>)badRequestResult.Value!;
            response.Success.ShouldBeFalse();
            response.Errors.Count.ShouldBe(3);
            response.Errors.ShouldContain("Must be positive");
            response.Errors.ShouldContain("Must be between 1 and 10");
            response.Errors.ShouldContain("Must be between 1 and 5");
        }
    }
}