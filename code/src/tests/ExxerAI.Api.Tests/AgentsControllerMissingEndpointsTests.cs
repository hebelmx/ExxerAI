using ExxerAI.Api.Controllers;
using ExxerAI.Application;
using Microsoft.AspNetCore.Mvc;
using Meziantou.Extensions.Logging.Xunit.v3;
using ExxerAI.Domain.Operations;
using Xunit;

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
        public async Task Should_ReturnNoContent_When_TaskAssignedSuccessfullyAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = taskId };

            _mockAgentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _controller.AssignTaskAsync(agentId, request, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldBeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Should_ReturnBadRequest_When_InvalidModelStateAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = Guid.NewGuid() };

            _controller.ModelState.AddModelError("TaskId", "Invalid task ID");

            // Act
            var result = await _controller.AssignTaskAsync(agentId, request, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
            var response = (ApiResponse<object>)badRequestResult.Value!;
            response.Success.ShouldBeFalse();
            response.Message.ShouldBe("Invalid request data");
            response.Errors.ShouldContain("Invalid task ID");
        }

        [Fact]
        public async Task Should_ReturnNotFound_When_AgentNotFoundAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = taskId };

            _mockAgentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Agent not found"));

            // Act
            var result = await _controller.AssignTaskAsync(agentId, request, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            var notFoundResult = result.ShouldBeOfType<NotFoundObjectResult>();
            var response = (ApiResponse<object>)notFoundResult.Value!;
            response.Success.ShouldBeFalse();
            response.Message.ShouldBe("Agent or task not found");
        }

        [Fact]
        public async Task Should_ReturnBadRequest_When_AssignmentFailsAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = taskId };

            _mockAgentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Assignment failed - agent at capacity"));

            // Act
            var result = await _controller.AssignTaskAsync(agentId, request, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
            var response = (ApiResponse<object>)badRequestResult.Value!;
            response.Success.ShouldBeFalse();
            response.Message.ShouldBe("Failed to assign task");
            response.Errors.ShouldContain("Assignment failed - agent at capacity");
        }

        [Fact]
        public async Task Should_HandleEmptyTaskId_When_EmptyGuidProvidedAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = Guid.Empty };

            _mockAgentService.AssignTaskAsync(agentId, Guid.Empty, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Invalid task ID"));

            // Act
            var result = await _controller.AssignTaskAsync(agentId, request, cancellationToken: TestContext.Current.CancellationToken);

            // Assert - Controller should process the request and let service handle validation
            await _mockAgentService.Received(1).AssignTaskAsync(agentId, Guid.Empty, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_HandleMultipleValidationErrors_When_ModelStateInvalidAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = Guid.NewGuid() };

            _controller.ModelState.AddModelError("TaskId", "TaskId is required");
            _controller.ModelState.AddModelError("Priority", "Priority must be set");

            // Act
            var result = await _controller.AssignTaskAsync(agentId, request, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
            var response = (ApiResponse<object>)badRequestResult.Value!;
            response.Success.ShouldBeFalse();
            response.Errors.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Should_FilterEmptyErrorMessages_When_ModelStateHasEmptyErrorsAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new AssignTaskRequest { TaskId = Guid.NewGuid() };

            _controller.ModelState.AddModelError("TaskId", "");
            _controller.ModelState.AddModelError("Priority", "Priority is required");

            // Act
            var result = await _controller.AssignTaskAsync(agentId, request, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            var badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
            var response = (ApiResponse<object>)badRequestResult.Value!;
            response.Success.ShouldBeFalse();
            response.Errors.Count.ShouldBe(1);
            response.Errors.ShouldContain("Priority is required");
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
        public async Task Should_ReturnOkWithAgent_When_SuitableAgentFoundAsync()
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
            var result = await _controller.FindBestAgentForTaskAsync(taskType, cancellationToken: TestContext.Current.CancellationToken);

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
        public async Task Should_ReturnNotFound_When_NoSuitableAgentFoundAsync()
        {
            // Arrange
            var taskType = "quantum-computing";

            _mockAgentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
                .Returns(Result<Agent>.WithFailure("No suitable agent found"));

            // Act
            var result = await _controller.FindBestAgentForTaskAsync(taskType, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<NotFoundObjectResult>();
            var notFoundResult = (NotFoundObjectResult)actionResult;
            var response = (ApiResponse<object>)notFoundResult.Value!;
            response.Success.ShouldBeFalse();
            response.Message.ShouldBe("No suitable agent found");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  \t  ")]
        public async Task Should_HandleEmptyOrWhitespaceTaskType_When_InvalidTaskTypeProvidedAsync(string taskType)
        {
            // Arrange
            _mockAgentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
                .Returns(Result<Agent>.WithFailure("Invalid task type"));

            // Act
            var result = await _controller.FindBestAgentForTaskAsync(taskType, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<NotFoundObjectResult>();
        }

        [Theory]
        [InlineData("web-scraping")]
        [InlineData("data-analysis")]
        [InlineData("ai-training")]
        [InlineData("document-processing")]
        public async Task Should_HandleDifferentTaskTypes_When_ValidTaskTypesProvidedAsync(string taskType)
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
            var result = await _controller.FindBestAgentForTaskAsync(taskType, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)actionResult;
            var response = (ApiResponse<AgentResponse>)okResult.Value!;
            response.Success.ShouldBeTrue();
            response.Data!.Name.ShouldBe($"Agent for {taskType}");
        }

        [Fact]
        public async Task Should_HandleVeryLongTaskType_When_ExtremeInputProvidedAsync()
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
            var result = await _controller.FindBestAgentForTaskAsync(taskType, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<OkObjectResult>();
            await _mockAgentService.Received(1).FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_HandleSpecialCharactersInTaskType_When_EncodedStringProvidedAsync()
        {
            // Arrange
            var taskType = "data-analysis&processing+visualization";
            var agent = new Agent { Id = Guid.NewGuid(), Name = "Special Agent", Status = AgentStatus.Active };

            _mockAgentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
                .Returns(Result<Agent>.Success(agent));

            // Act
            var result = await _controller.FindBestAgentForTaskAsync(taskType, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<OkObjectResult>();
        }
    }

    /// <summary>
    /// Additional edge case tests for existing endpoints with better mutation coverage
    /// </summary>
    public class ExistingEndpointMutationHuntingTests : AgentsControllerMissingEndpointsTests
    {
        public ExistingEndpointMutationHuntingTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task UpdateAgentConfiguration_Should_HandleNotFoundError_When_ErrorContainsNotFoundAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentConfigurationRequest();

            _mockAgentService.UpdateAgentConfigurationAsync(agentId, Arg.Any<AgentConfiguration>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Agent not found with specified ID"));

            // Act
            var result = await _controller.UpdateAgentConfigurationAsync(agentId, request, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldBeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateAgentConfiguration_Should_HandleGenericFailure_When_ErrorDoesNotContainNotFoundAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentConfigurationRequest();

            _mockAgentService.UpdateAgentConfigurationAsync(agentId, Arg.Any<AgentConfiguration>(), Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Configuration validation failed"));

            // Act
            var result = await _controller.UpdateAgentConfigurationAsync(agentId, request, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateAgentStatus_Should_HandleNotFoundError_When_ErrorContainsNotFoundAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var request = new UpdateAgentStatusRequest { Status = AgentStatus.Active };

            _mockAgentService.UpdateAgentStatusAsync(agentId, request.Status, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Agent not found in database"));

            // Act
            var result = await _controller.UpdateAgentStatusAsync(agentId, request, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldBeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteAgent_Should_HandleNotFoundError_When_ErrorContainsNotFoundAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();

            _mockAgentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Agent not found for deletion"));

            // Act
            var result = await _controller.DeleteAgentAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldBeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteAgent_Should_HandleGenericFailure_When_ErrorDoesNotContainNotFoundAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();

            _mockAgentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Cannot delete agent with active tasks"));

            // Act
            var result = await _controller.DeleteAgentAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Should_HandleNullErrorInResult_When_ServiceReturnsNullErrorAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();

            _mockAgentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
                .Returns(Result<Agent>.WithFailure((string)null!));

            // Act
            var result = await _controller.GetAgentAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<NotFoundObjectResult>();
            var notFoundResult = (NotFoundObjectResult)actionResult;
            var response = (ApiResponse<object>)notFoundResult.Value!;
            response.Errors.ShouldContain("Agent not found");
        }

        [Fact]
        public async Task Should_HandleEmptyStringErrorInResult_When_ServiceReturnsEmptyErrorAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();

            _mockAgentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
                .Returns(Result<Agent>.WithFailure(""));

            // Act
            var result = await _controller.GetAgentAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            var actionResult = result.Result;
            actionResult.ShouldBeOfType<NotFoundObjectResult>();
            var notFoundResult = (NotFoundObjectResult)actionResult;
            var response = (ApiResponse<object>)notFoundResult.Value!;
            response.Errors.ShouldContain("Agent not found");
        }
    }
}