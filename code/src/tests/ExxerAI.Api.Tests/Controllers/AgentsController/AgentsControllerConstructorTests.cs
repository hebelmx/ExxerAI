using ExxerAI.Api.Controllers;
using ExxerAI.Application.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace ExxerAI.Api.Tests.Controllers.Agents;

/// <summary>
/// Unit tests for AgentsController constructor validation
/// </summary>
public class AgentsControllerConstructorTests
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