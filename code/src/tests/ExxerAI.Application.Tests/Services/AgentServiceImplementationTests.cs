using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Implementation tests for AgentService - tests the real service implementation with mocked dependencies
/// </summary>
public class AgentServiceImplementationTests
{
	private readonly IAgentRepository _mockAgentRepository;
	private readonly ITaskRepository _mockTaskRepository;
	private readonly AgentService _agentService;

	public AgentServiceImplementationTests()
	{
		_mockAgentRepository = Substitute.For<IAgentRepository>();
		_mockTaskRepository = Substitute.For<ITaskRepository>();
		_agentService = new AgentService(_mockAgentRepository, _mockTaskRepository);
	}

	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_AgentRepositoryIsNull()
	{
		// Act & Assert
		Should.Throw<ArgumentNullException>(() => new AgentService(null!, _mockTaskRepository))
			.ParamName.ShouldBe("agentRepository");
	}

	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_TaskRepositoryIsNull()
	{
		// Act & Assert
		Should.Throw<ArgumentNullException>(() => new AgentService(_mockAgentRepository, null!))
			.ParamName.ShouldBe("taskRepository");
	}

	[Fact]
	public void Constructor_Should_InitializeCorrectly_When_ValidParametersProvided()
	{
		// Act
		var service = new AgentService(_mockAgentRepository, _mockTaskRepository);

		// Assert
		service.ShouldNotBeNull();
	}

	[Fact]
	public async Task CreateAgentAsync_Should_ReturnFailure_When_NameIsEmpty()
	{
		// Arrange
		var capabilities = new AgentCapabilities();

		// Act
		var result = await _agentService.CreateAgentAsync("", "description", capabilities);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Agent name cannot be empty");
	}

	[Fact]
	public async Task CreateAgentAsync_Should_ReturnFailure_When_NameIsWhitespace()
	{
		// Arrange
		var capabilities = new AgentCapabilities();

		// Act
		var result = await _agentService.CreateAgentAsync("   ", "description", capabilities);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Agent name cannot be empty");
	}

	[Fact]
	public async Task CreateAgentAsync_Should_ReturnFailure_When_DescriptionIsEmpty()
	{
		// Arrange
		var capabilities = new AgentCapabilities();

		// Act
		var result = await _agentService.CreateAgentAsync("TestAgent", "", capabilities);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Agent description cannot be empty");
	}

	[Fact]
	public async Task CreateAgentAsync_Should_ReturnFailure_When_CapabilitiesIsNull()
	{
		// Act
		var result = await _agentService.CreateAgentAsync("TestAgent", "description", null!);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Agent capabilities cannot be null");
	}

	[Fact]
	public async Task CreateAgentAsync_Should_ReturnSuccess_When_ValidParametersProvided()
	{
		// Arrange
		var capabilities = new AgentCapabilities();
		var expectedAgent = new Agent 
		{ 
			Id = Guid.NewGuid(), 
			Name = "TestAgent",
			Description = "Test Description",
			Capabilities = capabilities,
			Status = AgentStatus.Active
		};
		
		_mockAgentRepository.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithSuccess(expectedAgent));

		// Act
		var result = await _agentService.CreateAgentAsync("TestAgent", "Test Description", capabilities);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldNotBeNull();
		
		await _mockAgentRepository.Received(1).AddAsync(
			Arg.Is<Agent>(a => 
				a.Name == "TestAgent" && 
				a.Description == "Test Description" && 
				a.Capabilities == capabilities &&
				a.Status == AgentStatus.Active),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateAgentAsync_Should_ReturnFailure_When_RepositoryFails()
	{
		// Arrange
		var capabilities = new AgentCapabilities();
		_mockAgentRepository.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithFailure("Repository error"));

		// Act
		var result = await _agentService.CreateAgentAsync("TestAgent", "description", capabilities);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Repository error");
	}

	[Fact]
	public async Task CreateAgentAsync_Should_ReturnFailure_When_ExceptionThrown()
	{
		// Arrange
		var capabilities = new AgentCapabilities();
		_mockAgentRepository.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
			.Throws(new InvalidOperationException("Test exception"));

		// Act
		var result = await _agentService.CreateAgentAsync("TestAgent", "description", capabilities);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldStartWith("An error occurred while creating the agent:");
	}

