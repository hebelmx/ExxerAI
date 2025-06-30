using ExxerAI.Domain;
using Shouldly;

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
        var expectedStatus = EnumModel.FromName<AgentStatus>(statusName);

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

/// <summary>
/// Unit tests for AgentTask domain entity
/// </summary>
public class AgentTaskTests
{
    [Fact]
    public void Should_CreateTask_When_ValidDataProvided()
    {
        // Arrange & Act
        var task = new AgentTask
        {
            Title = "Test Task",
            Description = "A test task for development",
            TaskType = "TestType",
            Priority = TaskPriority.High,
            Status = TaskStatus.Pending
        };

        // Assert
        task.Id.ShouldNotBe(Guid.Empty);
        task.Title.ShouldBe("Test Task");
        task.Description.ShouldBe("A test task for development");
        task.TaskType.ShouldBe("TestType");
        task.Priority.ShouldBe(TaskPriority.High);
        task.Status.ShouldBe(TaskStatus.Pending);
        task.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Theory]
    [InlineData(nameof(TaskPriority.Low), 1)]
    [InlineData(nameof(TaskPriority.Normal), 2)]
    [InlineData(nameof(TaskPriority.High), 3)]
    [InlineData(nameof(TaskPriority.Critical), 4)]
    public void Should_HandleTaskPriorities_When_DifferentPrioritySet(string priorityName, int expectedOrder)
    {
        // Arrange
        var task = new AgentTask { Title = "PriorityTest" };
        var expectedPriority = EnumModel.FromName<TaskPriority>(priorityName);

        // Act
        task.Priority = expectedPriority;

        // Assert
        task.Priority.ShouldBe(expectedPriority);
        ((int)task.Priority).ShouldBe(expectedOrder);
    }

    [Fact]
    public void Should_AllowNullDeadline_When_TaskCreated()
    {
        // Arrange & Act
        var task = new AgentTask { Title = "No Deadline Task" };

        // Assert
        task.Deadline.ShouldBeNull();
    }

    [Fact]
    public void Should_SetDeadline_When_DeadlineProvided()
    {
        // Arrange
        var deadline = DateTime.UtcNow.AddDays(7);
        var task = new AgentTask { Title = "Deadline Task" };

        // Act
        task.Deadline = deadline;

        // Assert
        task.Deadline.ShouldBe(deadline);
    }
}

/// <summary>
/// Unit tests for LanguageModel domain entity
/// </summary>
public class LanguageModelTests
{
    [Fact]
    public void Should_CreateLanguageModel_When_ValidDataProvided()
    {
        // Arrange & Act
        var model = new LanguageModel
        {
            Name = "gpt-4",
            Provider = "OpenAI",
            Version = "2023-07-01",
            ContextWindowSize = 8192,
            IsAvailable = true
        };

        // Assert
        model.Id.ShouldNotBe(Guid.Empty);
        model.Name.ShouldBe("gpt-4");
        model.Provider.ShouldBe("OpenAI");
        model.Version.ShouldBe("2023-07-01");
        model.ContextWindowSize.ShouldBe(8192);
        model.IsAvailable.ShouldBeTrue();
        model.Capabilities.ShouldNotBeNull();
        model.Configuration.ShouldNotBeNull();
    }

    [Fact]
    public void Should_InitializeDefaultCapabilities_When_ModelCreated()
    {
        // Arrange & Act
        var model = new LanguageModel();

        // Assert
        model.Capabilities.ShouldNotBeNull();
        model.Capabilities.SupportsTextGeneration.ShouldBeTrue();
        model.Capabilities.MaxOutputTokens.ShouldBe(2048);
        model.Capabilities.SupportedFormats.ShouldContain("text");
    }

    [Fact]
    public void Should_InitializeDefaultConfiguration_When_ModelCreated()
    {
        // Arrange & Act
        var model = new LanguageModel();

        // Assert
        model.Configuration.ShouldNotBeNull();
        model.Configuration.DefaultTemperature.ShouldBe(0.7);
        model.Configuration.MaxTokensPerRequest.ShouldBe(1000);
        model.Configuration.RequestTimeoutSeconds.ShouldBe(30);
        model.Configuration.RateLimitPerMinute.ShouldBe(60);
    }
}

/// <summary>
/// Utility class for handling enum conversions in tests
/// </summary>
public static class EnumModel
{
    /// <summary>
    /// Converts a string name to an enum value
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="name">The enum name</param>
    /// <returns>The enum value</returns>
    public static T FromName<T>(string name) where T : struct, Enum
    {
        return Enum.Parse<T>(name);
    }
}
