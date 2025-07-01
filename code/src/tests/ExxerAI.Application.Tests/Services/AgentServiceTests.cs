using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Interface-Test-Driven Development (I-TDD) tests for IAgentService.
/// Tests focus on the interface contract and behavior, not implementation details.
/// </summary>
public class AgentServiceTests
{
    private readonly IAgentService _agentService;

    public AgentServiceTests()
    {
        _agentService = Substitute.For<IAgentService>();
    }

    #region CreateAgentAsync Tests

    [Fact]
    public async Task CreateAgentAsync_WithValidInput_ShouldReturnSuccessResult()
    {
        // Arrange
        var name = "TestAgent";
        var description = "Test Description";
        var capabilities = new AgentCapabilities();
        var expectedAgent = new Agent { Id = Guid.NewGuid(), Name = name, Description = description };
        var expectedResult = Result<Agent>.WithSuccess(expectedAgent);

        _agentService.CreateAgentAsync(name, description, capabilities, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.CreateAgentAsync(name, description, capabilities);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Name.ShouldBe(name);
        result.Data.Description.ShouldBe(description);
    }

    [Theory]
    [InlineData(null, "Valid Description")]
    [InlineData("", "Valid Description")]
    [InlineData("   ", "Valid Description")]
    [InlineData("Valid Name", null)]
    [InlineData("Valid Name", "")]
    public async Task CreateAgentAsync_WithInvalidInput_ShouldReturnFailureResult(string name, string description)
    {
        // Arrange
        var capabilities = new AgentCapabilities();
        var expectedResult = Result<Agent>.WithFailure("Invalid input provided");

        _agentService.CreateAgentAsync(name, description, capabilities, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.CreateAgentAsync(name, description, capabilities);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAgentAsync_WithNullCapabilities_ShouldReturnFailureResult()
    {
        // Arrange
        var name = "TestAgent";
        var description = "Test Description";
        AgentCapabilities capabilities = null!;
        var expectedResult = Result<Agent>.WithFailure("Agent capabilities cannot be null");

        _agentService.CreateAgentAsync(name, description, capabilities, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.CreateAgentAsync(name, description, capabilities);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAgentAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var name = "TestAgent";
        var description = "Test Description";
        var capabilities = new AgentCapabilities();
        var expectedResult = Result<Agent>.WithFailure("Operation was cancelled");

        _agentService.CreateAgentAsync(name, description, capabilities, cts.Token)
            .Returns(expectedResult);

        cts.Cancel();

        // Act
        var result = await _agentService.CreateAgentAsync(name, description, capabilities, cts.Token);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("cancelled");
    }

    #endregion

    #region GetAgentAsync Tests

    [Fact]
    public async Task GetAgentAsync_WithValidId_ShouldReturnAgent()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedAgent = new Agent { Id = agentId, Name = "TestAgent" };
        var expectedResult = Result<Agent>.WithSuccess(expectedAgent);

        _agentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.GetAgentAsync(agentId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Id.ShouldBe(agentId);
    }

    [Fact]
    public async Task GetAgentAsync_WithEmptyGuid_ShouldReturnFailureResult()
    {
        // Arrange
        var agentId = Guid.Empty;
        var expectedResult = Result<Agent>.WithFailure("Agent ID cannot be empty");

        _agentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.GetAgentAsync(agentId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAgentAsync_WithNonExistentId_ShouldReturnFailureResult()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedResult = Result<Agent>.WithFailure("Agent not found");

        _agentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.GetAgentAsync(agentId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region GetAllAgentsAsync Tests

    [Fact]
    public async Task GetAllAgentsAsync_WhenAgentsExist_ShouldReturnAllAgents()
    {
        // Arrange
        var agents = new List<Agent>
        {
            new() { Id = Guid.NewGuid(), Name = "Agent1" },
            new() { Id = Guid.NewGuid(), Name = "Agent2" },
            new() { Id = Guid.NewGuid(), Name = "Agent3" }
        };
        var expectedResult = Result<IEnumerable<Agent>>.WithSuccess(agents);

        _agentService.GetAllAgentsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.GetAllAgentsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count().ShouldBe(3);
    }

    [Fact]
    public async Task GetAllAgentsAsync_WhenNoAgentsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyAgents = new List<Agent>();
        var expectedResult = Result<IEnumerable<Agent>>.WithSuccess(emptyAgents);

        _agentService.GetAllAgentsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.GetAllAgentsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldBeEmpty();
    }

    #endregion

    #region GetActiveAgentsAsync Tests

    [Fact]
    public async Task GetActiveAgentsAsync_WhenActiveAgentsExist_ShouldReturnActiveAgentsOnly()
    {
        // Arrange
        var activeAgents = new List<Agent>
        {
            new() { Id = Guid.NewGuid(), Name = "ActiveAgent1", Status = AgentStatus.Active },
            new() { Id = Guid.NewGuid(), Name = "ActiveAgent2", Status = AgentStatus.Active }
        };
        var expectedResult = Result<IEnumerable<Agent>>.WithSuccess(activeAgents);

        _agentService.GetActiveAgentsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.GetActiveAgentsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count().ShouldBe(2);
        result.Data.All(a => a.Status == AgentStatus.Active).ShouldBeTrue();
    }

    [Fact]
    public async Task GetActiveAgentsAsync_WhenNoActiveAgents_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyAgents = new List<Agent>();
        var expectedResult = Result<IEnumerable<Agent>>.WithSuccess(emptyAgents);

        _agentService.GetActiveAgentsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.GetActiveAgentsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.ShouldBeEmpty();
    }

    #endregion

    #region UpdateAgentConfigurationAsync Tests

    [Fact]
    public async Task UpdateAgentConfigurationAsync_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var configuration = new AgentConfiguration();
        var expectedResult = Result<bool>.WithSuccess(true);

        _agentService.UpdateAgentConfigurationAsync(agentId, configuration, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateAgentConfigurationAsync_WithEmptyGuid_ShouldReturnFailure()
    {
        // Arrange
        var agentId = Guid.Empty;
        var configuration = new AgentConfiguration();
        var expectedResult = Result<bool>.WithFailure("Agent ID cannot be empty");

        _agentService.UpdateAgentConfigurationAsync(agentId, configuration, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateAgentConfigurationAsync_WithNullConfiguration_ShouldReturnFailure()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        AgentConfiguration configuration = null!;
        var expectedResult = Result<bool>.WithFailure("Configuration cannot be null");

        _agentService.UpdateAgentConfigurationAsync(agentId, configuration, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region UpdateAgentStatusAsync Tests

    [Theory]
    [InlineData(AgentStatus.Active)]
    [InlineData(AgentStatus.Inactive)]
    [InlineData(AgentStatus.Paused)]
    public async Task UpdateAgentStatusAsync_WithValidInput_ShouldReturnSuccess(AgentStatus status)
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _agentService.UpdateAgentStatusAsync(agentId, status, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.UpdateAgentStatusAsync(agentId, status);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateAgentStatusAsync_WithNonExistentAgent_ShouldReturnFailure()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var status = AgentStatus.Active;
        var expectedResult = Result<bool>.WithFailure("Agent not found");

        _agentService.UpdateAgentStatusAsync(agentId, status, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.UpdateAgentStatusAsync(agentId, status);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region AssignTaskAsync Tests

    [Fact]
    public async Task AssignTaskAsync_WithValidInput_ShouldReturnSuccess()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _agentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.AssignTaskAsync(agentId, taskId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task AssignTaskAsync_WithEmptyAgentId_ShouldReturnFailure()
    {
        // Arrange
        var agentId = Guid.Empty;
        var taskId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithFailure("Agent ID cannot be empty");

        _agentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.AssignTaskAsync(agentId, taskId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task AssignTaskAsync_WithEmptyTaskId_ShouldReturnFailure()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var taskId = Guid.Empty;
        var expectedResult = Result<bool>.WithFailure("Task ID cannot be empty");

        _agentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.AssignTaskAsync(agentId, taskId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region FindBestAgentForTaskAsync Tests

    [Fact]
    public async Task FindBestAgentForTaskAsync_WithValidTaskType_ShouldReturnBestAgent()
    {
        // Arrange
        var taskType = "DocumentProcessing";
        var bestAgent = new Agent { Id = Guid.NewGuid(), Name = "DocumentProcessor" };
        var expectedResult = Result<Agent>.WithSuccess(bestAgent);

        _agentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.FindBestAgentForTaskAsync(taskType);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Id.ShouldBe(bestAgent.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task FindBestAgentForTaskAsync_WithInvalidTaskType_ShouldReturnFailure(string taskType)
    {
        // Arrange
        var expectedResult = Result<Agent>.WithFailure("Task type cannot be null or empty");

        _agentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.FindBestAgentForTaskAsync(taskType);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task FindBestAgentForTaskAsync_WithNoSuitableAgent_ShouldReturnFailure()
    {
        // Arrange
        var taskType = "UnknownTaskType";
        var expectedResult = Result<Agent>.WithFailure("No suitable agent found");

        _agentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.FindBestAgentForTaskAsync(taskType);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region DeleteAgentAsync Tests

    [Fact]
    public async Task DeleteAgentAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _agentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.DeleteAgentAsync(agentId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteAgentAsync_WithEmptyGuid_ShouldReturnFailure()
    {
        // Arrange
        var agentId = Guid.Empty;
        var expectedResult = Result<bool>.WithFailure("Agent ID cannot be empty");

        _agentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.DeleteAgentAsync(agentId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteAgentAsync_WithNonExistentAgent_ShouldReturnFailure()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithFailure("Agent not found");

        _agentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.DeleteAgentAsync(agentId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region Interface Contract Tests

    [Fact]
    public async Task IAgentService_AllMethods_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - All methods should accept and handle cancellation tokens
        await Should.NotThrowAsync(async () =>
        {
            await _agentService.CreateAgentAsync("test", "test", new AgentCapabilities(), cts.Token);
            await _agentService.GetAgentAsync(Guid.NewGuid(), cts.Token);
            await _agentService.GetAllAgentsAsync(cts.Token);
            await _agentService.GetActiveAgentsAsync(cts.Token);
            await _agentService.UpdateAgentConfigurationAsync(Guid.NewGuid(), new AgentConfiguration(), cts.Token);
            await _agentService.UpdateAgentStatusAsync(Guid.NewGuid(), AgentStatus.Active, cts.Token);
            await _agentService.AssignTaskAsync(Guid.NewGuid(), Guid.NewGuid(), cts.Token);
            await _agentService.AssignTaskToAgentAsync(Guid.NewGuid(), Guid.NewGuid(), cts.Token);
            await _agentService.FindBestAgentForTaskAsync("test", cts.Token);
            await _agentService.DeleteAgentAsync(Guid.NewGuid(), cts.Token);
        });
    }

    [Fact]
    public void IAgentService_AllMethods_ShouldReturnResult()
    {
        // Arrange & Act & Assert - All methods should return Result<T> for consistent error handling
        var serviceType = typeof(IAgentService);
        var methods = serviceType.GetMethods();

        foreach (var method in methods.Where(m => !m.IsSpecialName))
        {
            var returnType = method.ReturnType;
            
            // Should be Task<Result<T>>
            returnType.IsGenericType.ShouldBeTrue($"Method {method.Name} should return a generic type");
            returnType.GetGenericTypeDefinition().ShouldBe(typeof(Task<>), $"Method {method.Name} should return Task");
            
            var taskInnerType = returnType.GetGenericArguments()[0];
            taskInnerType.IsGenericType.ShouldBeTrue($"Method {method.Name} should return Task<Result<T>>");
            taskInnerType.GetGenericTypeDefinition().ShouldBe(typeof(Result<>), $"Method {method.Name} should return Task<Result<T>>");
        }
    }

    #endregion
} 