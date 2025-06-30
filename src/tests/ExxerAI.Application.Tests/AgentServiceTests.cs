using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;
using TaskStatus = ExxerAI.Domain.TaskStatus;

namespace ExxerAI.Application.Tests;

/// <summary>
/// Represents an agent with its current task count for load balancing
/// </summary>
public class AgentTaskCount
{
	public Agent Agent { get; set; } = new();
	public int TaskCount { get; set; }
}

/// <summary>
/// Comprehensive unit tests for AgentService
/// </summary>
public class AgentServiceTests
{
	private readonly IAgentRepository _agentRepository;
	private readonly ITaskRepository _taskRepository;
	private readonly AgentService _agentService;

	public AgentServiceTests()
	{
		_agentRepository = Substitute.For<IAgentRepository>();
		_taskRepository = Substitute.For<ITaskRepository>();
		_agentService = new AgentService(_agentRepository, _taskRepository);
	}

	#region Constructor Tests

	[Fact]
	public void Should_ThrowArgumentNullException_When_AgentRepositoryIsNull()
	{
		// Arrange, Act & Assert
		Should.Throw<ArgumentNullException>(() => new AgentService(null!, _taskRepository))
			.ParamName.ShouldBe("agentRepository");
	}

	[Fact]
	public void Should_ThrowArgumentNullException_When_TaskRepositoryIsNull()
	{
		// Arrange, Act & Assert
		Should.Throw<ArgumentNullException>(() => new AgentService(_agentRepository, null!))
			.ParamName.ShouldBe("taskRepository");
	}

	[Fact]
	public void Should_CreateInstance_When_ValidDependenciesProvided()
	{
		// Arrange, Act & Assert
		_agentService.ShouldNotBeNull();
	}

	#endregion

	#region CreateAgentAsync Tests

	[Fact]
	public async Task Should_CreateAgent_When_ValidDataProvided()
	{
		// Arrange
		var name = "TestAgent";
		var description = "Test agent description";
		var capabilities = new AgentCapabilities();
		var expectedAgent = new Agent
		{
			Name = name,
			Description = description,
			Capabilities = capabilities,
			Status = AgentStatus.Active
		};

		_agentRepository.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.Success(expectedAgent));

		// Act
		var result = await _agentService.CreateAgentAsync(name, description, capabilities);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldNotBeNull();
		result.Value.Name.ShouldBe(name);
		result.Value.Description.ShouldBe(description);
		result.Value.Capabilities.ShouldBe(capabilities);
		result.Value.Status.ShouldBe(AgentStatus.Active);

		await _agentRepository.Received(1).AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public async Task Should_ReturnFailure_When_NameIsInvalid(string? invalidName)
	{
		// Arrange
		var description = "Valid description";
		var capabilities = new AgentCapabilities();

		// Act
		var result = await _agentService.CreateAgentAsync(invalidName!, description, capabilities);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Agent name cannot be empty");

		await _agentRepository.DidNotReceive().AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public async Task Should_ReturnFailure_When_DescriptionIsInvalid(string? invalidDescription)
	{
		// Arrange
		var name = "ValidName";
		var capabilities = new AgentCapabilities();

		// Act
		var result = await _agentService.CreateAgentAsync(name, invalidDescription!, capabilities);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Agent description cannot be empty");

		await _agentRepository.DidNotReceive().AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Should_ReturnFailure_When_CapabilitiesIsNull()
	{
		// Arrange
		var name = "ValidName";
		var description = "Valid description";

		// Act
		var result = await _agentService.CreateAgentAsync(name, description, null!);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Agent capabilities cannot be null");

		await _agentRepository.DidNotReceive().AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Should_ReturnFailure_When_RepositoryAddFails()
	{
		// Arrange
		var name = "TestAgent";
		var description = "Test description";
		var capabilities = new AgentCapabilities();
		var repositoryError = "Repository error";

		_agentRepository.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithFailure(repositoryError));

		// Act
		var result = await _agentService.CreateAgentAsync(name, description, capabilities);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain(repositoryError);
	}

	[Fact]
	public async Task Should_HandleException_When_RepositoryThrows()
	{
		// Arrange
		var name = "TestAgent";
		var description = "Test description";
		var capabilities = new AgentCapabilities();

		_agentRepository
			.When(x => x.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>()))
			.Do(x => throw new InvalidOperationException("Database error"));

		// Act
		var result = await _agentService.CreateAgentAsync(name, description, capabilities);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain(e => e.Contains("An error occurred while creating the agent"));
	}

	#endregion

	#region GetAgentAsync Tests

	[Fact]
	public async Task Should_GetAgent_When_ValidIdProvided()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var expectedAgent = new Agent { Id = agentId, Name = "TestAgent" };

		_agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.Success(expectedAgent));

