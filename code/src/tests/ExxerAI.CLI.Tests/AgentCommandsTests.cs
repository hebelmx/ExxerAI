using ExxerAI.Domain.Operations;
using Xunit;

namespace ExxerAI.CLI.Tests;

/// <summary>
/// Tests for AgentCommands CLI functionality
/// </summary>
public class AgentCommandsTests
{
    private readonly IAgentService _mockAgentService;
    private readonly IRepository<Agent> _mockAgentRepository;
    private readonly AgentCommands _agentCommands;

    public AgentCommandsTests()
    {
        _mockAgentService = Substitute.For<IAgentService>();
        _mockAgentRepository = Substitute.For<IRepository<Agent>>();
        _agentCommands = new AgentCommands(_mockAgentService, _mockAgentRepository);
    }

    [Fact]
    public void Constructor_Should_InitializeCorrectly_When_ValidParametersProvided()
    {
        // Arrange & Act
        var commands = new AgentCommands(_mockAgentService, _mockAgentRepository);

        // Assert
        commands.ShouldNotBeNull();
    }

    [Fact]
    public void ValidateConstructorParameters_Should_ReturnSuccess_When_AllParametersValid()
    {
        // Arrange
        var agentService = Substitute.For<IAgentService>();
        var agentRepository = Substitute.For<IRepository<Agent>>();

        // Act
        var result = AgentCommands.ValidateConstructorParameters(agentService, agentRepository);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void ValidateConstructorParameters_Should_ReturnFailure_When_AgentServiceIsNull()
    {
        // Arrange
        var agentRepository = Substitute.For<IRepository<Agent>>();

        // Act
        var result = AgentCommands.ValidateConstructorParameters(null!, agentRepository);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("agentService");
    }

    [Fact]
    public void ValidateConstructorParameters_Should_ReturnFailure_When_AgentRepositoryIsNull()
    {
        // Arrange
        var agentService = Substitute.For<IAgentService>();

        // Act
        var result = AgentCommands.ValidateConstructorParameters(agentService, null!);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error!.ShouldContain("agentRepository");
    }

    [Fact]
    public async Task ExecuteAsync_Should_ShowHelp_When_NoArgumentsProvided()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallListAgents_When_ListCommandProvided()
    {
        // Arrange
        var args = new[] { "list" };
        _mockAgentRepository.GetAllAsync().Returns(Result<IEnumerable<Agent>>.Success(new List<Agent>()));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentRepository.Received(1).GetAllAsync( TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallListAgents_When_LsCommandProvided()
    {
        // Arrange
        var args = new[] { "ls" };
        _mockAgentRepository.GetAllAsync().Returns(Result<IEnumerable<Agent>>.Success(new List<Agent>()));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentRepository.Received(1).GetAllAsync( TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallCreateAgent_When_CreateCommandProvided()
    {
        // Arrange
        var args = new[] { "create", "TestAgent" };
        var testAgent = new Agent { Id = Guid.NewGuid(), Name = "TestAgent" };
        _mockAgentService.CreateAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentCapabilities>())
            .Returns(Result<Agent>.Success(testAgent));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentService.Received(1).CreateAgentAsync(
            "TestAgent",
            "Auto-generated agent TestAgent",
            Arg.Any<AgentCapabilities>(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallDeleteAgent_When_DeleteCommandProvided()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "delete", agentId.ToString() };
        _mockAgentRepository.DeleteAsync(agentId).Returns(Task.FromResult(Result<bool>.Success(true)));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentRepository.Received(1).DeleteAsync(agentId, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallShowAgentStatus_When_StatusCommandProvided()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "agentStatus", agentId.ToString() };
        var testAgent = new Agent
        {
            Id = agentId,
            Name = "TestAgent",
            Capabilities = new AgentCapabilities()
        };
        _mockAgentRepository.GetByIdAsync(agentId).Returns(Task.FromResult(Result<Agent>.Success(testAgent)));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentRepository.Received(1).GetByIdAsync(agentId, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallUpdateAgent_When_UpdateCommandProvided()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "update", agentId.ToString(), "--name", "NewName" };
        var testAgent = new Agent { Id = agentId, Name = "OldName" };
        var updatedAgent = new Agent { Id = agentId, Name = "NewName" };
        _mockAgentRepository.GetByIdAsync(agentId).Returns(Task.FromResult(Result<Agent>.Success(testAgent)));
        _mockAgentRepository.UpdateAsync(Arg.Any<Agent>()).Returns(Task.FromResult(Result<Agent>.Success(updatedAgent)));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentRepository.Received(1).GetByIdAsync(agentId, TestContext.Current.CancellationToken);
        await _mockAgentRepository.Received(1).UpdateAsync(Arg.Is<Agent>(a => a.Name == "NewName"), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallActivateAgent_When_ActivateCommandProvided()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "activate", agentId.ToString() };
        var testAgent = new Agent { Id = agentId, Name = "TestAgent", Status = AgentStatus.Inactive };
        var updatedAgent = new Agent { Id = agentId, Name = "TestAgent", Status = AgentStatus.Active };
        _mockAgentRepository.GetByIdAsync(agentId).Returns(Task.FromResult(Result<Agent>.Success(testAgent)));
        _mockAgentRepository.UpdateAsync(Arg.Any<Agent>()).Returns(Task.FromResult(Result<Agent>.Success(updatedAgent)));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentRepository.Received(1).UpdateAsync(Arg.Is<Agent>(a => a.Status == AgentStatus.Active), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_Should_CallDeactivateAgent_When_DeactivateCommandProvided()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "deactivate", agentId.ToString() };
        var testAgent = new Agent { Id = agentId, Name = "TestAgent", Status = AgentStatus.Active };
        var updatedAgent = new Agent { Id = agentId, Name = "TestAgent", Status = AgentStatus.Inactive };
        _mockAgentRepository.GetByIdAsync(agentId).Returns(Task.FromResult(Result<Agent>.Success(testAgent)));
        _mockAgentRepository.UpdateAsync(Arg.Any<Agent>()).Returns(Task.FromResult(Result<Agent>.Success(updatedAgent)));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentRepository.Received(1).UpdateAsync(Arg.Is<Agent>(a => a.Status == AgentStatus.Inactive), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ShowHelp_When_HelpCommandProvided()
    {
        // Arrange
        var args = new[] { "help" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_ShowUnknownCommand_When_InvalidCommandProvided()
    {
        // Arrange
        var args = new[] { "invalid" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ListAgents_Should_ReturnSuccess_When_NoAgentsExist()
    {
        // Arrange
        var args = new[] { "list" };
        _mockAgentRepository.GetAllAsync().Returns(Task.FromResult(Result<IEnumerable<Agent>>.Success(new List<Agent>())));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ListAgents_Should_DisplayAgents_When_AgentsExist()
    {
        // Arrange
        var args = new[] { "list" };
        var agents = new List<Agent>
        {
            new() { Id = Guid.NewGuid(), Name = "Agent1", Status = AgentStatus.Active, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Agent2", Status = AgentStatus.Inactive, CreatedAt = DateTime.UtcNow }
        };
        _mockAgentRepository.GetAllAsync().Returns(Task.FromResult(Result<IEnumerable<Agent>>.Success(agents)));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ListAgents_Should_FilterByStatus_When_StatusFilterProvided()
    {
        // Arrange
        var args = new[] { "list", "--agentStatus", "Active" };
        var agents = new List<Agent>
        {
            new() { Id = Guid.NewGuid(), Name = "ActiveAgent", Status = AgentStatus.Active, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "InactiveAgent", Status = AgentStatus.Inactive, CreatedAt = DateTime.UtcNow }
        };
        _mockAgentRepository.GetAllAsync().Returns(Task.FromResult(Result<IEnumerable<Agent>>.Success(agents)));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ListAgents_Should_ReturnError_When_InvalidStatusFilter()
    {
        // Arrange
        var args = new[] { "list", "--agentStatus", "InvalidStatus" };
        _mockAgentRepository.GetAllAsync().Returns(Task.FromResult(Result<IEnumerable<Agent>>.Success(new List<Agent>())));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ListAgents_Should_ReturnError_When_RepositoryFails()
    {
        // Arrange
        var args = new[] { "list" };
        _mockAgentRepository.GetAllAsync().Returns(Task.FromResult(Result<IEnumerable<Agent>>.WithFailure("Database error")));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task CreateAgent_Should_ReturnError_When_NoNameProvided()
    {
        // Arrange
        var args = new[] { "create" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task CreateAgent_Should_CreateWithDescription_When_DescriptionProvided()
    {
        // Arrange
        var args = new[] { "create", "TestAgent", "--description", "Custom description" };
        var testAgent = new Agent { Id = Guid.NewGuid(), Name = "TestAgent" };
        _mockAgentService.CreateAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentCapabilities>())
            .Returns(Task.FromResult(Result<Agent>.Success(testAgent)));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentService.Received(1).CreateAgentAsync("TestAgent", "Custom description", Arg.Any<AgentCapabilities>(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CreateAgent_Should_ReturnError_When_ServiceFails()
    {
        // Arrange
        var args = new[] { "create", "TestAgent" };
        _mockAgentService.CreateAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentCapabilities>())
            .Returns(Task.FromResult(Result<Agent>.WithFailure("Service error")));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task DeleteAgent_Should_ReturnError_When_NoIdProvided()
    {
        // Arrange
        var args = new[] { "delete" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task DeleteAgent_Should_ReturnError_When_InvalidIdFormat()
    {
        // Arrange
        var args = new[] { "delete", "invalid-id" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task DeleteAgent_Should_ReturnError_When_RepositoryFails()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "delete", agentId.ToString() };
        _mockAgentRepository.DeleteAsync(agentId).Returns(Task.FromResult(Result<bool>.WithFailure("Delete failed")));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ShowAgentStatus_Should_ReturnError_When_NoIdProvided()
    {
        // Arrange
        var args = new[] { "agentStatus" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ShowAgentStatus_Should_ReturnError_When_InvalidIdFormat()
    {
        // Arrange
        var args = new[] { "agentStatus", "invalid-id" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ShowAgentStatus_Should_ReturnError_When_AgentNotFound()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "agentStatus", agentId.ToString() };
        _mockAgentRepository.GetByIdAsync(agentId).Returns(Task.FromResult(Result<Agent>.WithFailure("Agent not found")));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ShowAgentStatus_Should_ReturnError_When_RepositoryFails()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "agentStatus", agentId.ToString() };
        _mockAgentRepository.GetByIdAsync(agentId).Returns(Task.FromResult(Result<Agent>.WithFailure("Repository error")));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task UpdateAgent_Should_ReturnError_When_NoIdProvided()
    {
        // Arrange
        var args = new[] { "update" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task UpdateAgent_Should_ReturnError_When_InvalidIdFormat()
    {
        // Arrange
        var args = new[] { "update", "invalid-id" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task UpdateAgent_Should_UpdateDescription_When_DescriptionProvided()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "update", agentId.ToString(), "--description", "New description" };
        var testAgent = new Agent { Id = agentId, Name = "TestAgent", Description = "Old description" };
        var updatedAgent = new Agent { Id = agentId, Name = "TestAgent", Description = "New description" };
        _mockAgentRepository.GetByIdAsync(agentId).Returns(Task.FromResult(Result<Agent>.Success(testAgent)));
        _mockAgentRepository.UpdateAsync(Arg.Any<Agent>()).Returns(Task.FromResult(Result<Agent>.Success(updatedAgent)));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
        await _mockAgentRepository.Received(1).UpdateAsync(Arg.Is<Agent>(a => a.Description == "New description"), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UpdateAgent_Should_ReturnError_When_AgentNotFound()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "update", agentId.ToString(), "--name", "NewName" };
        _mockAgentRepository.GetByIdAsync(agentId).Returns(Task.FromResult(Result<Agent>.WithFailure("Not found")));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task UpdateAgent_Should_ReturnError_When_UpdateFails()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "update", agentId.ToString(), "--name", "NewName" };
        var testAgent = new Agent { Id = agentId, Name = "OldName" };
        _mockAgentRepository.GetByIdAsync(agentId).Returns(Task.FromResult(Result<Agent>.Success(testAgent)));
        _mockAgentRepository.UpdateAsync(Arg.Any<Agent>()).Returns(Task.FromResult(Result<Agent>.WithFailure("Update failed")));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ActivateAgent_Should_ReturnError_When_NoIdProvided()
    {
        // Arrange
        var args = new[] { "activate" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ActivateAgent_Should_ReturnError_When_InvalidIdFormat()
    {
        // Arrange
        var args = new[] { "activate", "invalid-id" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task DeactivateAgent_Should_ReturnError_When_NoIdProvided()
    {
        // Arrange
        var args = new[] { "deactivate" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task DeactivateAgent_Should_ReturnError_When_InvalidIdFormat()
    {
        // Arrange
        var args = new[] { "deactivate", "invalid-id" };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ChangeAgentStatus_Should_ReturnError_When_GetAgentFails()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "activate", agentId.ToString() };
        _mockAgentRepository.GetByIdAsync(agentId).Returns(Task.FromResult(Result<Agent>.WithFailure("Get failed")));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Fact]
    public async Task ChangeAgentStatus_Should_ReturnError_When_UpdateFails()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var args = new[] { "activate", agentId.ToString() };
        var testAgent = new Agent { Id = agentId, Name = "TestAgent", Status = AgentStatus.Inactive };
        _mockAgentRepository.GetByIdAsync(agentId).Returns(Task.FromResult(Result<Agent>.Success(testAgent)));
        _mockAgentRepository.UpdateAsync(Arg.Any<Agent>()).Returns(Task.FromResult(Result<Agent>.WithFailure("Update failed")));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("-h")]
    public async Task ExecuteAsync_Should_ShowHelp_When_HelpFlagsProvided(string helpFlag)
    {
        // Arrange
        var args = new[] { helpFlag };

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Theory]
    [InlineData("new")]
    [InlineData("remove")]
    [InlineData("rm")]
    [InlineData("info")]
    public async Task ExecuteAsync_Should_HandleCommandAliases_When_AliasesProvided(string alias)
    {
        // Arrange
        var args = alias switch
        {
            "new" => new[] { alias, "TestAgent" },
            "remove" or "rm" => new[] { alias, Guid.NewGuid().ToString() },
            "info" => new[] { alias, Guid.NewGuid().ToString() },
            _ => new[] { alias }
        };

        if (alias == "new")
        {
            var testAgent = new Agent { Id = Guid.NewGuid(), Name = "TestAgent" };
            _mockAgentService.CreateAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentCapabilities>())
                .Returns(Task.FromResult(Result<Agent>.Success(testAgent)));
        }
        else if (alias == "remove" || alias == "rm")
        {
            _mockAgentRepository.DeleteAsync(Arg.Any<Guid>()).Returns(Task.FromResult(Result<bool>.Success(true)));
        }
        else if (alias == "info")
        {
            var testAgent = new Agent { Id = Guid.NewGuid(), Name = "TestAgent", Capabilities = new AgentCapabilities() };
            _mockAgentRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult(Result<Agent>.Success(testAgent)));
        }

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_Should_HandleExceptions_When_ExceptionThrown()
    {
        // Arrange
        var args = new[] { "list" };
        _mockAgentRepository.When(x => x.GetAllAsync()).Do(x => throw new InvalidOperationException("Test exception"));

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(1);
    }

    [Theory]
    [InlineData("-s")]
    [InlineData("-d")]
    [InlineData("-n")]
    public async Task ExecuteAsync_Should_HandleShortFlags_When_ShortFlagsProvided(string shortFlag)
    {
        // Arrange
        var args = shortFlag switch
        {
            "-s" => new[] { "list", shortFlag, "Active" },
            "-d" => new[] { "create", "TestAgent", shortFlag, "Test description" },
            "-n" => new[] { "update", Guid.NewGuid().ToString(), shortFlag, "New name" },
            _ => Array.Empty<string>()
        };

        if (shortFlag == "-s")
        {
            _mockAgentRepository.GetAllAsync().Returns(Task.FromResult(Result<IEnumerable<Agent>>.Success(new List<Agent>())));
        }
        else if (shortFlag == "-d")
        {
            var testAgent = new Agent { Id = Guid.NewGuid(), Name = "TestAgent" };
            _mockAgentService.CreateAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentCapabilities>())
                .Returns(Task.FromResult(Result<Agent>.Success(testAgent)));
        }
        else if (shortFlag == "-n")
        {
            var testAgent = new Agent { Id = Guid.Parse(args[1]), Name = "OldName" };
            var updatedAgent = new Agent { Id = Guid.Parse(args[1]), Name = "New name" };
            _mockAgentRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult(Result<Agent>.Success(testAgent)));
            _mockAgentRepository.UpdateAsync(Arg.Any<Agent>()).Returns(Task.FromResult(Result<Agent>.Success(updatedAgent)));
        }

        // Act
        var exitCode = await _agentCommands.ExecuteAsync(args, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exitCode.ShouldBe(0);
    }
}