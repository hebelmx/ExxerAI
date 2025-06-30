using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;
using TaskStatus = ExxerAI.Domain.TaskStatus;

namespace ExxerAI.Application.Tests;

/// <summary>
/// Unit tests for AgentService
/// </summary>
public class AgentServiceTests
{
	private readonly IAgentRepository _mockAgentRepository;
	private readonly ITaskRepository _mockTaskRepository;
	private readonly AgentService _agentService;

	public AgentServiceTests()
	{
		_mockAgentRepository = Substitute.For<IAgentRepository>();
		_mockTaskRepository = Substitute.For<ITaskRepository>();
		_agentService = new AgentService(_mockAgentRepository, _mockTaskRepository);
	}

	/// <summary>
	/// Test class for dependency injection validation
	/// </summary>
	public class ConstructorTests
	{
		[Fact]
		public void Should_CreateAgentService_When_ValidDependenciesProvided()
		{
			// Arrange
			var agentRepo = Substitute.For<IAgentRepository>();
			var taskRepo = Substitute.For<ITaskRepository>();

			// Act
			var service = new AgentService(agentRepo, taskRepo);

			// Assert
			service.ShouldNotBeNull();
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_AgentRepositoryIsNull()
		{
			// Arrange
			var taskRepo = Substitute.For<ITaskRepository>();

			// Act & Assert
			Should.Throw<ArgumentNullException>(() => new AgentService(null!, taskRepo))
				.ParamName.ShouldBe("agentRepository");
		}

		[Fact]
		public void Should_ThrowArgumentNullException_When_TaskRepositoryIsNull()
		{
			// Arrange
			var agentRepo = Substitute.For<IAgentRepository>();

			// Act & Assert
			Should.Throw<ArgumentNullException>(() => new AgentService(agentRepo, null!))
				.ParamName.ShouldBe("taskRepository");
		}
	}

	/// <summary>
	/// Test class for CreateAgentAsync method
	/// </summary>
	public class CreateAgentAsyncTests : AgentServiceTests
	{
		[Fact]
		public async Task Should_CreateAgent_When_ValidParametersProvided()
		{
			// Arrange
			var name = "Test Agent";
			var description = "Test Description";
			var capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] };
			var expectedAgent = new Agent
			{
				Id = Guid.NewGuid(),
				Name = name,
				Description = description,
				Capabilities = capabilities,
				Status = AgentStatus.Active
			};

			_mockAgentRepository.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
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

			await _mockAgentRepository.Received(1).AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
		}

