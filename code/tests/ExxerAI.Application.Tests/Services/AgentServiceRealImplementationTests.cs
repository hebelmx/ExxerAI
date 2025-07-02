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
/// Real implementation tests for AgentService - flat structure for reliable test discovery
/// Testing actual business logic with comprehensive mutation hunting coverage
/// </summary>
public class AgentServiceRealImplementationTests
{
	private readonly IAgentRepository _mockAgentRepository;
	private readonly ITaskRepository _mockTaskRepository;
	private readonly ILogger<AgentService> _logger;
	private readonly AgentService _agentService;

	public AgentServiceRealImplementationTests(ITestOutputHelper testOutputHelper)
	{
		_mockAgentRepository = Substitute.For<IAgentRepository>();
		_mockTaskRepository = Substitute.For<ITaskRepository>();
		_logger = XUnitLogger.CreateLogger<AgentService>(testOutputHelper);
		_agentService = new AgentService(_mockAgentRepository, _mockTaskRepository);
	}

	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_AgentRepositoryIsNull()
	{
		// Arrange & Act & Assert
		Should.Throw<ArgumentNullException>(() => 
			new AgentService(null!, _mockTaskRepository))
			.ParamName.ShouldBe("agentRepository");
	}

	[Fact]
	public void Constructor_Should_ThrowArgumentNullException_When_TaskRepositoryIsNull()
	{
		// Arrange & Act & Assert
		Should.Throw<ArgumentNullException>(() => 
			new AgentService(_mockAgentRepository, null!))
			.ParamName.ShouldBe("taskRepository");
	}

	[Fact]
	public async Task CreateAgentAsync_Should_CreateAgent_When_ValidInputProvided()
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
		result.Data.Status.ShouldBe(AgentStatus.Active);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task CreateAgentAsync_Should_ReturnFailure_When_NameIsInvalid(string invalidName)
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
	}

	[Fact]
	public async Task CreateAgentAsync_Should_ReturnFailure_When_CapabilitiesIsNull()
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
	}

	[Fact]
	public async Task GetAgentAsync_Should_ReturnAgent_When_ValidIdProvided()
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
	}

	[Fact]
	public async Task GetAgentAsync_Should_ReturnFailure_When_AgentIdIsEmpty()
	{
		// Arrange
		var agentId = Guid.Empty;

		// Act
		var result = await _agentService.GetAgentAsync(agentId);

		// Assert
		result.ShouldNotBeNull();
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe("Agent ID cannot be empty");
	}

	[Fact]
	public async Task AssignTaskAsync_Should_AssignTask_When_ValidAgentAndTaskProvided()
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
	}

	[Fact]
	public async Task AssignTaskAsync_Should_ReturnFailure_When_AgentIdIsEmpty()
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
	}

	[Fact]
	public async Task AssignTaskAsync_Should_ReturnFailure_When_TaskIdIsEmpty()
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
	}

	[Fact]
	public async Task AssignTaskAsync_Should_ReturnFailure_When_AgentIsNotActive()
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
	}

	[Theory]
	[InlineData(Domain.TaskStatus.InProgress)]
	[InlineData(Domain.TaskStatus.Completed)]
	[InlineData(Domain.TaskStatus.Failed)]
	[InlineData(Domain.TaskStatus.Cancelled)]
	public async Task AssignTaskAsync_Should_ReturnFailure_When_TaskIsNotPending(Domain.TaskStatus taskStatus)
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
	}

	[Fact]
	public async Task FindBestAgentForTaskAsync_Should_FindBestAgent_When_SuitableAgentsExist()
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
	public async Task FindBestAgentForTaskAsync_Should_ReturnFailure_When_TaskTypeIsInvalid(string invalidTaskType)
	{
		// Act
		var result = await _agentService.FindBestAgentForTaskAsync(invalidTaskType);

		// Assert
		result.ShouldNotBeNull();
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe("Task type cannot be empty");
	}

	[Fact]
	public async Task DeleteAgentAsync_Should_DeleteAgent_When_ValidIdAndNoActiveTasks()
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
	}

	[Fact]
	public async Task DeleteAgentAsync_Should_ReturnFailure_When_AgentIdIsEmpty()
	{
		// Arrange
		var agentId = Guid.Empty;

		// Act
		var result = await _agentService.DeleteAgentAsync(agentId);

		// Assert
		result.ShouldNotBeNull();
		result.IsSuccess.ShouldBeFalse();
		result.Error.ShouldBe("Agent ID cannot be empty");
	}

	[Fact]
	public async Task DeleteAgentAsync_Should_ReturnFailure_When_AgentHasActiveTasksInProgress()
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
	}

	[Fact]
	public async Task UpdateAgentConfigurationAsync_Should_UpdateConfiguration_When_ValidInputProvided()
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
	}

	[Fact]
	public async Task UpdateAgentConfigurationAsync_Should_ReturnFailure_When_AgentIdIsEmpty()
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
	}

	[Fact]
	public async Task UpdateAgentConfigurationAsync_Should_ReturnFailure_When_ConfigurationIsNull()
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
	}
} 