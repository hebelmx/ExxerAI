using ExxerAI.Domain;
using ExxerAI.Domain.Operations;
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
result.Value!.ShouldNotBeNull();
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
result.Value!.ShouldBe(agents);
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
public async Task AssignTaskAsync_Should_ReturnSuccess_When_ValidParameters()
{
// Arrange
var agentId = Guid.NewGuid();
var taskId = Guid.NewGuid();
var agent = new Agent { Id = agentId, Status = AgentStatus.Active };
var task = new AgentTask { Id = taskId, AgentStatus = TaskAgentStatus.Pending };

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
result.Value!.ShouldBeTrue();
task.AssignedAgentId.ShouldBe(agentId);
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
public async Task DeleteAgentAsync_Should_ReturnSuccess_When_ValidParameters()
{
// Arrange
var agentId = Guid.NewGuid();
var agent = new Agent { Id = agentId };

_mockAgentRepository.GetByIdAsync(agentId, Arg.Any<CancellationToken>())
.Returns(Result<Agent>.WithSuccess(agent));
_mockTaskRepository.GetByAgentAsync(agentId, TaskAgentStatus.InProgress, Arg.Any<CancellationToken>())
.Returns(Result<IEnumerable<AgentTask>>.WithSuccess(new List<AgentTask>()));
_mockAgentRepository.DeleteAsync(agentId, Arg.Any<CancellationToken>())
.Returns(Result<bool>.WithSuccess(true));

// Act
var result = await _agentService.DeleteAgentAsync(agentId);

// Assert
result.IsSuccess.ShouldBeTrue();
result.Value!.ShouldBeTrue();
}
}
