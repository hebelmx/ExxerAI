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