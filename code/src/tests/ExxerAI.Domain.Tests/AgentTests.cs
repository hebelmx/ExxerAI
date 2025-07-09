using ExxerAI.Domain;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Unit tests for Agent domain entity
/// </summary>
public class AgentTests
{
    [Fact]
    public void Should_CreateAgent_When_ValidDataProvided()
    {
        // Arrange & Act
        var agent = new Agent
        {
            Name = "TestAgent",
            Description = "A test agent for development",
            Status = AgentStatus.Active
        };

        // Assert
        agent.Id.ShouldNotBe(Guid.Empty);
        agent.Name.ShouldBe("TestAgent");
        agent.Description.ShouldBe("A test agent for development");
        agent.Status.ShouldBe(AgentStatus.Active);
        agent.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        agent.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        agent.Capabilities.ShouldNotBeNull();
        agent.Configuration.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(nameof(AgentStatus.Active), "Agent ready for tasks")]
    [InlineData(nameof(AgentStatus.Busy), "Agent processing task")]
    [InlineData(nameof(AgentStatus.Paused), "Agent paused")]
    [InlineData(nameof(AgentStatus.Error), "Agent encountered error")]
    public void Should_HandleDifferentAgentStatuses_When_VariousStatesProvided(string statusName, string _)
    {
        // Arrange
        var agent = new Agent { Name = "StatusTestAgent" };
        var expectedStatus = EnumModelHelper.FromName<AgentStatus>(statusName);

        // Act
        agent.Status = expectedStatus;

        // Assert
        agent.Status.ShouldBe(expectedStatus);
        agent.ShouldNotBeNull();
    }

    [Fact]
    public void Should_InitializeEmptyCapabilities_When_AgentCreated()
    {
        // Arrange & Act
        var agent = new Agent();

        // Assert
        agent.Capabilities.ShouldNotBeNull();
        agent.Capabilities.SupportedTaskTypes.ShouldNotBeNull();
        agent.Capabilities.SupportedTaskTypes.ShouldBeEmpty();
    }

    [Fact]
    public void Should_InitializeEmptyConfiguration_When_AgentCreated()
    {
        // Arrange & Act
        var agent = new Agent();

        // Assert
        agent.Configuration.ShouldNotBeNull();
        agent.Configuration.CustomProperties.ShouldNotBeNull();
        agent.Configuration.CustomProperties.ShouldBeEmpty();
    }
}

// AgentTaskTests moved to separate AgentTaskTests.cs file for better organization

// TaskDataTests and TaskMetadataTests moved to separate AgentTaskTests.cs file for better organization