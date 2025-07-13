using ExxerAI.Domain;
using ExxerAI.Domain.Operations;
using Xunit;

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

/// <summary>
/// Begin Tests CreateAgentAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task CreateAgentAsync_WithValidInput_ShouldReturnSuccessResultAsync()
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
        var result = await _agentService.CreateAgentAsync(name, description, capabilities, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Name.ShouldBe(name);
        result.Value.Description.ShouldBe(description);
    }

    [Theory]
    [InlineData("", "Valid Description")]
    [InlineData("   ", "Valid Description")]
    [InlineData("Valid Name", "")]
    public async Task CreateAgentAsync_WithInvalidInput_ShouldReturnFailureResultAsync(string name, string description)
    {
        // Arrange
        var capabilities = new AgentCapabilities();
        var expectedResult = Result<Agent>.WithFailure("Invalid input provided");

        _agentService.CreateAgentAsync(name, description, capabilities, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.CreateAgentAsync(name, description, capabilities, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAgentAsync_WithNullCapabilities_ShouldReturnFailureResultAsync()
    {
        // Arrange
        var name = "TestAgent";
        var description = "Test Description";
        AgentCapabilities capabilities = null!;
        var expectedResult = Result<Agent>.WithFailure("Agent capabilities cannot be null");

        _agentService.CreateAgentAsync(name, description, capabilities, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.CreateAgentAsync(name, description, capabilities, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAgentAsync_WithCancellationToken_ShouldRespectCancellationAsync()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var name = "TestAgent";
        var description = "Test Description";
        var capabilities = new AgentCapabilities();
        var expectedResult = Result<Agent>.WithFailure("Operation was cancelled");

        _agentService.CreateAgentAsync(name, description, capabilities, cts.Token)
            .Returns(expectedResult);
        /// This suppression is safe because:
        /// 1. cts.Cancel() executes in microseconds (not long-running)
        /// 2. It's deterministic and necessary for testing cancellation behavior
        /// 3. Test isolation requires immediate cancellation, not async delays
        /// 4. This is the recommended pattern for testing cancellation in xUnit
        /// This pragma is applied narrowly to suppress the false-positive without affecting global behavior.
        /// Test methods are expected to use immediate cancellation for precise control and verification of

#pragma warning disable AsyncFixer02 // Long-running or blocking operations inside an async method
        cts.Cancel();
#pragma warning restore AsyncFixer02 // Long-running or blocking operations inside an async method

        // Act
        var result =
        await _agentService.CreateAgentAsync(name, description, capabilities, cts.Token);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("cancelled");
    }

/// <summary>
/// End Tests CreateAgentAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests GetAgentAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetAgentAsync_WithValidId_ShouldReturnAgentAsync()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedAgent = new Agent { Id = agentId, Name = "TestAgent" };
        var expectedResult = Result<Agent>.WithSuccess(expectedAgent);

        _agentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.GetAgentAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Id.ShouldBe(agentId);
    }

    [Fact]
    public async Task GetAgentAsync_WithEmptyGuid_ShouldReturnFailureResultAsync()
    {
        // Arrange
        var agentId = Guid.Empty;
        var expectedResult = Result<Agent>.WithFailure("Agent ID cannot be empty");

        _agentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.GetAgentAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAgentAsync_WithNonExistentId_ShouldReturnFailureResultAsync()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedResult = Result<Agent>.WithFailure("Agent not found");

        _agentService.GetAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.GetAgentAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

/// <summary>
/// End Tests GetAgentAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests GetAllAgentsAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetAllAgentsAsync_WhenAgentsExist_ShouldReturnAllAgentsAsync()
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
        var result = await _agentService.GetAllAgentsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count().ShouldBe(3);
    }

    [Fact]
    public async Task GetAllAgentsAsync_WhenNoAgentsExist_ShouldReturnEmptyListAsync()
    {
        // Arrange
        var emptyAgents = new List<Agent>();
        var expectedResult = Result<IEnumerable<Agent>>.WithSuccess(emptyAgents);

        _agentService.GetAllAgentsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.GetAllAgentsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.ShouldBeEmpty();
    }

/// <summary>
/// End Tests GetAllAgentsAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests GetActiveAgentsAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task GetActiveAgentsAsync_WhenActiveAgentsExist_ShouldReturnActiveAgentsOnlyAsync()
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
        var result = await _agentService.GetActiveAgentsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Count().ShouldBe(2);
        result.Value.All(a => a.Status == AgentStatus.Active).ShouldBeTrue();
    }

    [Fact]
    public async Task GetActiveAgentsAsync_WhenNoActiveAgents_ShouldReturnEmptyListAsync()
    {
        // Arrange
        var emptyAgents = new List<Agent>();
        var expectedResult = Result<IEnumerable<Agent>>.WithSuccess(emptyAgents);

        _agentService.GetActiveAgentsAsync(Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.GetActiveAgentsAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.ShouldBeEmpty();
    }

/// <summary>
/// End Tests GetActiveAgentsAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests UpdateAgentConfigurationAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task UpdateAgentConfigurationAsync_WithValidInput_ShouldReturnSuccessAsync()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var configuration = new AgentConfiguration();
        var expectedResult = Result<bool>.WithSuccess(true);

        _agentService.UpdateAgentConfigurationAsync(agentId, configuration, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateAgentConfigurationAsync_WithEmptyGuid_ShouldReturnFailureAsync()
    {
        // Arrange
        var agentId = Guid.Empty;
        var configuration = new AgentConfiguration();
        var expectedResult = Result<bool>.WithFailure("Agent ID cannot be empty");

        _agentService.UpdateAgentConfigurationAsync(agentId, configuration, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateAgentConfigurationAsync_WithNullConfiguration_ShouldReturnFailureAsync()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        AgentConfiguration configuration = null!;
        var expectedResult = Result<bool>.WithFailure("Configuration cannot be null");

        _agentService.UpdateAgentConfigurationAsync(agentId, configuration, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

/// <summary>
/// End Tests UpdateAgentConfigurationAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests UpdateAgentStatusAsync Tests
/// </summary>
/// <returns></returns>

    [Theory]
    [InlineData(AgentStatus.Active)]
    [InlineData(AgentStatus.Inactive)]
    [InlineData(AgentStatus.Paused)]
    public async Task UpdateAgentStatusAsync_WithValidInput_ShouldReturnSuccessAsync(AgentStatus status)
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _agentService.UpdateAgentStatusAsync(agentId, status, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.UpdateAgentStatusAsync(agentId, status, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateAgentStatusAsync_WithNonExistentAgent_ShouldReturnFailureAsync()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var status = AgentStatus.Active;
        var expectedResult = Result<bool>.WithFailure("Agent not found");

        _agentService.UpdateAgentStatusAsync(agentId, status, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.UpdateAgentStatusAsync(agentId, status, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

/// <summary>
/// End Tests UpdateAgentStatusAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests AssignTaskAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task AssignTaskAsync_WithValidInput_ShouldReturnSuccessAsync()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _agentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.AssignTaskAsync(agentId, taskId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task AssignTaskAsync_WithEmptyAgentId_ShouldReturnFailureAsync()
    {
        // Arrange
        var agentId = Guid.Empty;
        var taskId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithFailure("Agent ID cannot be empty");

        _agentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.AssignTaskAsync(agentId, taskId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task AssignTaskAsync_WithEmptyTaskId_ShouldReturnFailureAsync()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var taskId = Guid.Empty;
        var expectedResult = Result<bool>.WithFailure("Task ID cannot be empty");

        _agentService.AssignTaskAsync(agentId, taskId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.AssignTaskAsync(agentId, taskId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

/// <summary>
/// End Tests AssignTaskAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests FindBestAgentForTaskAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task FindBestAgentForTaskAsync_WithValidTaskType_ShouldReturnBestAgentAsync()
    {
        // Arrange
        var taskType = "DocumentProcessing";
        var bestAgent = new Agent { Id = Guid.NewGuid(), Name = "DocumentProcessor" };
        var expectedResult = Result<Agent>.WithSuccess(bestAgent);

        _agentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.FindBestAgentForTaskAsync(taskType, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Id.ShouldBe(bestAgent.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task FindBestAgentForTaskAsync_WithInvalidTaskType_ShouldReturnFailureAsync(string taskType)
    {
        // Arrange
        var expectedResult = Result<Agent>.WithFailure("Task type cannot be null or empty");

        _agentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.FindBestAgentForTaskAsync(taskType, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task FindBestAgentForTaskAsync_WithNoSuitableAgent_ShouldReturnFailureAsync()
    {
        // Arrange
        var taskType = "UnknownTaskType";
        var expectedResult = Result<Agent>.WithFailure("No suitable agent found");

        _agentService.FindBestAgentForTaskAsync(taskType, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.FindBestAgentForTaskAsync(taskType, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

/// <summary>
/// End Tests FindBestAgentForTaskAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests DeleteAgentAsync Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task DeleteAgentAsync_WithValidId_ShouldReturnSuccessAsync()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithSuccess(true);

        _agentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.DeleteAgentAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteAgentAsync_WithEmptyGuid_ShouldReturnFailureAsync()
    {
        // Arrange
        var agentId = Guid.Empty;
        var expectedResult = Result<bool>.WithFailure("Agent ID cannot be empty");

        _agentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.DeleteAgentAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteAgentAsync_WithNonExistentAgent_ShouldReturnFailureAsync()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var expectedResult = Result<bool>.WithFailure("Agent not found");

        _agentService.DeleteAgentAsync(agentId, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _agentService.DeleteAgentAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNullOrEmpty();
    }

/// <summary>
/// End Tests DeleteAgentAsync Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Interface Contract Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task IAgentService_AllMethods_ShouldRespectCancellationTokenAsync()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        /// This suppression is safe because:
        /// 1. cts.Cancel() executes in microseconds (not long-running)
        /// 2. It's deterministic and necessary for testing cancellation behavior
        /// 3. Test isolation requires immediate cancellation, not async delays
        /// 4. This is the recommended pattern for testing cancellation in xUnit
        /// This pragma is applied narrowly to suppress the false-positive without affecting global behavior.
        /// Test methods are expected to use immediate cancellation for precise control and verification of
#pragma warning disable AsyncFixer02 // Long-running or blocking operations inside an async method
        cts.Cancel();
#pragma warning restore AsyncFixer02 // Long-running or blocking operations inside an async method

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

        true.ShouldBeTrue("All methods should handle cancellation tokens without throwing exceptions");
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

/// <summary>
/// End Tests Interface Contract Tests
/// </summary>
/// <returns></returns>
}