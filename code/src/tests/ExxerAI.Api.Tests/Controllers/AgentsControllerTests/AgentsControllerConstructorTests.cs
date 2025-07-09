namespace ExxerAI.Api.Tests.Controllers.AgentsControllerTests;

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
        var logger = Substitute.For<ILogger<Api.Controllers.AgentsController>>();

        // Act
        var controller = new Api.Controllers.AgentsController(service, logger);

        // Assert
        controller.ShouldNotBeNull();
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_ServiceIsNull()
    {
        // Arrange
        var logger = Substitute.For<ILogger<Api.Controllers.AgentsController>>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Api.Controllers.AgentsController(null!, logger))
            .ParamName.ShouldBe("agentService");
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_LoggerIsNull()
    {
        // Arrange
        var service = Substitute.For<IAgentService>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new Api.Controllers.AgentsController(service, null!))
            .ParamName.ShouldBe("logger");
    }
} 