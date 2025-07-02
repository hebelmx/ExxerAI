using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using Microsoft.Extensions.Logging;
using Meziantou.Extensions.Logging.Xunit;
using NSubstitute;
using Shouldly;
using Xunit.Abstractions;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Real implementation tests for AgentService - testing business logic, not interface contracts
/// Following AgregationBoundedTest pattern with comprehensive mutation hunting coverage
/// </summary>
public class AgentServiceImplementationTests
{
	private readonly IAgentRepository _mockAgentRepository;
	private readonly ITaskRepository _mockTaskRepository;
	private readonly ILogger<AgentService> _logger;
	private readonly AgentService _agentService;

	public AgentServiceImplementationTests(ITestOutputHelper testOutputHelper)
	{
		_mockAgentRepository = Substitute.For<IAgentRepository>();
		_mockTaskRepository = Substitute.For<ITaskRepository>();
		_logger = XUnitLogger.CreateLogger<AgentService>(testOutputHelper);
		_agentService = new AgentService(_mockAgentRepository, _mockTaskRepository);
	}

	/// <summary>
	/// Constructor and dependency injection tests
	/// </summary>
	public class ConstructorTests : AgentServiceImplementationTests
	{
		public ConstructorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_AgentRepositoryIsNull()
		{
			// Arrange & Act & Assert
			Should.Throw<ArgumentNullException>(() => 
				new AgentService(null!, _mockTaskRepository))
				.ParamName.ShouldBe("agentRepository");
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_TaskRepositoryIsNull()
		{
			// Arrange & Act & Assert
			Should.Throw<ArgumentNullException>(() => 
				new AgentService(_mockAgentRepository, null!))
				.ParamName.ShouldBe("taskRepository");
		}

		[Fact]
		public void Should_CreateAgentService_When_ValidDependenciesProvided()
		{
			// Arrange & Act
			var service = new AgentService(_mockAgentRepository, _mockTaskRepository);

			// Assert
			service.ShouldNotBeNull();
			service.ShouldBeAssignableTo<IAgentService>();
		}
	}

	/// <summary>
	/// CreateAgentAsync implementation tests - comprehensive business logic coverage
	/// </summary>
	public class CreateAgentAsyncTests : AgentServiceImplementationTests
	{
		public CreateAgentAsyncTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{
		}

		[Fact]
		public async Task Should_CreateAgent_When_ValidInputProvided()
		{
			// Arrange
			var name = "Test Agent";
			var description = "Test Description";
			var capabilities = new AgentCapabilities();
			var expectedAgent = new Agent 
			{ 
				Id = Guid.NewGuid(), 
				Name = name, 
				Description = description,
				Capabilities = capabilities,
				Status = AgentStatus.Active
			};

			_mockAgentRepository.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(expectedAgent));

			// Act
			var result = await _agentService.CreateAgentAsync(name, description, capabilities);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeTrue();
			result.Data.ShouldNotBeNull();
			result.Data.Name.ShouldBe(name);
			result.Data.Description.ShouldBe(description);
			result.Data.Capabilities.ShouldBe(capabilities);
			result.Data.Status.ShouldBe(AgentStatus.Active);

			await _mockAgentRepository.Received(1).AddAsync(
				Arg.Is<Agent>(a => a.Name == name && a.Description == description && a.Status == AgentStatus.Active),
				Arg.Any<CancellationToken>());
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		[InlineData("\t")]
		[InlineData("\r\n")]
		public async Task Should_ReturnFailure_When_NameIsNullOrWhitespace(string invalidName)
		{
			// Arrange
			var description = "Valid Description";
			var capabilities = new AgentCapabilities();

			// Act
			var result = await _agentService.CreateAgentAsync(invalidName, description, capabilities);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Agent name cannot be empty");

			await _mockAgentRepository.DidNotReceive().AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		[InlineData("\t")]
		[InlineData("\r\n")]
		public async Task Should_ReturnFailure_When_DescriptionIsNullOrWhitespace(string invalidDescription)
		{
			// Arrange
			var name = "Valid Name";
			var capabilities = new AgentCapabilities();

			// Act
			var result = await _agentService.CreateAgentAsync(name, invalidDescription, capabilities);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Agent description cannot be empty");

			await _mockAgentRepository.DidNotReceive().AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_CapabilitiesIsNull()
		{
			// Arrange
			var name = "Valid Name";
			var description = "Valid Description";
			AgentCapabilities capabilities = null!;

			// Act
			var result = await _agentService.CreateAgentAsync(name, description, capabilities);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Agent capabilities cannot be null");

			await _mockAgentRepository.DidNotReceive().AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_RepositoryAddFails()
		{
			// Arrange
			var name = "Test Agent";
			var description = "Test Description";
			var capabilities = new AgentCapabilities();
			var repositoryError = "Database connection failed";

			_mockAgentRepository.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithFailure(repositoryError));

			// Act
			var result = await _agentService.CreateAgentAsync(name, description, capabilities);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe(repositoryError);
		}

		[Fact]
		public async Task Should_ReturnFailure_When_RepositoryThrowsException()
		{
			// Arrange
			var name = "Test Agent";
			var description = "Test Description";
			var capabilities = new AgentCapabilities();

			_mockAgentRepository.When(x => x.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>()))
				.Do(x => throw new InvalidOperationException("Database error"));

			// Act
			var result = await _agentService.CreateAgentAsync(name, description, capabilities);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldStartWith("An error occurred while creating the agent:");
			result.Error.ShouldContain("Database error");
		}

		[Fact]
		public async Task Should_HandleCancellation_When_CancellationTokenProvided()
		{
			// Arrange
			var cts = new CancellationTokenSource();
			var name = "Test Agent";
			var description = "Test Description";
			var capabilities = new AgentCapabilities();

			_mockAgentRepository.AddAsync(Arg.Any<Agent>(), cts.Token)
				.Returns(Task.FromCanceled<Result<Agent>>(cts.Token));

			cts.Cancel();

			// Act & Assert
			await Should.ThrowAsync<TaskCanceledException>(async () =>
				await _agentService.CreateAgentAsync(name, description, capabilities, cts.Token));
		}

		[Fact]
		public async Task Should_SetDefaultConfiguration_When_CreatingAgent()
		{
			// Arrange
			var name = "Test Agent";
			var description = "Test Description";
			var capabilities = new AgentCapabilities();
			Agent capturedAgent = null!;

			_mockAgentRepository.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
				.Returns(callInfo =>
				{
					capturedAgent = callInfo.Arg<Agent>();
					return Result<Agent>.WithSuccess(capturedAgent);
				});

			// Act
			await _agentService.CreateAgentAsync(name, description, capabilities);

			// Assert
			capturedAgent.ShouldNotBeNull();
			capturedAgent.Configuration.ShouldNotBeNull();
			capturedAgent.Configuration.ShouldBeOfType<AgentConfiguration>();
		}

		[Fact]
		public async Task Should_SetAgentStatusToActive_When_CreatingAgent()
		{
			// Arrange
			var name = "Test Agent";
			var description = "Test Description";
			var capabilities = new AgentCapabilities();
			Agent capturedAgent = null!;

			_mockAgentRepository.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
				.Returns(callInfo =>
				{
					capturedAgent = callInfo.Arg<Agent>();
					return Result<Agent>.WithSuccess(capturedAgent);
				});

			// Act
			await _agentService.CreateAgentAsync(name, description, capabilities);

			// Assert
			capturedAgent.ShouldNotBeNull();
			capturedAgent.Status.ShouldBe(AgentStatus.Active);
		}
	}

