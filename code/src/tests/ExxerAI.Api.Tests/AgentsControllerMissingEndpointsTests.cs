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
/// Unit tests for missing AgentsController endpoints - Phase 2 mutation hunting
/// </summary>
public class AgentsControllerMissingEndpointsTests
{
	private readonly IAgentService _mockAgentService;
	private readonly ILogger<AgentsController> _logger;
	private readonly AgentsController _controller;

	public AgentsControllerMissingEndpointsTests(ITestOutputHelper testOutputHelper)
	{
		_mockAgentService = Substitute.For<IAgentService>();
		_logger = XUnitLogger.CreateLogger<AgentsController>(testOutputHelper);
		_controller = new AgentsController(_mockAgentService, _logger);
	}

	/// <summary>
	/// Test class for POST /api/agents/{id}/tasks endpoint
	/// </summary>
	public class AssignTaskTests : AgentsControllerMissingEndpointsTests
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
	}

	/// <summary>
	/// Test class for GET /api/agents/best-for-task/{taskType} endpoint
	/// </summary>
	public class FindBestAgentForTaskTests : AgentsControllerMissingEndpointsTests
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
	/// Advanced edge case and exception handling tests for mutation hunting
	/// </summary>
	public class AdvancedMutationHuntingTests : AgentsControllerMissingEndpointsTests
	{
		public AdvancedMutationHuntingTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

			_mockAgentService.CreateAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentCapabilities>(), Arg.Any<CancellationToken>())
				.Throws(new OutOfMemoryException("System out of memory"));

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

		[Fact]
		public async Task Should_HandleNullErrorInResult_When_ServiceReturnsNullError()
		{
			// Arrange
			var agentId = Guid.NewGuid();

			_mockAgentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithFailure(null!));

			// Act
			var result = await _controller.GetAgent(agentId);

			// Assert
			var actionResult = result.Result;
			actionResult.ShouldBeOfType<NotFoundObjectResult>();
			var notFoundResult = (NotFoundObjectResult)actionResult;
			var response = (ApiResponse<object>)notFoundResult.Value!;
			response.Errors.ShouldContain("Agent not found");
		}

		[Fact]
		public async Task Should_HandleEmptyStringErrorInResult_When_ServiceReturnsEmptyError()
		{
			// Arrange
			var agentId = Guid.NewGuid();

			_mockAgentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithFailure(""));

			// Act
			var result = await _controller.GetAgent(agentId);

			// Assert
			var actionResult = result.Result;
			actionResult.ShouldBeOfType<NotFoundObjectResult>();
			var notFoundResult = (NotFoundObjectResult)actionResult;
			var response = (ApiResponse<object>)notFoundResult.Value!;
			response.Errors.ShouldContain("Agent not found");
		}

		[Fact]
		public async Task Should_HandleWhitespaceErrorInResult_When_ServiceReturnsWhitespaceError()
		{
			// Arrange
			var agentId = Guid.NewGuid();

			_mockAgentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<bool>.WithFailure("   "));

			// Act
			var result = await _controller.DeleteAgent(agentId);

			// Assert
			var actionResult = result.ShouldBeOfType<BadRequestObjectResult>();
			var response = (ApiResponse<object>)actionResult.Value!;
			response.Errors.ShouldContain("Agent deletion failed");
		}
	}
} 