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

    [Fact]
    public void Should_CalculateExecutionDuration_When_StartedAndCompleted()
    {
        // Arrange
        var task = new AgentTask { Title = "Duration Test" };
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddMinutes(30);

        // Act
        task.StartedAt = startTime;
        task.CompletedAt = endTime;

        // Assert
        task.ExecutionDuration.ShouldNotBeNull();
        task.ExecutionDuration.Value.TotalMinutes.ShouldBe(30, 0.1);
    }

    [Fact]
    public void Should_DetectOverdueTask_When_PastDeadline()
    {
        // Arrange
        var task = new AgentTask 
        { 
            Title = "Overdue Test",
            Deadline = DateTime.UtcNow.AddDays(-1),
            Status = TaskStatus.Pending
        };

        // Act & Assert
        task.IsOverdue.ShouldBeTrue();
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
/// Unit tests for Workflow domain entity
/// </summary>
public class WorkflowTests
{
    [Fact]
    public void Should_CreateWorkflow_When_ValidDataProvided()
    {
        // Arrange & Act
        var workflow = new Workflow
        {
            Name = "Test Workflow",
            Description = "A test workflow for development",
            Status = WorkflowStatus.Active
        };

        // Assert
        workflow.Id.ShouldNotBe(Guid.Empty);
        workflow.Name.ShouldBe("Test Workflow");
        workflow.Description.ShouldBe("A test workflow for development");
        workflow.Status.ShouldBe(WorkflowStatus.Active);
        workflow.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        workflow.Definition.ShouldNotBeNull();
        workflow.Executions.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(nameof(WorkflowStatus.Draft))]
    [InlineData(nameof(WorkflowStatus.Active))]
    [InlineData(nameof(WorkflowStatus.Running))]
    [InlineData(nameof(WorkflowStatus.Completed))]
    [InlineData(nameof(WorkflowStatus.Failed))]
    [InlineData(nameof(WorkflowStatus.Paused))]
    [InlineData(nameof(WorkflowStatus.Archived))]
    public void Should_HandleWorkflowStatuses_When_DifferentStatesProvided(string statusName)
    {
        // Arrange
        var workflow = new Workflow { Name = "Status Test" };
        var expectedStatus = EnumModel.FromName<WorkflowStatus>(statusName);

        // Act
        workflow.Status = expectedStatus;

        // Assert
        workflow.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public void Should_InitializeEmptyExecutions_When_WorkflowCreated()
    {
        // Arrange & Act
        var workflow = new Workflow();

        // Assert
        workflow.Executions.ShouldNotBeNull();
        workflow.Executions.ShouldBeEmpty();
    }
}

/// <summary>
/// Unit tests for WorkflowExecution domain entity
/// </summary>
public class WorkflowExecutionTests
{
    [Fact]
    public void Should_CreateWorkflowExecution_When_ValidDataProvided()
    {
        // Arrange & Act
        var execution = new WorkflowExecution
        {
            WorkflowId = Guid.NewGuid(),
            Status = WorkflowExecutionStatus.Running
        };

        // Assert
        execution.Id.ShouldNotBe(Guid.Empty);
        execution.WorkflowId.ShouldNotBe(Guid.Empty);
        execution.Status.ShouldBe(WorkflowExecutionStatus.Running);
        execution.StartedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        execution.Input.ShouldNotBeNull();
        execution.Output.ShouldNotBeNull();
        execution.StepExecutions.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(nameof(WorkflowExecutionStatus.Starting))]
    [InlineData(nameof(WorkflowExecutionStatus.Running))]
    [InlineData(nameof(WorkflowExecutionStatus.Completed))]
    [InlineData(nameof(WorkflowExecutionStatus.Failed))]
    [InlineData(nameof(WorkflowExecutionStatus.Cancelled))]
    [InlineData(nameof(WorkflowExecutionStatus.Paused))]
    public void Should_HandleExecutionStatuses_When_DifferentStatesProvided(string statusName)
    {
        // Arrange
        var execution = new WorkflowExecution();
        var expectedStatus = EnumModel.FromName<WorkflowExecutionStatus>(statusName);

        // Act
        execution.Status = expectedStatus;

        // Assert
        execution.Status.ShouldBe(expectedStatus);
    }
}

/// <summary>
/// Unit tests for Conversation domain entity
/// </summary>
public class ConversationTests
{
    [Fact]
    public void Should_CreateConversation_When_ValidDataProvided()
    {
        // Arrange & Act
        var conversation = new Conversation
        {
            Title = "Test Conversation",
            AgentId = Guid.NewGuid(),
            LanguageModelId = Guid.NewGuid(),
            Status = ConversationStatus.Active
        };

        // Assert
        conversation.Id.ShouldNotBe(Guid.Empty);
        conversation.Title.ShouldBe("Test Conversation");
        conversation.AgentId.ShouldNotBe(Guid.Empty);
        conversation.LanguageModelId.ShouldNotBe(Guid.Empty);
        conversation.Status.ShouldBe(ConversationStatus.Active);
        conversation.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        conversation.Messages.ShouldNotBeNull();
        conversation.Metadata.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(nameof(ConversationStatus.Active))]
    [InlineData(nameof(ConversationStatus.Paused))]
    [InlineData(nameof(ConversationStatus.Completed))]
    [InlineData(nameof(ConversationStatus.Archived))]
    public void Should_HandleConversationStatuses_When_DifferentStatesProvided(string statusName)
    {
        // Arrange
        var conversation = new Conversation();
        var expectedStatus = EnumModel.FromName<ConversationStatus>(statusName);

        // Act
        conversation.Status = expectedStatus;

        // Assert
        conversation.Status.ShouldBe(expectedStatus);
    }
}

/// <summary>
/// Unit tests for ConversationMessage domain entity
/// </summary>
public class ConversationMessageTests
{
    [Fact]
    public void Should_CreateMessage_When_ValidDataProvided()
    {
        // Arrange & Act
        var message = new ConversationMessage
        {
            ConversationId = Guid.NewGuid(),
            Role = MessageRole.User,
            Content = "Hello, world!",
            TokenCount = 3
        };

        // Assert
        message.Id.ShouldNotBe(Guid.Empty);
        message.ConversationId.ShouldNotBe(Guid.Empty);
        message.Role.ShouldBe(MessageRole.User);
        message.Content.ShouldBe("Hello, world!");
        message.TokenCount.ShouldBe(3);
        message.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        message.Metadata.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(nameof(MessageRole.System))]
    [InlineData(nameof(MessageRole.User))]
    [InlineData(nameof(MessageRole.Assistant))]
    [InlineData(nameof(MessageRole.Function))]
    public void Should_HandleMessageRoles_When_DifferentRolesProvided(string roleName)
    {
        // Arrange
        var message = new ConversationMessage();
        var expectedRole = EnumModel.FromName<MessageRole>(roleName);

        // Act
        message.Role = expectedRole;

        // Assert
        message.Role.ShouldBe(expectedRole);
    }
}

/// <summary>
/// Unit tests for TaskData value object
/// </summary>
public class TaskDataTests
{
    [Fact]
    public void Should_InitializeDefaults_When_Created()
    {
        // Arrange & Act
        var taskData = new TaskData();

        // Assert
        taskData.ContentType.ShouldBe("application/json");
        taskData.Content.ShouldBe(string.Empty);
        taskData.Properties.ShouldNotBeNull();
        taskData.Properties.ShouldBeEmpty();
    }

    [Fact]
    public void Should_AllowCustomProperties_When_Added()
    {
        // Arrange
        var taskData = new TaskData();

        // Act
        taskData.Properties["priority"] = "high";
        taskData.Properties["category"] = "development";

        // Assert
        taskData.Properties.Count.ShouldBe(2);
        taskData.Properties["priority"].ShouldBe("high");
        taskData.Properties["category"].ShouldBe("development");
    }
}

/// <summary>
/// Unit tests for TaskMetadata value object
/// </summary>
public class TaskMetadataTests
{
    [Fact]
    public void Should_InitializeEmptyCollections_When_Created()
    {
        // Arrange & Act
        var metadata = new TaskMetadata();

        // Assert
        metadata.Properties.ShouldNotBeNull();
        metadata.Properties.ShouldBeEmpty();
        metadata.Context.ShouldNotBeNull();
        metadata.Context.ShouldBeEmpty();
        metadata.Metrics.ShouldNotBeNull();
        metadata.Metrics.ShouldBeEmpty();
    }

    [Fact]
    public void Should_AllowMetricsStorage_When_Added()
    {
        // Arrange
        var metadata = new TaskMetadata();

        // Act
        metadata.Metrics["execution_time"] = 1500.5;
        metadata.Metrics["memory_usage"] = 256.0;

        // Assert
        metadata.Metrics.Count.ShouldBe(2);
        metadata.Metrics["execution_time"].ShouldBe(1500.5);
        metadata.Metrics["memory_usage"].ShouldBe(256.0);
    }
}

/// <summary>
/// Unit tests for ModelCapabilities value object
/// </summary>
public class ModelCapabilitiesTests
{
    [Fact]
    public void Should_InitializeDefaults_When_Created()
    {
        // Arrange & Act
        var capabilities = new ModelCapabilities();

        // Assert
        capabilities.SupportsTextGeneration.ShouldBeTrue();
        capabilities.SupportsCodeGeneration.ShouldBeFalse();
        capabilities.SupportsImageAnalysis.ShouldBeFalse();
        capabilities.SupportsFunctionCalling.ShouldBeFalse();
        capabilities.SupportsStreaming.ShouldBeTrue();
        capabilities.MaxOutputTokens.ShouldBe(2048);
        capabilities.SupportedFormats.ShouldNotBeNull();
        capabilities.SupportedFormats.ShouldContain("text");
    }

    [Fact]
    public void Should_AllowCapabilityConfiguration_When_Modified()
    {
        // Arrange
        var capabilities = new ModelCapabilities();

        // Act
        capabilities.SupportsCodeGeneration = true;
        capabilities.SupportsImageAnalysis = true;
        capabilities.MaxOutputTokens = 4096;

        // Assert
        capabilities.SupportsCodeGeneration.ShouldBeTrue();
        capabilities.SupportsImageAnalysis.ShouldBeTrue();
        capabilities.MaxOutputTokens.ShouldBe(4096);
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
