using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ExxerAI.Application.Tests;

/// <summary>
/// TDD tests for GeneralPurposeAgent behavior
/// </summary>
public class GeneralPurposeAgentTests
{
    private readonly ILogger<GeneralPurposeAgent> _mockLogger;
    private readonly ILLMProvider _mockLLMProvider;

    public GeneralPurposeAgentTests()
    {
        _mockLogger = Substitute.For<ILogger<GeneralPurposeAgent>>();
        _mockLLMProvider = Substitute.For<ILLMProvider>();
    }

    [Fact]
    public void Should_Create_Agent_With_Default_Properties()
    {
        // Arrange & Act
        var agent = new GeneralPurposeAgent(_mockLogger, _mockLLMProvider);

        // Assert
        agent.AgentId.ShouldNotBeNullOrEmpty();
        agent.AgentType.ShouldBe("GeneralPurpose");
    }

    [Fact]
    public void Should_Create_Agent_With_Custom_Properties()
    {
        // Arrange
        var customId = "test-agent-123";
        var customType = "TestAgent";

        // Act
        var agent = new GeneralPurposeAgent(_mockLogger, _mockLLMProvider, customId, customType);

        // Assert
        agent.AgentId.ShouldBe(customId);
        agent.AgentType.ShouldBe(customType);
    }

    [Fact]
    public async Task Should_Handle_Context_When_LLM_Is_Healthy_And_Input_Valid()
    {
        // Arrange
        _mockLLMProvider.IsHealthyAsync().Returns(true);
        var context = new AgentContext { Input = "Test task" };
        var agent = new GeneralPurposeAgent(_mockLogger, _mockLLMProvider);

        // Act
        var canHandle = await agent.CanHandleAsync(context);

        // Assert
        canHandle.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Not_Handle_Context_When_LLM_Is_Unhealthy()
    {
        // Arrange
        _mockLLMProvider.IsHealthyAsync().Returns(false);
        var context = new AgentContext { Input = "Test task" };
        var agent = new GeneralPurposeAgent(_mockLogger, _mockLLMProvider);

        // Act
        var canHandle = await agent.CanHandleAsync(context);

        // Assert
        canHandle.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Not_Handle_Context_When_Input_Is_Empty()
    {
        // Arrange
        _mockLLMProvider.IsHealthyAsync().Returns(true);
        var context = new AgentContext { Input = "" };
        var agent = new GeneralPurposeAgent(_mockLogger, _mockLLMProvider);

        // Act
        var canHandle = await agent.CanHandleAsync(context);

        // Assert
        canHandle.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Execute_Successfully_When_LLM_Returns_Valid_Response()
    {
        // Arrange
        var context = new AgentContext { Input = "Test task" };
        var expectedResult = new AgentResult 
        { 
            IsSuccessful = true, 
            Output = "AI response" 
        };
        
        _mockLLMProvider.GenerateAgentResponseAsync(context, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var agent = new GeneralPurposeAgent(_mockLogger, _mockLLMProvider);

        // Act
        var result = await agent.ExecuteAsync(context);

        // Assert
        result.IsSuccessful.ShouldBeTrue();
        result.Output.ShouldBe("AI response");
    }

    [Fact]
    public async Task Should_Return_Failure_When_LLM_Throws_Exception()
    {
        // Arrange
        var context = new AgentContext { Input = "Test task" };
        _mockLLMProvider.GenerateAgentResponseAsync(context, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("LLM error"));

        var agent = new GeneralPurposeAgent(_mockLogger, _mockLLMProvider);

        // Act
        var result = await agent.ExecuteAsync(context);

        // Assert
        result.IsSuccessful.ShouldBeFalse();
        result.ErrorMessage.ShouldContain("Execution failed");
        result.ErrorMessage.ShouldContain("LLM error");
    }

    [Fact]
    public async Task Should_Return_Ready_Status_When_LLM_Is_Healthy()
    {
        // Arrange
        _mockLLMProvider.IsHealthyAsync().Returns(true);
        var agent = new GeneralPurposeAgent(_mockLogger, _mockLLMProvider, "test-agent");

        // Act
        var status = await agent.GetAgentStatusAsync();

        // Assert
        status.ShouldContain("test-agent");
        status.ShouldContain("Ready");
    }

    [Fact]
    public async Task Should_Return_Unavailable_Status_When_LLM_Is_Unhealthy()
    {
        // Arrange
        _mockLLMProvider.IsHealthyAsync().Returns(false);
        var agent = new GeneralPurposeAgent(_mockLogger, _mockLLMProvider, "test-agent");

        // Act
        var status = await agent.GetAgentStatusAsync();

        // Assert
        status.ShouldContain("test-agent");
        status.ShouldContain("LLM Provider Unavailable");
    }
} 