		// Act
		var result = await _agentService.GetAgentAsync(agentId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBe(expectedAgent);

		await _agentRepository.Received(1).GetByIdAsync(agentId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Should_ReturnFailure_When_AgentIdIsEmpty()
	{
		// Arrange
		var agentId = Guid.Empty;

		// Act
		var result = await _agentService.GetAgentAsync(agentId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Agent ID cannot be empty");

		await _agentRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Should_ReturnFailure_When_AgentNotFound()
	{
		// Arrange
		var agentId = Guid.NewGuid();

		_agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.WithFailure("Not found"));

		// Act
		var result = await _agentService.GetAgentAsync(agentId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain($"Agent not found with ID: {agentId}");
	}

	#endregion

	#region GetActiveAgentsAsync Tests

	[Fact]
	public async Task Should_GetActiveAgents_When_Called()
	{
		// Arrange
		var activeAgents = new List<Agent>
		{
			new() { Id = Guid.NewGuid(), Name = "Agent1", Status = AgentStatus.Active },
			new() { Id = Guid.NewGuid(), Name = "Agent2", Status = AgentStatus.Active }
		};

		_agentRepository.GetByStatusAsync(AgentStatus.Active, Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.Success(activeAgents));

		// Act
		var result = await _agentService.GetActiveAgentsAsync();

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBe(activeAgents);

		await _agentRepository.Received(1).GetByStatusAsync(AgentStatus.Active, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Should_ReturnFailure_When_RepositoryGetActiveAgentsFails()
	{
		// Arrange
		var repositoryError = "Repository error";

		_agentRepository.GetByStatusAsync(AgentStatus.Active, Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.WithFailure(repositoryError));

		// Act
		var result = await _agentService.GetActiveAgentsAsync();

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain(repositoryError);
	}

	#endregion

	#region UpdateAgentConfigurationAsync Tests

	[Fact]
	public async Task Should_UpdateConfiguration_When_ValidDataProvided()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var configuration = new AgentConfiguration();
		var existingAgent = new Agent { Id = agentId, Name = "TestAgent" };

		_agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.Success(existingAgent));
		
		_agentRepository.UpdateAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		// Act
		var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		existingAgent.Configuration.ShouldBe(configuration);

		await _agentRepository.Received(1).GetByIdAsync(agentId, Arg.Any<CancellationToken>());
		await _agentRepository.Received(1).UpdateAsync(existingAgent, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Should_ReturnFailure_When_ConfigurationAgentIdIsEmpty()
	{
		// Arrange
		var agentId = Guid.Empty;
		var configuration = new AgentConfiguration();

		// Act
		var result = await _agentService.UpdateAgentConfigurationAsync(agentId, configuration);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Agent ID cannot be empty");
	}

	[Fact]
	public async Task Should_ReturnFailure_When_ConfigurationIsNull()
	{
		// Arrange
		var agentId = Guid.NewGuid();

		// Act
		var result = await _agentService.UpdateAgentConfigurationAsync(agentId, null!);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Configuration cannot be null");
	}

	#endregion

	#region UpdateAgentStatusAsync Tests

	[Fact]
	public async Task Should_UpdateStatus_When_ValidDataProvided()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var newStatus = AgentStatus.Paused;
		var existingAgent = new Agent { Id = agentId, Name = "TestAgent", Status = AgentStatus.Active };

		_agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.Success(existingAgent));
		
		_agentRepository.UpdateAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		// Act
		var result = await _agentService.UpdateAgentStatusAsync(agentId, newStatus);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		existingAgent.Status.ShouldBe(newStatus);

		await _agentRepository.Received(1).GetByIdAsync(agentId, Arg.Any<CancellationToken>());
		await _agentRepository.Received(1).UpdateAsync(existingAgent, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Should_ReturnFailure_When_StatusAgentIdIsEmpty()
	{
		// Arrange
		var agentId = Guid.Empty;
		var status = AgentStatus.Paused;

		// Act
		var result = await _agentService.UpdateAgentStatusAsync(agentId, status);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Agent ID cannot be empty");
	}

	#endregion

	#region AssignTaskAsync Tests

	[Fact]
	public async Task Should_AssignTask_When_ValidDataProvided()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var taskId = Guid.NewGuid();
		var agent = new Agent { Id = agentId, Status = AgentStatus.Active };
		var task = new AgentTask { Id = taskId, Status = TaskStatus.Pending };

		_agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.Success(agent));
		
		_taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Result<AgentTask>.Success(task));
		
		_taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		// Act
		var result = await _agentService.AssignTaskAsync(agentId, taskId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		task.AssignedAgentId.ShouldBe(agentId);

		await _agentRepository.Received(1).GetByIdAsync(agentId, Arg.Any<CancellationToken>());
		await _taskRepository.Received(1).GetByIdAsync(taskId, Arg.Any<CancellationToken>());
		await _taskRepository.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Should_ReturnFailure_When_AssignTaskAgentIdIsEmpty()
	{
		// Arrange
		var agentId = Guid.Empty;
		var taskId = Guid.NewGuid();

		// Act
		var result = await _agentService.AssignTaskAsync(agentId, taskId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Agent ID cannot be empty");
	}

	[Fact]
	public async Task Should_ReturnFailure_When_AssignTaskTaskIdIsEmpty()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var taskId = Guid.Empty;

		// Act
		var result = await _agentService.AssignTaskAsync(agentId, taskId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Task ID cannot be empty");
	}

	[Fact]
	public async Task Should_ReturnFailure_When_AgentIsNotActive()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var taskId = Guid.NewGuid();
		var agent = new Agent { Id = agentId, Status = AgentStatus.Paused };

		_agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.Success(agent));

		// Act
		var result = await _agentService.AssignTaskAsync(agentId, taskId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Agent");
		result.Errors.ShouldContain("is not active");
	}

	[Fact]
	public async Task Should_ReturnFailure_When_TaskIsNotPending()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var taskId = Guid.NewGuid();
		var agent = new Agent { Id = agentId, Status = AgentStatus.Active };
		var task = new AgentTask { Id = taskId, Status = TaskStatus.InProgress };

		_agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.Success(agent));
		
		_taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Result<AgentTask>.Success(task));

		// Act
		var result = await _agentService.AssignTaskAsync(agentId, taskId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Task");
		result.Errors.ShouldContain("is not available for assignment");
	}

	#endregion

	#region FindBestAgentForTaskAsync Tests

	[Fact]
	public async Task Should_FindBestAgent_When_ValidTaskTypeProvided()
	{
		// Arrange
		var taskType = "DataProcessing";
		var supportingAgents = new List<Agent>
		{
			new() { Id = Guid.NewGuid(), Status = AgentStatus.Active },
			new() { Id = Guid.NewGuid(), Status = AgentStatus.Active }
		};

		var agentsWithTaskCount = new List<AgentTaskCount>
		{
			new() { Agent = supportingAgents[0], TaskCount = 1 },
			new() { Agent = supportingAgents[1], TaskCount = 0 }
		};

		_agentRepository.FindByTaskTypeAsync(taskType, Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.Success(supportingAgents));
		
		_agentRepository.GetAgentsWithTaskCountAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<(Agent Agent, int TaskCount)>>.Success(
				agentsWithTaskCount.Select(atc => (atc.Agent, atc.TaskCount))));

		// Act
		var result = await _agentService.FindBestAgentForTaskAsync(taskType);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBe(supportingAgents[1]); // Agent with least tasks
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public async Task Should_ReturnFailure_When_TaskTypeIsInvalid(string? invalidTaskType)
	{
		// Act
		var result = await _agentService.FindBestAgentForTaskAsync(invalidTaskType!);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Task type cannot be empty");
	}

	[Fact]
	public async Task Should_ReturnFailure_When_NoSupportingAgentsFound()
	{
		// Arrange
		var taskType = "UnsupportedTask";
		var emptyAgentsList = new List<Agent>();

		_agentRepository.FindByTaskTypeAsync(taskType, Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.Success(emptyAgentsList));

		// Act
		var result = await _agentService.FindBestAgentForTaskAsync(taskType);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("No active agents found");
	}

	#endregion

	#region DeleteAgentAsync Tests

	[Fact]
	public async Task Should_DeleteAgent_When_ValidIdAndNoActiveTasks()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var agent = new Agent { Id = agentId };
		var emptyTasksList = new List<AgentTask>();

		_agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.Success(agent));
		
		_taskRepository.GetByAgentAsync(agentId, TaskStatus.InProgress, Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<AgentTask>>.Success(emptyTasksList));
		
		_agentRepository.DeleteAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		// Act
		var result = await _agentService.DeleteAgentAsync(agentId);

		// Assert
		result.IsSuccess.ShouldBeTrue();

		await _agentRepository.Received(1).DeleteAsync(agentId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Should_ReturnFailure_When_DeleteAgentIdIsEmpty()
	{
		// Arrange
		var agentId = Guid.Empty;

		// Act
		var result = await _agentService.DeleteAgentAsync(agentId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Agent ID cannot be empty");
	}

	[Fact]
	public async Task Should_ReturnFailure_When_AgentHasActiveTasks()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var agent = new Agent { Id = agentId };
		var activeTasks = new List<AgentTask>
		{
			new() { Id = Guid.NewGuid(), Status = TaskStatus.InProgress }
		};

		_agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.Success(agent));
		
		_taskRepository.GetByAgentAsync(agentId, TaskStatus.InProgress, Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<AgentTask>>.Success(activeTasks));

		// Act
		var result = await _agentService.DeleteAgentAsync(agentId);

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain("Cannot delete agent");
		result.Errors.ShouldContain("active tasks in progress");

		await _agentRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
	}

	#endregion

	#region GetAllAgentsAsync Tests

	[Fact]
	public async Task Should_GetAllAgents_When_Called()
	{
		// Arrange
		var allAgents = new List<Agent>
		{
			new() { Id = Guid.NewGuid(), Name = "Agent1" },
			new() { Id = Guid.NewGuid(), Name = "Agent2" },
			new() { Id = Guid.NewGuid(), Name = "Agent3" }
		};

		_agentRepository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.Success(allAgents));

		// Act
		var result = await _agentService.GetAllAgentsAsync();

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldBe(allAgents);

		await _agentRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Should_ReturnFailure_When_GetAllAgentsFails()
	{
		// Arrange
		var repositoryError = "Repository error";

		_agentRepository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<Agent>>.WithFailure(repositoryError));

		// Act
		var result = await _agentService.GetAllAgentsAsync();

		// Assert
		result.IsFailure.ShouldBeTrue();
		result.Errors.ShouldContain(repositoryError);
	}

	#endregion

	#region AssignTaskToAgentAsync Tests

	[Fact]
	public async Task Should_DelegateToAssignTaskAsync_When_AssignTaskToAgentAsyncCalled()
	{
		// Arrange
		var agentId = Guid.NewGuid();
		var taskId = Guid.NewGuid();
		var agent = new Agent { Id = agentId, Status = AgentStatus.Active };
		var task = new AgentTask { Id = taskId, Status = TaskStatus.Pending };

		_agentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.Success(agent));
		
		_taskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
			.Returns(Result<AgentTask>.Success(task));
		
		_taskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
			.Returns(Result.Success());

		// Act
		var result = await _agentService.AssignTaskToAgentAsync(agentId, taskId);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		task.AssignedAgentId.ShouldBe(agentId);

		// Verify the same repository calls as AssignTaskAsync
		await _agentRepository.Received(1).GetByIdAsync(agentId, Arg.Any<CancellationToken>());
		await _taskRepository.Received(1).GetByIdAsync(taskId, Arg.Any<CancellationToken>());
		await _taskRepository.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
	}

	#endregion
} 