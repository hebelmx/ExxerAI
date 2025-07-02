using ExxerAI.Application.Interfaces;
using ExxerAI.CLI.Commands;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;

namespace ExxerAI.CLI.Tests;

/// <summary>
/// Comprehensive unit tests for AgentCommands CLI functionality
/// Tests all agent command operations, validation, error handling, and edge cases
/// </summary>
public class AgentCommandsTests
{
    private readonly IAgentService _agentService;
    private readonly IRepository<Agent> _agentRepository;
    private readonly AgentCommands _agentCommands;
    private readonly StringWriter _consoleOutput;
    private readonly TextWriter _originalOutput;

    public AgentCommandsTests()
    {
        _agentService = Substitute.For<IAgentService>();
        _agentRepository = Substitute.For<IRepository<Agent>>();
        _agentCommands = new AgentCommands(_agentService, _agentRepository);
        
        // Capture console output for verification
        _consoleOutput = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _consoleOutput.Dispose();
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldInitializeSuccessfully()
    {
        // Act & Assert
        _agentCommands.ShouldNotBeNull();
    }

    [Fact]
    public void Constructor_WithNullAgentService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new AgentCommands(null!, _agentRepository));
    }

    [Fact]
    public void Constructor_WithNullAgentRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new AgentCommands(_agentService, null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyArgs_ShouldReturnZero()
    {
        // Act
        var result = await _agentCommands.ExecuteAsync([]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithHelpCommand_ShouldReturnZero()
    {
        // Act
        var result = await _agentCommands.ExecuteAsync(["help"]);

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCommand_ShouldReturnOne()
    {
        // Act
        var result = await _agentCommands.ExecuteAsync(["invalidcommand"]);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithListCommand_ShouldCallRepository()
    {
        // Arrange
        _agentRepository.GetAllAsync().Returns(Result<IEnumerable<Agent>>.WithSuccess([]));

        // Act
        var result = await _agentCommands.ExecuteAsync(["list"]);

        // Assert
        result.ShouldBe(0);
        await _agentRepository.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WithCreateCommand_ShouldCallAgentService()
    {
        // Arrange
        var testAgent = CreateTestAgent();
        _agentService.CreateAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentCapabilities>())
            .Returns(Result<Agent>.WithSuccess(testAgent));

        // Act
        var result = await _agentCommands.ExecuteAsync(["create", "TestAgent"]);

        // Assert
        result.ShouldBe(0);
        await _agentService.Received(1).CreateAgentAsync("TestAgent", Arg.Any<string>(), Arg.Any<AgentCapabilities>());
    }

    [Fact]
    public async Task ExecuteAsync_CreateWithoutName_ShouldReturnOne()
    {
        // Act
        var result = await _agentCommands.ExecuteAsync(["create"]);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_DeleteWithValidId_ShouldCallRepository()
    {
        // Arrange
        var testId = Guid.NewGuid();
        _agentRepository.DeleteAsync(testId).Returns(Result<bool>.WithSuccess(true));

        // Act
        var result = await _agentCommands.ExecuteAsync(["delete", testId.ToString()]);

        // Assert
        result.ShouldBe(0);
        await _agentRepository.Received(1).DeleteAsync(testId);
    }

    [Fact]
    public async Task ExecuteAsync_DeleteWithInvalidId_ShouldReturnOne()
    {
        // Act
        var result = await _agentCommands.ExecuteAsync(["delete", "invalid-id"]);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public async Task ExecuteAsync_StatusWithValidId_ShouldCallRepository()
    {
        // Arrange
        var testAgent = CreateTestAgent();
        _agentRepository.GetByIdAsync(testAgent.Id).Returns(Result<Agent>.WithSuccess(testAgent));

        // Act
        var result = await _agentCommands.ExecuteAsync(["status", testAgent.Id.ToString()]);

        // Assert
        result.ShouldBe(0);
        await _agentRepository.Received(1).GetByIdAsync(testAgent.Id);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateWithValidId_ShouldCallRepository()
    {
        // Arrange
        var testAgent = CreateTestAgent();
        _agentRepository.GetByIdAsync(testAgent.Id).Returns(Result<Agent>.WithSuccess(testAgent));
        _agentRepository.UpdateAsync(Arg.Any<Agent>()).Returns(Result<Agent>.WithSuccess(testAgent));

        // Act
        var result = await _agentCommands.ExecuteAsync(["update", testAgent.Id.ToString(), "--name", "NewName"]);

        // Assert
        result.ShouldBe(0);
        await _agentRepository.Received(1).GetByIdAsync(testAgent.Id);
        await _agentRepository.Received(1).UpdateAsync(Arg.Any<Agent>());
    }

    [Fact]
    public async Task ExecuteAsync_ActivateWithValidId_ShouldCallRepository()
    {
        // Arrange
        var testAgent = CreateTestAgent();
        _agentRepository.GetByIdAsync(testAgent.Id).Returns(Result<Agent>.WithSuccess(testAgent));
        _agentRepository.UpdateAsync(Arg.Any<Agent>()).Returns(Result<Agent>.WithSuccess(testAgent));

        // Act
        var result = await _agentCommands.ExecuteAsync(["activate", testAgent.Id.ToString()]);

        // Assert
        result.ShouldBe(0);
        await _agentRepository.Received(1).GetByIdAsync(testAgent.Id);
    }

    [Theory]
    [InlineData("list")]
    [InlineData("ls")]
    public async Task ExecuteAsync_ListAliases_ShouldWork(string command)
    {
        // Arrange
        _agentRepository.GetAllAsync().Returns(Result<IEnumerable<Agent>>.WithSuccess([]));

        // Act
        var result = await _agentCommands.ExecuteAsync([command]);

        // Assert
        result.ShouldBe(0);
    }

    [Theory]
    [InlineData("create")]
    [InlineData("new")]
    public async Task ExecuteAsync_CreateAliases_ShouldWork(string command)
    {
        // Arrange
        var testAgent = CreateTestAgent();
        _agentService.CreateAgentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AgentCapabilities>())
            .Returns(Result<Agent>.WithSuccess(testAgent));

        // Act
        var result = await _agentCommands.ExecuteAsync([command, "TestAgent"]);

        // Assert
        result.ShouldBe(0);
    }

    [Theory]
    [InlineData("delete")]
    [InlineData("remove")]
    [InlineData("rm")]
    public async Task ExecuteAsync_DeleteAliases_ShouldWork(string command)
    {
        // Arrange
        var testId = Guid.NewGuid();
        _agentRepository.DeleteAsync(testId).Returns(Result<bool>.WithSuccess(true));

        // Act
        var result = await _agentCommands.ExecuteAsync([command, testId.ToString()]);

        // Assert
        result.ShouldBe(0);
    }

    [Theory]
    [InlineData("status")]
    [InlineData("info")]
    public async Task ExecuteAsync_StatusAliases_ShouldWork(string command)
    {
        // Arrange
        var testAgent = CreateTestAgent();
        _agentRepository.GetByIdAsync(testAgent.Id).Returns(Result<Agent>.WithSuccess(testAgent));

        // Act
        var result = await _agentCommands.ExecuteAsync([command, testAgent.Id.ToString()]);

        // Assert
        result.ShouldBe(0);
    }

    [Theory]
    [InlineData("help")]
    [InlineData("--help")]
    [InlineData("-h")]
    public async Task ExecuteAsync_HelpAliases_ShouldReturnZero(string command)
    {
        // Act
        var result = await _agentCommands.ExecuteAsync([command]);

        // Assert
        result.ShouldBe(0);
    }

    private static Agent CreateTestAgent(string name = "TestAgent", AgentStatus status = AgentStatus.Active)
    {
        return new Agent
        {
            Id = Guid.NewGuid(),
            Name = name,
            Status = status,
            Description = $"Test agent {name}",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            Capabilities = new AgentCapabilities
            {
                CanProcessNaturalLanguage = true,
                CanGenerateCode = false,
                CanAnalyzeData = false,
                CanCallExternalAPIs = false,
                MaxConcurrentTasks = 1
            }
        };
    }
} 