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

	/// <summary>
	/// GetActiveAgentsAsync implementation tests
	/// </summary>
	public class GetActiveAgentsAsyncTests : AgentServiceImplementationTests
	{
		public GetActiveAgentsAsyncTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{
		}

		[Fact]
		public async Task Should_ReturnActiveAgents_When_ActiveAgentsExist()
		{
			// Arrange
			var activeAgents = new List<Agent>
			{
				new() { Id = Guid.NewGuid(), Name = "Agent1", Status = AgentStatus.Active },
				new() { Id = Guid.NewGuid(), Name = "Agent2", Status = AgentStatus.Active }
			};

			_mockAgentRepository.GetByStatusAsync(AgentStatus.Active, Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<Agent>>.WithSuccess(activeAgents));

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
		public async Task Should_ReturnEmptyList_When_NoActiveAgentsExist()
		{
			// Arrange
			var emptyAgents = new List<Agent>();

			_mockAgentRepository.GetByStatusAsync(AgentStatus.Active, Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<Agent>>.WithSuccess(emptyAgents));

			// Act
			var result = await _agentService.GetActiveAgentsAsync();

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
			_mockAgentRepository.GetByStatusAsync(AgentStatus.Active, Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<Agent>>.WithFailure("Database error"));

			// Act
			var result = await _agentService.GetActiveAgentsAsync();

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Database error");
		}

		[Fact]
		public async Task Should_ReturnFailure_When_RepositoryThrowsException()
		{
			// Arrange
			_mockAgentRepository.When(x => x.GetByStatusAsync(AgentStatus.Active, Arg.Any<CancellationToken>()))
				.Do(x => throw new InvalidOperationException("Connection failed"));

			// Act
			var result = await _agentService.GetActiveAgentsAsync();

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldStartWith("An error occurred while retrieving active agents:");
			result.Error.ShouldContain("Connection failed");
		}
	}

	/// <summary>
	/// UpdateAgentConfigurationAsync implementation tests - mutation hunting business logic
	/// </summary>
	public class UpdateAgentConfigurationAsyncTests : AgentServiceImplementationTests
	{
		public UpdateAgentConfigurationAsyncTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{
		}

		[Fact]
		public async Task Should_UpdateConfiguration_When_ValidInputProvided()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var configuration = new AgentConfiguration();
			var existingAgent = new Agent { Id = agentId, Name = "Test Agent", Configuration = new AgentConfiguration() };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(existingAgent));

			_mockAgentRepository.UpdateAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(existingAgent));

			// Act
			var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeTrue();
			result.Data.ShouldBeTrue();

			existingAgent.Configuration.ShouldBe(configuration);
			existingAgent.UpdatedAt.ShouldBeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

			await _mockAgentRepository.Received(1).UpdateAsync(
				Arg.Is<Agent>(a => a.Id == agentId && a.Configuration == configuration),
				Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_AgentIdIsEmpty()
		{
			// Arrange
			var agentId = Guid.Empty;
			var configuration = new AgentConfiguration();

			// Act
			var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Agent ID cannot be empty");

			await _mockAgentRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
			await _mockAgentRepository.DidNotReceive().UpdateAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_ConfigurationIsNull()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			AgentConfiguration configuration = null!;

			// Act
			var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Configuration cannot be null");

			await _mockAgentRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
			await _mockAgentRepository.DidNotReceive().UpdateAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_AgentNotFound()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var configuration = new AgentConfiguration();

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithFailure("Agent not found"));

			// Act
			var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe($"Agent not found with ID: {agentId}");

			await _mockAgentRepository.DidNotReceive().UpdateAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_UpdateFails()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var configuration = new AgentConfiguration();
			var existingAgent = new Agent { Id = agentId, Name = "Test Agent" };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(existingAgent));

			_mockAgentRepository.UpdateAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithFailure("Update failed"));

			// Act
			var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Update failed");
		}

		[Fact]
		public async Task Should_ReturnFailure_When_ExceptionThrown()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var configuration = new AgentConfiguration();

			_mockAgentRepository.When(x => x.GetByIdAsync(agentId, Arg.Any<CancellationToken>()))
				.Do(x => throw new TimeoutException("Database timeout"));

			// Act
			var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldStartWith("An error occurred while updating agent configuration:");
			result.Error.ShouldContain("Database timeout");
		}
	}

	/// <summary>
	/// AssignTaskAsync implementation tests - complex business logic with validation rules
	/// </summary>
	public class AssignTaskAsyncTests : AgentServiceImplementationTests
	{
		public AssignTaskAsyncTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{
		}

		[Fact]
		public async Task Should_AssignTask_When_ValidAgentAndTaskProvided()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var taskId = Guid.NewGuid();
			var agent = new Agent { Id = agentId, Status = AgentStatus.Active, Name = "Test Agent" };
			var task = new AgentTask { Id = taskId, Status = Domain.TaskStatus.Pending, Title = "Test Task" };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(agent));

			_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
				.Returns(Result<AgentTask>.WithSuccess(task));

			_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
				.Returns(Result<AgentTask>.WithSuccess(task));

			// Act
			var result = await _agentService.AssignTaskAsync(agentId, taskId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeTrue();
			result.Data.ShouldBeTrue();

			task.AssignedAgentId.ShouldBe(agentId);

			await _mockTaskRepository.Received(1).UpdateAsync(
				Arg.Is<AgentTask>(t => t.Id == taskId && t.AssignedAgentId == agentId),
				Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_AgentIdIsEmpty()
		{
			// Arrange
			var agentId = Guid.Empty;
			var taskId = Guid.NewGuid();

			// Act
			var result = await _agentService.AssignTaskAsync(agentId, taskId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Agent ID cannot be empty");

			await _mockAgentRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
			await _mockTaskRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_TaskIdIsEmpty()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var taskId = Guid.Empty;

			// Act
			var result = await _agentService.AssignTaskAsync(agentId, taskId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Task ID cannot be empty");

			await _mockAgentRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
			await _mockTaskRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_AgentNotFound()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var taskId = Guid.NewGuid();

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithFailure("Agent not found"));

			// Act
			var result = await _agentService.AssignTaskAsync(agentId, taskId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe($"Agent not found with ID: {agentId}");

			await _mockTaskRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_AgentIsNotActive()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var taskId = Guid.NewGuid();
			var agent = new Agent { Id = agentId, Status = AgentStatus.Inactive, Name = "Inactive Agent" };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(agent));

			// Act
			var result = await _agentService.AssignTaskAsync(agentId, taskId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe($"Agent {agentId} is not active and cannot be assigned tasks");

			await _mockTaskRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_TaskNotFound()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var taskId = Guid.NewGuid();
			var agent = new Agent { Id = agentId, Status = AgentStatus.Active };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(agent));

			_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
				.Returns(Result<AgentTask>.WithFailure("Task not found"));

			// Act
			var result = await _agentService.AssignTaskAsync(agentId, taskId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe($"Task not found with ID: {taskId}");
		}

		[Theory]
		[InlineData(Domain.TaskStatus.InProgress)]
		[InlineData(Domain.TaskStatus.Completed)]
		[InlineData(Domain.TaskStatus.Failed)]
		[InlineData(Domain.TaskStatus.Cancelled)]
		public async Task Should_ReturnFailure_When_TaskIsNotPending(Domain.TaskStatus taskStatus)
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var taskId = Guid.NewGuid();
			var agent = new Agent { Id = agentId, Status = AgentStatus.Active };
			var task = new AgentTask { Id = taskId, Status = taskStatus };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(agent));

			_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
				.Returns(Result<AgentTask>.WithSuccess(task));

			// Act
			var result = await _agentService.AssignTaskAsync(agentId, taskId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe($"Task {taskId} is not available for assignment (Status: {taskStatus})");

			await _mockTaskRepository.DidNotReceive().UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_TaskUpdateFails()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var taskId = Guid.NewGuid();
			var agent = new Agent { Id = agentId, Status = AgentStatus.Active };
			var task = new AgentTask { Id = taskId, Status = Domain.TaskStatus.Pending };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(agent));

			_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
				.Returns(Result<AgentTask>.WithSuccess(task));

			_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
				.Returns(Result<AgentTask>.WithFailure("Update failed"));

			// Act
			var result = await _agentService.AssignTaskAsync(agentId, taskId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Update failed");
		}
	}

	/// <summary>
	/// FindBestAgentForTaskAsync implementation tests - complex algorithm testing
	/// </summary>
	public class FindBestAgentForTaskAsyncTests : AgentServiceImplementationTests
	{
		public FindBestAgentForTaskAsyncTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{
		}

		[Fact]
		public async Task Should_FindBestAgent_When_SuitableAgentsExist()
		{
			// Arrange
			var taskType = "document-processing";
			var supportingAgents = new List<Agent>
			{
				new() { Id = Guid.NewGuid(), Name = "Agent1", Status = AgentStatus.Active },
				new() { Id = Guid.NewGuid(), Name = "Agent2", Status = AgentStatus.Active }
			};

			var agentsWithTaskCount = new List<AgentTaskCount>
			{
				new() { Agent = supportingAgents[0], TaskCount = 2 },
				new() { Agent = supportingAgents[1], TaskCount = 1 }
			};

			_mockAgentRepository.FindByTaskTypeAsync(taskType, Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<Agent>>.WithSuccess(supportingAgents));

			_mockAgentRepository.GetAgentsWithTaskCountAsync(Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<AgentTaskCount>>.WithSuccess(agentsWithTaskCount));

			// Act
			var result = await _agentService.FindBestAgentForTaskAsync(taskType);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeTrue();
			result.Data.ShouldNotBeNull();
			result.Data.Id.ShouldBe(supportingAgents[1].Id); // Agent with fewer tasks (1 vs 2)
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		[InlineData("\t")]
		public async Task Should_ReturnFailure_When_TaskTypeIsNullOrWhitespace(string invalidTaskType)
		{
			// Act
			var result = await _agentService.FindBestAgentForTaskAsync(invalidTaskType);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Task type cannot be empty");

			await _mockAgentRepository.DidNotReceive().FindByTaskTypeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_NoAgentsFound()
		{
			// Arrange
			var taskType = "non-existent-task";
			var emptyAgents = new List<Agent>();

			_mockAgentRepository.FindByTaskTypeAsync(taskType, Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<Agent>>.WithSuccess(emptyAgents));

			// Act
			var result = await _agentService.FindBestAgentForTaskAsync(taskType);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe($"No active agents found that support task type: {taskType}");
		}

		[Fact]
		public async Task Should_ReturnFailure_When_NoActiveAgentsFound()
		{
			// Arrange
			var taskType = "document-processing";
			var inactiveAgents = new List<Agent>
			{
				new() { Id = Guid.NewGuid(), Status = AgentStatus.Inactive },
				new() { Id = Guid.NewGuid(), Status = AgentStatus.Paused }
			};

			_mockAgentRepository.FindByTaskTypeAsync(taskType, Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<Agent>>.WithSuccess(inactiveAgents));

			// Act
			var result = await _agentService.FindBestAgentForTaskAsync(taskType);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe($"No active agents found that support task type: {taskType}");
		}

		[Fact]
		public async Task Should_FallbackToFirstAgent_When_TaskCountQueryFails()
		{
			// Arrange
			var taskType = "document-processing";
			var supportingAgents = new List<Agent>
			{
				new() { Id = Guid.NewGuid(), Name = "Agent1", Status = AgentStatus.Active },
				new() { Id = Guid.NewGuid(), Name = "Agent2", Status = AgentStatus.Active }
			};

			_mockAgentRepository.FindByTaskTypeAsync(taskType, Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<Agent>>.WithSuccess(supportingAgents));

			_mockAgentRepository.GetAgentsWithTaskCountAsync(Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<AgentTaskCount>>.WithFailure("Task count query failed"));

			// Act
			var result = await _agentService.FindBestAgentForTaskAsync(taskType);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeTrue();
			result.Data.ShouldNotBeNull();
			result.Data.Id.ShouldBe(supportingAgents[0].Id); // First agent as fallback
		}
	}

	/// <summary>
	/// DeleteAgentAsync implementation tests - cascade validation logic
	/// </summary>
	public class DeleteAgentAsyncTests : AgentServiceImplementationTests
	{
		public DeleteAgentAsyncTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
		{
		}

		[Fact]
		public async Task Should_DeleteAgent_When_ValidIdAndNoActiveTasks()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var agent = new Agent { Id = agentId, Name = "Test Agent" };
			var emptyTasks = new List<AgentTask>();

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(agent));

			_mockTaskRepository.GetByAgentAsync(agentId, Domain.TaskStatus.InProgress, Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<AgentTask>>.WithSuccess(emptyTasks));

			_mockAgentRepository.DeleteAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<bool>.WithSuccess(true));

			// Act
			var result = await _agentService.DeleteAgentAsync(agentId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeTrue();
			result.Data.ShouldBeTrue();

			await _mockAgentRepository.Received(1).DeleteAsync(agentId, Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_AgentIdIsEmpty()
		{
			// Arrange
			var agentId = Guid.Empty;

			// Act
			var result = await _agentService.DeleteAgentAsync(agentId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Agent ID cannot be empty");

			await _mockAgentRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
			await _mockAgentRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_AgentNotFound()
		{
			// Arrange
			var agentId = Guid.NewGuid();

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithFailure("Agent not found"));

			// Act
			var result = await _agentService.DeleteAgentAsync(agentId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe($"Agent not found with ID: {agentId}");

			await _mockAgentRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_AgentHasActiveTasksInProgress()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var agent = new Agent { Id = agentId, Name = "Test Agent" };
			var activeTasks = new List<AgentTask>
			{
				new() { Id = Guid.NewGuid(), Status = Domain.TaskStatus.InProgress, AssignedAgentId = agentId }
			};

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(agent));

			_mockTaskRepository.GetByAgentAsync(agentId, Domain.TaskStatus.InProgress, Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<AgentTask>>.WithSuccess(activeTasks));

			// Act
			var result = await _agentService.DeleteAgentAsync(agentId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe($"Cannot delete agent {agentId} as it has active tasks in progress");

			await _mockAgentRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_DeleteOperationFails()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var agent = new Agent { Id = agentId, Name = "Test Agent" };
			var emptyTasks = new List<AgentTask>();

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithSuccess(agent));

			_mockTaskRepository.GetByAgentAsync(agentId, Domain.TaskStatus.InProgress, Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<AgentTask>>.WithSuccess(emptyTasks));

			_mockAgentRepository.DeleteAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<bool>.WithFailure("Delete operation failed"));

			// Act
			var result = await _agentService.DeleteAgentAsync(agentId);

			// Assert
			result.ShouldNotBeNull();
			result.IsSuccess.ShouldBeFalse();
			result.Error.ShouldBe("Delete operation failed");
		}
	}
} 