		[Theory]
		[InlineData("", "Valid description")]
		[InlineData("   ", "Valid description")]
		[InlineData(null, "Valid description")]
		public async Task Should_ReturnFailure_When_NameIsInvalid(string? invalidName, string description)
		{
			// Arrange
			var capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] };

			// Act
			var result = await _agentService.CreateAgentAsync(invalidName!, description, capabilities);

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Errors.ShouldContain("Agent name cannot be empty");
			await _mockAgentRepository.DidNotReceive().AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
		}

		[Theory]
		[InlineData("Valid name", "")]
		[InlineData("Valid name", "   ")]
		[InlineData("Valid name", null)]
		public async Task Should_ReturnFailure_When_DescriptionIsInvalid(string name, string? invalidDescription)
		{
			// Arrange
			var capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] };

			// Act
			var result = await _agentService.CreateAgentAsync(name, invalidDescription!, capabilities);

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Errors.ShouldContain("Agent description cannot be empty");
			await _mockAgentRepository.DidNotReceive().AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_CapabilitiesIsNull()
		{
			// Arrange
			var name = "Test Agent";
			var description = "Test Description";

			// Act
			var result = await _agentService.CreateAgentAsync(name, description, null!);

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Errors.ShouldContain("Agent capabilities cannot be null");
			await _mockAgentRepository.DidNotReceive().AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_RepositoryAddFails()
		{
			// Arrange
			var name = "Test Agent";
			var description = "Test Description";
			var capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] };

			_mockAgentRepository.AddAsync(Arg.Any<Agent>(), Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithFailure("Repository error"));

			// Act
			var result = await _agentService.CreateAgentAsync(name, description, capabilities);

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Errors.ShouldContain("Repository error");
		}
	}

	/// <summary>
	/// Test class for GetAgentAsync method
	/// </summary>
	public class GetAgentAsyncTests : AgentServiceTests
	{
		[Fact]
		public async Task Should_ReturnAgent_When_ValidIdProvided()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var expectedAgent = new Agent { Id = agentId, Name = "Test Agent" };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.Success(expectedAgent));

			// Act
			var result = await _agentService.GetAgentAsync(agentId);

			// Assert
			result.IsSuccess.ShouldBeTrue();
			result.Value.ShouldBe(expectedAgent);
		}

		[Fact]
		public async Task Should_ReturnFailure_When_EmptyGuidProvided()
		{
			// Act
			var result = await _agentService.GetAgentAsync(Guid.Empty);

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Errors.ShouldContain("Agent ID cannot be empty");
			await _mockAgentRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_AgentNotFound()
		{
			// Arrange
			var agentId = Guid.NewGuid();

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.WithFailure("Not found"));

			// Act
			var result = await _agentService.GetAgentAsync(agentId);

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Errors.ShouldContain($"Agent not found with ID: {agentId}");
		}
	}

	/// <summary>
	/// Test class for AssignTaskAsync method
	/// </summary>
	public class AssignTaskAsyncTests : AgentServiceTests
	{
		[Fact]
		public async Task Should_AssignTask_When_ValidParametersProvided()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var taskId = Guid.NewGuid();

			var agent = new Agent { Id = agentId, Status = AgentStatus.Active };
			var task = new AgentTask { Id = taskId, Status = TaskStatus.Pending };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.Success(agent));

			_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
				.Returns(Result<AgentTask>.Success(task));

			_mockTaskRepository.UpdateAsync(Arg.Any<AgentTask>(), Arg.Any<CancellationToken>())
				.Returns(Result<AgentTask>.Success(task));

			// Act
			var result = await _agentService.AssignTaskAsync(agentId, taskId);

			// Assert
			result.IsSuccess.ShouldBeTrue();
			task.AssignedAgentId.ShouldBe(agentId);
			await _mockTaskRepository.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_AgentIdIsEmpty()
		{
			// Act
			var result = await _agentService.AssignTaskAsync(Guid.Empty, Guid.NewGuid());

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Errors.ShouldContain("Agent ID cannot be empty");
		}

		[Fact]
		public async Task Should_ReturnFailure_When_TaskIdIsEmpty()
		{
			// Act
			var result = await _agentService.AssignTaskAsync(Guid.NewGuid(), Guid.Empty);

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

			var agent = new Agent { Id = agentId, Status = AgentStatus.Inactive };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.Success(agent));

			// Act
			var result = await _agentService.AssignTaskAsync(agentId, taskId);

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Errors.ShouldContain($"Agent {agentId} is not active and cannot be assigned tasks");
		}

		[Fact]
		public async Task Should_ReturnFailure_When_TaskIsNotPending()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var taskId = Guid.NewGuid();

			var agent = new Agent { Id = agentId, Status = AgentStatus.Active };
			var task = new AgentTask { Id = taskId, Status = TaskStatus.InProgress };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.Success(agent));

			_mockTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
				.Returns(Result<AgentTask>.Success(task));

			// Act
			var result = await _agentService.AssignTaskAsync(agentId, taskId);

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Errors.ShouldContain($"Task {taskId} is not available for assignment (Status: {task.Status})");
		}
	}

	/// <summary>
	/// Test class for DeleteAgentAsync method
	/// </summary>
	public class DeleteAgentAsyncTests : AgentServiceTests
	{
		[Fact]
		public async Task Should_DeleteAgent_When_NoActiveTasksExist()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var agent = new Agent { Id = agentId };

			_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result<Agent>.Success(agent));

			_mockTaskRepository.GetByAgentAsync(agentId, TaskStatus.InProgress, Arg.Any<CancellationToken>())
				.Returns(Result<IEnumerable<AgentTask>>.Success(Array.Empty<AgentTask>()));

			_mockAgentRepository.DeleteAsync(agentId, Arg.Any<CancellationToken>())
				.Returns(Result.Success());

			// Act
			var result = await _agentService.DeleteAgentAsync(agentId);

			// Assert
			result.IsSuccess.ShouldBeTrue();
			await _mockAgentRepository.Received(1).DeleteAsync(agentId, Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Should_ReturnFailure_When_AgentHasActiveTasks()
		{
			// Arrange
			var agentId = Guid.NewGuid();
			var agent = new Agent { Id = agentId };
					var activeTasks = new[] { new AgentTask { Id = Guid.NewGuid(), Status = TaskStatus.InProgress } };

		_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
			.Returns(Result<Agent>.Success(agent));

		_mockTaskRepository.GetByAgentAsync(agentId, TaskStatus.InProgress, Arg.Any<CancellationToken>())
			.Returns(Result<IEnumerable<AgentTask>>.Success(activeTasks));

			// Act
			var result = await _agentService.DeleteAgentAsync(agentId);

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Errors.ShouldContain($"Cannot delete agent {agentId} as it has active tasks in progress");
			await _mockAgentRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
		}
	}
} 