	/// <summary>
	/// GetAgentAsync implementation tests - comprehensive validation and error handling
	/// </summary>
	public class GetAgentAsyncTests : AgentServiceImplementationTests
	{
		public GetAgentAsyncTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{
		}

		[Fact]
		public async Task Should_ReturnAgent_When_ValidIdProvided()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var expectedAgent = new Agent { Id = agentId, Name = "Test Agent" };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(expectedAgent));

			// Act
			var result = await _agentService.GetAgentAsync(agentId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeTrue();
			result.Data.ShouldNotBeNull();
			result.Data.Id.ShouldBe(agentId);
			result.Data.Name.ShouldBe("Test Agent");
		}

		[Fact]
		public async Task Should_ReturnFailure_When_AgentIdIsEmpty()
		{
			// Arrange
			var agentId = Guid.Empty;

			// Act
			var result = await _agentService.GetAgentAsync(agentId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Agent ID cannot be empty");

			await _mockAgentRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_RepositoryFailsToFindAgent()
		{
			// Arrange
			var agentId = Guid.NewGuid();

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithFailure("Agent not found"));

			// Act
			var result = await _agentService.GetAgentAsync(agentId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe($"Agent not found with ID: {agentId}");
		}

		[Fact]
		public async Task Should_ReturnFailure_When_RepositoryThrowsException()
		{
			// Arrange
			var agentId = Guid.NewGuid();

			_mockAgentRepository.When(x => x.GetByIdAsync(agentId, Arg.Any<CancellationToken>()))
				.Do(x => throw new TimeoutException("Database timeout"));

			// Act
			var result = await _agentService.GetAgentAsync(agentId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldStartWith("An error occurred while retrieving the agent:");
			result.Error.ShouldContain("Database timeout");
		}
	}

	/// <summary>
	/// GetAllAgentsAsync implementation tests
	/// </summary>
	public class GetAllAgentsAsyncTests : AgentServiceImplementationTests
	{
		public GetAllAgentsAsyncTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{
		}

		[Fact]
		public async Task Should_ReturnAllAgents_When_AgentsExist()
		{
			// Arrange
			var agents = new List<Agent>
			{
				new() { Id = Guid.NewGuid(), Name = "Agent1" },
				new() { Id = Guid.NewGuid(), Name = "Agent2" },
				new() { Id = Guid.NewGuid(), Name = "Agent3" }
			};

			_mockAgentRepository.GetAllAsync(Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<Agent>>.WithSuccess(agents));

			// Act
			var result = await _agentService.GetAllAgentsAsync();

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeTrue();
			result.Data.ShouldNotBeNull();
			result.Data.Count().ShouldBe(3);
		}

		[Fact]
		public async Task Should_ReturnEmptyList_When_NoAgentsExist()
		{
			// Arrange
			var emptyAgents = new List<Agent>();

			_mockAgentRepository.GetAllAsync(Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<Agent>>.WithSuccess(emptyAgents));

			// Act
			var result = await _agentService.GetAllAgentsAsync();

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeTrue();
			result.Data.ShouldNotBeNull();
			result.Data.ShouldBeEmpty();
		}

		[Fact]
		public async Task Should_ReturnFailure_When_RepositoryFails()
		{
			// Arrange
			_mockAgentRepository.GetAllAsync(Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<Agent>>.WithFailure("Database error"));

			// Act
			var result = await _agentService.GetAllAgentsAsync();

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Database error");
		}

		[Fact]
		public async Task Should_ReturnFailure_When_RepositoryThrowsException()
		{
			// Arrange
			_mockAgentRepository.When(x => x.GetAllAsync(Arg.Any<CancellationToken>()))
				.Do(x => throw new OutOfMemoryException("System out of memory"));

			// Act
			var result = await _agentService.GetAllAgentsAsync();

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldStartWith("An error occurred while retrieving all agents:");
			result.Error.ShouldContain("System out of memory");
		}
	}
} 