	[Fact]
	public async Task GetAgentAsync_Should_ReturnFailure_When_AgentIdIsEmpty()
	{
		// Act
		var result = await _agentService.GetAgentAsync(Guid.Empty);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Agent ID cannot be empty");
	}

	[Fact]
	public async Task GetAgentAsync_Should_ReturnSuccess_When_AgentExists()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var expectedAgent = new Agent { Id = agentId, Name = "TestAgent" };
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithSuccess(expectedAgent));

		// Act
		var result = await _agentService.GetAgentAsync(agentId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBe(expectedAgent);
	}

	[Fact]
	public async Task GetAgentAsync_Should_ReturnFailure_When_RepositoryFails()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithFailure("Not found"));

		// Act
		var result = await _agentService.GetAgentAsync(agentId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe($"Agent not found with ID: {agentId}");
	}

	[Fact]
	public async Task GetAgentAsync_Should_ReturnFailure_When_ExceptionThrown()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Throws(new InvalidOperationException("Test exception"));

		// Act
		var result = await _agentService.GetAgentAsync(agentId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldStartWith("An error occurred while retrieving the agent:");
	}

	[Fact]
	public async Task GetAllAgentsAsync_Should_ReturnSuccess_When_AgentsExist()
	{
		// Arrange
		var agents = new List<Agent>
		{
			new() { Id = Guid.NewGuid(), Name = "Agent1" },
			new() { Id = Guid.NewGuid(), Name = "Agent2" }
		};
		_mockAgentRepository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.WithSuccess(agents));

		// Act
		var result = await _agentService.GetAllAgentsAsync();

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBe(agents);
	}

	[Fact]
	public async Task GetAllAgentsAsync_Should_ReturnFailure_When_RepositoryFails()
	{
		// Arrange
		_mockAgentRepository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.WithFailure("Database error"));

		// Act
		var result = await _agentService.GetAllAgentsAsync();

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Database error");
	}

	[Fact]
	public async Task GetAllAgentsAsync_Should_ReturnFailure_When_ExceptionThrown()
	{
		// Arrange
		_mockAgentRepository.GetAllAsync(Arg.Any<CancellationToken>())
			.Throws(new InvalidOperationException("Test exception"));

		// Act
		var result = await _agentService.GetAllAgentsAsync();

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldStartWith("An error occurred while retrieving all agents:");
	}

	[Fact]
	public async Task GetActiveAgentsAsync_Should_ReturnSuccess_When_ActiveAgentsExist()
	{
		// Arrange
		var activeAgents = new List<Agent>
		{
			new() { Id = Guid.NewGuid(), Name = "ActiveAgent1", Status = AgentStatus.Active },
			new() { Id = Guid.NewGuid(), Name = "ActiveAgent2", Status = AgentStatus.Active }
		};
		_mockAgentRepository.GetByStatusAsync(AgentStatus.Active, Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.WithSuccess(activeAgents));

		// Act
		var result = await _agentService.GetActiveAgentsAsync();

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBe(activeAgents);
	}

	[Fact]
	public async Task GetActiveAgentsAsync_Should_ReturnFailure_When_RepositoryFails()
	{
		// Arrange
		_mockAgentRepository.GetByStatusAsync(AgentStatus.Active, Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.WithFailure("Database error"));

		// Act
		var result = await _agentService.GetActiveAgentsAsync();

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Database error");
	}

	[Fact]
	public async Task UpdateAgentConfigurationAsync_Should_ReturnFailure_When_AgentIdIsEmpty()
	{
		// Arrange
		var config = new AgentConfiguration();

		// Act
		var result = await _agentService.UpdateAgentConfigurationAsync(Guid.Empty, config);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Agent ID cannot be empty");
	}

	[Fact]
	public async Task UpdateAgentConfigurationAsync_Should_ReturnFailure_When_ConfigurationIsNull()
	{
		// Arrange
		var agentId = Guid.NewGuid();

		// Act
		var result = await _agentService.UpdateAgentConfigurationAsync(agentId, null!);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Configuration cannot be null");
	}

	[Fact]
	public async Task UpdateAgentConfigurationAsync_Should_ReturnFailure_When_AgentNotFound()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var config = new AgentConfiguration();
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithFailure("Not found"));

		// Act
		var result = await _agentService.UpdateAgentConfigurationAsync(agentId, config);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe($"Agent not found with ID: {agentId}");
	}

	[Fact]
	public async Task UpdateAgentConfigurationAsync_Should_ReturnSuccess_When_ValidParameters()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var config = new AgentConfiguration();
		var agent = new Agent { Id = agentId, Name = "TestAgent" };
		
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithSuccess(agent));
		_mockAgentRepository.UpdateAsync(agent, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithSuccess(agent));

		// Act
		var result = await _agentService.UpdateAgentConfigurationAsync(agentId, config);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBeTrue();
		agent.Configuration.ShouldBe(config);
		
		await _mockAgentRepository.Received(1).UpdateAsync(agent, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task UpdateAgentStatusAsync_Should_ReturnFailure_When_AgentIdIsEmpty()
	{
		// Act
		var result = await _agentService.UpdateAgentStatusAsync(Guid.Empty, AgentStatus.Active);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Agent ID cannot be empty");
	}

	[Fact]
	public async Task UpdateAgentStatusAsync_Should_ReturnSuccess_When_ValidParameters()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var agent = new Agent { Id = agentId, Name = "TestAgent", Status = AgentStatus.Inactive };
		
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithSuccess(agent));
		_mockAgentRepository.UpdateAsync(agent, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithSuccess(agent));

		// Act
		var result = await _agentService.UpdateAgentStatusAsync(agentId, AgentStatus.Active);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBeTrue();
		agent.Status.ShouldBe(AgentStatus.Active);
	}

	[Fact]
	public async Task AssignTaskAsync_Should_ReturnFailure_When_AgentIdIsEmpty()
	{
		// Act
		var result = await _agentService.AssignTaskAsync(Guid.Empty, Guid.NewGuid());

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Agent ID cannot be empty");
	}

	[Fact]
	public async Task AssignTaskAsync_Should_ReturnFailure_When_TaskIdIsEmpty()
	{
		// Act
		var result = await _agentService.AssignTaskAsync(Guid.NewGuid(), Guid.Empty);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Task ID cannot be empty");
	}

	[Fact]
	public async Task AssignTaskAsync_Should_ReturnFailure_When_AgentNotFound()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var taskId = Guid.NewGuid();
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithFailure("Not found"));

		// Act
		var result = await _agentService.AssignTaskAsync(agentId, taskId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe($"Agent not found with ID: {agentId}");
	}

	[Fact]
	public async Task AssignTaskAsync_Should_ReturnFailure_When_AgentNotActive()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var taskId = Guid.NewGuid();
		var agent = new Agent { Id = agentId, Status = AgentStatus.Inactive };
		
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithSuccess(agent));

		// Act
		var result = await _agentService.AssignTaskAsync(agentId, taskId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe($"Agent {agentId} is not active and cannot be assigned tasks");
	}

	[Fact]
	public async Task AssignTaskAsync_Should_ReturnFailure_When_TaskNotFound()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var taskId = Guid.NewGuid();
		var agent = new Agent { Id = agentId, Status = AgentStatus.Active };
		
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithSuccess(agent));
		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Result<AgentTask>.WithFailure("Not found"));

		// Act
		var result = await _agentService.AssignTaskAsync(agentId, taskId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe($"Task not found with ID: {taskId}");
	}

	[Fact]
	public async Task AssignTaskAsync_Should_ReturnFailure_When_TaskNotPending()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var taskId = Guid.NewGuid();
		var agent = new Agent { Id = agentId, Status = AgentStatus.Active };
		var task = new AgentTask { Id = taskId, Status = Domain.TaskStatus.InProgress };
		
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithSuccess(agent));
		_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Result<AgentTask>.WithSuccess(task));

		// Act
		var result = await _agentService.AssignTaskAsync(agentId, taskId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe($"Task {taskId} is not available for assignment (Status: {task.Status})");
	}

	[Fact]
	public async Task AssignTaskAsync_Should_ReturnSuccess_When_ValidParameters()
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
		_mockTaskRepository.UpdateAsync(task, Arg.Any<CancellationToken>())
			.Returns(Result<AgentTask>.WithSuccess(task));

		// Act
		var result = await _agentService.AssignTaskAsync(agentId, taskId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBeTrue();
		task.AssignedAgentId.ShouldBe(agentId);
	}

	[Fact]
	public async Task AssignTaskToAgentAsync_Should_CallAssignTaskAsync()
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
		_mockTaskRepository.UpdateAsync(task, Arg.Any<CancellationToken>())
			.Returns(Result<AgentTask>.WithSuccess(task));

		// Act
		var result = await _agentService.AssignTaskToAgentAsync(agentId, taskId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBeTrue();
		task.AssignedAgentId.ShouldBe(agentId);
	}

	[Fact]
	public async Task FindBestAgentForTaskAsync_Should_ReturnFailure_When_TaskTypeIsEmpty()
	{
		// Act
		var result = await _agentService.FindBestAgentForTaskAsync("");

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Task type cannot be empty");
	}

	[Fact]
	public async Task FindBestAgentForTaskAsync_Should_ReturnFailure_When_NoSupportingAgents()
	{
		// Arrange
		_mockAgentRepository.FindByTaskTypeAsync("DataProcessing", Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.WithSuccess(new List<Agent>()));

		// Act
		var result = await _agentService.FindBestAgentForTaskAsync("DataProcessing");

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("No active agents found that support task type: DataProcessing");
	}

	[Fact]
	public async Task FindBestAgentForTaskAsync_Should_ReturnFailure_When_NoActiveAgents()
	{
		// Arrange
		var agents = new List<Agent>
		{
			new() { Id = Guid.NewGuid(), Status = AgentStatus.Inactive }
		};
		_mockAgentRepository.FindByTaskTypeAsync("DataProcessing", Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.WithSuccess(agents));

		// Act
		var result = await _agentService.FindBestAgentForTaskAsync("DataProcessing");

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("No active agents found that support task type: DataProcessing");
	}

	[Fact]
	public async Task FindBestAgentForTaskAsync_Should_ReturnSuccess_When_ValidAgentFound()
	{
		// Arrange
		var agent = new Agent { Id = Guid.NewGuid(), Status = AgentStatus.Active };
		var agents = new List<Agent> { agent };
		
		_mockAgentRepository.FindByTaskTypeAsync("DataProcessing", Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.WithSuccess(agents));
		_mockAgentRepository.GetAgentsWithTaskCountAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<AgentWithTaskCount>>.WithFailure("No task count available"));

		// Act
		var result = await _agentService.FindBestAgentForTaskAsync("DataProcessing");

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBe(agent);
	}

	[Fact]
	public async Task DeleteAgentAsync_Should_ReturnFailure_When_AgentIdIsEmpty()
	{
		// Act
		var result = await _agentService.DeleteAgentAsync(Guid.Empty);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe("Agent ID cannot be empty");
	}

	[Fact]
	public async Task DeleteAgentAsync_Should_ReturnFailure_When_AgentNotFound()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithFailure("Not found"));

		// Act
		var result = await _agentService.DeleteAgentAsync(agentId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe($"Agent not found with ID: {agentId}");
	}

	[Fact]
	public async Task DeleteAgentAsync_Should_ReturnFailure_When_AgentHasActiveTasks()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var agent = new Agent { Id = agentId };
		var activeTasks = new List<AgentTask> { new() { Id = Guid.NewGuid() } };
		
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithSuccess(agent));
		_mockTaskRepository.GetByAgentAsync(agentId, Domain.TaskStatus.InProgress, Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<AgentTask>>.WithSuccess(activeTasks));

		// Act
		var result = await _agentService.DeleteAgentAsync(agentId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Error.ShouldBe($"Cannot delete agent {agentId} as it has active tasks in progress");
	}

	[Fact]
	public async Task DeleteAgentAsync_Should_ReturnSuccess_When_ValidParameters()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var agent = new Agent { Id = agentId };
		
		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithSuccess(agent));
		_mockTaskRepository.GetByAgentAsync(agentId, Domain.TaskStatus.InProgress, Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<AgentTask>>.WithSuccess(new List<AgentTask>()));
		_mockAgentRepository.DeleteAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<bool>.WithSuccess(true));

		// Act
		var result = await _agentService.DeleteAgentAsync(agentId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBeTrue();
	}
} 